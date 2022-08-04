using MaceEvolve.Controls;
using System;
using System.Windows.Forms;

namespace MaceEvolve
{
    public partial class MainForm : Form
    {
        public GameHost MainGameHost = new GameHost() { Dock = DockStyle.Fill };
        public MainForm()
        {
            InitializeComponent();
            Controls.Add(MainGameHost);
        }

        private void StartButton_Click(object sender, EventArgs e)
        {
            MainGameHost.Start();
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            MainGameHost.Stop();
        }

        private void ResetButton_Click(object sender, EventArgs e)
        {
            MainGameHost.Reset();
        }

        private void NextGenButton_Click(object sender, EventArgs e)
        {
            MainGameHost.SecondsUntilNewGeneration = 0;
            MainGameHost.NewGenerationTimer_Tick(this, e);
        }
    }
}