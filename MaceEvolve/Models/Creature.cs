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
        #endregion

        #region Properties
        private double MoveCost { get; set; } = 0.5;
        private double ReproductionCost { get; set; } = 50;
        private double IdleCost
        {
            get
            {
                return Metabolism;
            }
        }
        public Genome Genome { get; }
        public double Energy { get; set; } = 150;
        public double Speed { get; set; } = 1;
        public int SightRange { get; set; } = 200;
        public double Metabolism { get; set; } = 0.1;
        public List<Food> VisibleFood { get; set; }
        public double MaxEnergy = 150;
        public List<ProcessNode> InputNodes { get; }
        public Dictionary<CreatureValue, double> CurrentCreatureValues { get; } = Genome.CreatureInputs.ToDictionary(x => x, x => 0d);
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
            InputNodes = GetInputNodes(Genome.Genes);
            //ProcessNode OutputNodeTryEat = new ProcessNode()
            //{
            //    ConnectionWeight = 1,
            //    Inputs = new List<ProcessNode>()
            //    {
            //        new ProcessNode()
            //        {
            //            ConnectionWeight = 1,
            //            Inputs = new List<ProcessNode>()
            //            {
            //                new ProcessNode()
            //                {
            //                    ConnectionWeight = _Random.NextDouble(),
            //                    IsStartNode = true,
            //                    InputNodeCreatureInput = CreatureValue.PercentMaxEnergy,
            //                    StartNodeCreature = this
            //                },
            //                new ProcessNode()
            //                {
            //                    ConnectionWeight = _Random.NextDouble() / 4,
            //                    IsStartNode = true,
            //                    InputNodeCreatureInput = CreatureValue.ProximityToFood,
            //                    StartNodeCreature = this
            //                }
            //            }
            //        }
            //    }
            //};

            //ProcessNode OutputNodeIdle = new ProcessNode()
            //{
            //    ConnectionWeight = 1,
            //    Inputs = new List<ProcessNode>()
            //    {
            //        new ProcessNode()
            //        {
            //            ConnectionWeight = 1,
            //            Inputs = new List<ProcessNode>()
            //            {
            //                new ProcessNode()
            //                {
            //                    ConnectionWeight = _Random.NextDouble(),
            //                    IsStartNode = true,
            //                    InputNodeCreatureInput = CreatureValue.ProximityToFood,
            //                    StartNodeCreature = this
            //                }
            //            }
            //        }
            //    }
            //};

            //ProcessNode OutputNodeReproduce = new ProcessNode()
            //{
            //    ConnectionWeight = 1,
            //    Inputs = new List<ProcessNode>()
            //    {
            //        new ProcessNode()
            //        {
            //            ConnectionWeight = 1,
            //            Inputs = new List<ProcessNode>()
            //            {
            //                new ProcessNode()
            //                {
            //                    ConnectionWeight = _Random.NextDouble(),
            //                    IsStartNode = true,
            //                    InputNodeCreatureInput = CreatureValue.PercentMaxEnergy,
            //                    StartNodeCreature = this
            //                }
            //            }
            //        }
            //    }
            //};

            //OutputNodes.Add(OutputNodeTryEat);
            //OutputNodes.Add(OutputNodeIdle);
            //OutputNodes.Add(OutputNodeReproduce);
        }
        #endregion

        #region Methods
        public static List<ProcessNode> GetInputNodes(IEnumerable<ProcessNode> ProcessNodes)
        {
            List<ProcessNode> InputNodes = new List<ProcessNode>();
            foreach (ProcessNode ProcessNode in ProcessNodes)
            {
                if (ProcessNode.NodeType == NodeType.Input)
                {
                    InputNodes.Add(ProcessNode);
                }
                else
                {
                    InputNodes.AddRange(GetInputNodes(ProcessNode.Inputs));
                }
            }
            return InputNodes;
        }
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
            return Food.Where(Food => Food.Servings > 0 && GetDistanceFrom(Food.X, Food.Y) <= SightRange).OrderBy(Food => GetDistanceFrom(Food.X, Food.Y));
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
            Energy = Energy + Food.EnergyPerServing < MaxEnergy ? Energy + Food.EnergyPerServing : MaxEnergy;
        }
        public void Die()
        {
            Energy = 0;
            Color = Color.Brown;
        }
        public void Live()
        {
            foreach (CreatureValue CreatureInput in CurrentCreatureValues.Keys)
            {
                CurrentCreatureValues[CreatureInput] = GetCreatureInput(CreatureInput);
            }

            foreach (ProcessNode InputNode in InputNodes)
            {
                InputNode.InputNodeValue = CurrentCreatureValues[InputNode.InputNodeCreatureInput];
            }

            //Dictionary<ProcessNode, double> OutputNodesDesc;
            //OutputNodesDesc = Genome.Genes
            //    .Select(Node => (Node, Node.GetValue()))
            //    .OrderByDescending(x => x.Item2)
            //    .ToDictionary(x => x.Node, x => x.Item2);

            //CreatureOutput HighestValueOutput = OutputNodesDesc.Keys.First().OutputNodeCreatureOutput;

            Dictionary<CreatureOutput, double> OutputNodesDesc;
            OutputNodesDesc = Genome.Genes
                .Select(Node => (Node, Node.GetValue()))
                .OrderByDescending(x => x.Item2)
                .ToDictionary(x => x.Node.OutputNodeCreatureOutput, x => x.Item2);

            CreatureOutput HighestValueOutput = OutputNodesDesc.Keys.First();

            TryEatFoodInRange();

            ExecuteCreatureOutput(HighestValueOutput);
        }
        public void ExecuteCreatureOutput(CreatureOutput CreatureOutput)
        {
            switch (CreatureOutput)
            {
                case CreatureOutput.MoveForward:
                    MoveForward();
                    break;
                case CreatureOutput.MoveBackward:
                    MoveBackward();
                    break;
                case CreatureOutput.MoveLeft:
                    MoveLeft();
                    break;
                case CreatureOutput.MoveRight:
                    MoveRight();
                    break;
                case CreatureOutput.Reproduce:
                    Reproduce();
                    break;
                //case CreatureOutput.TryEat:
                //    TryEatFoodInRange();
                //    break;
                case CreatureOutput.Idle:
                    Idle();
                    break;
                default:
                    throw new NotImplementedException();
            }
        }
        public double GetCreatureInput(CreatureValue CreatureValue)
        {
            switch (CreatureValue)
            {
                case CreatureValue.PercentMaxEnergy:
                    return ProximityToFood(this);
                case CreatureValue.ProximityToFood:
                    return PercentMaxEnergy(this);
                default:
                    throw new NotImplementedException();
            }
        }


        #region Inputs
        public static double PercentMaxEnergy(Creature Creature)
        {
            return Globals.Map(Creature.Energy, 0, Creature.MaxEnergy, 0, 1);
        }
        public static double ProximityToFood(Creature Creature)
        {
            Food ClosestFood = Creature.VisibleFood.FirstOrDefault();

            if (ClosestFood == null) { return 0; }

            double DistanceFromFood = Creature.GetDistanceFrom(ClosestFood.X, ClosestFood.Y);

            return Globals.Map(DistanceFromFood, 0, Creature.SightRange, 0, 1);
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
            if (Y < GameHost.Bounds.Top)
            {
                Y += GameHost.Bounds.Height;
            }
            Energy -= MoveCost;
        }
        public void MoveBackward()
        {
            Y += Speed;
            if (Y > GameHost.Bounds.Bottom)
            {
                Y -= GameHost.Bounds.Height;
            }
            Energy -= MoveCost;
        }
        public void MoveLeft()
        {
            X -= Speed;
            if (X < GameHost.Bounds.Left)
            {
                X += GameHost.Bounds.Width;
            }
            Energy -= MoveCost;
        }
        public void MoveRight()
        {
            X += Speed;
            if (X < GameHost.Bounds.Right)
            {
                X -= GameHost.Bounds.Width;
            }
            Energy -= MoveCost;
        }
        public void Reproduce()
        {
            double RandomX = X + _Random.Next(-30, 30);
            double RandomY = Y + _Random.Next(-30, 30);

            if (RandomX < GameHost.Bounds.Left || RandomX > GameHost.Bounds.Right)
            {
                RandomX = X;
            }

            if (RandomY < GameHost.Bounds.Top || RandomY > GameHost.Bounds.Bottom)
            {
                RandomY = Y;
            }

            GameHost.BornCreatures.Enqueue(
                new Creature(new Genome(Genome.GenerateNodeNetwork(GameHost.MinNodeBreadth, GameHost.MaxNodeBreadth, _Random.Next(GameHost.MaxNodeDepth), NodeType.Output)))
                {
                    GameHost = GameHost,
                    X = RandomX,
                    Y = RandomY,
                    Size = 10,
                    Color = Color.FromArgb(255, 255, 255, 64),
                    Speed = 1.3,
                    Metabolism = 0.1,
                    Energy = _Random.Next((int)ReproductionCost, (int)(ReproductionCost * 1.5)),
                    SightRange = 100,
                    MaxEnergy = _Random.Next(100, 200)
                });

            Energy -= ReproductionCost;
        }
        #endregion

        #endregion
    }
}
