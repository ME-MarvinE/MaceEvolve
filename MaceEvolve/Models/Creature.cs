using MaceEvolve.Controls;
using MaceEvolve.Enums;
using MaceEvolve.Extensions;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace MaceEvolve.Models
{
    public class Creature : GameObject
    {
        #region Fields
        private double _energy;
        #endregion

        #region Properties
        public NeuralNetwork Brain { get; set; }
        private double MoveCost { get; set; } = 0.5;
        public Genome Genome;
        public double Energy
        {
            get
            {
                return _energy;
            }
            set
            {
                if (value < 0)
                {
                    _energy = 0;
                }
                else if (value > MaxEnergy)
                {
                    _energy = MaxEnergy;
                }
                else
                {
                    _energy = value;
                }
            }
        }
        public double MaxEnergy { get; set; } = 150;
        public double Speed { get; set; } = 1;
        public int SightRange { get; set; } = 200;
        public double Metabolism { get; set; } = 0.1;
        public int FoodEaten { get; set; }
        public bool IsDead { get; set; }
        //public int StomachSize { get; set; } = 5;
        //public List<food> StomachContents { get; set; } = 5;
        //public double DigestionRate = 0.1;
        public CreatureStepInfo CurrentStepInfo { get; private set; }
        #endregion

        #region Methods
        public void ExecuteAction(CreatureAction creatureAction)
        {
            switch (creatureAction)
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
        public IEnumerable<T> GetVisibleGameObjects<T>(IEnumerable<T> gameObjects) where T : GameObject
        {
            if (typeof(T) == typeof(Creature))
            {
                return gameObjects.Where(x => Globals.GetDistanceFrom(X, Y, x.X, x.Y) <= SightRange && x != this);
            }
            else
            {
                return gameObjects.Where(x => Globals.GetDistanceFrom(X, Y, x.X, x.Y) <= SightRange);
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
            CurrentStepInfo = new CreatureStepInfo();

            UpdateCurrentStepInfo();
            UpdateInputValues();

            Dictionary<int, double> nodeIdToOutputDict = Brain.LoggedStep(true, true);
            Dictionary<Node, double> nodeOutputsDict = nodeIdToOutputDict.OrderBy(x => x.Value).ToDictionary(x => Brain.NodeIdsToNodesDict[x.Key], x => x.Value);
            Node highestOutputNode = nodeOutputsDict.Keys.LastOrDefault(x => x.NodeType == NodeType.Output);

            if (highestOutputNode != null && nodeOutputsDict[highestOutputNode] > 0)
            {
                ExecuteAction(highestOutputNode.CreatureAction.Value);
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
            Brain.UpdateInputValue(CreatureInput.RandomInput, RandomInput());
        }
        public static Creature Reproduce(IList<Creature> parents, List<CreatureInput> inputs, List<CreatureAction> actions, double nodeBiasMaxVariancePercentage, double connectionWeightMaxVariancePercentage, double connectionWeightBound)
        {
            Dictionary<Creature, List<Connection>> availableParentConnections = parents.ToDictionary(x => x, x => x.Brain.Connections.ToList());
            Dictionary<Creature, Dictionary<int, int>> parentToOffspringNodesMap = new Dictionary<Creature, Dictionary<int, int>>();

            Creature offspring = new Creature()
            {
                Brain = new NeuralNetwork(new List<Node>(), inputs, actions, new List<Connection>())
            };

            double averageNumberOfParentConnections = parents.Average(x => x.Brain.Connections.Count);

            if (averageNumberOfParentConnections > 0 && averageNumberOfParentConnections < 1)
            {
                averageNumberOfParentConnections = 1;
            }

            while (offspring.Brain.Connections.Count < averageNumberOfParentConnections)
            {
                Creature randomParent = parents[Globals.Random.Next(parents.Count)];
                List<Connection> randomParentAvailableConnections = availableParentConnections[randomParent];

                if (randomParentAvailableConnections.Count > 0)
                {
                    Connection randomParentConnection = randomParentAvailableConnections[Globals.Random.Next(randomParentAvailableConnections.Count)];

                    //If a parent's node has not been added and mapped to an offspring's node, create a new node and map it to the parent's node.
                    if (!(parentToOffspringNodesMap.ContainsKey(randomParent) && parentToOffspringNodesMap[randomParent].ContainsKey(randomParentConnection.SourceId)))
                    {
                        Node randomParentConnectionSourceNode = randomParent.Brain.NodeIdsToNodesDict[randomParentConnection.SourceId];
                        Node newNode = new Node(randomParentConnectionSourceNode.NodeType, randomParentConnectionSourceNode.Bias, randomParentConnectionSourceNode.CreatureInput, randomParentConnectionSourceNode.CreatureAction);
                        int newNodeId = offspring.Brain.AddNode(newNode);

                        //Apply any variance to the node's bias.
                        newNode.Bias = Globals.Random.NextDoubleVariance(randomParentConnectionSourceNode.Bias, nodeBiasMaxVariancePercentage);

                        if (newNode.Bias < -1)
                        {
                            newNode.Bias = -1;
                        }
                        else if (newNode.Bias > 1)
                        {
                            newNode.Bias = 1;
                        }

                        //Map the newly added offspring node to the parent's node so that duplicates aren't created if two of the parent's connections reference the same node.
                        if (!parentToOffspringNodesMap.ContainsKey(randomParent))
                        {
                            parentToOffspringNodesMap.Add(randomParent, new Dictionary<int, int>());
                        }

                        parentToOffspringNodesMap[randomParent][randomParentConnection.SourceId] = newNodeId;
                    }

                    if (!(parentToOffspringNodesMap.ContainsKey(randomParent) && parentToOffspringNodesMap[randomParent].ContainsKey(randomParentConnection.TargetId)))
                    {
                        Node randomParentConnectionTargetNode = randomParent.Brain.NodeIdsToNodesDict[randomParentConnection.TargetId];
                        Node newNode = new Node(randomParentConnectionTargetNode.NodeType, randomParentConnectionTargetNode.Bias, randomParentConnectionTargetNode.CreatureInput, randomParentConnectionTargetNode.CreatureAction);
                        int newNodeId = offspring.Brain.AddNode(newNode);

                        //Map the newly added offspring node to the parent's node so that duplicates aren't created if two of the parent's connections reference the same node.
                        if (!parentToOffspringNodesMap.ContainsKey(randomParent))
                        {
                            parentToOffspringNodesMap.Add(randomParent, new Dictionary<int, int>());
                        }

                        parentToOffspringNodesMap[randomParent][randomParentConnection.TargetId] = newNodeId;
                    }

                    Connection connectionToAdd = new Connection()
                    {
                        SourceId = parentToOffspringNodesMap[randomParent][randomParentConnection.SourceId],
                        TargetId = parentToOffspringNodesMap[randomParent][randomParentConnection.TargetId]
                    };

                    //Apply any variance to the connection's weight.
                    connectionToAdd.Weight = Globals.Random.NextDoubleVariance(randomParentConnection.Weight, connectionWeightMaxVariancePercentage);

                    if (connectionToAdd.Weight < -connectionWeightBound)
                    {
                        connectionToAdd.Weight = -connectionWeightBound;
                    }
                    else if (connectionToAdd.Weight > connectionWeightBound)
                    {
                        connectionToAdd.Weight = connectionWeightBound;
                    }

                    offspring.Brain.Connections.Add(connectionToAdd);
                    availableParentConnections[randomParent].Remove(randomParentConnection);
                }
            }

            return offspring;
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
            Creature closestCreature = CurrentStepInfo.VisibleCreaturesOrderedByDistance[0];

            double horizontalDistanceToCreature = Globals.GetDistanceFrom(MX, MY, closestCreature.MX, MY);

            return Globals.Map(horizontalDistanceToCreature, 0, SightRange, 0, 1);
        }
        public double VerticalProximityToCreature()
        {
            if (CurrentStepInfo.VisibleCreaturesOrderedByDistance.Count == 0)
            {
                return 1;
            }

            //Visible creatures does not contain itself. No need to filter.
            Creature closestCreature = CurrentStepInfo.VisibleCreaturesOrderedByDistance[0];

            double verticalDistanceToCreature = Globals.GetDistanceFrom(MX, MY, MX, closestCreature.MY);

            return Globals.Map(verticalDistanceToCreature, 0, SightRange, 0, 1);
        }
        public double HorizontalProximityToFood()
        {
            if (CurrentStepInfo.VisibleFoodOrderedByDistance.Count == 0)
            {
                return 1;
            }

            Food closestFood = CurrentStepInfo.VisibleFoodOrderedByDistance[0];

            double horizontalDistanceToFood = Globals.GetDistanceFrom(MX, MY, closestFood.MX, MY);

            return Globals.Map(horizontalDistanceToFood, 0, SightRange, 0, 1);
        }
        public double VerticalProximityToFood()
        {
            if (CurrentStepInfo.VisibleFoodOrderedByDistance.Count == 0)
            {
                return 1;
            }

            Food closestFood = CurrentStepInfo.VisibleFoodOrderedByDistance[0];

            double verticalDistanceToFood = Globals.GetDistanceFrom(MX, MY, MX, closestFood.MY);

            return Globals.Map(verticalDistanceToFood, 0, SightRange, 0, 1);
        }
        public double DistanceFromTopWorldBound()
        {
            return Globals.Map(Y, GameHost.WorldBounds.Top, GameHost.WorldBounds.Bottom, 0, 1);
        }
        public double DistanceFromLeftWorldBound()
        {
            return Globals.Map(X, GameHost.WorldBounds.Left, GameHost.WorldBounds.Right, 0, 1);
        }
        public double RandomInput()
        {
            return Globals.Random.NextDouble();
        }
        #endregion

        #region Actions
        private void Eat(Food food)
        {
            Energy -= food.ServingDigestionCost;
            food.Servings -= 1;
            FoodEaten += 1;
            Energy += food.EnergyPerServing;
        }
        public bool TryEatFoodInRange()
        {
            if (CurrentStepInfo.VisibleFoodOrderedByDistance.Count == 0)
            {
                return false;
            }

            Food closestFood = CurrentStepInfo.VisibleFoodOrderedByDistance[0];

            if (Globals.GetDistanceFrom(MX, MY, closestFood.MX, closestFood.MY) > Size / 2)
            {
                return false;
            }

            Eat(closestFood);

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
                Food closestFood = CurrentStepInfo.VisibleFoodOrderedByDistance[0];

                double xDifference = X - closestFood.X;
                double yDifference = Y - closestFood.Y;

                if (xDifference + yDifference <= SightRange)
                {
                    if (yDifference > 0)
                    {
                        if (yDifference >= Speed)
                        {
                            MoveForward();
                        }
                    }
                    else if (yDifference < 0)
                    {
                        if (yDifference <= -Speed)
                        {
                            MoveBackward();
                        }
                    }

                    if (xDifference > 0)
                    {
                        if (xDifference >= Speed)
                        {
                            MoveLeft();
                        }
                    }
                    else if (xDifference < 0)
                    {
                        if (xDifference <= -Speed)
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
