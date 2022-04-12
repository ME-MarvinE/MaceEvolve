using MaceEvolve.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace MaceEvolve.Controls
{
    public partial class GameHost : UserControl
    {
        #region Fields
        protected static Random _Random = new Random();
        private int _TargetFPS = 5;
        #endregion

        #region Properties
        public List<Creature> Creatures { get; set; } = new List<Creature>();
        public List<Food> Food { get; set; } = new List<Food>();
        public Stopwatch Stopwatch { get; set; } = new Stopwatch();
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
        public Queue<Creature> BornCreatures { get; set; } = new Queue<Creature>();
        public int MaxCreatures { get; set; } = 300;
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
            Stopwatch.Reset();
            Creatures.Clear();
            Food.Clear();

            for (int i = 0; i < MaxCreatures; i++)
            {
                Creatures.Add(new Creature(new Genome(Genome.GetRandomizedGenes()))
                {
                    GameHost = this,
                    X = _Random.Next(Bounds.Left, Bounds.Right),
                    Y = _Random.Next(Bounds.Top, Bounds.Bottom),
                    Size = 10,
                    Color = Color.FromArgb(255, 64, 64, 255),
                    Speed = 1.3,
                    Metabolism = 0.1,
                    Energy = 150,
                    SightRange = 100,
                    MaxEnergy = 150
                });
            }

            for (int i = 0; i < MaxCreatures; i++)
            {
                Food.Add(new Apple()
                {
                    GameHost = this,
                    X = _Random.Next(Bounds.Left, Bounds.Right),
                    Y = _Random.Next(Bounds.Top, Bounds.Bottom),
                    Servings = 1,
                    EnergyPerServing = 30,
                    ServingDigestionCost = 0.05,
                    Size = 7,
                    Color = Color.Green
                });
            }
        }
        private void GameTimer_Tick(object sender, EventArgs e)
        {
            //Add creatures resulting from reproduction.
            while (BornCreatures.Count != 0)
            {
                Creatures.Add(BornCreatures.Dequeue());
            }

            List<Food> FoodList = new List<Food>(Food);
            List<Creature> CreaturesList = new List<Creature>(Creatures);

            //Remove depleted food.
            Food.RemoveAll(x => x.Servings <= 0);


            foreach (Food Food in FoodList)
            {
                Food.Update();
            }

            foreach (Creature Creature in CreaturesList)
            {
                Creature.Update();
            }

            //Add food.
            if (_Random.Next(0, 1001) <= 900) //80%
            {
                if (FoodList.Count < MaxFoodAmount)
                {
                    Food.Add(new Apple()
                    {
                        GameHost = this,
                        X = _Random.Next(Bounds.Left, Bounds.Right),
                        Y = _Random.Next(Bounds.Top, Bounds.Bottom),
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
