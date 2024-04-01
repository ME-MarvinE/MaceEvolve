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
        public MinMaxVal<int> CreatureConnectionsMinMax { get; set; } = MinMaxVal.Create(0, 64);
        public int MaxCreatureProcessNodes { get; set; } = 5;
        public float CreatureOffspringBrainMutationChance { get; set; } = 1 / 3f;
        public int CreatureOffspringBrainMutationAttempts { get; set; } = 1;
        public float ConnectionWeightBound { get; set; } = 4;
        public MinMaxVal<float> FoodSizeMinMax { get; set; } = MinMaxVal.Create(5f, 12);
        public MinMaxVal<float> CreatureSizeMinMax { get; set; } = MinMaxVal.Create(8f, 12);
        public MinMaxVal<float> GeneratedFoodMassMinMax { get; set; } = MinMaxVal.Create(4f, 12);
        public MinMaxVal<float> GeneratedCreatureMassMinMax { get; set; } = MinMaxVal.Create(700f, 900);
        public float MaxCreatureEnergy { get; set; } = 900;
        public float CreatureNutrients { get; set; } = 30;
        public float EnergyRequiredForCreatureToReproduce { get; set; } = 100;
        public float NutrientsRequiredForCreatureToReproduce { get; set; } = 50;
        public float MinimumSuccessfulCreatureFitness { get; set; } = 0.25f;
        public float ReproductionNodeBiasVariance { get; set; }
        public float ReproductionConnectionWeightVariance { get; set; }
        public MinMaxVal<float> FoodEnergyMinMax { get; set; } = MinMaxVal.Create(150f, 300);
        public MinMaxVal<float> FoodNutrientsMinMax { get; set; } = MinMaxVal.Create(10f, 50);
        public float CreatureSightRange { get; set; } = 100;
        public int MaxCreatureOffSpringPerReproduction { get; set; } = 1;
        public bool LoopWorldBounds { get; set; } = true;
        public float CreatureEnergyPerEat { get; set; } = 150;
        public float CreatureNutrientsPerEat { get; set; } = 50;
        public float MaxCreatureNutrients { get; set; } = 200;
        public int CreatureMaxAge { get; set; } = 20000;
        public int MinCreatureVisibilityPartitionSize { get; set; } = 100;
        public float CreatureMaxHealthPoints { get; set; } = 100;
        public int CreatureNaturalHealInterval { get; set; } = 100;
        public float CreatureMassRequiredToReproduce { get; set; } = 50;

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
                CreatureConnectionsMinMax = CreatureConnectionsMinMax,
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

                    if (gameObject.MY <= cellY)
                    {
                        gameObjectCellRow = cellRowIndex;
                    }

                    for (int cellColumnIndex = 0; cellColumnIndex < gridColumnCount && gameObjectCellColumn == null; cellColumnIndex++)
                    {
                        double cellX = (cellColumnIndex + 1) * cellSize;

                        if (gameObject.MX <= cellX)
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
                                        .Where(x =>
                                        {
                                            if (Globals.GetDistanceFrom(creature.MX, creature.MY, x.MX, x.MY) <= creature.SightRange && x != creature)
                                            {
                                                float angleFromSourceToTarget = Globals.GetAngleBetweenF(creature.MX, creature.MY, x.MX, x.MY);

                                                if (Math.Abs(Globals.AngleDifference(creature.ForwardAngle, -angleFromSourceToTarget)) <= (creature.FieldOfView / 2))
                                                {
                                                    return true;
                                                }
                                            }

                                            return false;
                                        });

                                    CurrentStep.VisibleCreaturesDict[creature].AddRange(visibleCreaturesInOtherCell);

                                    if (!CurrentStep.VisibleFoodDict.ContainsKey(creature))
                                    {
                                        CurrentStep.VisibleFoodDict[creature] = new List<TFood>();
                                    }

                                    IEnumerable<TFood> visibleFoodInOtherCell = partitionedFood[otherCellRowIndex, otherCellColumnIndex]
                                        .Where(x =>
                                        {
                                            if (Globals.GetDistanceFrom(creature.MX, creature.MY, x.MX, x.MY) <= creature.SightRange && x != creature)
                                            {
                                                float angleFromSourceToTarget = Globals.GetAngleBetweenF(creature.MX, creature.MY, x.MX, x.MY);

                                                if (Math.Abs(Globals.AngleDifference(creature.ForwardAngle, -angleFromSourceToTarget)) <= (creature.FieldOfView / 2))
                                                {
                                                    return true;
                                                }
                                            }

                                            return false;
                                        });

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

            Parallel.ForEach(CurrentStep.Creatures, creature =>
            {
                creature.Age += 1;

                if (creature.IsDead)
                {
                    creature.Mass *= 0.99f;
                    creature.Mass -= 0.01f;
                    creature.Nutrients *= 0.98f;
                    creature.Nutrients -= 0.01f;
                    creature.Energy *= 0.99f;
                    creature.Energy -= 0.01f;
                }
                else
                {
                    if (creature.StepsSinceLastNaturalHeal >= creature.NaturalHealInterval)
                    {
                        creature.StepsSinceLastNaturalHeal = 0;
                        creature.HealthPoints += creature.NaturalHealHealthPoints;
                    }
                    else
                    {
                        creature.StepsSinceLastNaturalHeal += 1;
                    }

                    if (Globals.ShouldCreatureBeDead(creature))
                    {
                        creature.Die();
                    }
                }
            });

            CurrentStep.Food = new ConcurrentBag<TFood>(CurrentStep.Food.Where(x => Globals.ShouldGameObjectExist(x)));
            CurrentStep.Creatures = new ConcurrentBag<TCreature>(CurrentStep.Creatures.Where(x => Globals.ShouldGameObjectExist(x)));
            CurrentStep.VisibleCreaturesDict.Clear();
            CurrentStep.VisibleFoodDict.Clear();
            CurrentStep.CreatureToCachedAreaDict.Clear();
            CurrentStep.FoodToCachedAreaDict.Clear();

            double sightRangeSum = 0;
            double? highestSightRange = null;
            double? sightRangeAverage = 100;
            double iterations = 0;

            foreach (var creature in CurrentStep.Creatures)
            {
                if (!creature.IsDead)
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


                    //Get the output nodes with the highest output values.
                    if (!creature.IsDead)
                    {
                        Dictionary<int, float> orderedNodeIdToOutputValueDict = nodeIdToOutputValueDict.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
                        Dictionary<CreatureAction, float> creatureActionToOutputValueDict = new Dictionary<CreatureAction, float>();
                        Dictionary<CreatureAction, StepAction<TCreature>> actionsToAdd = new Dictionary<CreatureAction, StepAction<TCreature>>();

                        foreach (var keyValuePair in orderedNodeIdToOutputValueDict)
                        {
                            if (actionsToAdd.Count >= 1)
                            {
                                break;
                            }

                            int nodeId = keyValuePair.Key;
                            float outputValue = keyValuePair.Value;
                            Node node = creature.Brain.NodeIdsToNodesDict[nodeId];

                            if (node.NodeType == NodeType.Output && outputValue > 0 && !actionsToAdd.ContainsKey(node.CreatureAction.Value))
                            {
                                creatureActionToOutputValueDict.Add(node.CreatureAction.Value, outputValue);
                                StepAction<TCreature> stepAction = new StepAction<TCreature>
                                {
                                    Creature = creature,
                                    Action = node.CreatureAction.Value,
                                    CreatureActionToOutputValueDict = creatureActionToOutputValueDict
                                };

                                actionsToAdd.Add(stepAction.Action, stepAction);
                            }
                        }

                        foreach (var keyValuePair in actionsToAdd)
                        {
                            stepResult.CalculatedActions.Enqueue(keyValuePair.Value);
                        }
                    }

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

                    //Identify the best creature in the step.
                    if (newBestCreature == null || (creature.TimesReproduced > newBestCreature.TimesReproduced))
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
                MaxEnergy = FoodEnergyMinMax.Max,
                MaxNutrients = FoodNutrientsMinMax.Max,
            };

            newFood.Mass = MaceRandom.Current.NextFloat(GeneratedFoodMassMinMax.Min, GeneratedFoodMassMinMax.Max);
            newFood.Energy = MaceRandom.Current.NextFloat(FoodEnergyMinMax.Min, FoodEnergyMinMax.Max);
            newFood.Nutrients = MaceRandom.Current.NextFloat(FoodNutrientsMinMax.Min, FoodNutrientsMinMax.Max);
            newFood.Size = Globals.Map(newFood.Mass, GeneratedFoodMassMinMax.Min, GeneratedFoodMassMinMax.Max, FoodSizeMinMax.Min, FoodSizeMinMax.Max);
            newFood.X = MaceRandom.Current.NextFloat(-newFood.Size / 2, (WorldBounds.X + WorldBounds.Width) - newFood.Size / 2);
            newFood.Y = MaceRandom.Current.NextFloat(-newFood.Size / 2, (WorldBounds.Y + WorldBounds.Height) - newFood.Size / 2);

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
                    Mass = MaceRandom.Current.NextFloat(GeneratedCreatureMassMinMax.Min, GeneratedCreatureMassMinMax.Max),
                    MaxEnergy = MaxCreatureEnergy,
                    Energy = MaxCreatureEnergy * 0.75f,
                    MaxAge = CreatureMaxAge,
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
                    HealthPoints = GeneratedCreatureMassMinMax.Max * 0.9f,
                    MaxHealthPoints = CreatureMaxHealthPoints,
                    NaturalHealInterval = CreatureNaturalHealInterval,
                    NaturalHealHealthPoints = CreatureMaxHealthPoints * 0.05f,
                    MassRequiredToReproduce = CreatureMassRequiredToReproduce,
                    MoveEffort = 1f
                };

                newCreature.Size = Globals.Map(newCreature.Mass, GeneratedCreatureMassMinMax.Min, GeneratedCreatureMassMinMax.Max, CreatureSizeMinMax.Min, CreatureSizeMinMax.Max);
                newCreature.X = MaceRandom.Current.NextFloat(-newCreature.Size / 2, (WorldBounds.X + WorldBounds.Width) - newCreature.Size / 2);
                newCreature.Y = MaceRandom.Current.NextFloat(-newCreature.Size / 2, (WorldBounds.Y + WorldBounds.Height) - newCreature.Size / 2);

                newCreature.Brain = new NeuralNetwork(NeuralNetwork.GenerateInputNodes(PossibleCreatureInputs)
                    .Concat(NeuralNetwork.GenerateProcessNodes(0, MaxCreatureProcessNodes))
                    .Concat(NeuralNetwork.GenerateOutputNodes(PossibleCreatureActions)));

                newCreature.Brain.Connections.Clear();
                newCreature.Brain.Connections.AddRange(newCreature.Brain.GenerateRandomConnections(CreatureConnectionsMinMax.Min, CreatureConnectionsMinMax.Max, ConnectionWeightBound));

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
