namespace GoodAI.Arnold.Forms
{
    partial class JsonEditForm
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
            this.content = new ScintillaNET.Scintilla();
            this.SuspendLayout();
            // 
            // content
            // 
            this.content.Dock = System.Windows.Forms.DockStyle.Fill;
            this.content.FontQuality = ScintillaNET.FontQuality.AntiAliased;
            this.content.IndentationGuides = ScintillaNET.IndentView.Real;
            this.content.Lexer = ScintillaNET.Lexer.Cpp;
            this.content.Location = new System.Drawing.Point(0, 0);
            this.content.Name = "content";
            this.content.ScrollWidth = 800;
            this.content.Size = new System.Drawing.Size(1114, 606);
            this.content.TabIndex = 0;
            this.content.UseTabs = false;
            // 
            // JsonEditForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1114, 606);
            this.Controls.Add(this.content);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "JsonEditForm";
            this.Text = "JsonEditForm";
            this.ResumeLayout(false);

        }

        #endregion

        private ScintillaNET.Scintilla content;
    }
}