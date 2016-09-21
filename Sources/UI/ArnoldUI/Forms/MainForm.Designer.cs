namespace GoodAI.Arnold
{
    sealed partial class MainForm
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
            WeifenLuo.WinFormsUI.Docking.DockPanelSkin dockPanelSkin1 = new WeifenLuo.WinFormsUI.Docking.DockPanelSkin();
            WeifenLuo.WinFormsUI.Docking.AutoHideStripSkin autoHideStripSkin1 = new WeifenLuo.WinFormsUI.Docking.AutoHideStripSkin();
            WeifenLuo.WinFormsUI.Docking.DockPanelGradient dockPanelGradient1 = new WeifenLuo.WinFormsUI.Docking.DockPanelGradient();
            WeifenLuo.WinFormsUI.Docking.TabGradient tabGradient1 = new WeifenLuo.WinFormsUI.Docking.TabGradient();
            WeifenLuo.WinFormsUI.Docking.DockPaneStripSkin dockPaneStripSkin1 = new WeifenLuo.WinFormsUI.Docking.DockPaneStripSkin();
            WeifenLuo.WinFormsUI.Docking.DockPaneStripGradient dockPaneStripGradient1 = new WeifenLuo.WinFormsUI.Docking.DockPaneStripGradient();
            WeifenLuo.WinFormsUI.Docking.TabGradient tabGradient2 = new WeifenLuo.WinFormsUI.Docking.TabGradient();
            WeifenLuo.WinFormsUI.Docking.DockPanelGradient dockPanelGradient2 = new WeifenLuo.WinFormsUI.Docking.DockPanelGradient();
            WeifenLuo.WinFormsUI.Docking.TabGradient tabGradient3 = new WeifenLuo.WinFormsUI.Docking.TabGradient();
            WeifenLuo.WinFormsUI.Docking.DockPaneStripToolWindowGradient dockPaneStripToolWindowGradient1 = new WeifenLuo.WinFormsUI.Docking.DockPaneStripToolWindowGradient();
            WeifenLuo.WinFormsUI.Docking.TabGradient tabGradient4 = new WeifenLuo.WinFormsUI.Docking.TabGradient();
            WeifenLuo.WinFormsUI.Docking.TabGradient tabGradient5 = new WeifenLuo.WinFormsUI.Docking.TabGradient();
            WeifenLuo.WinFormsUI.Docking.DockPanelGradient dockPanelGradient3 = new WeifenLuo.WinFormsUI.Docking.DockPanelGradient();
            WeifenLuo.WinFormsUI.Docking.TabGradient tabGradient6 = new WeifenLuo.WinFormsUI.Docking.TabGradient();
            WeifenLuo.WinFormsUI.Docking.TabGradient tabGradient7 = new WeifenLuo.WinFormsUI.Docking.TabGradient();
            this.dockPanel = new WeifenLuo.WinFormsUI.Docking.DockPanel();
            this.vS2012LightTheme1 = new WeifenLuo.WinFormsUI.Docking.VS2012LightTheme();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStrip1 = new System.Windows.Forms.ToolStrip();
            this.connectButton = new System.Windows.Forms.ToolStripButton();
            this.disconnectButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.newBlueprintButton = new System.Windows.Forms.ToolStripButton();
            this.openBlueprintButton = new System.Windows.Forms.ToolStripButton();
            this.saveBlueprintButton = new System.Windows.Forms.ToolStripButton();
            this.saveAsBlueprintButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.loadBlueprintButton = new System.Windows.Forms.ToolStripButton();
            this.clearBlueprintButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.runButton = new System.Windows.Forms.ToolStripButton();
            this.pauseButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.brainStepButton = new System.Windows.Forms.ToolStripButton();
            this.bodyStepButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.showVisualizationButton = new System.Windows.Forms.ToolStripButton();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.regularCheckpointingButton = new System.Windows.Forms.ToolStripButton();
            this.checkpointingIntervalTextBox = new System.Windows.Forms.ToolStripTextBox();
            this.statusStrip = new System.Windows.Forms.StatusStrip();
            this.openFileDialog = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            this.menuStrip1.SuspendLayout();
            this.toolStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // dockPanel
            // 
            this.dockPanel.BackgroundImageLayout = System.Windows.Forms.ImageLayout.Stretch;
            this.dockPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dockPanel.DocumentStyle = WeifenLuo.WinFormsUI.Docking.DocumentStyle.DockingWindow;
            this.dockPanel.Location = new System.Drawing.Point(0, 49);
            this.dockPanel.Name = "dockPanel";
            this.dockPanel.Size = new System.Drawing.Size(1194, 661);
            dockPanelGradient1.EndColor = System.Drawing.SystemColors.ControlLight;
            dockPanelGradient1.StartColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            autoHideStripSkin1.DockStripGradient = dockPanelGradient1;
            tabGradient1.EndColor = System.Drawing.SystemColors.Control;
            tabGradient1.StartColor = System.Drawing.SystemColors.Control;
            tabGradient1.TextColor = System.Drawing.SystemColors.ControlDarkDark;
            autoHideStripSkin1.TabGradient = tabGradient1;
            autoHideStripSkin1.TextFont = new System.Drawing.Font("Segoe UI", 9F);
            dockPanelSkin1.AutoHideStripSkin = autoHideStripSkin1;
            tabGradient2.EndColor = System.Drawing.Color.FromArgb(((int)(((byte)(204)))), ((int)(((byte)(206)))), ((int)(((byte)(219)))));
            tabGradient2.StartColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            tabGradient2.TextColor = System.Drawing.Color.White;
            dockPaneStripGradient1.ActiveTabGradient = tabGradient2;
            dockPanelGradient2.EndColor = System.Drawing.SystemColors.Control;
            dockPanelGradient2.StartColor = System.Drawing.SystemColors.Control;
            dockPaneStripGradient1.DockStripGradient = dockPanelGradient2;
            tabGradient3.EndColor = System.Drawing.Color.FromArgb(((int)(((byte)(28)))), ((int)(((byte)(151)))), ((int)(((byte)(234)))));
            tabGradient3.StartColor = System.Drawing.SystemColors.Control;
            tabGradient3.TextColor = System.Drawing.Color.Black;
            dockPaneStripGradient1.InactiveTabGradient = tabGradient3;
            dockPaneStripSkin1.DocumentGradient = dockPaneStripGradient1;
            dockPaneStripSkin1.TextFont = new System.Drawing.Font("Segoe UI", 9F);
            tabGradient4.EndColor = System.Drawing.Color.FromArgb(((int)(((byte)(80)))), ((int)(((byte)(170)))), ((int)(((byte)(220)))));
            tabGradient4.LinearGradientMode = System.Drawing.Drawing2D.LinearGradientMode.Vertical;
            tabGradient4.StartColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            tabGradient4.TextColor = System.Drawing.Color.White;
            dockPaneStripToolWindowGradient1.ActiveCaptionGradient = tabGradient4;
            tabGradient5.EndColor = System.Drawing.SystemColors.ControlLightLight;
            tabGradient5.StartColor = System.Drawing.SystemColors.ControlLightLight;
            tabGradient5.TextColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(122)))), ((int)(((byte)(204)))));
            dockPaneStripToolWindowGradient1.ActiveTabGradient = tabGradient5;
            dockPanelGradient3.EndColor = System.Drawing.SystemColors.Control;
            dockPanelGradient3.StartColor = System.Drawing.SystemColors.Control;
            dockPaneStripToolWindowGradient1.DockStripGradient = dockPanelGradient3;
            tabGradient6.EndColor = System.Drawing.SystemColors.ControlDark;
            tabGradient6.LinearGradientMode = System.Drawing.Drawing2D.LinearGradientMode.Vertical;
            tabGradient6.StartColor = System.Drawing.SystemColors.Control;
            tabGradient6.TextColor = System.Drawing.SystemColors.GrayText;
            dockPaneStripToolWindowGradient1.InactiveCaptionGradient = tabGradient6;
            tabGradient7.EndColor = System.Drawing.SystemColors.Control;
            tabGradient7.StartColor = System.Drawing.SystemColors.Control;
            tabGradient7.TextColor = System.Drawing.SystemColors.GrayText;
            dockPaneStripToolWindowGradient1.InactiveTabGradient = tabGradient7;
            dockPaneStripSkin1.ToolWindowGradient = dockPaneStripToolWindowGradient1;
            dockPanelSkin1.DockPaneStripSkin = dockPaneStripSkin1;
            this.dockPanel.Skin = dockPanelSkin1;
            this.dockPanel.TabIndex = 0;
            this.dockPanel.Theme = this.vS2012LightTheme1;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1194, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // fileMenuItem
            // 
            this.fileMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.exitToolStripMenuItem});
            this.fileMenuItem.Name = "fileMenuItem";
            this.fileMenuItem.Size = new System.Drawing.Size(37, 20);
            this.fileMenuItem.Text = "File";
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(92, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // toolStrip1
            // 
            this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.connectButton,
            this.disconnectButton,
            this.toolStripSeparator5,
            this.newBlueprintButton,
            this.openBlueprintButton,
            this.saveBlueprintButton,
            this.saveAsBlueprintButton,
            this.toolStripSeparator1,
            this.loadBlueprintButton,
            this.clearBlueprintButton,
            this.toolStripSeparator3,
            this.runButton,
            this.pauseButton,
            this.toolStripSeparator4,
            this.brainStepButton,
            this.bodyStepButton,
            this.toolStripSeparator2,
            this.showVisualizationButton,
            this.toolStripSeparator6,
            this.regularCheckpointingButton,
            this.checkpointingIntervalTextBox});
            this.toolStrip1.Location = new System.Drawing.Point(0, 24);
            this.toolStrip1.Name = "toolStrip1";
            this.toolStrip1.Size = new System.Drawing.Size(1194, 25);
            this.toolStrip1.TabIndex = 2;
            this.toolStrip1.Text = "toolStrip1";
            // 
            // connectButton
            // 
            this.connectButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.connectButton.Image = global::GoodAI.Arnold.Properties.Resources.Connect_16x;
            this.connectButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.connectButton.Name = "connectButton";
            this.connectButton.Size = new System.Drawing.Size(23, 22);
            this.connectButton.Text = "Connect to core";
            this.connectButton.Click += new System.EventHandler(this.connectButton_Click);
            // 
            // disconnectButton
            // 
            this.disconnectButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.disconnectButton.Image = global::GoodAI.Arnold.Properties.Resources.Disconnect_16x;
            this.disconnectButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.disconnectButton.Name = "disconnectButton";
            this.disconnectButton.Size = new System.Drawing.Size(23, 22);
            this.disconnectButton.Text = "Disconnect from core";
            this.disconnectButton.Click += new System.EventHandler(this.disconnectButton_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(6, 25);
            // 
            // newBlueprintButton
            // 
            this.newBlueprintButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.newBlueprintButton.Image = global::GoodAI.Arnold.Properties.Resources.NewFile_16x;
            this.newBlueprintButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.newBlueprintButton.Name = "newBlueprintButton";
            this.newBlueprintButton.Size = new System.Drawing.Size(23, 22);
            this.newBlueprintButton.Text = "New blueprint";
            this.newBlueprintButton.Click += new System.EventHandler(this.newBlueprintButton_Click);
            // 
            // openBlueprintButton
            // 
            this.openBlueprintButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.openBlueprintButton.Image = global::GoodAI.Arnold.Properties.Resources.OpenFolder_16x;
            this.openBlueprintButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.openBlueprintButton.Name = "openBlueprintButton";
            this.openBlueprintButton.Size = new System.Drawing.Size(23, 22);
            this.openBlueprintButton.Text = "Open blueprint ...";
            this.openBlueprintButton.Click += new System.EventHandler(this.openBlueprintButton_Click);
            // 
            // saveBlueprintButton
            // 
            this.saveBlueprintButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.saveBlueprintButton.Image = global::GoodAI.Arnold.Properties.Resources.Save_16x;
            this.saveBlueprintButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.saveBlueprintButton.Name = "saveBlueprintButton";
            this.saveBlueprintButton.Size = new System.Drawing.Size(23, 22);
            this.saveBlueprintButton.Text = "Save blueprint";
            this.saveBlueprintButton.Click += new System.EventHandler(this.saveBlueprintButton_Click);
            // 
            // saveAsBlueprintButton
            // 
            this.saveAsBlueprintButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.saveAsBlueprintButton.Image = global::GoodAI.Arnold.Properties.Resources.SaveAs_16x;
            this.saveAsBlueprintButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.saveAsBlueprintButton.Name = "saveAsBlueprintButton";
            this.saveAsBlueprintButton.Size = new System.Drawing.Size(23, 22);
            this.saveAsBlueprintButton.Text = "Save blueprint as ...";
            this.saveAsBlueprintButton.Click += new System.EventHandler(this.saveAsBlueprintButton_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(6, 25);
            // 
            // loadBlueprintButton
            // 
            this.loadBlueprintButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.loadBlueprintButton.Image = global::GoodAI.Arnold.Properties.Resources.Script_16x;
            this.loadBlueprintButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.loadBlueprintButton.Name = "loadBlueprintButton";
            this.loadBlueprintButton.Size = new System.Drawing.Size(23, 22);
            this.loadBlueprintButton.Text = "Load blueprint to core";
            this.loadBlueprintButton.Click += new System.EventHandler(this.loadBlueprintButton_Click);
            // 
            // clearBlueprintButton
            // 
            this.clearBlueprintButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.clearBlueprintButton.Image = global::GoodAI.Arnold.Properties.Resources.ScriptError_16x;
            this.clearBlueprintButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.clearBlueprintButton.Name = "clearBlueprintButton";
            this.clearBlueprintButton.Size = new System.Drawing.Size(23, 22);
            this.clearBlueprintButton.Text = "Clear blueprint from core";
            this.clearBlueprintButton.Click += new System.EventHandler(this.clearBlueprintButton_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(6, 25);
            // 
            // runButton
            // 
            this.runButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.runButton.Image = global::GoodAI.Arnold.Properties.Resources.Run_16x;
            this.runButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.runButton.Name = "runButton";
            this.runButton.Size = new System.Drawing.Size(23, 22);
            this.runButton.Text = "Run simulation";
            this.runButton.Click += new System.EventHandler(this.runButton_Click);
            // 
            // pauseButton
            // 
            this.pauseButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.pauseButton.Image = global::GoodAI.Arnold.Properties.Resources.Pause_16x;
            this.pauseButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.pauseButton.Name = "pauseButton";
            this.pauseButton.Size = new System.Drawing.Size(23, 22);
            this.pauseButton.Text = "Pause simulation";
            this.pauseButton.Click += new System.EventHandler(this.pauseButton_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(6, 25);
            // 
            // brainStepButton
            // 
            this.brainStepButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.brainStepButton.Image = global::GoodAI.Arnold.Properties.Resources.StepIn_16x;
            this.brainStepButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.brainStepButton.Name = "brainStepButton";
            this.brainStepButton.Size = new System.Drawing.Size(23, 22);
            this.brainStepButton.Text = "Perform brain step";
            this.brainStepButton.Click += new System.EventHandler(this.brainStepButton_Click);
            // 
            // bodyStepButton
            // 
            this.bodyStepButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.bodyStepButton.Image = global::GoodAI.Arnold.Properties.Resources.StepOver_16x;
            this.bodyStepButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.bodyStepButton.Name = "bodyStepButton";
            this.bodyStepButton.Size = new System.Drawing.Size(23, 22);
            this.bodyStepButton.Text = "Run to next body step";
            this.bodyStepButton.Click += new System.EventHandler(this.bodyStepButton_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(6, 25);
            // 
            // showVisualizationButton
            // 
            this.showVisualizationButton.CheckOnClick = true;
            this.showVisualizationButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.showVisualizationButton.Image = global::GoodAI.Arnold.Properties.Resources.observer_icon;
            this.showVisualizationButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.showVisualizationButton.Name = "showVisualizationButton";
            this.showVisualizationButton.Size = new System.Drawing.Size(23, 22);
            this.showVisualizationButton.Text = "Show visualization";
            this.showVisualizationButton.CheckedChanged += new System.EventHandler(this.showVisualizationButton_CheckedChanged);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(6, 25);
            // 
            // regularCheckpointingButton
            // 
            this.regularCheckpointingButton.CheckOnClick = true;
            this.regularCheckpointingButton.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.regularCheckpointingButton.Image = global::GoodAI.Arnold.Properties.Resources.Autosave;
            this.regularCheckpointingButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.regularCheckpointingButton.Name = "regularCheckpointingButton";
            this.regularCheckpointingButton.Size = new System.Drawing.Size(23, 22);
            this.regularCheckpointingButton.Text = "toolStripButton1";
            this.regularCheckpointingButton.ToolTipText = "Regular checkpointing enabled/disabled";
            this.regularCheckpointingButton.Click += new System.EventHandler(this.regularCheckpointingButton_Click);
            // 
            // checkpointingIntervalTextBox
            // 
            this.checkpointingIntervalTextBox.MaxLength = 7;
            this.checkpointingIntervalTextBox.Name = "checkpointingIntervalTextBox";
            this.checkpointingIntervalTextBox.Size = new System.Drawing.Size(40, 25);
            this.checkpointingIntervalTextBox.Text = "10";
            this.checkpointingIntervalTextBox.TextBoxTextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.checkpointingIntervalTextBox.ToolTipText = "Regular checkpointing interval in seconds";
            this.checkpointingIntervalTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.checkpointingIntervalTextBox_KeyDown);
            this.checkpointingIntervalTextBox.TextChanged += new System.EventHandler(this.checkpointingIntervalTextBox_TextChanged);
            // 
            // statusStrip
            // 
            this.statusStrip.Location = new System.Drawing.Point(0, 710);
            this.statusStrip.Name = "statusStrip";
            this.statusStrip.Size = new System.Drawing.Size(1194, 22);
            this.statusStrip.TabIndex = 3;
            this.statusStrip.Text = "statusStrip1";
            // 
            // openFileDialog
            // 
            this.openFileDialog.Filter = "blueprints|*.json|All files|*.*";
            this.openFileDialog.Title = "Open blueprint";
            // 
            // saveFileDialog
            // 
            this.saveFileDialog.Filter = "blueprints|*.json|All files|*.*";
            this.saveFileDialog.Title = "Save blueprint";
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1194, 732);
            this.Controls.Add(this.dockPanel);
            this.Controls.Add(this.statusStrip);
            this.Controls.Add(this.toolStrip1);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MainForm";
            this.Text = "Brain designer";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.toolStrip1.ResumeLayout(false);
            this.toolStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private WeifenLuo.WinFormsUI.Docking.DockPanel dockPanel;
        private WeifenLuo.WinFormsUI.Docking.VS2012LightTheme vS2012LightTheme1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileMenuItem;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.ToolStrip toolStrip1;
        private System.Windows.Forms.ToolStripButton runButton;
        private System.Windows.Forms.ToolStripButton pauseButton;
        private System.Windows.Forms.ToolStripButton disconnectButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripButton connectButton;
        private System.Windows.Forms.StatusStrip statusStrip;
        private System.Windows.Forms.ToolStripButton brainStepButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripButton showVisualizationButton;
        private System.Windows.Forms.ToolStripButton loadBlueprintButton;
        private System.Windows.Forms.ToolStripButton clearBlueprintButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripButton bodyStepButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripButton newBlueprintButton;
        private System.Windows.Forms.ToolStripButton openBlueprintButton;
        private System.Windows.Forms.ToolStripButton saveBlueprintButton;
        private System.Windows.Forms.ToolStripButton saveAsBlueprintButton;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.OpenFileDialog openFileDialog;
        private System.Windows.Forms.SaveFileDialog saveFileDialog;
        private System.Windows.Forms.ColorDialog colorDialog1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripButton regularCheckpointingButton;
        private System.Windows.Forms.ToolStripTextBox checkpointingIntervalTextBox;
    }
}

