using MaceEvolve.Core.Enums;
using MaceEvolve.Core.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace MaceEvolve.Core.Models
{
    public class Step<TCreature, TFood> : IStep<TCreature, TFood> where TCreature : ICreature, new() where TFood : IFood
    {
        #region Properties
        public ConcurrentQueue<StepAction<TCreature>> RequestedActions { get; set; } = new ConcurrentQueue<StepAction<TCreature>>();
        public ConcurrentDictionary<TCreature, List<NeuralNetworkStepNodeInfo>> CreaturesBrainOutput { get; set; } = new ConcurrentDictionary<TCreature, List<NeuralNetworkStepNodeInfo>>();
        public List<TCreature> Creatures { get; set; }
        public List<TFood> Food { get; set; }
        public IRectangle WorldBounds { get; set; }
        public int MinCreatureConnections { get; set; } = 4;
        public int MaxCreatureConnections { get; set; } = 128;
        public int MaxCreatureProcessNodes { get; set; } = 3;
        public float ConnectionWeightBound { get; set; } = 4;
        public bool LoopWorldBounds { get; set; }
        #endregion

        #region Methods
        public void QueueAction(StepAction<TCreature> stepAction)
        {
            RequestedActions.Enqueue(stepAction);
        }
        public void QueueAction(TCreature creature, CreatureAction creatureAction)
        {
            RequestedActions.Enqueue(new StepAction<TCreature>()
            {
                Creature = creature,
                Action = creatureAction
            });
        }
        public IEnumerable<TCreature> GetVisibleCreatures(TCreature creature)
        {
            return Creatures.Where(x => creature.IsWithinSight(x) && (ICreature)x != (ICreature)creature);
        }
        public IEnumerable<TFood> GetVisibleFood(TCreature creature)
        {
            return Food.Where(x => creature.IsWithinSight(x));
        }
        public IEnumerable<TCreature> GetVisibleCreaturesOrderedByDistance(TCreature creature, IEnumerable<TCreature> visibleCreatures = null)
        {
            if (visibleCreatures == null)
            {
                return GetVisibleCreatures(creature).OrderBy(x => Globals.GetDistanceFrom(creature.X, creature.Y, x.X, x.Y));
            }
            else
            {
                return visibleCreatures.OrderBy(x => Globals.GetDistanceFrom(creature.X, creature.Y, x.X, x.Y));
            }
        }
        public IEnumerable<TFood> GetVisibleFoodOrderedByDistance(TCreature creature, IEnumerable<TFood> visibleFood = null)
        {
            if (visibleFood == null)
            {
                return GetVisibleFood(creature).OrderBy(x => Globals.GetDistanceFrom(creature.X, creature.Y, x.X, x.Y));
            }
            else
            {
                return visibleFood.OrderBy(x => Globals.GetDistanceFrom(creature.X, creature.Y, x.X, x.Y));
            }
        }
        public bool CreatureTryEat(TCreature creature)
        {
            IEnumerable<TFood> VisibleFoodOrderedByDistance = GetVisibleFoodOrderedByDistance(creature);

            IFood closestFood = VisibleFoodOrderedByDistance.FirstOrDefault();

            if (closestFood != null && closestFood.Servings > 0 && Globals.GetDistanceFrom(creature.MX, creature.MY, closestFood.MX, closestFood.MY) < creature.Size / 2)
            {
                closestFood.Servings -= 1;
                creature.Energy -= closestFood.ServingDigestionCost;
                creature.Energy += closestFood.EnergyPerServing;
                creature.Nutrients += closestFood.NutrientsPerServing;
                creature.FoodEaten += 1;

                return true;
            }
            else
            {
                return false;
            }
        }
        public virtual IList<TCreature> CreatureTryReproduce(TCreature creature)
        {
            int numberOfChildrenThatCanBeCreated = (int)MathF.Floor(MathF.Min(creature.Energy / creature.EnergyRequiredToReproduce, creature.Nutrients / creature.NutrientsRequiredToReproduce));

            if (numberOfChildrenThatCanBeCreated == 0)
            {
                return new List<TCreature>();
            }

            List<TCreature> offSpring = new List<TCreature>();

            int childrenToCreate = Math.Min(MaceRandom.Current.Next(numberOfChildrenThatCanBeCreated + 1), creature.MaxOffspringPerReproduction);
            float maxXDistanceOfOffspring = creature.Size * 2;
            float maxYDistanceOfOffspring = creature.Size * 2;

            for (int i = 0; i < childrenToCreate; i++)
            {
                TCreature newCreature = new TCreature();
                newCreature.Brain = NeuralNetwork.CombineNetworks(new List<NeuralNetwork>() { creature.Brain });
                newCreature.Size = creature.Size;
                newCreature.Speed = creature.Speed;
                newCreature.Metabolism = creature.Metabolism;
                newCreature.MoveCost = creature.MoveCost;
                newCreature.SightRange = creature.SightRange;
                newCreature.MaxOffspringPerReproduction = creature.MaxOffspringPerReproduction;
                newCreature.Energy = creature.EnergyRequiredToReproduce / 2;
                newCreature.MaxEnergy = creature.MaxEnergy;
                newCreature.Nutrients = creature.NutrientsRequiredToReproduce / 2;
                newCreature.NutrientsRequiredToReproduce = creature.NutrientsRequiredToReproduce;
                newCreature.EnergyRequiredToReproduce = creature.EnergyRequiredToReproduce;
                newCreature.OffspringBrainMutationAttempts = creature.OffspringBrainMutationAttempts;

                newCreature.X = creature.X + MaceRandom.Current.NextFloat(-maxXDistanceOfOffspring, maxXDistanceOfOffspring + 1);
                newCreature.Y = creature.Y + MaceRandom.Current.NextFloat(-maxYDistanceOfOffspring, maxYDistanceOfOffspring + 1);

                if (creature.MX < WorldBounds.X)
                {
                    if (LoopWorldBounds)
                    {
                        creature.X = (WorldBounds.X + WorldBounds.Width) - creature.Size / 2;
                    }
                    else
                    {
                        creature.X = WorldBounds.X - creature.Size / 2;
                    }
                }
                else if (creature.MX > WorldBounds.X + WorldBounds.Width)
                {
                    if (LoopWorldBounds)
                    {
                        creature.X = WorldBounds.X - creature.Size / 2;
                    }
                    else
                    {
                        creature.X = (WorldBounds.X + WorldBounds.Width) - creature.Size / 2;
                    }
                }

                if (creature.MY < WorldBounds.Y)
                {
                    if (LoopWorldBounds)
                    {
                        creature.Y = (WorldBounds.Y + WorldBounds.Height) - creature.Size / 2;
                    }
                    else
                    {
                        creature.Y = WorldBounds.Y - creature.Size / 2;
                    }
                }
                else if (creature.MY > WorldBounds.Y + WorldBounds.Height)
                {
                    if (LoopWorldBounds)
                    {
                        creature.Y = WorldBounds.Y - creature.Size / 2;
                    }
                    else
                    {
                        creature.Y = (WorldBounds.Y + WorldBounds.Height) - creature.Size / 2;
                    }
                }

                creature.Energy -= creature.EnergyRequiredToReproduce;
                creature.Nutrients -= creature.NutrientsRequiredToReproduce;

                for (int j = 0; j < creature.OffspringBrainMutationAttempts; j++)
                {
                    bool mutated = newCreature.Brain.MutateNetwork(
                        createRandomNodeChance: creature.OffspringBrainMutationChance,
                        removeRandomNodeChance: creature.OffspringBrainMutationChance / 20,
                        mutateRandomNodeBiasChance: creature.OffspringBrainMutationChance,
                        createRandomConnectionChance: creature.OffspringBrainMutationChance,
                        removeRandomConnectionChance: creature.OffspringBrainMutationChance,
                        mutateRandomConnectionSourceChance: creature.OffspringBrainMutationChance,
                        mutateRandomConnectionTargetChance: creature.OffspringBrainMutationChance,
                        mutateRandomConnectionWeightChance: creature.OffspringBrainMutationChance,
                        possibleInputs: Globals.AllCreatureInputs,
                        possibleOutputs: Globals.AllCreatureActions,
                        minCreatureConnections: MinCreatureConnections,
                        maxCreatureConnections: MaxCreatureConnections,
                        maxCreatureProcessNodes: MaxCreatureProcessNodes,
                        connectionWeightBound: ConnectionWeightBound);
                }

                creature.TimesReproduced += 1;
                offSpring.Add(newCreature);
            }

            return offSpring;
        }
        public static TCreature CreateOffSpring(IList<TCreature> parents)
        {
            Dictionary<TCreature, List<Connection>> availableParentConnections = parents.ToDictionary(x => x, x => x.Brain.Connections.ToList());
            Dictionary<TCreature, Dictionary<int, int>> parentToOffspringNodesMap = new Dictionary<TCreature, Dictionary<int, int>>();

            TCreature offspring = new TCreature()
            {
                Brain = new NeuralNetwork()
            };

            float averageNumberOfParentConnections = (float)parents.Average(x => x.Brain.Connections.Count);

            if (averageNumberOfParentConnections > 0 && averageNumberOfParentConnections < 1)
            {
                averageNumberOfParentConnections = 1;
            }

            while (offspring.Brain.Connections.Count < averageNumberOfParentConnections)
            {
                TCreature randomParent = parents[MaceRandom.Current.Next(parents.Count)];
                List<Connection> randomParentAvailableConnections = availableParentConnections[randomParent];

                if (randomParentAvailableConnections.Count > 0)
                {
                    Connection randomParentConnection = randomParentAvailableConnections[MaceRandom.Current.Next(randomParentAvailableConnections.Count)];

                    //If a parent's node has not been added and mapped to an offspring's node, create a new node and map it to the parent's node.
                    if (!(parentToOffspringNodesMap.ContainsKey(randomParent) && parentToOffspringNodesMap[randomParent].ContainsKey(randomParentConnection.SourceId)))
                    {
                        Node randomParentConnectionSourceNode = randomParent.Brain.NodeIdsToNodesDict[randomParentConnection.SourceId];
                        Node newNode = new Node(randomParentConnectionSourceNode.NodeType, randomParentConnectionSourceNode.Bias, randomParentConnectionSourceNode.CreatureInput, randomParentConnectionSourceNode.CreatureAction);
                        int newNodeId = offspring.Brain.AddNode(newNode);

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

                    //Apply any variance to the connection's weight.
                    float connectionToAddWeight = randomParentConnection.Weight;
                    int connectionToAddSourceId = parentToOffspringNodesMap[randomParent][randomParentConnection.SourceId];
                    int connectionToAddTargetId = parentToOffspringNodesMap[randomParent][randomParentConnection.TargetId];

                    offspring.Brain.Connections.Add(new Connection(connectionToAddSourceId, connectionToAddTargetId, connectionToAddWeight));
                    availableParentConnections[randomParent].Remove(randomParentConnection);
                }
            }

            return offspring;
        }
        public void CreatureMoveForwards(TCreature creature)
        {
            creature.Y -= creature.Speed;
            if (creature.MY < WorldBounds.Y)
            {
                if (LoopWorldBounds)
                {
                    creature.Y = ((WorldBounds.Y + WorldBounds.Height) - creature.Size / 2) - (WorldBounds.Y - creature.MY);
                }
                else
                {
                    creature.Y += creature.Speed;
                }

                //Y += WorldBounds.WorldBounds.Height;
            }
            creature.Energy -= creature.MoveCost;
        }
        public void CreatureMoveBackwards(TCreature creature)
        {
            creature.Y += creature.Speed;
            float worldBoundsBottom = WorldBounds.Y + WorldBounds.Height;
            if (creature.MY > worldBoundsBottom)
            {
                if (LoopWorldBounds)
                {
                    creature.Y = (WorldBounds.Y - creature.Size / 2) + (creature.MY - worldBoundsBottom);
                }
                else
                {
                    creature.Y -= creature.Speed;
                }

                //Y -= WorldBounds.WorldBounds.Height;
            }
            creature.Energy -= creature.MoveCost;
        }
        public void CreatureMoveLeft(TCreature creature)
        {
            creature.X -= creature.Speed;
            if (creature.MX < WorldBounds.X)
            {
                if (LoopWorldBounds)
                {
                    creature.X = ((WorldBounds.X + WorldBounds.Width) - creature.Size / 2) - (WorldBounds.X - creature.MX);
                }
                else
                {
                    creature.X += creature.Speed;
                }

                //X += WorldBounds.WorldBounds.Width;
            }
            creature.Energy -= creature.MoveCost;
        }
        public void CreatureMoveRight(TCreature creature)
        {
            creature.X += creature.Speed;
            float worldBoundsRight = WorldBounds.X + WorldBounds.Width;
            if (creature.MX > worldBoundsRight)
            {
                if (LoopWorldBounds)
                {
                    creature.X = (WorldBounds.X - creature.Size / 2) + (creature.MX - worldBoundsRight);
                }
                else
                {
                    creature.X -= creature.Speed;
                }
                //X -= WorldBounds.WorldBounds.Width;
            }
            creature.Energy -= creature.MoveCost;
        }
        public void CreatureDoNothing()
        {

        }
        public void CreatureMoveTowardsClosestFood(TCreature creature)
        {
            IEnumerable<TFood> visibleFoodOrderedByDistance = GetVisibleFoodOrderedByDistance(creature);

            if (visibleFoodOrderedByDistance.Any())
            {
                IFood closestFood = visibleFoodOrderedByDistance.First();

                float xDifference = creature.X - closestFood.X;
                float yDifference = creature.Y - closestFood.Y;

                if (xDifference + yDifference <= creature.SightRange)
                {
                    if (yDifference > 0)
                    {
                        if (yDifference >= creature.Speed)
                        {
                            CreatureMoveForwards(creature);
                        }
                    }
                    else if (yDifference < 0)
                    {
                        if (yDifference <= -creature.Speed)
                        {
                            CreatureMoveBackwards(creature);
                        }
                    }

                    if (xDifference > 0)
                    {
                        if (xDifference >= creature.Speed)
                        {
                            CreatureMoveLeft(creature);
                        }
                    }
                    else if (xDifference < 0)
                    {
                        if (xDifference <= -creature.Speed)
                        {
                            CreatureMoveRight(creature);
                        }
                    }
                }
            }
        }
        public void ExecuteActions(IEnumerable<StepAction<TCreature>> stepActions)
        {
            foreach (var stepAction in stepActions)
            {
                if (!stepAction.Creature.IsDead)
                {
                    switch (stepAction.Action)
                    {
                        case CreatureAction.MoveForward:
                            CreatureMoveForwards(stepAction.Creature);
                            break;

                        case CreatureAction.MoveBackward:
                            CreatureMoveBackwards(stepAction.Creature);

                            break;

                        case CreatureAction.MoveLeft:
                            CreatureMoveLeft(stepAction.Creature);
                            break;

                        case CreatureAction.MoveRight:
                            CreatureMoveRight(stepAction.Creature);
                            break;

                        case CreatureAction.TryEat:
                            CreatureTryEat(stepAction.Creature);
                            break;

                        case CreatureAction.TryReproduce:
                            IList<TCreature> offSpring = CreatureTryReproduce(stepAction.Creature);
                            if (offSpring.Count > 0)
                            {
                                Creatures.AddRange(offSpring);
                            }
                            break;

                        case CreatureAction.DoNothing:
                            CreatureDoNothing();
                            break;

                        default:
                            throw new NotImplementedException();
                    }

                    stepAction.Creature.Energy -= stepAction.Creature.Metabolism;

                    if (stepAction.Creature.Energy <= 0)
                    {
                        stepAction.Creature.Die();
                    }
                }
            }
        }
        public Dictionary<CreatureInput, float> GenerateCreatureInputValues(IEnumerable<CreatureInput> creatureInputs, TCreature creature)
        {
            Dictionary<CreatureInput, float> creatureInputValues = new Dictionary<CreatureInput, float>();

            List<TCreature> visibleCreatures = null;
            List<TFood> visibleFood = null;
            List<TCreature> visibleCreaturesOrderedByDistance = null;
            List<TFood> visibleFoodOrderedByDistance = null;

            foreach (var creatureInput in creatureInputs)
            {
                float creatureInputValue;

                if (!creatureInputValues.TryGetValue(creatureInput, out creatureInputValue))
                {
                    switch (creatureInput)
                    {
                        case CreatureInput.PercentMaxEnergy:
                            creatureInputValue = Globals.Map(creature.Energy, 0, creature.MaxEnergy, 0, 1);
                            break;

                        case CreatureInput.ProximityToCreatureToLeft:
                            visibleCreatures ??= GetVisibleCreatures(creature).ToList();
                            visibleCreaturesOrderedByDistance ??= GetVisibleCreaturesOrderedByDistance(creature, visibleCreatures).ToList();

                            ICreature closestCreatureToLeft = visibleCreaturesOrderedByDistance.Find(x => x.MX <= creature.MX);

                            if (closestCreatureToLeft == null)
                            {
                                creatureInputValue = 0;
                            }
                            else
                            {
                                float distanceFromClosestCreatureToLeft = Globals.GetDistanceFrom(creature.MX, creature.MY, closestCreatureToLeft.MX, creature.MY);

                                creatureInputValue = Globals.Map(distanceFromClosestCreatureToLeft, 0, creature.SightRange, 1, 0);
                            }
                            break;

                        case CreatureInput.ProximityToCreatureToRight:
                            visibleCreatures ??= GetVisibleCreatures(creature).ToList();
                            visibleCreaturesOrderedByDistance ??= GetVisibleCreaturesOrderedByDistance(creature, visibleCreatures).ToList();

                            ICreature closestCreatureToRight = visibleCreaturesOrderedByDistance.Find(x => x.MX >= creature.MX);

                            if (closestCreatureToRight == null)
                            {
                                creatureInputValue = 0;
                            }
                            else
                            {
                                float distanceFromClosestCreatureToRight = Globals.GetDistanceFrom(creature.MX, creature.MY, closestCreatureToRight.MX, creature.MY);

                                creatureInputValue = Globals.Map(distanceFromClosestCreatureToRight, 0, creature.SightRange, 1, 0);
                            }
                            break;

                        case CreatureInput.ProximityToCreatureToFront:
                            visibleCreatures ??= GetVisibleCreatures(creature).ToList();
                            visibleCreaturesOrderedByDistance ??= GetVisibleCreaturesOrderedByDistance(creature, visibleCreatures).ToList();

                            ICreature closestCreatureToFront = visibleCreaturesOrderedByDistance.Find(x => x.MY <= creature.MY);

                            if (closestCreatureToFront == null)
                            {
                                creatureInputValue = 0;
                            }
                            else
                            {
                                float distanceFromClosestCreatureToFront = Globals.GetDistanceFrom(creature.MX, creature.MY, creature.MX, closestCreatureToFront.MY);

                                creatureInputValue = Globals.Map(distanceFromClosestCreatureToFront, 0, creature.SightRange, 1, 0);
                            }
                            break;

                        case CreatureInput.ProximityToCreatureToBack:
                            visibleCreatures ??= GetVisibleCreatures(creature).ToList();
                            visibleCreaturesOrderedByDistance ??= GetVisibleCreaturesOrderedByDistance(creature, visibleCreatures).ToList();

                            ICreature closestCreatureToBack = visibleCreaturesOrderedByDistance.Find(x => x.MY >= creature.MY);

                            if (closestCreatureToBack == null)
                            {
                                creatureInputValue = 0;
                            }
                            else
                            {
                                float distanceFromClosestCreatureToBack = Globals.GetDistanceFrom(creature.MX, creature.MY, creature.MX, closestCreatureToBack.MY);

                                creatureInputValue = Globals.Map(distanceFromClosestCreatureToBack, 0, creature.SightRange, 1, 0);
                            }
                            break;

                        case CreatureInput.ProximityToFoodToLeft:
                            visibleFood ??= GetVisibleFood(creature).ToList();
                            visibleFoodOrderedByDistance ??= GetVisibleFoodOrderedByDistance(creature, visibleFood).ToList();

                            IFood closestFoodToLeft = visibleFoodOrderedByDistance.Find(x => x.MX <= creature.MX);

                            if (closestFoodToLeft == null)
                            {
                                creatureInputValue = 0;
                            }
                            else
                            {
                                float distanceFromClosestFoodToLeft = Globals.GetDistanceFrom(creature.MX, creature.MY, closestFoodToLeft.MX, creature.MY);

                                creatureInputValue = Globals.Map(distanceFromClosestFoodToLeft, 0, creature.SightRange, 1, 0);
                            }
                            break;

                        case CreatureInput.ProximityToFoodToRight:
                            visibleFood ??= GetVisibleFood(creature).ToList();
                            visibleFoodOrderedByDistance ??= GetVisibleFoodOrderedByDistance(creature, visibleFood).ToList();

                            IFood closestFoodToRight = visibleFoodOrderedByDistance.Find(x => x.MX >= creature.MX);

                            if (closestFoodToRight == null)
                            {
                                creatureInputValue = 0;
                            }
                            else
                            {
                                float distanceFromClosestFoodToRight = Globals.GetDistanceFrom(creature.MX, creature.MY, closestFoodToRight.MX, creature.MY);

                                creatureInputValue = Globals.Map(distanceFromClosestFoodToRight, 0, creature.SightRange, 1, 0);
                            }
                            break;

                        case CreatureInput.ProximityToFoodToFront:
                            visibleFood ??= GetVisibleFood(creature).ToList();
                            visibleFoodOrderedByDistance ??= GetVisibleFoodOrderedByDistance(creature, visibleFood).ToList();

                            IFood closestFoodToFront = visibleFoodOrderedByDistance.Find(x => x.MY <= creature.MY);

                            if (closestFoodToFront == null)
                            {
                                creatureInputValue = 0;
                            }
                            else
                            {
                                float distanceFromClosestFoodToFront = Globals.GetDistanceFrom(creature.MX, creature.MY, creature.MX, closestFoodToFront.MY);

                                creatureInputValue = Globals.Map(distanceFromClosestFoodToFront, 0, creature.SightRange, 1, 0);
                            }
                            break;

                        case CreatureInput.ProximityToFoodToBack:
                            visibleFood ??= GetVisibleFood(creature).ToList();
                            visibleFoodOrderedByDistance ??= GetVisibleFoodOrderedByDistance(creature, visibleFood).ToList();

                            IFood closestFoodToBack = visibleFoodOrderedByDistance.Find(x => x.MY >= creature.MY);

                            if (closestFoodToBack == null)
                            {
                                creatureInputValue = 0;
                            }
                            else
                            {
                                float distanceFromClosestFoodToBack = Globals.GetDistanceFrom(creature.MX, creature.MY, creature.MX, closestFoodToBack.MY);

                                creatureInputValue = Globals.Map(distanceFromClosestFoodToBack, 0, creature.SightRange, 1, 0);
                            }

                            break;

                        case CreatureInput.DistanceFromTopWorldBound:
                            creatureInputValue = Globals.Map(creature.Y, WorldBounds.Y, WorldBounds.Y + WorldBounds.Height, 0, 1);
                            break;

                        case CreatureInput.DistanceFromLeftWorldBound:
                            creatureInputValue = Globals.Map(creature.X, WorldBounds.X, WorldBounds.X + WorldBounds.Width, 0, 1);
                            break;

                        case CreatureInput.RandomInput:
                            creatureInputValue = MaceRandom.Current.NextFloat();
                            break;

                        case CreatureInput.PercentNutrientsRequiredToReproduce:
                            creatureInputValue = Globals.Map(creature.Nutrients, 0, creature.NutrientsRequiredToReproduce, 0, 1);
                            break;

                        case CreatureInput.PercentEnergyRequiredToReproduce:
                            creatureInputValue = Globals.Map(creature.Energy, 0, creature.EnergyRequiredToReproduce, 0, 1);
                            break;

                        default:
                            throw new NotImplementedException($"{nameof(CreatureInput)} '{creatureInput}' has not been implemented.");
                    }
                }

                creatureInputValues[creatureInput] = creatureInputValue;
            }

            return creatureInputValues;
        }
        #endregion
    }
}
