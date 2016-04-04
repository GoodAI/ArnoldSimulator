namespace GoodAI.Arnold.Forms
{
    partial class GraphForm
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
            Graph.Compatibility.AlwaysCompatible alwaysCompatible5 = new Graph.Compatibility.AlwaysCompatible();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(GraphForm));
            this.graphControl = new Graph.GraphControl();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.addRegionButton = new System.Windows.Forms.Button();
            this.addInputButton = new System.Windows.Forms.Button();
            this.removeInputButton = new System.Windows.Forms.Button();
            this.addOutputButton = new System.Windows.Forms.Button();
            this.removeOutputButton = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // graphControl
            // 
            this.graphControl.AllowDrop = true;
            this.graphControl.BackColor = System.Drawing.SystemColors.ControlDark;
            this.graphControl.CompatibilityStrategy = alwaysCompatible5;
            this.graphControl.ConnectorSafeBounds = 0;
            resources.ApplyResources(this.graphControl, "graphControl");
            this.graphControl.FocusElement = null;
            this.graphControl.HighlightCompatible = true;
            this.graphControl.LargeGridStep = 128F;
            this.graphControl.LargeStepGridColor = System.Drawing.Color.FromArgb(((int)(((byte)(180)))), ((int)(((byte)(180)))), ((int)(((byte)(180)))));
            this.graphControl.Name = "graphControl";
            this.graphControl.ShowLabels = true;
            this.graphControl.SmallGridStep = 16F;
            this.graphControl.SmallStepGridColor = System.Drawing.Color.DarkGray;
            this.graphControl.MouseDown += new System.Windows.Forms.MouseEventHandler(this.graphControl_MouseDown);
            this.graphControl.MouseLeave += new System.EventHandler(this.graphControl_MouseLeave);
            // 
            // splitContainer1
            // 
            resources.ApplyResources(this.splitContainer1, "splitContainer1");
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.removeOutputButton);
            this.splitContainer1.Panel1.Controls.Add(this.addOutputButton);
            this.splitContainer1.Panel1.Controls.Add(this.removeInputButton);
            this.splitContainer1.Panel1.Controls.Add(this.addInputButton);
            this.splitContainer1.Panel1.Controls.Add(this.addRegionButton);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.graphControl);
            // 
            // addRegionButton
            // 
            resources.ApplyResources(this.addRegionButton, "addRegionButton");
            this.addRegionButton.Name = "addRegionButton";
            this.addRegionButton.UseVisualStyleBackColor = true;
            this.addRegionButton.Click += new System.EventHandler(this.addRegionButton_Click);
            // 
            // addInputButton
            // 
            resources.ApplyResources(this.addInputButton, "addInputButton");
            this.addInputButton.Name = "addInputButton";
            this.addInputButton.UseVisualStyleBackColor = true;
            this.addInputButton.Click += new System.EventHandler(this.addInputButton_Click);
            // 
            // removeInputButton
            // 
            resources.ApplyResources(this.removeInputButton, "removeInputButton");
            this.removeInputButton.Name = "removeInputButton";
            this.removeInputButton.UseVisualStyleBackColor = true;
            this.removeInputButton.Click += new System.EventHandler(this.removeInputButton_Click);
            // 
            // addOutputButton
            // 
            resources.ApplyResources(this.addOutputButton, "addOutputButton");
            this.addOutputButton.Name = "addOutputButton";
            this.addOutputButton.UseVisualStyleBackColor = true;
            this.addOutputButton.Click += new System.EventHandler(this.addOutputButton_Click);
            // 
            // removeOutputButton
            // 
            resources.ApplyResources(this.removeOutputButton, "removeOutputButton");
            this.removeOutputButton.Name = "removeOutputButton";
            this.removeOutputButton.UseVisualStyleBackColor = true;
            this.removeOutputButton.Click += new System.EventHandler(this.removeOutputButton_Click);
            // 
            // GraphForm
            // 
            resources.ApplyResources(this, "$this");
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Name = "GraphForm";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private Graph.GraphControl graphControl;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Button addRegionButton;
        private System.Windows.Forms.Button removeOutputButton;
        private System.Windows.Forms.Button addOutputButton;
        private System.Windows.Forms.Button removeInputButton;
        private System.Windows.Forms.Button addInputButton;
    }
}