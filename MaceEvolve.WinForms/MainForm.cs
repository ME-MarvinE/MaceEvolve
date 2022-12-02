using MaceEvolve.Core;
using MaceEvolve.Core.Models;
using MaceEvolve.WinForms.Controls;
using MaceEvolve.WinForms.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace MaceEvolve.WinForms
{
    public partial class MainForm : Form
    {
        #region Fields
        private Random _random = new Random();
        private int _targetFPS = 10;
        private int _targetTPS = 10;
        private double _secondsUntilNewGeneration;
        private int _generationCount = 1;
        private NeuralNetworkViewer _bestCreatureNeuralNetworkViewer;
        private NeuralNetworkViewer _selectedCreatureNeuralNetworkViewer;
        #endregion

        #region Properties
        public GameHost<GraphicalCreature, GraphicalFood> MainGameHost { get; set; } = new GameHost<GraphicalCreature, GraphicalFood>();
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
        public double SecondsUntilNewGeneration
        {
            get
            {
                return _secondsUntilNewGeneration;
            }
            set
            {
                _secondsUntilNewGeneration = value;
                lblGenEndsIn.Text = $"Ends in {string.Format("{0:0.0}", SecondsUntilNewGeneration)}s";
            }
        }
        public double SecondsPerGeneration { get; set; } = 30;
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
        public NeuralNetworkViewer SelectedCreatureNeuralNetworkViewer
        {
            get
            {
                return UpdateOrCreateNetworkViewer(MainGameHost.SelectedCreature?.Brain, _selectedCreatureNeuralNetworkViewer);
            }
        }
        public NeuralNetworkViewer BestCreatureNeuralNetworkViewer

        {
            get
            {
                return UpdateOrCreateNetworkViewer(MainGameHost.BestCreature?.Brain, _bestCreatureNeuralNetworkViewer);
            }
        }
        #endregion

        #region Constructors
        public MainForm()
        {
            InitializeComponent();
            DoubleBuffered = true;
            TargetTPS = 60;

            MainGameHost.BestCreatureChanged += MainGameHost_BestCreatureChanged;
            MainGameHost.SelectedCreatureChanged += MainGameHost_SelectedCreatureChanged;

            TargetFPS = TargetTPS;
            MainGameHost.CreatureSpeed = MainGameHost.UseSuccessBounds ? 3.5 : 2.75;
            Reset();
        }
        #endregion

        #region Methods
        public void Start()
        {
            GameTimer.Start();
            DrawTimer.Start();
            NewGenerationTimer.Start();
            MainGameHost.Stopwatch.Start();
        }
        public void Stop()
        {
            GameTimer.Stop();
            DrawTimer.Stop();
            NewGenerationTimer.Stop();
            MainGameHost.Stopwatch.Stop();
        }
        public void Reset()
        {
            MainGameHost.Reset();

            MainGameHost.WorldBounds = new Core.Models.Rectangle(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);

            double MiddleWorldBoundsX = Globals.MiddleX(MainGameHost.WorldBounds.X, MainGameHost.WorldBounds.Width);
            double MiddleWorldBoundsY = Globals.MiddleX(MainGameHost.WorldBounds.Y, MainGameHost.WorldBounds.Height);

            MainGameHost.SuccessBounds = new Core.Models.Rectangle(MiddleWorldBoundsX - 75, MiddleWorldBoundsY - 75, 150, 150);

            MainGameHost.Food.AddRange(GenerateFood());
            MainGameHost.Creatures.AddRange(GenerateCreatures());
            SecondsUntilNewGeneration = SecondsPerGeneration;
            GenerationCount = 1;

            Invalidate();
        }
        public List<GraphicalFood> GenerateFood()
        {
            List<GraphicalFood> foodList = new List<GraphicalFood>();

            foodList.AddRange(MainGameHost.GenerateFood());

            foreach (var food in foodList)
            {
                food.Color = Color.Green;
            }

            return foodList;
        }
        public List<GraphicalCreature> GenerateCreatures()
        {
            List<GraphicalCreature> creatures = new List<GraphicalCreature>();

            creatures.AddRange(MainGameHost.GenerateCreatures());

            foreach (var creature in creatures)
            {
                creature.Color = Color.FromArgb(255, 64, 64, _random.Next(256));
            }

            return creatures;
        }
        public List<GraphicalCreature> NewGenerationAsexual()
        {
            List<GraphicalCreature> newGenerationCreatures = MainGameHost.NewGenerationAsexual();

            foreach (var creature in newGenerationCreatures)
            {
                creature.Color = Color.FromArgb(255, 64, 64, _random.Next(256));
            }

            return newGenerationCreatures;
        }
        public static Point Middle(int x, int y, int width, int height)
        {
            return new Point(x + width / 2, y + height / 2);
        }
        public static Point Middle(double x, double y, double width, double height)
        {
            return new Point((int)(x + width / 2), (int)(y + height / 2));
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
        private void MainGameHost_SelectedCreatureChanged(object sender, ValueChangedEventArgs<GraphicalCreature> e)
        {
            _selectedCreatureNeuralNetworkViewer = UpdateOrCreateNetworkViewer(MainGameHost.SelectedCreature?.Brain, _selectedCreatureNeuralNetworkViewer);
        }
        private void MainGameHost_BestCreatureChanged(object sender, ValueChangedEventArgs<GraphicalCreature> e)
        {
            _bestCreatureNeuralNetworkViewer = UpdateOrCreateNetworkViewer(MainGameHost.BestCreature?.Brain, _bestCreatureNeuralNetworkViewer);
        }
        private void StartButton_Click(object sender, EventArgs e)
        {
            Start();
        }
        private void StopButton_Click(object sender, EventArgs e)
        {
            Stop();
        }
        private void ResetButton_Click(object sender, EventArgs e)
        {
            Reset();
        }
        private void NextGenButton_Click(object sender, EventArgs e)
        {
            SecondsUntilNewGeneration = 0;
            NewGenerationTimer_Tick(this, e);
        }
        private void btnTrackBestCreature_Click(object sender, EventArgs e)
        {
            NetworkViewerForm networkViewerForm = new NetworkViewerForm(BestCreatureNeuralNetworkViewer);
            networkViewerForm.Show();
        }
        private void DrawTimer_Tick(object sender, EventArgs e)
        {
            Invalidate();
        }
        private void GameTimer_Tick(object sender, EventArgs e)
        {
            MainGameHost.Update();
        }
        private void MainForm_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

            foreach (var creature in MainGameHost.Creatures)
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

                if (creature == MainGameHost.SelectedCreature)
                {
                    creatureRingColor = Color.White;
                }
                else if (creature == MainGameHost.BestCreature)
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

            foreach (GraphicalFood food in MainGameHost.Food)
            {
                using (SolidBrush brush = new SolidBrush(food.Color))
                {
                    e.Graphics.FillEllipse(brush, (float)food.X, (float)food.Y, (float)food.Size, (float)food.Size);
                }
            }

            if (MainGameHost.UseSuccessBounds)
            {
                e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(100, Color.Green)), new System.Drawing.Rectangle((int)MainGameHost.SuccessBounds.X, (int)MainGameHost.SuccessBounds.Y, (int)MainGameHost.SuccessBounds.Width, (int)MainGameHost.SuccessBounds.Height));
            }
        }
        private void MainForm_MouseClick(object sender, MouseEventArgs e)
        {
            Point relativeMouseLocation = new Point(e.X, e.Y);
            IEnumerable<GraphicalCreature> creaturesOrderedByDistanceToMouse = MainGameHost.Creatures.OrderBy(x => Globals.GetDistanceFrom(relativeMouseLocation.X, relativeMouseLocation.Y, x.MX, x.MY));

            GraphicalCreature oldSelectedCreature = MainGameHost.SelectedCreature;
            GraphicalCreature newSelectedCreature = creaturesOrderedByDistanceToMouse.FirstOrDefault();

            MainGameHost.SelectedCreature = newSelectedCreature;

            if (newSelectedCreature != null && oldSelectedCreature != newSelectedCreature)
            {
                NetworkViewerForm networkViewerForm = new NetworkViewerForm(SelectedCreatureNeuralNetworkViewer);
                networkViewerForm.Show();
            }

            Invalidate();
        }
        private void NewGenerationTimer_Tick(object sender, EventArgs e)
        {
            SecondsUntilNewGeneration -= 0.1;
            if (SecondsUntilNewGeneration <= 0)
            {
                SecondsUntilNewGeneration = SecondsPerGeneration;
                List<GraphicalCreature> newGenerationCreatures = NewGenerationAsexual();

                if (newGenerationCreatures.Count > 0)
                {
                    MainGameHost.Reset();
                    MainGameHost.Food.AddRange(GenerateFood());
                    MainGameHost.Creatures = newGenerationCreatures;
                    GenerationCount += 1;
                }
                else
                {
                    Reset();
                }
            }
        }
        #endregion
    }
}