using System.Windows.Forms;

namespace MaceEvolve.WinForms
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
            this.components = new System.ComponentModel.Container();
            this.StartButton = new System.Windows.Forms.Button();
            this.StopButton = new System.Windows.Forms.Button();
            this.ResetButton = new System.Windows.Forms.Button();
            this.btnTrackBestCreature = new System.Windows.Forms.Button();
            this.lblGenEndsIn = new System.Windows.Forms.Label();
            this.lblGenerationCount = new System.Windows.Forms.Label();
            this.lblSimulationRunning = new System.Windows.Forms.Label();
            this.btnForwardGen = new System.Windows.Forms.Button();
            this.btnForwardGens = new System.Windows.Forms.Button();
            this.btnForwardAllGens = new System.Windows.Forms.Button();
            this.GameTimer = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // StartButton
            // 
            this.StartButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.StartButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.StartButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.StartButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.StartButton.Location = new System.Drawing.Point(377, 14);
            this.StartButton.Name = "StartButton";
            this.StartButton.Size = new System.Drawing.Size(75, 32);
            this.StartButton.TabIndex = 1;
            this.StartButton.Text = "Start";
            this.StartButton.UseVisualStyleBackColor = false;
            this.StartButton.Click += new System.EventHandler(this.StartButton_Click);
            // 
            // StopButton
            // 
            this.StopButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.StopButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.StopButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.StopButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.StopButton.Location = new System.Drawing.Point(458, 14);
            this.StopButton.Name = "StopButton";
            this.StopButton.Size = new System.Drawing.Size(75, 32);
            this.StopButton.TabIndex = 1;
            this.StopButton.Text = "Stop";
            this.StopButton.UseVisualStyleBackColor = false;
            this.StopButton.Click += new System.EventHandler(this.StopButton_Click);
            // 
            // ResetButton
            // 
            this.ResetButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ResetButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.ResetButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.ResetButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ResetButton.Location = new System.Drawing.Point(539, 14);
            this.ResetButton.Name = "ResetButton";
            this.ResetButton.Size = new System.Drawing.Size(75, 32);
            this.ResetButton.TabIndex = 1;
            this.ResetButton.Text = "Reset";
            this.ResetButton.UseVisualStyleBackColor = false;
            this.ResetButton.Click += new System.EventHandler(this.ResetButton_Click);
            // 
            // btnTrackBestCreature
            // 
            this.btnTrackBestCreature.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnTrackBestCreature.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.btnTrackBestCreature.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnTrackBestCreature.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTrackBestCreature.Location = new System.Drawing.Point(620, 14);
            this.btnTrackBestCreature.Name = "btnTrackBestCreature";
            this.btnTrackBestCreature.Size = new System.Drawing.Size(156, 32);
            this.btnTrackBestCreature.TabIndex = 2;
            this.btnTrackBestCreature.Text = "Track Best Creature";
            this.btnTrackBestCreature.UseVisualStyleBackColor = false;
            this.btnTrackBestCreature.Click += new System.EventHandler(this.btnTrackBestCreature_Click);
            // 
            // lblGenEndsIn
            // 
            this.lblGenEndsIn.AutoSize = true;
            this.lblGenEndsIn.BackColor = System.Drawing.Color.Transparent;
            this.lblGenEndsIn.Font = new System.Drawing.Font("Yu Gothic UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblGenEndsIn.ForeColor = System.Drawing.Color.White;
            this.lblGenEndsIn.Location = new System.Drawing.Point(15, 72);
            this.lblGenEndsIn.Name = "lblGenEndsIn";
            this.lblGenEndsIn.Size = new System.Drawing.Size(80, 21);
            this.lblGenEndsIn.TabIndex = 4;
            this.lblGenEndsIn.Text = "Ends In 0s";
            // 
            // lblGenerationCount
            // 
            this.lblGenerationCount.AutoSize = true;
            this.lblGenerationCount.BackColor = System.Drawing.Color.Transparent;
            this.lblGenerationCount.Font = new System.Drawing.Font("Yu Gothic UI", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblGenerationCount.ForeColor = System.Drawing.Color.White;
            this.lblGenerationCount.Location = new System.Drawing.Point(12, 40);
            this.lblGenerationCount.Name = "lblGenerationCount";
            this.lblGenerationCount.Size = new System.Drawing.Size(75, 32);
            this.lblGenerationCount.TabIndex = 5;
            this.lblGenerationCount.Text = "Gen 1";
            // 
            // lblSimulationRunning
            // 
            this.lblSimulationRunning.AutoSize = true;
            this.lblSimulationRunning.BackColor = System.Drawing.Color.Transparent;
            this.lblSimulationRunning.Font = new System.Drawing.Font("Yu Gothic UI", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblSimulationRunning.ForeColor = System.Drawing.Color.White;
            this.lblSimulationRunning.Location = new System.Drawing.Point(12, 9);
            this.lblSimulationRunning.Name = "lblSimulationRunning";
            this.lblSimulationRunning.Size = new System.Drawing.Size(105, 32);
            this.lblSimulationRunning.TabIndex = 6;
            this.lblSimulationRunning.Text = "Stopped";
            // 
            // btnForwardGen
            // 
            this.btnForwardGen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnForwardGen.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.btnForwardGen.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnForwardGen.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnForwardGen.Location = new System.Drawing.Point(620, 52);
            this.btnForwardGen.Name = "btnForwardGen";
            this.btnForwardGen.Size = new System.Drawing.Size(156, 32);
            this.btnForwardGen.TabIndex = 1;
            this.btnForwardGen.Text = "Forward 1 Gen";
            this.btnForwardGen.UseVisualStyleBackColor = false;
            this.btnForwardGen.Click += new System.EventHandler(this.btnForwardGen_Click);
            // 
            // btnForwardGens
            // 
            this.btnForwardGens.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnForwardGens.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.btnForwardGens.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnForwardGens.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnForwardGens.Location = new System.Drawing.Point(620, 90);
            this.btnForwardGens.Name = "btnForwardGens";
            this.btnForwardGens.Size = new System.Drawing.Size(156, 32);
            this.btnForwardGens.TabIndex = 1;
            this.btnForwardGens.Text = "Forward 100 Gens";
            this.btnForwardGens.UseVisualStyleBackColor = false;
            this.btnForwardGens.Click += new System.EventHandler(this.btnForwardGens_Click);
            // 
            // btnForwardAllGens
            // 
            this.btnForwardAllGens.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnForwardAllGens.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.btnForwardAllGens.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnForwardAllGens.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnForwardAllGens.Location = new System.Drawing.Point(620, 128);
            this.btnForwardAllGens.Name = "btnForwardAllGens";
            this.btnForwardAllGens.Size = new System.Drawing.Size(156, 32);
            this.btnForwardAllGens.TabIndex = 1;
            this.btnForwardAllGens.Text = "Forward All Gens";
            this.btnForwardAllGens.UseVisualStyleBackColor = false;
            this.btnForwardAllGens.Click += new System.EventHandler(this.btnForwardAllGens_Click);
            // 
            // GameTimer
            // 
            this.GameTimer.Enabled = true;
            this.GameTimer.Tick += new System.EventHandler(this.GameTimer_Tick);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(32)))), ((int)(((byte)(32)))));
            this.ClientSize = new System.Drawing.Size(784, 661);
            this.Controls.Add(this.lblSimulationRunning);
            this.Controls.Add(this.lblGenEndsIn);
            this.Controls.Add(this.lblGenerationCount);
            this.Controls.Add(this.btnTrackBestCreature);
            this.Controls.Add(this.ResetButton);
            this.Controls.Add(this.StopButton);
            this.Controls.Add(this.btnForwardAllGens);
            this.Controls.Add(this.btnForwardGens);
            this.Controls.Add(this.btnForwardGen);
            this.Controls.Add(this.StartButton);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Mace Evolution";
            this.Load += new System.EventHandler(this.MainForm_Load);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.MainForm_Paint);
            this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.MainForm_MouseClick);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private Button StartButton;
        private Button StopButton;
        private Button ResetButton;
        private Button btnTrackBestCreature;
        private Label lblGenEndsIn;
        private Label lblGenerationCount;
        private Label lblSimulationRunning;
        private Button btnForwardGen;
        private Button btnForwardGens;
        private Button btnForwardAllGens;
        private Timer GameTimer;
    }
}