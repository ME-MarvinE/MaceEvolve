using MaceEvolve.Core.Models;
using MaceEvolve.SilkGL.JsonContractResolvers;
using MaceEvolve.SilkGL.Models;
using Newtonsoft.Json;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;
using Window = Silk.NET.Windowing.Window;

namespace MaceEvolve.SilkGL
{
    public class MaceEvolveApp
    {
        #region Properties
        public bool IsSimulationRunning { get; set; }
        public Timer PeriodicInfoTimer { get; }
        public bool AutoClearConsole { get; set; }
        public bool PeriodicInfo { get; set; } = true;
        protected static JsonSerializerSettings SaveStepSerializerSettings { get; }
        protected static JsonSerializerSettings LoadStepSerializerSettings { get; }
        public GL gl { get; set; }
        public IWindow MainWindow { get; set; }
        public int SimulationTPS { get; set; }
        public int CurrentRunTicksElapsed { get; set; }
        public long AllRunsElapsed { get; set; }
        public bool GatherStepInfoForAllCreatures { get; set; }
        public bool IsInFastMode { get; set; }
        public Color BackgroundColor { get; set; }
        public GraphicalGameHost<GraphicalStep<GraphicalCreature, GraphicalFood>, GraphicalCreature, GraphicalFood> MainGameHost { get; set; }
        public IWindow BestCreatureNetworkViewerWindow { get; set; }
        public float SimulationMspt
        {
            get
            {
                return (1f / SimulationTPS) * 1000;
            }
        }
        public List<TimeSpan> FailedRunsUptimes { get; set; } = new List<TimeSpan>();
        public StepResult<GraphicalCreature> PreviousStepResult { get; set; }
        public double LastUpdateDeltaTime { get; set; }
        public double LastRenderDeltaTime { get; set; }
        #endregion

        #region Constructors
        static MaceEvolveApp()
        {
            IgnorePropertiesContractResolver ignorePropertiesContractResolver = new IgnorePropertiesContractResolver(nameof(GraphicalStep<GraphicalCreature, GraphicalFood>.VisibleCreaturesDict), nameof(GraphicalStep<GraphicalCreature, GraphicalFood>.VisibleFoodDict), nameof(GraphicalStep<GraphicalCreature, GraphicalFood>.CreatureToCachedAreaDict), nameof(GraphicalStep<GraphicalCreature, GraphicalFood>.FoodToCachedAreaDict));

            SaveStepSerializerSettings = new JsonSerializerSettings() { Formatting = Formatting.Indented, ContractResolver = ignorePropertiesContractResolver };
            LoadStepSerializerSettings = new JsonSerializerSettings() { ContractResolver = ignorePropertiesContractResolver };
        }
        public MaceEvolveApp()
        {
            //_graphics = new GraphicsDeviceManager(this);

            //if (GraphicsDevice == null)
            //{
            //    _graphics.ApplyChanges();
            //}

            //_graphics.PreferredBackBufferWidth = 800 - 17;
            //_graphics.PreferredBackBufferHeight = 700 - 40;

            //_graphics.ApplyChanges();

            //MainWindow.KeyDown += Window_KeyDown;

            //Content.RootDirectory = "Content";
            //IsMouseVisible = true;

            MainGameHost = new GraphicalGameHost<GraphicalStep<GraphicalCreature, GraphicalFood>, GraphicalCreature, GraphicalFood>();

            SimulationTPS = 60;
            PeriodicInfoTimer = new Timer(1000) { Enabled = PeriodicInfo };
            PeriodicInfoTimer.Elapsed += PeriodicInfoTimer_Elapsed;

            WindowOptions options = WindowOptions.Default;
            options.Size = new Vector2D<int>(800, 600);
            options.Title = "Mace Evolve";
            options.UpdatesPerSecond = SimulationTPS;

            MainWindow = Window.Create(options);
            BackgroundColor = Color.FromArgb(32, 32, 32);

            MainWindow.Load += MainWindow_Load;
            MainWindow.Update += MainWindow_Update;
            MainWindow.Render += MainWindow_Render;

            MainWindow.Run();
        }
        #endregion

