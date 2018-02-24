﻿using System.IO;
using Maker.Hevadea.Game.Entities.Creatures;
using Maker.Utils;
using Maker.Utils.Enums;
using Maker.Utils.Json;

namespace Maker.Hevadea.Game.Storage
{
    public class GameLoader
    {
        private string _status;
        private bool _isDone = false;

        private void SetStatus(string status)
        {
            _status = status;
            Logger.Log<GameManager>(LoggerLevel.Info, status);
        }
        
        public GameManager Load(string path)
        {
            Logger.Log<GameManager>(LoggerLevel.Info, $"Loading world from '{path}'.");
            
            SetStatus("read_files");
            string worldStr = File.ReadAllText($"{path}\\world.json");
            string playerStr = File.ReadAllText($"{path}\\player.json");
            
            SetStatus("create_game_objects");
            var world = new World();
            var player = new PlayerEntity();
            
            SetStatus("deserialize_game_data");
            var worldDara = worldStr.FromJson<WorldStorage>();
            var playerData = playerStr.FromJson<EntityStorage>();
            
            SetStatus("loading_game_data");
            world.Load(worldDara);
            player.Load(playerData);
            
            Logger.Log<GameManager>(LoggerLevel.Fine, "Done!");
            
            return new GameManager(world, player);
        }

        public string GetStatus()
        {
            return _status;
        }
    }
}