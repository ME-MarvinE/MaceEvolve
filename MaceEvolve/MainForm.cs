using MaceEvolve.Controls;
using System;
using System.Drawing;
using System.Windows.Forms;

namespace MaceEvolve
{
    public partial class MainForm : Form
    {
        public GameHost MainGameHost = new GameHost()
        {
            Dock = DockStyle.Fill,
            GenLabelTextColor = Color.White,
            GenEndsInLabelTextColor = Color.White,
            SecondsUntilNewGeneration = 12,
            GenerationCount = 1
        };
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

        private void btnTrackBestCreature_Click(object sender, EventArgs e)
        {
            NetworkViewerForm NetworkViewerForm = new NetworkViewerForm(MainGameHost.BestCreatureNeuralNetworkViewer);
            NetworkViewerForm.Show();
        }
    }
}