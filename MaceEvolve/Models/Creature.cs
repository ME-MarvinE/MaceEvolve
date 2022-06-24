using MaceEvolve.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace MaceEvolve.Models
{
    public class Creature : GameObject
    {
        #region Fields
        private List<Food> FoodList { get; set; }
        private List<Creature> CreatureList { get; set; }
        #endregion

        #region Properties
        public NeuralNetwork Brain { get; }
        private double MoveCost { get; set; } = 0.5;
        public Genome Genome;
        public double Energy { get; set; } = 150;
        public double Speed { get; set; } = 1;
        public int SightRange { get; set; } = 200;
        public double Metabolism { get; set; } = 0.1;
        public List<Food> VisibleFood { get; set; }
        public List<Creature> VisibleCreatures { get; set; }
        //public int StomachSize { get; set; } = 5;
        //public List<Food> StomachContents { get; set; } = 5;
        //public double DigestionRate = 0.1;
        #endregion

        #region Constructors
        public Creature()
            : this(new Genome())
        {
        }
        public Creature(Genome Genome)
            : this(new NeuralNetwork(Globals.AllCreatureInputs, 5, Globals.AllCreatureActions, 10, 50))
        {
            this.Genome = Genome;
        }
        public Creature(NeuralNetwork Brain)
        {
            this.Brain = Brain;
        }
        #endregion

        #region Methods
        public void ExecuteAction(CreatureAction CreatureAction)
        {
            switch (CreatureAction)
            {
                case CreatureAction.MoveForward:
                    MoveForward();
                    break;

                case CreatureAction.MoveBackward:
                    MoveBackward();
                    break;

                case CreatureAction.MoveLeft:
                    MoveLeft();
                    break;

                case CreatureAction.MoveRight:
                    MoveRight();
                    break;

                case CreatureAction.TryEat:
                    TryEatFoodInRange();
                    break;

                default:
                    throw new NotImplementedException();
            }
        }
        public override void Update()
        {
            if (Energy > 0)
            {
                FoodList = GameHost.Food.Where(x => x.Servings > 0).ToList();
                CreatureList = new List<Creature>(GameHost.Creatures);
                VisibleFood = GetVisibleFood(FoodList).ToList();
                VisibleCreatures = GetVisibleCreatures(CreatureList).ToList();

                Live();

                if (Energy <= 0)
                {
                    Die();
                }
            }
        }
        public IEnumerable<Food> GetVisibleFood(IEnumerable<Food> Food)
        {
            return Food.Where(Food => GetDistanceFrom(Food.X, Food.Y) <= SightRange).OrderBy(Food => GetDistanceFrom(Food.X, Food.Y));
        }
        public IEnumerable<Creature> GetVisibleCreatures(IEnumerable<Creature> Creatures)
        {
            return Creatures.Where(Creature => GetDistanceFrom(Creature.X, Creature.Y) <= SightRange).OrderBy(Creature => GetDistanceFrom(Creature.X, Creature.Y));
        }
        public int GetDistanceFrom(int TargetX, int TargetY)
        {
            return (int)GetDistanceFrom((double)TargetX, (double)TargetY);
        }
        public double GetDistanceFrom(double TargetX, double TargetY)
        {
            return Globals.ToPositive(X - TargetX) + Globals.ToPositive(Y - TargetY);
        }
        public void Die()
        {
            Energy = 0;
            Color = Color.Brown;
        }
        public void Live()
        {
            Think();

            List<OutputNode> OrderedOutputNodes = Brain.OutputNodes.OrderBy(x => x.PreviousOutput).ToList();
            OutputNode HighestOutputNode = OrderedOutputNodes.LastOrDefault();

            if (HighestOutputNode != null && HighestOutputNode.PreviousOutput > 0)
            {
                ExecuteAction(HighestOutputNode.CreatureAction);
            }

            Energy -= Metabolism;
        }
        public void Think()
        {
            UpdateInputValues();
            UpdateOutputValues();
        }
        public void UpdateInputValues()
        {
            Brain.InputValues[CreatureInput.PercentMaxEnergy] = PercentMaxEnergy(this);
            Brain.InputValues[CreatureInput.ProximityToFood] = ProximityToFood(this);
            Brain.InputValues[CreatureInput.ProximityToCreature] = ProximityToCreature(this);
        }
        public void UpdateOutputValues()
        {
            foreach (var OutputNode in Brain.OutputNodes)
            {
                OutputNode.EvaluateValue(Brain);
            }
        }

        #region CreatureValues
        //Creature values map from 0 to 1.
        public static double PercentMaxEnergy(Creature Creature)
        {
            return Globals.Map(Creature.Energy, 0, 100, 0, 1);
        }
        public static double ProximityToFood(Creature Creature)
        {
            Food ClosestFood = Creature.VisibleFood.FirstOrDefault();

            if (ClosestFood == null) { return 0; }

            double DistanceFromFood = Creature.GetDistanceFrom(ClosestFood.X, ClosestFood.Y);

            return Globals.Map(DistanceFromFood, 0, Creature.SightRange, 0, 1);
        }
        public static double ProximityToCreature(Creature Creature)
        {
            Creature ClosestCreature = Creature.VisibleCreatures.FirstOrDefault();

            if (ClosestCreature == null) { return 0; }

            double DistanceFromCreature = Creature.GetDistanceFrom(ClosestCreature.X, ClosestCreature.Y);

            return Globals.Map(DistanceFromCreature, 0, Creature.SightRange, 0, 1);
        }
        #endregion

        #region Actions
        private void Eat(Food Food)
        {
            Energy -= Food.ServingDigestionCost;
            Food.Servings -= 1;
            Energy += Food.EnergyPerServing;
        }
        public bool TryEatFoodInRange()
        {
            Food ClosestFood = VisibleFood.FirstOrDefault();

            if (ClosestFood != null && GetDistanceFrom(ClosestFood.X, ClosestFood.Y) <= Size / 2)
            {
                Eat(ClosestFood);
                return true;
            }

            return false;
        }
        public void MoveForward()
        {
            Y -= Speed;
            if (Y < GameHost.WorldBounds.Top)
            {
                Y += GameHost.WorldBounds.Height;
            }
            Energy -= MoveCost;
        }
        public void MoveBackward()
        {
            Y += Speed;
            if (Y > GameHost.WorldBounds.Bottom)
            {
                Y -= GameHost.WorldBounds.Height;
            }
            Energy -= MoveCost;
        }
        public void MoveLeft()
        {
            X -= Speed;
            if (X < GameHost.WorldBounds.Left)
            {
                X += GameHost.WorldBounds.Width;
            }
            Energy -= MoveCost;
        }
        public void MoveRight()
        {
            X += Speed;
            if (X > GameHost.WorldBounds.Right)
            {
                X -= GameHost.WorldBounds.Width;
            }
            Energy -= MoveCost;
        }
        public void Move()
        {
            Food ClosestFood = VisibleFood.FirstOrDefault();

            if (ClosestFood != null)
            {
                double XDifference = X - ClosestFood.X;
                double YDifference = Y - ClosestFood.Y;

                if (XDifference + YDifference <= SightRange)
                {
                    if (YDifference > 0)
                    {
                        if (YDifference >= Speed)
                        {
                            MoveForward();
                        }
                    }
                    else if (YDifference < 0)
                    {
                        if (YDifference <= -Speed)
                        {
                            MoveBackward();
                        }
                    }

                    if (XDifference > 0)
                    {
                        if (XDifference >= Speed)
                        {
                            MoveLeft();
                        }
                    }
                    else if (XDifference < 0)
                    {
                        if (XDifference <= -Speed)
                        {
                            MoveRight();
                        }
                    }
                }
            }
        }
        #endregion

        #endregion
    }
}
