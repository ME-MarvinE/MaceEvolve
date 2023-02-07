﻿using MaceEvolve.Core;
using MaceEvolve.Core.Enums;
using MaceEvolve.Core.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace MaceEvolve.WinForms.Controls
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
        public static Color GetConnectionPenColor(Connection connection)
        {
            if (connection.Weight == 0)
            {
                return Color.Gray;
            }
            if (connection.Weight > 0)
            {
                return Color.FromArgb(0, (int)Globals.Map(connection.Weight, 0, 4, 0, 255), 0);
            }
            else
            {
                return Color.FromArgb((int)Globals.Map(connection.Weight, 0, -4, 0, 255), 0, 0);
            }
        }
        public static int GetConnectionPenSize(Connection connection)
        {
            if (connection.Weight == 0)
            {
                return (int)Globals.Map(connection.Weight, -4, 4, 2, 8);
            }
            if (connection.Weight > 0)
            {
                return (int)Globals.Map(connection.Weight, 0, 4, 2, 8);
            }
            else
            {
                return (int)Globals.Map(connection.Weight, 0, -4, 2, 8);
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
                //Currently, duplicate connections will draw over each other.
                List<Connection> drawableConnections = NeuralNetwork.Connections.Where(x => DrawnNodeIdsToGameObject.Keys.Contains(x.SourceId) && DrawnNodeIdsToGameObject.Keys.Contains(x.TargetId)).ToList();

                foreach (var connection in drawableConnections)
                {
                    if (connection.SourceId != connection.TargetId)
                    {
                        GameObject sourceIdGameObject = DrawnNodeIdsToGameObject[connection.SourceId];
                        GameObject targetIdGameObject = DrawnNodeIdsToGameObject[connection.TargetId];
                        Color penColor = GetConnectionPenColor(connection);
                        float penSize = GetConnectionPenSize(connection);

                        e.Graphics.DrawLine(new Pen(penColor, penSize), sourceIdGameObject.MX, sourceIdGameObject.MY, targetIdGameObject.MX, targetIdGameObject.MY);
                    }
                }

                NeuralNetworkStepNodeInfo highestOutputNodeStepInfo = NeuralNetwork.PreviousStepInfo.Where(x => x.NodeType == NodeType.Output).OrderBy(x => x.PreviousOutput).LastOrDefault();

                //Draw nodes and self referencing connections.
                foreach (var keyValuePair in DrawnNodeIdsToGameObject)
                {
                    int nodeId = keyValuePair.Key;
                    Node node = NeuralNetwork.NodeIdsToNodesDict[nodeId];
                    GameObject nodeGameObject = keyValuePair.Value;
                    Brush nodeBrush = NodeTypeToBrushDict[node.NodeType];

                    //Draw the node's self referencing connections.
                    List<Connection> selfReferencingConnections = drawableConnections.Where(x => x.SourceId == nodeId && x.SourceId == x.TargetId).ToList();
                    float selfReferencingConnectionAngle = selfReferencingConnections.Count <= 1 ? 0 : 360 / selfReferencingConnections.Count;

                    for (int i = 0; i < selfReferencingConnections.Count; i++)
                    {
                        float angleToDrawConnection = (i + 1) * selfReferencingConnectionAngle;
                        angleToDrawConnection += 225; //Offset because ellipse is drawn from top left. This makes the first circle be drawn above te node instead of to the right.

                        Connection connection = selfReferencingConnections[i];
                        GameObject sourceIdGameObject = DrawnNodeIdsToGameObject[connection.SourceId];
                        GameObject targetIdGameObject = DrawnNodeIdsToGameObject[connection.TargetId];

                        Color penColor = GetConnectionPenColor(connection);
                        float penSize = GetConnectionPenSize(connection);

                        e.Graphics.TranslateTransform(sourceIdGameObject.MX, sourceIdGameObject.MY);
                        e.Graphics.RotateTransform(angleToDrawConnection);
                        e.Graphics.DrawEllipse(new Pen(penColor, penSize), 0, 0, sourceIdGameObject.Size * 0.75f, sourceIdGameObject.Size * 0.75f);
                        e.Graphics.ResetTransform();
                    }

                    //Draw the node.
                    NeuralNetworkStepNodeInfo nodeNetworkStepInfo = NeuralNetwork.PreviousStepInfo.FirstOrDefault(x => x.NodeId == nodeId);

                    string previousOutputString = nodeNetworkStepInfo == null ? "N/A" : string.Format("{0:0.##}", nodeNetworkStepInfo.PreviousOutput);
                    int nodeIdFontSize = NodeFontSize - 4;
                    int nodePreviousOutputFontSize = NodeFontSize;

                    e.Graphics.FillEllipse(nodeBrush, nodeGameObject.X, nodeGameObject.Y, nodeGameObject.Size, nodeGameObject.Size);

                    if (highestOutputNodeStepInfo != null && nodeNetworkStepInfo == highestOutputNodeStepInfo)
                    {
                        e.Graphics.DrawEllipse(new Pen(Color.White, 3), nodeGameObject.X, nodeGameObject.Y, nodeGameObject.Size, nodeGameObject.Size);
                    }

                    if (nodeId == SelectedNodeId)
                    {
                        e.Graphics.DrawEllipse(new Pen(Color.White, 6), nodeGameObject.X, nodeGameObject.Y, nodeGameObject.Size, nodeGameObject.Size);
                    }

                    e.Graphics.DrawString($"{nodeId}", new Font(FontFamily.GenericSansSerif, nodeIdFontSize, FontStyle.Bold), new SolidBrush(Color.Black), nodeGameObject.MX - nodeIdFontSize, nodeGameObject.MY - nodePreviousOutputFontSize * 2);
                    e.Graphics.DrawString(previousOutputString, new Font(FontFamily.GenericSansSerif, nodePreviousOutputFontSize), new SolidBrush(Color.Black), nodeGameObject.MX - nodePreviousOutputFontSize * 2, nodeGameObject.MY);
                }

                lblNetworkConnectionsCount.Text = $"Connections: {NeuralNetwork.Connections.Count}";
                lblNetworkNodesCount.Text = $"Nodes: {NeuralNetwork.NodeIdsToNodesDict.Count}";

                lblSelectedNodeId.Visible = SelectedNodeId != null;
                lblSelectedNodePreviousOutput.Visible = SelectedNodeId != null;
                lblSelectedNodeConnectionCount.Visible = SelectedNodeId != null;
                lblNodeInputOrAction.Visible = SelectedNodeId != null;
                if (SelectedNodeId != null)
                {
                    NeuralNetworkStepNodeInfo selectedNodeStepInfo = NeuralNetwork.PreviousStepInfo.FirstOrDefault(x => x.NodeId == SelectedNodeId);

                    lblSelectedNodeId.Text = $"Id: {SelectedNodeId}";
                    lblSelectedNodePreviousOutput.Text = $"Previous Output: {(selectedNodeStepInfo == null ? "N/A" : selectedNodeStepInfo.PreviousOutput)}";
                    lblSelectedNodeConnectionCount.Text = $"Connections: {NeuralNetwork.Connections.Where(x => x.SourceId == SelectedNodeId || x.TargetId == SelectedNodeId).Count()}";

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
