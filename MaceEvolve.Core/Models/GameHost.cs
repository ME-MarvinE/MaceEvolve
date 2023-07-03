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
        public virtual StepResult<TCreature> NextStep(IEnumerable<StepAction<TCreature>> actionsToExecute, bool gatherBestCreatureInfo, bool gatherSelectedCreatureInfo, bool gatherAliveCreatureInfo, bool gatherDeadCreatureInfo)
        {
            CurrentStep.Creatures = new ConcurrentBag<TCreature>(CurrentStep.Creatures.Where(x => !x.IsDead));
            CurrentStep.Food = new ConcurrentBag<TFood>(CurrentStep.Food.Where(x => x.Energy > 0));
            CurrentStep.ExecuteActions(actionsToExecute);

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
