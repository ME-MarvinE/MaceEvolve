using MaceEvolve.Core;
using MaceEvolve.Core.Models;
using MaceEvolve.Mono.Desktop.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using System;
using System.Collections.Generic;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace MaceEvolve.Mono.Desktop
{
    public class Game1 : Game
    {
        #region Fields
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private Random _random = new Random();
        #endregion

        #region Properties
        public int SimulationTPS { get; set; }
        public long TicksPerGeneration { get; set; }
        public int GenerationCount { get; set; }
        public bool SimulationRunning { get; set; }
        public int GenerationsToRunFor { get; set; }
        public Color BackgroundColor { get; set; }
        public int TicksInCurrentGeneration { get; set; }
        public GameHost<GraphicalCreature, GraphicalFood> MainGameHost { get; set; }
        public SpriteFont UIFont { get; set; }
        public GameWindow BestCreatureNetworkViewerWindow { get; set; }
        public double SimulationMspt
        {
            get
            {
                return (1d / SimulationTPS) * 1000;
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
        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);

            if (GraphicsDevice == null)
            {
                _graphics.ApplyChanges();
            }

            _graphics.PreferredBackBufferWidth = 800;
            _graphics.PreferredBackBufferHeight = 700;

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

            GenerationsToRunFor = 100;
            SimulationTPS = 60;
            TicksPerGeneration = SimulationTPS * 30; //30 Seconds per generation.
            TargetElapsedTime = TimeSpan.FromSeconds(1d / SimulationTPS);

            MainGameHost = new GameHost<GraphicalCreature, GraphicalFood>();
            MainGameHost.CreatureSize = 5;
            MainGameHost.FoodSize = MainGameHost.CreatureSize * 0.7;
            MainGameHost.CreatureSpeed = MainGameHost.UseSuccessBounds ? 1.5 * 1.3 : 1.5;

            Reset();

            Window.AllowUserResizing = true;

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            UIFont = Content.Load<SpriteFont>("UIFont");
        }

        protected override void Update(GameTime gameTime)
        {

            if (SimulationRunning && GenerationCount < GenerationsToRunFor)
            {
                UpdateSimulation();
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(BackgroundColor);

            TimeSpan timeInCurrentGeneration = TimeSpan.FromMilliseconds(TicksInCurrentGeneration * SimulationMspt);
            TimeSpan timePerGeneration = TimeSpan.FromMilliseconds(TicksPerGeneration * SimulationMspt);
            TimeSpan timeInSimulation = TimeSpan.FromMilliseconds(TicksElapsed * SimulationMspt);
            TimeSpan timePerSimulation = TimeSpan.FromMilliseconds(TicksWhenSimulationEnds * SimulationMspt);
            TimeSpan timeUntilSimulationEnds = TimeSpan.FromMilliseconds(TicksUntilSimulationIsCompleted * SimulationMspt);


            _spriteBatch.Begin();

            foreach (var creature in MainGameHost.Creatures)
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

                _spriteBatch.DrawCircle(Convert.ToSingle(creature.MX), Convert.ToSingle(creature.MY), Convert.ToSingle(creature.Size), 18, creatureColor, Convert.ToSingle(creature.Size));

                if (creatureRingColor != null)
                {
                    _spriteBatch.DrawCircle(Convert.ToSingle(creature.MX), Convert.ToSingle(creature.MY), Convert.ToSingle(creature.Size + 2), 18, creatureRingColor.Value, Convert.ToSingle(creature.Size * 0.3));
                }
            }
            foreach (var food in MainGameHost.Food)
            {
                _spriteBatch.DrawCircle(Convert.ToSingle(food.MX), Convert.ToSingle(food.MY), Convert.ToSingle(food.Size), 18, food.Color, Convert.ToSingle(food.Size));
            }

            if (MainGameHost.UseSuccessBounds)
            {
                _spriteBatch.DrawRectangle(Convert.ToSingle(MainGameHost.SuccessBounds.X), Convert.ToSingle(MainGameHost.SuccessBounds.Y), Convert.ToSingle(MainGameHost.SuccessBounds.Width), Convert.ToSingle(MainGameHost.SuccessBounds.Height), new Color(Color.Green, 100));
            }

            _spriteBatch.DrawString(UIFont, $"{(SimulationRunning ? "Running" : "Stopped")}, {timeInSimulation:d\\d' 'h\\h' 'm\\m' 's\\.ff\\s}/{timePerSimulation:d\\d' 'h\\h' 'm\\m' 's\\.ff\\s}/{timeUntilSimulationEnds:d\\d' 'h\\h' 'm\\m' 's\\.ff\\s}", new Vector2(10, 10), Color.White);
            _spriteBatch.DrawString(UIFont, $"Gen {GenerationCount}/{GenerationsToRunFor}, {timeInCurrentGeneration:s\\.ff\\s}/{timePerGeneration:s\\.ff\\s}", new Vector2(10, 40), Color.White);

            _spriteBatch.End();

            base.Draw(gameTime);
        }
        public void UpdateSimulation()
        {
            MainGameHost.Update();
            TicksInCurrentGeneration += 1;

            if (TicksInCurrentGeneration >= TicksPerGeneration)
            {
                NewGeneration();
            }
        }
        public bool RunSimulation(int generationsToRunFor, int ticksPerGeneration = 1800)
        {
            if (ticksPerGeneration < 1) { throw new ArgumentOutOfRangeException($"{nameof(ticksPerGeneration)} must be greater than 0"); }

            int generationCount = 0;

            while (generationCount < generationsToRunFor)
            {
                for (int ticksInCurrentGeneration = 0; ticksInCurrentGeneration < ticksPerGeneration; ticksInCurrentGeneration++)
                {
                    MainGameHost.Update();
                }

                List<GraphicalCreature> newGenerationCreatures = NewGenerationAsexual();

                if (newGenerationCreatures.Count > 0)
                {
                    MainGameHost.Reset();
                    MainGameHost.Food.AddRange(GenerateFood());
                    MainGameHost.Creatures = newGenerationCreatures;
                    generationCount += 1;
                }
                else
                {
                    return false;
                }
            }

            return true;
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
                creature.Color = new Color(64, 64, _random.Next(256));
            }

            return creatures;
        }
        public List<GraphicalCreature> NewGenerationAsexual()
        {
            List<GraphicalCreature> newGenerationCreatures = MainGameHost.NewGenerationAsexual();

            foreach (var creature in newGenerationCreatures)
            {
                creature.Color = new Color(64, 64, _random.Next(256));
            }

            return newGenerationCreatures;
        }
        public void NewGeneration()
        {
            List<GraphicalCreature> newGenerationCreatures = NewGenerationAsexual();

            if (newGenerationCreatures.Count > 0)
            {
                MainGameHost.Reset();
                MainGameHost.Food.AddRange(GenerateFood());
                MainGameHost.Creatures = newGenerationCreatures;

                TicksInCurrentGeneration = 0;
                GenerationCount += 1;
            }
            else
            {
                Reset();
            }
        }
        public void Reset()
        {
            MainGameHost.Reset();

            Rectangle gameBounds = _graphics.GraphicsDevice.PresentationParameters.Bounds;
            MainGameHost.WorldBounds = new Core.Models.Rectangle(gameBounds.X, gameBounds.Y, gameBounds.Width, gameBounds.Height);

            double MiddleWorldBoundsX = Globals.MiddleX(MainGameHost.WorldBounds.X, MainGameHost.WorldBounds.Width);
            double MiddleWorldBoundsY = Globals.MiddleX(MainGameHost.WorldBounds.Y, MainGameHost.WorldBounds.Height);

            MainGameHost.SuccessBounds = new Core.Models.Rectangle(MiddleWorldBoundsX - 75, MiddleWorldBoundsY - 75, 150, 150);

            MainGameHost.Food.AddRange(GenerateFood());
            MainGameHost.Creatures.AddRange(GenerateCreatures());

            TicksInCurrentGeneration = 0;
            GenerationCount = 1;
        }
        private void Window_KeyDown(object sender, InputKeyEventArgs e)
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

            if (GamePad.GetState(PlayerIndex.One).Buttons.X == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.N))
            {
                //Doesn't work if loop runs from 0 to x.
                for (long i = TicksUntilCurrentGenerationIsCompleted; i > 0; i--)
                {
                    UpdateSimulation();
                }
            }

            if (GamePad.GetState(PlayerIndex.One).Buttons.Y == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.F))
            {
                //Doesn't work if loop runs from 0 to x.
                for (long i = TicksUntilSimulationIsCompleted; i > 0; i--)
                {
                    UpdateSimulation();
                }
            }
        }
        #endregion
    }
}