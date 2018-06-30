using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;
using Steamworks;

namespace SEModLoader
{
    public class ModLoader : MonoBehaviour
    {
        private static object _initLock = new object();
        public static ModLoader Instance; // This should be changed to be a property

        private static string SteamAppDir;
        private static string AppRootDir;
        private static string AppDataDir;
        private static string ManagedDir;
        private static string MyGamesModDir;
        private static string ModLoaderDir;

        public static List<object> LoadedModules = new List<object>();

        public static void Init()
        {
            lock (_initLock)
            {
                if (!Instance)
                {
                    GameObject go = new GameObject();
                    go.name = "ModLoader";
                    Instance = go.AddComponent<ModLoader>();
                    DontDestroyOnLoad(go);
                    SteamAPI.Init();
                    SteamApps.GetAppInstallDir(AppId_t.SpaceStationOnline, out SteamAppDir, 1024u);

                    AppDataDir = Path.GetFullPath(Application.dataPath);
                    ManagedDir = Path.GetFullPath(Path.Combine(AppDataDir, "Managed"));
                    AppRootDir = Path.GetDirectoryName(AppDataDir);
                    MyGamesModDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), @"My Games\Stationeers\mods");
                    ModLoaderDir = Path.Combine(AppRootDir, "Mods");

                    if (!Directory.Exists(ModLoaderDir))
                    {
                        Directory.CreateDirectory(ModLoaderDir);
                    }

                    if (!Directory.Exists(MyGamesModDir))
                    {
                        Directory.CreateDirectory(MyGamesModDir);
                    }

                    LoadMods();
                }
            }
        }

        public static void LoadMods()
        {
            foreach (var file in Directory.GetFiles(ModLoaderDir, "*.dll"))
            {
                LoadDLL(file);
            }

            foreach (var file in Directory.GetFiles(MyGamesModDir, "*.dll"))
            {
                LoadDLL(file);
            }
        }

        public static void LoadDLL(string file)
        {
            Assembly assembly = Assembly.LoadFrom(file);

            var types = from type in assembly.GetTypes()
                where typeof(IMod).IsAssignableFrom(type)
                select type;

            foreach (Type type in types)
            {
                LoadedModules.Add(type);
                type.GetMethod("Init").Invoke(null, null);
            }
        }
    }
}