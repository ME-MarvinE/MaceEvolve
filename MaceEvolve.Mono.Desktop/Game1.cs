﻿using MaceEvolve.Core;
using MaceEvolve.Core.Interfaces;
using MaceEvolve.Core.Models;
using MaceEvolve.Mono.Desktop.Models;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended;
using MonoGame.Extended.BitmapFonts;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
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
        public bool IsInFastMode { get; set; }
        public GameHost<Step<GraphicalCreature, GraphicalFood>, GraphicalCreature, GraphicalFood> MainGameHost { get; set; }
        public SpriteFont UIFont { get; set; }
        public SpriteFont BigUIFont { get; set; }
        public GameWindow BestCreatureNetworkViewerWindow { get; set; }
        public bool GatherStepInfoForAllCreatures { get; set; }
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

            GenerationsToRunFor = 500000;
            SimulationTPS = 60;
            TicksPerGeneration = SimulationTPS * 30; //30 Seconds per generation.
            TargetElapsedTime = TimeSpan.FromSeconds(1f / SimulationTPS);

            MainGameHost = new GameHost<Step<GraphicalCreature, GraphicalFood>, GraphicalCreature, GraphicalFood>();
            MainGameHost.CreatureSize = 5;
            MainGameHost.FoodSize = MainGameHost.CreatureSize * 0.7f;
            MainGameHost.CreatureSpeed = MainGameHost.UseSuccessBounds ? 2.75f * 1.3f : 2.75f;

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

            if (!IsInFastMode && SimulationRunning && GenerationCount < GenerationsToRunFor)
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

            if (!IsInFastMode)
            {
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
            }

            if (MainGameHost.UseSuccessBounds)
            {
                _spriteBatch.DrawRectangle(MainGameHost.SuccessBounds.X, MainGameHost.SuccessBounds.Y, MainGameHost.SuccessBounds.Width, MainGameHost.SuccessBounds.Height, new Color(Color.Green, 100));
            }

            _spriteBatch.DrawString(BigUIFont, SimulationRunning ? "Running" : "Stopped", new Vector2(10, 15), Color.White);
            _spriteBatch.DrawString(BigUIFont, $"Gen {GenerationCount}", new Vector2(10, 45), Color.White);
            _spriteBatch.DrawString(UIFont, $"{timeInSimulation:d\\d' 'h\\h' 'm\\m' 's\\.ff\\s}/{timePerSimulation:d\\d' 'h\\h' 'm\\m' 's\\.ff\\s}/{timeUntilSimulationEnds:d\\d' 'h\\h' 'm\\m' 's\\.ff\\s}" +
                $"\nGen {GenerationCount}/{GenerationsToRunFor}, {timeInCurrentGeneration:s\\.ff\\s}/{timePerGeneration:s\\.ff\\s}", new Vector2(10, 75), Color.White);

            _spriteBatch.DrawString(UIFont, $"Gather Step Info For All Creatures: {(GatherStepInfoForAllCreatures ? "Enabled" : "Disabled")}", new Vector2(10, 115), Color.White);

            _spriteBatch.End();

            base.Draw(gameTime);
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
        public void NewGeneration()
        {
            List<GraphicalCreature> newGenerationCreatures = NewGenerationAsexual();

            if (newGenerationCreatures.Count > 0)
            {
                MainGameHost.ResetStep(newGenerationCreatures, GenerateFood());

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

            float MiddleWorldBoundsX = Globals.MiddleX(MainGameHost.WorldBounds.X, MainGameHost.WorldBounds.Width);
            float MiddleWorldBoundsY = Globals.MiddleX(MainGameHost.WorldBounds.Y, MainGameHost.WorldBounds.Height);

            MainGameHost.SuccessBounds = new Core.Models.Rectangle(MiddleWorldBoundsX - 75, MiddleWorldBoundsY - 75, 150, 150);

            MainGameHost.CurrentStep = new Step<GraphicalCreature, GraphicalFood>()
            {
                Creatures = GenerateCreatures(),
                Food = GenerateFood(),
                WorldBounds = MainGameHost.WorldBounds
            };

            TicksInCurrentGeneration = 0;
            GenerationCount = 1;
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

            if (GamePad.GetState(PlayerIndex.One).Buttons.X == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.N))
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

            if (GamePad.GetState(PlayerIndex.One).Buttons.Y == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.F))
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

            if (GamePad.GetState(PlayerIndex.One).Buttons.RightStick == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.G))
            {
                GatherStepInfoForAllCreatures = !GatherStepInfoForAllCreatures;
            }
        }
        #endregion
    }
}