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
            components = new System.ComponentModel.Container();
            StartButton = new Button();
            StopButton = new Button();
            ResetButton = new Button();
            btnTrackBestCreature = new Button();
            lblGenEndsIn = new Label();
            lblGenerationCount = new Label();
            lblSimulationRunning = new Label();
            btnForwardGen = new Button();
            btnForwardGens = new Button();
            btnForwardAllGens = new Button();
            GameTimer = new Timer(components);
            GatherStepInfoForAllCreaturesButton = new Button();
            SuspendLayout();
            // 
            // StartButton
            // 
            StartButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            StartButton.BackColor = System.Drawing.Color.FromArgb(64, 64, 64);
            StartButton.Cursor = Cursors.Hand;
            StartButton.FlatStyle = FlatStyle.Flat;
            StartButton.Location = new System.Drawing.Point(377, 14);
            StartButton.Name = "StartButton";
            StartButton.Size = new System.Drawing.Size(75, 32);
            StartButton.TabIndex = 1;
            StartButton.Text = "Start";
            StartButton.UseVisualStyleBackColor = false;
            StartButton.Click += StartButton_Click;
            // 
            // StopButton
            // 
            StopButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            StopButton.BackColor = System.Drawing.Color.FromArgb(64, 64, 64);
            StopButton.Cursor = Cursors.Hand;
            StopButton.FlatStyle = FlatStyle.Flat;
            StopButton.Location = new System.Drawing.Point(458, 14);
            StopButton.Name = "StopButton";
            StopButton.Size = new System.Drawing.Size(75, 32);
            StopButton.TabIndex = 1;
            StopButton.Text = "Stop";
            StopButton.UseVisualStyleBackColor = false;
            StopButton.Click += StopButton_Click;
            // 
            // ResetButton
            // 
            ResetButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            ResetButton.BackColor = System.Drawing.Color.FromArgb(64, 64, 64);
            ResetButton.Cursor = Cursors.Hand;
            ResetButton.FlatStyle = FlatStyle.Flat;
            ResetButton.Location = new System.Drawing.Point(539, 14);
            ResetButton.Name = "ResetButton";
            ResetButton.Size = new System.Drawing.Size(75, 32);
            ResetButton.TabIndex = 1;
            ResetButton.Text = "Reset";
            ResetButton.UseVisualStyleBackColor = false;
            ResetButton.Click += ResetButton_Click;
            // 
            // btnTrackBestCreature
            // 
            btnTrackBestCreature.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnTrackBestCreature.BackColor = System.Drawing.Color.FromArgb(64, 64, 64);
            btnTrackBestCreature.Cursor = Cursors.Hand;
            btnTrackBestCreature.FlatStyle = FlatStyle.Flat;
            btnTrackBestCreature.Location = new System.Drawing.Point(620, 14);
            btnTrackBestCreature.Name = "btnTrackBestCreature";
            btnTrackBestCreature.Size = new System.Drawing.Size(156, 32);
            btnTrackBestCreature.TabIndex = 2;
            btnTrackBestCreature.Text = "Track Best Creature";
            btnTrackBestCreature.UseVisualStyleBackColor = false;
            btnTrackBestCreature.Click += btnTrackBestCreature_Click;
            // 
            // lblGenEndsIn
            // 
            lblGenEndsIn.AutoSize = true;
            lblGenEndsIn.BackColor = System.Drawing.Color.Transparent;
            lblGenEndsIn.Font = new System.Drawing.Font("Yu Gothic UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            lblGenEndsIn.ForeColor = System.Drawing.Color.White;
            lblGenEndsIn.Location = new System.Drawing.Point(15, 72);
            lblGenEndsIn.Name = "lblGenEndsIn";
            lblGenEndsIn.Size = new System.Drawing.Size(80, 21);
            lblGenEndsIn.TabIndex = 4;
            lblGenEndsIn.Text = "Ends In 0s";
            // 
            // lblGenerationCount
            // 
            lblGenerationCount.AutoSize = true;
            lblGenerationCount.BackColor = System.Drawing.Color.Transparent;
            lblGenerationCount.Font = new System.Drawing.Font("Yu Gothic UI", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            lblGenerationCount.ForeColor = System.Drawing.Color.White;
            lblGenerationCount.Location = new System.Drawing.Point(12, 40);
            lblGenerationCount.Name = "lblGenerationCount";
            lblGenerationCount.Size = new System.Drawing.Size(75, 32);
            lblGenerationCount.TabIndex = 5;
            lblGenerationCount.Text = "Gen 1";
            // 
            // lblSimulationRunning
            // 
            lblSimulationRunning.AutoSize = true;
            lblSimulationRunning.BackColor = System.Drawing.Color.Transparent;
            lblSimulationRunning.Font = new System.Drawing.Font("Yu Gothic UI", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            lblSimulationRunning.ForeColor = System.Drawing.Color.White;
            lblSimulationRunning.Location = new System.Drawing.Point(12, 9);
            lblSimulationRunning.Name = "lblSimulationRunning";
            lblSimulationRunning.Size = new System.Drawing.Size(105, 32);
            lblSimulationRunning.TabIndex = 6;
            lblSimulationRunning.Text = "Stopped";
            // 
            // btnForwardGen
            // 
            btnForwardGen.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnForwardGen.BackColor = System.Drawing.Color.FromArgb(64, 64, 64);
            btnForwardGen.Cursor = Cursors.Hand;
            btnForwardGen.FlatStyle = FlatStyle.Flat;
            btnForwardGen.Location = new System.Drawing.Point(620, 52);
            btnForwardGen.Name = "btnForwardGen";
            btnForwardGen.Size = new System.Drawing.Size(156, 32);
            btnForwardGen.TabIndex = 1;
            btnForwardGen.Text = "Forward 1 Gen";
            btnForwardGen.UseVisualStyleBackColor = false;
            btnForwardGen.Click += btnForwardGen_Click;
            // 
            // btnForwardGens
            // 
            btnForwardGens.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnForwardGens.BackColor = System.Drawing.Color.FromArgb(64, 64, 64);
            btnForwardGens.Cursor = Cursors.Hand;
            btnForwardGens.FlatStyle = FlatStyle.Flat;
            btnForwardGens.Location = new System.Drawing.Point(620, 90);
            btnForwardGens.Name = "btnForwardGens";
            btnForwardGens.Size = new System.Drawing.Size(156, 32);
            btnForwardGens.TabIndex = 1;
            btnForwardGens.Text = "Forward 100 Gens";
            btnForwardGens.UseVisualStyleBackColor = false;
            btnForwardGens.Click += btnForwardGens_Click;
            // 
            // btnForwardAllGens
            // 
            btnForwardAllGens.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnForwardAllGens.BackColor = System.Drawing.Color.FromArgb(64, 64, 64);
            btnForwardAllGens.Cursor = Cursors.Hand;
            btnForwardAllGens.FlatStyle = FlatStyle.Flat;
            btnForwardAllGens.Location = new System.Drawing.Point(620, 128);
            btnForwardAllGens.Name = "btnForwardAllGens";
            btnForwardAllGens.Size = new System.Drawing.Size(156, 32);
            btnForwardAllGens.TabIndex = 1;
            btnForwardAllGens.Text = "Forward All Gens";
            btnForwardAllGens.UseVisualStyleBackColor = false;
            btnForwardAllGens.Click += btnForwardAllGens_Click;
            // 
            // GameTimer
            // 
            GameTimer.Enabled = true;
            GameTimer.Tick += GameTimer_Tick;
            // 
            // GatherStepInfoForAllCreaturesButton
            // 
            GatherStepInfoForAllCreaturesButton.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            GatherStepInfoForAllCreaturesButton.BackColor = System.Drawing.Color.FromArgb(64, 64, 64);
            GatherStepInfoForAllCreaturesButton.Cursor = Cursors.Hand;
            GatherStepInfoForAllCreaturesButton.FlatStyle = FlatStyle.Flat;
            GatherStepInfoForAllCreaturesButton.Location = new System.Drawing.Point(511, 611);
            GatherStepInfoForAllCreaturesButton.Name = "GatherStepInfoForAllCreaturesButton";
            GatherStepInfoForAllCreaturesButton.Size = new System.Drawing.Size(265, 38);
            GatherStepInfoForAllCreaturesButton.TabIndex = 1;
            GatherStepInfoForAllCreaturesButton.Text = "Gather Step Info For All Creatures: ";
            GatherStepInfoForAllCreaturesButton.UseVisualStyleBackColor = false;
            GatherStepInfoForAllCreaturesButton.Click += GatherStepInfoForAllCreaturesButton_Click;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = System.Drawing.Color.FromArgb(32, 32, 32);
            ClientSize = new System.Drawing.Size(784, 661);
            Controls.Add(lblSimulationRunning);
            Controls.Add(lblGenEndsIn);
            Controls.Add(lblGenerationCount);
            Controls.Add(btnTrackBestCreature);
            Controls.Add(GatherStepInfoForAllCreaturesButton);
            Controls.Add(ResetButton);
            Controls.Add(StopButton);
            Controls.Add(btnForwardAllGens);
            Controls.Add(btnForwardGens);
            Controls.Add(btnForwardGen);
            Controls.Add(StartButton);
            KeyPreview = true;
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Mace Evolution";
            Load += MainForm_Load;
            Paint += MainForm_Paint;
            KeyDown += MainForm_KeyDown;
            MouseClick += MainForm_MouseClick;
            ResumeLayout(false);
            PerformLayout();
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
        private Button GatherStepInfoForAllCreaturesButton;
    }
}