using MaceEvolve.Core.Enums;
using MaceEvolve.Core.Extensions;
using MaceEvolve.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace MaceEvolve.Core.Models
{
    public class GameHost<TCreature, TFood> where TCreature : class, ICreature, new() where TFood : class, IFood, new()
    {
        #region Fields
        protected static Random random = new Random();
        protected TCreature bestCreature;
        protected TCreature selectedCreature;
        #endregion

        #region Properties
        public Step<TCreature, TFood> CurrentStep { get; set; }
        public int MaxCreatureAmount { get; set; } = 300;
        public int MaxFoodAmount { get; set; } = 350;
        public IRectangle WorldBounds { get; set; } = new Rectangle(0, 0, 512, 512);
        public IRectangle SuccessBounds { get; set; }
        public int MinCreatureConnections { get; set; } = 4;
        public int MaxCreatureConnections { get; set; } = 128;
        public float CreatureSpeed { get; set; }
        public int MaxCreatureProcessNodes { get; set; } = 3;
        public float MutationChance { get; set; } = 1 / 3f;
        public int MutationAttempts { get; set; } = 1;
        public float ConnectionWeightBound { get; set; } = 4;
        public float MaxCreatureEnergy { get; set; } = 150;
        public float FoodSize { get; set; } = 7;
        public float CreatureSize { get; set; } = 10;
        public float MinimumSuccessfulCreatureFitness { get; set; } = 0.5f;
        public float ReproductionNodeBiasVariance { get; set; }
        public float ReproductionConnectionWeightVariance { get; set; }
        public ReadOnlyCollection<CreatureInput> PossibleCreatureInputs { get; } = Globals.AllCreatureInputs;
        public ReadOnlyCollection<CreatureAction> PossibleCreatureActions { get; } = Globals.AllCreatureActions;
        public bool UseSuccessBounds { get; set; }
        public TCreature SelectedCreature
        {
            get
            {
                return selectedCreature;
            }
            set
            {
                if (selectedCreature != value)
                {
                    var oldSelectedCreature = selectedCreature;
                    selectedCreature = value;

                    OnSelectedCreatureChanged(this, new ValueChangedEventArgs<TCreature>(oldSelectedCreature, selectedCreature));
                }
            }
        }
        public TCreature BestCreature
        {
            get
            {
                return bestCreature;
            }
            set
            {
                if (bestCreature != value)
                {
                    var oldBestCreature = bestCreature;
                    bestCreature = value;

                    OnBestCreatureChanged(this, new ValueChangedEventArgs<TCreature>(oldBestCreature, bestCreature));
                }
            }
        }
        #endregion

        #region Events
        public event EventHandler<ValueChangedEventArgs<TCreature>> BestCreatureChanged;
        public event EventHandler<ValueChangedEventArgs<TCreature>> SelectedCreatureChanged;
        #endregion

        #region Methods
        public virtual void Reset()
        {
            CurrentStep = null;
            BestCreature = null;
            SelectedCreature = null;
        }
        public virtual List<TCreature> CreateNewGenerationSexual(IEnumerable<TCreature> sourceCreatures)
        {
            Dictionary<TCreature, float> successfulCreaturesFitnesses = GetFitnesses(sourceCreatures);
            Dictionary<TCreature, float> topPercentileCreatureFitnessesOrderedDescending = successfulCreaturesFitnesses.Where(x => x.Value >= MinimumSuccessfulCreatureFitness).OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
            List<TCreature> topPercentileCreatureFitnessesOrderedDescendingList = topPercentileCreatureFitnessesOrderedDescending.Keys.ToList();

            if (topPercentileCreatureFitnessesOrderedDescending.Count == 0)
            {
                return new List<TCreature>();
            }

            List<TCreature> newCreatures = new List<TCreature>();

            while (newCreatures.Count < MaxCreatureAmount)
            {
                foreach (var keyValuePair in topPercentileCreatureFitnessesOrderedDescending)
                {
                    TCreature successfulCreature = keyValuePair.Key;
                    float successfulCreatureFitness = keyValuePair.Value;

                    if (random.NextFloat() <= successfulCreatureFitness)
                    {
                        int numberOfChildrenToCreate = UseSuccessBounds ? (int)Globals.Map(successfulCreatureFitness, 0, 1, 0, MaxCreatureAmount / 10) : successfulCreature.FoodEaten;

                        for (int i = 0; i < numberOfChildrenToCreate; i++)
                        {
                            if (newCreatures.Count >= MaxCreatureAmount)
                            {
                                break;
                            }

                            TCreature newCreature = Creature.Reproduce(topPercentileCreatureFitnessesOrderedDescendingList);
                            newCreature.X = random.NextFloat(0, WorldBounds.X + WorldBounds.Width);
                            newCreature.Y = random.NextFloat(0, WorldBounds.Y + WorldBounds.Height);
                            newCreature.Size = CreatureSize;
                            newCreature.Speed = CreatureSpeed;
                            newCreature.Metabolism = 0.1f;
                            newCreature.Energy = MaxCreatureEnergy;
                            newCreature.MaxEnergy = MaxCreatureEnergy;
                            newCreature.SightRange = 100;

                            for (int j = 0; j < MutationAttempts; j++)
                            {
                                bool mutated = MutateNetwork(newCreature.Brain,
                                    createRandomNodeChance: MutationChance,
                                    removeRandomNodeChance: MutationChance / 20,
                                    mutateRandomNodeBiasChance: MutationChance,
                                    createRandomConnectionChance: MutationChance,
                                    removeRandomConnectionChance: MutationChance,
                                    mutateRandomConnectionSourceChance: MutationChance,
                                    mutateRandomConnectionTargetChance: MutationChance,
                                    mutateRandomConnectionWeightChance: MutationChance);
                            }

                            newCreatures.Add(newCreature);
                        }
                    }
                }
            }

            return newCreatures;
        }
        public virtual List<TCreature> CreateNewGenerationAsexual(IEnumerable<TCreature> sourceCreatures)
        {
            Dictionary<TCreature, float> successfulCreaturesFitnesses = GetFitnesses(sourceCreatures);
            Dictionary<TCreature, float> topPercentileCreatureFitnessesOrderedDescending = successfulCreaturesFitnesses.Where(x => x.Value >= MinimumSuccessfulCreatureFitness).OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);

            if (topPercentileCreatureFitnessesOrderedDescending.Count == 0)
            {
                return new List<TCreature>();
            }

            List<TCreature> newCreatures = new List<TCreature>();

            while (newCreatures.Count < MaxCreatureAmount)
            {
                foreach (var keyValuePair in topPercentileCreatureFitnessesOrderedDescending)
                {
                    TCreature successfulCreature = keyValuePair.Key;
                    float successfulCreatureFitness = keyValuePair.Value;

                    if (random.NextFloat() <= successfulCreatureFitness)
                    {
                        int numberOfChildrenToCreate = UseSuccessBounds ? (int)Globals.Map(successfulCreatureFitness, 0, 1, 0, MaxCreatureAmount / 10) : successfulCreature.FoodEaten;

                        for (int i = 0; i < numberOfChildrenToCreate; i++)
                        {
                            if (newCreatures.Count >= MaxCreatureAmount)
                            {
                                break;
                            }

                            TCreature newCreature = Creature.Reproduce(new List<TCreature>() { successfulCreature });
                            newCreature.X = random.NextFloat(0, WorldBounds.X + WorldBounds.Width);
                            newCreature.Y = random.NextFloat(0, WorldBounds.Y + WorldBounds.Height);
                            newCreature.Size = CreatureSize;
                            newCreature.Speed = CreatureSpeed;
                            newCreature.Metabolism = 0.1f;
                            newCreature.Energy = MaxCreatureEnergy;
                            newCreature.MaxEnergy = MaxCreatureEnergy;
                            newCreature.SightRange = 100;

                            for (int j = 0; j < MutationAttempts; j++)
                            {
                                bool mutated = MutateNetwork(newCreature.Brain,
                                    createRandomNodeChance: MutationChance,
                                    removeRandomNodeChance: MutationChance / 20,
                                    mutateRandomNodeBiasChance: MutationChance,
                                    createRandomConnectionChance: MutationChance,
                                    removeRandomConnectionChance: MutationChance,
                                    mutateRandomConnectionSourceChance: MutationChance,
                                    mutateRandomConnectionTargetChance: MutationChance,
                                    mutateRandomConnectionWeightChance: MutationChance);
                            }

                            newCreatures.Add(newCreature);
                        }
                    }
                }
            }

            return newCreatures;
        }
        public virtual Dictionary<TCreature, float> GetFitnesses(IEnumerable<TCreature> creatures)
        {
            if (creatures == null) { throw new ArgumentNullException(); }

            if (!creatures.Any())
            {
                return new Dictionary<TCreature, float>();
            }

            Dictionary<TCreature, float> successfulCreaturesFitnesses = new Dictionary<TCreature, float>();

            if (UseSuccessBounds)
            {
                float successBoundsMiddleX = Globals.MiddleX(SuccessBounds.X, SuccessBounds.Width);
                float successBoundsMiddleY = Globals.MiddleY(SuccessBounds.Y, SuccessBounds.Height);

                foreach (var creature in creatures)
                {
                    float distanceFromMiddle = Globals.GetDistanceFrom(creature.MX, creature.MY, successBoundsMiddleX, successBoundsMiddleY);
                    float successBoundsHypotenuse = Globals.Hypotenuse(SuccessBounds.Width, SuccessBounds.Height);

                    successfulCreaturesFitnesses.Add(creature, Globals.Map(distanceFromMiddle, 0, successBoundsHypotenuse, 1, 0)); ;
                }
            }
            else
            {
                int mostFoodEaten = creatures.Max(x => x.FoodEaten);

                if (mostFoodEaten == 0)
                {
                    return new Dictionary<TCreature, float>();
                }

                successfulCreaturesFitnesses = creatures.ToDictionary(
                    x => x,
                    x => (float)x.FoodEaten / mostFoodEaten);
            }

            return successfulCreaturesFitnesses;
        }
        public virtual bool MutateNetwork(NeuralNetwork network, float createRandomNodeChance, float removeRandomNodeChance, float mutateRandomNodeBiasChance, float createRandomConnectionChance, float removeRandomConnectionChance, float mutateRandomConnectionSourceChance, float mutateRandomConnectionTargetChance, float mutateRandomConnectionWeightChance)
        {
            int processNodeCount = network.GetNodeIds((_, node) => node.NodeType == NodeType.Process).Count;
            bool mutationAttempted = false;

            //Things should be removed before being added so that there isn't a chance that the newly added thing is deleted straight after.
            //Connections should be added after nodes are added so that there is a chance the newly created node gets a connection.

            //Remove an existing node. Input nodes should not be removed. 
            if (random.NextFloat() <= removeRandomNodeChance)
            {
                mutationAttempted = true;

                List<int> processNodeIds = network.GetNodeIds(predicate: (_, node) => node.NodeType == NodeType.Process);
                List<int> outputNodeIds = network.GetNodeIds(predicate: (_, node) => node.NodeType == NodeType.Output);

                List<int> possibleNodeIdsToRemove = new List<int>(processNodeIds);

                //There must be at least one target node in a network.
                if (outputNodeIds.Count > 1)
                {
                    possibleNodeIdsToRemove.AddRange(outputNodeIds);
                }

                if (possibleNodeIdsToRemove.Count > 0)
                {
                    int nodeIdToRemove = possibleNodeIdsToRemove[random.Next(possibleNodeIdsToRemove.Count)];

                    network.RemoveNode(nodeIdToRemove, true);
                }
            }

            //Change a random node's bias.
            if (random.NextFloat() <= mutateRandomNodeBiasChance)
            {
                mutationAttempted = true;

                int randomNodeId = network.GetNodeIds()[random.Next(network.NodeIdsToNodesDict.Count)];
                Node randomNode = network.NodeIdsToNodesDict[randomNodeId];
                Node newNode = new Node(randomNode.NodeType, random.NextFloat(-1, 1), randomNode.CreatureInput, randomNode.CreatureAction);

                network.ReplaceNode(randomNodeId, newNode);
            }

            //Create a new node with a default connection.
            if (random.NextFloat() <= createRandomNodeChance)
            {
                List<CreatureInput> possibleCreatureInputsToAdd = GetPossibleInputsToAdd(network).ToList();
                List<CreatureAction> possibleCreatureActionsToAdd = GetPossibleActionsToAdd(network).ToList();

                mutationAttempted = true;

                Node nodeToAdd;
                float nodeTypeRandomNum = random.NextFloat();
                float chanceForSingleNodeType = 1f / Globals.AllNodeTypes.Count;

                if (nodeTypeRandomNum <= chanceForSingleNodeType && possibleCreatureInputsToAdd.Count > 0)
                {
                    nodeToAdd = new Node(NodeType.Input, random.NextFloat(-1, 1), possibleCreatureInputsToAdd[random.Next(possibleCreatureInputsToAdd.Count)]);
                }
                else if (nodeTypeRandomNum <= chanceForSingleNodeType * 2 && possibleCreatureActionsToAdd.Count > 0)
                {
                    nodeToAdd = new Node(NodeType.Output, random.NextFloat(-1, 1), creatureAction: possibleCreatureActionsToAdd[random.Next(possibleCreatureActionsToAdd.Count)]);
                }
                else if (processNodeCount < MaxCreatureProcessNodes)
                {
                    nodeToAdd = new Node(NodeType.Process, random.NextFloat(-1, 1));
                }
                else
                {
                    nodeToAdd = null;
                }

                if (nodeToAdd != null)
                {
                    List<int> possibleSourceNodesIds = network.GetNodeIds((_, node) => node.NodeType == NodeType.Input || node.NodeType == NodeType.Process);
                    List<int> possibleTargetNodesIds = network.GetNodeIds((_, node) => node.NodeType == NodeType.Output || node.NodeType == NodeType.Process);

                    int nodeToAddId = network.AddNode(nodeToAdd);

                    if (network.Connections.Count < MaxCreatureConnections && possibleSourceNodesIds.Count > 0 && possibleTargetNodesIds.Count > 0)
                    {
                        Connection newConnection;
                        float newConnectionWeight = random.NextFloat(-ConnectionWeightBound, ConnectionWeightBound);

                        switch (nodeToAdd.NodeType)
                        {
                            case NodeType.Input:
                                newConnection = new Connection(nodeToAddId, possibleTargetNodesIds[random.Next(possibleTargetNodesIds.Count)], newConnectionWeight);
                                break;

                            case NodeType.Process:
                                if (random.NextDouble() <= 0.5)
                                {
                                    newConnection = new Connection(nodeToAddId, possibleTargetNodesIds[random.Next(possibleTargetNodesIds.Count)], newConnectionWeight);
                                }
                                else
                                {
                                    newConnection = new Connection(possibleSourceNodesIds[random.Next(possibleSourceNodesIds.Count)], nodeToAddId, newConnectionWeight);
                                }
                                break;

                            case NodeType.Output:
                                newConnection = new Connection(possibleSourceNodesIds[random.Next(possibleSourceNodesIds.Count)], nodeToAddId, newConnectionWeight);
                                break;

                            default:
                                throw new NotImplementedException();
                        }

                        network.Connections.Add(newConnection);
                    }
                }
            }

            //Remove a random connection.
            if (network.Connections.Count > MinCreatureConnections && random.NextFloat() <= removeRandomConnectionChance)
            {
                mutationAttempted = true;

                Connection randomConnection = network.Connections[random.Next(network.Connections.Count)];
                network.Connections.Remove(randomConnection);
            }

            //Change a random connection's weight.
            if (network.Connections.Count > 0 && random.NextFloat() <= mutateRandomConnectionWeightChance)
            {
                mutationAttempted = true;

                int randomConnectionIndex = random.Next(network.Connections.Count);

                network.Connections[randomConnectionIndex] = new Connection(network.Connections[randomConnectionIndex].SourceId, network.Connections[randomConnectionIndex].TargetId, random.NextFloat(-ConnectionWeightBound, ConnectionWeightBound));
            }

            //Change a random connection's source.
            if (network.Connections.Count > 0 && Globals.Random.NextFloat() <= mutateRandomConnectionSourceChance)
            {
                mutationAttempted = true;

                int randomConnectionIndex = random.Next(network.Connections.Count);
                List<int> possibleSourceNodesIds = network.GetNodeIds(predicate: (_, node) => node.NodeType == NodeType.Input || node.NodeType == NodeType.Process);

                if (possibleSourceNodesIds.Count > 0)
                {
                    int randomSourceNodeId = possibleSourceNodesIds[Globals.Random.Next(possibleSourceNodesIds.Count)];

                    Connection mutatedConnection = new Connection(randomSourceNodeId, network.Connections[randomConnectionIndex].TargetId, network.Connections[randomConnectionIndex].Weight);
                    network.Connections[randomConnectionIndex] = mutatedConnection;
                }
            }

            //Change a random connection's target.
            if (network.Connections.Count > 0 && Globals.Random.NextFloat() <= mutateRandomConnectionTargetChance)
            {
                mutationAttempted = true;

                int randomConnectionIndex = random.Next(network.Connections.Count);
                List<int> possibleTargetNodesIds = network.GetNodeIds(predicate: (_, node) => node.NodeType == NodeType.Output || node.NodeType == NodeType.Process);

                if (possibleTargetNodesIds.Count > 0)
                {
                    int randomTargetNodeId = possibleTargetNodesIds[Globals.Random.Next(possibleTargetNodesIds.Count)];

                    Connection mutatedConnection = new Connection(network.Connections[randomConnectionIndex].SourceId, randomTargetNodeId, network.Connections[randomConnectionIndex].Weight);
                    network.Connections[randomConnectionIndex] = mutatedConnection;
                }
            }

            //Create a new connection.
            if (network.Connections.Count < MaxCreatureConnections && random.NextDouble() <= createRandomConnectionChance)
            {
                mutationAttempted = true;

                Connection? newConnection = network.GenerateRandomConnections(1, 1, ConnectionWeightBound).FirstOrDefault();

                if (newConnection != null)
                {
                    network.Connections.Add(newConnection.Value);
                }
            }

            return mutationAttempted;
        }
        public virtual IEnumerable<CreatureInput> GetPossibleInputsToAdd(NeuralNetwork network)
        {
            //Return any inputs that aren't already used by a node in the network.
            return PossibleCreatureInputs.Where(x => !network.NodeIdsToNodesDict.Any(y => y.Value.NodeType == NodeType.Input && x == y.Value.CreatureInput));
        }
        public virtual IEnumerable<CreatureAction> GetPossibleActionsToAdd(NeuralNetwork network)
        {
            //Return any actions that aren't already used by a node in the network.
            return PossibleCreatureActions.Where(x => !network.NodeIdsToNodesDict.Any(y => y.Value.NodeType == NodeType.Output && x == y.Value.CreatureAction));
        }
        public virtual void NextStep(bool gatherInfoForAllCreatures = false)
        {
            Step<TCreature, TFood>.ExecuteActions(CurrentStep.RequestedActions, CurrentStep);

            Step<TCreature, TFood> generatedStep = new Step<TCreature, TFood>(CurrentStep.Creatures.ToList(), CurrentStep.Food.Where(x => x.Servings > 0).ToList(), WorldBounds);

            TCreature newBestCreature = null;

            float successBoundsMiddleX = Globals.MiddleX(SuccessBounds.X, SuccessBounds.Width);
            float successBoundsMiddleY = Globals.MiddleY(SuccessBounds.Y, SuccessBounds.Height);

            foreach (TCreature creature in generatedStep.Creatures)
            {
                if (!creature.IsDead || creature == BestCreature || creature == SelectedCreature)
                {
                    IEnumerable<CreatureInput> inputsRequiredForStep = creature.Brain.GetInputsRequiredForStep();
                    Dictionary<CreatureInput, float> inputsToInputValuesDict = new Dictionary<CreatureInput, float>();

                    foreach (var input in inputsRequiredForStep)
                    {
                        inputsToInputValuesDict.Add(input, generatedStep.GenerateCreatureInputValue(input, creature));
                    }

                    Queue<StepAction<TCreature>> creatureStepActions = GenerateCreatureActions(creature, inputsToInputValuesDict, gatherInfoForAllCreatures || creature == BestCreature || creature == SelectedCreature);

                    foreach (var creatureStepAction in creatureStepActions)
                    {
                        generatedStep.QueueAction(creatureStepAction);
                    }
                }

                //Update best creature.
                if (UseSuccessBounds)
                {
                    float distanceFromMiddle = Globals.GetDistanceFrom(creature.MX, creature.MY, successBoundsMiddleX, successBoundsMiddleY);
                    float? newBestCreatureDistanceFromMiddle = newBestCreature == null ? (float?)null : Globals.GetDistanceFrom(newBestCreature.MX, newBestCreature.MY, successBoundsMiddleX, successBoundsMiddleY);

                    if (newBestCreatureDistanceFromMiddle == null || distanceFromMiddle < newBestCreatureDistanceFromMiddle)
                    {
                        newBestCreature = creature;
                    }
                }
                else
                {
                    if (newBestCreature == null || creature.FoodEaten > newBestCreature.FoodEaten)
                    {
                        newBestCreature = creature;
                    }
                }
            }

            if (newBestCreature != null && BestCreature != newBestCreature)
            {
                BestCreature = newBestCreature;
            }

            if (random.NextFloat() <= 0.8)
            {
                if (generatedStep.Food.Count < MaxFoodAmount)
                {
                    generatedStep.Food.Add(CreateFoodWithRandomLocation());
                }
            }

            CurrentStep = generatedStep;
        }
        public virtual TFood CreateFoodWithRandomLocation()
        {
            return new TFood()
            {
                X = random.NextFloat(0, WorldBounds.X + WorldBounds.Width),
                Y = random.NextFloat(0, WorldBounds.Y + WorldBounds.Height),
                Servings = 1,
                EnergyPerServing = 30,
                ServingDigestionCost = 0.05f,
                Size = FoodSize
            };
        }
        public virtual List<TFood> GenerateFood()
        {
            List<TFood> food = new List<TFood>();

            for (int i = 0; i < MaxFoodAmount; i++)
            {
                food.Add(CreateFoodWithRandomLocation());
            }

            return food;
        }
        public virtual List<TCreature> GenerateCreatures()
        {
            List<TCreature> creatures = new List<TCreature>();

            for (int i = 0; i < MaxCreatureAmount; i++)
            {
                TCreature newCreature = new TCreature()
                {
                    X = random.NextFloat(0, WorldBounds.X + WorldBounds.Width),
                    Y = random.NextFloat(0, WorldBounds.Y + WorldBounds.Height),
                    Size = CreatureSize,
                    Speed = CreatureSpeed,
                    Metabolism = 0.1f,
                    Energy = MaxCreatureEnergy,
                    MaxEnergy = MaxCreatureEnergy,
                    SightRange = 100
                };

                newCreature.Brain = new NeuralNetwork(NeuralNetwork.GenerateInputNodes(PossibleCreatureInputs)
                    .Concat(NeuralNetwork.GenerateProcessNodes(MaxCreatureProcessNodes))
                    .Concat(NeuralNetwork.GenerateOutputNodes(PossibleCreatureActions)));

                newCreature.Brain.Connections = newCreature.Brain.GenerateRandomConnections(MinCreatureConnections, MinCreatureConnections, ConnectionWeightBound);

                creatures.Add(newCreature);
            }

            return creatures;
        }
        public static Queue<StepAction<TCreature>> GenerateCreatureActions(TCreature creature, Dictionary<CreatureInput, float> inputsToInputValuesDict, bool trackStepInfo)
        {
            Dictionary<int, float> nodeIdToOutputDict = creature.Brain.Step(inputsToInputValuesDict, trackStepInfo);

            if (nodeIdToOutputDict.Count == 0)
            {
                return new Queue<StepAction<TCreature>>();
            }

            Node highestOutputNode = null;
            float? highestOutputNodeOutputValue = null;

            foreach (var keyValuePair in nodeIdToOutputDict)
            {
                Node node = creature.Brain.NodeIdsToNodesDict[keyValuePair.Key];
                float outputValue = keyValuePair.Value;

                if (node.NodeType == NodeType.Output && (highestOutputNodeOutputValue == null || outputValue > highestOutputNodeOutputValue))
                {
                    highestOutputNode = node;
                    highestOutputNodeOutputValue = outputValue;
                }
            }

            if (highestOutputNode == null || highestOutputNodeOutputValue == null)
            {
                return new Queue<StepAction<TCreature>>();
            }

            Queue<StepAction<TCreature>> actions = new Queue<StepAction<TCreature>>();

            if (highestOutputNodeOutputValue.Value > 0)
            {
                actions.Enqueue(new StepAction<TCreature>() { Creature = creature, Action = highestOutputNode.CreatureAction.Value });
            }

            return actions;
        }
        protected virtual void OnBestCreatureChanged(object sender, ValueChangedEventArgs<TCreature> e)
        {
            BestCreatureChanged?.Invoke(this, e);
        }
        protected virtual void OnSelectedCreatureChanged(object sender, ValueChangedEventArgs<TCreature> e)
        {
            SelectedCreatureChanged?.Invoke(this, e);
        }
        //public void RunSimulation(long ticksToRunFor, int ticksPerGeneration = 15000)
        //{
        //    if (ticksPerGeneration < 1)
        //    {
        //        throw new ArgumentOutOfRangeException($"{nameof(ticksPerGeneration)} must be greater than 0");
        //    }

        //    long ticks = 0;
        //    int ticksInCurrentGeneration = 0;

        //    while (ticks < ticksToRunFor)
        //    {
        //        Update();
        //        ticks += 1;
        //        ticksInCurrentGeneration += 1;

        //        if (ticksInCurrentGeneration >= ticksPerGeneration)
        //        {
        //            ticksInCurrentGeneration = 0;

        //            List<TCreature> newGenerationCreatures = NewGenerationAsexual();

        //            if (newGenerationCreatures.Count > 0)
        //            {
        //                Reset();
        //                Food.AddRange(GenerateFood());
        //                Creatures = newGenerationCreatures;
        //            }
        //            else
        //            {
        //                Reset();
        //            }
        //        }
        //    }
        //}

        #endregion
    }
}
