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
            this.components = new System.ComponentModel.Container();
            this.StartButton = new System.Windows.Forms.Button();
            this.StopButton = new System.Windows.Forms.Button();
            this.ResetButton = new System.Windows.Forms.Button();
            this.NextGenButton = new System.Windows.Forms.Button();
            this.btnTrackBestCreature = new System.Windows.Forms.Button();
            this.DrawTimer = new System.Windows.Forms.Timer(this.components);
            this.GameTimer = new System.Windows.Forms.Timer(this.components);
            this.lblGenEndsIn = new System.Windows.Forms.Label();
            this.lblGenerationCount = new System.Windows.Forms.Label();
            this.NewGenerationTimer = new System.Windows.Forms.Timer(this.components);
            this.SuspendLayout();
            // 
            // StartButton
            // 
            this.StartButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.StartButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.StartButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.StartButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.StartButton.Location = new System.Drawing.Point(454, 12);
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
            this.StopButton.Location = new System.Drawing.Point(535, 12);
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
            this.ResetButton.Location = new System.Drawing.Point(616, 12);
            this.ResetButton.Name = "ResetButton";
            this.ResetButton.Size = new System.Drawing.Size(75, 32);
            this.ResetButton.TabIndex = 1;
            this.ResetButton.Text = "Reset";
            this.ResetButton.UseVisualStyleBackColor = false;
            this.ResetButton.Click += new System.EventHandler(this.ResetButton_Click);
            // 
            // NextGenButton
            // 
            this.NextGenButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.NextGenButton.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.NextGenButton.Cursor = System.Windows.Forms.Cursors.Hand;
            this.NextGenButton.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.NextGenButton.Location = new System.Drawing.Point(697, 12);
            this.NextGenButton.Name = "NextGenButton";
            this.NextGenButton.Size = new System.Drawing.Size(75, 32);
            this.NextGenButton.TabIndex = 1;
            this.NextGenButton.Text = "Next Gen";
            this.NextGenButton.UseVisualStyleBackColor = false;
            this.NextGenButton.Click += new System.EventHandler(this.NextGenButton_Click);
            // 
            // btnTrackBestCreature
            // 
            this.btnTrackBestCreature.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnTrackBestCreature.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.btnTrackBestCreature.Cursor = System.Windows.Forms.Cursors.Hand;
            this.btnTrackBestCreature.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnTrackBestCreature.Location = new System.Drawing.Point(616, 50);
            this.btnTrackBestCreature.Name = "btnTrackBestCreature";
            this.btnTrackBestCreature.Size = new System.Drawing.Size(156, 32);
            this.btnTrackBestCreature.TabIndex = 2;
            this.btnTrackBestCreature.Text = "Track Best Creature";
            this.btnTrackBestCreature.UseVisualStyleBackColor = false;
            this.btnTrackBestCreature.Click += new System.EventHandler(this.btnTrackBestCreature_Click);
            // 
            // DrawTimer
            // 
            this.DrawTimer.Interval = 17;
            this.DrawTimer.Tick += new System.EventHandler(this.DrawTimer_Tick);
            // 
            // GameTimer
            // 
            this.GameTimer.Interval = 17;
            this.GameTimer.Tick += new System.EventHandler(this.GameTimer_Tick);
            // 
            // lblGenEndsIn
            // 
            this.lblGenEndsIn.AutoSize = true;
            this.lblGenEndsIn.BackColor = System.Drawing.Color.Transparent;
            this.lblGenEndsIn.Font = new System.Drawing.Font("Yu Gothic UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lblGenEndsIn.ForeColor = System.Drawing.Color.White;
            this.lblGenEndsIn.Location = new System.Drawing.Point(15, 44);
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
            this.lblGenerationCount.Location = new System.Drawing.Point(12, 12);
            this.lblGenerationCount.Name = "lblGenerationCount";
            this.lblGenerationCount.Size = new System.Drawing.Size(75, 32);
            this.lblGenerationCount.TabIndex = 5;
            this.lblGenerationCount.Text = "Gen 1";
            // 
            // NewGenerationTimer
            // 
            this.NewGenerationTimer.Tick += new System.EventHandler(this.NewGenerationTimer_Tick);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(32)))), ((int)(((byte)(32)))), ((int)(((byte)(32)))));
            this.ClientSize = new System.Drawing.Size(784, 661);
            this.Controls.Add(this.lblGenEndsIn);
            this.Controls.Add(this.lblGenerationCount);
            this.Controls.Add(this.btnTrackBestCreature);
            this.Controls.Add(this.NextGenButton);
            this.Controls.Add(this.ResetButton);
            this.Controls.Add(this.StopButton);
            this.Controls.Add(this.StartButton);
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Mace Evolution";
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.MainForm_Paint);
            this.MouseClick += new System.Windows.Forms.MouseEventHandler(this.MainForm_MouseClick);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private Button StartButton;
        private Button StopButton;
        private Button ResetButton;
        private Button NextGenButton;
        private Button btnTrackBestCreature;
        private Timer DrawTimer;
        private Timer GameTimer;
        private Label lblGenEndsIn;
        private Label lblGenerationCount;
        private Timer NewGenerationTimer;
    }
}