using MaceEvolve.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace MaceEvolve.Controls
{
    public partial class NeuralNetworkViewer : UserControl
    {
        #region Fields
        public NeuralNetwork _NeuralNetwork;
        #endregion

        #region Properties
        public NeuralNetwork NeuralNetwork
        {
            get
            {
                return _NeuralNetwork;
            }
            set
            {
                if (_NeuralNetwork != value)
                {
                    SelectedNodeId = null;
                    MovingNodeId = null;

                    _NeuralNetwork = value;

                    ResetDrawnNodes();
                }
            }
        }
        private Dictionary<NodeType, Color> _NodeTypeToColorDict { get; } = new Dictionary<NodeType, Color>()
        {
            { NodeType.Input, Color.Blue },
            { NodeType.Process, Color.Gray },
            { NodeType.Output, Color.Orange }
        };
        private Dictionary<NodeType, Brush> _NodeTypeToBrushDict { get; }
        private Dictionary<int, GameObject> DrawnNodeIdsToGameObject { get; set; } = new Dictionary<int, GameObject>();
        public int NodeSize = 75;
        public int NodeFontSize = 14;
        public int? SelectedNodeId { get; set; }
        public int? MovingNodeId { get; set; }
        #endregion

        #region Constructors
        public NeuralNetworkViewer()
            : this(null)
        {
        }
        public NeuralNetworkViewer(NeuralNetwork NeuralNetwork)
        {
            this.NeuralNetwork = NeuralNetwork;

            _NodeTypeToBrushDict = new Dictionary<NodeType, Brush>()
            {
                { NodeType.Input, new SolidBrush(_NodeTypeToColorDict[NodeType.Input]) },
                { NodeType.Process, new SolidBrush(_NodeTypeToColorDict[NodeType.Process]) },
                { NodeType.Output, new SolidBrush(_NodeTypeToColorDict[NodeType.Output]) }
            };

            DoubleBuffered = true;

            InitializeComponent();
        }
        #endregion

        #region Methods
        public void ResetDrawnNodes()
        {
            DrawnNodeIdsToGameObject.Clear();

            Dictionary<int, Node> NodeIdsToNodesDict = NeuralNetwork.NodeIdsToNodesDict.ToDictionary(x => x.Key, x => x.Value);

            foreach (var KeyValuePair in NodeIdsToNodesDict)
            {
                int NodeId = KeyValuePair.Key;
                Node Node = KeyValuePair.Value;

                GameObject NodeGameObject = new GameObject();
                NodeGameObject.Size = NodeSize;

                int XLowerimit;
                int XUpperLimit;
                switch (Node.NodeType)
                {
                    case NodeType.Input:
                        XLowerimit = Bounds.Left;
                        XUpperLimit = (Bounds.Left + Bounds.Width / 3) - (int)NodeGameObject.Size;
                        break;

                    case NodeType.Process:
                        XLowerimit = Bounds.Left + Bounds.Width / 3;
                        XUpperLimit = (Bounds.Left + (Bounds.Width / 3) * 2) - (int)NodeGameObject.Size;
                        break;

                    case NodeType.Output:
                        XLowerimit = Bounds.Left + (Bounds.Left + (Bounds.Width / 3) * 2);
                        XUpperLimit = (Bounds.Left + (Bounds.Width / 3) * 3) - (int)NodeGameObject.Size;
                        break;

                    default:
                        throw new NotImplementedException(nameof(Node.NodeType));
                }

                NodeGameObject.X = Globals.Random.Next(XLowerimit, XUpperLimit);
                NodeGameObject.Y = Globals.Random.Next(Bounds.Bottom - (int)NodeGameObject.Size);

                DrawnNodeIdsToGameObject.Add(NodeId, NodeGameObject);
            }
        }

        private void NeuralNetworkViewer_Paint(object sender, PaintEventArgs e)
        {
            if (NeuralNetwork != null)
            {
                Node SelectedNode = SelectedNodeId == null ? null : NeuralNetwork.NodeIdsToNodesDict[SelectedNodeId.Value];
                e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
                e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

                //Draw connections between nodes.
                //Currently, duplicate connections will draw over each other. Self referencing connections aren't supported.
                List<Connection> NetworkConnectionsList = NeuralNetwork.Connections.ToList();
                foreach (var Connection in NetworkConnectionsList)
                {
                    GameObject SourceIdGameObject;
                    GameObject TargetIdGameObject;

                    if (DrawnNodeIdsToGameObject.TryGetValue(Connection.SourceId, out SourceIdGameObject) && DrawnNodeIdsToGameObject.TryGetValue(Connection.TargetId, out TargetIdGameObject))
                    {
                        Color PenColor;
                        float PenSize;
                        if (Connection.Weight == 0)
                        {
                            PenColor = Color.Gray;
                            PenSize = (int)Globals.Map(Connection.Weight, -4, 4, 2, 8);
                        }
                        if (Connection.Weight > 0)
                        {
                            PenColor = Color.FromArgb(0, (int)Globals.Map(Connection.Weight, 0, 4, 0, 255), 0);
                            PenSize = (int)Globals.Map(Connection.Weight, 0, 4, 2, 8);
                        }
                        else
                        {
                            PenColor = Color.FromArgb((int)Globals.Map(Connection.Weight, 0, -4, 0, 255), 0, 0);
                            PenSize = (int)Globals.Map(Connection.Weight, 0, -4, 2, 8);
                        }

                        e.Graphics.DrawLine(new Pen(PenColor, PenSize), (int)SourceIdGameObject.MX, (int)SourceIdGameObject.MY, (int)TargetIdGameObject.MX, (int)TargetIdGameObject.MY);
                    }
                }

                NeuralNetworkStepInfo HighestOutputNodeStepInfo = NeuralNetwork.PreviousStepInfo.Where(x => x.NodeType == NodeType.Output).OrderBy(x => x.PreviousOutput).LastOrDefault();

                //Draw nodes.
                foreach (var KeyValuePair in DrawnNodeIdsToGameObject)
                {
                    int NodeId = KeyValuePair.Key;
                    Node Node = NeuralNetwork.NodeIdsToNodesDict[NodeId];
                    GameObject NodeGameObject = KeyValuePair.Value;

                    Brush NodeBrush = _NodeTypeToBrushDict[Node.NodeType];

                    NeuralNetworkStepInfo NodeNetworkStepInfo = NeuralNetwork.PreviousStepInfo.FirstOrDefault(x => x.NodeId == NodeId);
                    string PreviousOutputString = NodeNetworkStepInfo == null ? null : string.Format("{0:0.##}", NodeNetworkStepInfo.PreviousOutput);
                    int NodeIdFontSize = NodeFontSize - 4;
                    int NodePreviousOutputFontSize = NodeFontSize;

                    e.Graphics.FillEllipse(NodeBrush, (float)NodeGameObject.X, (float)NodeGameObject.Y, (float)NodeGameObject.Size, (float)NodeGameObject.Size);

                    if (HighestOutputNodeStepInfo != null && NodeNetworkStepInfo == HighestOutputNodeStepInfo)
                    {
                        e.Graphics.DrawEllipse(new Pen(Color.White, 3), (float)NodeGameObject.X, (float)NodeGameObject.Y, (float)NodeGameObject.Size, (float)NodeGameObject.Size);
                    }

                    if (NodeId == SelectedNodeId)
                    {
                        e.Graphics.DrawEllipse(new Pen(Color.White, 6), (float)NodeGameObject.X, (float)NodeGameObject.Y, (float)NodeGameObject.Size, (float)NodeGameObject.Size);
                    }

                    e.Graphics.DrawString($"{NodeId}", new Font(FontFamily.GenericSansSerif, NodeIdFontSize, FontStyle.Bold), new SolidBrush(Color.Black), (float)NodeGameObject.MX - NodeIdFontSize, (float)NodeGameObject.MY - NodePreviousOutputFontSize * 2);
                    e.Graphics.DrawString(PreviousOutputString, new Font(FontFamily.GenericSansSerif, NodePreviousOutputFontSize), new SolidBrush(Color.Black), (float)NodeGameObject.MX - NodePreviousOutputFontSize * 2, (float)NodeGameObject.MY);

                }

                lblNetworkConnectionsCount.Text = $"Connections: {NeuralNetwork.Connections.Count}";
                lblNetworkNodesCount.Text = $"Nodes: {NeuralNetwork.NodeIdsToNodesDict.Count}";

                lblSelectedNodeId.Visible = SelectedNodeId != null;
                lblSelectedNodePreviousOutput.Visible = SelectedNodeId != null;
                lblSelectedNodeConnectionCount.Visible = SelectedNodeId != null;
                if (SelectedNodeId != null)
                {
                    lblSelectedNodeId.Text = $"Id: {SelectedNodeId}";
                    lblSelectedNodePreviousOutput.Text = $"Previous Output: {NeuralNetwork.PreviousStepInfo.FirstOrDefault(x => x.NodeId == SelectedNodeId).PreviousOutput}";
                    lblSelectedNodeConnectionCount.Text = $"Connections: {NetworkConnectionsList.Where(x => x.SourceId == SelectedNodeId || x.TargetId == SelectedNodeId).Count()}";
                }


            }
        }
        private void NeuralNetworkViewer_MouseDown(object sender, MouseEventArgs e)
        {
            Point RelativeMouseLocation = new Point(e.X - Bounds.Location.X, e.Y - Bounds.Location.Y);

            Dictionary<int, GameObject> NodeIdsOrderedByDistanceToMouse = DrawnNodeIdsToGameObject.OrderBy(x => Globals.GetDistanceFrom(RelativeMouseLocation.X, RelativeMouseLocation.Y, x.Value.MX, x.Value.MY)).ToDictionary(x => x.Key, x => x.Value);

            int? ClosestNodeId = NodeIdsOrderedByDistanceToMouse.Count == 0 ? null : NodeIdsOrderedByDistanceToMouse.FirstOrDefault().Key;

            if (ClosestNodeId == null || Globals.GetDistanceFrom(RelativeMouseLocation.X, RelativeMouseLocation.Y, NodeIdsOrderedByDistanceToMouse[ClosestNodeId.Value].MX, NodeIdsOrderedByDistanceToMouse[ClosestNodeId.Value].MY) > NodeIdsOrderedByDistanceToMouse[ClosestNodeId.Value].Size / 2)
            {
                SelectedNodeId = null;
            }
            else
            {
                SelectedNodeId = ClosestNodeId.Value;
            }

            MovingNodeId = SelectedNodeId;
        }
        private void NeuralNetworkViewer_MouseUp(object sender, MouseEventArgs e)
        {
            MovingNodeId = null;
        }
        private void NeuralNetworkViewer_MouseMove(object sender, MouseEventArgs e)
        {
            Point RelativeMouseLocation = new Point(e.X - Bounds.Location.X, e.Y - Bounds.Location.Y);

            if (MovingNodeId != null)
            {
                GameObject MovingNodeGameObject = DrawnNodeIdsToGameObject[MovingNodeId.Value];

                MovingNodeGameObject.X = RelativeMouseLocation.X - MovingNodeGameObject.Size / 2;
                MovingNodeGameObject.Y = RelativeMouseLocation.Y - MovingNodeGameObject.Size / 2;
            }
        }
        private void DrawTimer_Tick(object sender, System.EventArgs e)
        {
            Invalidate();
        }
        #endregion
    }
}
