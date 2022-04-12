using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using MaceEvolve.Enums;

namespace MaceEvolve.Models
{
    public class Creature : GameObject
    {
        #region Fields
        private List<Food> FoodList { get; set; }
        private List<Creature> CreaturesList { get; set; }
        private List<ProcessNode> OutputNodes { get; set; } = new List<ProcessNode>();
        #endregion

        #region Properties
        private double MoveCost { get; set; } = 0.5;
        private double IdleCost
        {
            get
            {
                return Metabolism;
            }
        }
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
            ProcessNode OutputNodeTryEat = new ProcessNode()
            {
                ConnectionWeight = 1,
                Inputs = new List<ProcessNode>()
                {
                    new ProcessNode()
                    {
                        ConnectionWeight = 1,
                        Inputs = new List<ProcessNode>()
                        {
                            new ProcessNode()
                            {
                                ConnectionWeight = 2,
                                IsStartNode = true,
                                StartNodeValue = CreatureValue.PercentMaxEnergy,
                                StartNodeCreature = this
                            },
                            new ProcessNode()
                            {
                                ConnectionWeight = 1,
                                IsStartNode = true,
                                StartNodeValue = CreatureValue.ProximityToFood,
                                StartNodeCreature = this
                            }
                        }
                    }
                }
            };

            ProcessNode OutputNodeIdle = new ProcessNode()
            {
                ConnectionWeight = 1,
                Inputs = new List<ProcessNode>()
                {
                    new ProcessNode()
                    {
                        ConnectionWeight = 1,
                        Inputs = new List<ProcessNode>()
                        {
                            new ProcessNode()
                            {
                                ConnectionWeight = 0.2,
                                IsStartNode = true,
                                StartNodeValue = CreatureValue.ProximityToFood,
                                StartNodeCreature = this
                            }
                        }
                    }
                }
            };

            OutputNodes.Add(OutputNodeTryEat);
            OutputNodes.Add(OutputNodeIdle);
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

            Live();
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
 
        public void Eat(Food Food)
        {
            Energy -= Food.ServingDigestionCost;
            Food.Servings -= 1;
            Energy += Food.EnergyPerServing;
        }
        public void Die()
        {
            Energy = 0;
            Color = Color.Brown;
        }
        public void Live()
        {
            double TryEatValue = OutputNodes[0].GetValue();
            double DieValue = OutputNodes[1].GetValue();

            if (TryEatValue > DieValue)
            {
                if (TryEatFoodInRange())
                {
                }
                else
                {
                    Move();
                }
                
            }
            else
            {
                Idle();
            }
        }


        #region CreatureValues
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
        #endregion

        #region Processes
        public double ChanceToTryEat(Dictionary<CreatureValue, double> Values)
        {
            double Output = 0;
            foreach (var KeyValuePair in Values)
            {
                double InputResult;
                switch (KeyValuePair.Key)
                {
                    case CreatureValue.ProximityToFood:
                        InputResult = ProximityToFood(this);
                        break;

                    case CreatureValue.PercentMaxEnergy:
                        InputResult = PercentMaxEnergy(this);
                        break;

                    default:
                        throw new NotImplementedException();
                }
                Output += InputResult * KeyValuePair.Value;
            }
            return Globals.Sigmoid(Output);
        }
        public double Neuron(Dictionary<CreatureProcess, double> Values)
        {
            double Output = 0;
            foreach (var KeyValuePair in Values)
            {
                double InputResult;
                switch (KeyValuePair.Key)
                {
                    case CreatureProcess.ChanceToTryEat:
                        InputResult = ProximityToFood(this);
                        break;

                    default:
                        throw new NotImplementedException();
                }
                Output += InputResult * KeyValuePair.Value;
            }
            return Globals.Sigmoid(Output);
        }
        #endregion

        #region Outputs
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
        public void Idle()
        {
            Energy -= IdleCost;
        }
        public void MoveForward()
        {
            Y -= Speed;
            Energy -= MoveCost;
        }
        public void MoveBackward()
        {
            Y += Speed;
            Energy -= MoveCost;
        }
        public void MoveLeft()
        {
            X -= Speed;
            Energy -= MoveCost;
        }
        public void MoveRight()
        {
            X += Speed;
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
