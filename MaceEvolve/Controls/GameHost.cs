using MaceEvolve.Core;
using MaceEvolve.Core.Enums;
using MaceEvolve.Core.Extensions;
using MaceEvolve.Core.Models;
using MaceEvolve.Models;
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
        protected static Random random = new Random();
        private int _targetFPS = 10;
        private int _targetTPS = 10;
        private GraphicalCreature _selectedCreature;
        private GraphicalCreature _bestCreature;
        private NeuralNetworkViewer _bestCreatureNeuralNetworkViewer;
        private NeuralNetworkViewer _selectedCreatureNeuralNetworkViewer;
        private double _secondsUntilNewGeneration;
        private int _generationCount = 1;
        #endregion

        #region Properties
        public List<GraphicalCreature> Creatures { get; set; } = new List<GraphicalCreature>();
        public List<GraphicalFood> Food { get; set; } = new List<GraphicalFood>();
        public Stopwatch Stopwatch { get; set; } = new Stopwatch();
        public int MaxCreatureAmount { get; set; } = 150;
        public int MaxFoodAmount { get; set; } = 350;
        public int TargetFPS
        {
            get
            {
                return _targetFPS;
            }
            set
            {
                _targetFPS = value;
                DrawTimer.Interval = 1000 / TargetFPS;
            }
        }
        public int TargetTPS
        {
            get
            {
                return _targetTPS;
            }
            set
            {
                _targetTPS = value;
                GameTimer.Interval = 1000 / _targetTPS;
            }
        }
        public Rectangle WorldBounds { get; set; }
        public Rectangle SuccessBounds { get; set; }
        public int MinCreatureConnections { get; set; } = 4;
        public int MaxCreatureConnections { get; set; } = 128;
        public double CreatureSpeed { get; set; }
        public double NewGenerationInterval { get; set; } = 36;
        public double SecondsUntilNewGeneration
        {
            get
            {
                return _secondsUntilNewGeneration;
            }
            set
            {
                _secondsUntilNewGeneration = value;
                lblGenEndsIn.Text = $"Ends in {string.Format("{0:0}", SecondsUntilNewGeneration)}s";
            }
        }
        public int MaxCreatureProcessNodes { get; set; } = 4;
        public double MutationChance { get; set; } = 0.3;
        public double MutationAttempts { get; set; } = 2;
        public double ConnectionWeightBound { get; set; } = 4;
        public double MaxCreatureEnergy { get; set; } = 150;
        public double SuccessfulCreaturesPercentile { get; set; } = 10;
        public int GenerationCount
        {
            get
            {
                return _generationCount;
            }
            set
            {
                _generationCount = value;
                lblGenerationCount.Text = $"Gen {GenerationCount}";
            }
        }
        public double ReproductionNodeBiasVariance = 0.05;
        public double ReproductionConnectionWeightVariance = 0.05;
        public ReadOnlyCollection<CreatureInput> PossibleCreatureInputs { get; } = Globals.AllCreatureInputs;
        public ReadOnlyCollection<CreatureAction> PossibleCreatureActions { get; } = Globals.AllCreatureActions;
        public bool UseSuccessBounds { get; set; }
        public GraphicalCreature SelectedCreature
        {
            get
            {
                return _selectedCreature;
            }
            set
            {
                _selectedCreature = value;
                _selectedCreatureNeuralNetworkViewer = UpdateOrCreateNetworkViewer(_selectedCreature?.Brain, _selectedCreatureNeuralNetworkViewer);
            }
        }
        public GraphicalCreature BestCreature
        {
            get
            {
                return _bestCreature;
            }
            set
            {
                _bestCreature = value;
                _bestCreatureNeuralNetworkViewer = UpdateOrCreateNetworkViewer(_bestCreature?.Brain, _bestCreatureNeuralNetworkViewer);
            }
        }
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
        public Color GenEndsInLabelTextColor
        {
            get
            {
                return lblGenEndsIn.ForeColor;
            }
            set
            {
                lblGenEndsIn.ForeColor = value;
            }
        }
        public NeuralNetworkViewer SelectedCreatureNeuralNetworkViewer
        {
            get
            {
                return UpdateOrCreateNetworkViewer(_selectedCreature?.Brain, _selectedCreatureNeuralNetworkViewer);
            }
        }
        public NeuralNetworkViewer BestCreatureNeuralNetworkViewer

        {
            get
            {
                return UpdateOrCreateNetworkViewer(_bestCreature?.Brain, _bestCreatureNeuralNetworkViewer);
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

            Point middleOfWorldBounds = Middle(WorldBounds.X, WorldBounds.Y, WorldBounds.Width, WorldBounds.Height);
            //SuccessBounds = new Rectangle(WorldBounds.Location.X, WorldBounds.Location.Y, 150, 150);
            SuccessBounds = new Rectangle(middleOfWorldBounds.X - 75, middleOfWorldBounds.Y - 75, 150, 150);

            Stopwatch.Reset();
            SecondsUntilNewGeneration = NewGenerationInterval;
            Creatures.Clear();
            ResetFood();
            GenerationCount = 1;
            BestCreature = null;
            SelectedCreature = null;

            Creatures.AddRange(GenerateCreatures());

            Invalidate();
        }
        public List<GraphicalCreature> NewGenerationSexual()
        {
            List<GraphicalCreature> creaturesList = new List<GraphicalCreature>(Creatures);
            IEnumerable<GraphicalCreature> successfulCreatures = GetSuccessfulCreatures(creaturesList);
            Dictionary<GraphicalCreature, double> successfulCreaturesFitnesses = GetFitnesses(successfulCreatures);

            if (!successfulCreaturesFitnesses.Any())
            {
                return new List<GraphicalCreature>();
            }

            List<GraphicalCreature> newCreatures = new List<GraphicalCreature>();

            GraphicalCreature newCreature = Creature.Reproduce(successfulCreatures.ToList(), PossibleCreatureInputs.ToList(), PossibleCreatureActions.ToList(), ReproductionNodeBiasVariance, ReproductionConnectionWeightVariance, ConnectionWeightBound);
            newCreature.X = random.Next(WorldBounds.Left + WorldBounds.Width);
            newCreature.Y = random.Next(WorldBounds.Top + WorldBounds.Height);
            newCreature.Size = 10;
            newCreature.Color = Color.FromArgb(255, 64, 64, random.Next(256));
            newCreature.Speed = CreatureSpeed;
            newCreature.Metabolism = 0.1;
            newCreature.Energy = MaxCreatureEnergy;
            newCreature.MaxEnergy = MaxCreatureEnergy;
            newCreature.SightRange = 100;

            for (int i = 0; i < MutationAttempts; i++)
            {
                bool mutated = MutateNetwork(newCreature.Brain,
                    createRandomNodeChance: MutationChance * 0,
                    removeRandomNodeChance: MutationChance * 0,
                    mutateRandomNodeBiasChance: MutationChance * 2,
                    createRandomConnectionChance: MutationChance,
                    removeRandomConnectionChance: MutationChance,
                    mutateRandomConnectionSourceChance: MutationChance / 2,
                    mutateRandomConnectionTargetChance: MutationChance / 2,
                    mutateRandomConnectionWeightChance: MutationChance * 2);
            }

            newCreatures.Add(newCreature);

            return newCreatures;
        }
        public List<GraphicalCreature> NewGenerationAsexual()
        {
            List<GraphicalCreature> creaturesList = new List<GraphicalCreature>(Creatures);
            IEnumerable<GraphicalCreature> successfulCreatures = GetSuccessfulCreatures(creaturesList);
            Dictionary<GraphicalCreature, double> successfulCreaturesFitnesses = GetFitnesses(successfulCreatures);

            if (!successfulCreaturesFitnesses.Any())
            {
                return new List<GraphicalCreature>();
            }

            List<GraphicalCreature> newCreatures = new List<GraphicalCreature>();

            while (newCreatures.Count < MaxCreatureAmount)
            {
                foreach (var creatureFitnessPair in successfulCreaturesFitnesses.OrderByDescending(x => x.Value))
                {
                    GraphicalCreature successfulCreature = creatureFitnessPair.Key;

                    if (random.NextDouble() <= creatureFitnessPair.Value && newCreatures.Count < MaxCreatureAmount)
                    {
                        int numberOfChildrenToCreate = UseSuccessBounds ? (int)Globals.Map(creatureFitnessPair.Value, 0, 1, 0, MaxCreatureAmount / 10) : successfulCreature.FoodEaten;

                        for (int i = 0; i < numberOfChildrenToCreate; i++)
                        {
                            if (newCreatures.Count < MaxCreatureAmount)
                            {
                                GraphicalCreature newCreature = Creature.Reproduce(new List<GraphicalCreature>() { successfulCreature }, PossibleCreatureInputs.ToList(), PossibleCreatureActions.ToList(), ReproductionNodeBiasVariance, ReproductionConnectionWeightVariance, ConnectionWeightBound);
                                newCreature.X = random.Next(WorldBounds.Left + WorldBounds.Width);
                                newCreature.Y = random.Next(WorldBounds.Top + WorldBounds.Height);
                                newCreature.Size = 10;
                                newCreature.Color = Color.FromArgb(255, 64, 64, random.Next(256));
                                newCreature.Speed = CreatureSpeed;
                                newCreature.Metabolism = 0.1;
                                newCreature.Energy = MaxCreatureEnergy;
                                newCreature.MaxEnergy = MaxCreatureEnergy;
                                newCreature.SightRange = 100;

                                for (int j = 0; j < MutationAttempts; j++)
                                {
                                    bool Mutated = MutateNetwork(newCreature.Brain,
                                        createRandomNodeChance: MutationChance,
                                        removeRandomNodeChance: MutationChance,
                                        mutateRandomNodeBiasChance: MutationChance,
                                        createRandomConnectionChance: MutationChance,
                                        removeRandomConnectionChance: MutationChance,
                                        mutateRandomConnectionSourceChance: MutationChance,
                                        mutateRandomConnectionTargetChance: MutationChance,
                                        mutateRandomConnectionWeightChance: MutationChance);
                                }

                                newCreatures.Add(newCreature);
                            }
                        }
                    }
                }
            }

            return newCreatures;
        }
        public IEnumerable<GraphicalCreature> GetSuccessfulCreatures(IEnumerable<GraphicalCreature> creatures)
        {
            if (creatures == null) { throw new ArgumentNullException(); }

            if (!creatures.Any())
            {
                return new List<GraphicalCreature>();
            }

            if (UseSuccessBounds)
            {
                return creatures.Where(x => x.X > SuccessBounds.Left && x.X < SuccessBounds.Right && x.Y > SuccessBounds.Top && x.Y < SuccessBounds.Bottom).ToList();
            }
            else
            {
                //return creatures.Where(x => x.FoodEaten > 0).ToList();

                double indexMultiplierForTopPercentile = (1 - (double)SuccessfulCreaturesPercentile / 100);
                int topPercentileStartingIndex = (int)(creatures.Count() * indexMultiplierForTopPercentile) - 1;

                List<GraphicalCreature> orderedCreatures = creatures.OrderBy(x => x.FoodEaten).ToList();
                return orderedCreatures.SkipWhile(x => orderedCreatures.IndexOf(x) < topPercentileStartingIndex).Where(x => x.FoodEaten > 0);
            }
        }
        public Dictionary<GraphicalCreature, double> GetFitnesses(IEnumerable<GraphicalCreature> creatures)
        {
            if (creatures == null) { throw new ArgumentNullException(); }

            if (!creatures.Any())
            {
                return new Dictionary<GraphicalCreature, double>();
            }

            Dictionary<GraphicalCreature, double> successfulCreaturesFitnesses = new Dictionary<GraphicalCreature, double>();

            if (UseSuccessBounds)
            {
                Point middleOfSuccessBounds = Middle(SuccessBounds.X, SuccessBounds.Y, SuccessBounds.Width, SuccessBounds.Height);

                foreach (var creature in creatures)
                {
                    double distanceFromMiddle = Globals.GetDistanceFrom(creature.MX, creature.MY, middleOfSuccessBounds.X, middleOfSuccessBounds.Y);
                    double successBoundsHypotenuse = Globals.Hypotenuse(SuccessBounds.Width, SuccessBounds.Height);

                    successfulCreaturesFitnesses.Add(creature, Globals.Map(distanceFromMiddle, 0, successBoundsHypotenuse, 1, 0)); ;
                }
            }
            else
            {
                int mostFoodEaten = creatures.Max(x => x.FoodEaten);

                if (mostFoodEaten == 0)
                {
                    return new Dictionary<GraphicalCreature, double>();
                }

                successfulCreaturesFitnesses = creatures.ToDictionary(
                    x => x,
                    x => (double)x.FoodEaten / mostFoodEaten);
            }

            return successfulCreaturesFitnesses;
        }
        public bool MutateNetwork(NeuralNetwork network, double createRandomNodeChance, double removeRandomNodeChance, double mutateRandomNodeBiasChance, double createRandomConnectionChance, double removeRandomConnectionChance, double mutateRandomConnectionSourceChance, double mutateRandomConnectionTargetChance, double mutateRandomConnectionWeightChance)
        {
            int processNodeCount = network.NodeIdsToNodesDict.Values.Where(x => x.NodeType == NodeType.Process).Count();
            bool mutationAttempted = false;

            //Things should be removed before being added so that there isn't a chance that the newly added thing is deleted straight after.
            //Connections should be added after nodes are added so that there is a chance the newly created node gets a connection.

            //Remove an existing node. Input nodes should not be removed. 
            if (random.NextDouble() <= removeRandomNodeChance)
            {
                mutationAttempted = true;

                IEnumerable<Node> processNodes = network.NodeIdsToNodesDict.Values.Where(x => x.NodeType == NodeType.Process);
                IEnumerable<Node> outputNodes = network.NodeIdsToNodesDict.Values.Where(x => x.NodeType == NodeType.Output);

                List<Node> possibleNodesToRemove = new List<Node>(processNodes);

                //There must be at least one target node in a network.
                if (outputNodes.ElementAtOrDefault(1) != null)
                {
                    possibleNodesToRemove.AddRange(outputNodes);
                }

                if (possibleNodesToRemove.Count > 0)
                {
                    Node nodeToRemove = possibleNodesToRemove[random.Next(possibleNodesToRemove.Count)];
                    int nodeIdToRemove = network.NodesToNodeIdsDict[nodeToRemove];

                    network.RemoveNode(nodeIdToRemove, true);
                }
            }

            //Change a random node's bias.
            if (random.NextDouble() <= mutateRandomNodeBiasChance)
            {
                mutationAttempted = true;

                List<Node> nodesList = network.NodeIdsToNodesDict.Values.ToList();
                Node randomNode = nodesList[random.Next(nodesList.Count)];
                randomNode.Bias = random.NextDouble(-1, 1);
            }

            //Create a new node with a default connection.
            if (random.NextDouble() <= createRandomNodeChance)
            {
                List<CreatureInput> possibleCreatureInputsToAdd = GetPossibleInputsToAdd(network).ToList();
                List<CreatureAction> possibleCreatureActionsToAdd = GetPossibleActionsToAdd(network).ToList();

                mutationAttempted = true;

                Node nodeToAdd;
                double nodeTypeRandomNum = random.NextDouble();
                double chanceForSingleNodeType = 1d / Globals.AllNodeTypes.Count;

                if (nodeTypeRandomNum <= chanceForSingleNodeType && possibleCreatureInputsToAdd.Count > 0)
                {
                    nodeToAdd = new Node(NodeType.Input, random.NextDouble(-1, 1), possibleCreatureInputsToAdd[random.Next(possibleCreatureInputsToAdd.Count)]);
                }
                else if (nodeTypeRandomNum <= chanceForSingleNodeType * 2 && possibleCreatureActionsToAdd.Count > 0)
                {
                    nodeToAdd = new Node(NodeType.Output, random.NextDouble(-1, 1), creatureAction: possibleCreatureActionsToAdd[random.Next(possibleCreatureActionsToAdd.Count)]);
                }
                else if (processNodeCount < MaxCreatureProcessNodes)
                {
                    nodeToAdd = new Node(NodeType.Process, random.NextDouble(-1, 1));
                }
                else
                {
                    nodeToAdd = null;
                }

                if (nodeToAdd != null)
                {
                    List<Node> possibleSourceNodes = NeuralNetwork.GetSourceNodes(network.NodeIdsToNodesDict.Values).ToList();
                    List<Node> possibleTargetNodes = NeuralNetwork.GetTargetNodes(network.NodeIdsToNodesDict.Values).ToList();

                    network.AddNode(nodeToAdd);
                    int nodeToAddId = network.NodesToNodeIdsDict[nodeToAdd];

                    if (network.Connections.Count < MaxCreatureConnections && possibleSourceNodes.Count > 0 && possibleTargetNodes.Count > 0)
                    {
                        Connection newConnection = new Connection() { Weight = random.NextDouble(-ConnectionWeightBound, ConnectionWeightBound) };

                        switch (nodeToAdd.NodeType)
                        {
                            case NodeType.Input:
                                newConnection.SourceId = nodeToAddId;
                                newConnection.TargetId = network.NodesToNodeIdsDict[possibleTargetNodes[random.Next(possibleTargetNodes.Count)]];
                                break;

                            case NodeType.Process:
                                if (random.NextDouble() <= 0.5)
                                {
                                    newConnection.SourceId = nodeToAddId;
                                    newConnection.TargetId = network.NodesToNodeIdsDict[possibleTargetNodes[random.Next(possibleTargetNodes.Count)]];
                                }
                                else
                                {
                                    newConnection.SourceId = network.NodesToNodeIdsDict[possibleSourceNodes[random.Next(possibleSourceNodes.Count)]];
                                    newConnection.TargetId = nodeToAddId;
                                }
                                break;

                            case NodeType.Output:
                                newConnection.SourceId = network.NodesToNodeIdsDict[possibleSourceNodes[random.Next(possibleSourceNodes.Count)]];
                                newConnection.TargetId = nodeToAddId;
                                break;

                            default:
                                throw new NotImplementedException();
                        }

                        network.Connections.Add(newConnection);
                    }
                }
            }

            //Remove a random connection.
            if (network.Connections.Count > MinCreatureConnections && random.NextDouble() <= removeRandomConnectionChance)
            {
                mutationAttempted = true;

                Connection randomConnection = network.Connections[random.Next(network.Connections.Count)];
                network.Connections.Remove(randomConnection);
            }

            //Change a random connection's weight.
            if (network.Connections.Count > 0 && random.NextDouble() <= mutateRandomConnectionWeightChance)
            {
                mutationAttempted = true;

                Connection randomConnection = network.Connections[random.Next(network.Connections.Count)];

                randomConnection.Weight = random.NextDouble(-ConnectionWeightBound, ConnectionWeightBound);
            }

            //Change a random connection's source.
            if (network.Connections.Count > 0 && network.MutateConnectionSource(mutateRandomConnectionSourceChance, network.Connections[random.Next(network.Connections.Count)]))
            {
                mutationAttempted = true;
            }

            //Change a random connection's target.
            if (network.Connections.Count > 0 && network.MutateConnectionTarget(mutateRandomConnectionTargetChance, network.Connections[random.Next(network.Connections.Count)]))
            {
                mutationAttempted = true;
            }

            //Create a new connection.
            if (network.Connections.Count < MaxCreatureConnections && random.NextDouble() <= createRandomConnectionChance)
            {
                mutationAttempted = true;
                Connection newConnection = network.GenerateRandomConnections(1, 1, ConnectionWeightBound).FirstOrDefault();

                if (newConnection != null)
                {
                    network.Connections.Add(newConnection);
                }
            }

            return mutationAttempted;
        }
        public IEnumerable<CreatureInput> GetPossibleInputsToAdd(NeuralNetwork network)
        {
            //Return any inputs that aren't already used by a node in the network.
            return PossibleCreatureInputs.Where(x => !network.NodeIdsToNodesDict.Any(y => y.Value.NodeType == NodeType.Input && x == y.Value.CreatureInput));
        }
        public IEnumerable<CreatureAction> GetPossibleActionsToAdd(NeuralNetwork network)
        {
            //Return any actions that aren't already used by a node in the network.
            return PossibleCreatureActions.Where(x => !network.NodeIdsToNodesDict.Any(y => y.Value.NodeType == NodeType.Output && x == y.Value.CreatureAction));
        }
        private void GameTimer_Tick(object sender, EventArgs e)
        {
            GraphicalCreature newBestCreature = null;
            Point middleOfSuccessBounds = Middle(SuccessBounds.X, SuccessBounds.Y, SuccessBounds.Width, SuccessBounds.Height);

            Food.RemoveAll(x => x.Servings <= 0);

            foreach (GraphicalCreature creature in Creatures)
            {
                if (!creature.IsDead)
                {
                    EnvironmentInfo environmentInfo = new EnvironmentInfo(Creatures.AsReadOnly(), Food.AsReadOnly(), new Core.Models.Rectangle(WorldBounds.X, WorldBounds.Y, WorldBounds.Width, WorldBounds.Height));

                    creature.Live(environmentInfo);

                    if (creature.Energy <= 0)
                    {
                        creature.Die();
                    }

                    Food.RemoveAll(x => x.Servings <= 0);
                }

                if (UseSuccessBounds)
                {
                    double distanceFromMiddle = Globals.GetDistanceFrom(creature.MX, creature.MY, middleOfSuccessBounds.X, middleOfSuccessBounds.Y);
                    double? newBestCreatureDistanceFromMiddle = newBestCreature == null ? null : Globals.GetDistanceFrom(newBestCreature.MX, newBestCreature.MY, middleOfSuccessBounds.X, middleOfSuccessBounds.Y);

                    if (newBestCreatureDistanceFromMiddle == null || distanceFromMiddle < newBestCreatureDistanceFromMiddle)
                    {
                        newBestCreature = creature;
                    }
                }
                else
                {
                    if (newBestCreature == null || creature.FoodEaten > newBestCreature.FoodEaten)
                    {
                        newBestCreature = creature;
                    }
                }
            }

            if (random.NextDouble() <= 0.8)
            {
                if (Food.Count < MaxFoodAmount)
                {
                    Food.Add(new GraphicalFood()
                    {
                        X = random.Next(WorldBounds.Left + WorldBounds.Width),
                        Y = random.Next(WorldBounds.Top + WorldBounds.Height),
                        Servings = 1,
                        EnergyPerServing = 30,
                        ServingDigestionCost = 0.05,
                        Size = 7,
                        Color = Color.Green
                    });
                }
            }

            if (newBestCreature != null && BestCreature != newBestCreature)
            {
                BestCreature = newBestCreature;
            }
        }
        private void GameHost_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

            foreach (GraphicalCreature creature in Creatures)
            {
                Color creatureColor;

                if (creature.IsDead)
                {
                    creatureColor = Color.FromArgb(255, 165, 41, 41);
                }
                else
                {
                    creatureColor = Color.FromArgb(creature.Color.A, creature.Color.R, creature.Color.G, creature.Color.B);
                }

                Color? creatureRingColor;

                if (creature == SelectedCreature)
                {
                    creatureRingColor = Color.White;
                }
                else if (creature == BestCreature)
                {
                    creatureRingColor = Color.Gold;
                }
                else
                {
                    creatureRingColor = null;
                }

                using (SolidBrush brush = new SolidBrush(creatureColor))
                {
                    e.Graphics.FillEllipse(brush, (float)creature.X, (float)creature.Y, (float)creature.Size, (float)creature.Size);
                }

                if (creatureRingColor != null)
                {
                    using (Pen pen = new Pen(creatureRingColor.Value, 2))
                    {
                        e.Graphics.DrawEllipse(pen, (float)creature.X, (float)creature.Y, (float)creature.Size, (float)creature.Size);
                    }
                }
            }

            foreach (GraphicalFood food in Food)
            {
                using (SolidBrush brush = new SolidBrush(food.Color))
                {
                    e.Graphics.FillEllipse(brush, (float)food.X, (float)food.Y, (float)food.Size, (float)food.Size);
                }
            }

            if (UseSuccessBounds)
            {
                e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(100, Color.Green)), SuccessBounds);
            }
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
                    Rectangle oldWorldBounds = new Rectangle(WorldBounds.Location, WorldBounds.Size);
                    Reset();
                    WorldBounds = oldWorldBounds;
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
                Food.Add(new GraphicalFood()
                {
                    X = random.Next(WorldBounds.Left + WorldBounds.Width),
                    Y = random.Next(WorldBounds.Top + WorldBounds.Height),
                    Servings = 1,
                    EnergyPerServing = 30,
                    ServingDigestionCost = 0.05,
                    Size = 7,
                    Color = Color.Green
                });
            }
        }
        public List<GraphicalCreature> GenerateCreatures()
        {
            List<GraphicalCreature> creatures = new List<GraphicalCreature>();

            for (int i = 0; i < MaxCreatureAmount; i++)
            {
                GraphicalCreature newCreature = new GraphicalCreature()
                {
                    Brain = new NeuralNetwork(PossibleCreatureInputs.ToList(), MaxCreatureProcessNodes, PossibleCreatureActions.ToList()),
                    X = random.Next(WorldBounds.Left + WorldBounds.Width),
                    Y = random.Next(WorldBounds.Top + WorldBounds.Height),
                    Size = 10,
                    Color = Color.FromArgb(255, 64, 64, Math.Max(96, random.Next(256))),
                    Speed = CreatureSpeed,
                    Metabolism = 0.1,
                    Energy = MaxCreatureEnergy,
                    MaxEnergy = MaxCreatureEnergy,
                    SightRange = 100
                };

                newCreature.Brain.Connections = newCreature.Brain.GenerateRandomConnections(MinCreatureConnections, MaxCreatureConnections, ConnectionWeightBound);
                creatures.Add(newCreature);
            }

            return creatures;
        }
        private void GameHost_MouseClick(object sender, MouseEventArgs e)
        {
            Point relativeMouseLocation = new Point(e.X - Bounds.Location.X, e.Y - Bounds.Location.Y);
            IEnumerable<GraphicalCreature> creaturesOrderedByDistanceToMouse = Creatures.OrderBy(x => Globals.GetDistanceFrom(relativeMouseLocation.X, relativeMouseLocation.Y, x.MX, x.MY));

            GraphicalCreature oldSelectedCreature = SelectedCreature;
            GraphicalCreature newSelectedCreature = creaturesOrderedByDistanceToMouse.FirstOrDefault();

            SelectedCreature = newSelectedCreature;

            if (newSelectedCreature != null && oldSelectedCreature != newSelectedCreature)
            {
                NetworkViewerForm networkViewerForm = new NetworkViewerForm(SelectedCreatureNeuralNetworkViewer);
                networkViewerForm.Show();
            }

            Invalidate();
        }
        public static void ChangeNetworkViewerNetwork(NeuralNetworkViewer networkViewer, NeuralNetwork newNeuralNetwork)
        {
            networkViewer.NeuralNetwork = newNeuralNetwork;
            networkViewer.ResetDrawnNodes();
        }
        public NeuralNetworkViewer UpdateOrCreateNetworkViewer(NeuralNetwork neuralNetwork, NeuralNetworkViewer networkViewer = null)
        {
            NeuralNetworkViewer returnedNetworkViewer = networkViewer;

            bool createNewNetworkViewer = returnedNetworkViewer == null || returnedNetworkViewer.IsDisposed;

            if (createNewNetworkViewer)
            {
                returnedNetworkViewer = new NeuralNetworkViewer();
                returnedNetworkViewer.Dock = DockStyle.Fill;
                returnedNetworkViewer.BackColor = BackColor;
                returnedNetworkViewer.lblNetworkConnectionsCount.ForeColor = Color.White;
                returnedNetworkViewer.lblNetworkNodesCount.ForeColor = Color.White;
                returnedNetworkViewer.lblSelectedNodeId.ForeColor = Color.White;
                returnedNetworkViewer.lblSelectedNodePreviousOutput.ForeColor = Color.White;
                returnedNetworkViewer.lblSelectedNodeConnectionCount.ForeColor = Color.White;
                returnedNetworkViewer.lblNodeInputOrAction.ForeColor = Color.White;
                returnedNetworkViewer.DrawTimer.Interval = 1000 / TargetFPS;
            }

            returnedNetworkViewer.NeuralNetwork = neuralNetwork;

            return returnedNetworkViewer;
        }
        public static Point Middle(int x, int y, int width, int height)
        {
            return new Point(x + width / 2, y + height / 2);
        }
        public static Point Middle(double x, double y, double width, double height)
        {
            return new Point((int)(x + width / 2), (int)(y + height / 2));
        }
        #endregion
    }
}
