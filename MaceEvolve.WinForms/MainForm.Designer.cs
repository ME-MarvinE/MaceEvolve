﻿using System.Windows.Forms;

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
            btnFastFoward = new Button();
            GameTimer = new Timer(components);
            GatherStepInfoForAllCreaturesButton = new Button();
            btnLoadStep = new Button();
            btnSaveCurrentStep = new Button();
            btnBenchmark = new Button();
            nudSimulationTPS = new NumericUpDown();
            lblSimulationTPS = new Label();
            btnHideUI = new Button();
            nudSimulationFPS = new NumericUpDown();
            lblSimulationFPS = new Label();
            btnLinkFPSAndTPS = new Button();
            DrawTimer = new Timer(components);
            ((System.ComponentModel.ISupportInitialize)nudSimulationTPS).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudSimulationFPS).BeginInit();
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
            // btnFastFoward
            // 
            btnFastFoward.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnFastFoward.BackColor = System.Drawing.Color.FromArgb(64, 64, 64);
            btnFastFoward.Cursor = Cursors.Hand;
            btnFastFoward.FlatStyle = FlatStyle.Flat;
            btnFastFoward.Location = new System.Drawing.Point(620, 52);
            btnFastFoward.Name = "btnFastFoward";
            btnFastFoward.Size = new System.Drawing.Size(156, 32);
            btnFastFoward.TabIndex = 1;
            btnFastFoward.Text = "Fast Forward";
            btnFastFoward.UseVisualStyleBackColor = false;
            btnFastFoward.Click += btnFastForward_Click;
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
            // btnLoadStep
            // 
            btnLoadStep.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnLoadStep.BackColor = System.Drawing.Color.FromArgb(64, 64, 64);
            btnLoadStep.Cursor = Cursors.Hand;
            btnLoadStep.FlatStyle = FlatStyle.Flat;
            btnLoadStep.Location = new System.Drawing.Point(620, 128);
            btnLoadStep.Name = "btnLoadStep";
            btnLoadStep.Size = new System.Drawing.Size(156, 32);
            btnLoadStep.TabIndex = 1;
            btnLoadStep.Text = "Load Step";
            btnLoadStep.UseVisualStyleBackColor = false;
            btnLoadStep.Click += btnLoadStep_Click;
            // 
            // btnSaveCurrentStep
            // 
            btnSaveCurrentStep.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnSaveCurrentStep.BackColor = System.Drawing.Color.FromArgb(64, 64, 64);
            btnSaveCurrentStep.Cursor = Cursors.Hand;
            btnSaveCurrentStep.FlatStyle = FlatStyle.Flat;
            btnSaveCurrentStep.Location = new System.Drawing.Point(620, 90);
            btnSaveCurrentStep.Name = "btnSaveCurrentStep";
            btnSaveCurrentStep.Size = new System.Drawing.Size(156, 32);
            btnSaveCurrentStep.TabIndex = 1;
            btnSaveCurrentStep.Text = "Save Current Step";
            btnSaveCurrentStep.UseVisualStyleBackColor = false;
            btnSaveCurrentStep.Click += btnSaveCurrentStep_Click;
            // 
            // btnBenchmark
            // 
            btnBenchmark.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnBenchmark.BackColor = System.Drawing.Color.FromArgb(64, 64, 64);
            btnBenchmark.Cursor = Cursors.Hand;
            btnBenchmark.FlatStyle = FlatStyle.Flat;
            btnBenchmark.Location = new System.Drawing.Point(620, 166);
            btnBenchmark.Name = "btnBenchmark";
            btnBenchmark.Size = new System.Drawing.Size(156, 32);
            btnBenchmark.TabIndex = 1;
            btnBenchmark.Text = "Benchmark";
            btnBenchmark.UseVisualStyleBackColor = false;
            btnBenchmark.Click += btnBenchmark_Click;
            // 
            // nudSimulationTPS
            // 
            nudSimulationTPS.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            nudSimulationTPS.Location = new System.Drawing.Point(620, 544);
            nudSimulationTPS.Maximum = new decimal(new int[] { 8192, 0, 0, 0 });
            nudSimulationTPS.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            nudSimulationTPS.Name = "nudSimulationTPS";
            nudSimulationTPS.Size = new System.Drawing.Size(48, 23);
            nudSimulationTPS.TabIndex = 7;
            nudSimulationTPS.Value = new decimal(new int[] { 1, 0, 0, 0 });
            nudSimulationTPS.ValueChanged += nudSimulationTPS_ValueChanged;
            // 
            // lblSimulationTPS
            // 
            lblSimulationTPS.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            lblSimulationTPS.AutoSize = true;
            lblSimulationTPS.BackColor = System.Drawing.Color.Transparent;
            lblSimulationTPS.Font = new System.Drawing.Font("Yu Gothic UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            lblSimulationTPS.ForeColor = System.Drawing.Color.White;
            lblSimulationTPS.Location = new System.Drawing.Point(578, 544);
            lblSimulationTPS.Name = "lblSimulationTPS";
            lblSimulationTPS.Size = new System.Drawing.Size(36, 21);
            lblSimulationTPS.TabIndex = 4;
            lblSimulationTPS.Text = "TPS";
            // 
            // btnHideUI
            // 
            btnHideUI.Anchor = AnchorStyles.Bottom | AnchorStyles.Left;
            btnHideUI.BackColor = System.Drawing.Color.FromArgb(64, 64, 64);
            btnHideUI.Cursor = Cursors.Hand;
            btnHideUI.FlatStyle = FlatStyle.Flat;
            btnHideUI.Location = new System.Drawing.Point(12, 622);
            btnHideUI.Name = "btnHideUI";
            btnHideUI.Size = new System.Drawing.Size(32, 27);
            btnHideUI.TabIndex = 1;
            btnHideUI.Text = "UI";
            btnHideUI.UseVisualStyleBackColor = false;
            btnHideUI.Click += btnHideUI_Click;
            // 
            // nudSimulationFPS
            // 
            nudSimulationFPS.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            nudSimulationFPS.Location = new System.Drawing.Point(726, 544);
            nudSimulationFPS.Maximum = new decimal(new int[] { 8192, 0, 0, 0 });
            nudSimulationFPS.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            nudSimulationFPS.Name = "nudSimulationFPS";
            nudSimulationFPS.Size = new System.Drawing.Size(48, 23);
            nudSimulationFPS.TabIndex = 9;
            nudSimulationFPS.Value = new decimal(new int[] { 1, 0, 0, 0 });
            nudSimulationFPS.ValueChanged += nudSimulationFPS_ValueChanged;
            // 
            // lblSimulationFPS
            // 
            lblSimulationFPS.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            lblSimulationFPS.AutoSize = true;
            lblSimulationFPS.BackColor = System.Drawing.Color.Transparent;
            lblSimulationFPS.Font = new System.Drawing.Font("Yu Gothic UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            lblSimulationFPS.ForeColor = System.Drawing.Color.White;
            lblSimulationFPS.Location = new System.Drawing.Point(684, 544);
            lblSimulationFPS.Name = "lblSimulationFPS";
            lblSimulationFPS.Size = new System.Drawing.Size(36, 21);
            lblSimulationFPS.TabIndex = 8;
            lblSimulationFPS.Text = "FPS";
            // 
            // btnLinkFPSAndTPS
            // 
            btnLinkFPSAndTPS.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnLinkFPSAndTPS.BackColor = System.Drawing.Color.FromArgb(64, 64, 64);
            btnLinkFPSAndTPS.Cursor = Cursors.Hand;
            btnLinkFPSAndTPS.FlatStyle = FlatStyle.Flat;
            btnLinkFPSAndTPS.Location = new System.Drawing.Point(578, 573);
            btnLinkFPSAndTPS.Name = "btnLinkFPSAndTPS";
            btnLinkFPSAndTPS.Size = new System.Drawing.Size(198, 32);
            btnLinkFPSAndTPS.TabIndex = 1;
            btnLinkFPSAndTPS.Text = "Link FPS and TPS:";
            btnLinkFPSAndTPS.UseVisualStyleBackColor = false;
            btnLinkFPSAndTPS.Click += btnLinkFPSAndTPS_Click;
            // 
            // DrawTimer
            // 
            DrawTimer.Enabled = true;
            DrawTimer.Tick += DrawTimer_Tick;
            // 
            // MainForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = System.Drawing.Color.FromArgb(32, 32, 32);
            ClientSize = new System.Drawing.Size(784, 661);
            Controls.Add(nudSimulationFPS);
            Controls.Add(lblSimulationFPS);
            Controls.Add(nudSimulationTPS);
            Controls.Add(lblSimulationRunning);
            Controls.Add(lblSimulationTPS);
            Controls.Add(lblGenEndsIn);
            Controls.Add(lblGenerationCount);
            Controls.Add(btnTrackBestCreature);
            Controls.Add(GatherStepInfoForAllCreaturesButton);
            Controls.Add(ResetButton);
            Controls.Add(StopButton);
            Controls.Add(btnSaveCurrentStep);
            Controls.Add(btnHideUI);
            Controls.Add(btnLinkFPSAndTPS);
            Controls.Add(btnBenchmark);
            Controls.Add(btnLoadStep);
            Controls.Add(btnFastFoward);
            Controls.Add(StartButton);
            DoubleBuffered = true;
            Name = "MainForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Mace Evolution";
            Load += MainForm_Load;
            Paint += MainForm_Paint;
            MouseClick += MainForm_MouseClick;
            ((System.ComponentModel.ISupportInitialize)nudSimulationTPS).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudSimulationFPS).EndInit();
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
        private Button btnFastFoward;
        private Timer GameTimer;
        private Button GatherStepInfoForAllCreaturesButton;
        private Button btnLoadStepList;
        private Button btnLoadStep;
        private Button btnSaveCurrentStep;
        private Button btnBenchmark;
        private NumericUpDown nudSimulationTPS;
        private Label lblSimulationTPS;
        private Button btnHideUI;
        private NumericUpDown nudSimulationFPS;
        private Label lblSimulationFPS;
        private Button btnLinkFPSAndTPS;
        private Timer DrawTimer;
    }
}