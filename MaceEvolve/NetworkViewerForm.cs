using MaceEvolve.Controls;
using System.Windows.Forms;

namespace MaceEvolve
{
    public partial class NetworkViewerForm : Form
    {
        #region Fields
        private NeuralNetworkViewer _NetworkViewer;
        #endregion

        #region Properties
        public NeuralNetworkViewer NetworkViewer
        {
            get
            {
                return _NetworkViewer;
            }
            set
            {
                if (_NetworkViewer != value && value != null)
                {
                    if (_NetworkViewer != null)
                    {
                        Controls.Clear();
                    }

                    _NetworkViewer = value;
                    Controls.Add(_NetworkViewer);
                }

            }
        }
        #endregion

        #region Constructors
        public NetworkViewerForm()
            : this(null)
        {
        }
        public NetworkViewerForm(NeuralNetworkViewer networkViewer)
        {
            InitializeComponent();

            NetworkViewer = networkViewer;
        }
        #endregion
    }
}
