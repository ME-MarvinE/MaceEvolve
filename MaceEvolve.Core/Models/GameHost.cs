using MaceEvolve.Core.Enums;
using MaceEvolve.Core.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace MaceEvolve.Core.Models
{
    public class GameHost<TStep, TCreature, TFood> where TStep : class, IStep<TCreature, TFood>, new() where TCreature : class, ICreature, new() where TFood : class, IFood, new()
    {
        #region Fields
        protected TCreature bestCreature;
        protected TCreature selectedCreature;
        #endregion

        #region Properties
        public TStep CurrentStep { get; set; }
        public int MaxCreatureAmount { get; set; } = 1000;
        public int MaxFoodAmount { get; set; } = 350;
        public IRectangle WorldBounds { get; set; } = new Rectangle(0, 0, 512, 512);
        public int MinCreatureConnections { get; set; } = 4;
        public int MaxCreatureConnections { get; set; } = 64;
        public float CreatureSpeed { get; set; }
        public int MaxCreatureProcessNodes { get; set; } = 2;
        public float CreatureOffspringBrainMutationChance { get; set; } = 1 / 3f;
        public int CreatureOffspringBrainMutationAttempts { get; set; } = 1;
        public float ConnectionWeightBound { get; set; } = 4;
        public float MaxCreatureEnergy { get; set; } = 900;
        public float MinFoodSize { get; set; } = 2;
        public float MaxFoodSize { get; set; } = 7;
        public float CreatureSize { get; set; } = 10;
        public float CreatureNutrients { get; set; } = 30;
        public float EnergyRequiredForCreatureToReproduce { get; set; } = 100;
        public float NutrientsRequiredForCreatureToReproduce { get; set; } = 50;
        public float MinimumSuccessfulCreatureFitness { get; set; } = 0.25f;
        public float ReproductionNodeBiasVariance { get; set; }
        public float ReproductionConnectionWeightVariance { get; set; }
        public float CreatureMetabolism { get; set; } = 0.1f;
        public float MinFoodEnergy { get; set; } = 150;
        public float MaxFoodEnergy { get; set; } = 300;
        public float MinFoodNutrients { get; set; } = 10;
        public float MaxFoodNutrients { get; set; } = 50;
        public float CreatureSightRange { get; set; } = 100;
        public int MaxCreatureOffSpringPerReproduction { get; set; } = 1;
        public bool LoopWorldBounds { get; set; } = true;
        public float CreatureEnergyPerEat { get; set; } = 150;
        public float CreatureNutrientsPerEat { get; set; } = 50;
        public float MaxCreatureNutrients { get; set; } = 200;
        public int CreatureMaxAge { get; set; } = 4000;
        public int MinCreatureVisibilityPartitionSize { get; set; } = 100;

        public ReadOnlyCollection<CreatureInput> PossibleCreatureInputs { get; } = Globals.AllCreatureInputs;
        public ReadOnlyCollection<CreatureAction> PossibleCreatureActions { get; } = Globals.AllCreatureActions;
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
        public virtual void ResetStep(IEnumerable<TCreature> creatures, IEnumerable<TFood> food)
        {
            BestCreature = null;
            SelectedCreature = null;
            CurrentStep = new TStep()
            {
                Creatures = new ConcurrentBag<TCreature>(creatures),
                Food = new ConcurrentBag<TFood>(food),
                WorldBounds = new Rectangle(WorldBounds.X, WorldBounds.Y, WorldBounds.Width, WorldBounds.Height),
                ConnectionWeightBound = ConnectionWeightBound,
                MinCreatureConnections = MinCreatureConnections,
                MaxCreatureConnections = MaxCreatureConnections,
                MaxCreatureProcessNodes = MaxCreatureProcessNodes,
                LoopWorldBounds = LoopWorldBounds
            };
        }
        public List<T>[,] CreatePartitionedGrid<T>(IEnumerable<T> gameObjects, int gridRowCount, int gridColumnCount, double cellSize) where T : IGameObject
        {
            List<T>[,] gameObjectsGrid = new List<T>[gridRowCount, gridColumnCount];

            for (int cellRow = 0; cellRow < gridRowCount; cellRow++)
            {
                for (int cellColumn = 0; cellColumn < gridColumnCount; cellColumn++)
                {
                    gameObjectsGrid[cellRow, cellColumn] = new List<T>();
                }
            }

            foreach (var gameObject in gameObjects)
            {
                int? gameObjectCellRow = null;
                int? gameObjectCellColumn = null;

                for (int cellRowIndex = 0; cellRowIndex < gridRowCount && gameObjectCellRow == null; cellRowIndex++)
                {
                    double cellY = (cellRowIndex + 1) * cellSize;

                    if (gameObject.MY < cellY)
                    {
                        gameObjectCellRow = cellRowIndex;
                    }

                    for (int cellColumnIndex = 0; cellColumnIndex < gridColumnCount && gameObjectCellColumn == null; cellColumnIndex++)
                    {
                        double cellX = (cellColumnIndex + 1) * cellSize;

                        if (gameObject.MX < cellX)
                        {
                            gameObjectCellColumn = cellColumnIndex;
                        }
                    }
                }

                gameObjectsGrid[gameObjectCellRow.Value, gameObjectCellColumn.Value].Add(gameObject);
            }

            return gameObjectsGrid;
        }
        public void CalculateCreaturesVisibleGameObjects(int partitionRowCount, int partitionColumnCount, double cellSize)
        {
            List<TCreature>[,] partitionedCreatures = CreatePartitionedGrid(CurrentStep.Creatures, partitionRowCount, partitionColumnCount, cellSize);
            List<TFood>[,] partitionedFood = CreatePartitionedGrid(CurrentStep.Food, partitionRowCount, partitionColumnCount, cellSize);

            int maxParallelismCount = Math.Min(Environment.ProcessorCount, partitionRowCount);

            Parallel.For(0, maxParallelismCount, i =>
            {
                int localPartitionRowCount;
                int localPartitionRowStartIndex;
                int localPartitionRowEnd;

                localPartitionRowCount = Math.Max((int)Math.Ceiling(partitionRowCount / (double)maxParallelismCount), 1);
                localPartitionRowStartIndex = Math.Min(partitionRowCount - 1, i * localPartitionRowCount);

                localPartitionRowEnd = Math.Min(partitionRowCount, localPartitionRowStartIndex + localPartitionRowCount);

                for (int cellRowIndex = localPartitionRowStartIndex; cellRowIndex < localPartitionRowEnd; cellRowIndex++)
                {
                    for (int cellColumnIndex = 0; cellColumnIndex < partitionColumnCount; cellColumnIndex++)
                    {
                        //Check cells around the current cell.
                        for (int rowOffset = -1; rowOffset <= 1; rowOffset++)
                        {
                            int otherCellRowIndex = cellRowIndex + rowOffset;

                            if (otherCellRowIndex < 0)
                            {
                                continue;
                            }
                            else if (otherCellRowIndex >= partitionRowCount)
                            {
                                break;
                            }

                            for (int columnOffset = -1; columnOffset <= 1; columnOffset++)
                            {
                                int otherCellColumnIndex = cellColumnIndex + columnOffset;

                                if (otherCellColumnIndex < 0)
                                {
                                    continue;
                                }
                                else if (otherCellColumnIndex >= partitionColumnCount)
                                {
                                    break;
                                }

                                foreach (var creature in partitionedCreatures[cellRowIndex, cellColumnIndex])
                                {
                                    if (!CurrentStep.VisibleCreaturesDict.ContainsKey(creature))
                                    {
                                        CurrentStep.VisibleCreaturesDict[creature] = new List<TCreature>();
                                    }

                                    IEnumerable<TCreature> visibleCreaturesInOtherCell = partitionedCreatures[otherCellRowIndex, otherCellColumnIndex]
                                        .Where(x => Globals.GetDistanceFrom(creature.MX, creature.MY, x.MX, x.MY) <= creature.SightRange && x != this);

                                    CurrentStep.VisibleCreaturesDict[creature].AddRange(visibleCreaturesInOtherCell);

                                    if (!CurrentStep.VisibleFoodDict.ContainsKey(creature))
                                    {
                                        CurrentStep.VisibleFoodDict[creature] = new List<TFood>();
                                    }

                                    IEnumerable<TFood> visibleFoodInOtherCell = partitionedFood[otherCellRowIndex, otherCellColumnIndex]
                                        .Where(x => Globals.GetDistanceFrom(creature.MX, creature.MY, x.MX, x.MY) <= creature.SightRange);

                                    CurrentStep.VisibleFoodDict[creature].AddRange(visibleFoodInOtherCell);
                                }
                            }
                        }
                    }
                }
            });
        }
        public virtual StepResult<TCreature> NextStep(IEnumerable<StepAction<TCreature>> actionsToExecute, bool gatherBestCreatureInfo, bool gatherSelectedCreatureInfo, bool gatherAliveCreatureInfo, bool gatherDeadCreatureInfo)
        {
            CurrentStep.ExecuteActions(actionsToExecute);
            CurrentStep.Creatures = new ConcurrentBag<TCreature>(CurrentStep.Creatures.Where(x => !x.IsDead));
            CurrentStep.Food = new ConcurrentBag<TFood>(CurrentStep.Food.Where(x => x.Energy > 0));
            CurrentStep.VisibleCreaturesDict.Clear();
            CurrentStep.VisibleFoodDict.Clear();

            double sightRangeSum = 0;
            double? highestSightRange = null;
            double? sightRangeAverage = 100;
            double iterations = 0;

            foreach (var creature in CurrentStep.Creatures)
            {
                sightRangeSum += creature.SightRange;

                if (creature.SightRange > highestSightRange || highestSightRange == null)
                {
                    highestSightRange = creature.SightRange;
                }

                iterations += 1;

                if (iterations >= 100 || sightRangeSum >= 2000000000)
                {
                    break;
                }
            }

            sightRangeAverage ??= sightRangeSum / iterations;

            double partitionSize;

            if (highestSightRange == 0 || highestSightRange == null)
            {
                partitionSize = MinCreatureVisibilityPartitionSize;
            }
            else
            {
                partitionSize = Math.Max(sightRangeAverage.Value, highestSightRange.Value);
            }

            int gridColumnCount = (int)Math.Ceiling(WorldBounds.Width / partitionSize);
            int gridRowCount = (int)Math.Ceiling(WorldBounds.Height / partitionSize);

            CalculateCreaturesVisibleGameObjects(gridRowCount, gridColumnCount, partitionSize);

            StepResult<TCreature> stepResult = new StepResult<TCreature>(new ConcurrentQueue<StepAction<TCreature>>(), new ConcurrentDictionary<TCreature, List<NeuralNetworkStepNodeInfo>>());

            TCreature newBestCreature = BestCreature == null || BestCreature.IsDead ? null : BestCreature;

            Parallel.ForEach(CurrentStep.Creatures, creature =>
            {
                bool shouldTrackBrainOutput = (!creature.IsDead && gatherAliveCreatureInfo) || (creature.IsDead && gatherDeadCreatureInfo) || (creature == newBestCreature && gatherBestCreatureInfo) || (creature == SelectedCreature && gatherSelectedCreatureInfo);
                bool shouldEvaluateCreature = !creature.IsDead || shouldTrackBrainOutput;

                if (shouldEvaluateCreature)
                {
                    //Calculate the output values for the creature's nodes.
                    IEnumerable<CreatureInput> inputsRequiredForStep = creature.Brain.GetInputsRequiredForStep();
                    Dictionary<CreatureInput, float> inputToInputValueDict = CurrentStep.GenerateCreatureInputValues(inputsRequiredForStep, creature);
                    Dictionary<int, float> nodeIdToOutputValueDict = creature.Brain.GenerateNodeOutputs(inputToInputValueDict);

                    //Get the output node with the highest output value.
                    int? highestOutputNodeId = null;

                    foreach (var keyValuePair in nodeIdToOutputValueDict)
                    {
                        Node node = creature.Brain.NodeIdsToNodesDict[keyValuePair.Key];
                        float outputValue = keyValuePair.Value;

                        if (node.NodeType == NodeType.Output && outputValue > 0 && (highestOutputNodeId == null || outputValue > nodeIdToOutputValueDict[highestOutputNodeId.Value]))
                        {
                            highestOutputNodeId = keyValuePair.Key;
                        }
                    }

                    StepAction<TCreature> stepAction = new StepAction<TCreature>()
                    {
                        Creature = creature
                    };

                    if (highestOutputNodeId != null && nodeIdToOutputValueDict[highestOutputNodeId.Value] > 0)
                    {
                        stepAction.Action = creature.Brain.NodeIdsToNodesDict[highestOutputNodeId.Value].CreatureAction.Value;
                    }
                    else
                    {
                        stepAction.Action = CreatureAction.DoNothing;
                    }

                    stepResult.CalculatedActions.Enqueue(stepAction);

                    //Store properties of the creature's current status.
                    if (shouldTrackBrainOutput)
                    {
                        List<NeuralNetworkStepNodeInfo> currentStepNodeInfo = new List<NeuralNetworkStepNodeInfo>();

                        foreach (var keyValuePair in nodeIdToOutputValueDict)
                        {
                            int nodeId = keyValuePair.Key;
                            Node node = creature.Brain.NodeIdsToNodesDict[nodeId];
                            float output = keyValuePair.Value;

                            NeuralNetworkStepNodeInfo currentStepCurrentNodeInfo = new NeuralNetworkStepNodeInfo()
                            {
                                NodeId = nodeId,
                                Bias = node.Bias,
                                CreatureAction = node.CreatureAction,
                                CreatureInput = node.CreatureInput,
                                NodeType = node.NodeType,
                                PreviousOutput = output,
                            };

                            foreach (var connection in creature.Brain.Connections)
                            {
                                bool sourceIdIsNodeId = connection.SourceId == nodeId;
                                bool targetIdIsNodeId = connection.TargetId == nodeId;

                                if (sourceIdIsNodeId || targetIdIsNodeId)
                                {
                                    currentStepCurrentNodeInfo.Connections.Add(connection);
                                }

                                if (sourceIdIsNodeId)
                                {
                                    currentStepCurrentNodeInfo.ConnectionsFrom.Add(connection);
                                }

                                if (targetIdIsNodeId)
                                {
                                    currentStepCurrentNodeInfo.ConnectionsTo.Add(connection);
                                }
                            }

                            currentStepNodeInfo.Add(currentStepCurrentNodeInfo);
                        }

                        stepResult.CreaturesBrainOutputs[creature] = currentStepNodeInfo;
                    }

                    if (!creature.IsDead)
                    {
                        creature.Age += 1;
                        if (creature.Age > CreatureMaxAge)
                        {
                            creature.Die();
                        }
                    }

                    //Identify the best creature in the step.
                    if (newBestCreature == null || (creature.FoodEaten > newBestCreature.FoodEaten && creature.TimesReproduced > newBestCreature.TimesReproduced))
                    {
                        newBestCreature = creature;
                    }
                }
            });

            BestCreature = newBestCreature;

            if (MaceRandom.Current.NextFloat() <= 0.8 && CurrentStep.Food.Count < MaxFoodAmount)
            {
                CurrentStep.Food.Add(CreateFoodWithRandomLocation());
            }

            return stepResult;
        }
        public virtual TFood CreateFoodWithRandomLocation()
        {
            TFood newFood = new TFood()
            {
                X = MaceRandom.Current.NextFloat(0, WorldBounds.X + WorldBounds.Width),
                Y = MaceRandom.Current.NextFloat(0, WorldBounds.Y + WorldBounds.Height),
                MaxEnergy = MaxFoodEnergy,
                MaxNutrients = MaxFoodNutrients
            };

            newFood.Energy = MaceRandom.Current.NextFloat(MinFoodEnergy, MaxFoodEnergy);
            newFood.Nutrients = MaceRandom.Current.NextFloat(MinFoodNutrients, MaxFoodNutrients);
            newFood.Size = Globals.Map(newFood.Energy, 0, newFood.MaxEnergy, MinFoodSize, MaxFoodSize);

            return newFood;
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
                    X = MaceRandom.Current.NextFloat(0, WorldBounds.X + WorldBounds.Width),
                    Y = MaceRandom.Current.NextFloat(0, WorldBounds.Y + WorldBounds.Height),
                    Size = CreatureSize,
                    Speed = CreatureSpeed,
                    Metabolism = CreatureMetabolism,
                    MaxAge = CreatureMaxAge,
                    MaxEnergy = MaxCreatureEnergy,
                    Energy = MaxCreatureEnergy * 0.75f,
                    SightRange = CreatureSightRange,
                    MaxNutrients = MaxCreatureNutrients,
                    Nutrients = CreatureNutrients,
                    NutrientsRequiredToReproduce = NutrientsRequiredForCreatureToReproduce,
                    EnergyRequiredToReproduce = EnergyRequiredForCreatureToReproduce,
                    MaxOffspringPerReproduction = MaxCreatureOffSpringPerReproduction,
                    OffspringBrainMutationAttempts = CreatureOffspringBrainMutationAttempts,
                    OffspringBrainMutationChance = CreatureOffspringBrainMutationChance,
                    EnergyPerEat = CreatureEnergyPerEat,
                    NutrientsPerEat = CreatureNutrientsPerEat,
                    MoveCost = 0.25f
                };

                newCreature.Brain = new NeuralNetwork(NeuralNetwork.GenerateInputNodes(PossibleCreatureInputs)
                    .Concat(NeuralNetwork.GenerateProcessNodes(MaxCreatureProcessNodes, 0.75f))
                    .Concat(NeuralNetwork.GenerateOutputNodes(PossibleCreatureActions)));

                newCreature.Brain.Connections.Clear();
                newCreature.Brain.Connections.AddRange(newCreature.Brain.GenerateRandomConnections(MinCreatureConnections, MaxCreatureConnections, ConnectionWeightBound));

                creatures.Add(newCreature);
            }

            return creatures;
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
