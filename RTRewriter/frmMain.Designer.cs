namespace RTRewriter
{
    partial class frmMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.chkHighResolutionFix = new System.Windows.Forms.CheckBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.chkEnableHiDef = new System.Windows.Forms.CheckBox();
            this.chkCooperativeFullscreen = new System.Windows.Forms.CheckBox();
            this.btnUpdate = new System.Windows.Forms.Button();
            this.btnOpenFolder = new System.Windows.Forms.Button();
            this.chkDisableAchievements = new System.Windows.Forms.CheckBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.chkSwapLeftRight = new System.Windows.Forms.CheckBox();
            this.chkAlwaysGravityGlobe = new System.Windows.Forms.CheckBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.chkDoubleUISize = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // chkHighResolutionFix
            // 
            this.chkHighResolutionFix.AutoSize = true;
            this.chkHighResolutionFix.Checked = true;
            this.chkHighResolutionFix.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkHighResolutionFix.Enabled = false;
            this.chkHighResolutionFix.Location = new System.Drawing.Point(6, 71);
            this.chkHighResolutionFix.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.chkHighResolutionFix.Name = "chkHighResolutionFix";
            this.chkHighResolutionFix.Size = new System.Drawing.Size(634, 32);
            this.chkHighResolutionFix.TabIndex = 1;
            this.chkHighResolutionFix.Text = "Enable Resolutions Up To 8192x8192 (Requires XNA \"Hi-Def\" Profile)";
            this.chkHighResolutionFix.UseVisualStyleBackColor = true;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.chkEnableHiDef);
            this.groupBox1.Controls.Add(this.chkHighResolutionFix);
            this.groupBox1.Controls.Add(this.chkCooperativeFullscreen);
            this.groupBox1.Location = new System.Drawing.Point(13, 13);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(669, 155);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Display";
            // 
            // chkEnableHiDef
            // 
            this.chkEnableHiDef.AutoSize = true;
            this.chkEnableHiDef.Checked = true;
            this.chkEnableHiDef.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkEnableHiDef.Enabled = false;
            this.chkEnableHiDef.Location = new System.Drawing.Point(6, 32);
            this.chkEnableHiDef.Name = "chkEnableHiDef";
            this.chkEnableHiDef.Size = new System.Drawing.Size(280, 32);
            this.chkEnableHiDef.TabIndex = 0;
            this.chkEnableHiDef.Text = "Enable XNA \"Hi-Def\" Profile";
            this.chkEnableHiDef.UseVisualStyleBackColor = true;
            this.chkEnableHiDef.CheckedChanged += new System.EventHandler(this.chkEnableHiDef_CheckedChanged);
            // 
            // chkCooperativeFullscreen
            // 
            this.chkCooperativeFullscreen.AutoSize = true;
            this.chkCooperativeFullscreen.Location = new System.Drawing.Point(6, 110);
            this.chkCooperativeFullscreen.Name = "chkCooperativeFullscreen";
            this.chkCooperativeFullscreen.Size = new System.Drawing.Size(299, 32);
            this.chkCooperativeFullscreen.TabIndex = 2;
            this.chkCooperativeFullscreen.Text = "Enable Cooperative Fullscreen";
            this.toolTip1.SetToolTip(this.chkCooperativeFullscreen, "Essentially a borderless window.  If you want a non-fullscreen borderless window," +
        " edit config.json after patching.");
            this.chkCooperativeFullscreen.UseVisualStyleBackColor = true;
            // 
            // btnUpdate
            // 
            this.btnUpdate.Location = new System.Drawing.Point(512, 465);
            this.btnUpdate.Name = "btnUpdate";
            this.btnUpdate.Size = new System.Drawing.Size(170, 43);
            this.btnUpdate.TabIndex = 4;
            this.btnUpdate.Text = "Update Terraria";
            this.btnUpdate.UseVisualStyleBackColor = true;
            this.btnUpdate.Click += new System.EventHandler(this.btnUpdate_Click);
            // 
            // btnOpenFolder
            // 
            this.btnOpenFolder.Location = new System.Drawing.Point(168, 465);
            this.btnOpenFolder.Name = "btnOpenFolder";
            this.btnOpenFolder.Size = new System.Drawing.Size(341, 43);
            this.btnOpenFolder.TabIndex = 3;
            this.btnOpenFolder.Text = "Open Terraria Save Game Folder";
            this.btnOpenFolder.UseVisualStyleBackColor = true;
            this.btnOpenFolder.Click += new System.EventHandler(this.btnOpenFolder_Click);
            // 
            // chkDisableAchievements
            // 
            this.chkDisableAchievements.AutoSize = true;
            this.chkDisableAchievements.Location = new System.Drawing.Point(6, 32);
            this.chkDisableAchievements.Name = "chkDisableAchievements";
            this.chkDisableAchievements.Size = new System.Drawing.Size(228, 32);
            this.chkDisableAchievements.TabIndex = 0;
            this.chkDisableAchievements.Text = "Disable Achievements";
            this.toolTip1.SetToolTip(this.chkDisableAchievements, "Prevents achievements from being unlocked.  Progress towards achievements is stil" +
        "l recorded, but the achievement itself will not activate.");
            this.chkDisableAchievements.UseVisualStyleBackColor = true;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.chkDoubleUISize);
            this.groupBox3.Controls.Add(this.chkSwapLeftRight);
            this.groupBox3.Controls.Add(this.chkAlwaysGravityGlobe);
            this.groupBox3.Controls.Add(this.chkDisableAchievements);
            this.groupBox3.Location = new System.Drawing.Point(13, 174);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(669, 285);
            this.groupBox3.TabIndex = 2;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Other Requests From People Using This Tool";
            // 
            // chkSwapLeftRight
            // 
            this.chkSwapLeftRight.AutoSize = true;
            this.chkSwapLeftRight.Location = new System.Drawing.Point(6, 70);
            this.chkSwapLeftRight.Name = "chkSwapLeftRight";
            this.chkSwapLeftRight.Size = new System.Drawing.Size(313, 32);
            this.chkSwapLeftRight.TabIndex = 3;
            this.chkSwapLeftRight.Text = "Swap Left/Right Mouse Buttons";
            this.toolTip1.SetToolTip(this.chkSwapLeftRight, "For left-handed users, swaps all references for left/right mouse buttons in the c" +
        "ode for you.");
            this.chkSwapLeftRight.UseVisualStyleBackColor = true;
            // 
            // chkAlwaysGravityGlobe
            // 
            this.chkAlwaysGravityGlobe.AutoSize = true;
            this.chkAlwaysGravityGlobe.Location = new System.Drawing.Point(6, 108);
            this.chkAlwaysGravityGlobe.Name = "chkAlwaysGravityGlobe";
            this.chkAlwaysGravityGlobe.Size = new System.Drawing.Size(383, 32);
            this.chkAlwaysGravityGlobe.TabIndex = 2;
            this.chkAlwaysGravityGlobe.Text = "Always Act Like You Have Gravity Globe";
            this.toolTip1.SetToolTip(this.chkAlwaysGravityGlobe, "Pressing Up (default: W) will invert gravity.");
            this.chkAlwaysGravityGlobe.UseVisualStyleBackColor = true;
            // 
            // chkDoubleUISize
            // 
            this.chkDoubleUISize.AutoSize = true;
            this.chkDoubleUISize.Location = new System.Drawing.Point(6, 146);
            this.chkDoubleUISize.Name = "chkDoubleUISize";
            this.chkDoubleUISize.Size = new System.Drawing.Size(552, 32);
            this.chkDoubleUISize.TabIndex = 4;
            this.chkDoubleUISize.Text = "Double The Size Of The UI Where Possible (EXPERIMENTAL)";
            this.toolTip1.SetToolTip(this.chkDoubleUISize, "Pressing Up (default: W) will invert gravity.");
            this.chkDoubleUISize.UseVisualStyleBackColor = true;
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(11F, 28F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(694, 520);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.btnOpenFolder);
            this.Controls.Add(this.btnUpdate);
            this.Controls.Add(this.groupBox1);
            this.Font = new System.Drawing.Font("Segoe UI", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "frmMain";
            this.Text = "RomTerraria 4: RTRewriter Harder";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.CheckBox chkHighResolutionFix;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Button btnUpdate;
        private System.Windows.Forms.CheckBox chkEnableHiDef;
        private System.Windows.Forms.CheckBox chkCooperativeFullscreen;
        private System.Windows.Forms.Button btnOpenFolder;
        private System.Windows.Forms.CheckBox chkDisableAchievements;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.CheckBox chkAlwaysGravityGlobe;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.CheckBox chkSwapLeftRight;
        private System.Windows.Forms.CheckBox chkDoubleUISize;
    }
}

