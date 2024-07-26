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
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using System.Timers;
using Color = System.Drawing.Color;
using CoreGlobals = MaceEvolve.Core.Globals;
using Window = Silk.NET.Windowing.Window;

namespace MaceEvolve.SilkGL
{
    public class MaceEvolveApp
    {
        #region Properties
        private const int _VERTICES_DATALENGTH = 3;
        private GL _gl;
        private uint _vao;
        private uint _vbo;
        private uint _ebo;
        private uint _program;
        private int _verticesPerCircle = 90;
        private const string SHADER_FILLCOLOR_VARIABLE_NAME = "drawColor";
        private const string VERTEX_CODE = @$"
            #version 330 core
            layout (location = 0) in vec3 aPosition;

            uniform vec4 {SHADER_FILLCOLOR_VARIABLE_NAME};

            out vec4 vertexColor;

            void main()
            {{
                gl_Position = vec4(aPosition, 1.0);
                vertexColor = {SHADER_FILLCOLOR_VARIABLE_NAME};
            }}";

        private const string FRAGMENT_CODE = @"
            #version 330 core
            in vec4 vertexColor;

            out vec4 FragColor;

            void main()
            {
                FragColor = vertexColor;
            }";
        public bool IsSimulationRunning { get; set; }
        public Timer PeriodicInfoTimer { get; }
        public bool AutoClearConsole { get; set; }
        public bool PeriodicInfo { get; set; } = true;
        protected static JsonSerializerSettings SaveStepSerializerSettings { get; }
        protected static JsonSerializerSettings LoadStepSerializerSettings { get; }
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
            MainGameHost = new GraphicalGameHost<GraphicalStep<GraphicalCreature, GraphicalFood>, GraphicalCreature, GraphicalFood>();

            SimulationTPS = 60;
            PeriodicInfoTimer = new Timer(1000) { Enabled = PeriodicInfo };
            PeriodicInfoTimer.Elapsed += PeriodicInfoTimer_Elapsed;

            WindowOptions options = WindowOptions.Default;
            options.Size = new Vector2D<int>(800, 600);
            options.Title = "Mace Evolve";
            options.UpdatesPerSecond = SimulationTPS;
            options.FramesPerSecond = 60;
            options.VSync = false;

            MainWindow = Window.Create(options);
            BackgroundColor = Color.FromArgb(32, 32, 32);

            MainWindow.Load += MainWindow_Load;
            MainWindow.Update += MainWindow_Update;
            MainWindow.Render += MainWindow_Render;

            MainWindow.Run();
        }
        #endregion

        #region Methods
        private void MainWindow_Load()
        {
            InitialiseGL();

            Reset();

            IInputContext input = MainWindow.CreateInput();

            foreach (var keyboard in input.Keyboards)
            {
                keyboard.KeyDown += Keyboard_KeyDown;
            }

            ListCommands();
            ListSimulationInfo();
        }
        private void InitialiseGL()
        {
            _gl = MainWindow.CreateOpenGL();
            _gl.ClearColor(BackgroundColor);

            // For colouring things
            uint vertexShader = _gl.CreateShader(ShaderType.VertexShader);
            _gl.ShaderSource(vertexShader, VERTEX_CODE);
            _gl.CompileShader(vertexShader);
            _gl.GetShader(vertexShader, ShaderParameterName.CompileStatus, out int vertexShaderLinkStatus);

            if (vertexShaderLinkStatus != (int)GLEnum.True)
            {
                throw new Exception("Vertex shader failed to compile: " + _gl.GetShaderInfoLog(vertexShader));
            }

            uint fragmentShader = _gl.CreateShader(ShaderType.FragmentShader);
            _gl.ShaderSource(fragmentShader, FRAGMENT_CODE);
            _gl.CompileShader(fragmentShader);
            _gl.GetShader(fragmentShader, ShaderParameterName.CompileStatus, out int fragmentShaderLinkStatus);

            if (fragmentShaderLinkStatus != (int)GLEnum.True)
            {
                throw new Exception("Fragment shader failed to compile: " + _gl.GetShaderInfoLog(fragmentShader));
            }

            _program = _gl.CreateProgram();
            _gl.AttachShader(_program, vertexShader);
            _gl.AttachShader(_program, fragmentShader);
            _gl.LinkProgram(_program);
            _gl.GetProgram(_program, ProgramPropertyARB.LinkStatus, out int programLinkStatus);

            if (programLinkStatus != (int)GLEnum.True)
            {
                throw new Exception("Program failed to link: " + _gl.GetProgramInfoLog(_program));
            }

            _gl.DetachShader(_program, vertexShader);
            _gl.DetachShader(_program, fragmentShader);
            _gl.DeleteShader(vertexShader);
            _gl.DeleteShader(fragmentShader);

            // Allows for colours with alpha values to work.
            _gl.Enable(EnableCap.Blend);
            _gl.BlendFunc(BlendingFactor.SrcAlpha, BlendingFactor.OneMinusSrcAlpha);

            // For drawing things
            _vao = _gl.GenVertexArray();
            _gl.BindVertexArray(_vao);

            _vbo = _gl.GenBuffer();
            _gl.BindBuffer(BufferTargetARB.ArrayBuffer, _vbo);

            _gl.VertexAttribPointer(0, _VERTICES_DATALENGTH, VertexAttribPointerType.Float, false, 0, IntPtr.Zero);
            _gl.EnableVertexAttribArray(0);
            _gl.UseProgram(_program);
        }
        private void MainWindow_Render(double deltaTime)
        {
            LastRenderDeltaTime = deltaTime;
            _gl.Clear(ClearBufferMask.ColorBufferBit);

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

                if (creatureRingColor != null)
                {
                    float[] bodyRingVertices = GenerateLocationalCircleVertices(creature.MX, creature.MY, creature.Size, _verticesPerCircle);

                    SetShaderColor(creatureRingColor.Value, _program);
                    _gl.BufferData(BufferTargetARB.ArrayBuffer, (uint)(bodyRingVertices.Length * sizeof(float)), [.. bodyRingVertices], BufferUsageARB.DynamicDraw);
                    _gl.DrawArrays(PrimitiveType.TriangleFan, 1, (uint)bodyRingVertices.Length);
                }

                float[] bodyVertices = GenerateLocationalCircleVertices(creature.MX, creature.MY, creature.Size, _verticesPerCircle);

                SetShaderColor(creatureColor, _program);
                _gl.BufferData(BufferTargetARB.ArrayBuffer, (uint)(bodyVertices.Length * sizeof(float)), [.. bodyVertices], BufferUsageARB.DynamicDraw);
                _gl.DrawArrays(PrimitiveType.TriangleFan, 1, (uint)bodyVertices.Length);
            }

