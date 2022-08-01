using MaceEvolve.Enums;
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
        protected static Random _Random = new Random();
        private int _TargetFPS = 10;
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
        public Rectangle WorldBounds { get; set; }
        public Rectangle SuccessBounds { get; set; }
        public int MinCreatureConnections { get; set; } = 32;
        public int MaxCreatureConnections { get; set; } = 32;
        public double CreatureSpeed { get; set; } = 2.75;
        public double NewGenerationInterval { get; set; } = 12;
        public double SecondsUntilNewGeneration { get; set; } = 12;
        public int MaxCreatureProcessNodes { get; set; } = 8;
        public double MutationChance { get; set; } = 0.1;
        public double ConnectionWeightBound { get; set; } = 4;
        public double MaxCreatureEnergy { get; set; } = 150;
        public double SuccessfulCreaturesPercentile { get; set; } = 10;
        public int GenerationCount = 1;
        public ReadOnlyCollection<CreatureInput> PossibleCreatureInputs { get; } = Globals.AllCreatureInputs.AsReadOnly();
        public ReadOnlyCollection<CreatureAction> PossibleCreatureActions{ get; } = Globals.AllCreatureActions.AsReadOnly();
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
            SuccessBounds = new Rectangle(WorldBounds.Location, new Size(150, 150));
            Stopwatch.Reset();
            SecondsUntilNewGeneration = NewGenerationInterval;
            Creatures.Clear();
            ResetFood();
            GenerationCount = 1;

            Creatures.AddRange(GenerateCreatures());

            lblGenerationCount.Text = $"Gen {GenerationCount}";
            Invalidate();
        }
        public List<Creature> NewGenerationSexual()
        {
            List<Creature> CreaturesList = new List<Creature>(Creatures);
            List<Creature> SuccessfulCreatures = GetSuccessfulCreatures(CreaturesList).ToList();
            List<Creature> NewCreatures = new List<Creature>();

            int TotalFoodEaten = SuccessfulCreatures.Count == 0 ? 0 : SuccessfulCreatures.Sum(x => x.FoodEaten);
            int MostFoodEaten = SuccessfulCreatures.Count == 0 ? 0 : SuccessfulCreatures.Max(x => x.FoodEaten);

            if (SuccessfulCreatures.Count > 0 && TotalFoodEaten > 0)
            {
                for (int i = 0; i < MaxCreatureAmount; i++)
                {
                    Creature NewCreature = Creature.Reproduce(SuccessfulCreatures, PossibleCreatureInputs.ToList(), PossibleCreatureActions.ToList());
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

                    IEnumerable<Node> NewCreatureInputNodes = NewCreature.Brain.Nodes.Where(x => x.NodeType == NodeType.Input);
                    IEnumerable<Node> NewCreatureProcessNodes = NewCreature.Brain.Nodes.Where(x => x.NodeType == NodeType.Process);
                    IEnumerable<Node> NewCreatureOutputNodes = NewCreature.Brain.Nodes.Where(x => x.NodeType == NodeType.Output);

                    foreach (var ActionToAdd in PossibleCreatureActions.Where(x => !NewCreatureOutputNodes.Any(y => y.CreatureAction == x)))
                    {
                        NewCreature.Brain.Nodes.Add(new Node(NodeType.Output, 0, CreatureAction: ActionToAdd));
                    }

                    NeuralNetwork.MutateInputNodeCount(MutationChance, NewCreature.Brain.Nodes, MaxCreatureProcessNodes, PossibleCreatureInputs);
                    NeuralNetwork.MutateProcessNodeCount(MutationChance, NewCreature.Brain.Nodes, MaxCreatureProcessNodes);
                    NeuralNetwork.MutateConnectionWeights(MutationChance, NewCreature.Brain.Connections, ConnectionWeightBound);
                    NeuralNetwork.MutateConnections(MutationChance, NewCreature.Brain.Nodes, NewCreature.Brain.Connections);
                    NeuralNetwork.MutateNodeBiases(MutationChance, NewCreature.Brain.Nodes);

                    NewCreatures.Add(NewCreature);
                }
            }

            return NewCreatures;
        }
        public List<Creature> NewGenerationAsexual()
        {
            List<Creature> CreaturesList = new List<Creature>(Creatures);
            Dictionary<Creature, double> SuccessfulCreaturesFitnesses = new Dictionary<Creature, double>();
            List<Creature> NewCreatures = new List<Creature>();

            List<Creature> SuccessfulCreatures = GetSuccessfulCreatures(CreaturesList);

            int TotalFoodEaten = SuccessfulCreatures.Count == 0 ? 0 : SuccessfulCreatures.Sum(x => x.FoodEaten);
            int MostFoodEaten = SuccessfulCreatures.Count == 0 ? 0 : SuccessfulCreatures.Max(x => x.FoodEaten);

            foreach (var Creature in SuccessfulCreatures)
            {
                double CreatureFitness = MostFoodEaten == 0 ? 0 : (double)Creature.FoodEaten / MostFoodEaten;

                SuccessfulCreaturesFitnesses.Add(Creature, CreatureFitness);
            }

            if (SuccessfulCreatures.Count > 0 && TotalFoodEaten > 0)
            {
                while (NewCreatures.Count < MaxCreatureAmount)
                {
                    int RandomSuccessfulCreatureNum = _Random.Next(0, SuccessfulCreatures.Count);
                    Creature RandomSuccessfulCreature = SuccessfulCreatures[RandomSuccessfulCreatureNum];

                    if (TotalFoodEaten > 0 && _Random.NextDouble() < SuccessfulCreaturesFitnesses[RandomSuccessfulCreature])
                    {
                        for (int i = 0; i < RandomSuccessfulCreature.FoodEaten; i++)
                        {
                            Creature NewCreature = Creature.Reproduce(new List<Creature>() { RandomSuccessfulCreature }, PossibleCreatureInputs.ToList(), PossibleCreatureActions.ToList());
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

                            IEnumerable<Node> NewCreatureInputNodes = NewCreature.Brain.Nodes.Where(x => x.NodeType == NodeType.Input);
                            IEnumerable<Node> NewCreatureProcessNodes = NewCreature.Brain.Nodes.Where(x => x.NodeType == NodeType.Process);
                            IEnumerable<Node> NewCreatureOutputNodes = NewCreature.Brain.Nodes.Where(x => x.NodeType == NodeType.Output);

                            foreach (var ActionToAdd in Globals.AllCreatureActions.Where(x => !NewCreatureOutputNodes.Any(y => y.CreatureAction == x)))
                            {
                                NewCreature.Brain.Nodes.Add(new Node(NodeType.Output, 0, CreatureAction: ActionToAdd));
                            }

                            NeuralNetwork.MutateInputNodeCount(MutationChance, NewCreature.Brain.Nodes, MaxCreatureProcessNodes, PossibleCreatureInputs);
                            NeuralNetwork.MutateProcessNodeCount(MutationChance, NewCreature.Brain.Nodes, MaxCreatureProcessNodes);
                            NeuralNetwork.MutateConnections(MutationChance, NewCreature.Brain.Nodes, NewCreature.Brain.Connections);
                            NeuralNetwork.MutateConnectionWeights(MutationChance, NewCreature.Brain.Connections, ConnectionWeightBound);
                            NeuralNetwork.MutateNodeBiases(MutationChance, NewCreature.Brain.Nodes);

                            NewCreatures.Add(NewCreature);

                            if (NewCreatures.Count >= MaxCreatureAmount)
                            {
                                break;
                            }
                        }
                    }
                }
            }

            return NewCreatures;
        }
        public List<Creature> GetSuccessfulCreatures(IEnumerable<Creature> Creatures)
        {
            //return Creatures.Where(x => x.X > SuccessBounds.Left && x.X < SuccessBounds.Right && x.Y > SuccessBounds.Top && x.Y < SuccessBounds.Bottom).ToList();

            double IndexMultiplierForTopPercentile = (1 - (double)SuccessfulCreaturesPercentile / 100);
            int TopPercentileStartingIndex = (int)(Creatures.Count() * IndexMultiplierForTopPercentile) - 1;

            List<Creature> OrderedCreatures = Creatures.OrderBy(x => x.FoodEaten).ToList();
            List<Creature> SuccessfulCreatures = new List<Creature>();

            for (int i = TopPercentileStartingIndex; i < OrderedCreatures.Count; i++)
            {
                SuccessfulCreatures.Add(OrderedCreatures[i]);
            }

            return SuccessfulCreatures;
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

            foreach (Creature Creature in CreaturesList)
            {
                Creature.Update();
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
            TargetFPS = 60;
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
                    GenerationCount += 1;
                }
                else
                {
                    Reset();
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
                Creatures.Add(new Creature(new NeuralNetwork(PossibleCreatureInputs.ToList(), MaxCreatureProcessNodes, PossibleCreatureActions.ToList(), MinCreatureConnections, MaxCreatureConnections, ConnectionWeightBound))
                {
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
                });
            }

            return Creatures;
        }
        #endregion
    }
}
