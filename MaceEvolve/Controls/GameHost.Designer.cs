namespace MaceEvolve.Controls
{
    partial class GameHost
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.GameTimer = new System.Windows.Forms.Timer(this.components);
            this.DrawTimer = new System.Windows.Forms.Timer(this.components);
            this.NewGenerationTimer = new System.Windows.Forms.Timer(this.components);
            this.lblGenerationCount = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // GameTimer
            // 
            this.GameTimer.Interval = 17;
            this.GameTimer.Tick += new System.EventHandler(this.GameTimer_Tick);
            // 
            // DrawTimer
            // 
            this.DrawTimer.Interval = 17;
            this.DrawTimer.Tick += new System.EventHandler(this.DrawTimer_Tick);
            // 
            // NewGenerationTimer
            // 
            this.NewGenerationTimer.Tick += new System.EventHandler(this.NewGenerationTimer_Tick);
            // 
            // lblGenerationCount
            // 
            this.lblGenerationCount.AutoSize = true;
            this.lblGenerationCount.BackColor = System.Drawing.Color.Transparent;
            this.lblGenerationCount.Font = new System.Drawing.Font("Yu Gothic UI", 18F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.lblGenerationCount.Location = new System.Drawing.Point(13, 11);
            this.lblGenerationCount.Name = "lblGenerationCount";
            this.lblGenerationCount.Size = new System.Drawing.Size(78, 32);
            this.lblGenerationCount.TabIndex = 3;
            this.lblGenerationCount.Text = "Gen 0";
            // 
            // GameHost
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lblGenerationCount);
            this.DoubleBuffered = true;
            this.Name = "GameHost";
            this.Size = new System.Drawing.Size(446, 285);
            this.Load += new System.EventHandler(this.GameHost_Load);
            this.Paint += new System.Windows.Forms.PaintEventHandler(this.GameHost_Paint);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Timer GameTimer;
        private System.Windows.Forms.Timer DrawTimer;
        private System.Windows.Forms.Timer NewGenerationTimer;
        private System.Windows.Forms.Label lblGenerationCount;
    }
}
