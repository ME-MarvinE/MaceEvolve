﻿using MaceEvolve.Enums;
using MaceEvolve.Models;
using System;
using System.Collections.Generic;
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
        public double NewGenerationInterval { get; set; } = 10;
        public double SecondsUntilNewGeneration { get; set; } = 10;
        public int MaxCreatureProcessNodes { get; set; } = 5;
        public double MutationChance { get; set; } = 0.2;
        public double ConnectionWeightBound { get; set; } = 4;
        public double MaxCreatureEnergy { get; set; } = 150;
        public double SuccessfulCreaturesPercentile { get; set; } = 50;
        public int GenerationCount = 0;
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
            Food.Clear();
            GenerationCount = 0;

            for (int i = 0; i < MaxCreatureAmount; i++)
            {
                Creatures.Add(new Creature(new NeuralNetwork(Globals.AllCreatureInputs, MaxCreatureProcessNodes, Globals.AllCreatureActions, MinCreatureConnections, MaxCreatureConnections, ConnectionWeightBound))
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

            lblGenerationCount.Text = $"Gen {GenerationCount}";
        }
        public void NewGenerationSexual()
        {
            List<Creature> CreaturesList = new List<Creature>(Creatures);
            List<Creature> SuccessfulCreatures = GetSuccessfulCreatures(CreaturesList).ToList();
            List<Creature> NewCreatures = new List<Creature>();

            for (int i = 0; i < MaxCreatureAmount; i++)
            {
                Creature NewCreature = Creature.Reproduce(SuccessfulCreatures, Globals.AllCreatureInputs.ToList(), Globals.AllCreatureActions.ToList());
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

                IEnumerable<InputNode> NewCreatureInputNodes = NeuralNetwork.GetInputNodes(NewCreature.Brain.Nodes);
                IEnumerable<ProcessNode> NewCreatureProcessNodes = NeuralNetwork.GetProcessNodes(NewCreature.Brain.Nodes);
                IEnumerable<OutputNode> NewCreatureOutputNodes = NeuralNetwork.GetOutputNodes(NewCreature.Brain.Nodes);

                foreach (var InputToAdd in Globals.AllCreatureInputs.Where(x => !NewCreatureInputNodes.Any(y => y.CreatureInput == x)))
                {
                    NewCreature.Brain.Nodes.Add(new InputNode(InputToAdd, Globals.Map(_Random.NextDouble(), 0, 1, -1, 1)));
                }

                foreach (var ActionToAdd in Globals.AllCreatureActions.Where(x => !NewCreatureOutputNodes.Any(y => y.CreatureAction == x)))
                {
                    NewCreature.Brain.Nodes.Add(new OutputNode(ActionToAdd, Globals.Map(_Random.NextDouble(), 0, 1, -1, 1)));
                }

                for (int j = NewCreatureProcessNodes.Count(); j < MaxCreatureProcessNodes; j++)
                {
                    NewCreature.Brain.Nodes.Add(new ProcessNode(0));
                }

                NeuralNetwork.MutateConnectionWeights(MutationChance, NewCreature.Brain.Connections, ConnectionWeightBound);
                NeuralNetwork.MutateConnections(MutationChance, NewCreature.Brain.Nodes, NewCreature.Brain.Connections);
                NeuralNetwork.MutateNodeBiases(MutationChance, NewCreature.Brain.Nodes);

                NewCreatures.Add(NewCreature);
            }

            Creatures = NewCreatures;
            GenerationCount += 1;
        }
        public void NewGenerationAsexual()
        {
            List<Creature> CreaturesList = new List<Creature>(Creatures);
            Dictionary<Creature, double> SuccessfulCreaturesFitnesses = new Dictionary<Creature, double>();
            List<Creature> NewCreatures = new List<Creature>();

            int TotalFoodEaten = CreaturesList.Count == 0 ? 0 : CreaturesList.Sum(x => x.FoodEaten);
            int MostFoodEaten = CreaturesList.Count == 0 ? 0 : CreaturesList.Max(x => x.FoodEaten);
            double AverageFoodEaten = CreaturesList.Count == 0 ? 0 : (double)TotalFoodEaten / CreaturesList.Count;
            int TopPercentileStartingIndex = (int)(CreaturesList.Count * (1 - (double)SuccessfulCreaturesPercentile / 100)) - 1;

            List<Creature> OrderedCreatures = CreaturesList.OrderBy(x => x.FoodEaten).ToList();
            List<Creature> SuccessfulCreatures = new List<Creature>();

            for (int i = (int)TopPercentileStartingIndex; i < OrderedCreatures.Count; i++)
            {
                SuccessfulCreatures.Add(OrderedCreatures[i]);
            }

            if (SuccessfulCreatures.Count == 0)
            {
                SuccessfulCreatures = OrderedCreatures;
            }

            foreach (var Creature in SuccessfulCreatures)
            {
                double CreatureFitness = MostFoodEaten == 0 ? 0 : (double)Creature.FoodEaten / MostFoodEaten;

                SuccessfulCreaturesFitnesses.Add(Creature, CreatureFitness);
            }

            while (NewCreatures.Count < MaxCreatureAmount && SuccessfulCreatures.Count > 0)
            {
                int RandomSuccessfulCreatureNum = _Random.Next(0, SuccessfulCreatures.Count);
                Creature RandomSuccessfulCreature = SuccessfulCreatures[RandomSuccessfulCreatureNum];

                if (TotalFoodEaten == 0 || _Random.NextDouble() < SuccessfulCreaturesFitnesses[RandomSuccessfulCreature])
                {
                    Creature NewCreature = Creature.Reproduce(new List<Creature>() { RandomSuccessfulCreature }, Globals.AllCreatureInputs.ToList(), Globals.AllCreatureActions.ToList());
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

                    IEnumerable<InputNode> NewCreatureInputNodes = NeuralNetwork.GetInputNodes(NewCreature.Brain.Nodes);
                    IEnumerable<ProcessNode> NewCreatureProcessNodes = NeuralNetwork.GetProcessNodes(NewCreature.Brain.Nodes);
                    IEnumerable<OutputNode> NewCreatureOutputNodes = NeuralNetwork.GetOutputNodes(NewCreature.Brain.Nodes);

                    foreach (var InputToAdd in Globals.AllCreatureInputs.Where(x => !NewCreatureInputNodes.Any(y => y.CreatureInput == x)))
                    {
                        NewCreature.Brain.Nodes.Add(new InputNode(InputToAdd, 0));
                    }

                    foreach (var ActionToAdd in Globals.AllCreatureActions.Where(x => !NewCreatureOutputNodes.Any(y => y.CreatureAction == x)))
                    {
                        NewCreature.Brain.Nodes.Add(new OutputNode(ActionToAdd, 0));
                    }

                    for (int i = NewCreatureProcessNodes.Count(); i < MaxCreatureProcessNodes; i++)
                    {
                        NewCreature.Brain.Nodes.Add(new ProcessNode(0));
                    }

                    NeuralNetwork.MutateNodeBiases(MutationChance, NewCreature.Brain.Nodes);

                    NeuralNetwork.MutateConnections(MutationChance, NewCreature.Brain.Nodes, NewCreature.Brain.Connections);
                    NeuralNetwork.MutateConnectionWeights(MutationChance, NewCreature.Brain.Connections, ConnectionWeightBound);


                    NewCreatures.Add(NewCreature);
                }
            }

            Creatures = NewCreatures;
            GenerationCount += 1;
        }
        public IEnumerable<Creature> GetSuccessfulCreatures(IEnumerable<Creature> Creatures)
        {
            return Creatures.Where(x => x.X > SuccessBounds.Left && x.X < SuccessBounds.Right && x.Y > SuccessBounds.Top && x.Y < SuccessBounds.Bottom);
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
        private void NewGenerationTimer_Tick(object sender, EventArgs e)
        {
            if (SecondsUntilNewGeneration <= 0)
            {
                SecondsUntilNewGeneration = NewGenerationInterval;
                NewGenerationAsexual();
            }
            else
            {
                SecondsUntilNewGeneration -= 0.1;
            }
        }
        #endregion
    }
}
