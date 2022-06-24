using MaceEvolve.Enums;
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
            Stopwatch.Reset();
            Creatures.Clear();
            Food.Clear();

            for (int i = 0; i < MaxCreatureAmount; i++)
            {
                Creatures.Add(new Creature(new NeuralNetwork(Globals.AllCreatureInputs, 2, Globals.AllCreatureActions, 2, 10))
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

            foreach (var Creature in CreaturesList.Where(x => x.Energy > 0))
            {

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
