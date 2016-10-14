namespace GoodAI.Arnold.Forms
{
    partial class SettingsForm
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
            this.coreProcessDirectoryTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.coreProcessArgumentsTextBox = new System.Windows.Forms.TextBox();
            this.substitutedArgumentsTextBox = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.portTextBox = new System.Windows.Forms.TextBox();
            this.localOptionsGroupBox = new System.Windows.Forms.GroupBox();
            this.runtimeOptionsGroupBox = new System.Windows.Forms.GroupBox();
            this.label5 = new System.Windows.Forms.Label();
            this.loadBalancingIntervalTextBox = new System.Windows.Forms.TextBox();
            this.loadBalancingEnabledCheckBox = new System.Windows.Forms.CheckBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.remoteCoreRadioButton = new System.Windows.Forms.RadioButton();
            this.localCoreRadioButton = new System.Windows.Forms.RadioButton();
            this.remoteOptionsGroupBox = new System.Windows.Forms.GroupBox();
            this.label6 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.remoteCoreHostTextBox = new System.Windows.Forms.TextBox();
            this.remoteCorePortTextBox = new System.Windows.Forms.TextBox();
            this.localOptionsGroupBox.SuspendLayout();
            this.runtimeOptionsGroupBox.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.remoteOptionsGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // coreProcessDirectoryTextBox
            // 
            this.coreProcessDirectoryTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.coreProcessDirectoryTextBox.Location = new System.Drawing.Point(94, 25);
            this.coreProcessDirectoryTextBox.Name = "coreProcessDirectoryTextBox";
            this.coreProcessDirectoryTextBox.Size = new System.Drawing.Size(383, 23);
            this.coreProcessDirectoryTextBox.TabIndex = 0;
            this.coreProcessDirectoryTextBox.Text = "../../../../core/core/debug";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(7, 29);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(55, 15);
            this.label1.TabIndex = 1;
            this.label1.Text = "Directory";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(7, 87);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(66, 15);
            this.label2.TabIndex = 2;
            this.label2.Text = "Arguments";
            // 
            // coreProcessArgumentsTextBox
            // 
            this.coreProcessArgumentsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.coreProcessArgumentsTextBox.Location = new System.Drawing.Point(8, 105);
            this.coreProcessArgumentsTextBox.Multiline = true;
            this.coreProcessArgumentsTextBox.Name = "coreProcessArgumentsTextBox";
            this.coreProcessArgumentsTextBox.Size = new System.Drawing.Size(469, 69);
            this.coreProcessArgumentsTextBox.TabIndex = 3;
            this.coreProcessArgumentsTextBox.Text = "core +p4 ++ppn 4 +noisomalloc +LBCommOff +balancer DistributedLB +cs +ss ++verbos" +
    "e ++server ++server-port {Port}";
            this.coreProcessArgumentsTextBox.TextChanged += new System.EventHandler(this.coreProcessArgumentsTextBox_TextChanged);
            // 
            // substitutedArgumentsTextBox
            // 
            this.substitutedArgumentsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.substitutedArgumentsTextBox.Enabled = false;
            this.substitutedArgumentsTextBox.Location = new System.Drawing.Point(8, 196);
            this.substitutedArgumentsTextBox.Multiline = true;
            this.substitutedArgumentsTextBox.Name = "substitutedArgumentsTextBox";
            this.substitutedArgumentsTextBox.Size = new System.Drawing.Size(469, 69);
            this.substitutedArgumentsTextBox.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(7, 178);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(159, 15);
            this.label3.TabIndex = 4;
            this.label3.Text = "Arguments after substitution";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(7, 59);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(29, 15);
            this.label4.TabIndex = 7;
            this.label4.Text = "Port";
            // 
            // portTextBox
            // 
            this.portTextBox.Location = new System.Drawing.Point(94, 55);
            this.portTextBox.MaxLength = 10;
            this.portTextBox.Name = "portTextBox";
            this.portTextBox.Size = new System.Drawing.Size(58, 23);
            this.portTextBox.TabIndex = 6;
            this.portTextBox.Text = "46324";
            this.portTextBox.TextChanged += new System.EventHandler(this.portTextBox_TextChanged);
            // 
            // localOptionsGroupBox
            // 
            this.localOptionsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.localOptionsGroupBox.Controls.Add(this.label1);
            this.localOptionsGroupBox.Controls.Add(this.label4);
            this.localOptionsGroupBox.Controls.Add(this.coreProcessDirectoryTextBox);
            this.localOptionsGroupBox.Controls.Add(this.portTextBox);
            this.localOptionsGroupBox.Controls.Add(this.label2);
            this.localOptionsGroupBox.Controls.Add(this.substitutedArgumentsTextBox);
            this.localOptionsGroupBox.Controls.Add(this.coreProcessArgumentsTextBox);
            this.localOptionsGroupBox.Controls.Add(this.label3);
            this.localOptionsGroupBox.Location = new System.Drawing.Point(3, 92);
            this.localOptionsGroupBox.Name = "localOptionsGroupBox";
            this.localOptionsGroupBox.Size = new System.Drawing.Size(492, 275);
            this.localOptionsGroupBox.TabIndex = 8;
            this.localOptionsGroupBox.TabStop = false;
            this.localOptionsGroupBox.Text = "Local core options";
            // 
            // runtimeOptionsGroupBox
            // 
            this.runtimeOptionsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.runtimeOptionsGroupBox.Controls.Add(this.label5);
            this.runtimeOptionsGroupBox.Controls.Add(this.loadBalancingIntervalTextBox);
            this.runtimeOptionsGroupBox.Controls.Add(this.loadBalancingEnabledCheckBox);
            this.runtimeOptionsGroupBox.Location = new System.Drawing.Point(3, 470);
            this.runtimeOptionsGroupBox.Name = "runtimeOptionsGroupBox";
            this.runtimeOptionsGroupBox.Size = new System.Drawing.Size(492, 87);
            this.runtimeOptionsGroupBox.TabIndex = 9;
            this.runtimeOptionsGroupBox.TabStop = false;
            this.runtimeOptionsGroupBox.Text = "Core runtime options";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(7, 50);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(146, 15);
            this.label5.TabIndex = 9;
            this.label5.Text = "Load balancing interval (s)";
            // 
            // loadBalancingIntervalTextBox
            // 
            this.loadBalancingIntervalTextBox.Location = new System.Drawing.Point(176, 46);
            this.loadBalancingIntervalTextBox.MaxLength = 10;
            this.loadBalancingIntervalTextBox.Name = "loadBalancingIntervalTextBox";
            this.loadBalancingIntervalTextBox.Size = new System.Drawing.Size(63, 23);
            this.loadBalancingIntervalTextBox.TabIndex = 8;
            this.loadBalancingIntervalTextBox.Text = "15";
            this.loadBalancingIntervalTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.loadBalancingIntervalTextBox.TextChanged += new System.EventHandler(this.loadBalancingIntervalTextBox_TextChanged);
            this.loadBalancingIntervalTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.loadBalancingIntervalTextBox_KeyDown);
            // 
            // loadBalancingEnabledCheckBox
            // 
            this.loadBalancingEnabledCheckBox.AutoSize = true;
            this.loadBalancingEnabledCheckBox.Checked = true;
            this.loadBalancingEnabledCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.loadBalancingEnabledCheckBox.Location = new System.Drawing.Point(10, 23);
            this.loadBalancingEnabledCheckBox.Name = "loadBalancingEnabledCheckBox";
            this.loadBalancingEnabledCheckBox.Size = new System.Drawing.Size(142, 19);
            this.loadBalancingEnabledCheckBox.TabIndex = 0;
            this.loadBalancingEnabledCheckBox.Text = "Enable load balancing";
            this.loadBalancingEnabledCheckBox.UseVisualStyleBackColor = true;
            this.loadBalancingEnabledCheckBox.CheckedChanged += new System.EventHandler(this.loadBalancingEnabledCheckBox_CheckedChanged);
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this.remoteCoreRadioButton);
            this.groupBox3.Controls.Add(this.localCoreRadioButton);
            this.groupBox3.Location = new System.Drawing.Point(3, 3);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(492, 83);
            this.groupBox3.TabIndex = 9;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Core mode";
            // 
            // remoteCoreRadioButton
            // 
            this.remoteCoreRadioButton.AutoSize = true;
            this.remoteCoreRadioButton.Location = new System.Drawing.Point(10, 45);
            this.remoteCoreRadioButton.Name = "remoteCoreRadioButton";
            this.remoteCoreRadioButton.Size = new System.Drawing.Size(151, 19);
            this.remoteCoreRadioButton.TabIndex = 11;
            this.remoteCoreRadioButton.Text = "Connect to remote core";
            this.remoteCoreRadioButton.UseVisualStyleBackColor = true;
            this.remoteCoreRadioButton.CheckedChanged += new System.EventHandler(this.remoteCoreRadioButton_CheckedChanged);
            // 
            // localCoreRadioButton
            // 
            this.localCoreRadioButton.AutoSize = true;
            this.localCoreRadioButton.Checked = true;
            this.localCoreRadioButton.Location = new System.Drawing.Point(10, 22);
            this.localCoreRadioButton.Name = "localCoreRadioButton";
            this.localCoreRadioButton.Size = new System.Drawing.Size(128, 19);
            this.localCoreRadioButton.TabIndex = 10;
            this.localCoreRadioButton.TabStop = true;
            this.localCoreRadioButton.Text = "Local machine core";
            this.localCoreRadioButton.UseVisualStyleBackColor = true;
            this.localCoreRadioButton.CheckedChanged += new System.EventHandler(this.localCoreRadioButton_CheckedChanged);
            // 
            // remoteOptionsGroupBox
            // 
            this.remoteOptionsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.remoteOptionsGroupBox.Controls.Add(this.label6);
            this.remoteOptionsGroupBox.Controls.Add(this.label7);
            this.remoteOptionsGroupBox.Controls.Add(this.remoteCoreHostTextBox);
            this.remoteOptionsGroupBox.Controls.Add(this.remoteCorePortTextBox);
            this.remoteOptionsGroupBox.Location = new System.Drawing.Point(3, 373);
            this.remoteOptionsGroupBox.Name = "remoteOptionsGroupBox";
            this.remoteOptionsGroupBox.Size = new System.Drawing.Size(492, 91);
            this.remoteOptionsGroupBox.TabIndex = 8;
            this.remoteOptionsGroupBox.TabStop = false;
            this.remoteOptionsGroupBox.Text = "Remote core options";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(7, 29);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(32, 15);
            this.label6.TabIndex = 1;
            this.label6.Text = "Host";
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(7, 59);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(29, 15);
            this.label7.TabIndex = 7;
            this.label7.Text = "Port";
            // 
            // remoteCoreHostTextBox
            // 
            this.remoteCoreHostTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.remoteCoreHostTextBox.Location = new System.Drawing.Point(94, 25);
            this.remoteCoreHostTextBox.Name = "remoteCoreHostTextBox";
            this.remoteCoreHostTextBox.Size = new System.Drawing.Size(383, 23);
            this.remoteCoreHostTextBox.TabIndex = 0;
            this.remoteCoreHostTextBox.Text = "localhost";
            // 
            // remoteCorePortTextBox
            // 
            this.remoteCorePortTextBox.Location = new System.Drawing.Point(94, 55);
            this.remoteCorePortTextBox.MaxLength = 10;
            this.remoteCorePortTextBox.Name = "remoteCorePortTextBox";
            this.remoteCorePortTextBox.Size = new System.Drawing.Size(58, 23);
            this.remoteCorePortTextBox.TabIndex = 6;
            this.remoteCorePortTextBox.Text = "46324";
            this.remoteCorePortTextBox.TextChanged += new System.EventHandler(this.portTextBox_TextChanged);
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(502, 566);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.runtimeOptionsGroupBox);
            this.Controls.Add(this.remoteOptionsGroupBox);
            this.Controls.Add(this.localOptionsGroupBox);
            this.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.HideOnClose = true;
            this.Name = "SettingsForm";
            this.Text = "Settings";
            this.Load += new System.EventHandler(this.SettingsForm_Load);
            this.localOptionsGroupBox.ResumeLayout(false);
            this.localOptionsGroupBox.PerformLayout();
            this.runtimeOptionsGroupBox.ResumeLayout(false);
            this.runtimeOptionsGroupBox.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.remoteOptionsGroupBox.ResumeLayout(false);
            this.remoteOptionsGroupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TextBox coreProcessDirectoryTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox coreProcessArgumentsTextBox;
        private System.Windows.Forms.TextBox substitutedArgumentsTextBox;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox portTextBox;
        private System.Windows.Forms.GroupBox localOptionsGroupBox;
        private System.Windows.Forms.GroupBox runtimeOptionsGroupBox;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox loadBalancingIntervalTextBox;
        private System.Windows.Forms.CheckBox loadBalancingEnabledCheckBox;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.RadioButton remoteCoreRadioButton;
        private System.Windows.Forms.RadioButton localCoreRadioButton;
        private System.Windows.Forms.GroupBox remoteOptionsGroupBox;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.TextBox remoteCoreHostTextBox;
        private System.Windows.Forms.TextBox remoteCorePortTextBox;
    }
}