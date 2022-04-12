using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace MaceEvolve.Models
{
    public class Creature : GameObject
    {

        public enum Inputs
        {
            DistanceFromNearestFood,
            VisibleFood,
            Energy,
            Metabolism,
            Speed
        }
        public enum Processes
        {

        }
        public enum Outputs
        {
            Die,
            MoveForward,
            MoveBackward,
            MoveLeft,
            MoveRight,
            TryEat
        }

        #region Fields
        private List<Food> FoodList { get; set; }
        private List<Creature> CreaturesList { get; set; }
        #endregion

        #region Properties
        public Genome Genome;
        public double Energy { get; set; } = 150;
        public double Speed { get; set; } = 1;
        public int SightRange { get; set; } = 200;
        public double Metabolism { get; set; } = 0.1;
        public List<Food> VisibleFood { get; set; }
        //public int StomachSize { get; set; } = 5;
        //public List<Food> StomachContents { get; set; } = 5;
        //public double DigestionRate = 0.1;
        #endregion

        #region Constructors
        public Creature()
            :this(new Genome())
        {
        }
        public Creature(Genome Genome)
        {
            this.Genome = Genome;
        }
        #endregion

        #region Methods
        public override void Update()
        {
            if (Energy <= 0)
            {
                Die();
                return;
            }

            Energy -= Metabolism;

            if (Energy <= 0)
            {
                Die();
                return;
            }

            FoodList = GameHost.Food.Where(x => x.Servings > 0).ToList();
            CreaturesList = new List<Creature>(GameHost.Creatures);
            VisibleFood = GetVisibleFood(FoodList).ToList();

            if (TryEatFoodInRange())
            {
                return;
            }
            else
            {
                Move();
            }
        }
        public IEnumerable<Food> GetVisibleFood(IEnumerable<Food> Food)
        {
            return Food.Where(Food => GetDistanceFrom(Food.X, Food.Y) <= SightRange).OrderBy(Food => GetDistanceFrom(Food.X, Food.Y));
        }
        public int GetDistanceFrom(int TargetX, int TargetY)
        {
            return (int)GetDistanceFrom((double)TargetX, (double)TargetY);
        }
        public double GetDistanceFrom(double TargetX, double TargetY)
        {
            return Globals.ToPositive(X - TargetX) + Globals.ToPositive(Y - TargetY);
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
        public void Eat(Food Food)
        {
            Energy -= Food.ServingDigestionCost;
            Food.Servings -= 1;
            Energy += Food.EnergyPerServing;
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
        public void Die()
        {
            Color = Color.Brown;
        }
        public void MoveForward()
        {
            Y -= Speed;
            Energy -= 0.15;
        }
        public void MoveBackward()
        {
            Y += Speed;
            Energy -= 0.15;
        }
        public void MoveLeft()
        {
            X -= Speed;
            Energy -= 0.15;
        }
        public void MoveRight()
        {
            X += Speed;
            Energy -= 0.15;
        }
        public double Sigmoid(double Num)
        {
            return 1 / (1 + Math.Exp(-Num));
        }
        public double SigmoidDerivative(double Num)
        {
            return Num * (1 - Num);
        }

        #region Inputs
        public double Input_DistanceFromFood(double Weight)
        {
            Food ClosestFood = VisibleFood.FirstOrDefault();

            if (ClosestFood == null) { return 0; }

            double DistanceFromFood = GetDistanceFrom(ClosestFood.X, ClosestFood.Y);

            return Globals.Map(DistanceFromFood, 0, DistanceFromFood, 1, 0);
        }
        #endregion

        #endregion
    }
}
