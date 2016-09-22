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
            this.SuspendLayout();
            // 
            // coreProcessDirectoryTextBox
            // 
            this.coreProcessDirectoryTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.coreProcessDirectoryTextBox.Location = new System.Drawing.Point(12, 25);
            this.coreProcessDirectoryTextBox.Name = "coreProcessDirectoryTextBox";
            this.coreProcessDirectoryTextBox.Size = new System.Drawing.Size(430, 20);
            this.coreProcessDirectoryTextBox.TabIndex = 0;
            this.coreProcessDirectoryTextBox.Text = "../../../../core/core/debug";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(115, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Core Process Directory";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 87);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(123, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Core Process Arguments";
            // 
            // coreProcessArgumentsTextBox
            // 
            this.coreProcessArgumentsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.coreProcessArgumentsTextBox.Location = new System.Drawing.Point(11, 103);
            this.coreProcessArgumentsTextBox.Multiline = true;
            this.coreProcessArgumentsTextBox.Name = "coreProcessArgumentsTextBox";
            this.coreProcessArgumentsTextBox.Size = new System.Drawing.Size(430, 60);
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
            this.substitutedArgumentsTextBox.Location = new System.Drawing.Point(11, 182);
            this.substitutedArgumentsTextBox.Multiline = true;
            this.substitutedArgumentsTextBox.Name = "substitutedArgumentsTextBox";
            this.substitutedArgumentsTextBox.Size = new System.Drawing.Size(430, 60);
            this.substitutedArgumentsTextBox.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 166);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(139, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Arguments after Substitution";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(9, 48);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(26, 13);
            this.label4.TabIndex = 7;
            this.label4.Text = "Port";
            // 
            // portTextBox
            // 
            this.portTextBox.Location = new System.Drawing.Point(11, 64);
            this.portTextBox.MaxLength = 10;
            this.portTextBox.Name = "portTextBox";
            this.portTextBox.Size = new System.Drawing.Size(55, 20);
            this.portTextBox.TabIndex = 6;
            this.portTextBox.Text = "46324";
            this.portTextBox.TextChanged += new System.EventHandler(this.portTextBox_TextChanged);
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(454, 323);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.portTextBox);
            this.Controls.Add(this.substitutedArgumentsTextBox);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.coreProcessArgumentsTextBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.coreProcessDirectoryTextBox);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.HideOnClose = true;
            this.Name = "SettingsForm";
            this.Text = "Settings";
            this.ResumeLayout(false);
            this.PerformLayout();

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
    }
}