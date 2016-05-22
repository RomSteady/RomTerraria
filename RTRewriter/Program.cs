using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace RTRewriter
{
    static class Program
    {
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
                return false;
            }

            // Last chance: let's operate under the assumption that Terraria is in a different Steam install folder
            string steamConfig = Path.Combine(steamFolder, @"config/config.vdf");
            FindDetails.Add(String.Format("Checking Steam config at {0}", steamConfig));
            GamePath = ParseConfig(steamConfig, @"InstallConfigStore/Software/Valve/Steam/apps/105600/installdir");

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
                    } else
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


        const string TestVersionWarining =
            @"This is a pre-release version of the RomTerraria launcher.  Some features may not work.
Please reply to the appropriate thread on http://romsteady.blogspot.com with bug reports.
Do not ask others to use this launcher without pointing them to that post.
Don't upload to other sites yet.

Do you agree to these terms?
";

        const string InstallElsewhereWarning =
            @"RomTerraria should not be installed in the same folder where Terraria.exe lives.

Please unpack RomTerraria somewhere else and run it to prevent unexpected behavior.";

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (!FindGame())
            {
                FindDetails.Insert(0, "Unable to find Terraria.\n\nIf the Steam version, Make sure Steam is running.\n\nIf the GOG version, make sure you've run it once.\n\nDetails:");
                MessageBox.Show(String.Join("\n", FindDetails.ToArray()));
                return;
            }

            string rtPath = Path.GetDirectoryName((new System.Uri(Assembly.GetExecutingAssembly().CodeBase)).AbsolutePath);
            if (String.Compare(rtPath, GamePath, true) == 0)
            {
                MessageBox.Show(InstallElsewhereWarning, "Fatal Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            /*
            if (MessageBox.Show(TestVersionWarining, "RomTerraria 4 Pre-Release", MessageBoxButtons.YesNo) != DialogResult.Yes)
            {
                return;
            }
            */

            frmMain form = new frmMain();
            form.GamePath = GamePath;
            Application.Run(form);
        }
    }
}
