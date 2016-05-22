using Microsoft.Win32;
using System;
using System.IO;

namespace RTHooks
{
    public static class StartupHelpers
    {
        public static string GetTerrariaFolder()
        {
            const string RegistryKey = @"HKEY_LOCAL_MACHINE\SOFTWARE\re-logic\terraria";
            const string RegistryValue = @"install_path";
            string GamePath = Convert.ToString(Registry.GetValue(RegistryKey, RegistryValue, ""));
            if (String.IsNullOrWhiteSpace(GamePath))
            {
                throw new ArgumentException("The registry doesn't point to where Terraria is installed.  Please run Terraria once before running RomTerraria.");
            }

            if (!Directory.Exists(GamePath))
            {
                throw new ArgumentException("There is no version of Terraria where the Terraria registry keys are pointing to.  Please reinstall Terraria, run it once, then repatch RomTerraria.");
            }

            return GamePath;
        }

        public static void ChangeWorkingDirectory()
        {
            Directory.SetCurrentDirectory(GetTerrariaFolder());
        }
    }
}
