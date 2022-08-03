using System.Windows.Forms;

namespace MaceEvolve
{
    partial class MainForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            System.Diagnostics.Stopwatch stopwatch1 = new System.Diagnostics.Stopwatch();
            this.StartButton = new System.Windows.Forms.Button();
            this.StopButton = new System.Windows.Forms.Button();
            this.ResetButton = new System.Windows.Forms.Button();
            this.NextGenButton = new System.Windows.Forms.Button();
            this.MainGameHost = new MaceEvolve.Controls.GameHost();
            this.SuspendLayout();
            // 
            // StartButton
            // 
            this.StartButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.StartButton.Location = new System.Drawing.Point(454, 12);
            this.StartButton.Name = "StartButton";
            this.StartButton.Size = new System.Drawing.Size(75, 32);
            this.StartButton.TabIndex = 1;
            this.StartButton.Text = "Start";
            this.StartButton.UseVisualStyleBackColor = true;
            this.StartButton.Click += new System.EventHandler(this.StartButton_Click);
            // 
            // StopButton
            // 
            this.StopButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.StopButton.Location = new System.Drawing.Point(535, 12);
            this.StopButton.Name = "StopButton";
            this.StopButton.Size = new System.Drawing.Size(75, 32);
            this.StopButton.TabIndex = 1;
            this.StopButton.Text = "Stop";
            this.StopButton.UseVisualStyleBackColor = true;
            this.StopButton.Click += new System.EventHandler(this.StopButton_Click);
            // 
            // ResetButton
            // 
            this.ResetButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ResetButton.Location = new System.Drawing.Point(616, 12);
            this.ResetButton.Name = "ResetButton";
            this.ResetButton.Size = new System.Drawing.Size(75, 32);
            this.ResetButton.TabIndex = 1;
            this.ResetButton.Text = "Reset";
            this.ResetButton.UseVisualStyleBackColor = true;
            this.ResetButton.Click += new System.EventHandler(this.ResetButton_Click);
            // 
            // NextGenButton
            // 
            this.NextGenButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.NextGenButton.Location = new System.Drawing.Point(697, 12);
            this.NextGenButton.Name = "NextGenButton";
            this.NextGenButton.Size = new System.Drawing.Size(75, 32);
            this.NextGenButton.TabIndex = 1;
            this.NextGenButton.Text = "Next Gen";
            this.NextGenButton.UseVisualStyleBackColor = true;
            this.NextGenButton.Click += new System.EventHandler(this.NextGenButton_Click);
            // 
            // MainGameHost
            // 
            this.MainGameHost.ConnectionWeightBound = 4D;
            this.MainGameHost.CreatureSpeed = 2.75D;
            this.MainGameHost.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainGameHost.Location = new System.Drawing.Point(0, 0);
            this.MainGameHost.MaxCreatureAmount = 150;
            this.MainGameHost.MaxCreatureConnections = 32;
            this.MainGameHost.MaxCreatureEnergy = 150D;
            this.MainGameHost.MaxCreatureProcessNodes = 8;
            this.MainGameHost.MaxFoodAmount = 350;
            this.MainGameHost.MinCreatureConnections = 32;
            this.MainGameHost.MutationChance = 0.15D;
            this.MainGameHost.Name = "MainGameHost";
            this.MainGameHost.NewGenerationInterval = 12D;
            this.MainGameHost.SecondsUntilNewGeneration = 12D;
            this.MainGameHost.Size = new System.Drawing.Size(784, 661);
            this.MainGameHost.Stopwatch = stopwatch1;
            this.MainGameHost.SuccessBounds = new System.Drawing.Rectangle(0, 0, 150, 150);
            this.MainGameHost.SuccessfulCreaturesPercentile = 10D;
            this.MainGameHost.TabIndex = 2;
            this.MainGameHost.TargetFPS = 60;
            this.MainGameHost.WorldBounds = new System.Drawing.Rectangle(0, 0, 784, 661);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(784, 661);
            this.Controls.Add(this.NextGenButton);
            this.Controls.Add(this.ResetButton);
            this.Controls.Add(this.StopButton);
            this.Controls.Add(this.StartButton);
            this.Controls.Add(this.MainGameHost);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Mace Evolution";
            this.ResumeLayout(false);

        }

        #endregion
        private Button StartButton;
        private Button StopButton;
        private Button ResetButton;
        private Button NextGenButton;
        private Controls.GameHost MainGameHost;
    }
}