using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Mono.Cecil.Rocks;

namespace SEPatcherLib
{
    public class SEPatcher
    {
        public const string NormalExt = ".dll";
        public const string BackupExt = ".orig.dll";
        public const string TempExt = ".temp.dll";

        public string AssemblyName;
        public string AssemblyBackupName;
        public string AssemblyTempName;
        public string ModLoaderName;

        public string SERootDir;
        public string ManagedDir;
        public string BackupDir;
        public string LocalModDir;

        private string _localDir;

        public static string PersonalModDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), @"My Games\Stationeers\mods");

        public SEPatcher(string seRootDir, string assemblyName = "Assembly-CSharp.dll", string modLoaderName = "SEModLoader.dll")
        {
            SERootDir = seRootDir;
            AssemblyName = assemblyName;
            ModLoaderName = modLoaderName;

            AssemblyBackupName = Path.ChangeExtension(AssemblyName, BackupExt);
            AssemblyTempName = Path.ChangeExtension(AssemblyName, TempExt);

            ManagedDir = GetManagedDir();
            BackupDir = GetBackupDir();
            LocalModDir = GetLocalModDir();
            _localDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            if (!ValidGameDir())
            {
                Console.WriteLine("Invalid Stationeers Director");
                throw new ArgumentException("Invalid Stationeers Director", "seRootDir");
            }

            if (!ValidModLoaderFile())
            {
                Console.WriteLine("Could not find target Mod Loader");
                throw new ArgumentException("Could not find target Mod Loader", "modLoaderName");
            }
        }

        public string GetManagedDir()
        {
            return Path.Combine(SERootDir, @"rocketstation_Data\Managed");
        }

        public string GetBackupDir()
        {
            return Path.Combine(GetManagedDir(), "Unpatched");
        }

        public string GetLocalModDir()
        {
            return Path.Combine(SERootDir, "Mods");
        }

        public bool ValidGameDir()
        {
            if (!Directory.Exists(SERootDir))
            {
                return false;
            }

            if (!Directory.Exists(ManagedDir))
            {
                return false;
            }

            if (!File.Exists(Path.Combine(ManagedDir, AssemblyName)))
            {
                return false;
            }

            return true;
        }

        public bool ValidModLoaderFile()
        {
            if (!File.Exists(Path.Combine(_localDir, ModLoaderName)))
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public void PatchAssembly()
        {
            SetupFolders();
            BackupFiles();

            CopyToWorkingDir();

            using (var cSharpAssembly = LoadAssembly(Path.Combine(_localDir, AssemblyBackupName)))
            using (var modLoaderAssembly = LoadAssembly(Path.Combine(_localDir, ModLoaderName)))
            {
                ModuleDefinition cSharpAssemblyModule = cSharpAssembly.MainModule;

                TypeDefinition worldManager = cSharpAssemblyModule.Types.FirstOrDefault(t => (t.Name == "WorldManager"));
                MethodDefinition worldManagerAwake = worldManager.Methods.FirstOrDefault(m => (m.Name == "Awake"));

                ILProcessor processorAwake = worldManagerAwake.Body.GetILProcessor();

                TypeDefinition typeDefinition = modLoaderAssembly.MainModule.GetType("SEModLoader.ModLoader");

                MethodDefinition methodDefinition = typeDefinition.Methods.Single(m => m.Name == "Init");
                MethodReference methodReference = worldManagerAwake.Module.ImportReference(methodDefinition);

                processorAwake.InsertBefore(worldManagerAwake.Body.Instructions.Last(),
                    processorAwake.Create(OpCodes.Call, methodReference));

                worldManagerAwake.Body.OptimizeMacros();

                cSharpAssembly.Write(Path.Combine(_localDir, AssemblyTempName));
            }

            CopyPatchedFiles();
            CleanupWorkingDir();

            CopyAssemblies();
        }

        public void SetupFolders()
        {
            if (!Directory.Exists(BackupDir))
            {
                Directory.CreateDirectory(BackupDir);
            }

            if (!Directory.Exists(LocalModDir))
            {
                Directory.CreateDirectory(LocalModDir);
            }
        }

        public void BackupFiles()
        {
            File.Copy(Path.Combine(ManagedDir, AssemblyName), Path.Combine(BackupDir, AssemblyBackupName), true);
        }

        public void CopyToWorkingDir()
        {
            File.Copy(Path.Combine(ManagedDir, AssemblyName), Path.Combine(_localDir, AssemblyBackupName), true);
        }

        public void CopyPatchedFiles()
        {
            File.Copy(Path.Combine(_localDir, AssemblyTempName), Path.Combine(ManagedDir, AssemblyName), true);
        }

        public void CleanupWorkingDir()
        {
            File.Delete(Path.Combine(_localDir, AssemblyBackupName));
            File.Delete(Path.Combine(_localDir, AssemblyTempName));
        }

        public void CopyAssemblies()
        {
            File.Copy(Path.Combine(_localDir, ModLoaderName), Path.Combine(ManagedDir, ModLoaderName), true);
            File.Copy(Path.Combine(_localDir, "0Harmony.dll"), Path.Combine(ManagedDir, "0Harmony.dll"));
        }

        public AssemblyDefinition LoadAssembly(string path)
        {
            if (File.Exists(path))
            {
                try
                {
                    var resolver = new DefaultAssemblyResolver();
                    resolver.AddSearchDirectory(ManagedDir);
                    resolver.AddSearchDirectory(_localDir);
                    AssemblyDefinition assembly = AssemblyDefinition.ReadAssembly(path, new ReaderParameters { AssemblyResolver = resolver });
                    return assembly;
                }
                catch (Exception e)
                {
                    Console.WriteLine(string.Format("Couldn't load assembly {0}{1}{2}", path, Environment.NewLine, e.Message));
                }
            }
            else
            {
                Console.WriteLine(string.Format("Assembly {0} doesn't exist", path));
            }

            return null;
        }
    }
}
