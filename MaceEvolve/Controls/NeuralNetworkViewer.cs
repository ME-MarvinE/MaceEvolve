using MaceEvolve.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace MaceEvolve.Controls
{
    public partial class NeuralNetworkViewer : UserControl
    {
        #region Fields
        public NeuralNetwork _neuralNetwork;
        #endregion

        #region Properties
        public NeuralNetwork NeuralNetwork
        {
            get
            {
                return _neuralNetwork;
            }
            set
            {
                if (_neuralNetwork != value)
                {
                    SelectedNodeId = null;
                    MovingNodeId = null;

                    _neuralNetwork = value;

                    ResetDrawnNodes();
                }
            }
        }
        private Dictionary<NodeType, Color> NodeTypeToColorDict { get; } = new Dictionary<NodeType, Color>()
        {
            { NodeType.Input, Color.Blue },
            { NodeType.Process, Color.Gray },
            { NodeType.Output, Color.Orange }
        };
        private Dictionary<NodeType, Brush> NodeTypeToBrushDict { get; }
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
        public NeuralNetworkViewer(NeuralNetwork neuralNetwork)
        {
            NeuralNetwork = neuralNetwork;

            NodeTypeToBrushDict = new Dictionary<NodeType, Brush>()
            {
                { NodeType.Input, new SolidBrush(NodeTypeToColorDict[NodeType.Input]) },
                { NodeType.Process, new SolidBrush(NodeTypeToColorDict[NodeType.Process]) },
                { NodeType.Output, new SolidBrush(NodeTypeToColorDict[NodeType.Output]) }
            };

            DoubleBuffered = true;

            InitializeComponent();
        }
        #endregion

        #region Methods
        public void ResetDrawnNodes()
        {
            DrawnNodeIdsToGameObject.Clear();

            if (NeuralNetwork != null)
            {
                Dictionary<int, Node> nodeIdsToNodesDict = NeuralNetwork.NodeIdsToNodesDict.ToDictionary(x => x.Key, x => x.Value);

                foreach (var keyValuePair in nodeIdsToNodesDict)
                {
                    int nodeId = keyValuePair.Key;
                    Node node = keyValuePair.Value;

                    GameObject nodeGameObject = new GameObject();
                    nodeGameObject.Size = NodeSize;

                    int xLowerimit;
                    int xUpperLimit;
                    switch (node.NodeType)
                    {
                        case NodeType.Input:
                            xLowerimit = Bounds.Left;
                            xUpperLimit = (Bounds.Left + Bounds.Width / 3) - (int)nodeGameObject.Size;
                            break;

                        case NodeType.Process:
                            xLowerimit = Bounds.Left + Bounds.Width / 3;
                            xUpperLimit = (Bounds.Left + (Bounds.Width / 3) * 2) - (int)nodeGameObject.Size;
                            break;

                        case NodeType.Output:
                            xLowerimit = Bounds.Left + (Bounds.Left + (Bounds.Width / 3) * 2);
                            xUpperLimit = (Bounds.Left + (Bounds.Width / 3) * 3) - (int)nodeGameObject.Size;
                            break;

                        default:
                            throw new NotImplementedException(nameof(node.NodeType));
                    }
                    if (xUpperLimit < xLowerimit)
                    {
                        xUpperLimit = xLowerimit;
                    }

                    nodeGameObject.X = Globals.Random.Next(xLowerimit, xUpperLimit);
                    nodeGameObject.Y = (Bounds.Bottom - nodeGameObject.Size) > 0 ? Globals.Random.Next(Bounds.Bottom - (int)nodeGameObject.Size) : 0;

                    DrawnNodeIdsToGameObject.Add(nodeId, nodeGameObject);
                }
            }
        }
        private void NeuralNetworkViewer_Paint(object sender, PaintEventArgs e)
        {
            if (NeuralNetwork != null)
            {
                Node selectedNode = SelectedNodeId == null ? null : NeuralNetwork.NodeIdsToNodesDict[SelectedNodeId.Value];
                e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
                e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

                //Draw connections between nodes.
                //Currently, duplicate connections will draw over each other. Self referencing connections aren't supported.
                List<Connection> networkConnectionsList = NeuralNetwork.Connections.ToList();
                foreach (var connection in networkConnectionsList)
                {
                    GameObject sourceIdGameObject;
                    GameObject targetIdGameObject;

                    if (DrawnNodeIdsToGameObject.TryGetValue(connection.SourceId, out sourceIdGameObject) && DrawnNodeIdsToGameObject.TryGetValue(connection.TargetId, out targetIdGameObject))
                    {
                        Color penColor;
                        float penSize;

                        if (connection.Weight == 0)
                        {
                            penColor = Color.Gray;
                            penSize = (int)Globals.Map(connection.Weight, -4, 4, 2, 8);
                        }
                        if (connection.Weight > 0)
                        {
                            penColor = Color.FromArgb(0, (int)Globals.Map(connection.Weight, 0, 4, 0, 255), 0);
                            penSize = (int)Globals.Map(connection.Weight, 0, 4, 2, 8);
                        }
                        else
                        {
                            penColor = Color.FromArgb((int)Globals.Map(connection.Weight, 0, -4, 0, 255), 0, 0);
                            penSize = (int)Globals.Map(connection.Weight, 0, -4, 2, 8);
                        }

                        e.Graphics.DrawLine(new Pen(penColor, penSize), (int)sourceIdGameObject.MX, (int)sourceIdGameObject.MY, (int)targetIdGameObject.MX, (int)targetIdGameObject.MY);
                    }
                }

                NeuralNetworkStepInfo highestOutputNodeStepInfo = NeuralNetwork.PreviousStepInfo.Where(x => x.NodeType == NodeType.Output).OrderBy(x => x.PreviousOutput).LastOrDefault();

                //Draw nodes.
                foreach (var keyValuePair in DrawnNodeIdsToGameObject)
                {
                    int nodeId = keyValuePair.Key;
                    Node node = NeuralNetwork.NodeIdsToNodesDict[nodeId];
                    GameObject nodeGameObject = keyValuePair.Value;
                    Brush nodeBrush = NodeTypeToBrushDict[node.NodeType];

                    NeuralNetworkStepInfo nodeNetworkStepInfo = NeuralNetwork.PreviousStepInfo.FirstOrDefault(x => x.NodeId == nodeId);

                    string previousOutputString = nodeNetworkStepInfo == null ? null : string.Format("{0:0.##}", nodeNetworkStepInfo.PreviousOutput);
                    int nodeIdFontSize = NodeFontSize - 4;
                    int nodePreviousOutputFontSize = NodeFontSize;

                    e.Graphics.FillEllipse(nodeBrush, (float)nodeGameObject.X, (float)nodeGameObject.Y, (float)nodeGameObject.Size, (float)nodeGameObject.Size);

                    if (highestOutputNodeStepInfo != null && nodeNetworkStepInfo == highestOutputNodeStepInfo)
                    {
                        e.Graphics.DrawEllipse(new Pen(Color.White, 3), (float)nodeGameObject.X, (float)nodeGameObject.Y, (float)nodeGameObject.Size, (float)nodeGameObject.Size);
                    }

                    if (nodeId == SelectedNodeId)
                    {
                        e.Graphics.DrawEllipse(new Pen(Color.White, 6), (float)nodeGameObject.X, (float)nodeGameObject.Y, (float)nodeGameObject.Size, (float)nodeGameObject.Size);
                    }

                    e.Graphics.DrawString($"{nodeId}", new Font(FontFamily.GenericSansSerif, nodeIdFontSize, FontStyle.Bold), new SolidBrush(Color.Black), (float)nodeGameObject.MX - nodeIdFontSize, (float)nodeGameObject.MY - nodePreviousOutputFontSize * 2);
                    e.Graphics.DrawString(previousOutputString, new Font(FontFamily.GenericSansSerif, nodePreviousOutputFontSize), new SolidBrush(Color.Black), (float)nodeGameObject.MX - nodePreviousOutputFontSize * 2, (float)nodeGameObject.MY);

                }

                lblNetworkConnectionsCount.Text = $"Connections: {NeuralNetwork.Connections.Count}";
                lblNetworkNodesCount.Text = $"Nodes: {NeuralNetwork.NodeIdsToNodesDict.Count}";

                lblSelectedNodeId.Visible = SelectedNodeId != null;
                lblSelectedNodePreviousOutput.Visible = SelectedNodeId != null;
                lblSelectedNodeConnectionCount.Visible = SelectedNodeId != null;
                lblNodeInputOrAction.Visible = SelectedNodeId != null;
                if (SelectedNodeId != null)
                {
                    lblSelectedNodeId.Text = $"Id: {SelectedNodeId}";
                    lblSelectedNodePreviousOutput.Text = $"Previous Output: {NeuralNetwork.PreviousStepInfo.FirstOrDefault(x => x.NodeId == SelectedNodeId).PreviousOutput}";
                    lblSelectedNodeConnectionCount.Text = $"Connections: {networkConnectionsList.Where(x => x.SourceId == SelectedNodeId || x.TargetId == SelectedNodeId).Count()}";

                    switch (selectedNode.NodeType)
                    {
                        case NodeType.Input:
                            lblNodeInputOrAction.Text = $"Type: Input ({selectedNode.CreatureInput})";
                            break;

                        case NodeType.Process:
                            lblNodeInputOrAction.Text = "Type: Process";
                            break;

                        case NodeType.Output:
                            lblNodeInputOrAction.Text = $"Type: Output ({selectedNode.CreatureAction})";
                            break;

                        default:
                            throw new NotImplementedException();
                    }
                }
            }
        }
        private void NeuralNetworkViewer_MouseDown(object sender, MouseEventArgs e)
        {
            Point relativeMouseLocation = new Point(e.X - Bounds.Location.X, e.Y - Bounds.Location.Y);

            Dictionary<int, GameObject> nodeIdsOrderedByDistanceToMouse = DrawnNodeIdsToGameObject.OrderBy(x => Globals.GetDistanceFrom(relativeMouseLocation.X, relativeMouseLocation.Y, x.Value.MX, x.Value.MY)).ToDictionary(x => x.Key, x => x.Value);

            int? closestNodeId = nodeIdsOrderedByDistanceToMouse.Count == 0 ? null : nodeIdsOrderedByDistanceToMouse.FirstOrDefault().Key;

            if (closestNodeId == null || Globals.GetDistanceFrom(relativeMouseLocation.X, relativeMouseLocation.Y, nodeIdsOrderedByDistanceToMouse[closestNodeId.Value].MX, nodeIdsOrderedByDistanceToMouse[closestNodeId.Value].MY) > nodeIdsOrderedByDistanceToMouse[closestNodeId.Value].Size / 2)
            {
                SelectedNodeId = null;
            }
            else
            {
                SelectedNodeId = closestNodeId.Value;
            }

            MovingNodeId = SelectedNodeId;
        }
        private void NeuralNetworkViewer_MouseUp(object sender, MouseEventArgs e)
        {
            MovingNodeId = null;
        }
        private void NeuralNetworkViewer_MouseMove(object sender, MouseEventArgs e)
        {
            Point relativeMouseLocation = new Point(e.X - Bounds.Location.X, e.Y - Bounds.Location.Y);

            if (MovingNodeId != null)
            {
                GameObject movingNodeGameObject = DrawnNodeIdsToGameObject[MovingNodeId.Value];

                movingNodeGameObject.X = relativeMouseLocation.X - movingNodeGameObject.Size / 2;
                movingNodeGameObject.Y = relativeMouseLocation.Y - movingNodeGameObject.Size / 2;
            }
        }
        private void DrawTimer_Tick(object sender, EventArgs e)
        {
            Invalidate();
        }
        #endregion
    }
}
