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
        #region Fields
        private List<Apple> FoodList { get; set; }
        private List<Creature> CreaturesList { get; set; }
        #endregion

        #region Properties
        public Genome Genome;
        public double Energy { get; set; } = 150;
        public double Speed { get; set; } = 1;
        public int SightRange { get; set; } = 200;
        public double Metabolism { get; set; } = 0.1;
        public List<Apple> VisibleFood { get; set; }
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
        public IEnumerable<Apple> GetVisibleFood(IEnumerable<Apple> Food)
        {
            return Food.Where(Apple => GetDistanceFrom(Apple.X, Apple.Y) <= SightRange).OrderBy(Apple => GetDistanceFrom(Apple.X, Apple.Y));
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
            Apple ClosestApple = VisibleFood.FirstOrDefault();

            if (ClosestApple != null && GetDistanceFrom(ClosestApple.X, ClosestApple.Y) <= Size / 2)
            {
                Eat(ClosestApple);
                return true;
            }

            return false;
        }
        public void Eat(Apple Apple)
        {
            Energy -= Apple.ServingDigestionCost;
            Apple.Servings -= 1;
            Energy += Apple.EnergyPerServing;
        }
        public void Move()
        {
            Apple ClosestApple = VisibleFood.FirstOrDefault();

            if (ClosestApple != null)
            {
                double XDifference = X - ClosestApple.X;
                double YDifference = Y - ClosestApple.Y;

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

        #region Inputs
        public double Input_DistanceFromFood()
        {
            Apple ClosestApple = VisibleFood.FirstOrDefault();
            double DistanceFromFood = ClosestApple == null ? 0 : GetDistanceFrom(ClosestApple.X, ClosestApple.Y);
            return ClosestApple == null ? 0 : Globals.Map(DistanceFromFood, 0, DistanceFromFood, 1, 0);
        }
        #endregion

        #endregion
    }
}
