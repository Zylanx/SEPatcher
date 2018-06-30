using System;
using System.IO;
using SEPatcherLib;

namespace SEPatcherConsole
{
    class SEPatcherConsole
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                PrintHelp();
                return;
            }

            string seRootDir = args[0].TrimEnd('"');

            if (!Directory.Exists(seRootDir))
            {
                Console.WriteLine(String.Format("Error: Invalid path \"{0}\"", seRootDir));
                PrintHelp();
                return;
            }
            else
            {
                try
                {
                    SEPatcher sePatcher = new SEPatcher(seRootDir, "Assembly-CSharp.dll", "SEModLoader.dll");
                    sePatcher.PatchAssembly();
                }
                catch (ArgumentException e)
                {
                    Console.WriteLine(String.Format("Error: Invalid path \"{0}\"", seRootDir));
                    PrintHelp();
                    return;
                }
            }
        }

        public static void PrintHelp()
        {
            Console.WriteLine("Usage: SEPatcher.exe \"Drive:/Path/To/Stationeers/Folder\"");
            Console.WriteLine("");
            Console.WriteLine("Example - SEPatcher.exe \"C:/Progam Files(x86)/Steam/steamapps/common/Stationeers/\"");
            Console.WriteLine("");
        }
    }
}