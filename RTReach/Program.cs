using Microsoft.Win32;
using Mono.Cecil;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RTReach
{
    static class Program
    {
        static MemoryStream reachprofilestream;
        static AssemblyDefinition terraria;

        static string GamePath = "";

        static List<string> FindDetails = new List<string>();

        static List<Process> GetProcessesByName(string machine, string filter, RegexOptions options)
        {
            List<Process> processList = new List<Process>();

            // Get the current processes
            Process[] runningProcesses = Process.GetProcesses(machine);

            // Find those that match the specified regular expression
            Regex processFilter = new Regex(filter, options);
            foreach (Process current in runningProcesses)
            {
                // Check for a match.
                if (processFilter.IsMatch(current.ProcessName))
                {
                    processList.Add(current);
                }
                // Dispose of any we're not keeping
                else current.Dispose();
            }

            // Return the filtered list as an array
            return processList;
        }

        static bool FindGame()
        {

            // Check Steam first
            var steamCandidates = GetProcessesByName(".", "steam", RegexOptions.IgnoreCase);
            string steamFolder = String.Empty;
            foreach (var p in steamCandidates)
            {
                try
                {
                    if (p.MainModule.ModuleName.ToLower() == "steam.exe")
                    {
                        FindDetails.Add("Steam.exe process found.");
                        FileInfo f = new FileInfo(p.MainModule.FileName);
                        steamFolder = f.DirectoryName;
                        var d = f.Directory.GetDirectories("steamapps");
                        if (d.Count() > 0)
                        {
                            FindDetails.Add("Steamapps folder found.");
                            d = d[0].GetDirectories("common");
                            if (d.Count() > 0)
                            {
                                FindDetails.Add("Common folder found.");
                                d = d[0].GetDirectories("terraria");
                                if (d.Count() > 0)
                                {
                                    FindDetails.Add("Terraria folder found.");
                                    GamePath = d[0].FullName;
                                    return true;
                                }
                            }
                        }
                    }
                }
                catch (System.ComponentModel.Win32Exception)
                {
                    // Ignore this...means we were looking at SteamService.
                }
            }

            if (FindDetails.Count == 0)
            {
                FindDetails.Add("Could not find a process named Steam.exe running.");
            }
            else if (!String.IsNullOrWhiteSpace(steamFolder))
            {
                // Last chance: let's operate under the assumption that Terraria is in a different Steam install folder
                string steamConfig = Path.Combine(steamFolder, @"config/config.vdf");
                FindDetails.Add(String.Format("Checking Steam config at {0}", steamConfig));
                GamePath = ParseConfig(steamConfig, @"InstallConfigStore/Software/Valve/Steam/apps/105600/installdir");
            }

            if (String.IsNullOrWhiteSpace(GamePath))
            {
                // Okay, it's not the Steam version.  Let's see if it's the GOG.com version.
                // We're going to look for this registry key and return it: HKLM\SOFTWARE\re-logic\terraria
                FindDetails.Add("Steam version not found.  Checking registry for location of GOG.com version.");
                const string RegistryKey = @"HKEY_LOCAL_MACHINE\SOFTWARE\re-logic\terraria";
                const string RegistryValue = @"install_path";
                GamePath = Convert.ToString(Registry.GetValue(RegistryKey, RegistryValue, ""));
            }

            if (String.IsNullOrWhiteSpace(GamePath))
            {
                // Not all GOG versions had that registry key...let's let them browse for their Terraria folder
                FolderBrowserDialog fbd = new FolderBrowserDialog();
                fbd.ShowNewFolderButton = false;
                fbd.Description = "RomTerraria couldn't automatically find you Terraria folder.  Please select your Terraria folder.";
                var result = fbd.ShowDialog();
                if (result == DialogResult.OK)
                {
                    var tempPath = fbd.SelectedPath;
                    var tempFile = Path.Combine(tempPath, "Terraria.exe");
                    if (File.Exists(tempFile))
                    {
                        GamePath = tempPath;
                    }
                    else
                    {
                        FindDetails.Add(String.Format("Terraria.exe not found in " + tempPath));
                    }
                }
            }

            return !String.IsNullOrWhiteSpace(GamePath);
        }

        private static string ParseConfig(string steamConfig, string configNode)
        {
            Stack<string> keys = new Stack<string>();
            string lastString = String.Empty;
            using (StreamReader sr = new StreamReader(steamConfig))
            {
                while (!sr.EndOfStream)
                {
                    string s = sr.ReadLine().Trim();
                    if (s.StartsWith("{"))
                    {
                        keys.Push(lastString);
                    }
                    else if (s.StartsWith("}"))
                    {
                        keys.Pop();
                    }
                    else if (s.StartsWith("\""))
                    {
                        var tokens = s.Split('\t').Where(a => !String.IsNullOrWhiteSpace(a)).ToArray();
                        lastString = tokens[0].Trim('\"');
                        if (tokens.Length > 1)
                        {
                            string key = String.Join("/", keys.ToArray().Reverse()) + "/" + lastString;
                            Debug.WriteLine(key, "ParseConfig");
                            if (key.Equals(configNode, StringComparison.InvariantCultureIgnoreCase))
                            {
                                return tokens[1].Trim('\"').Replace(@"\\", @"\");
                            }
                        }
                    }
                }
            }

            return null;
        }

        static string SaveGameFolder()
        {
            string NewLocation = Path.GetFullPath(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "./My Games/Terraria"));
            return NewLocation;
        }

        static void EnableReachProfile()
        {
            EmbeddedResource reachprofile = null;

            reachprofilestream = new MemoryStream();

            System.IO.StreamWriter sw = new System.IO.StreamWriter(reachprofilestream);
            sw.Write("Windows.v4.0.Reach");
            sw.Flush();
            reachprofilestream.Seek(0, System.IO.SeekOrigin.Begin);
            reachprofile = new EmbeddedResource("Microsoft.Xna.Framework.RuntimeProfile",
                ManifestResourceAttributes.Private, reachprofilestream);



            for (int i = 0; i < terraria.MainModule.Resources.Count; i++)
            {
                if (terraria.MainModule.Resources[i].Name == "Microsoft.Xna.Framework.RuntimeProfile")
                {
                    terraria.MainModule.Resources[i] = reachprofile;
                }
            }
        }

        // From https://github.com/dougbenham/TerrariaPatcher/blob/ca332bbb04d576f641d6c8de70c65f4eb84ddb61/IL.cs
        static void MakeLargeAddressAware(string file)
        {
            using (var stream = File.Open(file, FileMode.Open, FileAccess.ReadWrite))
            {
                const int IMAGE_FILE_LARGE_ADDRESS_AWARE = 0x20;

                var br = new BinaryReader(stream);
                var bw = new BinaryWriter(stream);

                if (br.ReadInt16() != 0x5A4D)       //No MZ Header
                    return;

                br.BaseStream.Position = 0x3C;
                var peloc = br.ReadInt32();         //Get the PE header location.

                br.BaseStream.Position = peloc;
                if (br.ReadInt32() != 0x4550)       //No PE header
                    return;

                br.BaseStream.Position += 0x12;

                var position = br.BaseStream.Position;
                var flags = br.ReadInt16();
                bool isLAA = (flags & IMAGE_FILE_LARGE_ADDRESS_AWARE) == IMAGE_FILE_LARGE_ADDRESS_AWARE;
                if (isLAA)                          //Already Large Address Aware
                    return;

                flags |= IMAGE_FILE_LARGE_ADDRESS_AWARE;

                bw.Seek((int)position, SeekOrigin.Begin);
                bw.Write(flags);
                bw.Flush();
            }
        }

        [STAThread]
        static void Main()
        {
            if (!FindGame())
            {
                FindDetails.Insert(0, "Unable to find Terraria.\n\nIf the Steam version, make sure Steam is running.\n\nIf the GOG version, make sure you've run it once.\n\nDetails:");
                MessageBox.Show(String.Join("\n", FindDetails.ToArray()));
                return;
            }

            string NewLocation = SaveGameFolder();

            string assemblyIn = String.Format(@"{0}\Terraria.exe", GamePath);
            string assemblyOut = String.Format(@"{0}\Terraria.exe", NewLocation);

            terraria = AssemblyDefinition.ReadAssembly(assemblyIn);
            EnableReachProfile();
            terraria.Write(assemblyOut);
            if (reachprofilestream != null)
            {
                reachprofilestream.Close();
                reachprofilestream = null;
            }
            MakeLargeAddressAware(assemblyOut);

            MessageBox.Show("A new version of Terraria.exe has been dropped in your save game folder.  Replace the version of Terraria in your install folder with this executable to enable Reach profile.");
        }
    }
}
