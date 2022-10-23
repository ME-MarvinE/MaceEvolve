﻿using MaceEvolve.Controls;
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

            Dictionary<int, double> NodeIdToOutputDict = Brain.LoggedStep(true, true);
            Dictionary<Node, double> NodeOutputsDict = NodeIdToOutputDict.OrderBy(x => x.Value).ToDictionary(x => Brain.NodeIdsToNodesDict[x.Key], x => x.Value);
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
            Brain.UpdateInputValue(CreatureInput.DistanceFromTopWorldBound, DistanceFromTopWorldBound());
            Brain.UpdateInputValue(CreatureInput.DistanceFromLeftWorldBound, DistanceFromLeftWorldBound());
        }
        public static Creature Reproduce(IList<Creature> Parents, List<CreatureInput> Inputs, List<CreatureAction> Actions)
        {
            Dictionary<Creature, List<Connection>> AvailableParentConnections = Parents.ToDictionary(x => x, x => x.Brain.Connections.ToList());
            Dictionary<Creature, Dictionary<int, int>> ParentToOffSpringNodesMap = new Dictionary<Creature, Dictionary<int, int>>();

            Creature OffSpring = new Creature()
            {
                Brain = new NeuralNetwork(new List<Node>(), Inputs, Actions, new List<Connection>())
            };

            double AverageNumberOfParentConnections = Parents.Average(x => x.Brain.Connections.Count);

            if (AverageNumberOfParentConnections > 0 && AverageNumberOfParentConnections < 1)
            {
                AverageNumberOfParentConnections = 1;
            }

            while (OffSpring.Brain.Connections.Count < AverageNumberOfParentConnections)
            {
                Creature RandomParent = Parents[Globals.Random.Next(Parents.Count)];
                List<Connection> RandomParentAvailableConnections = AvailableParentConnections[RandomParent];

                if (RandomParentAvailableConnections.Count > 0)
                {
                    Connection RandomParentConnection = RandomParentAvailableConnections[Globals.Random.Next(RandomParentAvailableConnections.Count)];

                    //If a parent's node has not been added and mapped to an offspring's node, create a new node and map it to the parent's node.
                    if (!(ParentToOffSpringNodesMap.ContainsKey(RandomParent) && ParentToOffSpringNodesMap[RandomParent].ContainsKey(RandomParentConnection.SourceId)))
                    {
                        Node RandomParentConnectionSourceNode = RandomParent.Brain.NodeIdsToNodesDict[RandomParentConnection.SourceId];
                        Node NewNode = new Node(RandomParentConnectionSourceNode.NodeType, RandomParentConnectionSourceNode.Bias, RandomParentConnectionSourceNode.CreatureInput, RandomParentConnectionSourceNode.CreatureAction);
                        int NewNodeId = OffSpring.Brain.AddNode(NewNode);

                        //Map the newly added offspring node to the parent's node so that duplicates aren't created if two of the parent's connections reference the same node.
                        if (!ParentToOffSpringNodesMap.ContainsKey(RandomParent))
                        {
                            ParentToOffSpringNodesMap.Add(RandomParent, new Dictionary<int, int>());
                        }

                        ParentToOffSpringNodesMap[RandomParent][RandomParentConnection.SourceId] = NewNodeId;
                    }

                    if (!(ParentToOffSpringNodesMap.ContainsKey(RandomParent) && ParentToOffSpringNodesMap[RandomParent].ContainsKey(RandomParentConnection.TargetId)))
                    {
                        Node RandomParentConnectionTargetNode = RandomParent.Brain.NodeIdsToNodesDict[RandomParentConnection.TargetId];
                        Node NewNode = new Node(RandomParentConnectionTargetNode.NodeType, RandomParentConnectionTargetNode.Bias, RandomParentConnectionTargetNode.CreatureInput, RandomParentConnectionTargetNode.CreatureAction);
                        int NewNodeId = OffSpring.Brain.AddNode(NewNode);

                        //Map the newly added offspring node to the parent's node so that duplicates aren't created if two of the parent's connections reference the same node.
                        if (!ParentToOffSpringNodesMap.ContainsKey(RandomParent))
                        {
                            ParentToOffSpringNodesMap.Add(RandomParent, new Dictionary<int, int>());
                        }

                        ParentToOffSpringNodesMap[RandomParent][RandomParentConnection.TargetId] = NewNodeId;
                    }

                    Connection ConnectionToAdd = new Connection()
                    {
                        Weight = RandomParentConnection.Weight,
                        SourceId = ParentToOffSpringNodesMap[RandomParent][RandomParentConnection.SourceId],
                        TargetId = ParentToOffSpringNodesMap[RandomParent][RandomParentConnection.TargetId]
                    };

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
        public double DistanceFromTopWorldBound()
        {
            return Globals.Map(Y, GameHost.WorldBounds.Top, GameHost.WorldBounds.Bottom, 0, 1);
        }
        public double DistanceFromLeftWorldBound()
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
