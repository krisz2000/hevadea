﻿using Hevadea.Framework;
using Hevadea.GameObjects;
using Hevadea.GameObjects.Entities;
using Hevadea.WorldGenerator;
using System.IO;

namespace Hevadea.Loading
{
    public static class TaskFactorie
    {
        public static LoadingTask NewWorld(string path, Generator generator, int seed)
        {
            return new LoadingTask((tasks, reporter) =>
            {
                SetLastGame(path);
                reporter.RepportStatus("Generating world...");
                generator.Seed = seed;

                Game game = new Game
                {
                    SavePath = path,
                    MainPlayer = (Player)EntityFactory.PLAYER.Construct(),
                    World = generator.Generate(reporter)
                };

                tasks.Result = game;
            });
        }

        public static LoadingTask ConnectToServer(string ip, int port)
        {
            return new LoadingTask((task, reporter) =>
            {
                var game = new RemoteGame(ip, port);
                game.Connect();
                game.Initialize();
                task.Result = game;
            });
        }

        public static LoadingTask SaveWorld(Game game, string savePath = null)
        {
            return new LoadingTask((task, reporter) =>
            {
                SetLastGame(savePath ?? game.SavePath);
                game.Save(savePath ?? game.SavePath, reporter);
            });
        }

        public static LoadingTask LoadWorld(string path)
        {
            return new LoadingTask((task, reporter) =>
            {
                Game game = Game.Load(path, reporter);
                game.Initialize();
                task.Result = game;
            });
        }

        public static void SetLastGame(string path)
        {
            File.WriteAllText(Rise.Platform.GetStorageFolder() + "/.lastgame", path);
        }
    }
}