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
        public int MinCreatureConnections { get; set; } = 2;
        public int MaxCreatureConnections { get; set; } = 10;
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
        }
        public void Stop()
        {
            GameTimer.Stop();
            DrawTimer.Stop();
            Stopwatch.Stop();
        }
        public void Reset()
        {
            WorldBounds = new Rectangle(Bounds.Location, Bounds.Size);
            SuccessBounds = new Rectangle(WorldBounds.Location, new Size(100, WorldBounds.Height));
            Stopwatch.Reset();
            Creatures.Clear();
            Food.Clear();

            for (int i = 0; i < MaxCreatureAmount; i++)
            {
                Creatures.Add(new Creature(new NeuralNetwork(Globals.AllCreatureInputs, 2, Globals.AllCreatureActions, MinCreatureConnections, MaxCreatureConnections))
                {
                    GameHost = this,
                    X = _Random.Next(WorldBounds.Left + WorldBounds.Width),
                    Y = _Random.Next(WorldBounds.Top + WorldBounds.Height),
                    Size = 10,
                    Color = Color.FromArgb(255, 64, 64, 255),
                    Speed = 1.3,
                    Metabolism = 0.1,
                    Energy = 150,
                    SightRange = 100
                });
            }
        }
        public void NewGeneration()
        {
            List<Food> FoodList = new List<Food>(Food);
            List<Creature> CreaturesList = new List<Creature>(Creatures);
            List<Creature> SuccessfulCreatures = CreaturesList.Where(x => x.MX > SuccessBounds.Left && x.MX < SuccessBounds.Right && x.MX > SuccessBounds.Top && x.MX < SuccessBounds.Bottom).ToList();
            List<Creature> NewCreatures = new List<Creature>();

            for (int i = 0; i < MaxCreatureAmount; i++)
            {
                Creature NewCreature = Creature.SexuallyReproduce(SuccessfulCreatures, Globals.AllCreatureInputs, Globals.AllCreatureActions);
                NewCreature.GameHost = this;
                NewCreature.X = _Random.Next(WorldBounds.Left + WorldBounds.Width);
                NewCreature.Y = _Random.Next(WorldBounds.Top + WorldBounds.Height);
                NewCreature.Size = 10;
                NewCreature.Color = Color.FromArgb(255, 64, 64, 255);
                NewCreature.Speed = 1.3;
                NewCreature.Metabolism = 0.1;
                NewCreature.Energy = 150;
                NewCreature.SightRange = 100;

                NewCreatures.Add(NewCreature);
            }

            Creatures = NewCreatures;
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

            if (_Random.Next(0, 1001) <= 800) //80%
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
        #endregion
    }
}
