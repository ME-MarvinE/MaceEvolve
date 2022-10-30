using MaceEvolve.Enums;
using MaceEvolve.Extensions;
using MaceEvolve.Models;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using Rectangle = System.Drawing.Rectangle;

namespace MaceEvolve.Controls
{
    public partial class GameHost : UserControl
    {
        #region Fields
        protected static Random _Random = new Random();
        private int _TargetFPS = 10;
        private int _TargetTPS = 10;
        private Creature _SelectedCreature;
        private Creature _BestCreature;
        private NeuralNetworkViewer _BestCreatureNeuralNetworkViewer;
        private NeuralNetworkViewer _SelectedCreatureNeuralNetworkViewer;
        #endregion

        #region Properties
        public List<Creature> Creatures { get; set; } = new List<Creature>();
        public List<Food> Food { get; set; } = new List<Food>();
        public Stopwatch Stopwatch { get; set; } = new Stopwatch();
        public int MaxCreatureAmount { get; set; } = 150;
        public int MaxFoodAmount { get; set; } = 350;
        public int TargetFPS
        {
            get
            {
                return _TargetFPS;
            }
            set
            {
                _TargetFPS = value;
                DrawTimer.Interval = 1000 / TargetFPS;
            }
        }
        public int TargetTPS
        {
            get
            {
                return _TargetTPS;
            }
            set
            {
                _TargetTPS = value;
                GameTimer.Interval = 1000 / _TargetTPS;
            }
        }
        public Rectangle WorldBounds { get; set; }
        public Rectangle SuccessBounds { get; set; }
        public int MinCreatureConnections { get; set; } = 4;
        public int MaxCreatureConnections { get; set; } = 128;
        public double CreatureSpeed { get; set; }
        public double NewGenerationInterval { get; set; } = 36;
        public double SecondsUntilNewGeneration { get; set; } = 12;
        public int MaxCreatureProcessNodes { get; set; } = 4;
        public double MutationChance { get; set; } = 0.1;
        public double MutationAttempts { get; set; } = 10;
        public double ConnectionWeightBound { get; set; } = 4;
        public double MaxCreatureEnergy { get; set; } = 150;
        public double SuccessfulCreaturesPercentile { get; set; } = 10;
        public int GenerationCount { get; set; } = 1;
        public double ReproductionNodeBiasVariance = 0.05;
        public double ReproductionConnectionWeightVariance = 0.05;
        public ReadOnlyCollection<CreatureInput> PossibleCreatureInputs { get; } = Globals.AllCreatureInputs;
        public ReadOnlyCollection<CreatureAction> PossibleCreatureActions { get; } = Globals.AllCreatureActions;
        public bool UseSuccessBounds { get; set; } = true;
        public Creature SelectedCreature
        {
            get
            {
                return _SelectedCreature;
            }
            set
            {
                _SelectedCreature = value;
                _SelectedCreatureNeuralNetworkViewer = UpdateOrCreateNetworkViewer(_SelectedCreature?.Brain, _SelectedCreatureNeuralNetworkViewer);
            }
        }
        public Creature BestCreature
        {
            get
            {
                return _BestCreature;
            }
            set
            {
                _BestCreature = value;
                _BestCreatureNeuralNetworkViewer = UpdateOrCreateNetworkViewer(_BestCreature?.Brain, _BestCreatureNeuralNetworkViewer);
            }
        }
        public Color? SelectedCreaturePreviousColor { get; set; }
        public Color GenLabelTextColor
        {
            get
            {
                return lblGenerationCount.ForeColor;
            }
            set
            {
                lblGenerationCount.ForeColor = value;
            }
        }
        public NeuralNetworkViewer SelectedCreatureNeuralNetworkViewer
        {
            get
            {
                return UpdateOrCreateNetworkViewer(_SelectedCreature?.Brain, _SelectedCreatureNeuralNetworkViewer);
            }
        }
        public NeuralNetworkViewer BestCreatureNeuralNetworkViewer

        {
            get
            {
                return UpdateOrCreateNetworkViewer(_BestCreature?.Brain, _BestCreatureNeuralNetworkViewer);
            }
        }
        #endregion

        #region Constructors
        public GameHost()
        {
            InitializeComponent();
        }
        #endregion

