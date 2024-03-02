using MaceEvolve.Core.Models;
using MaceEvolve.Console.JsonContractResolvers;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace MaceEvolve.Console
{
    public class Program
    {
        #region Properties
        public bool IsSimulationRunning { get; set; }
        public bool GatherStepInfoForAllCreatures { get; set; }
        public GameHost<Step<Creature, Food>, Creature, Food> MainGameHost { get; }
        public List<TimeSpan> FailedRunsUptimes { get; set; } = new List<TimeSpan>();
        public int CurrentRunTicksElapsed { get; set; }
        public float SimulationMspt
        {
            get
            {
                return (1f / SimulationTPS) * 1000;
            }
        }
        public int SimulationTPS { get; set; }
        public bool IsInFastMode { get; set; }
        public Timer GameTimer { get; }
        public Timer PeriodicInfoTimer { get; }
        public bool AutoClearConsole { get; set; }
        public bool PeriodicInfo { get; set; } = true;
        public bool GameTimerElapsing { get; set; }
        public StepResult<Creature> PreviousStepResult { get; set; }
        protected static JsonSerializerSettings SaveStepSerializerSettings { get; }
        protected static JsonSerializerSettings LoadStepSerializerSettings { get; }
        #endregion

        #region Constructors
        static Program()
        {
            IgnorePropertiesContractResolver ignorePropertiesContractResolver = new IgnorePropertiesContractResolver(nameof(Step<Creature, Food>.VisibleCreaturesDict), nameof(Step<Creature, Food>.VisibleFoodDict), nameof(Step<Creature, Food>.CreatureToCachedAreaDict), nameof(Step<Creature, Food>.FoodToCachedAreaDict));

            SaveStepSerializerSettings = new JsonSerializerSettings() { Formatting = Formatting.Indented, ContractResolver = ignorePropertiesContractResolver };
            LoadStepSerializerSettings = new JsonSerializerSettings() { ContractResolver = ignorePropertiesContractResolver };
        }
        public Program()
        {
            MainGameHost = new GameHost<Step<Creature, Food>, Creature, Food>();

            SimulationTPS = 60;

            GameTimer = new Timer(SimulationTPS) { Enabled = true };
            GameTimer.Elapsed += GameTimer_Elapsed;
            PeriodicInfoTimer = new Timer(1000) { Enabled = PeriodicInfo };
            PeriodicInfoTimer.Elapsed += PeriodicInfoTimer_Elapsed;

            Reset();
        }
        #endregion

        #region Methods
        static void Main(string[] args)
        {
            Program program = new Program();
            ListCommands();
            program.ListSimulationInfo();

            while (true)
            {
                program.ExecuteKeyAction(System.Console.ReadKey().Key);
            }
        }
        public void ExecuteKeyAction(ConsoleKey key)
        {
            if (AutoClearConsole)
            {
                System.Console.Clear();
            }
            else
            {
                System.Console.WriteLine("");
            }

            switch (key)
            {
                case ConsoleKey.Escape:
                    Environment.Exit(0);
                    break;
                case ConsoleKey.P:
                    IsSimulationRunning = !IsSimulationRunning;
                    break;
                case ConsoleKey.R:
                    Reset();
                    System.Console.WriteLine("Simulation has been reset.");
                    break;
                case ConsoleKey.F:
                    IsInFastMode = true;

                    _ = Task.Factory.StartNew(() =>
                    {
                        while (IsSimulationRunning)
                        {
                            UpdateSimulation();
                        }

                        IsInFastMode = false;
                    }, TaskCreationOptions.LongRunning);
                    break;
                case ConsoleKey.G:
                    GatherStepInfoForAllCreatures = !GatherStepInfoForAllCreatures;
                    break;
                case ConsoleKey.H:
                    ListCommands();
                    break;
                case ConsoleKey.I:
                    ListSimulationInfo();
                    break;
                case ConsoleKey.C:
                    System.Console.Clear();
                    break;
                case ConsoleKey.A:
                    AutoClearConsole = !AutoClearConsole;
                    break;
                case ConsoleKey.O:
                    PeriodicInfo = !PeriodicInfo;
                    PeriodicInfoTimer.Enabled = PeriodicInfo;
                    break;
                case ConsoleKey.S:
                    {
                        System.Console.WriteLine("Enter file path");
                        string filePath = System.Console.ReadLine();

                        System.Console.WriteLine("Saving Current Step...");
                        SaveStep(MainGameHost.CurrentStep, filePath);
                        System.Console.WriteLine("Step Saved Successfully.");
                    }

                    break;
                case ConsoleKey.L:
                    {
                        System.Console.WriteLine("Enter file path");
                        string savedStepFilePath = System.Console.ReadLine();

                        if (Path.Exists(savedStepFilePath))
                        {
                            System.Console.WriteLine("Loading Step...");
                            Step<Creature, Food> savedStep = LoadSavedStep(savedStepFilePath);

                            PreviousStepResult.CreaturesBrainOutputs.Clear();
                            PreviousStepResult.CalculatedActions.Clear();
                            MainGameHost.ConnectionWeightBound = savedStep.ConnectionWeightBound;
                            MainGameHost.MinCreatureConnections = savedStep.MinCreatureConnections;
                            MainGameHost.MaxCreatureConnections = savedStep.MaxCreatureConnections;
                            MainGameHost.MaxCreatureProcessNodes = savedStep.MaxCreatureProcessNodes;
                            MainGameHost.LoopWorldBounds = savedStep.LoopWorldBounds;
                            MainGameHost.WorldBounds = savedStep.WorldBounds;
                            MainGameHost.ResetStep(savedStep.Creatures, savedStep.Food);

                            System.Console.WriteLine("Step Loaded Successfully.");
                        }
                        else
                        {
                            System.Console.WriteLine("Unable to locate file.");
                        }
                    }

                    break;
                case ConsoleKey.B:
                    BenchmarkSteps(200);
                    break;
            }
        }
        public void SaveStep(Step<Creature, Food> step, string filePath)
        {
            string directoryName = Path.GetDirectoryName(filePath);

            if (!Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }

            string serializedStep = JsonConvert.SerializeObject(step, SaveStepSerializerSettings);
            File.WriteAllText(filePath, serializedStep);
        }
        public Step<Creature, Food> LoadSavedStep(string filePath)
        {
            string serializedStep = File.ReadAllText(filePath);
            Step<Creature, Food> savedStep = JsonConvert.DeserializeObject<Step<Creature, Food>>(serializedStep, LoadStepSerializerSettings);
            return savedStep;
        }
        public void UpdateSimulation()
        {
            PreviousStepResult = MainGameHost.NextStep(PreviousStepResult.CalculatedActions, true, true, GatherStepInfoForAllCreatures, GatherStepInfoForAllCreatures);

            CurrentRunTicksElapsed += 1;

            if (CurrentRunTicksElapsed % 500 == 0 && MainGameHost.CurrentStep.Creatures.All(x => x.IsDead))
            {
                FailRun();
            }
        }
        public void FailRun()
        {
            PreviousStepResult = new StepResult<Creature>(new ConcurrentQueue<StepAction<Creature>>());
            MainGameHost.ResetStep(MainGameHost.GenerateCreatures(), MainGameHost.GenerateFood());

            FailedRunsUptimes.Add(TimeSpan.FromMilliseconds(CurrentRunTicksElapsed * SimulationMspt));
            CurrentRunTicksElapsed = 0;
        }
        public void Reset()
        {
            MainGameHost.WorldBounds = new Rectangle(0, 0, 784, 661); //Same value as MaceEvolve.WinForms ClientRectangle
            PreviousStepResult = new StepResult<Creature>(new ConcurrentQueue<StepAction<Creature>>());
            MainGameHost.ResetStep(MainGameHost.GenerateCreatures(), MainGameHost.GenerateFood());

            FailedRunsUptimes.Clear();
            CurrentRunTicksElapsed = 0;
        }
        public static void ListCommands()
        {
            System.Console.WriteLine("");
            System.Console.WriteLine("--------------------------Commands-----------------------");
            System.Console.WriteLine("P = Start/Pause");
            System.Console.WriteLine("R = Reset");
            System.Console.WriteLine("G = Toggle Gather step info for all creatures");
            System.Console.WriteLine("Escape = Exit");
            System.Console.WriteLine("H = List Commands");
            System.Console.WriteLine("I = List Simulation Info");
            System.Console.WriteLine("C = Clear Console");
            System.Console.WriteLine("A = Toggle Auto Clear Console");
            System.Console.WriteLine("O = Toggle Periodic Info");
            System.Console.WriteLine("S = Save Creatures");
            System.Console.WriteLine("L = Load Creatures");
            System.Console.WriteLine("B = Benchmark Current Step");
            System.Console.WriteLine("---------------------------------------------------------");
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

            System.Console.WriteLine("");
            System.Console.WriteLine("--------------------------Simulation Info----------------");
            System.Console.WriteLine(IsSimulationRunning ? "Running" : "Stopped");
            System.Console.WriteLine($"Uptime: {timeInAllRuns:d\\d' 'h\\h' 'm\\m' 's\\.ff\\s}, Failed: {timeInFailedRuns:d\\d' 'h\\h' 'm\\m' 's\\.ff\\s}" +
                $"\nRun {FailedRunsUptimes.Count + 1}, {timeInCurrentRun:d\\d' 'h\\h' 'm\\m' 's\\.ff\\s}, Average: {averageTimePerRun:d\\d' 'h\\h' 'm\\m' 's\\.ff\\s}");
            System.Console.WriteLine("-----------------------");
            System.Console.WriteLine($"{MainGameHost.CurrentStep.Creatures.Count} Creatures");
            System.Console.WriteLine($"{MainGameHost.CurrentStep.Food.Count} Food");
            System.Console.WriteLine($"Fast Mode: {IsInFastMode}");
            System.Console.WriteLine($"Gather Step Info For All Creatures: {GatherStepInfoForAllCreatures}");
            System.Console.WriteLine($"Auto Clear Console: {AutoClearConsole}");
            System.Console.WriteLine($"Periodic Info: {PeriodicInfo}");
            System.Console.WriteLine("---------------------------------------------------------");
        }
        public async void BenchmarkSteps(int numberOfStepsToBenchmark)
        {
            IsSimulationRunning = false;

            System.Console.WriteLine("Benchmarking...");

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

            System.Console.WriteLine("--------------------------Benchmark----------------------");
            System.Console.WriteLine($"Time taken for {numberOfStepsToBenchmark} steps: {stopWatch.ElapsedMilliseconds / 1000d}s");
            System.Console.WriteLine("---------------------------------------------------------");
        }
        private void GameTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            if (!IsInFastMode && IsSimulationRunning && !GameTimerElapsing)
            {
                GameTimerElapsing = true;
                UpdateSimulation();
                GameTimerElapsing = false;
            }
        }
        private void PeriodicInfoTimer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            if (IsSimulationRunning)
            {
                ListSimulationInfo();
            }
        }
        #endregion
    }
}