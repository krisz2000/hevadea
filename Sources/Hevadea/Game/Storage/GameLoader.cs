﻿using Hevadea.Framework.Utils;
using Hevadea.Framework.Utils.Json;
using System.IO;
using Hevadea.Game.Entities;
using Hevadea.Game.Worlds;
using Hevadea.Game.Registry;

namespace Hevadea.Game.Storage
{
    public class GameLoader
    {
        private string _status;
        private bool _isDone = false;
        private float _progress = 0f;

        private void SetStatus(string status)
        {
            _status = status;
            Logger.Log<GameManager>(LoggerLevel.Info, status);
        }
        
        public GameManager Load(string path)
        {
            Logger.Log<GameManager>(LoggerLevel.Info, $"Loading world from '{path}'.");
            
            SetStatus("Loading file...");
            string worldStr = File.ReadAllText($"{path}/world.json");
            string playerStr = File.ReadAllText($"{path}/player.json");
            _progress = 0.25f;

            SetStatus("Creating game objects...");
            var world = new World();
            var player = new EntityPlayer();
            player.Blueprint = ENTITIES.PLAYER;
            _progress = 0.5f;

            SetStatus("Loading game data...");
            var worldDara = worldStr.FromJson<WorldStorage>();
            var playerData = playerStr.FromJson<EntityStorage>();
            _progress = 0.75f;

            SetStatus("Initialization...");
            world.Load(worldDara);
            player.Load(playerData);
            _progress = 1f;

            Logger.Log<GameManager>(LoggerLevel.Fine, "Done!");
            _isDone = true;
            return new GameManager(world, player);
        }

        public float GetProgress()
        {
            return _progress;
        }

        public string GetStatus()
        {
            return _status;
        }
    }
}