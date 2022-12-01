using MaceEvolve.Core.Enums;
using MaceEvolve.Core.Extensions;
using MaceEvolve.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Timers;

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
        public List<TCreature> Creatures { get; set; } = new List<TCreature>();
        public List<IFood> Food { get; set; } = new List<IFood>();
        public Stopwatch Stopwatch { get; set; } = new Stopwatch();
        public int MaxCreatureAmount { get; set; } = 150;
        public int MaxFoodAmount { get; set; } = 350;
        public Rectangle WorldBounds { get; set; }
        public Rectangle SuccessBounds { get; set; }
        public int MinCreatureConnections { get; set; } = 4;
        public int MaxCreatureConnections { get; set; } = 128;
        public double CreatureSpeed { get; set; }
        public int TicksPerGeneration { get; set; }
        public int TicksUntilNextGeneration { get; set; }
        public int MaxCreatureProcessNodes { get; set; } = 4;
        public double MutationChance { get; set; } = 0.25;
        public double MutationAttempts { get; set; } = 2;
        public double ConnectionWeightBound { get; set; } = 4;
        public double MaxCreatureEnergy { get; set; } = 150;
        public double SuccessfulCreaturesPercentile { get; set; } = 10;
        public int GenerationCount { get; set; } = 1;
        public double ReproductionNodeBiasVariance = 0.05;
        public double ReproductionConnectionWeightVariance = 0.05;
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

        #region Constructors
        public GameHost()
        {
            CreatureSpeed = UseSuccessBounds ? 3.5 : 2.75;

            Reset();
        }
        #endregion

        #region Methods
        public void Reset()
        {
            Stopwatch.Reset();
            TicksUntilNextGeneration = TicksPerGeneration;
            Creatures.Clear();
            ResetFood();
            GenerationCount = 1;
            BestCreature = null;
            SelectedCreature = null;

            Creatures.AddRange(GenerateCreatures());
        }
        public List<TCreature> NewGenerationSexual()
        {
            List<TCreature> creaturesList = new List<TCreature>(Creatures);
            IEnumerable<TCreature> successfulCreatures = GetSuccessfulCreatures(creaturesList);
            Dictionary<TCreature, double> successfulCreaturesFitnesses = GetFitnesses(successfulCreatures);

            if (!successfulCreaturesFitnesses.Any())
            {
                return new List<TCreature>();
            }

            List<TCreature> newCreatures = new List<TCreature>();

            TCreature newCreature = Creature.Reproduce(successfulCreatures.ToList(), PossibleCreatureInputs.ToList(), PossibleCreatureActions.ToList(), ReproductionNodeBiasVariance, ReproductionConnectionWeightVariance, ConnectionWeightBound);
            newCreature.X = random.NextDouble(0, WorldBounds.X + WorldBounds.Width);
            newCreature.Y = random.NextDouble(0, WorldBounds.Y + WorldBounds.Height);
            newCreature.Size = 10;
            newCreature.Speed = CreatureSpeed;
            newCreature.Metabolism = 0.1;
            newCreature.Energy = MaxCreatureEnergy;
            newCreature.MaxEnergy = MaxCreatureEnergy;
            newCreature.SightRange = 100;

            for (int i = 0; i < MutationAttempts; i++)
            {
                bool mutated = MutateNetwork(newCreature.Brain,
                    createRandomNodeChance: MutationChance * 0,
                    removeRandomNodeChance: MutationChance * 0,
                    mutateRandomNodeBiasChance: MutationChance * 2,
                    createRandomConnectionChance: MutationChance,
                    removeRandomConnectionChance: MutationChance,
                    mutateRandomConnectionSourceChance: MutationChance / 2,
                    mutateRandomConnectionTargetChance: MutationChance / 2,
                    mutateRandomConnectionWeightChance: MutationChance * 2);
            }

            newCreatures.Add(newCreature);

            return newCreatures;
        }
        public List<TCreature> NewGenerationAsexual()
        {
            List<TCreature> creaturesList = new List<TCreature>(Creatures);
            IEnumerable<TCreature> successfulCreatures = GetSuccessfulCreatures(creaturesList);
            Dictionary<TCreature, double> successfulCreaturesFitnesses = GetFitnesses(successfulCreatures);

            if (!successfulCreaturesFitnesses.Any())
            {
                return new List<TCreature>();
            }

            List<TCreature> newCreatures = new List<TCreature>();

            while (newCreatures.Count < MaxCreatureAmount)
            {
                foreach (var creatureFitnessPair in successfulCreaturesFitnesses.OrderByDescending(x => x.Value))
                {
                    TCreature successfulCreature = creatureFitnessPair.Key;

                    if (random.NextDouble() <= creatureFitnessPair.Value && newCreatures.Count < MaxCreatureAmount)
                    {
                        int numberOfChildrenToCreate = UseSuccessBounds ? (int)Globals.Map(creatureFitnessPair.Value, 0, 1, 0, MaxCreatureAmount / 10) : successfulCreature.FoodEaten;

                        for (int i = 0; i < numberOfChildrenToCreate; i++)
                        {
                            if (newCreatures.Count < MaxCreatureAmount)
                            {
                                TCreature newCreature = Creature.Reproduce(new List<TCreature>() { successfulCreature }, PossibleCreatureInputs.ToList(), PossibleCreatureActions.ToList(), ReproductionNodeBiasVariance, ReproductionConnectionWeightVariance, ConnectionWeightBound);
                                newCreature.X = random.NextDouble(0, WorldBounds.X + WorldBounds.Width);
                                newCreature.Y = random.NextDouble(0, WorldBounds.Y + WorldBounds.Height);
                                newCreature.Size = 10;
                                newCreature.Speed = CreatureSpeed;
                                newCreature.Metabolism = 0.1;
                                newCreature.Energy = MaxCreatureEnergy;
                                newCreature.MaxEnergy = MaxCreatureEnergy;
                                newCreature.SightRange = 100;

                                for (int j = 0; j < MutationAttempts; j++)
                                {
                                    bool Mutated = MutateNetwork(newCreature.Brain,
                                        createRandomNodeChance: MutationChance,
                                        removeRandomNodeChance: MutationChance,
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
            }

            return newCreatures;
        }
        public IEnumerable<TCreature> GetSuccessfulCreatures(IEnumerable<TCreature> creatures)
        {
            if (creatures == null) { throw new ArgumentNullException(); }

            if (!creatures.Any())
            {
                return new List<TCreature>();
            }

            if (UseSuccessBounds)
            {
                double successBoundsRight = SuccessBounds.X + SuccessBounds.Width;
                double successBoundsBottom = SuccessBounds.Y + SuccessBounds.Height;
                return creatures.Where(x => x.X > SuccessBounds.X && x.X < successBoundsRight && x.Y > SuccessBounds.Y && x.Y < successBoundsBottom).ToList();
            }
            else
            {
                //return creatures.Where(x => x.FoodEaten > 0).ToList();

                double indexMultiplierForTopPercentile = (1 - (double)SuccessfulCreaturesPercentile / 100);
                int topPercentileStartingIndex = (int)(creatures.Count() * indexMultiplierForTopPercentile) - 1;

                List<TCreature> orderedCreatures = creatures.OrderBy(x => x.FoodEaten).ToList();
                return orderedCreatures.SkipWhile(x => orderedCreatures.IndexOf(x) < topPercentileStartingIndex).Where(x => x.FoodEaten > 0);
            }
        }
        public Dictionary<TCreature, double> GetFitnesses(IEnumerable<TCreature> creatures)
        {
            if (creatures == null) { throw new ArgumentNullException(); }

            if (!creatures.Any())
            {
                return new Dictionary<TCreature, double>();
            }

            Dictionary<TCreature, double> successfulCreaturesFitnesses = new Dictionary<TCreature, double>();

            if (UseSuccessBounds)
            {
                double successBoundsMiddleX = Globals.MiddleX(SuccessBounds.X, SuccessBounds.Width);
                double successBoundsMiddleY = Globals.MiddleY(SuccessBounds.Y, SuccessBounds.Height);

                foreach (var creature in creatures)
                {
                    double distanceFromMiddle = Globals.GetDistanceFrom(creature.MX, creature.MY, successBoundsMiddleX, successBoundsMiddleY);
                    double successBoundsHypotenuse = Globals.Hypotenuse(SuccessBounds.Width, SuccessBounds.Height);

                    successfulCreaturesFitnesses.Add(creature, Globals.Map(distanceFromMiddle, 0, successBoundsHypotenuse, 1, 0)); ;
                }
            }
            else
            {
                int mostFoodEaten = creatures.Max(x => x.FoodEaten);

                if (mostFoodEaten == 0)
                {
                    return new Dictionary<TCreature, double>();
                }

                successfulCreaturesFitnesses = creatures.ToDictionary(
                    x => x,
                    x => (double)x.FoodEaten / mostFoodEaten);
            }

            return successfulCreaturesFitnesses;
        }
        public bool MutateNetwork(NeuralNetwork network, double createRandomNodeChance, double removeRandomNodeChance, double mutateRandomNodeBiasChance, double createRandomConnectionChance, double removeRandomConnectionChance, double mutateRandomConnectionSourceChance, double mutateRandomConnectionTargetChance, double mutateRandomConnectionWeightChance)
        {
            int processNodeCount = network.NodeIdsToNodesDict.Values.Where(x => x.NodeType == NodeType.Process).Count();
            bool mutationAttempted = false;

            //Things should be removed before being added so that there isn't a chance that the newly added thing is deleted straight after.
            //Connections should be added after nodes are added so that there is a chance the newly created node gets a connection.

            //Remove an existing node. Input nodes should not be removed. 
            if (random.NextDouble() <= removeRandomNodeChance)
            {
                mutationAttempted = true;

                IEnumerable<Node> processNodes = network.NodeIdsToNodesDict.Values.Where(x => x.NodeType == NodeType.Process);
                IEnumerable<Node> outputNodes = network.NodeIdsToNodesDict.Values.Where(x => x.NodeType == NodeType.Output);

                List<Node> possibleNodesToRemove = new List<Node>(processNodes);

                //There must be at least one target node in a network.
                if (outputNodes.ElementAtOrDefault(1) != null)
                {
                    possibleNodesToRemove.AddRange(outputNodes);
                }

                if (possibleNodesToRemove.Count > 0)
                {
                    Node nodeToRemove = possibleNodesToRemove[random.Next(possibleNodesToRemove.Count)];
                    int nodeIdToRemove = network.NodesToNodeIdsDict[nodeToRemove];

                    network.RemoveNode(nodeIdToRemove, true);
                }
            }

            //Change a random node's bias.
            if (random.NextDouble() <= mutateRandomNodeBiasChance)
            {
                mutationAttempted = true;

                List<Node> nodesList = network.NodeIdsToNodesDict.Values.ToList();
                Node randomNode = nodesList[random.Next(nodesList.Count)];
                randomNode.Bias = random.NextDouble(-1, 1);
            }

            //Create a new node with a default connection.
            if (random.NextDouble() <= createRandomNodeChance)
            {
                List<CreatureInput> possibleCreatureInputsToAdd = GetPossibleInputsToAdd(network).ToList();
                List<CreatureAction> possibleCreatureActionsToAdd = GetPossibleActionsToAdd(network).ToList();

                mutationAttempted = true;

                Node nodeToAdd;
                double nodeTypeRandomNum = random.NextDouble();
                double chanceForSingleNodeType = 1d / Globals.AllNodeTypes.Count;

                if (nodeTypeRandomNum <= chanceForSingleNodeType && possibleCreatureInputsToAdd.Count > 0)
                {
                    nodeToAdd = new Node(NodeType.Input, random.NextDouble(-1, 1), possibleCreatureInputsToAdd[random.Next(possibleCreatureInputsToAdd.Count)]);
                }
                else if (nodeTypeRandomNum <= chanceForSingleNodeType * 2 && possibleCreatureActionsToAdd.Count > 0)
                {
                    nodeToAdd = new Node(NodeType.Output, random.NextDouble(-1, 1), creatureAction: possibleCreatureActionsToAdd[random.Next(possibleCreatureActionsToAdd.Count)]);
                }
                else if (processNodeCount < MaxCreatureProcessNodes)
                {
                    nodeToAdd = new Node(NodeType.Process, random.NextDouble(-1, 1));
                }
                else
                {
                    nodeToAdd = null;
                }

                if (nodeToAdd != null)
                {
                    List<Node> possibleSourceNodes = NeuralNetwork.GetSourceNodes(network.NodeIdsToNodesDict.Values).ToList();
                    List<Node> possibleTargetNodes = NeuralNetwork.GetTargetNodes(network.NodeIdsToNodesDict.Values).ToList();

                    network.AddNode(nodeToAdd);
                    int nodeToAddId = network.NodesToNodeIdsDict[nodeToAdd];

                    if (network.Connections.Count < MaxCreatureConnections && possibleSourceNodes.Count > 0 && possibleTargetNodes.Count > 0)
                    {
                        Connection newConnection = new Connection() { Weight = random.NextDouble(-ConnectionWeightBound, ConnectionWeightBound) };

                        switch (nodeToAdd.NodeType)
                        {
                            case NodeType.Input:
                                newConnection.SourceId = nodeToAddId;
                                newConnection.TargetId = network.NodesToNodeIdsDict[possibleTargetNodes[random.Next(possibleTargetNodes.Count)]];
                                break;

                            case NodeType.Process:
                                if (random.NextDouble() <= 0.5)
                                {
                                    newConnection.SourceId = nodeToAddId;
                                    newConnection.TargetId = network.NodesToNodeIdsDict[possibleTargetNodes[random.Next(possibleTargetNodes.Count)]];
                                }
                                else
                                {
                                    newConnection.SourceId = network.NodesToNodeIdsDict[possibleSourceNodes[random.Next(possibleSourceNodes.Count)]];
                                    newConnection.TargetId = nodeToAddId;
                                }
                                break;

                            case NodeType.Output:
                                newConnection.SourceId = network.NodesToNodeIdsDict[possibleSourceNodes[random.Next(possibleSourceNodes.Count)]];
                                newConnection.TargetId = nodeToAddId;
                                break;

                            default:
                                throw new NotImplementedException();
                        }

                        network.Connections.Add(newConnection);
                    }
                }
            }

            //Remove a random connection.
            if (network.Connections.Count > MinCreatureConnections && random.NextDouble() <= removeRandomConnectionChance)
            {
                mutationAttempted = true;

                Connection randomConnection = network.Connections[random.Next(network.Connections.Count)];
                network.Connections.Remove(randomConnection);
            }

            //Change a random connection's weight.
            if (network.Connections.Count > 0 && random.NextDouble() <= mutateRandomConnectionWeightChance)
            {
                mutationAttempted = true;

                Connection randomConnection = network.Connections[random.Next(network.Connections.Count)];

                randomConnection.Weight = random.NextDouble(-ConnectionWeightBound, ConnectionWeightBound);
            }

            //Change a random connection's source.
            if (network.Connections.Count > 0 && network.MutateConnectionSource(mutateRandomConnectionSourceChance, network.Connections[random.Next(network.Connections.Count)]))
            {
                mutationAttempted = true;
            }

            //Change a random connection's target.
            if (network.Connections.Count > 0 && network.MutateConnectionTarget(mutateRandomConnectionTargetChance, network.Connections[random.Next(network.Connections.Count)]))
            {
                mutationAttempted = true;
            }

            //Create a new connection.
            if (network.Connections.Count < MaxCreatureConnections && random.NextDouble() <= createRandomConnectionChance)
            {
                mutationAttempted = true;
                Connection newConnection = network.GenerateRandomConnections(1, 1, ConnectionWeightBound).FirstOrDefault();

                if (newConnection != null)
                {
                    network.Connections.Add(newConnection);
                }
            }

            return mutationAttempted;
        }
        public IEnumerable<CreatureInput> GetPossibleInputsToAdd(NeuralNetwork network)
        {
            //Return any inputs that aren't already used by a node in the network.
            return PossibleCreatureInputs.Where(x => !network.NodeIdsToNodesDict.Any(y => y.Value.NodeType == NodeType.Input && x == y.Value.CreatureInput));
        }
        public IEnumerable<CreatureAction> GetPossibleActionsToAdd(NeuralNetwork network)
        {
            //Return any actions that aren't already used by a node in the network.
            return PossibleCreatureActions.Where(x => !network.NodeIdsToNodesDict.Any(y => y.Value.NodeType == NodeType.Output && x == y.Value.CreatureAction));
        }
        public void Update()
        {
            TCreature newBestCreature = null;

            double successBoundsMiddleX = Globals.MiddleX(SuccessBounds.X, SuccessBounds.Width);
            double successBoundsMiddleY = Globals.MiddleY(SuccessBounds.Y, SuccessBounds.Height);

            Food.RemoveAll(x => x.Servings <= 0);

            foreach (TCreature creature in Creatures)
            {
                if (!creature.IsDead)
                {
                    EnvironmentInfo environmentInfo = new EnvironmentInfo(Creatures.AsReadOnly(), Food.AsReadOnly(), WorldBounds);

                    creature.Live(environmentInfo);

                    if (creature.Energy <= 0)
                    {
                        creature.Die();
                    }

                    Food.RemoveAll(x => x.Servings <= 0);
                }

                if (UseSuccessBounds)
                {
                    double distanceFromMiddle = Globals.GetDistanceFrom(creature.MX, creature.MY, successBoundsMiddleX, successBoundsMiddleY);
                    double? newBestCreatureDistanceFromMiddle = newBestCreature == null ? (double?)null : Globals.GetDistanceFrom(newBestCreature.MX, newBestCreature.MY, successBoundsMiddleX, successBoundsMiddleY);

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

            if (random.NextDouble() <= 0.8)
            {
                if (Food.Count < MaxFoodAmount)
                {
                    Food.Add(new TFood()
                    {
                        X = random.NextDouble(0, WorldBounds.X + WorldBounds.Width),
                        Y = random.NextDouble(0, WorldBounds.Y + WorldBounds.Height),
                        Servings = 1,
                        EnergyPerServing = 30,
                        ServingDigestionCost = 0.05,
                        Size = 7
                    });
                }
            }

            if (newBestCreature != null && BestCreature != newBestCreature)
            {
                BestCreature = newBestCreature;
            }

            Tick();
        }
        private void Tick()
        {
            if (TicksUntilNextGeneration <= 0)
            {
                TicksUntilNextGeneration = TicksPerGeneration;
                Creatures = NewGenerationAsexual();

                if (Creatures.Count > 0)
                {
                    ResetFood();
                    SelectedCreature = null;
                    GenerationCount += 1;
                }
                else
                {
                    Reset();
                }
            }
            else
            {
                TicksUntilNextGeneration -= 1;
            }
        }
        public void ResetFood()
        {
            Food.Clear();

            for (int i = 0; i < MaxFoodAmount; i++)
            {
                Food.Add(new TFood()
                {
                    X = random.NextDouble(0, WorldBounds.X + WorldBounds.Width),
                    Y = random.NextDouble(0, WorldBounds.Y + WorldBounds.Height),
                    Servings = 1,
                    EnergyPerServing = 30,
                    ServingDigestionCost = 0.05,
                    Size = 7
                });
            }
        }
        public List<TCreature> GenerateCreatures()
        {
            List<TCreature> creatures = new List<TCreature>();

            for (int i = 0; i < MaxCreatureAmount; i++)
            {
                TCreature newCreature = new TCreature()
                {
                    Brain = new NeuralNetwork(PossibleCreatureInputs.ToList(), MaxCreatureProcessNodes, PossibleCreatureActions.ToList()),
                    X = random.NextDouble(0, WorldBounds.X + WorldBounds.Width),
                    Y = random.NextDouble(0, WorldBounds.Y + WorldBounds.Height),
                    Size = 10,
                    Speed = CreatureSpeed,
                    Metabolism = 0.1,
                    Energy = MaxCreatureEnergy,
                    MaxEnergy = MaxCreatureEnergy,
                    SightRange = 100
                };

                newCreature.Brain.Connections = newCreature.Brain.GenerateRandomConnections(MinCreatureConnections, MaxCreatureConnections, ConnectionWeightBound);
                creatures.Add(newCreature);
            }

            return creatures;
        }
        protected void OnBestCreatureChanged(object sender, ValueChangedEventArgs<TCreature> e)
        {
            BestCreatureChanged?.Invoke(this, e);
        }
        protected void OnSelectedCreatureChanged(object sender, ValueChangedEventArgs<TCreature> e)
        {
            SelectedCreatureChanged?.Invoke(this, e);
        }
        #endregion
    }
}
