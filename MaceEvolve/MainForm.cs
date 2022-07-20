using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MaceEvolve
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
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