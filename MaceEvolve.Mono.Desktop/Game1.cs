using MaceEvolve.Core;
using MaceEvolve.Core.Interfaces;
using MaceEvolve.Core.Models;
using MaceEvolve.Mono.Desktop.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace MaceEvolve.Mono.Desktop
{
    public class Game1 : Game
    {
        #region Fields
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        #endregion

        #region Properties
        public int SimulationTPS { get; set; }
        public bool SimulationRunning { get; set; }
        public int CurrentRunTicksElapsed { get; set; }
        public long AllRunsElapsed { get; set; }
        public bool GatherStepInfoForAllCreatures { get; set; }
        public bool IsInFastMode { get; set; }
        public Color BackgroundColor { get; set; }
        public GraphicalGameHost<GraphicalStep<GraphicalCreature, GraphicalFood, GraphicalTree>, GraphicalCreature, GraphicalFood, GraphicalTree> MainGameHost { get; set; }
        public SpriteFont UIFont { get; set; }
        public SpriteFont BigUIFont { get; set; }
        public GameWindow BestCreatureNetworkViewerWindow { get; set; }
        public float SimulationMspt
        {
            get
            {
                return (1f / SimulationTPS) * 1000;
            }
        }
        public List<TimeSpan> FailedRunsUptimes { get; set; } = new List<TimeSpan>();
        public StepResult<GraphicalCreature> PreviousStepResult { get; set; }
        public bool ShowTreeColorByAge { get; set; } = true;
        #endregion

        #region Constructors
        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);

            if (GraphicsDevice == null)
            {
                _graphics.ApplyChanges();
            }

            _graphics.PreferredBackBufferWidth = 800 - 17;
            _graphics.PreferredBackBufferHeight = 700 - 40;

            _graphics.ApplyChanges();

            Window.KeyDown += Window_KeyDown;

            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }
        #endregion

        #region Methods
        protected override void Initialize()
        {
            Window.Title = "Mace Evolve";
            BackgroundColor = new Color(32, 32, 32);

            SimulationTPS = 60;
            TargetElapsedTime = TimeSpan.FromSeconds(1f / SimulationTPS);

            MainGameHost = new GraphicalGameHost<GraphicalStep<GraphicalCreature, GraphicalFood, GraphicalTree>, GraphicalCreature, GraphicalFood, GraphicalTree>();

            Reset();

            Window.AllowUserResizing = true;

            base.Initialize();
        }
        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            UIFont = Content.Load<SpriteFont>("UIFont");
            BigUIFont = Content.Load<SpriteFont>("BigUIFont");
        }
        protected override void Update(GameTime gameTime)
        {
            if (!IsInFastMode && SimulationRunning)
            {
                UpdateSimulation();
            }

            base.Update(gameTime);
        }
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(BackgroundColor);

            double millisecondsInFailedRuns = 0;

            foreach (var failedRunTimeSpan in FailedRunsUptimes)
            {
                millisecondsInFailedRuns += failedRunTimeSpan.TotalMilliseconds;
            }

            TimeSpan timeInCurrentRun = TimeSpan.FromMilliseconds(CurrentRunTicksElapsed * SimulationMspt);
            TimeSpan timeInFailedRuns = TimeSpan.FromMilliseconds(millisecondsInFailedRuns);
            TimeSpan timeInAllRuns = timeInFailedRuns.Add(timeInCurrentRun);
            TimeSpan averageTimePerRun = TimeSpan.FromMilliseconds(FailedRunsUptimes.Count == 0 ? 0 : FailedRunsUptimes.Average(x => x.TotalMilliseconds));

            _spriteBatch.Begin();

            foreach (var tree in MainGameHost.CurrentStep.Trees)
            {
                Color treeColorToUse;

                if (ShowTreeColorByAge)
                {
                    int treeR = (int)(80 * ((float)tree.Age / tree.MaxAge));
                    int treeG = (int)Globals.Map(170 * ((float)tree.Age / tree.MaxAge), 0, 170, 170, 40);
                    int treeB = (int)(10 * ((float)tree.Age / tree.MaxAge));

                    treeColorToUse = new Color(treeR, treeG, treeB, tree.Color.A);
                }
                else
                {
                    treeColorToUse = tree.Color;
                }

                _spriteBatch.DrawCircle(tree.MX, tree.MY, tree.Size, 360, treeColorToUse, tree.Size);
            }

            foreach (var creature in MainGameHost.CurrentStep.Creatures)
            {
                Color creatureColor;

                if (creature.IsDead)
                {
                    creatureColor = new Color(165, 41, 41);
                }
                else
                {
                    creatureColor = new Color(creature.Color.R, creature.Color.G, creature.Color.B, creature.Color.A);
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

                _spriteBatch.DrawCircle(creature.MX, creature.MY, creature.Size, 18, creatureColor, creature.Size);

                if (creatureRingColor != null)
                {
                    _spriteBatch.DrawCircle(creature.MX, creature.MY, creature.Size + 2, 18, creatureRingColor.Value, creature.Size * 0.3f);
                }
            }
            foreach (var food in MainGameHost.CurrentStep.Food)
            {
                _spriteBatch.DrawCircle(food.MX, food.MY, food.Size, 18, food.Color, food.Size);
            }

            if (SimulationRunning)
            {
                _spriteBatch.DrawString(BigUIFont, IsInFastMode ? "Running (Fast)" : "Running", new Vector2(10, 15), Color.White);
            }
            else
            {
                _spriteBatch.DrawString(BigUIFont, IsInFastMode ? "Stopped (Fast)" : "Stopped", new Vector2(10, 15), Color.White);
            }

            _spriteBatch.DrawString(BigUIFont, $"Run {FailedRunsUptimes.Count + 1}", new Vector2(10, 45), Color.White);
            _spriteBatch.DrawString(UIFont, $"Uptime: {timeInAllRuns:d\\d' 'h\\h' 'm\\m' 's\\.ff\\s}, Failed: {timeInFailedRuns:d\\d' 'h\\h' 'm\\m' 's\\.ff\\s}" +
                $"\nRun {FailedRunsUptimes.Count + 1}, {timeInCurrentRun:d\\d' 'h\\h' 'm\\m' 's\\.ff\\s}, Average: {averageTimePerRun:d\\d' 'h\\h' 'm\\m' 's\\.ff\\s}", new Vector2(10, 75), Color.White);

            _spriteBatch.DrawString(UIFont, $"Gather Step Info For All Creatures: {(GatherStepInfoForAllCreatures ? "Enabled" : "Disabled")}", new Vector2(10, 115), Color.White);

            _spriteBatch.End();

            base.Draw(gameTime);
        }
        public void UpdateSimulation()
        {
            MainGameHost.CreatureOffspringColor = new Color(64, 64, MaceRandom.Current.Next(256));
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
        public List<GraphicalTree> GenerateTrees(List<GraphicalTree> treesToConvert = null)
        {
            List<GraphicalTree> treeList = treesToConvert ?? MainGameHost.GenerateTrees();

            foreach (var tree in treeList)
            {
                tree.Color = new Color (30, 170, 0, 50);
            }

            return treeList;
        }
        public List<GraphicalCreature> GenerateCreatures()
        {
            List<GraphicalCreature> creatures = new List<GraphicalCreature>();

            creatures.AddRange(MainGameHost.GenerateCreatures());

            foreach (var creature in creatures)
            {
                creature.Color = new Color(64, 64, MaceRandom.Current.Next(256));
            }

            return creatures;
        }
        public void Reset()
        {
            Rectangle gameBounds = _graphics.GraphicsDevice.PresentationParameters.Bounds;
            MainGameHost.WorldBounds = new Core.Models.Rectangle(gameBounds.X, gameBounds.Y, gameBounds.Width, gameBounds.Height);

            PreviousStepResult = new StepResult<GraphicalCreature>(new ConcurrentQueue<StepAction<GraphicalCreature>>());
            MainGameHost.ResetStep(GenerateCreatures(), GenerateFood(), GenerateTrees());

            FailedRunsUptimes.Clear();
            CurrentRunTicksElapsed = 0;
        }
        public void FailRun()
        {
            PreviousStepResult = new StepResult<GraphicalCreature>(new ConcurrentQueue<StepAction<GraphicalCreature>>());
            MainGameHost.ResetStep(GenerateCreatures(), GenerateFood(), GenerateTrees());

            FailedRunsUptimes.Add(TimeSpan.FromMilliseconds(CurrentRunTicksElapsed * SimulationMspt));
            CurrentRunTicksElapsed = 0;
        }
        private async void Window_KeyDown(object sender, InputKeyEventArgs e)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                Exit();
            }

            if (GamePad.GetState(PlayerIndex.One).Buttons.A == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.P))
            {
                SimulationRunning = !SimulationRunning;
            }

            if (GamePad.GetState(PlayerIndex.One).Buttons.B == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.R))
            {
                Reset();
            }

            if (GamePad.GetState(PlayerIndex.One).Buttons.Y == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.F))
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

            if (GamePad.GetState(PlayerIndex.One).Buttons.RightStick == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.G))
            {
                GatherStepInfoForAllCreatures = !GatherStepInfoForAllCreatures;
            }
        }
        #endregion
    }
}