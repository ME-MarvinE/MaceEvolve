using MaceEvolve.Core;
using MaceEvolve.Core.Enums;
using MaceEvolve.Core.Models;
using MaceEvolve.WinForms.Models;
using System;
using System.Collections.Concurrent;
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
        private int _maxNodeStaggerLevel = 5;
        #endregion

        #region Properties
        public IDictionary<GraphicalCreature, List<NeuralNetworkStepNodeInfo>> CreaturesBrainOutput { get; set; }
        public NeuralNetwork NeuralNetwork
        {
            get
            {
                return _neuralNetwork;
            }
            set
            {
                lock (_lock)
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
        }
        private Dictionary<NodeType, Color> NodeTypeToColorDict { get; } = new Dictionary<NodeType, Color>()
        {
            { NodeType.Input, Color.Blue },
            { NodeType.Process, Color.Gray },
            { NodeType.Output, Color.Orange }
        };
        private Dictionary<NodeType, Brush> NodeTypeToBrushDict { get; }
        private ConcurrentDictionary<int, GameObject> DrawnNodeIdsToGameObject { get; set; } = new ConcurrentDictionary<int, GameObject>();
        public float NodeSize = 75;
        public int NodeFontSize = 14;
        public int? SelectedNodeId { get; set; }
        public int? MovingNodeId { get; set; }
        private object _lock { get; set; } = new object();
        float MinNodeVerticalSpacing { get; set; }
        public int SelectedNodeConnectionsHighlightSize { get; set; } = 1;
        public Color SelectedNodeConnectionsColor { get; set; } = Color.White;
        public Color ActiveNodeConnectionsColor { get; set; } = Color.FromArgb(25, 255, 255, 255);
        public int ActiveNodeConnectionsHighlightSize { get; set; } = 1;
        int MaxNodeStaggerLevel
        {
            get
            {
                return _maxNodeStaggerLevel;
            }
            set
            {
                _maxNodeStaggerLevel = value;
                nudMaxNodeStaggerLevel.Value = _maxNodeStaggerLevel;
            }
        }
        #endregion

        #region Constructors
        public NeuralNetworkViewer()
        {
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
                List<int> nodeIds = NeuralNetwork.GetNodeIds();
                List<int> inputNodeIds = new List<int>();
                List<int> processNodeIds = new List<int>();
                List<int> outputNodeIds = new List<int>();

                foreach (var nodeId in nodeIds)
                {
                    switch (NeuralNetwork.NodeIdsToNodesDict[nodeId].NodeType)
                    {
                        case NodeType.Input:
                            inputNodeIds.Add(nodeId);
                            break;

                        case NodeType.Process:
                            processNodeIds.Add(nodeId);
                            break;

                        case NodeType.Output:
                            outputNodeIds.Add(nodeId);
                            break;

                        default:
                            throw new NotImplementedException(Enum.GetName(NeuralNetwork.NodeIdsToNodesDict[nodeId].NodeType));
                    }
                }

                Dictionary<int, float> nodeIdToYPosition = new Dictionary<int, float>();
                Dictionary<int, float> nodeIdToXPosition = new Dictionary<int, float>();
                float inputNodeIdVerticalSpacing = Math.Max(Math.Max(0, Bounds.Bottom - (NodeSize / 2)) / Math.Max(1, inputNodeIds.Count), MinNodeVerticalSpacing);
                float inputNodeMinX = Bounds.Left;
                float inputNodeMaxX = (inputNodeMinX + Bounds.Width / Globals.AllNodeTypes.Count);

                float processNodeIdVerticalSpacing = Math.Max(Math.Max(0, Bounds.Bottom - (NodeSize / 2)) / Math.Max(1, processNodeIds.Count), MinNodeVerticalSpacing);
                float processNodeMinX = inputNodeMaxX;
                float processNodeMaxX = (processNodeMinX + Bounds.Width / Globals.AllNodeTypes.Count);

                float outputNodeIdVerticalSpacing = Math.Max(Math.Max(0, Bounds.Bottom - (NodeSize / 2)) / Math.Max(1, outputNodeIds.Count), MinNodeVerticalSpacing);
                float outputNodeMinX = processNodeMaxX;
                float outputNodeMaxX = (outputNodeMinX + Bounds.Width / Globals.AllNodeTypes.Count);


                Dictionary<NodeType, int> nodeTypeToIteratorDict = Globals.AllNodeTypes.ToDictionary(x => x, x => 0);

                for (int i = 0; i < nodeIds.Count; i++)
                {
                    int nodeId = nodeIds[i];
                    Node node = NeuralNetwork.NodeIdsToNodesDict[nodeId];
                    int iterator = nodeTypeToIteratorDict[node.NodeType];

                    float verticalSpacing;
                    float minX;
                    float maxX;

                    switch (node.NodeType)
                    {
                        case NodeType.Input:
                            verticalSpacing = inputNodeIdVerticalSpacing;
                            minX = inputNodeMinX;
                            maxX = inputNodeMaxX;
                            break;

                        case NodeType.Process:
                            verticalSpacing = processNodeIdVerticalSpacing;
                            minX = processNodeMinX;
                            maxX = processNodeMaxX;
                            break;

                        case NodeType.Output:
                            verticalSpacing = outputNodeIdVerticalSpacing;
                            minX = outputNodeMinX;
                            maxX = outputNodeMaxX;
                            break;

                        default:
                            throw new NotImplementedException(Enum.GetName(node.NodeType));
                    }

                    float availableHorizontalSpace = (maxX - NodeSize) - minX;
                    bool stagger = verticalSpacing < NodeSize || node.NodeType == NodeType.Process;
                    float xPosition = (minX + availableHorizontalSpace / 2);

                    if (stagger)
                    {
                        int staggerPosition = iterator % Math.Max(MaxNodeStaggerLevel, 1);
                        float middleStaggerPosition = MaxNodeStaggerLevel / 3;
                        float staggerSize = Math.Min(NodeSize, availableHorizontalSpace / MaxNodeStaggerLevel);

                        if (staggerPosition < middleStaggerPosition)
                        {
                            xPosition -= staggerSize * staggerPosition;
                        }
                        else
                        {
                            xPosition += staggerSize * staggerPosition;
                        }

                        xPosition -= staggerSize * ((MaxNodeStaggerLevel - 1) / 2);
                    }

                    nodeIdToXPosition.Add(nodeId, xPosition);
                    nodeIdToYPosition.Add(nodeId, Bounds.Top + verticalSpacing * iterator);
                    nodeTypeToIteratorDict[node.NodeType] += 1;
                }

                for (int i = 0; i < nodeIds.Count; i++)
                {
                    int nodeId = nodeIds[i];

                    GameObject nodeGameObject = new GameObject
                    {
                        Size = NodeSize,
                        X = nodeIdToXPosition[nodeId],
                        Y = nodeIdToYPosition[nodeId]
                    };

                    DrawnNodeIdsToGameObject[nodeId] = nodeGameObject;
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
            if (connection.Weight >= 0)
            {
                return (int)Globals.Map(connection.Weight, 0, 5, 3, 9);
            }
            else
            {
                return (int)Globals.Map(connection.Weight, 0, -5, 3, 9);
            }
        }
        private void NeuralNetworkViewer_Paint(object sender, PaintEventArgs e)
        {
            lock (_lock)
            {
                if (NeuralNetwork != null && CreaturesBrainOutput != null)
                {
                    GraphicalCreature networkCreatureInStep = CreaturesBrainOutput.FirstOrDefault(x => x.Key.Brain == NeuralNetwork).Key;

                    if (networkCreatureInStep == null)
                    {
                        return;
                    }

                    e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
                    e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    //Draw connections between nodes.
                    //Currently, duplicate connections will draw over each other.
                    List<Connection> drawableConnections = networkCreatureInStep.Brain.Connections.Where(x => DrawnNodeIdsToGameObject.Keys.Contains(x.SourceId) && DrawnNodeIdsToGameObject.Keys.Contains(x.TargetId)).ToList();
                    NeuralNetworkStepNodeInfo highestOutputNodeStepInfo = CreaturesBrainOutput[networkCreatureInStep].Where(x => x.NodeType == NodeType.Output).OrderBy(x => x.PreviousOutput).LastOrDefault();

                    foreach (var connection in drawableConnections)
                    {
                        if (connection.SourceId != connection.TargetId)
                        {
                            GameObject sourceIdGameObject = DrawnNodeIdsToGameObject[connection.SourceId];
                            GameObject targetIdGameObject = DrawnNodeIdsToGameObject[connection.TargetId];
                            Color penColor = GetConnectionPenColor(connection);
                            float penSize = GetConnectionPenSize(connection);

                            e.Graphics.DrawLine(new Pen(penColor, penSize), sourceIdGameObject.MX, sourceIdGameObject.MY, targetIdGameObject.MX, targetIdGameObject.MY);

                            if (connection.SourceId == SelectedNodeId || connection.TargetId == SelectedNodeId || connection.SourceId == highestOutputNodeStepInfo?.NodeId || connection.TargetId == highestOutputNodeStepInfo?.NodeId)
                            {
                                e.Graphics.DrawLine(new Pen(SelectedNodeConnectionsColor, SelectedNodeConnectionsHighlightSize), sourceIdGameObject.MX, sourceIdGameObject.MY, targetIdGameObject.MX, targetIdGameObject.MY);
                            }

                            if (connection.SourceId == highestOutputNodeStepInfo?.NodeId || connection.TargetId == highestOutputNodeStepInfo?.NodeId)
                            {
                                e.Graphics.DrawLine(new Pen(ActiveNodeConnectionsColor, SelectedNodeConnectionsHighlightSize), sourceIdGameObject.MX, sourceIdGameObject.MY, targetIdGameObject.MX, targetIdGameObject.MY);
                            }
                        }
                    }

                    //Draw nodes and self referencing connections.
                    foreach (var keyValuePair in DrawnNodeIdsToGameObject)
                    {
                        int nodeId = keyValuePair.Key;
                        Node node = networkCreatureInStep.Brain.NodeIdsToNodesDict[nodeId];
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

                            if (connection.SourceId == SelectedNodeId || connection.TargetId == SelectedNodeId)
                            {
                                e.Graphics.DrawEllipse(new Pen(SelectedNodeConnectionsColor, SelectedNodeConnectionsHighlightSize), 0, 0, sourceIdGameObject.Size * 0.75f, sourceIdGameObject.Size * 0.75f);
                            }

                            if (connection.SourceId == highestOutputNodeStepInfo?.NodeId || connection.TargetId == highestOutputNodeStepInfo?.NodeId)
                            {
                                e.Graphics.DrawEllipse(new Pen(ActiveNodeConnectionsColor, ActiveNodeConnectionsHighlightSize), 0, 0, sourceIdGameObject.Size * 0.75f, sourceIdGameObject.Size * 0.75f);
                            }

                            e.Graphics.ResetTransform();
                        }

                        //Draw the node.
                        NeuralNetworkStepNodeInfo nodeNetworkStepInfo = CreaturesBrainOutput[networkCreatureInStep].Find(x => x.NodeId == nodeId);

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

                    lblNetworkConnectionsCount.Text = $"Connections: {networkCreatureInStep.Brain.Connections.Count}";
                    lblNetworkNodesCount.Text = $"Nodes: {networkCreatureInStep.Brain.NodeIdsToNodesDict.Count}";

                    lblSelectedNodeId.Visible = SelectedNodeId != null;
                    lblSelectedNodePreviousOutput.Visible = SelectedNodeId != null;
                    lblSelectedNodeConnectionCount.Visible = SelectedNodeId != null;
                    lblNodeInputOrAction.Visible = SelectedNodeId != null;
                    if (SelectedNodeId != null)
                    {
                        NeuralNetworkStepNodeInfo selectedNodeStepInfo = CreaturesBrainOutput[networkCreatureInStep].Find(x => x.NodeId == SelectedNodeId);

                        lblSelectedNodeId.Text = $"Id: {SelectedNodeId}";
                        lblSelectedNodePreviousOutput.Text = $"Previous Output: {(selectedNodeStepInfo == null ? "N/A" : selectedNodeStepInfo.PreviousOutput)}";
                        lblSelectedNodeConnectionCount.Text = $"Connections: {networkCreatureInStep.Brain.Connections.Count(x => x.SourceId == SelectedNodeId || x.TargetId == SelectedNodeId)}";


                        Node selectedNode = networkCreatureInStep.Brain.NodeIdsToNodesDict[SelectedNodeId.Value];

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

                    {
                        List<int> nodeIds = NeuralNetwork.GetNodeIds();
                        List<int> inputNodeIds = new List<int>();
                        List<int> processNodeIds = new List<int>();
                        List<int> outputNodeIds = new List<int>();

                        foreach (var nodeId in nodeIds)
                        {
                            switch (NeuralNetwork.NodeIdsToNodesDict[nodeId].NodeType)
                            {
                                case NodeType.Input:
                                    inputNodeIds.Add(nodeId);
                                    break;

                                case NodeType.Process:
                                    processNodeIds.Add(nodeId);
                                    break;

                                case NodeType.Output:
                                    outputNodeIds.Add(nodeId);
                                    break;

                                default:
                                    throw new NotImplementedException(Enum.GetName(NeuralNetwork.NodeIdsToNodesDict[nodeId].NodeType));
                            }
                        }
                    }

                }
            }
        }
        private void NeuralNetworkViewer_MouseDown(object sender, MouseEventArgs e)
        {
            if (NeuralNetwork != null && CreaturesBrainOutput != null)
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
        }
        private void NeuralNetworkViewer_MouseUp(object sender, MouseEventArgs e)
        {
            if (NeuralNetwork != null && CreaturesBrainOutput != null)
            {
                MovingNodeId = null;
            }
        }
        private void NeuralNetworkViewer_MouseMove(object sender, MouseEventArgs e)
        {
            Point relativeMouseLocation = new Point(e.X - Bounds.Location.X, e.Y - Bounds.Location.Y);

            if (MovingNodeId != null && NeuralNetwork != null && CreaturesBrainOutput != null)
            {
                GameObject movingNodeGameObject = DrawnNodeIdsToGameObject[MovingNodeId.Value];

                movingNodeGameObject.X = relativeMouseLocation.X - movingNodeGameObject.Size / 2;
                movingNodeGameObject.Y = relativeMouseLocation.Y - movingNodeGameObject.Size / 2;
            }
        }
        private void DrawTimer_Tick(object sender, EventArgs e)
        {
            if (Visible)
            {
                Invalidate();
            }
        }
        private void nudMaxNodeStaggerLevel_ValueChanged(object sender, EventArgs e)
        {
            MaxNodeStaggerLevel = (int)nudMaxNodeStaggerLevel.Value;
        }
        #endregion

        private void NeuralNetworkViewer_Load(object sender, EventArgs e)
        {
            MaxNodeStaggerLevel = 2;
        }
    }
}
