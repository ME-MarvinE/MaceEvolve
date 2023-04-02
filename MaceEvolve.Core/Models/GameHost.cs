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
    public class GameHost<TStep, TCreature, TFood> where TCreature : class, ICreature, new() where TFood : class, IFood, new() where TStep : class, IStep<TCreature, TFood>, new()
    {
        #region Fields
        protected TCreature bestCreature;
        protected TCreature selectedCreature;
        #endregion

        #region Properties
        public TStep CurrentStep { get; private set; }
        public int MaxCreatureAmount { get; set; } = 300;
        public int MaxFoodAmount { get; set; } = 350;
        public IRectangle WorldBounds { get; set; } = new Rectangle(0, 0, 512, 512);
        public IRectangle SuccessBounds { get; set; }
        public int MinCreatureConnections { get; set; } = 4;
        public int MaxCreatureConnections { get; set; } = 64;
        public float CreatureSpeed { get; set; }
        public int MaxCreatureProcessNodes { get; set; } = 2;
        public float CreatureOffspringBrainMutationChance { get; set; } = 1 / 3f;
        public int CreatureOffspringBrainMutationAttempts { get; set; } = 1;
        public float ConnectionWeightBound { get; set; } = 4;
        public float MaxCreatureEnergy { get; set; } = 1000;
        public float FoodSize { get; set; } = 7;
        public float CreatureSize { get; set; } = 10;
        public float InitialCreatureNutrients { get; set; } = 30;
        public float EnergyRequiredForCreatureToReproduce { get; set; } = 100;
        public float NutrientsRequiredForCreatureToReproduce { get; set; } = 50;
        public float MinimumSuccessfulCreatureFitness { get; set; } = 0.25f;
        public float ReproductionNodeBiasVariance { get; set; }
        public float ReproductionConnectionWeightVariance { get; set; }
        public float CreatureMetabolism { get; set; } = 0.1f;
        public float FoodEnergyPerServing { get; set; } = 150;
        public float FoodNutrientsPerServing { get; set; } = 50;
        public float FoodServingDigestionCost { get; set; } = 0.05f;
        public float CreatureSightRange { get; set; } = 100;
        public int MaxCreatureOffSpringPerReproduction { get; set; } = 2;
        public bool LoopWorldBounds { get; set; } = true;

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
        public virtual void ResetStep(List<TCreature> creatures, List<TFood> food)
        {
            BestCreature = null;
            SelectedCreature = null;
            CurrentStep = CreateStep(creatures, food);
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

                    successfulCreaturesFitnesses.Add(creature, Globals.Map(distanceFromMiddle, 0, successBoundsHypotenuse, 1, 0));
                }
            }
            else
            {
                float mostEnergy = creatures.Max(x => x.Energy);
                float mostNutrients = creatures.Max(x => x.Nutrients);
                float mostTimesReproduced = creatures.Max(x => x.TimesReproduced);

                if (mostEnergy == 0 && mostNutrients == 0 && mostTimesReproduced == 0)
                {
                    return new Dictionary<TCreature, float>();
                }

                foreach (var creature in creatures)
                {
                    float energyFitness = mostEnergy == 0 ? 0 : creature.Energy / mostEnergy;
                    float nutrientsFitness = mostNutrients == 0 ? 0 : creature.Nutrients / mostNutrients;
                    float timesReproducedFitness = mostTimesReproduced == 0 ? 0 : creature.TimesReproduced / mostTimesReproduced;
                    float fitness = Globals.Map(energyFitness + nutrientsFitness + timesReproducedFitness, 0, 3, 0, 1);

                    successfulCreaturesFitnesses.Add(creature, fitness);
                }
            }

            return successfulCreaturesFitnesses;
        }
        public virtual TStep CreateStep(IEnumerable<TCreature> creatures, IEnumerable<TFood> food)
        {
            return new TStep()
            {
                Creatures = new ConcurrentBag<TCreature>(creatures),
                Food = new ConcurrentBag<TFood>(food),
                WorldBounds = WorldBounds,
                ConnectionWeightBound = ConnectionWeightBound,
                MinCreatureConnections = MinCreatureConnections,
                MaxCreatureConnections = MaxCreatureConnections,
                MaxCreatureProcessNodes = MaxCreatureProcessNodes,
                LoopWorldBounds = LoopWorldBounds
            };
        }
        public virtual void NextStep(bool gatherInfoForAllCreatures = false)
        {
            TStep generatedStep = CreateStep(CurrentStep.Creatures.Where(x => !x.IsDead), CurrentStep.Food.Where(x => x.Servings > 0));

            generatedStep.ExecuteActions(CurrentStep.RequestedActions);

            TCreature newBestCreature = null;

            float successBoundsMiddleX = Globals.MiddleX(SuccessBounds.X, SuccessBounds.Width);
            float successBoundsMiddleY = Globals.MiddleY(SuccessBounds.Y, SuccessBounds.Height);

            Parallel.ForEach(generatedStep.Creatures, new ParallelOptions() { MaxDegreeOfParallelism = Environment.ProcessorCount }, creature =>
            {
                if (!creature.IsDead || creature == BestCreature || creature == SelectedCreature)
                {
                    //Calculate the output values for the creature's nodes.
                    IEnumerable<CreatureInput> inputsRequiredForStep = creature.Brain.GetInputsRequiredForStep();
                    Dictionary<CreatureInput, float> inputToInputValueDict = generatedStep.GenerateCreatureInputValues(inputsRequiredForStep, creature);
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

                    generatedStep.QueueAction(stepAction);

                    //Store properties of the creature's current status.
                    bool trackBrainOutput = gatherInfoForAllCreatures || creature == BestCreature || creature == SelectedCreature;

                    if (trackBrainOutput)
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

                        generatedStep.CreaturesBrainOutput[creature] = currentStepNodeInfo;
                    }
                }

                //Identify the best creature in the step.

                if (UseSuccessBounds)
                {
                    float distanceFromMiddle = Globals.GetDistanceFrom(creature.MX, creature.MY, successBoundsMiddleX, successBoundsMiddleY);
                    float? newBestCreatureDistanceFromMiddle = newBestCreature == null ? (float?)null : Globals.GetDistanceFrom(newBestCreature.MX, newBestCreature.MY, successBoundsMiddleX, successBoundsMiddleY);

                    if (newBestCreatureDistanceFromMiddle == null || distanceFromMiddle < newBestCreatureDistanceFromMiddle)
                    {
                        newBestCreature = creature;
                    }
                }
                else if (newBestCreature == null || creature.TimesReproduced > newBestCreature.TimesReproduced)
                {
                    newBestCreature = creature;
                }
            });

            BestCreature = newBestCreature;

            if (MaceRandom.Current.NextFloat() <= 0.8 && generatedStep.Food.Count < MaxFoodAmount)
            {
                generatedStep.Food.Add(CreateFoodWithRandomLocation());
            }

            CurrentStep = generatedStep;
        }
        public virtual TFood CreateFoodWithRandomLocation()
        {
            return new TFood()
            {
                X = MaceRandom.Current.NextFloat(0, WorldBounds.X + WorldBounds.Width),
                Y = MaceRandom.Current.NextFloat(0, WorldBounds.Y + WorldBounds.Height),
                Servings = 1,
                EnergyPerServing = FoodEnergyPerServing,
                ServingDigestionCost = FoodServingDigestionCost,
                Size = FoodSize,
                NutrientsPerServing = FoodNutrientsPerServing,
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
                    X = MaceRandom.Current.NextFloat(0, WorldBounds.X + WorldBounds.Width),
                    Y = MaceRandom.Current.NextFloat(0, WorldBounds.Y + WorldBounds.Height),
                    Size = CreatureSize,
                    Speed = CreatureSpeed,
                    Metabolism = CreatureMetabolism,
                    Energy = MaxCreatureEnergy * 0.75f,
                    MaxEnergy = MaxCreatureEnergy,
                    SightRange = CreatureSightRange,
                    Nutrients = InitialCreatureNutrients,
                    NutrientsRequiredToReproduce = NutrientsRequiredForCreatureToReproduce,
                    EnergyRequiredToReproduce = EnergyRequiredForCreatureToReproduce,
                    MaxOffspringPerReproduction = MaxCreatureOffSpringPerReproduction,
                    OffspringBrainMutationAttempts = CreatureOffspringBrainMutationAttempts,
                    OffspringBrainMutationChance = CreatureOffspringBrainMutationChance,
                    MoveCost = 0.25f
                };

                newCreature.Brain = new NeuralNetwork(NeuralNetwork.GenerateInputNodes(PossibleCreatureInputs)
                    .Concat(NeuralNetwork.GenerateProcessNodes(MaxCreatureProcessNodes, 0.75f))
                    .Concat(NeuralNetwork.GenerateOutputNodes(PossibleCreatureActions)));

                newCreature.Brain.Connections = newCreature.Brain.GenerateRandomConnections(MinCreatureConnections, MaxCreatureConnections, ConnectionWeightBound);

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
