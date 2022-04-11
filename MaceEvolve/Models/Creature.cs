using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaceEvolve.Models
{
    public class Creature : GameObject
    {
        #region Fields
        private List<Apple> FoodList { get; set; }
        private List<Creature> CreaturesList { get; set; }
        #endregion

        #region Properties
        public double Energy { get; set; } = 150;
        public double Speed { get; set; } = 1;
        public int SightRange { get; set; } = 200;
        public double Metabolism { get; set; } = 0.1;
        #endregion

        #region Constructors
        public Creature()
        {
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

            if (TryEatFoodInRange())
            {
                return;
            }
            else
            {
                Move();
            }
        }
        public IEnumerable<Apple> GetClosestFood(IEnumerable<Apple> Food)
        {
            return Food?.OrderBy(Apple => GetDistanceFrom(Apple.X, Apple.Y));
        }
        public int GetDistanceFrom(int TargetX, int TargetY)
        {
            return (int)Math.Sqrt(Math.Pow(X - TargetX, 2) + Math.Pow(Y - TargetY, 2));
        }
        public double GetDistanceFrom(double TargetX, double TargetY)
        {
            return Math.Sqrt(Math.Pow(X - TargetX, 2) + Math.Pow(Y - TargetY, 2));
        }
        public int GetDistanceFrom(Point TargetLocation)
        {
            return GetDistanceFrom(TargetLocation.X, TargetLocation.Y);
        }
        public bool TryEatFoodInRange()
        {
            Apple ClosestApple = GetClosestFood(FoodList).FirstOrDefault();

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
            Apple ClosestApple = GetClosestFood(FoodList).FirstOrDefault();
            double NewX = X;
            double NewY = Y;

            if (ClosestApple != null)
            {
                double XDifference = X - ClosestApple.X;
                double YDifference = Y - ClosestApple.Y;

                if (XDifference + YDifference <= SightRange)
                {
                    if (XDifference > 0)
                    {
                        NewX = XDifference >= Speed ? X - Speed : NewX;
                    }
                    else if (XDifference < 0)
                    {
                        NewX = XDifference <= -Speed ? X + Speed : NewX;
                    }

                    if (YDifference > 0)
                    {
                        NewY = YDifference >= Speed ? Y - Speed : NewY;
                    }
                    else if (YDifference < 0)
                    {
                        NewY = YDifference <= -Speed ? Y + Speed : NewY;
                    }
                }
            }

            if (NewX != X || NewY != Y)
            {
                Rectangle = new Rectangle(NewX, NewY, Size, Size);
                Energy -= 0.3;
            }
        }
        public void Die()
        {
            Color = Color.Brown;
        }
        #endregion
    }
}
