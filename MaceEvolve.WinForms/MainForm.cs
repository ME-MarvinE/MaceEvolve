using MaceEvolve.Core;
using MaceEvolve.Core.Interfaces;
using MaceEvolve.Core.Models;
using MaceEvolve.WinForms.Controls;
using MaceEvolve.WinForms.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MaceEvolve.WinForms
{
    public partial class MainForm : Form
    {
        #region Properties
        public int SimulationTPS { get; set; }
        public bool SimulationRunning { get; set; }
        public int CurrentRunTicksElapsed { get; set; }
        public long AllRunsElapsed { get; set; }
        public bool GatherStepInfoForAllCreatures { get; set; }
        public bool IsInFastMode { get; set; }
        public GraphicalGameHost<GraphicalStep<GraphicalCreature, GraphicalFood>, GraphicalCreature, GraphicalFood> MainGameHost { get; set; }
        public NetworkViewerForm SelectedCreatureNetworkViewerForm { get; set; }
        public NetworkViewerForm BestCreatureNetworkViewerForm { get; set; }
        public float SimulationMspt
        {
            get
            {
                return (1f / SimulationTPS) * 1000;
            }
        }
        public List<TimeSpan> FailedRunsUptimes { get; set; } = new List<TimeSpan>();
        #endregion

        #region Constructors
        public MainForm()
        {
            InitializeComponent();

            DoubleBuffered = true;
        }
        #endregion

        #region Methods
        public void Reset()
        {
            MainGameHost.WorldBounds = new Core.Models.Rectangle(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);

            BestCreatureNetworkViewerForm.NetworkViewer.Step = null;
            BestCreatureNetworkViewerForm.NetworkViewer.NeuralNetwork = null;

            SelectedCreatureNetworkViewerForm.NetworkViewer.Step = null;
            SelectedCreatureNetworkViewerForm.NetworkViewer.NeuralNetwork = null;

            MainGameHost.ResetStep(GenerateCreatures(), GenerateFood());

            FailedRunsUptimes.Clear();
            CurrentRunTicksElapsed = 0;
        }
        public void FailRun()
        {
            BestCreatureNetworkViewerForm.NetworkViewer.Step = null;
            BestCreatureNetworkViewerForm.NetworkViewer.NeuralNetwork = null;

            SelectedCreatureNetworkViewerForm.NetworkViewer.Step = null;
            SelectedCreatureNetworkViewerForm.NetworkViewer.NeuralNetwork = null;

            MainGameHost.ResetStep(GenerateCreatures(), GenerateFood());

            FailedRunsUptimes.Add(TimeSpan.FromMilliseconds(CurrentRunTicksElapsed * SimulationMspt));
            CurrentRunTicksElapsed = 0;
        }
        public List<GraphicalFood> GenerateFood(List<GraphicalFood> foodToCovert = null)
        {
            List<GraphicalFood> foodList = foodToCovert ?? MainGameHost.GenerateFood();

            foreach (var food in foodList)
            {
                food.Color = Color.Green;
            }

            return foodList;
        }
        public List<GraphicalCreature> GenerateCreatures(List<GraphicalCreature> creaturesToCovert = null)
        {
            List<GraphicalCreature> creatures = creaturesToCovert ?? MainGameHost.GenerateCreatures();

            foreach (var creature in creatures)
            {
                creature.Color = Color.FromArgb(255, 64, 64, MaceRandom.Current.Next(256));
            }

            return creatures;
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
        private void MainGameHost_SelectedCreatureChanged(object sender, ValueChangedEventArgs<GraphicalCreature> e)
        {
            SelectedCreatureNetworkViewerForm.NetworkViewer.NeuralNetwork = MainGameHost.SelectedCreature?.Brain;
        }
        private void MainGameHost_BestCreatureChanged(object sender, ValueChangedEventArgs<GraphicalCreature> e)
        {
            BestCreatureNetworkViewerForm.NetworkViewer.NeuralNetwork = MainGameHost.BestCreature?.Brain;
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
        private async void btnFastForward_Click(object sender, EventArgs e)
        {
            IsInFastMode = true;

            await Task.Factory.StartNew(() =>
            {
                while (SimulationRunning)
                {
                    UpdateSimulation();
                }
            }, TaskCreationOptions.LongRunning);
            IsInFastMode = false;
        }
        private void btnTrackBestCreature_Click(object sender, EventArgs e)
        {
            BestCreatureNetworkViewerForm.Show();
        }
        private void GameTimer_Tick(object sender, EventArgs e)
        {
            if (!IsInFastMode && SimulationRunning)
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

            foreach (var food in MainGameHost.CurrentStep.Food)
            {
                using (SolidBrush brush = new SolidBrush(food.Color))
                {
                    e.Graphics.FillEllipse(brush, food.X, food.Y, food.Size, food.Size);
                }
            }
        }
        private void MainForm_MouseClick(object sender, MouseEventArgs e)
        {
            Point relativeMouseLocation = new Point(e.X, e.Y);
            IEnumerable<GraphicalCreature> creaturesOrderedByDistanceToMouse = MainGameHost.CurrentStep.Creatures.OrderBy(x => Globals.GetDistanceFrom(relativeMouseLocation.X, relativeMouseLocation.Y, x.MX, x.MY));

            GraphicalCreature oldSelectedCreature = MainGameHost.SelectedCreature;
            GraphicalCreature newSelectedCreature = creaturesOrderedByDistanceToMouse.FirstOrDefault();

            MainGameHost.SelectedCreature = newSelectedCreature;

            if (newSelectedCreature != null && (oldSelectedCreature != newSelectedCreature || SelectedCreatureNetworkViewerForm.Visible == false))
            {
                SelectedCreatureNetworkViewerForm.Show();
            }

            Invalidate();
        }
        public void UpdateSimulation()
        {
            MainGameHost.CreatureOffspringColor = Color.FromArgb(255, 64, 64, MaceRandom.Current.Next(256));
            MainGameHost.NextStep(GatherStepInfoForAllCreatures);

            SelectedCreatureNetworkViewerForm.NetworkViewer.Step = MainGameHost.CurrentStep;
            BestCreatureNetworkViewerForm.NetworkViewer.Step = MainGameHost.CurrentStep;

            CurrentRunTicksElapsed += 1;

            if (CurrentRunTicksElapsed % 500 == 0 && MainGameHost.CurrentStep.Creatures.All(x => x.IsDead))
            {
                FailRun();
            }
        }
        private void MainForm_Load(object sender, EventArgs e)
        {
            SimulationTPS = 60;

            MainGameHost = new GraphicalGameHost<GraphicalStep<GraphicalCreature, GraphicalFood>, GraphicalCreature, GraphicalFood>();
            MainGameHost.CreatureSize = 10;
            float baseFoodSize = MainGameHost.CreatureSize;
            MainGameHost.MinFoodSize = baseFoodSize * 0.2f;
            MainGameHost.MaxFoodSize = baseFoodSize * 1.2f;
            MainGameHost.CreatureSpeed = 2.75f;

            MainGameHost.BestCreatureChanged += MainGameHost_BestCreatureChanged;
            MainGameHost.SelectedCreatureChanged += MainGameHost_SelectedCreatureChanged;
            GameTimer.Interval = (int)SimulationMspt;

            BestCreatureNetworkViewerForm = new NetworkViewerForm();
            BestCreatureNetworkViewerForm.NetworkViewer = new NeuralNetworkViewer();
            BestCreatureNetworkViewerForm.NetworkViewer.Dock = DockStyle.Fill;
            BestCreatureNetworkViewerForm.NetworkViewer.BackColor = BackColor;
            BestCreatureNetworkViewerForm.NetworkViewer.lblNetworkConnectionsCount.ForeColor = Color.White;
            BestCreatureNetworkViewerForm.NetworkViewer.lblNetworkNodesCount.ForeColor = Color.White;
            BestCreatureNetworkViewerForm.NetworkViewer.lblSelectedNodeId.ForeColor = Color.White;
            BestCreatureNetworkViewerForm.NetworkViewer.lblSelectedNodePreviousOutput.ForeColor = Color.White;
            BestCreatureNetworkViewerForm.NetworkViewer.lblSelectedNodeConnectionCount.ForeColor = Color.White;
            BestCreatureNetworkViewerForm.NetworkViewer.lblNodeInputOrAction.ForeColor = Color.White;
            BestCreatureNetworkViewerForm.NetworkViewer.DrawTimer.Interval = GameTimer.Interval;
            BestCreatureNetworkViewerForm.NetworkViewer.Step = MainGameHost.CurrentStep;

            SelectedCreatureNetworkViewerForm = new NetworkViewerForm();
            SelectedCreatureNetworkViewerForm.NetworkViewer = new NeuralNetworkViewer();
            SelectedCreatureNetworkViewerForm.NetworkViewer.Dock = DockStyle.Fill;
            SelectedCreatureNetworkViewerForm.NetworkViewer.BackColor = BackColor;
            SelectedCreatureNetworkViewerForm.NetworkViewer.lblNetworkConnectionsCount.ForeColor = Color.White;
            SelectedCreatureNetworkViewerForm.NetworkViewer.lblNetworkNodesCount.ForeColor = Color.White;
            SelectedCreatureNetworkViewerForm.NetworkViewer.lblSelectedNodeId.ForeColor = Color.White;
            SelectedCreatureNetworkViewerForm.NetworkViewer.lblSelectedNodePreviousOutput.ForeColor = Color.White;
            SelectedCreatureNetworkViewerForm.NetworkViewer.lblSelectedNodeConnectionCount.ForeColor = Color.White;
            SelectedCreatureNetworkViewerForm.NetworkViewer.lblNodeInputOrAction.ForeColor = Color.White;
            SelectedCreatureNetworkViewerForm.NetworkViewer.DrawTimer.Interval = GameTimer.Interval;
            SelectedCreatureNetworkViewerForm.NetworkViewer.Step = MainGameHost.CurrentStep;

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
            btnFastFoward.Visible = isControlMenuVisible;
        }
        private void UpdateUIText()
        {
            double millisecondsInFailedRuns = 0;

            foreach (var failedRunTimeSpan in FailedRunsUptimes)
            {
                millisecondsInFailedRuns += failedRunTimeSpan.TotalMilliseconds;
            }

            TimeSpan timeInCurrentRun = TimeSpan.FromMilliseconds(CurrentRunTicksElapsed * SimulationMspt);
            TimeSpan timeInFailedRuns = TimeSpan.FromMilliseconds(millisecondsInFailedRuns);
            TimeSpan timeInAllRuns = timeInFailedRuns.Add(timeInCurrentRun);
            TimeSpan averageTimePerRun = TimeSpan.FromMilliseconds(FailedRunsUptimes.Count == 0 ? 0 : FailedRunsUptimes.Average(x => x.TotalMilliseconds));

            if (SimulationRunning)
            {
                lblSimulationRunning.Text = IsInFastMode ? "Running (Fast)" : "Running";
            }
            else
            {
                lblSimulationRunning.Text = IsInFastMode ? "Stopped (Fast)" : "Stopped";
            }

            lblGenerationCount.Text = $"Run {FailedRunsUptimes.Count + 1}";
            lblGenEndsIn.Text = $"Uptime: {timeInAllRuns:d\\d' 'h\\h' 'm\\m' 's\\.ff\\s}, Failed: {timeInFailedRuns:d\\d' 'h\\h' 'm\\m' 's\\.ff\\s}" +
                $"\nRun {FailedRunsUptimes.Count + 1}, {timeInCurrentRun:d\\d' 'h\\h' 'm\\m' 's\\.ff\\s}, Average: {averageTimePerRun:d\\d' 'h\\h' 'm\\m' 's\\.ff\\s}";

            GatherStepInfoForAllCreaturesButton.Text = $"Gather Step Info For All Creatures: {(GatherStepInfoForAllCreatures ? "Enabled" : "Disabled")}";
        }
        #endregion

        private void btnLoadStep_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog() { Filter = "JSON Files (*.json)|*.json", DefaultExt = "json" };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string serializedStep = File.ReadAllText(openFileDialog.FileName);
                GraphicalStep<GraphicalCreature, GraphicalFood> savedStep = JsonConvert.DeserializeObject<GraphicalStep<GraphicalCreature, GraphicalFood>>(serializedStep);

                MainGameHost.ConnectionWeightBound = savedStep.ConnectionWeightBound;
                MainGameHost.MinCreatureConnections = savedStep.MinCreatureConnections;
                MainGameHost.MaxCreatureConnections = savedStep.MaxCreatureConnections;
                MainGameHost.MaxCreatureProcessNodes = savedStep.MaxCreatureProcessNodes;
                MainGameHost.LoopWorldBounds = savedStep.LoopWorldBounds;
                MainGameHost.WorldBounds = savedStep.WorldBounds;
                MainGameHost.CreatureOffspringColor = savedStep.CreatureOffspringColor;
                MainGameHost.ResetStep(GenerateCreatures(savedStep.Creatures.ToList()), GenerateFood(savedStep.Food.ToList()));
                MessageBox.Show("Step Loaded Successfully.");
            }
        }

        private void btnSaveCurrentStep_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog() { Filter = "JSON Files (*.json)|*.json", DefaultExt = "json" };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string serializedStep = JsonConvert.SerializeObject(MainGameHost.CurrentStep, new JsonSerializerSettings() { Formatting = Formatting.Indented });
                File.WriteAllText(saveFileDialog.FileName, serializedStep);
                MessageBox.Show("Step Saved Successfully.");
            }
        }
    }
}