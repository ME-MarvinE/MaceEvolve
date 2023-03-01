using MaceEvolve.Core;
using MaceEvolve.Core.Models;
using MaceEvolve.WinForms.Controls;
using MaceEvolve.WinForms.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MaceEvolve.WinForms
{
    public partial class MainForm : Form
    {
        #region Fields
        private Random _random = new Random();
        private NeuralNetworkViewer _bestCreatureNeuralNetworkViewer;
        private NeuralNetworkViewer _selectedCreatureNeuralNetworkViewer;
        #endregion

        #region Properties
        public int SimulationTPS { get; set; }
        public long TicksPerGeneration { get; set; }
        public int GenerationCount { get; set; }
        public bool SimulationRunning { get; set; }
        public int GenerationsToRunFor { get; set; }
        public int TicksInCurrentGeneration { get; set; }
        public bool GatherStepInfoForAllCreatures { get; set; }
        public bool IsInFastMode { get; set; }
        public GameHost<GraphicalCreature, GraphicalFood> MainGameHost { get; set; }
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
        public float SimulationMspt
        {
            get
            {
                return (1f / SimulationTPS) * 1000;
            }
        }
        public long TicksWhenSimulationEnds
        {
            get
            {
                return TicksPerGeneration * GenerationsToRunFor;
            }
        }
        public long TicksUntilSimulationIsCompleted
        {
            get
            {
                return TicksWhenSimulationEnds - TicksElapsed;
            }
        }
        public long TicksElapsed
        {
            get
            {
                return TicksPerGeneration * (GenerationCount - 1) + TicksInCurrentGeneration;
            }
        }
        public long TicksUntilCurrentGenerationIsCompleted
        {
            get
            {
                return TicksPerGeneration - TicksInCurrentGeneration;
            }
        }
        #endregion

        #region Constructors
        public MainForm()
        {
            InitializeComponent();
            GatherStepInfoForAllCreaturesButton.Text = $"Gather Step Info For All Creatures: {GatherStepInfoForAllCreatures}";
            DoubleBuffered = true;
        }
        #endregion

        #region Methods
        public void Reset()
        {
            MainGameHost.Reset();

            MainGameHost.WorldBounds = new Core.Models.Rectangle(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);

            float MiddleWorldBoundsX = Globals.MiddleX(MainGameHost.WorldBounds.X, MainGameHost.WorldBounds.Width);
            float MiddleWorldBoundsY = Globals.MiddleX(MainGameHost.WorldBounds.Y, MainGameHost.WorldBounds.Height);

            MainGameHost.SuccessBounds = new Core.Models.Rectangle(MiddleWorldBoundsX - 75, MiddleWorldBoundsY - 75, 150, 150);

            MainGameHost.CurrentStep = new Step<GraphicalCreature, GraphicalFood>(GenerateCreatures(), GenerateFood(), MainGameHost.WorldBounds);

            TicksInCurrentGeneration = 0;
            GenerationCount = 1;
            UpdateUIText();
            GatherStepInfoForAllCreaturesButton.Text = $"Gather Step Info For All Creatures: {(GatherStepInfoForAllCreatures ? "Enabled" : "Disabled")}";
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
            List<GraphicalCreature> newGenerationCreatures = MainGameHost.CreateNewGenerationAsexual(MainGameHost.CurrentStep.Creatures);

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
        public static Point Middle(float x, float y, float width, float height)
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
                returnedNetworkViewer.DrawTimer.Interval = GameTimer.Interval;
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
            SimulationRunning = true;
        }
        private void StopButton_Click(object sender, EventArgs e)
        {
            SimulationRunning = false;
        }
        private void ResetButton_Click(object sender, EventArgs e)
        {
            Reset();
        }
        private async void btnForwardGen_Click(object sender, EventArgs e)
        {
            IsInFastMode = true;
            await Task.Run(() =>
            {
                //Doesn't work if loop runs from 0 to x.
                for (long i = TicksUntilCurrentGenerationIsCompleted; i > 0 && SimulationRunning; i--)
                {
                    UpdateSimulation();
                }
            });
            IsInFastMode = false;
        }
        private async void btnForwardGens_Click(object sender, EventArgs e)
        {
            IsInFastMode = true;
            //Doesn't work if loop runs from 0 to x.
            await Task.Run(() =>
            {
                long ticksIn100Generations = TicksPerGeneration * 100;
                for (long i = ticksIn100Generations; i > 0 && SimulationRunning; i--)
                {
                    UpdateSimulation();
                }
            });
            IsInFastMode = false;
        }
        private async void btnForwardAllGens_Click(object sender, EventArgs e)
        {
            IsInFastMode = true;
            await Task.Run(() =>
            {
                //Doesn't work if loop runs from 0 to x.
                for (long i = TicksUntilSimulationIsCompleted; i > 0 && SimulationRunning; i--)
                {
                    UpdateSimulation();
                }
            });
            IsInFastMode = false;
        }
        private void btnTrackBestCreature_Click(object sender, EventArgs e)
        {
            if (IsInFastMode)
            {
                return;
            }

            NetworkViewerForm networkViewerForm = new NetworkViewerForm(BestCreatureNeuralNetworkViewer);
            networkViewerForm.Show();
        }
        private void GameTimer_Tick(object sender, EventArgs e)
        {
            if (!IsInFastMode && SimulationRunning && GenerationCount <= GenerationsToRunFor)
            {
                UpdateSimulation();
            }

            Invalidate();
        }
        private void MainForm_Paint(object sender, PaintEventArgs e)
        {
            UpdateUIText();

            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

            if (!IsInFastMode)
            {
                foreach (var creature in MainGameHost.CurrentStep.Creatures)
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
                        e.Graphics.FillEllipse(brush, creature.X, creature.Y, creature.Size, creature.Size);
                    }

                    if (creatureRingColor != null)
                    {
                        using (Pen pen = new Pen(creatureRingColor.Value, 2))
                        {
                            e.Graphics.DrawEllipse(pen, creature.X, creature.Y, creature.Size, creature.Size);
                        }
                    }
                }

                foreach (GraphicalFood food in MainGameHost.CurrentStep.Food)
                {
                    using (SolidBrush brush = new SolidBrush(Color.Green))
                    {
                        e.Graphics.FillEllipse(brush, food.X, food.Y, food.Size, food.Size);
                    }
                }
            }

            if (MainGameHost.UseSuccessBounds)
            {
                e.Graphics.FillRectangle(new SolidBrush(Color.FromArgb(100, Color.Green)), new System.Drawing.Rectangle((int)MainGameHost.SuccessBounds.X, (int)MainGameHost.SuccessBounds.Y, (int)MainGameHost.SuccessBounds.Width, (int)MainGameHost.SuccessBounds.Height));
            }
        }
        private void MainForm_MouseClick(object sender, MouseEventArgs e)
        {
            if (IsInFastMode)
            {
                return;
            }

            Point relativeMouseLocation = new Point(e.X, e.Y);
            IEnumerable<GraphicalCreature> creaturesOrderedByDistanceToMouse = MainGameHost.CurrentStep.Creatures.OrderBy(x => Globals.GetDistanceFrom(relativeMouseLocation.X, relativeMouseLocation.Y, x.MX, x.MY));

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
        public void NewGeneration()
        {
            List<GraphicalCreature> newGenerationCreatures = NewGenerationAsexual();

            if (newGenerationCreatures.Count > 0)
            {
                MainGameHost.Reset();
                MainGameHost.CurrentStep = new Step<GraphicalCreature, GraphicalFood>(newGenerationCreatures, GenerateFood(), MainGameHost.WorldBounds);

                TicksInCurrentGeneration = 0;
                GenerationCount += 1;
            }
            else
            {
                Reset();
            }
        }
        public void UpdateSimulation()
        {
            MainGameHost.NextStep(GatherStepInfoForAllCreatures);

            TicksInCurrentGeneration += 1;

            if (TicksInCurrentGeneration >= TicksPerGeneration)
            {
                NewGeneration();
            }
        }
        private void MainForm_Load(object sender, EventArgs e)
        {
            GenerationsToRunFor = 500000;
            SimulationTPS = 60;
            TicksPerGeneration = SimulationTPS * 30; //30 Seconds per generation.

            MainGameHost = new GameHost<GraphicalCreature, GraphicalFood>();
            MainGameHost.CreatureSize = 10;
            MainGameHost.FoodSize = MainGameHost.CreatureSize * 0.7f;
            MainGameHost.CreatureSpeed = MainGameHost.UseSuccessBounds ? 2.75f * 1.3f : 2.75f;

            MainGameHost.BestCreatureChanged += MainGameHost_BestCreatureChanged;
            MainGameHost.SelectedCreatureChanged += MainGameHost_SelectedCreatureChanged;
            GameTimer.Interval = (int)SimulationMspt;

            Reset();
        }
        private void GatherStepInfoForAllCreaturesButton_Click(object sender, EventArgs e)
        {
            GatherStepInfoForAllCreatures = !GatherStepInfoForAllCreatures;
        }
        private void MainForm_KeyDown(object sender, KeyEventArgs e)
        {
            bool isControlMenuVisible = StartButton.Visible;

            if (e.KeyCode == Keys.D0)
            {
                isControlMenuVisible = !isControlMenuVisible;
            }

            StartButton.Visible = isControlMenuVisible;
            StopButton.Visible = isControlMenuVisible;
            ResetButton.Visible = isControlMenuVisible;
            GatherStepInfoForAllCreaturesButton.Visible = isControlMenuVisible;
            btnTrackBestCreature.Visible = isControlMenuVisible;
            btnForwardGen.Visible = isControlMenuVisible;
            btnForwardGens.Visible = isControlMenuVisible;
            btnForwardAllGens.Visible = isControlMenuVisible;
        }
        private void UpdateUIText()
        {
            TimeSpan timeInCurrentGeneration = TimeSpan.FromMilliseconds(TicksInCurrentGeneration * SimulationMspt);
            TimeSpan timePerGeneration = TimeSpan.FromMilliseconds(TicksPerGeneration * SimulationMspt);
            TimeSpan timeInSimulation = TimeSpan.FromMilliseconds(TicksElapsed * SimulationMspt);
            TimeSpan timePerSimulation = TimeSpan.FromMilliseconds(TicksWhenSimulationEnds * SimulationMspt);
            TimeSpan timeUntilSimulationEnds = TimeSpan.FromMilliseconds(TicksUntilSimulationIsCompleted * SimulationMspt);

            lblSimulationRunning.Text = SimulationRunning ? "Running" : "Stopped";
            lblGenerationCount.Text = $"Gen {GenerationCount}";
            lblGenEndsIn.Text = $"{timeInSimulation:d\\d' 'h\\h' 'm\\m' 's\\.ff\\s}/{timePerSimulation:d\\d' 'h\\h' 'm\\m' 's\\.ff\\s}/{timeUntilSimulationEnds:d\\d' 'h\\h' 'm\\m' 's\\.ff\\s}" +
            $"\nGen {GenerationCount}/{GenerationsToRunFor}, {timeInCurrentGeneration:s\\.ff\\s}/{timePerGeneration:s\\.ff\\s}";
            GatherStepInfoForAllCreaturesButton.Text = $"Gather Step Info For All Creatures: {(GatherStepInfoForAllCreatures ? "Enabled" : "Disabled")}";
        }
        #endregion
    }
}