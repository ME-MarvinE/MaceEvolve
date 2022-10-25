using MaceEvolve.Controls;
using MaceEvolve.Models;
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
        public NetworkViewerForm(NeuralNetworkViewer NetworkViewer)
        {
            InitializeComponent();

            this.NetworkViewer = NetworkViewer;
        }
        #endregion
    }
}
