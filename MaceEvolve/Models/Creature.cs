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
        public NeuralNetwork Brain { get; set; }
        private double MoveCost { get; set; } = 0.5;
        public Genome Genome;
        public double Energy { get; set; } = 150;
        public double MaxEnergy { get; set; } = 150;
        public double Speed { get; set; } = 1;
        public int SightRange { get; set; } = 200;
        public double Metabolism { get; set; } = 0.1;
        public List<Food> VisibleFood { get; set; }
        public List<Creature> VisibleCreatures { get; set; }
        public int FoodEaten { get; set; }
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
            : this(new NeuralNetwork(Globals.AllCreatureInputs, 5, Globals.AllCreatureActions, 10, 50, 4))
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
                Live();

                if (Energy <= 0)
                {
                    Die();
                }
            }
        }
        public IEnumerable<Food> GetVisibleFood(IEnumerable<Food> Food)
        {
            return Food.Where(Food => Globals.GetDistanceFrom(X, Y, Food.X, Food.Y) <= SightRange).OrderBy(Food => Globals.GetDistanceFrom(X, Y, Food.X, Food.Y));
        }
        public IEnumerable<Creature> GetVisibleCreatures(IEnumerable<Creature> Creatures)
        {
            return Creatures.Where(Creature => Globals.GetDistanceFrom(X, Y, Creature.X, Creature.Y) <= SightRange).OrderBy(Creature => Globals.GetDistanceFrom(X, Y, Creature.X, Creature.Y));
        }
        public void Die()
        {
            Color = Color.Brown;
            Energy = 0;
        }
        public void Live()
        {
            Think();

            List<Node> OrderedOutputNodes = Brain.Nodes.Where(x => x.NodeType == NodeType.Output).OrderBy(x => x.PreviousOutput).ToList();
            Node HighestOutputNode = OrderedOutputNodes.LastOrDefault();

            if (HighestOutputNode != null && HighestOutputNode.PreviousOutput > 0)
            {
                ExecuteAction(HighestOutputNode.CreatureAction.Value);
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
            FoodList = GameHost.Food.Where(x => x.Servings > 0).ToList();
            CreatureList = new List<Creature>(GameHost.Creatures);
            VisibleFood = GetVisibleFood(FoodList).ToList();
            VisibleCreatures = GetVisibleCreatures(CreatureList).ToList();

            Brain.UpdateInputValue(CreatureInput.PercentMaxEnergy, PercentMaxEnergy(this));
            Brain.UpdateInputValue(CreatureInput.VerticalProximityToFood, VerticalProximityToFood(this));
            Brain.UpdateInputValue(CreatureInput.HorizontalProximityToFood, HorizontalProximityToFood(this));
            Brain.UpdateInputValue(CreatureInput.ProximityToCreature, ProximityToCreature(this));
            Brain.UpdateInputValue(CreatureInput.VerticalWorldBoundProximity, VerticalWorldBoundProximity(this));
            Brain.UpdateInputValue(CreatureInput.HorizontalWorldBoundProximity, HorizontalWorldBoundProximity(this));
        }
        public void UpdateOutputValues()
        {
            foreach (var OutputNode in Brain.Nodes.Where(x => x.NodeType == NodeType.Output && x.CreatureAction != null && Brain.Actions.Contains(x.CreatureAction.Value)))
            {
                OutputNode.GenerateOutput(Brain);
            }
        }
        public static Creature Reproduce(IEnumerable<Creature> Parents, List<CreatureInput> Inputs, List<CreatureAction> Actions)
        {
            List<Creature> ParentsList = new List<Creature>(Parents);
            Dictionary<Creature, List<Connection>> AvailableParentConnections = ParentsList.ToDictionary(x => x, x => x.Brain.Connections.ToList());
            Dictionary<Creature, Dictionary<Node, Node>> ParentToOffSpringNodesMap = new Dictionary<Creature, Dictionary<Node, Node>>();
            //Dictionary<Node, Node> ParentNodeToOffSpringNodeMap = new Dictionary<Node, Node>();

            Creature OffSpring = new Creature(new NeuralNetwork(new List<Node>(), Inputs, Actions, new List<Connection>()));

            int AverageNumberOfParentConnections = (int)Parents.Average(x => x.Brain.Connections.Count);

            if (AverageNumberOfParentConnections < 1)
            {
                AverageNumberOfParentConnections = 1;
            }

            while (OffSpring.Brain.Connections.Count < AverageNumberOfParentConnections)
            {
                Creature RandomParent = ParentsList[_Random.Next(ParentsList.Count)];
                List<Connection> RandomParentConnections = AvailableParentConnections[RandomParent];
                Connection RandomParentConnection = RandomParentConnections[_Random.Next(RandomParentConnections.Count)];
                Node RandomParentConnectionSourceNode = RandomParent.Brain.Nodes[RandomParentConnection.SourceId];
                Node RandomParentConnectionTargetNode = RandomParent.Brain.Nodes[RandomParentConnection.TargetId];

                Connection ConnectionToAdd = new Connection() { Weight = RandomParentConnection.Weight };

                if (ParentToOffSpringNodesMap.ContainsKey(RandomParent) && ParentToOffSpringNodesMap[RandomParent].ContainsKey(RandomParentConnectionSourceNode))
                {
                    ConnectionToAdd.SourceId = OffSpring.Brain.GetNodeId(ParentToOffSpringNodesMap[RandomParent][RandomParentConnectionSourceNode]);
                }
                else
                {
                    Node NodeToAdd = new Node(RandomParentConnectionSourceNode.NodeType, RandomParentConnectionSourceNode.Bias, RandomParentConnectionSourceNode.CreatureInput, RandomParentConnectionSourceNode.CreatureAction);

                    OffSpring.Brain.Nodes.Add(NodeToAdd);

                    if (!ParentToOffSpringNodesMap.ContainsKey(RandomParent))
                    {
                        ParentToOffSpringNodesMap.Add(RandomParent, new Dictionary<Node, Node>());
                    }

                    if (!ParentToOffSpringNodesMap[RandomParent].ContainsKey(RandomParentConnectionSourceNode))
                    {
                        ParentToOffSpringNodesMap[RandomParent].Add(RandomParentConnectionSourceNode, NodeToAdd);
                    }

                    ConnectionToAdd.SourceId = OffSpring.Brain.GetNodeId(NodeToAdd);
                }

                if (ParentToOffSpringNodesMap.ContainsKey(RandomParent) && ParentToOffSpringNodesMap[RandomParent].ContainsKey(RandomParentConnectionTargetNode))
                {
                    ConnectionToAdd.TargetId = OffSpring.Brain.GetNodeId(ParentToOffSpringNodesMap[RandomParent][RandomParentConnectionTargetNode]);
                }
                else
                {
                    Node NodeToAdd = new Node(RandomParentConnectionTargetNode.NodeType, RandomParentConnectionTargetNode.Bias, RandomParentConnectionTargetNode.CreatureInput, RandomParentConnectionTargetNode.CreatureAction);

                    OffSpring.Brain.Nodes.Add(NodeToAdd);

                    if (!ParentToOffSpringNodesMap.ContainsKey(RandomParent))
                    {
                        ParentToOffSpringNodesMap.Add(RandomParent, new Dictionary<Node, Node>());
                    }

                    if (!ParentToOffSpringNodesMap[RandomParent].ContainsKey(RandomParentConnectionTargetNode))
                    {
                        ParentToOffSpringNodesMap[RandomParent].Add(RandomParentConnectionTargetNode, NodeToAdd);
                    }

                    ConnectionToAdd.TargetId = OffSpring.Brain.GetNodeId(NodeToAdd);
                }

                OffSpring.Brain.Connections.Add(ConnectionToAdd);
                AvailableParentConnections[RandomParent].Remove(RandomParentConnection);
            }

            return OffSpring;
        }
        #region CreatureValues
        //Creature values map from 0 to 1.
        public static double PercentMaxEnergy(Creature Creature)
        {
            return Globals.Map(Creature.Energy, 0, Creature.MaxEnergy, 0, 1);
        }
        public static double ProximityToCreature(Creature Creature)
        {
            Creature ClosestCreature = Creature.VisibleCreatures.FirstOrDefault();

            if (ClosestCreature == null) { return 0; }

            double DistanceFromCreature = Globals.GetDistanceFrom(Creature.X, Creature.Y, ClosestCreature.X, ClosestCreature.Y);

            return Globals.Map(DistanceFromCreature, 0, Creature.SightRange, 0, 1);
        }
        public static double HorizontalProximityToFood(Creature Creature)
        {
            Food ClosestFood = Creature.VisibleFood.FirstOrDefault();

            if (ClosestFood == null) { return 1; }

            double HorizontalDistanceToFood = Globals.GetDistanceFrom(Creature.MX, Creature.MY, ClosestFood.MX, Creature.MY);

            return Globals.Map(HorizontalDistanceToFood, 0, Creature.SightRange, 0, 1);
        }
        public static double VerticalProximityToFood(Creature Creature)
        {
            Food ClosestFood = Creature.VisibleFood.FirstOrDefault();

            if (ClosestFood == null) { return 1; }

            double VerticalDistanceToFood = Globals.GetDistanceFrom(Creature.MX, Creature.MY, Creature.MX, ClosestFood.MY);

            return Globals.Map(VerticalDistanceToFood, 0, Creature.SightRange, 0, 1);
        }
        public static double VerticalWorldBoundProximity(Creature Creature)
        {
            return Globals.Map(Creature.Y, Creature.GameHost.WorldBounds.Top, Creature.GameHost.WorldBounds.Bottom, 0, 1);
        }
        public static double HorizontalWorldBoundProximity(Creature Creature)
        {
            return Globals.Map(Creature.X, Creature.GameHost.WorldBounds.Left, Creature.GameHost.WorldBounds.Right, 0, 1);
        }
        #endregion

        #region Actions
        private void Eat(Food Food)
        {
            Energy -= Food.ServingDigestionCost;
            Food.Servings -= 1;
            FoodEaten += 1;
            Energy += Food.EnergyPerServing;
        }
        public bool TryEatFoodInRange()
        {
            Food ClosestFood = VisibleFood.FirstOrDefault();

            if (ClosestFood != null && Globals.GetDistanceFrom(X, Y, ClosestFood.X, ClosestFood.Y) <= Size / 2)
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
                Y += Speed;
                //Y += GameHost.WorldBounds.Height;
            }
            Energy -= MoveCost;
        }
        public void MoveBackward()
        {
            Y += Speed;
            if (Y > GameHost.WorldBounds.Bottom)
            {
                Y -= Speed;
                //Y -= GameHost.WorldBounds.Height;
            }
            Energy -= MoveCost;
        }
        public void MoveLeft()
        {
            X -= Speed;
            if (X < GameHost.WorldBounds.Left)
            {
                X += Speed;
                //X += GameHost.WorldBounds.Width;
            }
            Energy -= MoveCost;
        }
        public void MoveRight()
        {
            X += Speed;
            if (X > GameHost.WorldBounds.Right)
            {
                X -= Speed;
                //X -= GameHost.WorldBounds.Width;
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
