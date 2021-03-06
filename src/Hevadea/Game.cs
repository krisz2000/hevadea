﻿using Hevadea.Framework;
using Hevadea.Framework.Platform;
using Hevadea.Framework.Scening;
using Hevadea.Framework.Threading;
using Hevadea.Loading;
using Hevadea.Scenes;
using Hevadea.Scenes.MainMenu;
using Hevadea.WorldGenerator;
using System.IO;

namespace Hevadea
{
    public static class Game
    {
        public static readonly int Unit = 16;
        public static readonly string Name = "Hevadea";
        public static readonly string Version = "0.1.0";
        public static readonly int VersionNumber = 1;

        public static string GetSaveFolder()
        {
            return Rise.Platform.GetStorageFolder() + "/Saves/";
        }

        public static void SetLastGame(string path)
        {
            File.WriteAllText(Rise.Platform.GetStorageFolder() + "/.lastgame", path);
        }

        public static string GetLastGame()
        {
            if (File.Exists(Rise.Platform.GetStorageFolder() + "/.lastgame"))
                return File.ReadAllText(Rise.Platform.GetStorageFolder() + "/.lastgame");

            return null;
        }

        public static void GoToMainMenu()
        {
            Rise.Scene.Switch(Rise.Platform.Family == PlatformFamily.Desktop ? new DesktopMainMenu() : (Scene)new MobileMainMenu());
        }

        public static void Play(string gamePath)
        {
            var job = Jobs.LoadWorld.SetArguments(new Jobs.WorldLoadInfo(gamePath));

            job.Finish += (sender, e)
                => Rise.Scene.Switch(new SceneGameplay((GameState)((Job)sender).Result));

            Rise.Scene.Switch(new LoadingScene(job));
        }

        public static void New(string name, string seedString, Generator generator)
        {
            if (!int.TryParse(seedString, out int seed))
            {
                seed = seedString.GetHashCode();
            }

            New(name, seed, generator);
        }

        public static void New(string name, int seed, Generator generator)
        {
            var job = Jobs.GenerateWorld;
            job.SetArguments(new Jobs.WorldGeneratorInfo($"{GetSaveFolder()}{name}/", seed, generator));

            job.Finish += (sender, e) =>
            {
                GameState gameState = (GameState)((Job)sender).Result;
                gameState.Initialize();
                Rise.Scene.Switch(new SceneGameplay(gameState));
            };
            Rise.Scene.Switch(new LoadingScene(job));
        }
    }
}