using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace RTRewriter
{
    public partial class frmMain : Form
    {
        public string GamePath { get; set; }

        public frmMain()
        {
            InitializeComponent();
        }

        private string SaveGameFolder()
        {
            string NewLocation = Path.GetFullPath(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "./My Games/Terraria"));
            return NewLocation;
        }

        private void btnUpdate_Click(object sender, EventArgs e)
        {
            this.Enabled = false;
            Application.DoEvents();

            string NewLocation = SaveGameFolder();

            string inFile = String.Format(@"{0}\Terraria.exe", GamePath);
            string outFile = String.Format(@"{0}\Terraria.exe", NewLocation);
            System.IO.File.Copy("RTHooks.dll", String.Format(@"{0}\RTHooks.dll", NewLocation), true);
            System.IO.File.Copy("Terraria.exe.config", String.Format(@"{0}\Terraria.exe.config", NewLocation), true);

            try
            {
                using (Cecil.Rewriter rewriter = new Cecil.Rewriter(inFile, outFile))
                {
                    rewriter.CondenseSteam();
                    rewriter.FixConfigFileNames();
                    rewriter.ChangeWorkingDirectory();

                    if (chkEnableHiDef.Checked)
                    {
                        rewriter.EnableHiDefProfile();
                    }

                    if (chkHighResolutionFix.Checked && chkEnableHiDef.Checked)
                    {
                        rewriter.UpMaxResolution();
                    }

                    if (chkCooperativeFullscreen.Checked)
                    {
                        rewriter.CooperativeFullscreen();
                    }

                    if (chkDisableAchievements.Checked)
                    {
                        rewriter.DisableAchievements();
                    }

                    if (chkAlwaysGravityGlobe.Checked)
                    {
                        rewriter.ForceGravityGlobeOn();
                    }

                    if (chkSwapLeftRight.Checked)
                    {
                        rewriter.SwapLeftRightMouseButtons();
                    }

                    if (chkDoubleUISize.Checked)
                    {
                        rewriter.DoubleUISize();
                    }

                    rewriter.Save();
                }
                MessageBox.Show(String.Format("You now have Terraria.exe in your Terraria save game folder:\n{0}\n\n" +
                    "Rename Terraria.exe in your Terraria folder to Terraria.Original.exe.\n\n" +
                    "Copy Terraria.exe, Terraria.exe.config, and RTHooks.dll from your save folder to your Terraria folder, then launch via Steam.", NewLocation));
            }
            catch (Exception ex)
            {
                MessageBox.Show(String.Format("{0}\n\n{1}", ex.GetType().ToString(), ex.Message), "Oops!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            this.Enabled = true;
            Application.DoEvents();
        }

        private void chkEnableHiDef_CheckedChanged(object sender, EventArgs e)
        {
            chkHighResolutionFix.Enabled = chkEnableHiDef.Checked;
        }

        private void btnOpenFolder_Click(object sender, EventArgs e)
        {
            Process.Start(SaveGameFolder());
        }
    }
}