            foreach (var food in MainGameHost.CurrentStep.Food)
            {
                float[] bodyVertices = GenerateLocationalCircleVertices(food.MX, food.MY, food.Size, _verticesPerCircle);

                SetShaderColor(food.Color, _program);
                _gl.BufferData(BufferTargetARB.ArrayBuffer, (uint)(bodyVertices.Length * sizeof(float)), [.. bodyVertices], BufferUsageARB.DynamicDraw);
                _gl.DrawArrays(PrimitiveType.TriangleFan, 1, (uint)bodyVertices.Length);
            }
        }
        private void MainWindow_Update(double deltaTime)
        {
            LastUpdateDeltaTime = deltaTime;

            if (!IsInFastMode && IsSimulationRunning)
            {
                UpdateSimulation();
            }
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
                            MainGameHost.CreatureConnectionsMinMax = savedStep.CreatureConnectionsMinMax;
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
        public static string GetGroupedDataDisplay(IList<float> data, int dataLength)
        {
            string returnString = "";
            for (int i = 0; i < data.Count / dataLength; i++)
            {
                returnString += "[";
                for (int j = 0; j < dataLength; j++)
                {
                    returnString += $"{data[(i * dataLength) + j]}";

                    if (j != dataLength - 1)
                    {
                        returnString += ", ";
                    }
                }

                returnString += "]";
                if (i != (data.Count / dataLength) - 1)
                {
                    returnString += ",\n";
                }
            }

            return returnString;
        }
        public void SetShaderVec3(string name, Vector3 value, uint program)
        {
            int location = _gl.GetUniformLocation(program, name);
            _gl.Uniform3(location, value);
        }
        public void SetShaderVec4(string name, Vector4 value, uint program)
        {
            int location = _gl.GetUniformLocation(program, name);
            _gl.Uniform4(location, value);
        }
        public void SetShaderColor(Color color, uint program, string fillColorVariableName = SHADER_FILLCOLOR_VARIABLE_NAME)
        {
            float mappedR = CoreGlobals.Map(color.R, 0, 255, 0, 1f);
            float mappedG = CoreGlobals.Map(color.G, 0, 255, 0, 1f);
            float mappedB = CoreGlobals.Map(color.B, 0, 255, 0, 1f);
            float mappedA = CoreGlobals.Map(color.A, 0, 255, 0, 1f);
            SetShaderVec4(fillColorVariableName, new Vector4(mappedR, mappedG, mappedB, mappedA), program);
        }
        public float[] GenerateLocationalCircleVertices(float x, float y, float radius, int verticesPerCircle)
        {
            return Globals.GenerateCircleVertices(
                    CoreGlobals.Map(x, MainGameHost.WorldBounds.X, MainGameHost.WorldBounds.X + MainGameHost.WorldBounds.Width, -1f, 1f),
                    CoreGlobals.Map(y, MainGameHost.WorldBounds.Y, MainGameHost.WorldBounds.Y + MainGameHost.WorldBounds.Height, -1f, 1f),
                    CoreGlobals.Map(radius, MainGameHost.WorldBounds.X, MainGameHost.WorldBounds.X + MainGameHost.WorldBounds.Width, 0f, 1f), _verticesPerCircle);
        }
        #endregion
    }
}