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
            btnFastFoward = new Button();
            GameTimer = new Timer(components);
            btnLoadStep = new Button();
            btnSaveCurrentStep = new Button();
            btnBenchmark = new Button();
            nudSimulationTPS = new NumericUpDown();
            lblSimulationTPS = new Label();
            nudSimulationFPS = new NumericUpDown();
            lblSimulationFPS = new Label();
            DrawTimer = new Timer(components);
            chkLinkFpsAndTps = new CheckBox();
            chkGatherStepInfoForAllCreatures = new CheckBox();
            chkShowUI = new CheckBox();
            btnUpdateWorldBounds = new Button();
            chkShowTreeColorByAge = new CheckBox();
            chkUseGenerations = new CheckBox();
            nudSuccessBoundsX = new NumericUpDown();
            nudSuccessBoundsY = new NumericUpDown();
            chkUseSuccessBounds = new CheckBox();
            btnCenterSuccessBounds = new Button();
            lblSuccessBoundsX = new Label();
            lblSuccessBoundsY = new Label();
            lblSuccessBoundsWidth = new Label();
            lblSuccessBoundsHeight = new Label();
            nudSuccessBoundsWidth = new NumericUpDown();
            nudSuccessBoundsHeight = new NumericUpDown();
            nudStepsPerGeneration = new NumericUpDown();
            lblStepsPerGeneration = new Label();
            nudMinSuccessfulCreatureFitness = new NumericUpDown();
            lblMinSuccessfulCreatureFitness = new Label();
            ((System.ComponentModel.ISupportInitialize)nudSimulationTPS).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudSimulationFPS).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudSuccessBoundsX).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudSuccessBoundsY).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudSuccessBoundsWidth).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudSuccessBoundsHeight).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudStepsPerGeneration).BeginInit();
            ((System.ComponentModel.ISupportInitialize)nudMinSuccessfulCreatureFitness).BeginInit();
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
            lblGenEndsIn.Font = new System.Drawing.Font("Yu Gothic UI", 12F);
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
            lblGenerationCount.Font = new System.Drawing.Font("Yu Gothic UI", 18F, System.Drawing.FontStyle.Bold);
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
            lblSimulationRunning.Font = new System.Drawing.Font("Yu Gothic UI", 18F, System.Drawing.FontStyle.Bold);
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
            nudSimulationTPS.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            nudSimulationTPS.Location = new System.Drawing.Point(620, 242);
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
            lblSimulationTPS.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblSimulationTPS.AutoSize = true;
            lblSimulationTPS.BackColor = System.Drawing.Color.Transparent;
            lblSimulationTPS.Font = new System.Drawing.Font("Yu Gothic UI", 12F);
            lblSimulationTPS.ForeColor = System.Drawing.Color.White;
            lblSimulationTPS.Location = new System.Drawing.Point(578, 242);
            lblSimulationTPS.Name = "lblSimulationTPS";
            lblSimulationTPS.Size = new System.Drawing.Size(36, 21);
            lblSimulationTPS.TabIndex = 4;
            lblSimulationTPS.Text = "TPS";
            // 
            // nudSimulationFPS
            // 
            nudSimulationFPS.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            nudSimulationFPS.Location = new System.Drawing.Point(726, 242);
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
            lblSimulationFPS.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblSimulationFPS.AutoSize = true;
            lblSimulationFPS.BackColor = System.Drawing.Color.Transparent;
            lblSimulationFPS.Font = new System.Drawing.Font("Yu Gothic UI", 12F);
            lblSimulationFPS.ForeColor = System.Drawing.Color.White;
            lblSimulationFPS.Location = new System.Drawing.Point(684, 242);
            lblSimulationFPS.Name = "lblSimulationFPS";
            lblSimulationFPS.Size = new System.Drawing.Size(36, 21);
            lblSimulationFPS.TabIndex = 8;
            lblSimulationFPS.Text = "FPS";
            // 
            // DrawTimer
            // 
            DrawTimer.Enabled = true;
            DrawTimer.Tick += DrawTimer_Tick;
            // 
            // chkLinkFpsAndTps
            // 
            chkLinkFpsAndTps.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            chkLinkFpsAndTps.AutoSize = true;
            chkLinkFpsAndTps.BackColor = System.Drawing.Color.Transparent;
            chkLinkFpsAndTps.Font = new System.Drawing.Font("Yu Gothic UI", 9F);
            chkLinkFpsAndTps.ForeColor = System.Drawing.Color.White;
            chkLinkFpsAndTps.Location = new System.Drawing.Point(661, 580);
            chkLinkFpsAndTps.Name = "chkLinkFpsAndTps";
            chkLinkFpsAndTps.RightToLeft = RightToLeft.Yes;
            chkLinkFpsAndTps.Size = new System.Drawing.Size(115, 19);
            chkLinkFpsAndTps.TabIndex = 11;
            chkLinkFpsAndTps.Text = "Link FPS and TPS";
            chkLinkFpsAndTps.UseVisualStyleBackColor = false;
            chkLinkFpsAndTps.CheckedChanged += chkLinkFpsAndTps_CheckedChanged;
            // 
            // chkGatherStepInfoForAllCreatures
            // 
            chkGatherStepInfoForAllCreatures.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            chkGatherStepInfoForAllCreatures.AutoSize = true;
            chkGatherStepInfoForAllCreatures.BackColor = System.Drawing.Color.Transparent;
            chkGatherStepInfoForAllCreatures.Font = new System.Drawing.Font("Yu Gothic UI", 9F);
            chkGatherStepInfoForAllCreatures.ForeColor = System.Drawing.Color.White;
            chkGatherStepInfoForAllCreatures.Location = new System.Drawing.Point(576, 605);
            chkGatherStepInfoForAllCreatures.Name = "chkGatherStepInfoForAllCreatures";
            chkGatherStepInfoForAllCreatures.RightToLeft = RightToLeft.Yes;
            chkGatherStepInfoForAllCreatures.Size = new System.Drawing.Size(200, 19);
            chkGatherStepInfoForAllCreatures.TabIndex = 11;
            chkGatherStepInfoForAllCreatures.Text = "Gather Step Info For All Creatures";
            chkGatherStepInfoForAllCreatures.UseVisualStyleBackColor = false;
            chkGatherStepInfoForAllCreatures.CheckedChanged += chkGatherStepInfoForAllCreatures_CheckedChanged;
            // 
            // chkShowUI
            // 
            chkShowUI.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            chkShowUI.AutoSize = true;
            chkShowUI.BackColor = System.Drawing.Color.Transparent;
            chkShowUI.Font = new System.Drawing.Font("Yu Gothic UI", 9F);
            chkShowUI.ForeColor = System.Drawing.Color.White;
            chkShowUI.Location = new System.Drawing.Point(739, 630);
            chkShowUI.Name = "chkShowUI";
            chkShowUI.RightToLeft = RightToLeft.Yes;
            chkShowUI.Size = new System.Drawing.Size(37, 19);
            chkShowUI.TabIndex = 11;
            chkShowUI.Text = "UI";
            chkShowUI.UseVisualStyleBackColor = false;
            chkShowUI.CheckedChanged += chkShowUI_CheckedChanged;
            // 
            // btnUpdateWorldBounds
            // 
            btnUpdateWorldBounds.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnUpdateWorldBounds.BackColor = System.Drawing.Color.FromArgb(64, 64, 64);
            btnUpdateWorldBounds.Cursor = Cursors.Hand;
            btnUpdateWorldBounds.FlatStyle = FlatStyle.Flat;
            btnUpdateWorldBounds.Location = new System.Drawing.Point(620, 204);
            btnUpdateWorldBounds.Name = "btnUpdateWorldBounds";
            btnUpdateWorldBounds.Size = new System.Drawing.Size(156, 32);
            btnUpdateWorldBounds.TabIndex = 1;
            btnUpdateWorldBounds.Text = "Update World Bounds";
            btnUpdateWorldBounds.UseVisualStyleBackColor = false;
            btnUpdateWorldBounds.Click += btnUpdateWorldBounds_Click;
            // 
            // chkShowTreeColorByAge
            // 
            chkShowTreeColorByAge.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            chkShowTreeColorByAge.AutoSize = true;
            chkShowTreeColorByAge.BackColor = System.Drawing.Color.Transparent;
            chkShowTreeColorByAge.Font = new System.Drawing.Font("Yu Gothic UI", 9F);
            chkShowTreeColorByAge.ForeColor = System.Drawing.Color.White;
            chkShowTreeColorByAge.Location = new System.Drawing.Point(626, 555);
            chkShowTreeColorByAge.Name = "chkShowTreeColorByAge";
            chkShowTreeColorByAge.RightToLeft = RightToLeft.Yes;
            chkShowTreeColorByAge.Size = new System.Drawing.Size(150, 19);
            chkShowTreeColorByAge.TabIndex = 11;
            chkShowTreeColorByAge.Text = "Show Tree Color By Age";
            chkShowTreeColorByAge.UseVisualStyleBackColor = false;
            chkShowTreeColorByAge.CheckedChanged += chkShowTreeColorByAge_CheckedChanged;
            // 
            // chkUseGenerations
            // 
            chkUseGenerations.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            chkUseGenerations.AutoSize = true;
            chkUseGenerations.BackColor = System.Drawing.Color.Transparent;
            chkUseGenerations.Font = new System.Drawing.Font("Yu Gothic UI", 9F);
            chkUseGenerations.ForeColor = System.Drawing.Color.White;
            chkUseGenerations.Location = new System.Drawing.Point(683, 530);
            chkUseGenerations.Name = "chkUseGenerations";
            chkUseGenerations.RightToLeft = RightToLeft.Yes;
            chkUseGenerations.Size = new System.Drawing.Size(93, 19);
            chkUseGenerations.TabIndex = 11;
            chkUseGenerations.Text = "Generational";
            chkUseGenerations.UseVisualStyleBackColor = false;
            chkUseGenerations.CheckedChanged += chkUseGenerations_CheckedChanged;
            // 
            // nudSuccessBoundsX
            // 
            nudSuccessBoundsX.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            nudSuccessBoundsX.Location = new System.Drawing.Point(622, 409);
            nudSuccessBoundsX.Maximum = new decimal(new int[] { 65536, 0, 0, 0 });
            nudSuccessBoundsX.Name = "nudSuccessBoundsX";
            nudSuccessBoundsX.Size = new System.Drawing.Size(48, 23);
            nudSuccessBoundsX.TabIndex = 7;
            nudSuccessBoundsX.ValueChanged += nudSuccessBoundsX_ValueChanged;
            // 
            // nudSuccessBoundsY
            // 
            nudSuccessBoundsY.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            nudSuccessBoundsY.Location = new System.Drawing.Point(730, 409);
            nudSuccessBoundsY.Maximum = new decimal(new int[] { 65536, 0, 0, 0 });
            nudSuccessBoundsY.Name = "nudSuccessBoundsY";
            nudSuccessBoundsY.Size = new System.Drawing.Size(48, 23);
            nudSuccessBoundsY.TabIndex = 9;
            nudSuccessBoundsY.ValueChanged += nudSuccessBoundsY_ValueChanged;
            // 
            // chkUseSuccessBounds
            // 
            chkUseSuccessBounds.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            chkUseSuccessBounds.AutoSize = true;
            chkUseSuccessBounds.BackColor = System.Drawing.Color.Transparent;
            chkUseSuccessBounds.Font = new System.Drawing.Font("Yu Gothic UI", 9F);
            chkUseSuccessBounds.ForeColor = System.Drawing.Color.White;
            chkUseSuccessBounds.Location = new System.Drawing.Point(644, 505);
            chkUseSuccessBounds.Name = "chkUseSuccessBounds";
            chkUseSuccessBounds.RightToLeft = RightToLeft.Yes;
            chkUseSuccessBounds.Size = new System.Drawing.Size(132, 19);
            chkUseSuccessBounds.TabIndex = 11;
            chkUseSuccessBounds.Text = "Use Success Bounds";
            chkUseSuccessBounds.UseVisualStyleBackColor = false;
            chkUseSuccessBounds.CheckedChanged += chkUseSuccessBounds_CheckedChanged;
            // 
            // btnCenterSuccessBounds
            // 
            btnCenterSuccessBounds.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnCenterSuccessBounds.BackColor = System.Drawing.Color.FromArgb(64, 64, 64);
            btnCenterSuccessBounds.Cursor = Cursors.Hand;
            btnCenterSuccessBounds.FlatStyle = FlatStyle.Flat;
            btnCenterSuccessBounds.Location = new System.Drawing.Point(622, 467);
            btnCenterSuccessBounds.Name = "btnCenterSuccessBounds";
            btnCenterSuccessBounds.Size = new System.Drawing.Size(156, 32);
            btnCenterSuccessBounds.TabIndex = 1;
            btnCenterSuccessBounds.Text = "Center Success Bounds";
            btnCenterSuccessBounds.UseVisualStyleBackColor = false;
            btnCenterSuccessBounds.Click += btnCenterSuccessBounds_Click;
            // 
            // lblSuccessBoundsX
            // 
            lblSuccessBoundsX.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblSuccessBoundsX.AutoSize = true;
            lblSuccessBoundsX.BackColor = System.Drawing.Color.Transparent;
            lblSuccessBoundsX.Font = new System.Drawing.Font("Yu Gothic UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lblSuccessBoundsX.ForeColor = System.Drawing.Color.White;
            lblSuccessBoundsX.Location = new System.Drawing.Point(604, 411);
            lblSuccessBoundsX.Name = "lblSuccessBoundsX";
            lblSuccessBoundsX.Size = new System.Drawing.Size(14, 15);
            lblSuccessBoundsX.TabIndex = 4;
            lblSuccessBoundsX.Text = "X";
            // 
            // lblSuccessBoundsY
            // 
            lblSuccessBoundsY.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblSuccessBoundsY.AutoSize = true;
            lblSuccessBoundsY.BackColor = System.Drawing.Color.Transparent;
            lblSuccessBoundsY.Font = new System.Drawing.Font("Yu Gothic UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lblSuccessBoundsY.ForeColor = System.Drawing.Color.White;
            lblSuccessBoundsY.Location = new System.Drawing.Point(710, 411);
            lblSuccessBoundsY.Name = "lblSuccessBoundsY";
            lblSuccessBoundsY.Size = new System.Drawing.Size(14, 15);
            lblSuccessBoundsY.TabIndex = 8;
            lblSuccessBoundsY.Text = "Y";
            // 
            // lblSuccessBoundsWidth
            // 
            lblSuccessBoundsWidth.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblSuccessBoundsWidth.AutoSize = true;
            lblSuccessBoundsWidth.BackColor = System.Drawing.Color.Transparent;
            lblSuccessBoundsWidth.Font = new System.Drawing.Font("Yu Gothic UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lblSuccessBoundsWidth.ForeColor = System.Drawing.Color.White;
            lblSuccessBoundsWidth.Location = new System.Drawing.Point(578, 440);
            lblSuccessBoundsWidth.Name = "lblSuccessBoundsWidth";
            lblSuccessBoundsWidth.Size = new System.Drawing.Size(39, 15);
            lblSuccessBoundsWidth.TabIndex = 4;
            lblSuccessBoundsWidth.Text = "Width";
            // 
            // lblSuccessBoundsHeight
            // 
            lblSuccessBoundsHeight.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblSuccessBoundsHeight.AutoSize = true;
            lblSuccessBoundsHeight.BackColor = System.Drawing.Color.Transparent;
            lblSuccessBoundsHeight.Font = new System.Drawing.Font("Yu Gothic UI", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
            lblSuccessBoundsHeight.ForeColor = System.Drawing.Color.White;
            lblSuccessBoundsHeight.Location = new System.Drawing.Point(681, 440);
            lblSuccessBoundsHeight.Name = "lblSuccessBoundsHeight";
            lblSuccessBoundsHeight.Size = new System.Drawing.Size(43, 15);
            lblSuccessBoundsHeight.TabIndex = 8;
            lblSuccessBoundsHeight.Text = "Height";
            // 
            // nudSuccessBoundsWidth
            // 
            nudSuccessBoundsWidth.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            nudSuccessBoundsWidth.Location = new System.Drawing.Point(622, 438);
            nudSuccessBoundsWidth.Maximum = new decimal(new int[] { 65536, 0, 0, 0 });
            nudSuccessBoundsWidth.Name = "nudSuccessBoundsWidth";
            nudSuccessBoundsWidth.Size = new System.Drawing.Size(48, 23);
            nudSuccessBoundsWidth.TabIndex = 7;
            nudSuccessBoundsWidth.ValueChanged += nudSuccessBoundsWidth_ValueChanged;
            // 
            // nudSuccessBoundsHeight
            // 
            nudSuccessBoundsHeight.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            nudSuccessBoundsHeight.Location = new System.Drawing.Point(730, 438);
            nudSuccessBoundsHeight.Maximum = new decimal(new int[] { 65536, 0, 0, 0 });
            nudSuccessBoundsHeight.Name = "nudSuccessBoundsHeight";
            nudSuccessBoundsHeight.Size = new System.Drawing.Size(48, 23);
            nudSuccessBoundsHeight.TabIndex = 9;
            nudSuccessBoundsHeight.ValueChanged += nudSuccessBoundsHeight_ValueChanged;
            // 
            // nudStepsPerGeneration
            // 
            nudStepsPerGeneration.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            nudStepsPerGeneration.Location = new System.Drawing.Point(726, 271);
            nudStepsPerGeneration.Maximum = new decimal(new int[] { 8192, 0, 0, 0 });
            nudStepsPerGeneration.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            nudStepsPerGeneration.Name = "nudStepsPerGeneration";
            nudStepsPerGeneration.Size = new System.Drawing.Size(48, 23);
            nudStepsPerGeneration.TabIndex = 9;
            nudStepsPerGeneration.Value = new decimal(new int[] { 1, 0, 0, 0 });
            nudStepsPerGeneration.ValueChanged += nudStepsPerGeneration_ValueChanged;
            // 
            // lblStepsPerGeneration
            // 
            lblStepsPerGeneration.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblStepsPerGeneration.AutoSize = true;
            lblStepsPerGeneration.BackColor = System.Drawing.Color.Transparent;
            lblStepsPerGeneration.Font = new System.Drawing.Font("Yu Gothic UI", 12F);
            lblStepsPerGeneration.ForeColor = System.Drawing.Color.White;
            lblStepsPerGeneration.Location = new System.Drawing.Point(613, 271);
            lblStepsPerGeneration.Name = "lblStepsPerGeneration";
            lblStepsPerGeneration.Size = new System.Drawing.Size(107, 21);
            lblStepsPerGeneration.TabIndex = 8;
            lblStepsPerGeneration.Text = "Steps Per Gen";
            // 
            // nudMinSuccessfulCreatureFitness
            // 
            nudMinSuccessfulCreatureFitness.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            nudMinSuccessfulCreatureFitness.DecimalPlaces = 2;
            nudMinSuccessfulCreatureFitness.ImeMode = ImeMode.Katakana;
            nudMinSuccessfulCreatureFitness.Increment = new decimal(new int[] { 1, 0, 0, 65536 });
            nudMinSuccessfulCreatureFitness.Location = new System.Drawing.Point(726, 300);
            nudMinSuccessfulCreatureFitness.Maximum = new decimal(new int[] { 1, 0, 0, 0 });
            nudMinSuccessfulCreatureFitness.Name = "nudMinSuccessfulCreatureFitness";
            nudMinSuccessfulCreatureFitness.Size = new System.Drawing.Size(48, 23);
            nudMinSuccessfulCreatureFitness.TabIndex = 9;
            nudMinSuccessfulCreatureFitness.Value = new decimal(new int[] { 1, 0, 0, 0 });
            nudMinSuccessfulCreatureFitness.ValueChanged += nudMinSuccessfulCreatureFitness_ValueChanged;
            // 
            // lblMinSuccessfulCreatureFitness
            // 
            lblMinSuccessfulCreatureFitness.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            lblMinSuccessfulCreatureFitness.AutoSize = true;
            lblMinSuccessfulCreatureFitness.BackColor = System.Drawing.Color.Transparent;
            lblMinSuccessfulCreatureFitness.Font = new System.Drawing.Font("Yu Gothic UI", 12F);
            lblMinSuccessfulCreatureFitness.ForeColor = System.Drawing.Color.White;
            lblMinSuccessfulCreatureFitness.Location = new System.Drawing.Point(498, 302);
            lblMinSuccessfulCreatureFitness.Name = "lblMinSuccessfulCreatureFitness";
            lblMinSuccessfulCreatureFitness.Size = new System.Drawing.Size(229, 21);
            lblMinSuccessfulCreatureFitness.TabIndex = 8;
            lblMinSuccessfulCreatureFitness.Text = "Min Successful Creature Fitness";
            // 
            // MainForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = System.Drawing.Color.FromArgb(32, 32, 32);
            ClientSize = new System.Drawing.Size(784, 661);
            Controls.Add(chkShowUI);
            Controls.Add(chkGatherStepInfoForAllCreatures);
            Controls.Add(chkUseSuccessBounds);
            Controls.Add(chkUseGenerations);
            Controls.Add(chkShowTreeColorByAge);
            Controls.Add(chkLinkFpsAndTps);
            Controls.Add(nudSuccessBoundsHeight);
            Controls.Add(nudSuccessBoundsY);
            Controls.Add(nudMinSuccessfulCreatureFitness);
            Controls.Add(nudStepsPerGeneration);
            Controls.Add(nudSimulationFPS);
            Controls.Add(nudSuccessBoundsWidth);
            Controls.Add(nudSuccessBoundsX);
            Controls.Add(lblSuccessBoundsHeight);
            Controls.Add(lblSuccessBoundsY);
            Controls.Add(lblMinSuccessfulCreatureFitness);
            Controls.Add(lblStepsPerGeneration);
            Controls.Add(lblSimulationFPS);
            Controls.Add(nudSimulationTPS);
            Controls.Add(lblSimulationRunning);
            Controls.Add(lblSuccessBoundsWidth);
            Controls.Add(lblSuccessBoundsX);
            Controls.Add(lblSimulationTPS);
            Controls.Add(lblGenEndsIn);
            Controls.Add(lblGenerationCount);
            Controls.Add(btnTrackBestCreature);
            Controls.Add(btnCenterSuccessBounds);
            Controls.Add(btnUpdateWorldBounds);
            Controls.Add(ResetButton);
            Controls.Add(StopButton);
            Controls.Add(btnSaveCurrentStep);
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
            ((System.ComponentModel.ISupportInitialize)nudSuccessBoundsX).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudSuccessBoundsY).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudSuccessBoundsWidth).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudSuccessBoundsHeight).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudStepsPerGeneration).EndInit();
            ((System.ComponentModel.ISupportInitialize)nudMinSuccessfulCreatureFitness).EndInit();
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
        private Button btnLoadStepList;
        private Button btnLoadStep;
        private Button btnSaveCurrentStep;
        private Button btnBenchmark;
        private NumericUpDown nudSimulationTPS;
        private Label lblSimulationTPS;
        private NumericUpDown nudSimulationFPS;
        private Label lblSimulationFPS;
        private Timer DrawTimer;
        private CheckBox chkLinkFpsAndTps;
        private CheckBox chkGatherStepInfoForAllCreatures;
        private CheckBox chkShowUI;
        private Button btnUpdateWorldBounds;
        private CheckBox chkShowTreeColorByAge;
        private CheckBox chkUseGenerations;
        private NumericUpDown nudSuccessBoundsX;
        private NumericUpDown nudSuccessBoundsY;
        private CheckBox chkUseSuccessBounds;
        private Button btnCenterSuccessBounds;
        private Label lblSuccessBoundsX;
        private Label lblSuccessBoundsY;
        private Label lblSuccessBoundsWidth;
        private Label lblSuccessBoundsHeight;
        private NumericUpDown nudSuccessBoundsWidth;
        private NumericUpDown nudSuccessBoundsHeight;
        private NumericUpDown nudStepsPerGeneration;
        private Label lblStepsPerGeneration;
        private Label label1;
        private NumericUpDown nudMinSuccessfulCreatureFitness;
        private Label lblMinSuccessfulCreatureFitness;
    }
}