        #region Methods
        private void MainWindow_Render(double deltaTime)
        {
            LastRenderDeltaTime = deltaTime;

            gl.ClearColor(BackgroundColor);
            gl.Clear(ClearBufferMask.ColorBufferBit);

            double millisecondsInFailedRuns = 0;

            foreach (var failedRunTimeSpan in FailedRunsUptimes)
            {
                millisecondsInFailedRuns += failedRunTimeSpan.TotalMilliseconds;
            }

            TimeSpan timeInCurrentRun = TimeSpan.FromMilliseconds(CurrentRunTicksElapsed * SimulationMspt);
            TimeSpan timeInFailedRuns = TimeSpan.FromMilliseconds(millisecondsInFailedRuns);
            TimeSpan timeInAllRuns = timeInFailedRuns.Add(timeInCurrentRun);
            TimeSpan averageTimePerRun = TimeSpan.FromMilliseconds(FailedRunsUptimes.Count == 0 ? 0 : FailedRunsUptimes.Average(x => x.TotalMilliseconds));

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

                //_spriteBatch.DrawCircle(creature.MX, creature.MY, creature.Size, 18, creatureColor, creature.Size);

                if (creatureRingColor != null)
                {
                    // _spriteBatch.DrawCircle(creature.MX, creature.MY, creature.Size + 2, 18, creatureRingColor.Value, creature.Size * 0.3f);
                }
            }
            foreach (var food in MainGameHost.CurrentStep.Food)
            {
                //_spriteBatch.DrawCircle(food.MX, food.MY, food.Size, 18, food.Color, food.Size);
            }

            if (IsSimulationRunning)
            {
                //_spriteBatch.DrawString(BigUIFont, IsInFastMode ? "Running (Fast)" : "Running", new Vector2(10, 15), Color.White);
            }
            else
            {
                //_spriteBatch.DrawString(BigUIFont, IsInFastMode ? "Stopped (Fast)" : "Stopped", new Vector2(10, 15), Color.White);
            }

            //_spriteBatch.DrawString(BigUIFont, $"Run {FailedRunsUptimes.Count + 1}", new Vector2(10, 45), Color.White);
            //_spriteBatch.DrawString(UIFont, $"Uptime: {timeInAllRuns:d\\d' 'h\\h' 'm\\m' 's\\.ff\\s}, Failed: {timeInFailedRuns:d\\d' 'h\\h' 'm\\m' 's\\.ff\\s}" +
            //    $"\nRun {FailedRunsUptimes.Count + 1}, {timeInCurrentRun:d\\d' 'h\\h' 'm\\m' 's\\.ff\\s}, Average: {averageTimePerRun:d\\d' 'h\\h' 'm\\m' 's\\.ff\\s}", new Vector2(10, 75), Color.White);

            //_spriteBatch.DrawString(UIFont, $"Gather Step Info For All Creatures: {(GatherStepInfoForAllCreatures ? "Enabled" : "Disabled")}", new Vector2(10, 115), Color.White);

            //_spriteBatch.End();
        }
        private void MainWindow_Update(double deltaTime)
        {
            LastUpdateDeltaTime = deltaTime;

            if (!IsInFastMode && IsSimulationRunning)
            {
                UpdateSimulation();
            }
        }
        private void MainWindow_Load()
        {
            gl = GL.GetApi(MainWindow);
            Reset();

            IInputContext input = MainWindow.CreateInput();

            foreach (var keyboard in input.Keyboards)
            {
                keyboard.KeyDown += Keyboard_KeyDown;
            }

            ListCommands();
            ListSimulationInfo();
        }
        public void UpdateSimulation()
        {
            MainGameHost.CreatureOffspringColor = Color.FromArgb(64, 64, MaceRandom.Current.Next(256));
            PreviousStepResult = MainGameHost.NextStep(PreviousStepResult.CalculatedActions, true, true, GatherStepInfoForAllCreatures, GatherStepInfoForAllCreatures);

            CurrentRunTicksElapsed += 1;

            if (CurrentRunTicksElapsed % 500 == 0 && MainGameHost.CurrentStep.Creatures.All(x => x.IsDead))
            {
                FailRun();
            }
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
                creature.Color = Color.FromArgb(64, 64, MaceRandom.Current.Next(256));
            }

