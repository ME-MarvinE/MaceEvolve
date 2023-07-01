using MaceEvolve.Core.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Timers;

namespace MaceEvolve.Benchmark
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
        #endregion

        #region Constructors
        public Program()
        {
            MainGameHost = new GameHost<Step<Creature, Food>, Creature, Food>
            {
                CreatureSize = 10,
                CreatureSpeed = 2.75f
            };

            float baseFoodSize = MainGameHost.CreatureSize;
            MainGameHost.MinFoodSize = baseFoodSize * 0.2f;
            MainGameHost.MaxFoodSize = baseFoodSize * 1.2f;

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
                program.ExecuteKeyAction(Console.ReadKey().Key);
            }
        }
        public void ExecuteKeyAction(ConsoleKey key)
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
                case ConsoleKey.Escape:
                    Environment.Exit(0);
                    break;
                case ConsoleKey.P:
                    IsSimulationRunning = !IsSimulationRunning;
                    break;
                case ConsoleKey.R:
                    Reset();
                    Console.WriteLine("Simulation has been reset.");
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
                    Console.Clear();
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
                        Console.WriteLine("Enter file path");
                        string filePath = Console.ReadLine();

                        Console.WriteLine("Saving Current Step...");
                        SaveStep(MainGameHost.CurrentStep, filePath);
                        Console.WriteLine("Step Saved Successfully.");
                    }

                    break;
                case ConsoleKey.L:
                    {
                        Console.WriteLine("Enter file path");
                        string savedStepFilePath = Console.ReadLine();

                        if (Path.Exists(savedStepFilePath))
                        {
                            Console.WriteLine("Loading Step...");
                            Step<Creature, Food> savedStep = LoadSavedStep(savedStepFilePath);

                            MainGameHost.ConnectionWeightBound = savedStep.ConnectionWeightBound;
                            MainGameHost.MinCreatureConnections = savedStep.MinCreatureConnections;
                            MainGameHost.MaxCreatureConnections = savedStep.MaxCreatureConnections;
                            MainGameHost.MaxCreatureProcessNodes = savedStep.MaxCreatureProcessNodes;
                            MainGameHost.LoopWorldBounds = savedStep.LoopWorldBounds;
                            MainGameHost.WorldBounds = savedStep.WorldBounds;
                            MainGameHost.CurrentStep = savedStep;

                            Console.WriteLine("Step Loaded Successfully.");
                        }
                        else
                        {
                            Console.WriteLine("Unable to locate file.");
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

            string serializedStep = JsonConvert.SerializeObject(step, new JsonSerializerSettings() { Formatting = Formatting.Indented });
            File.WriteAllText(filePath, serializedStep);
        }
        public Step<Creature, Food> LoadSavedStep(string filePath)
        {
            string serializedStep = File.ReadAllText(filePath);
            Step<Creature, Food> savedStep = JsonConvert.DeserializeObject<Step<Creature, Food>>(serializedStep);
            return savedStep;
        }
        public void UpdateSimulation()
        {
            MainGameHost.NextStep(GatherStepInfoForAllCreatures);

            CurrentRunTicksElapsed += 1;

            if (CurrentRunTicksElapsed % 500 == 0 && MainGameHost.CurrentStep.Creatures.All(x => x.IsDead))
            {
                FailRun();
            }
        }
        public void FailRun()
        {
            MainGameHost.ResetStep(MainGameHost.GenerateCreatures(), MainGameHost.GenerateFood());

            FailedRunsUptimes.Add(TimeSpan.FromMilliseconds(CurrentRunTicksElapsed * SimulationMspt));
            CurrentRunTicksElapsed = 0;
        }
        public void Reset()
        {
            MainGameHost.WorldBounds = new Rectangle(0, 0, 784, 661); //Same value as MaceEvolve.WinForms ClientRectangle
            MainGameHost.ResetStep(MainGameHost.GenerateCreatures(), MainGameHost.GenerateFood());

            FailedRunsUptimes.Clear();
            CurrentRunTicksElapsed = 0;
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
            Console.WriteLine("---------------------------------------------------------");
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