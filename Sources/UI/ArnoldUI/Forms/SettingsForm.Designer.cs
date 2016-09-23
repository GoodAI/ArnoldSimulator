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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label5 = new System.Windows.Forms.Label();
            this.loadBalancingIntervalTextBox = new System.Windows.Forms.TextBox();
            this.loadBalancingEnabledCheckBox = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // coreProcessDirectoryTextBox
            // 
            this.coreProcessDirectoryTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.coreProcessDirectoryTextBox.Location = new System.Drawing.Point(81, 22);
            this.coreProcessDirectoryTextBox.Name = "coreProcessDirectoryTextBox";
            this.coreProcessDirectoryTextBox.Size = new System.Drawing.Size(329, 20);
            this.coreProcessDirectoryTextBox.TabIndex = 0;
            this.coreProcessDirectoryTextBox.Text = "../../../../core/core/debug";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(6, 25);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(49, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Directory";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 75);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(57, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Arguments";
            // 
            // coreProcessArgumentsTextBox
            // 
            this.coreProcessArgumentsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.coreProcessArgumentsTextBox.Location = new System.Drawing.Point(7, 91);
            this.coreProcessArgumentsTextBox.Multiline = true;
            this.coreProcessArgumentsTextBox.Name = "coreProcessArgumentsTextBox";
            this.coreProcessArgumentsTextBox.Size = new System.Drawing.Size(403, 60);
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
            this.substitutedArgumentsTextBox.Location = new System.Drawing.Point(7, 170);
            this.substitutedArgumentsTextBox.Multiline = true;
            this.substitutedArgumentsTextBox.Name = "substitutedArgumentsTextBox";
            this.substitutedArgumentsTextBox.Size = new System.Drawing.Size(403, 60);
            this.substitutedArgumentsTextBox.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 154);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(139, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Arguments after Substitution";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 51);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(26, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "Port";
            // 
            // portTextBox
            // 
            this.portTextBox.Location = new System.Drawing.Point(81, 48);
            this.portTextBox.MaxLength = 10;
            this.portTextBox.Name = "portTextBox";
            this.portTextBox.Size = new System.Drawing.Size(50, 20);
            this.portTextBox.TabIndex = 6;
            this.portTextBox.Text = "46324";
            this.portTextBox.TextChanged += new System.EventHandler(this.portTextBox_TextChanged);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.coreProcessDirectoryTextBox);
            this.groupBox1.Controls.Add(this.portTextBox);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.substitutedArgumentsTextBox);
            this.groupBox1.Controls.Add(this.coreProcessArgumentsTextBox);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Location = new System.Drawing.Point(3, 5);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(422, 238);
            this.groupBox1.TabIndex = 8;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Core Process Options";
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.loadBalancingIntervalTextBox);
            this.groupBox2.Controls.Add(this.loadBalancingEnabledCheckBox);
            this.groupBox2.Location = new System.Drawing.Point(3, 249);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(422, 100);
            this.groupBox2.TabIndex = 9;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Core Runtime Options";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(6, 43);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(133, 13);
            this.label5.TabIndex = 9;
            this.label5.Text = "Load Balancing Interval (s)";
            // 
            // loadBalancingIntervalTextBox
            // 
            this.loadBalancingIntervalTextBox.Location = new System.Drawing.Point(151, 40);
            this.loadBalancingIntervalTextBox.MaxLength = 10;
            this.loadBalancingIntervalTextBox.Name = "loadBalancingIntervalTextBox";
            this.loadBalancingIntervalTextBox.Size = new System.Drawing.Size(55, 20);
            this.loadBalancingIntervalTextBox.TabIndex = 8;
            this.loadBalancingIntervalTextBox.Text = "15";
            this.loadBalancingIntervalTextBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.loadBalancingIntervalTextBox.TextChanged += new System.EventHandler(this.loadBalancingIntervalTextBox_TextChanged);
            this.loadBalancingIntervalTextBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.loadBalancingIntervalTextBox_KeyDown);
            // 
            // loadBalancingEnabledCheckBox
            // 
            this.loadBalancingEnabledCheckBox.AutoSize = true;
            this.loadBalancingEnabledCheckBox.Location = new System.Drawing.Point(9, 20);
            this.loadBalancingEnabledCheckBox.Name = "loadBalancingEnabledCheckBox";
            this.loadBalancingEnabledCheckBox.Size = new System.Drawing.Size(136, 17);
            this.loadBalancingEnabledCheckBox.TabIndex = 0;
            this.loadBalancingEnabledCheckBox.Text = "Enable Load Balancing";
            this.loadBalancingEnabledCheckBox.UseVisualStyleBackColor = true;
            this.loadBalancingEnabledCheckBox.CheckedChanged += new System.EventHandler(this.loadBalancingEnabledCheckBox_CheckedChanged);
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(430, 495);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.HideOnClose = true;
            this.Name = "SettingsForm";
            this.Text = "Settings";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
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
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox loadBalancingIntervalTextBox;
        private System.Windows.Forms.CheckBox loadBalancingEnabledCheckBox;
    }
}