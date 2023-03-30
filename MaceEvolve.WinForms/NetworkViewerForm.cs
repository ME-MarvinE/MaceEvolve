using MaceEvolve.WinForms.Controls;
using System.Windows.Forms;

namespace MaceEvolve.WinForms
{
    public partial class NetworkViewerForm : Form
    {
        #region Fields
        private NeuralNetworkViewer _networkViewer;
        #endregion

        #region Properties
        public NeuralNetworkViewer NetworkViewer
        {
            get
            {
                return _networkViewer;
            }
            set
            {
                if (_networkViewer != value)
                {
                    if (_networkViewer != null)
                    {
                        Controls.Clear();
                    }

                    _networkViewer = value;
                    Controls.Add(_networkViewer);
                }

            }
        }
        #endregion

        #region Constructor
        public NetworkViewerForm()
        {
            InitializeComponent();
            FormClosing += NetworkViewerForm_FormClosing;
        }
        #endregion

        #region Methods
        private void NetworkViewerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Hide();
            e.Cancel = true;
        }
        #endregion
    }
}
