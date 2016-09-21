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
            this.CoreProcessArgumentsTextBox = new System.Windows.Forms.TextBox();
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
            this.label1.Location = new System.Drawing.Point(12, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(115, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Core Process Directory";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(13, 52);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(123, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Core Process Arguments";
            // 
            // CoreProcessArgumentsTextBox
            // 
            this.CoreProcessArgumentsTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CoreProcessArgumentsTextBox.Location = new System.Drawing.Point(12, 68);
            this.CoreProcessArgumentsTextBox.Multiline = true;
            this.CoreProcessArgumentsTextBox.Name = "CoreProcessArgumentsTextBox";
            this.CoreProcessArgumentsTextBox.Size = new System.Drawing.Size(430, 60);
            this.CoreProcessArgumentsTextBox.TabIndex = 3;
            this.CoreProcessArgumentsTextBox.Text = "core +p4 ++ppn 4 +noisomalloc +LBCommOff +balancer DistributedLB +cs +ss ++verbos" +
    "e ++server ++server-port 46324\"";
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(454, 323);
            this.Controls.Add(this.CoreProcessArgumentsTextBox);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.coreProcessDirectoryTextBox);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "SettingsForm";
            this.Text = "Settings";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox coreProcessDirectoryTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox CoreProcessArgumentsTextBox;
    }
}