            return creatures;
        }
        public void Reset()
        {
            MainGameHost.WorldBounds = new Core.Models.Rectangle(0, 0, MainWindow.Size.X, MainWindow.Size.Y);

            PreviousStepResult = new StepResult<GraphicalCreature>(new ConcurrentQueue<StepAction<GraphicalCreature>>());
            MainGameHost.ResetStep(GenerateCreatures(), GenerateFood());

            FailedRunsUptimes.Clear();
            CurrentRunTicksElapsed = 0;
        }
        public void FailRun()
        {
            PreviousStepResult = new StepResult<GraphicalCreature>(new ConcurrentQueue<StepAction<GraphicalCreature>>());
            MainGameHost.ResetStep(GenerateCreatures(), GenerateFood());

            FailedRunsUptimes.Add(TimeSpan.FromMilliseconds(CurrentRunTicksElapsed * SimulationMspt));
            CurrentRunTicksElapsed = 0;
        }
        private async void Keyboard_KeyDown(IKeyboard keyboard, Key key, int keyCode)
        {
            ExecuteKeyAction(key);
        }
        public void ExecuteKeyAction(Key key)
        {
            if (AutoClearConsole)
            {
                Console.Clear();
            }
            else
            {
                Console.WriteLine("");
            }

            switch (key)
            {
                case Key.Escape:
                    MainWindow.Close();
                    break;
                case Key.P:
                    IsSimulationRunning = !IsSimulationRunning;
                    break;
                case Key.R:
                    Reset();
                    Console.WriteLine("Simulation has been reset.");
                    break;
                case Key.F:
                    IsInFastMode = true;

                    Task.Factory.StartNew(() =>
                    {
                        while (IsSimulationRunning)
                        {
                            UpdateSimulation();
                        }

                        IsInFastMode = false;
                    }, TaskCreationOptions.LongRunning);
                    break;
                case Key.G:
                    GatherStepInfoForAllCreatures = !GatherStepInfoForAllCreatures;
                    break;
                case Key.H:
                    ListCommands();
                    break;
                case Key.I:
                    ListSimulationInfo();
                    break;
                case Key.C:
                    Console.Clear();
                    break;
                case Key.A:
                    AutoClearConsole = !AutoClearConsole;
                    break;
                case Key.O:
                    PeriodicInfo = !PeriodicInfo;
                    PeriodicInfoTimer.Enabled = PeriodicInfo;
                    break;
                case Key.S:
                    {
                        Console.WriteLine("Enter file path");
                        string filePath = Console.ReadLine();

                        Console.WriteLine("Saving Current Step...");
                        SaveStep(MainGameHost.CurrentStep, filePath);
                        Console.WriteLine("Step Saved Successfully.");
                    }

                    break;
                case Key.L:
                    {
                        Console.WriteLine("Enter file path");
                        string savedStepFilePath = Console.ReadLine();

                        if (Path.Exists(savedStepFilePath))
                        {
                            Console.WriteLine("Loading Step...");
                            GraphicalStep<GraphicalCreature, GraphicalFood> savedStep = LoadSavedStep(savedStepFilePath);

                            PreviousStepResult.CreaturesBrainOutputs.Clear();
                            PreviousStepResult.CalculatedActions.Clear();
                            MainGameHost.ConnectionWeightBound = savedStep.ConnectionWeightBound;
                            MainGameHost.MinCreatureConnections = savedStep.MinCreatureConnections;
                            MainGameHost.MaxCreatureConnections = savedStep.MaxCreatureConnections;
                            MainGameHost.MaxCreatureProcessNodes = savedStep.MaxCreatureProcessNodes;
                            MainGameHost.LoopWorldBounds = savedStep.LoopWorldBounds;
                            MainGameHost.WorldBounds = savedStep.WorldBounds;
                            MainGameHost.ResetStep(savedStep.Creatures, savedStep.Food);

                            Console.WriteLine("Step Loaded Successfully.");
                        }
                        else
                        {
                            Console.WriteLine("Unable to locate file.");
                        }
                    }

                    break;
                case Key.B:
                    BenchmarkSteps(200);
                    break;
                case Key.T:
                    UpdateSimulation();
                    break;
            }
        }
        public static void ListCommands()
        {
            Console.WriteLine("");
            Console.WriteLine("--------------------------Commands-----------------------");
            Console.WriteLine("P = Start/Pause");
            Console.WriteLine("R = Reset");
            Console.WriteLine("G = Toggle Gather step info for all creatures");
            Console.WriteLine("Escape = Exit");
            Console.WriteLine("H = List Commands");
            Console.WriteLine("I = List Simulation Info");
            Console.WriteLine("C = Clear Console");
            Console.WriteLine("A = Toggle Auto Clear Console");
            Console.WriteLine("O = Toggle Periodic Info");
            Console.WriteLine("S = Save Creatures");
            Console.WriteLine("L = Load Creatures");
            Console.WriteLine("B = Benchmark Current Step");
            Console.WriteLine("---------------------------------------------------------");
        }
        public void ListSimulationInfo()
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

            Console.WriteLine("");
            Console.WriteLine("--------------------------Simulation Info----------------");
            Console.WriteLine(IsSimulationRunning ? "Running" : "Stopped");
            Console.WriteLine($"Uptime: {timeInAllRuns:d\\d' 'h\\h' 'm\\m' 's\\.ff\\s}, Failed: {timeInFailedRuns:d\\d' 'h\\h' 'm\\m' 's\\.ff\\s}" +
                $"\nRun {FailedRunsUptimes.Count + 1}, {timeInCurrentRun:d\\d' 'h\\h' 'm\\m' 's\\.ff\\s}, Average: {averageTimePerRun:d\\d' 'h\\h' 'm\\m' 's\\.ff\\s}");
            Console.WriteLine("-----------------------");
            Console.WriteLine($"{MainGameHost.CurrentStep.Creatures.Count} Creatures");
            Console.WriteLine($"{MainGameHost.CurrentStep.Food.Count} Food");
            Console.WriteLine($"Fast Mode: {IsInFastMode}");
            Console.WriteLine($"Gather Step Info For All Creatures: {GatherStepInfoForAllCreatures}");
            Console.WriteLine($"Auto Clear Console: {AutoClearConsole}");
            Console.WriteLine($"Periodic Info: {PeriodicInfo}");
            Console.WriteLine($"Updates Per Second: {1 / LastUpdateDeltaTime:0.#}");
            Console.WriteLine($"Target Updates Per Second: {SimulationTPS}");
            Console.WriteLine($"Frames Per Second: {1 / LastRenderDeltaTime:0.#}");
            Console.WriteLine("---------------------------------------------------------");
        }
        public void SaveStep(GraphicalStep<GraphicalCreature, GraphicalFood> step, string filePath)
        {
            string directoryName = Path.GetDirectoryName(filePath);

            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }

            string serializedStep = JsonConvert.SerializeObject(step, SaveStepSerializerSettings);
            File.WriteAllText(filePath, serializedStep);
        }
        public GraphicalStep<GraphicalCreature, GraphicalFood> LoadSavedStep(string filePath)
        {
            string serializedStep = File.ReadAllText(filePath);
            GraphicalStep<GraphicalCreature, GraphicalFood> savedStep = JsonConvert.DeserializeObject<GraphicalStep<GraphicalCreature, GraphicalFood>>(serializedStep, LoadStepSerializerSettings);
            return savedStep;
        }
        private void PeriodicInfoTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            if (IsSimulationRunning)
            {
                ListSimulationInfo();
            }
        }
        public async void BenchmarkSteps(int numberOfStepsToBenchmark)
        {
            IsSimulationRunning = false;

            Console.WriteLine("Benchmarking...");

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

            Console.WriteLine("--------------------------Benchmark----------------------");
            Console.WriteLine($"Time taken for {numberOfStepsToBenchmark} steps: {stopWatch.ElapsedMilliseconds / 1000d}s");
            Console.WriteLine("---------------------------------------------------------");
        }
        #endregion
    }
}