        #region Methods
        public void Start()
        {
            GameTimer.Start();
            DrawTimer.Start();
            Stopwatch.Start();
            NewGenerationTimer.Start();
        }
        public void Stop()
        {
            GameTimer.Stop();
            DrawTimer.Stop();
            Stopwatch.Stop();
            NewGenerationTimer.Stop();
        }
        public void Reset()
        {
            WorldBounds = new Rectangle(Bounds.Location, Bounds.Size);

            Point MiddleOfWorldBounds = Globals.Middle(WorldBounds.X, WorldBounds.Y, WorldBounds.Width, WorldBounds.Height);
            //SuccessBounds = new Rectangle(WorldBounds.Location.X, WorldBounds.Location.Y, 150, 150);
            SuccessBounds = new Rectangle(MiddleOfWorldBounds.X - 75, MiddleOfWorldBounds.Y - 75, 150, 150);

            Stopwatch.Reset();
            SecondsUntilNewGeneration = NewGenerationInterval;
            Creatures.Clear();
            ResetFood();
            GenerationCount = 1;
            BestCreature = null;
            SelectedCreature = null;

            Creatures.AddRange(GenerateCreatures());

            lblGenerationCount.Text = $"Gen {GenerationCount}";
            Invalidate();
        }
        public List<Creature> NewGenerationSexual()
        {
            List<Creature> CreaturesList = new List<Creature>(Creatures);
            IEnumerable<Creature> SuccessfulCreatures = GetSuccessfulCreatures(CreaturesList);
            Dictionary<Creature, double> SuccessfulCreaturesFitnesses = GetFitnesses(SuccessfulCreatures);

            if (!SuccessfulCreaturesFitnesses.Any())
            {
                return new List<Creature>();
            }

            List<Creature> NewCreatures = new List<Creature>();

            Creature NewCreature = Creature.Reproduce(SuccessfulCreatures.ToList(), PossibleCreatureInputs.ToList(), PossibleCreatureActions.ToList(), ReproductionNodeBiasVariance, ReproductionConnectionWeightVariance, ConnectionWeightBound);
            NewCreature.GameHost = this;
            NewCreature.X = _Random.Next(WorldBounds.Left + WorldBounds.Width);
            NewCreature.Y = _Random.Next(WorldBounds.Top + WorldBounds.Height);
            NewCreature.Size = 10;
            NewCreature.Color = Color.FromArgb(255, 64, 64, _Random.Next(256));
            NewCreature.Speed = CreatureSpeed;
            NewCreature.Metabolism = 0.1;
            NewCreature.Energy = MaxCreatureEnergy;
            NewCreature.MaxEnergy = MaxCreatureEnergy;
            NewCreature.SightRange = 100;

            for (int i = 0; i < MutationAttempts; i++)
            {
                bool Mutated = MutateNetwork(NewCreature.Brain,
                    CreateRandomNodeChance: MutationChance * 0,
                    RemoveRandomNodeChance: MutationChance * 0,
                    MutateRandomNodeBiasChance: MutationChance * 2,
                    CreateRandomConnectionChance: MutationChance,
                    RemoveRandomConnectionChance: MutationChance,
                    MutateRandomConnectionSourceChance: MutationChance / 2,
                    MutateRandomConnectionTargetChance: MutationChance / 2,
                    MutateRandomConnectionWeightChance: MutationChance * 2);
            }

            NewCreatures.Add(NewCreature);

            return NewCreatures;
        }
        public List<Creature> NewGenerationAsexual()
        {
            List<Creature> CreaturesList = new List<Creature>(Creatures);
            IEnumerable<Creature> SuccessfulCreatures = GetSuccessfulCreatures(CreaturesList);
            Dictionary<Creature, double> SuccessfulCreaturesFitnesses = GetFitnesses(SuccessfulCreatures);

            if (!SuccessfulCreaturesFitnesses.Any())
            {
                return new List<Creature>();
            }

            List<Creature> NewCreatures = new List<Creature>();

            while (NewCreatures.Count < MaxCreatureAmount)
            {
                foreach (var CreatureFitnessPair in SuccessfulCreaturesFitnesses.OrderByDescending(x => x.Value))
                {
                    Creature SuccessfulCreature = CreatureFitnessPair.Key;

                    if (_Random.NextDouble() <= CreatureFitnessPair.Value && NewCreatures.Count < MaxCreatureAmount)
                    {
                        int NumberOfChildrenToCreate = UseSuccessBounds ? (int)Globals.Map(CreatureFitnessPair.Value, 0, 1, 0, MaxCreatureAmount / 10) : SuccessfulCreature.FoodEaten;

                        for (int i = 0; i < NumberOfChildrenToCreate; i++)
                        {
                            if (NewCreatures.Count < MaxCreatureAmount)
                            {
                                Creature NewCreature = Creature.Reproduce(new List<Creature>() { SuccessfulCreature }, PossibleCreatureInputs.ToList(), PossibleCreatureActions.ToList(), ReproductionNodeBiasVariance, ReproductionConnectionWeightVariance, ConnectionWeightBound);
                                NewCreature.GameHost = this;
                                NewCreature.X = _Random.Next(WorldBounds.Left + WorldBounds.Width);
                                NewCreature.Y = _Random.Next(WorldBounds.Top + WorldBounds.Height);
                                NewCreature.Size = 10;
                                NewCreature.Color = Color.FromArgb(255, 64, 64, _Random.Next(256));
                                NewCreature.Speed = CreatureSpeed;
                                NewCreature.Metabolism = 0.1;
                                NewCreature.Energy = MaxCreatureEnergy;
                                NewCreature.MaxEnergy = MaxCreatureEnergy;
                                NewCreature.SightRange = 100;

                                for (int j = 0; j < MutationAttempts; j++)
                                {
                                    bool Mutated = MutateNetwork(NewCreature.Brain,
                                        CreateRandomNodeChance: MutationChance * 2,
                                        RemoveRandomNodeChance: MutationChance * 0,
                                        MutateRandomNodeBiasChance: MutationChance * 2,
                                        CreateRandomConnectionChance: MutationChance,
                                        RemoveRandomConnectionChance: MutationChance,
                                        MutateRandomConnectionSourceChance: MutationChance / 2,
                                        MutateRandomConnectionTargetChance: MutationChance / 2,
                                        MutateRandomConnectionWeightChance: MutationChance * 2);
                                }

                                NewCreatures.Add(NewCreature);
                            }
                        }
                    }
                }
            }

            return NewCreatures;
        }
        public IEnumerable<Creature> GetSuccessfulCreatures(IEnumerable<Creature> Creatures)
        {
            if (Creatures == null) { throw new ArgumentNullException(); }

            if (!Creatures.Any())
            {
                return new List<Creature>();
            }

            if (UseSuccessBounds)
            {
                return Creatures.Where(x => x.X > SuccessBounds.Left && x.X < SuccessBounds.Right && x.Y > SuccessBounds.Top && x.Y < SuccessBounds.Bottom).ToList();
            }
            else
            {
                //return Creatures.Where(x => x.FoodEaten > 0).ToList();

                double IndexMultiplierForTopPercentile = (1 - (double)SuccessfulCreaturesPercentile / 100);
                int TopPercentileStartingIndex = (int)(Creatures.Count() * IndexMultiplierForTopPercentile) - 1;

                List<Creature> OrderedCreatures = Creatures.OrderBy(x => x.FoodEaten).ToList();
                return OrderedCreatures.SkipWhile(x => OrderedCreatures.IndexOf(x) < TopPercentileStartingIndex).Where(x => x.FoodEaten > 0);
            }
        }
        public Dictionary<Creature, double> GetFitnesses(IEnumerable<Creature> Creatures)
        {
            if (Creatures == null) { throw new ArgumentNullException(); }

            if (!Creatures.Any())
            {
                return new Dictionary<Creature, double>();
            }

            Dictionary<Creature, double> SuccessfulCreaturesFitnesses = new Dictionary<Creature, double>();

            if (UseSuccessBounds)
            {
                Point MiddleOfSuccessBounds = Globals.Middle(SuccessBounds.X, SuccessBounds.Y, SuccessBounds.Width, SuccessBounds.Height);

                foreach (var Creature in Creatures)
                {
                    double DistanceFromMiddle = Globals.GetDistanceFrom(Creature.MX, Creature.MY, MiddleOfSuccessBounds.X, MiddleOfSuccessBounds.Y);
                    double SuccessBoundsHypotenuse = Globals.Hypotenuse(SuccessBounds.Width, SuccessBounds.Height);

                    SuccessfulCreaturesFitnesses.Add(Creature, Globals.Map(DistanceFromMiddle, 0, SuccessBoundsHypotenuse, 1, 0)); ;
                }
            }
            else
            {
                int MostFoodEaten = Creatures.Max(x => x.FoodEaten);

                if (MostFoodEaten == 0)
                {
                    return new Dictionary<Creature, double>();
                }

                SuccessfulCreaturesFitnesses = Creatures.ToDictionary(
                    x => x,
                    x => (double)x.FoodEaten / MostFoodEaten);
            }

            return SuccessfulCreaturesFitnesses;
        }
        public bool MutateNetwork(NeuralNetwork Network, double CreateRandomNodeChance, double RemoveRandomNodeChance, double MutateRandomNodeBiasChance, double CreateRandomConnectionChance, double RemoveRandomConnectionChance, double MutateRandomConnectionSourceChance, double MutateRandomConnectionTargetChance, double MutateRandomConnectionWeightChance)
        {
            List<CreatureInput> PossibleCreatureInputsToAdd = GetPossibleInputsToAdd(Network).ToList();
            List<CreatureAction> PossibleCreatureActionsToAdd = GetPossibleActionsToAdd(Network).ToList();
            int ProcessNodeCount = Network.NodeIdsToNodesDict.Values.Where(x => x.NodeType == NodeType.Process).Count();
            bool MutationOccurred = false;

            //Things should be removed before being added so that there isn't a chance that the newly added thing is deleted straight after.
            //Connections should be added after nodes are added so that there is a chance the newly created node gets a connection.

            //Remove an existing node. Input nodes should not be removed.
            List<Node> PossibleNodesToRemove = NeuralNetwork.GetPossibleTargetNodes(Network.NodeIdsToNodesDict.Values).ToList();
            if (PossibleNodesToRemove.Count > 0 && _Random.NextDouble() <= RemoveRandomNodeChance)
            {
                Node NodeToRemove = PossibleNodesToRemove[_Random.Next(PossibleNodesToRemove.Count)];
                int NodeIdToRemove = Network.NodesToNodeIdsDict[NodeToRemove];

                Network.RemoveNode(NodeIdToRemove, true);
                MutationOccurred = true;
            }

            //Change a random node's bias.
            if (_Random.NextDouble() <= MutateRandomNodeBiasChance)
            {
                List<Node> NodesList = Network.NodeIdsToNodesDict.Values.ToList();
                Node RandomNode = NodesList[_Random.Next(NodesList.Count)];
                RandomNode.Bias = _Random.NextDouble(-1, 1);

                MutationOccurred = true;
            }

            //Create a new node with a default connection.
            if (_Random.NextDouble() <= CreateRandomNodeChance)
            {
                Node NodeToAdd;
                double NodeTypeRandomNum = _Random.NextDouble();
                double ChanceForSingleNodeType = CreateRandomNodeChance / 3;

                if (NodeTypeRandomNum <= ChanceForSingleNodeType && PossibleCreatureInputsToAdd.Count > 0)
                {
                    NodeToAdd = new Node(NodeType.Input, _Random.NextDouble(-1, 1), PossibleCreatureInputsToAdd[_Random.Next(PossibleCreatureInputsToAdd.Count)]);
                }
                else if ((NodeTypeRandomNum <= ChanceForSingleNodeType * 2 || PossibleCreatureInputsToAdd.Count == 0) && PossibleCreatureActionsToAdd.Count > 0)
                {
                    NodeToAdd = new Node(NodeType.Output, _Random.NextDouble(-1, 1), CreatureAction: PossibleCreatureActionsToAdd[_Random.Next(PossibleCreatureActionsToAdd.Count)]);
                }
                else if (ProcessNodeCount < MaxCreatureProcessNodes)
                {
                    NodeToAdd = new Node(NodeType.Process, _Random.NextDouble(-1, 1));
                }
                else
                {
                    NodeToAdd = null;
                }

                if (NodeToAdd != null)
                {
                    List<Node> PossibleSourceNodes = NeuralNetwork.GetPossibleSourceNodes(Network.NodeIdsToNodesDict.Values).ToList();
                    List<Node> PossibleTargetNodes = NeuralNetwork.GetPossibleTargetNodes(Network.NodeIdsToNodesDict.Values).ToList();

                    Network.AddNode(NodeToAdd);
                    int NodeToAddId = Network.NodesToNodeIdsDict[NodeToAdd];

                    if (Network.Connections.Count < MaxCreatureConnections && PossibleSourceNodes.Count > 0 && PossibleTargetNodes.Count > 0)
                    {
                        Connection NewConnection = new Connection() { Weight = _Random.NextDouble(-ConnectionWeightBound, ConnectionWeightBound) };

                        switch (NodeToAdd.NodeType)
                        {
                            case NodeType.Input:
                                NewConnection.SourceId = NodeToAddId;
                                NewConnection.TargetId = Network.NodesToNodeIdsDict[PossibleTargetNodes[_Random.Next(PossibleTargetNodes.Count)]];
                                break;

                            case NodeType.Process:
                                if (_Random.NextDouble() <= 0.5)
                                {
                                    NewConnection.SourceId = NodeToAddId;
                                    NewConnection.TargetId = Network.NodesToNodeIdsDict[PossibleTargetNodes[_Random.Next(PossibleTargetNodes.Count)]];
                                }
                                else
                                {
                                    NewConnection.SourceId = Network.NodesToNodeIdsDict[PossibleSourceNodes[_Random.Next(PossibleSourceNodes.Count)]];
                                    NewConnection.TargetId = NodeToAddId;
                                }
                                break;

                            case NodeType.Output:
                                NewConnection.SourceId = Network.NodesToNodeIdsDict[PossibleSourceNodes[_Random.Next(PossibleSourceNodes.Count)]];
                                NewConnection.TargetId = NodeToAddId;
                                break;

                            default:
                                throw new NotImplementedException();
                        }

                        Network.Connections.Add(NewConnection);
                    }

                    MutationOccurred = true;
                }
            }

            //Remove a random connection.
            if (Network.Connections.Count > MinCreatureConnections && _Random.NextDouble() <= RemoveRandomConnectionChance)
            {
                Connection RandomConnection = Network.Connections[_Random.Next(Network.Connections.Count)];
                Network.Connections.Remove(RandomConnection);
            }

            //Change a random connection's weight.
            if (Network.Connections.Count > 0 && _Random.NextDouble() <= MutateRandomConnectionWeightChance)
            {
                Connection RandomConnection = Network.Connections[_Random.Next(Network.Connections.Count)];

                RandomConnection.Weight = _Random.NextDouble(-ConnectionWeightBound, ConnectionWeightBound);

                MutationOccurred = true;
            }

            //Change a random connection's source.
            if (Network.Connections.Count > 0 && Network.MutateConnectionSource(MutateRandomConnectionSourceChance, Network.Connections[_Random.Next(Network.Connections.Count)]))
            {
                MutationOccurred = true;
            }

            //Change a random connection's target.
            if (Network.Connections.Count > 0 && Network.MutateConnectionTarget(MutateRandomConnectionTargetChance, Network.Connections[_Random.Next(Network.Connections.Count)]))
            {
                MutationOccurred = true;
            }

            //Create a new connection.
            if (Network.Connections.Count < MaxCreatureConnections && _Random.NextDouble() <= CreateRandomConnectionChance)
            {
                Connection NewConnection = Network.GenerateRandomConnections(1, 1, ConnectionWeightBound).FirstOrDefault();

                if (NewConnection != null)
                {
                    Network.Connections.Add(NewConnection);
                    MutationOccurred = true;
                }
            }

            return MutationOccurred;
        }
        public IEnumerable<CreatureInput> GetPossibleInputsToAdd(NeuralNetwork Network)
        {
            //Return any inputs that aren't already used by a node in the network.
            return PossibleCreatureInputs.Where(x => !Network.NodeIdsToNodesDict.Any(y => y.Value.NodeType == NodeType.Input && x == y.Value.CreatureInput));
        }
        public IEnumerable<CreatureAction> GetPossibleActionsToAdd(NeuralNetwork Network)
        {
            //Return any actions that aren't already used by a node in the network.
            return PossibleCreatureActions.Where(x => !Network.NodeIdsToNodesDict.Any(y => y.Value.NodeType == NodeType.Output && x == y.Value.CreatureAction));
        }
        private void GameTimer_Tick(object sender, EventArgs e)
        {
            List<Food> FoodList = new List<Food>(Food);
            List<Creature> CreaturesList = new List<Creature>(Creatures);

            Food.RemoveAll(x => x.Servings <= 0);

            foreach (Food Food in FoodList)
            {
                Food.Update();
            }

            Creature NewBestCreature = null;

            Point MiddleOfSuccessBounds = Globals.Middle(SuccessBounds.X, SuccessBounds.Y, SuccessBounds.Width, SuccessBounds.Height);

            foreach (Creature Creature in CreaturesList)
            {
                Creature.Update();

                if (UseSuccessBounds)
                {
                    double DistanceFromMiddle = Globals.GetDistanceFrom(Creature.MX, Creature.MY, MiddleOfSuccessBounds.X, MiddleOfSuccessBounds.Y);
                    double? NewBestCreatureDistanceFromMiddle = NewBestCreature == null ? null : Globals.GetDistanceFrom(NewBestCreature.MX, NewBestCreature.MY, MiddleOfSuccessBounds.X, MiddleOfSuccessBounds.Y);

                    if (NewBestCreatureDistanceFromMiddle == null || DistanceFromMiddle < NewBestCreatureDistanceFromMiddle)
                    {
                        NewBestCreature = Creature;
                    }
                }
                else
                {
                    if (NewBestCreature == null || Creature.FoodEaten > NewBestCreature.FoodEaten)
                    {
                        NewBestCreature = Creature;
                    }
                }
            }

            if (_Random.NextDouble() <= 0.8)
            {
                if (FoodList.Count < MaxFoodAmount)
                {
                    Food.Add(new Apple()
                    {
                        GameHost = this,
                        X = _Random.Next(WorldBounds.Left + WorldBounds.Width),
                        Y = _Random.Next(WorldBounds.Top + WorldBounds.Height),
                        Servings = 1,
                        EnergyPerServing = 30,
                        ServingDigestionCost = 0.05,
                        Size = 7,
                        Color = Color.Green
                    });
                }
            }

            lblGenerationCount.Text = $"Gen {GenerationCount}";

            if (NewBestCreature != null && BestCreature != NewBestCreature)
            {
                BestCreature = NewBestCreature;
            }
        }
        private void GameHost_Paint(object sender, PaintEventArgs e)
        {
            List<Food> FoodList = new List<Food>(Food);
            List<Creature> CreaturesList = new List<Creature>(Creatures);
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

            foreach (Creature Creature in CreaturesList)
            {
                Creature.Draw(e);
            }

            foreach (Food Food in FoodList)
            {
                Food.Draw(e);
            }

            e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(100, Color.Green)), SuccessBounds);
        }
        private void GameHost_Load(object sender, EventArgs e)
        {
            DoubleBuffered = true;
            TargetTPS = 60;
            TargetFPS = TargetTPS;
            CreatureSpeed = UseSuccessBounds ? 3.5 : 2.75;

            Reset();
        }
        private void DrawTimer_Tick(object sender, EventArgs e)
        {
            Invalidate();
        }
        public void NewGenerationTimer_Tick(object sender, EventArgs e)
        {
            if (SecondsUntilNewGeneration <= 0)
            {
                SecondsUntilNewGeneration = NewGenerationInterval;
                Creatures = NewGenerationAsexual();

                if (Creatures.Count > 0)
                {
                    ResetFood();
                    SelectedCreature = null;
                    GenerationCount += 1;
                }
                else
                {
                    Rectangle OldWorldBounds = new Rectangle(WorldBounds.Location, WorldBounds.Size);
                    Reset();
                    WorldBounds = OldWorldBounds;
                }
            }
            else
            {
                SecondsUntilNewGeneration -= 0.1;
            }
        }
        public void ResetFood()
        {
            Food.Clear();

            for (int i = 0; i < MaxFoodAmount; i++)
            {
                Food.Add(new Apple()
                {
                    GameHost = this,
                    X = _Random.Next(WorldBounds.Left + WorldBounds.Width),
                    Y = _Random.Next(WorldBounds.Top + WorldBounds.Height),
                    Servings = 1,
                    EnergyPerServing = 30,
                    ServingDigestionCost = 0.05,
                    Size = 7,
                    Color = Color.Green
                });
            }
        }
        public List<Creature> GenerateCreatures()
        {
            List<Creature> Creatures = new List<Creature>();

            for (int i = 0; i < MaxCreatureAmount; i++)
            {
                Creature NewCreature = new Creature()
                {
                    Brain = new NeuralNetwork(PossibleCreatureInputs.ToList(), MaxCreatureProcessNodes, PossibleCreatureActions.ToList()),
                    GameHost = this,
                    X = _Random.Next(WorldBounds.Left + WorldBounds.Width),
                    Y = _Random.Next(WorldBounds.Top + WorldBounds.Height),
                    Size = 10,
                    Color = Color.FromArgb(255, 64, 64, _Random.Next(256)),
                    Speed = CreatureSpeed,
                    Metabolism = 0.1,
                    Energy = MaxCreatureEnergy,
                    MaxEnergy = MaxCreatureEnergy,
                    SightRange = 100
                };

                NewCreature.Brain.Connections = NewCreature.Brain.GenerateRandomConnections(MinCreatureConnections, MaxCreatureConnections, ConnectionWeightBound);
                Creatures.Add(NewCreature);
            }

            return Creatures;
        }
        private void GameHost_MouseClick(object sender, MouseEventArgs e)
        {
            Point RelativeMouseLocation = new Point(e.X - Bounds.Location.X, e.Y - Bounds.Location.Y);
            IEnumerable<Creature> CreaturesOrderedByDistanceToMouse = Creatures.OrderBy(x => Globals.GetDistanceFrom(RelativeMouseLocation.X, RelativeMouseLocation.Y, x.MX, x.MY));

            Creature OldSelectedCreature = SelectedCreature;
            Creature NewSelectedCreature = CreaturesOrderedByDistanceToMouse.FirstOrDefault();

            if (SelectedCreature == null)
            {
                if (NewSelectedCreature != null)
                {
                    SelectedCreature = NewSelectedCreature;
                    SelectedCreaturePreviousColor = NewSelectedCreature.Color;
                }
            }
            else
            {
                SelectedCreature.Color = SelectedCreaturePreviousColor.Value;
                SelectedCreature = NewSelectedCreature;
                SelectedCreaturePreviousColor = NewSelectedCreature?.Color;

                if (NewSelectedCreature != null)
                {
                    NewSelectedCreature.Color = Color.White;
                }
            }

            if (NewSelectedCreature != null && OldSelectedCreature != NewSelectedCreature)
            {
                NetworkViewerForm NetworkViewerForm = new NetworkViewerForm(SelectedCreatureNeuralNetworkViewer);
                NetworkViewerForm.Show();
            }

            Invalidate();
        }
        public static void ChangeNetworkViewerNetwork(NeuralNetworkViewer NetworkViewer, NeuralNetwork NewNeuralNetwork)
        {
            NetworkViewer.NeuralNetwork = NewNeuralNetwork;
            NetworkViewer.ResetDrawnNodes();
        }
        public NeuralNetworkViewer UpdateOrCreateNetworkViewer(NeuralNetwork NeuralNetwork, NeuralNetworkViewer NetworkViewer = null)
        {
            NeuralNetworkViewer ReturnedNetworkViewer = NetworkViewer;

            bool CreateNewNetworkViewer = ReturnedNetworkViewer == null || ReturnedNetworkViewer.IsDisposed;

            if (CreateNewNetworkViewer)
            {
                ReturnedNetworkViewer = new NeuralNetworkViewer();
                ReturnedNetworkViewer.Dock = DockStyle.Fill;
                ReturnedNetworkViewer.BackColor = BackColor;
                ReturnedNetworkViewer.lblNetworkConnectionsCount.ForeColor = Color.White;
                ReturnedNetworkViewer.lblNetworkNodesCount.ForeColor = Color.White;
                ReturnedNetworkViewer.lblSelectedNodeId.ForeColor = Color.White;
                ReturnedNetworkViewer.lblSelectedNodePreviousOutput.ForeColor = Color.White;
                ReturnedNetworkViewer.lblSelectedNodeConnectionCount.ForeColor = Color.White;
                ReturnedNetworkViewer.DrawTimer.Interval = 1000 / TargetFPS;
            }

            ReturnedNetworkViewer.NeuralNetwork = NeuralNetwork;

            return ReturnedNetworkViewer;
        }
        #endregion
    }
}
