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
    }
}