using MaceEvolve.Core;
using MaceEvolve.Core.Enums;
using MaceEvolve.Core.Interfaces;
using MaceEvolve.Core.Models;
using MaceEvolve.WinForms.Controls;
using MaceEvolve.WinForms.JsonContractResolvers;
using MaceEvolve.WinForms.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
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
        #region Fields
        private int _simulationTPS = 60;
        private int _simulationFPS = 60;
        private bool _linkFPSAndTPS;
        private bool _isInFastMode;
        private bool _gatherStepInfoForAllCreatures;
        private bool _simulationRunning;
        private bool _isUIVisible = true;
        private bool _showTreeColorByAge = true;
        #endregion

        #region Properties
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int SimulationTPS
        {
            get
            {
                return _simulationTPS;
            }
            set
            {
                _simulationTPS = value;
                nudSimulationTPS.Value = value;
                GameTimer.Interval = Math.Max((int)SimulationMspt, 1);

                if (LinkFPSAndTPS && SimulationFPS != value)
                {
                    SimulationFPS = SimulationTPS;
                }
            }
        }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int SimulationFPS
        {
            get
            {
                return _simulationFPS;
            }
            set
            {
                _simulationFPS = value;
                nudSimulationFPS.Value = value;
                DrawTimer.Interval = (int)Math.Max((1f / SimulationFPS) * 1000, 1);
                BestCreatureNetworkViewerForm.NetworkViewer.DrawTimer.Interval = DrawTimer.Interval;
                SelectedCreatureNetworkViewerForm.NetworkViewer.DrawTimer.Interval = DrawTimer.Interval;

                if (LinkFPSAndTPS && SimulationTPS != value)
                {
                    SimulationTPS = SimulationFPS;
                }
            }
        }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool SimulationRunning
        {
            get
            {
                return _simulationRunning;
            }
            set
            {
                _simulationRunning = value;
                UpdateIsRunningText();
            }
        }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int CurrentRunTicksElapsed { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public long AllRunsElapsed { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool GatherStepInfoForAllCreatures
        {
            get
            {
                return _gatherStepInfoForAllCreatures;
            }
            set
            {
                _gatherStepInfoForAllCreatures = value;
                chkGatherStepInfoForAllCreatures.Checked = _gatherStepInfoForAllCreatures;
            }
        }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsInFastMode
        {
            get
            {
                return _isInFastMode;
            }
            set
            {
                _isInFastMode = value;
                UpdateIsRunningText();
            }
        }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public GraphicalGameHost<GraphicalStep<GraphicalCreature, GraphicalFood, GraphicalTree>, GraphicalCreature, GraphicalFood, GraphicalTree> MainGameHost { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public NetworkViewerForm SelectedCreatureNetworkViewerForm { get; set; }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public NetworkViewerForm BestCreatureNetworkViewerForm { get; set; }
        public float SimulationMspt
        {
            get
            {
                return (1f / SimulationTPS) * 1000;
            }
        }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public List<TimeSpan> FailedRunsUptimes { get; set; } = new List<TimeSpan>();
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public StepResult<GraphicalCreature> PreviousStepResult { get; set; }
        protected static JsonSerializerSettings SaveStepSerializerSettings { get; }
        protected static JsonSerializerSettings LoadStepSerializerSettings { get; }
        protected Pen FieldOfViewPen { get; set; } = new Pen(Color.FromArgb(255, 20, 20, 255), 1);
        protected SolidBrush FieldOfViewBrush { get; set; } = new SolidBrush(Color.FromArgb(50, 20, 175, 200));
        protected SolidBrush SuccessBoundsBrush = new SolidBrush(Color.FromArgb(64, 0, 255, 0));
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool LinkFPSAndTPS
        {
            get
            {
                return _linkFPSAndTPS;
            }
            set
            {
                _linkFPSAndTPS = value;
                SimulationFPS = SimulationTPS;
                chkLinkFpsAndTps.Checked = _linkFPSAndTPS;
            }
        }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool ShowTreeColorByAge
        {
            get
            {
                return _showTreeColorByAge;
            }
            set
            {
                _showTreeColorByAge = value;
                chkShowTreeColorByAge.Checked = _showTreeColorByAge;
            }
        }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsUIVisible
        {
            get
            {
                return _isUIVisible;
            }
            set
            {
                _isUIVisible = value;
                chkShowUI.Checked = _isUIVisible;
                ToggleUI(_isUIVisible);
            }
        }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool UseGenerations
        {
            get
            {
                return MainGameHost.UseGenerations;
            }
            set
            {
                MainGameHost.UseGenerations = value;
                chkUseGenerations.Checked = value;
                ToggleGenerationsUI(value && IsUIVisible);
            }
        }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool UseSuccessBounds
        {
            get
            {
                return MainGameHost.UseSuccessBounds;
            }
            set
            {
                MainGameHost.UseSuccessBounds = value;
                chkUseSuccessBounds.Checked = value;
            }
        }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IRectangle SuccessBounds
        {
            get
            {
                return MainGameHost.SuccessBounds;
            }
            set
            {
                MainGameHost.SuccessBounds = value;
                nudSuccessBoundsX.Value = (decimal)value.X;
                nudSuccessBoundsY.Value = (decimal)value.Y;
                nudSuccessBoundsWidth.Value = (decimal)value.Width;
                nudSuccessBoundsHeight.Value = (decimal)value.Height;
            }
        }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public int StepsPerGeneration
        {
            get
            {
                return MainGameHost.StepsPerGeneration;
            }
            set
            {
                MainGameHost.StepsPerGeneration = value;
                nudStepsPerGeneration.Value = value;
            }
        }
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public float MinSuccessfulCreatureFitness
        {
            get
            {
                return MainGameHost.MinimumSuccessfulCreatureFitness;
            }
            set
            {
                MainGameHost.MinimumSuccessfulCreatureFitness = value;
                nudMinSuccessfulCreatureFitness.Value = (decimal)value;
            }
        }
        #endregion

        #region Constructors
        static MainForm()
        {
            IgnorePropertiesContractResolver ignorePropertiesContractResolver = new IgnorePropertiesContractResolver(nameof(GraphicalStep<GraphicalCreature, GraphicalFood, GraphicalTree>.VisibleCreaturesDict), nameof(GraphicalStep<GraphicalCreature, GraphicalFood, GraphicalTree>.VisibleFoodDict), nameof(GraphicalStep<GraphicalCreature, GraphicalFood, GraphicalTree>.CreatureToCachedAreaDict), nameof(GraphicalStep<GraphicalCreature, GraphicalFood, GraphicalTree>.FoodToCachedAreaDict));

            SaveStepSerializerSettings = new JsonSerializerSettings() { Formatting = Formatting.Indented, ContractResolver = ignorePropertiesContractResolver };
            LoadStepSerializerSettings = new JsonSerializerSettings() { ContractResolver = ignorePropertiesContractResolver };
        }
        public MainForm()
        {
            InitializeComponent();
            DoubleBuffered = true;
        }
        #endregion

        #region Methods
        public async Task BenchmarkSteps(int numberOfStepsToBenchmark)
        {
            SimulationRunning = false;

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            await Task.Factory.StartNew(() =>
            {
                for (int i = 0; i < numberOfStepsToBenchmark; i++)
                {
                    UpdateSimulation();
                }
            }, TaskCreationOptions.LongRunning);

            stopWatch.Stop();

            MessageBox.Show($"Time taken for {numberOfStepsToBenchmark} steps: {stopWatch.ElapsedMilliseconds / 1000d}s");
        }
        public GraphicalStep<GraphicalCreature, GraphicalFood, GraphicalTree> LoadSavedStep(string filePath)
        {
            string serializedStep = File.ReadAllText(filePath);
            GraphicalStep<GraphicalCreature, GraphicalFood, GraphicalTree> savedStep = JsonConvert.DeserializeObject<GraphicalStep<GraphicalCreature, GraphicalFood, GraphicalTree>>(serializedStep, LoadStepSerializerSettings);

            foreach (var creature in savedStep.Creatures)
            {
                foreach (var nodeId in creature.Brain.NodeIdsToNodesDict.Keys.ToList())
                {
                    if (creature.Brain.NodeIdsToNodesDict[nodeId].CreatureInput != null && Enum.GetName<CreatureInput>(creature.Brain.NodeIdsToNodesDict[nodeId].CreatureInput.Value) == null)
                    {
                        creature.Brain.RemoveNode(nodeId, true);
                    }
                }

                foreach (var nodeId in creature.Brain.NodeIdsToNodesDict.Keys.ToList())
                {
                    if (creature.Brain.NodeIdsToNodesDict[nodeId].CreatureAction != null && Enum.GetName<CreatureAction>(creature.Brain.NodeIdsToNodesDict[nodeId].CreatureAction.Value) == null)
                    {
                        creature.Brain.RemoveNode(nodeId, true);
                    }
                }
            }

            return savedStep;
        }
        public void Reset()
        {
            MainGameHost.WorldBounds = new Core.Models.Rectangle(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);
            SuccessBounds = MainGameHost.SuccessBounds;
            BestCreatureNetworkViewerForm.NetworkViewer.CreaturesBrainOutput = null;
            BestCreatureNetworkViewerForm.NetworkViewer.NeuralNetwork = null;

            SelectedCreatureNetworkViewerForm.NetworkViewer.CreaturesBrainOutput = null;
            SelectedCreatureNetworkViewerForm.NetworkViewer.NeuralNetwork = null;

            PreviousStepResult = new StepResult<GraphicalCreature>(new ConcurrentQueue<StepAction<GraphicalCreature>>());

            if (UseGenerations)
            {
                MainGameHost.FailGeneration();
            }
            else
            {
                MainGameHost.ResetStep(MainGameHost.GenerateCreatures(), MainGameHost.GenerateFood(), MainGameHost.GenerateTrees());
            }

            FailedRunsUptimes.Clear();
            CurrentRunTicksElapsed = 0;
        }
        public static Core.Models.Rectangle GetCenteredRectangle(IRectangle rectangle, IRectangle rectangleToCenterIn)
        {
            float MiddleWorldBoundsX = Globals.MiddleX(rectangleToCenterIn.X, rectangleToCenterIn.Width);
            float MiddleWorldBoundsY = Globals.MiddleX(rectangleToCenterIn.Y, rectangleToCenterIn.Height);

            return new Core.Models.Rectangle(MiddleWorldBoundsX - rectangle.Width / 2, MiddleWorldBoundsY - rectangle.Height / 2, rectangle.Width, rectangle.Height);
        }
        public void FailRun()
        {
            CleanForNewRunOrGeneration();
            MainGameHost.ResetStep(MainGameHost.GenerateCreatures
                (), MainGameHost.GenerateFood(), MainGameHost.GenerateTrees());
            FailedRunsUptimes.Add(TimeSpan.FromMilliseconds(CurrentRunTicksElapsed * SimulationMspt));
            CurrentRunTicksElapsed = 0;
        }
        private void CenterSuccessBounds()
        {
            Core.Models.Rectangle newSuccessBounds = GetCenteredRectangle(MainGameHost.SuccessBounds, MainGameHost.WorldBounds);
            SuccessBounds = newSuccessBounds;
            //MainGameHost.CurrentStep.SuccessBounds = newSuccessBounds;
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
        }
        private void MainForm_Paint(object sender, PaintEventArgs e)
        {
            UpdateUIText();

            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;

            if (UseSuccessBounds)
            {
                e.Graphics.FillRectangle(SuccessBoundsBrush, SuccessBounds.X, SuccessBounds.Y, SuccessBounds.Width, SuccessBounds.Height);
            }

            foreach (var creature in MainGameHost.CurrentStep.Creatures)
            {
                Color creatureColor = creature.Color;
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

            foreach (var tree in MainGameHost.CurrentStep.Trees)
            {
                Color treeColorToUse;

                if (ShowTreeColorByAge)
                {
                    int treeR = (int)(80 * ((float)tree.Age / tree.MaxAge));
                    int treeG = (int)Globals.Map(170 * ((float)tree.Age / tree.MaxAge), 0, 170, 170, 40);
                    int treeB = (int)(10 * ((float)tree.Age / tree.MaxAge));

                    treeColorToUse = Color.FromArgb(tree.Color.A, treeR, treeG, treeB);
                }
                else
                {
                    treeColorToUse = tree.Color;
                }

                using (SolidBrush brush = new SolidBrush(treeColorToUse))
                {
                    e.Graphics.FillEllipse(brush, tree.X, tree.Y, tree.Size, tree.Size);
                }
            }

            foreach (var food in MainGameHost.CurrentStep.Food)
            {
                using (SolidBrush brush = new SolidBrush(food.Color))
                {
                    e.Graphics.FillEllipse(brush, food.X, food.Y, food.Size, food.Size);
                }
            }

            foreach (var creature in MainGameHost.CurrentStep.Creatures)
            {
                bool drawFieldOfView = creature == MainGameHost.SelectedCreature || (creature == MainGameHost.BestCreature && BestCreatureNetworkViewerForm?.Visible == true);

                if (drawFieldOfView)
                {
                    List<GraphicalCreature> visibleCreaturesOrderedByDistance = MainGameHost.CurrentStep.Creatures.Where(x =>
                    {
                        if (Globals.GetDistanceFrom(creature.MX, creature.MY, x.MX, x.MY) <= creature.SightRange && x != creature)
                        {
                            float angleFromSourceToTarget = Globals.GetAngleBetweenF(creature.MX, creature.MY, x.MX, x.MY);

                            if (Math.Abs(Globals.AngleDifference(creature.ForwardAngle, -angleFromSourceToTarget)) <= (creature.FieldOfView / 2))
                            {
                                return true;
                            }
                        }

                        return false;
                    }).OrderBy(x => Globals.GetDistanceFrom(creature.MX, creature.MY, x.MX, x.MY)).ToList();

                    foreach (var seenCreature in visibleCreaturesOrderedByDistance)
                    {
                        using (Pen pen = new Pen(Color.FromArgb(200, 32, 32), 2))
                        {
                            e.Graphics.DrawEllipse(pen, seenCreature.X, seenCreature.Y, seenCreature.Size, seenCreature.Size);
                        }
                    }

                    GraphicalCreature closestVisibleCreature = visibleCreaturesOrderedByDistance.FirstOrDefault();

                    if (closestVisibleCreature != null)
                    {
                        float angleFromClosestVisibleCreature = Globals.GetAngleBetweenF(creature.MX, creature.MY, closestVisibleCreature.MX, closestVisibleCreature.MY);

                        using (Pen pen = new Pen(Color.FromArgb(255, 255, 32), 2))
                        {
                            e.Graphics.DrawEllipse(pen, closestVisibleCreature.X, closestVisibleCreature.Y, closestVisibleCreature.Size, closestVisibleCreature.Size);
                        }

                        e.Graphics.FillPath(FieldOfViewBrush, CreateFieldOfViewPath(creature.MX, creature.MY, closestVisibleCreature.Size + 2, creature.SightRange, Globals.Angle180RangeTo360Range(angleFromClosestVisibleCreature)));
                    }

                    e.Graphics.FillPath(FieldOfViewBrush, CreateFieldOfViewPath(creature.MX, creature.MY, creature.FieldOfView, creature.SightRange, creature.ForwardAngle));

                    PointF forwardDirectionLineTarget = Globals.GetAngledLineTarget(creature.MX, creature.MY, creature.SightRange / 4, creature.ForwardAngle);
                    e.Graphics.DrawLine(FieldOfViewPen, creature.MX, creature.MY, forwardDirectionLineTarget.X, forwardDirectionLineTarget.Y);
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
            int generationCount = MainGameHost.GenerationCount;

            PreviousStepResult = MainGameHost.NextStep(PreviousStepResult.CalculatedActions, true, true, GatherStepInfoForAllCreatures, GatherStepInfoForAllCreatures);

            if (generationCount != MainGameHost.GenerationCount)
            {
                if (generationCount == 0)
                {
                    GenerationFailed();
                }
                else
                {
                    GenerationEnded();
                }
            }

            SelectedCreatureNetworkViewerForm.NetworkViewer.CreaturesBrainOutput = PreviousStepResult.CreaturesBrainOutputs;
            BestCreatureNetworkViewerForm.NetworkViewer.CreaturesBrainOutput = PreviousStepResult.CreaturesBrainOutputs;

            CurrentRunTicksElapsed += 1;

            if (!UseGenerations && CurrentRunTicksElapsed % 500 == 0 && MainGameHost.CurrentStep.Creatures.All(x => x.IsDead))
            {
                FailRun();
            }
        }
        public void GenerationEnded()
        {
            CleanForNewRunOrGeneration();
            CurrentRunTicksElapsed = 0;
        }
        public void GenerationFailed()
        {
            CleanForNewRunOrGeneration();
            CurrentRunTicksElapsed = 0;
        }
        private void CleanForNewRunOrGeneration()
        {
            BestCreatureNetworkViewerForm.NetworkViewer.CreaturesBrainOutput = null;
            BestCreatureNetworkViewerForm.NetworkViewer.NeuralNetwork = null;

            SelectedCreatureNetworkViewerForm.NetworkViewer.CreaturesBrainOutput = null;
            SelectedCreatureNetworkViewerForm.NetworkViewer.NeuralNetwork = null;

            PreviousStepResult = new StepResult<GraphicalCreature>(new ConcurrentQueue<StepAction<GraphicalCreature>>());
        }
        private void MainForm_Load(object sender, EventArgs e)
        {
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
            BestCreatureNetworkViewerForm.NetworkViewer.DrawTimer.Interval = DrawTimer.Interval;

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
            SelectedCreatureNetworkViewerForm.NetworkViewer.DrawTimer.Interval = DrawTimer.Interval;

            MainGameHost = new GraphicalGameHost<GraphicalStep<GraphicalCreature, GraphicalFood, GraphicalTree>, GraphicalCreature, GraphicalFood, GraphicalTree>();
            MainGameHost.BestCreatureChanged += MainGameHost_BestCreatureChanged;
            MainGameHost.SelectedCreatureChanged += MainGameHost_SelectedCreatureChanged;

            InitDefaults();
            Reset();
            CenterSuccessBounds();
        }
        private void InitDefaults()
        {
            GatherStepInfoForAllCreatures = !GatherStepInfoForAllCreatures;
            GatherStepInfoForAllCreatures = !GatherStepInfoForAllCreatures;

            LinkFPSAndTPS = !LinkFPSAndTPS;
            LinkFPSAndTPS = !LinkFPSAndTPS;

            IsUIVisible = !IsUIVisible;
            IsUIVisible = !IsUIVisible;

            ShowTreeColorByAge = !ShowTreeColorByAge;
            ShowTreeColorByAge = !ShowTreeColorByAge;

            int oldTPS = SimulationTPS;
            SimulationTPS = 5;
            SimulationTPS = oldTPS;

            int oldFPS = SimulationFPS;
            SimulationFPS = 5;
            SimulationFPS = oldFPS;

            UseSuccessBounds = !UseSuccessBounds;
            UseSuccessBounds = !UseSuccessBounds;
            UseGenerations = !UseGenerations;
            UseGenerations = !UseGenerations;

            float oldMinSuccessfulCreatureFitness = MinSuccessfulCreatureFitness;
            MinSuccessfulCreatureFitness = 1;
            MinSuccessfulCreatureFitness = oldMinSuccessfulCreatureFitness;

            int oldStepsPerGeneration = StepsPerGeneration;
            StepsPerGeneration = 5;
            StepsPerGeneration = oldStepsPerGeneration;
        }
        private void ToggleUI(bool isVisible)
        {
            StartButton.Visible = isVisible;
            StopButton.Visible = isVisible;
            ResetButton.Visible = isVisible;
            chkGatherStepInfoForAllCreatures.Visible = isVisible;
            btnTrackBestCreature.Visible = isVisible;
            btnFastFoward.Visible = isVisible;
            btnBenchmark.Visible = isVisible;
            btnSaveCurrentStep.Visible = isVisible;
            btnLoadStep.Visible = isVisible;
            lblSimulationTPS.Visible = isVisible;
            nudSimulationTPS.Visible = isVisible;
            lblSimulationFPS.Visible = isVisible;
            nudSimulationFPS.Visible = isVisible;
            chkLinkFpsAndTps.Visible = isVisible;
            btnUpdateWorldBounds.Visible = isVisible;
            chkShowTreeColorByAge.Visible = isVisible;
            chkUseGenerations.Visible = isVisible;
            ToggleGenerationsUI(isVisible && chkUseGenerations.Checked);
        }
        private void ToggleGenerationsUI(bool isVisible)
        {
            chkUseSuccessBounds.Visible = isVisible;
            lblSuccessBoundsX.Visible = isVisible;
            nudSuccessBoundsX.Visible = isVisible;
            lblSuccessBoundsY.Visible = isVisible;
            nudSuccessBoundsY.Visible = isVisible;
            lblSuccessBoundsWidth.Visible = isVisible;
            nudSuccessBoundsWidth.Visible = isVisible;
            lblSuccessBoundsHeight.Visible = isVisible;
            nudSuccessBoundsHeight.Visible = isVisible;
            btnCenterSuccessBounds.Visible = isVisible;
            lblStepsPerGeneration.Visible = isVisible;
            nudStepsPerGeneration.Visible = isVisible;
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

            lblGenerationCount.Text = $"{(UseGenerations ? "Gen" : "Run")} {FailedRunsUptimes.Count + 1}";
            lblGenEndsIn.Text = $"Uptime: {timeInAllRuns:d\\d' 'h\\h' 'm\\m' 's\\.ff\\s}, Failed: {timeInFailedRuns:d\\d' 'h\\h' 'm\\m' 's\\.ff\\s}" +
                $"\nRun {FailedRunsUptimes.Count + 1}, {timeInCurrentRun:d\\d' 'h\\h' 'm\\m' 's\\.ff\\s}, Average: {averageTimePerRun:d\\d' 'h\\h' 'm\\m' 's\\.ff\\s}";
        }
        private void btnLoadStep_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog() { Filter = "JSON Files (*.json)|*.json", DefaultExt = "json" };

            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                GraphicalStep<GraphicalCreature, GraphicalFood, GraphicalTree> savedStep = LoadSavedStep(openFileDialog.FileName);

                PreviousStepResult.CreaturesBrainOutputs.Clear();
                PreviousStepResult.CalculatedActions.Clear();
                MainGameHost.ConnectionWeightBound = savedStep.ConnectionWeightBound;
                MainGameHost.CreatureConnectionsMinMax = savedStep.CreatureConnectionsMinMax;
                MainGameHost.MaxCreatureProcessNodes = savedStep.MaxCreatureProcessNodes;
                MainGameHost.LoopWorldBounds = savedStep.LoopWorldBounds;
                MainGameHost.WorldBounds = savedStep.WorldBounds;
                MainGameHost.CreatureOffspringColor = savedStep.CreatureOffspringColor;
                MainGameHost.ResetStep(MainGameHost.GenerateCreatures(savedStep.Creatures), MainGameHost.GenerateFood(savedStep.Food), MainGameHost.GenerateTrees(savedStep.Trees == null || savedStep.Trees.IsEmpty ? Enumerable.Empty<GraphicalTree>() : savedStep.Trees));
                MessageBox.Show("Step Loaded Successfully.");
            }
        }
        private void btnSaveCurrentStep_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog() { Filter = "JSON Files (*.json)|*.json", DefaultExt = "json" };

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                string serializedStep = JsonConvert.SerializeObject(MainGameHost.CurrentStep, SaveStepSerializerSettings);
                File.WriteAllText(saveFileDialog.FileName, serializedStep);
                MessageBox.Show("Step Saved Successfully.");
            }
        }
        private async void btnBenchmark_Click(object sender, EventArgs e)
        {
            await BenchmarkSteps(200);
        }
        private static GraphicsPath CreateFieldOfViewPath(float locationX, float locationY, float fieldOfView, float distance, float angle)
        {
            GraphicsPath path = new GraphicsPath();
            PointF leftLineTarget = Globals.GetAngledLineTarget(locationX, locationY, distance, angle - fieldOfView / 2);
            //PointF rightLineTarget = GetAngledLineTarget(locationX, locationY, distance, angle + fieldOfView / 2);
            path.AddLine(locationX, locationY, leftLineTarget.X, leftLineTarget.Y);
            path.AddArc(locationX - distance, locationY - distance, distance * 2, distance * 2, angle - fieldOfView / 2, fieldOfView);
            //path.AddLine(locationX, locationY, rightLineTarget.X, rightLineTarget.Y);
            path.CloseFigure();

            return path;
        }
        private void nudSimulationTPS_ValueChanged(object sender, EventArgs e)
        {
            SimulationTPS = (int)nudSimulationTPS.Value;
        }
        private void nudSimulationFPS_ValueChanged(object sender, EventArgs e)
        {
            SimulationFPS = (int)nudSimulationFPS.Value;
        }
        private void DrawTimer_Tick(object sender, EventArgs e)
        {
            Invalidate();
        }
        private void UpdateIsRunningText()
        {
            if (SimulationRunning)
            {
                lblSimulationRunning.Text = IsInFastMode ? "Running (Fast)" : "Running";
            }
            else
            {
                lblSimulationRunning.Text = IsInFastMode ? "Stopped (Fast)" : "Stopped";
            }
        }
        private void chkLinkFpsAndTps_CheckedChanged(object sender, EventArgs e)
        {
            LinkFPSAndTPS = chkLinkFpsAndTps.Checked;
        }
        private void chkGatherStepInfoForAllCreatures_CheckedChanged(object sender, EventArgs e)
        {
            GatherStepInfoForAllCreatures = chkGatherStepInfoForAllCreatures.Checked;
        }
        private void chkShowUI_CheckedChanged(object sender, EventArgs e)
        {
            IsUIVisible = chkShowUI.Checked;
        }
        private void btnUpdateWorldBounds_Click(object sender, EventArgs e)
        {
            Core.Models.Rectangle newWorldBounds = new Core.Models.Rectangle(ClientRectangle.X, ClientRectangle.Y, ClientRectangle.Width, ClientRectangle.Height);
            MainGameHost.WorldBounds = newWorldBounds;
            MainGameHost.CurrentStep.WorldBounds = newWorldBounds;
        }
        private void chkShowTreeColorByAge_CheckedChanged(object sender, EventArgs e)
        {
            ShowTreeColorByAge = chkShowTreeColorByAge.Checked;
        }
        private void chkUseGenerations_CheckedChanged(object sender, EventArgs e)
        {
            UseGenerations = chkUseGenerations.Checked;
        }
        private void btnCenterSuccessBounds_Click(object sender, EventArgs e)
        {
            CenterSuccessBounds();
        }
        private void chkUseSuccessBounds_CheckedChanged(object sender, EventArgs e)
        {
            UseSuccessBounds = chkUseSuccessBounds.Checked;
        }
        private void nudSuccessBoundsX_ValueChanged(object sender, EventArgs e)
        {
            SuccessBounds.X = (float)nudSuccessBoundsX.Value;
        }
        private void nudSuccessBoundsY_ValueChanged(object sender, EventArgs e)
        {
            SuccessBounds.Y = (float)nudSuccessBoundsY.Value;
        }
        private void nudSuccessBoundsWidth_ValueChanged(object sender, EventArgs e)
        {
            SuccessBounds.Width = (float)nudSuccessBoundsWidth.Value;
        }
        private void nudSuccessBoundsHeight_ValueChanged(object sender, EventArgs e)
        {
            SuccessBounds.Height = (float)nudSuccessBoundsHeight.Value;
        }
        private void nudStepsPerGeneration_ValueChanged(object sender, EventArgs e)
        {
            StepsPerGeneration = (int)nudStepsPerGeneration.Value;
        }
        private void nudMinSuccessfulCreatureFitness_ValueChanged(object sender, EventArgs e)
        {
            MinSuccessfulCreatureFitness = (float)nudMinSuccessfulCreatureFitness.Value;
        }
        #endregion
    }
}