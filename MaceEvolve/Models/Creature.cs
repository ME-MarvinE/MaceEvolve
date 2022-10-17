using MaceEvolve.Controls;
using MaceEvolve.Enums;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace MaceEvolve.Models
{
    public class Creature : GameObject
    {
        #region Properties
        public NeuralNetwork Brain { get; set; }
        private double MoveCost { get; set; } = 0.5;
        public Genome Genome;
        public double Energy { get; set; } = 150;
        public double MaxEnergy { get; set; } = 150;
        public double Speed { get; set; } = 1;
        public int SightRange { get; set; } = 200;
        public double Metabolism { get; set; } = 0.1;
        public int FoodEaten { get; set; }
        public bool IsDead { get; set; }
        //public int StomachSize { get; set; } = 5;
        //public List<Food> StomachContents { get; set; } = 5;
        //public double DigestionRate = 0.1;
        public CreatureStepInfo CurrentStepInfo { get; private set; }

        #endregion

        #region Constructors
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
            if (!IsDead)
            {
                Live();

                if (Energy < 0)
                {
                    Die();
                }
            }

        }
        public IEnumerable<T> GetVisibleGameObjects<T>(IEnumerable<T> GameObjects) where T : GameObject
        {
            if (typeof(T) == typeof(Creature))
            {
                return GameObjects.Where(x => Globals.GetDistanceFrom(X, Y, x.X, x.Y) <= SightRange && x != this);
            }
            else
            {
                return GameObjects.Where(x => Globals.GetDistanceFrom(X, Y, x.X, x.Y) <= SightRange);
            }
        }
        public void Die()
        {
            IsDead = true;
            Color = Color.Brown;
            Energy = 0;
        }
        public void Live()
        {
            if (GameHost.SelectedCreature == this)
            {
                bool IsSelected = true;
            }
            CurrentStepInfo = new CreatureStepInfo();

            UpdateCurrentStepInfo();
            UpdateInputValues();

            Dictionary<int, double> NodeIdToOutputDict = Brain.LoggedStep(true, false);
            Dictionary<Node, double> NodeOutputsDict = NodeIdToOutputDict.OrderBy(x => x.Value).ToDictionary(x => Brain.Nodes[x.Key], x => x.Value);
            Node HighestOutputNode = NodeOutputsDict.Keys.LastOrDefault(x => x.NodeType == NodeType.Output);

            if (HighestOutputNode != null && NodeOutputsDict[HighestOutputNode] > 0)
            {
                ExecuteAction(HighestOutputNode.CreatureAction.Value);
            }

            Energy -= Metabolism;
        }
        public void UpdateCurrentStepInfo()
        {
            CurrentStepInfo.ExistingFood = GameHost.Food.Where(x => x.Servings > 0).ToList();
            CurrentStepInfo.ExistingCreatures = GameHost.Creatures.ToList();
            CurrentStepInfo.VisibleFood = GetVisibleGameObjects(CurrentStepInfo.ExistingFood).ToList();
            CurrentStepInfo.VisibleCreatures = GetVisibleGameObjects(CurrentStepInfo.ExistingCreatures).ToList();
            CurrentStepInfo.VisibleFoodOrderedByDistance = CurrentStepInfo.VisibleFood.OrderBy(x => Globals.GetDistanceFrom(X, Y, x.X, x.Y)).ToList();
            CurrentStepInfo.VisibleCreaturesOrderedByDistance = CurrentStepInfo.VisibleCreatures.OrderBy(x => Globals.GetDistanceFrom(X, Y, x.X, x.Y)).ToList();
        }
        public void UpdateInputValues()
        {
            Brain.UpdateInputValue(CreatureInput.PercentMaxEnergy, PercentMaxEnergy());
            Brain.UpdateInputValue(CreatureInput.VerticalProximityToFood, VerticalProximityToFood());
            Brain.UpdateInputValue(CreatureInput.HorizontalProximityToFood, HorizontalProximityToFood());
            Brain.UpdateInputValue(CreatureInput.VerticalProximityToCreature, VerticalProximityToCreature());
            Brain.UpdateInputValue(CreatureInput.HorizontalProximityToCreature, HorizontalProximityToCreature());
            Brain.UpdateInputValue(CreatureInput.VerticalWorldBoundProximity, VerticalWorldBoundProximity());
            Brain.UpdateInputValue(CreatureInput.HorizontalWorldBoundProximity, HorizontalWorldBoundProximity());
        }
        public static Creature Reproduce(IEnumerable<Creature> Parents, List<CreatureInput> Inputs, List<CreatureAction> Actions)
        {
            List<Creature> ParentsList = new List<Creature>(Parents);
            Dictionary<Creature, List<Connection>> AvailableParentConnections = ParentsList.ToDictionary(x => x, x => x.Brain.Connections.ToList());
            Dictionary<Creature, Dictionary<Node, Node>> ParentToOffSpringNodesMap = new Dictionary<Creature, Dictionary<Node, Node>>();
            //Dictionary<Node, Node> ParentNodeToOffSpringNodeMap = new Dictionary<Node, Node>();

            Creature OffSpring = new Creature(new NeuralNetwork(new List<Node>(), Inputs, Actions, new List<Connection>()));

            double AverageNumberOfParentConnections = Parents.Average(x => x.Brain.Connections.Count);

            if (AverageNumberOfParentConnections > 0 && AverageNumberOfParentConnections < 1)
            {
                AverageNumberOfParentConnections = 1;
            }

            while (OffSpring.Brain.Connections.Count < AverageNumberOfParentConnections)
            {
                Creature RandomParent = ParentsList[Globals.Random.Next(ParentsList.Count)];
                List<Connection> RandomParentConnections = AvailableParentConnections[RandomParent];

                if (RandomParentConnections.Count > 0)
                {
                    Connection RandomParentConnection = RandomParentConnections[Globals.Random.Next(RandomParentConnections.Count)];
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
            }

            return OffSpring;
        }

        #region CreatureValues
        //x values map from 0 to 1.
        public double PercentMaxEnergy()
        {
            return Globals.Map(Energy, 0, MaxEnergy, 0, 1);
        }
        public double HorizontalProximityToCreature()
        {
            if (CurrentStepInfo.VisibleCreaturesOrderedByDistance.Count == 0)
            {
                return 1;
            }

            //Visible creatures does not contain itself. No need to filter.
            Creature ClosestCreature = CurrentStepInfo.VisibleCreaturesOrderedByDistance[0];

            double HorizontalDistanceToCreature = Globals.GetDistanceFrom(MX, MY, ClosestCreature.MX, MY);

            return Globals.Map(HorizontalDistanceToCreature, 0, SightRange, 0, 1);
        }
        public double VerticalProximityToCreature()
        {
            if (CurrentStepInfo.VisibleCreaturesOrderedByDistance.Count == 0)
            {
                return 1;
            }

            //Visible creatures does not contain itself. No need to filter.
            Creature ClosestCreature = CurrentStepInfo.VisibleCreaturesOrderedByDistance[0];

            double VerticalDistanceToCreature = Globals.GetDistanceFrom(MX, MY, MX, ClosestCreature.MY);

            return Globals.Map(VerticalDistanceToCreature, 0, SightRange, 0, 1);
        }
        public double HorizontalProximityToFood()
        {
            if (CurrentStepInfo.VisibleFoodOrderedByDistance.Count == 0)
            {
                return 1;
            }

            Food ClosestFood = CurrentStepInfo.VisibleFoodOrderedByDistance[0];

            double HorizontalDistanceToFood = Globals.GetDistanceFrom(MX, MY, ClosestFood.MX, MY);

            return Globals.Map(HorizontalDistanceToFood, 0, SightRange, 0, 1);
        }
        public double VerticalProximityToFood()
        {
            if (CurrentStepInfo.VisibleFoodOrderedByDistance.Count == 0)
            {
                return 1;
            }

            Food ClosestFood = CurrentStepInfo.VisibleFoodOrderedByDistance[0];

            double VerticalDistanceToFood = Globals.GetDistanceFrom(MX, MY, MX, ClosestFood.MY);

            return Globals.Map(VerticalDistanceToFood, 0, SightRange, 0, 1);
        }
        public double VerticalWorldBoundProximity()
        {
            return Globals.Map(Y, GameHost.WorldBounds.Top, GameHost.WorldBounds.Bottom, 0, 1);
        }
        public double HorizontalWorldBoundProximity()
        {
            return Globals.Map(X, GameHost.WorldBounds.Left, GameHost.WorldBounds.Right, 0, 1);
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
            if (GameHost.SelectedCreature == this)
            {
                bool IsSelected = true;
            }

            if (CurrentStepInfo.VisibleFoodOrderedByDistance.Count == 0)
            {
                return false;
            }

            Food ClosestFood = CurrentStepInfo.VisibleFoodOrderedByDistance[0];

            if (Globals.GetDistanceFrom(MX, MY, ClosestFood.MX, ClosestFood.MY) > Size / 2)
            {
                return false;
            }

            Eat(ClosestFood);

            return true;
        }
        public void MoveForward()
        {
            Y -= Speed;
            if (MY < GameHost.WorldBounds.Top)
            {
                Y += Speed;
                //Y += GameHost.WorldBounds.Height;
            }
            Energy -= MoveCost;
        }
        public void MoveBackward()
        {
            Y += Speed;
            if (MY > GameHost.WorldBounds.Bottom)
            {
                Y -= Speed;
                //Y -= GameHost.WorldBounds.Height;
            }
            Energy -= MoveCost;
        }
        public void MoveLeft()
        {
            X -= Speed;
            if (MX < GameHost.WorldBounds.Left)
            {
                X += Speed;
                //X += GameHost.WorldBounds.Width;
            }
            Energy -= MoveCost;
        }
        public void MoveRight()
        {
            X += Speed;
            if (MX > GameHost.WorldBounds.Right)
            {
                X -= Speed;
                //X -= GameHost.WorldBounds.Width;
            }
            Energy -= MoveCost;
        }
        public void MoveTowardsClosestFood()
        {
            if (CurrentStepInfo.VisibleFoodOrderedByDistance.Count > 0)
            {
                Food ClosestFood = CurrentStepInfo.VisibleFoodOrderedByDistance[0];

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
