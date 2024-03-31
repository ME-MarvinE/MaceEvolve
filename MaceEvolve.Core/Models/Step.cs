﻿using MaceEvolve.Core.Enums;
using MaceEvolve.Core.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace MaceEvolve.Core.Models
{
    public class Step<TCreature, TFood> : IStep<TCreature, TFood> where TCreature : class, ICreature, new() where TFood : class, IFood
    {
        #region Fields
        private static float _creatureTurnSpeed = 25;
        private static float _AttackInitiatorScoreModifier = 1.25f;
        #endregion

        #region Properties
        public ConcurrentBag<TCreature> Creatures { get; set; }
        public ConcurrentBag<TFood> Food { get; set; }
        public Rectangle WorldBounds { get; set; }
        public MinMaxVal<int> CreatureConnectionsMinMax { get; set; } = MinMaxVal.Create(4, 128);
        public int MaxCreatureProcessNodes { get; set; } = 3;
        public float ConnectionWeightBound { get; set; } = 4;
        public bool LoopWorldBounds { get; set; }
        public float ParentAttributesVariance { get; set; } = 0.05f;

        public ConcurrentDictionary<TCreature, List<TCreature>> VisibleCreaturesDict { get; } = new ConcurrentDictionary<TCreature, List<TCreature>>();
        public ConcurrentDictionary<TCreature, List<TFood>> VisibleFoodDict { get; } = new ConcurrentDictionary<TCreature, List<TFood>>();
        public ConcurrentDictionary<TCreature, float> CreatureToCachedAreaDict { get; } = new ConcurrentDictionary<TCreature, float>();
        public ConcurrentDictionary<TFood, float> FoodToCachedAreaDict { get; } = new ConcurrentDictionary<TFood, float>();
        #endregion

        #region Methods
        public bool? CreatureTryEat(TCreature creature)
        {
            IEnumerable<TFood> visibleFoodOrderedByDistance = VisibleFoodDict[creature].OrderBy(x => Globals.GetDistanceFrom(creature.X, creature.Y, x.X, x.Y));
            IFood closestFood = visibleFoodOrderedByDistance.FirstOrDefault();

            creature.AttemptedEatsCount += 1;

            if (closestFood == null)
            {
                return null;
            }

            if (Globals.GetDistanceFrom(creature.MX, creature.MY, closestFood.MX, closestFood.MY) <= (closestFood.Size + creature.Size) / 2)
            {
                float energyToTake = Math.Min(creature.EnergyPerEat, closestFood.Energy);
                float nutrientsToTake = Math.Min(creature.NutrientsPerEat, closestFood.Nutrients);
                float massToTake = closestFood.Mass;

                closestFood.Energy -= energyToTake;
                creature.Energy += energyToTake;
                closestFood.Nutrients -= nutrientsToTake;
                creature.Nutrients += nutrientsToTake;
                creature.Mass += massToTake;
                closestFood.Mass -= massToTake;
                creature.FoodEaten += 1;

                return true;
            }
            else
            {
                return null;
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
                newCreature.Mass = creature.MassRequiredToReproduce;
                newCreature.MassRequiredToReproduce = creature.MassRequiredToReproduce;
                newCreature.MaxEnergy = creature.MaxEnergy;
                newCreature.Size = MaceRandom.Current.NextFloatVariance(creature.Size, ParentAttributesVariance);
                newCreature.MoveCost = creature.MoveCost;
                newCreature.AttackCost = creature.AttackCost;
                newCreature.DefendCost = creature.DefendCost;
                newCreature.MaxAge = creature.MaxAge;
                newCreature.SightRange = creature.SightRange;
                newCreature.MaxOffspringPerReproduction = creature.MaxOffspringPerReproduction;
                newCreature.Energy = creature.EnergyRequiredToReproduce;
                newCreature.MaxNutrients = creature.MaxNutrients;
                newCreature.Nutrients = creature.NutrientsRequiredToReproduce;
                newCreature.NutrientsRequiredToReproduce = creature.NutrientsRequiredToReproduce;
                newCreature.EnergyRequiredToReproduce = creature.EnergyRequiredToReproduce;
                newCreature.OffspringBrainMutationAttempts = creature.OffspringBrainMutationAttempts;
                newCreature.EnergyPerEat = creature.EnergyPerEat;
                newCreature.NutrientsPerEat = creature.NutrientsPerEat;
                newCreature.MaxHealthPoints = creature.MaxHealthPoints;
                newCreature.HealthPoints = newCreature.MaxHealthPoints * 0.9f;
                newCreature.NaturalHealHealthPoints = newCreature.MaxHealthPoints * 0.05f;
                newCreature.NaturalHealInterval = creature.NaturalHealInterval;
                newCreature.MoveEffort = 1f;
                newCreature.X = creature.X + MaceRandom.Current.NextFloat(-maxXDistanceOfOffspring, maxXDistanceOfOffspring + 1);
                newCreature.Y = creature.Y + MaceRandom.Current.NextFloat(-maxYDistanceOfOffspring, maxYDistanceOfOffspring + 1);

                if (newCreature.MX < WorldBounds.X)
                {
                    if (LoopWorldBounds)
                    {
                        newCreature.X = (WorldBounds.X + WorldBounds.Width) - newCreature.Size / 2;
                    }
                    else
                    {
                        newCreature.X = WorldBounds.X - newCreature.Size / 2;
                    }
                }
                else if (newCreature.MX > WorldBounds.X + WorldBounds.Width)
                {
                    if (LoopWorldBounds)
                    {
                        newCreature.X = WorldBounds.X - newCreature.Size / 2;
                    }
                    else
                    {
                        newCreature.X = (WorldBounds.X + WorldBounds.Width) - newCreature.Size / 2;
                    }
                }

                if (creature.MY < WorldBounds.Y)
                {
                    if (LoopWorldBounds)
                    {
                        newCreature.Y = (WorldBounds.Y + WorldBounds.Height) - newCreature.Size / 2;
                    }
                    else
                    {
                        newCreature.Y = WorldBounds.Y - newCreature.Size / 2;
                    }
                }
                else if (newCreature.MY > WorldBounds.Y + WorldBounds.Height)
                {
                    if (LoopWorldBounds)
                    {
                        newCreature.Y = WorldBounds.Y - newCreature.Size / 2;
                    }
                    else
                    {
                        newCreature.Y = (WorldBounds.Y + WorldBounds.Height) - newCreature.Size / 2;
                    }
                }

                for (int j = 0; j < creature.OffspringBrainMutationAttempts; j++)
                {
                    bool mutated = newCreature.Brain.MutateNetwork(
                        createRandomNodeChance: creature.OffspringBrainMutationChance,
                        removeRandomNodeChance: creature.OffspringBrainMutationChance,
                        mutateRandomNodeBiasChance: creature.OffspringBrainMutationChance,
                        createRandomConnectionChance: creature.OffspringBrainMutationChance,
                        removeRandomConnectionChance: creature.OffspringBrainMutationChance,
                        mutateRandomConnectionSourceChance: creature.OffspringBrainMutationChance,
                        mutateRandomConnectionTargetChance: creature.OffspringBrainMutationChance,
                        mutateRandomConnectionWeightChance: creature.OffspringBrainMutationChance,
                        possibleInputs: Globals.AllCreatureInputs,
                        possibleOutputs: Globals.AllCreatureActions,
                        minCreatureConnections: CreatureConnectionsMinMax.Min,
                        maxCreatureConnections: CreatureConnectionsMinMax.Max,
                        maxCreatureProcessNodes: MaxCreatureProcessNodes,
                        connectionWeightBound: ConnectionWeightBound);
                }

                creature.Energy -= creature.EnergyRequiredToReproduce;
                creature.Nutrients -= creature.NutrientsRequiredToReproduce;
                creature.Mass -= creature.MassRequiredToReproduce;
                creature.TimesReproduced += 1;

                offSpring.Add(newCreature);
            }

            return offSpring;
        }
        private static void LimitCreatureBounds(TCreature creature, Rectangle worldBounds, bool loopWorldBounds)
        {
            float worldBoundsBottom = worldBounds.Y + worldBounds.Height;
            float worldBoundsRight = worldBounds.X + worldBounds.Width;

            if (creature.MY < worldBounds.Y)
            {
                if (loopWorldBounds)
                {
                    creature.Y = ((worldBounds.Y + worldBounds.Height) - creature.Size / 2) - (worldBounds.Y - creature.MY);
                }
                else
                {
                    creature.Y += creature.Speed * creature.MoveEffort;
                }
            }
            else if (creature.MY > worldBoundsBottom)
            {
                if (loopWorldBounds)
                {
                    creature.Y = (worldBounds.Y - creature.Size / 2) + (creature.MY - worldBoundsBottom);
                }
                else
                {
                    creature.Y -= creature.Speed * creature.MoveEffort;
                }
            }

            if (creature.MX < worldBounds.X)
            {
                if (loopWorldBounds)
                {
                    creature.X = ((worldBounds.X + worldBounds.Width) - creature.Size / 2) - (worldBounds.X - creature.MX);
                }
                else
                {
                    creature.X += creature.Speed * creature.MoveEffort;
                }
            }
            else if (creature.MX > worldBoundsRight)
            {
                if (loopWorldBounds)
                {
                    creature.X = (worldBounds.X - creature.Size / 2) + (creature.MX - worldBoundsRight);
                }
                else
                {
                    creature.X -= creature.Speed * creature.MoveEffort;
                }
            }
        }
        public void CreatureMoveForwards(TCreature creature)
        {
            CreatureMove(creature, creature.ForwardAngle);
        }
        public void CreatureMoveBackwards(TCreature creature)
        {
            CreatureMove(creature, creature.ForwardAngle + 180);
        }
        public void CreatureMoveLeft(TCreature creature)
        {
            CreatureMove(creature, creature.ForwardAngle - 90);
        }
        public void CreatureMoveRight(TCreature creature)
        {
            CreatureMove(creature, creature.ForwardAngle + 90);
        }
        private void CreatureMove(TCreature creature, float angle)
        {
            creature.X += MathF.Cos(Globals.AngleToRadians(angle)) * creature.Speed * creature.MoveEffort;
            creature.Y += MathF.Sin(Globals.AngleToRadians(angle)) * creature.Speed * creature.MoveEffort;
            LimitCreatureBounds(creature, WorldBounds, LoopWorldBounds);
            creature.Energy -= creature.MoveCost * creature.MoveEffort;
        }
        public void CreatureDoNothing()
        {

        }
        public bool? CreatureTryAttack(TCreature creature)
        {
            IEnumerable<TCreature> visibleCreaturesOrderedByDistance = VisibleCreaturesDict[creature].OrderBy(x => Globals.GetDistanceFrom(creature.X, creature.Y, x.X, x.Y));
            TCreature closestCreature = visibleCreaturesOrderedByDistance.FirstOrDefault();
            creature.AttemptedAttacksCount += 1;

            if (closestCreature == null)
            {
                return null;
            }

            bool? creatureAttackWasSuccessful;
            if (Globals.GetDistanceFrom(creature.MX, creature.MY, closestCreature.MX, closestCreature.MY) < (closestCreature.Size + creature.Size) / 2)
            {
                creatureAttackWasSuccessful = InitiateAttack(creature, closestCreature);
            }
            else
            {
                creatureAttackWasSuccessful = null;
                creature.Energy -= creature.AttackCost;
            }

            return creatureAttackWasSuccessful;
        }
        public static bool InitiateAttack(TCreature initiatingCreature, TCreature defendingCreature)
        {
            initiatingCreature.InitiatedAttacksCount += 1;
            float initiatorAttackScore = GetAttackScore(initiatingCreature, true);
            float defenderAttackScore = GetAttackScore(defendingCreature, false);
            float totalAttackScore = initiatorAttackScore + defenderAttackScore;
            float chanceToLandAttack = totalAttackScore == 0 ? 0 : initiatorAttackScore / totalAttackScore;
            bool initiatorLandedAttack = chanceToLandAttack != 0 && MaceRandom.Current.NextFloat() <= chanceToLandAttack;

            bool wasAttackSuccessful;
            if (initiatorLandedAttack)
            {
                TransferAttackValues(initiatingCreature, initiatorAttackScore, true, defendingCreature, defenderAttackScore);
                initiatingCreature.SuccessfulAttacksCount += 1;
                wasAttackSuccessful = true;
            }
            else
            {
                TransferAttackValues(defendingCreature, defenderAttackScore, false, initiatingCreature, initiatorAttackScore);
                defendingCreature.AttacksEvadedCount += 1;
                wasAttackSuccessful = false;
            }

            return wasAttackSuccessful;
        }
        public static void TransferAttackValues(TCreature winningCreature, float winnerAttackScore, bool winningCreatureWasInitiator, TCreature losingCreature, float loserAttackScore)
        {
            float totalAttackScore = winnerAttackScore + loserAttackScore;
            float winningCreatureEffort = totalAttackScore == 0 ? 0 : totalAttackScore / winnerAttackScore;
            float losingCreatureEffort = totalAttackScore == 0 ? 0 : totalAttackScore / loserAttackScore;

            if (winningCreatureWasInitiator)
            {
                float percentOfTargetSize = losingCreature.Size == 0 ? 1 : winningCreature.Size / losingCreature.Size;
                float energyToTake = Math.Min(losingCreature.Energy, (losingCreature.MaxEnergy / 8) * percentOfTargetSize);
                float massToTake = Math.Min(losingCreature.Mass, (losingCreature.Mass / 8) * percentOfTargetSize);
                float healthToTake = Math.Min(losingCreature.HealthPoints, losingCreature.MaxHealthPoints * percentOfTargetSize);
                float nutrientsToTake = Math.Min(losingCreature.Nutrients, (losingCreature.Nutrients / 8) * percentOfTargetSize);

                losingCreature.Energy -= energyToTake;
                winningCreature.Energy += energyToTake;
                losingCreature.Nutrients -= nutrientsToTake;
                winningCreature.Nutrients += nutrientsToTake;
                losingCreature.Mass -= massToTake;
                winningCreature.Mass += massToTake;
                losingCreature.HealthPoints -= healthToTake;

                winningCreature.Energy -= winningCreature.AttackCost * winningCreatureEffort;
                losingCreature.Energy -= losingCreature.DefendCost * losingCreatureEffort;
            }
            else
            {
                winningCreature.Energy -= winningCreature.DefendCost * winningCreatureEffort;
                losingCreature.Energy -= losingCreature.AttackCost * losingCreatureEffort;
            }
        }
        public void CreatureTurnLeft(TCreature creature)
        {
            CreatureTurn(creature, -_creatureTurnSpeed);
        }
        public void CreatureTurnRight(TCreature creature)
        {
            CreatureTurn(creature, _creatureTurnSpeed);
        }
        private void CreatureTurn(TCreature creature, float angle)
        {
            creature.ForwardAngle += angle;
        }
        private void CreatureSetMoveEffort(TCreature creature, float moveEffort)
        {
            creature.MoveEffort = moveEffort;
        }
        public void CreatureMoveTowardsClosestFood(TCreature creature)
        {
            IEnumerable<TFood> visibleFoodOrderedByDistance = VisibleFoodDict[creature].OrderBy(x => Globals.GetDistanceFrom(creature.X, creature.Y, x.X, x.Y));

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
        public static float GetAttackScore(TCreature creature, bool isInitiator)
        {
            if (creature.IsDead)
            {
                return 0;
            }

            float attackScore = creature.Mass * creature.Energy;

            if (isInitiator)
            {
                attackScore *= _AttackInitiatorScoreModifier;
            }

            return attackScore;
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
                                foreach (var creature in offSpring)
                                {
                                    Creatures.Add(creature);
                                }
                            }
                            break;

                        case CreatureAction.DoNothing:
                            CreatureDoNothing();
                            break;

                        case CreatureAction.TryAttack:
                            CreatureTryAttack(stepAction.Creature);
                            break;

                        case CreatureAction.TurnLeft:
                            CreatureTurnLeft(stepAction.Creature);
                            break;

                        case CreatureAction.TurnRight:
                            CreatureTurnRight(stepAction.Creature);
                            break;

                        case CreatureAction.SetMoveEffort:
                            CreatureSetMoveEffort(stepAction.Creature, stepAction.CreatureActionToOutputValueDict[CreatureAction.SetMoveEffort]);
                            break;

                        default:
                            throw new NotImplementedException($"{nameof(CreatureAction)} '{stepAction.Action}' has not been implemented.");
                    }

                    stepAction.Creature.Energy -= stepAction.Creature.Metabolism;

                    if (Globals.ShouldCreatureBeDead(stepAction.Creature))
                    {
                        stepAction.Creature.Die();
                    }
                }
            }
        }
        public Dictionary<CreatureInput, float> GenerateCreatureInputValues(IEnumerable<CreatureInput> creatureInputs, TCreature creature)
        {
            Dictionary<CreatureInput, float> creatureInputValues = new Dictionary<CreatureInput, float>();

            List<TCreature> visibleCreatures = VisibleCreaturesDict[creature];
            IEnumerable<TCreature> visibleCreaturesOrderedByDistance = visibleCreatures.OrderBy(x => Globals.GetDistanceFrom(creature.MX, creature.MY, x.MX, x.MY));
            List<TCreature> visibleCreaturesOrderedByDistanceList = null;
            List<TFood> visibleFood = VisibleFoodDict[creature];
            IEnumerable<TFood> visibleFoodOrderedByDistance = visibleFood.OrderBy(x => Globals.GetDistanceFrom(creature.MX, creature.MY, x.MX, x.MY));
            List<TFood> visibleFoodOrderedByDistanceList = null;
            TCreature closestVisibleCreature = null;
            TFood closestVisibleFood = null;
            float? visibleArea = null;

            foreach (var creatureInput in creatureInputs)
            {
                float creatureInputValue;

                if (!creatureInputValues.TryGetValue(creatureInput, out creatureInputValue))
                {
                    switch (creatureInput)
                    {
                        case CreatureInput.PercentMaxEnergy:
                            creatureInputValue = creature.MaxEnergy == 0 ? 1 : creature.Energy / creature.MaxEnergy;
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
                            creatureInputValue = creature.NutrientsRequiredToReproduce == 0 ? 1 : creature.Nutrients / creature.NutrientsRequiredToReproduce;
                            break;

                        case CreatureInput.PercentEnergyRequiredToReproduce:
                            creatureInputValue = creature.EnergyRequiredToReproduce == 0 ? 1 : creature.Energy / creature.EnergyRequiredToReproduce;
                            break;

                        case CreatureInput.PercentMaxAge:
                            creatureInputValue = creature.MaxAge == 0 ? 1 : (float)creature.Age / creature.MaxAge;
                            break;
                        case CreatureInput.PercentMaxHealth:
                            creatureInputValue = creature.MaxHealthPoints == 0 ? 1 : creature.HealthPoints / creature.MaxHealthPoints;
                            break;

                        case CreatureInput.WillNaturallyHeal:
                            creatureInputValue = creature.NaturalHealInterval == 0 ? 1 : (float)creature.StepsSinceLastNaturalHeal / creature.NaturalHealInterval;
                            break;

                        case CreatureInput.PercentMassRequiredToReproduce:
                            creatureInputValue = creature.MassRequiredToReproduce == 0 ? 1 : creature.Mass / creature.MassRequiredToReproduce;
                            break;

                        case CreatureInput.VisibleAreaCreatureDensity:
                            visibleArea ??= GetCreatureVisibleArea(creature);

                            if (visibleArea == 0)
                            {
                                creatureInputValue = 0;
                            }
                            else
                            {
                                float creaturesArea = 0;

                                foreach (var visibleCreature in visibleCreatures)
                                {
                                    float visibleCreatureArea = CreatureToCachedAreaDict.GetOrAdd(visibleCreature, (x) => GetCircleArea(x.Size / 2));
                                    creaturesArea += visibleCreatureArea;
                                }

                                creatureInputValue = creaturesArea >= visibleArea ? 1 : creaturesArea / visibleArea.Value;
                            }
                            break;

                        case CreatureInput.VisibleAreaFoodDensity:
                            visibleArea ??= GetCreatureVisibleArea(creature);

                            if (visibleArea == 0)
                            {
                                creatureInputValue = 0;
                            }
                            else
                            {
                                float foodArea = 0;

                                foreach (var vFood in visibleFood)
                                {
                                    float visibleFoodArea = FoodToCachedAreaDict.GetOrAdd(vFood, (x) => GetCircleArea(x.Size / 2));
                                    foodArea += visibleFoodArea;
                                }

                                creatureInputValue = foodArea >= visibleArea ? 1 : foodArea / visibleArea.Value;
                            }
                            break;

                        case CreatureInput.AngleToClosestVisibleCreature:
                            closestVisibleCreature ??= visibleCreaturesOrderedByDistance.FirstOrDefault();

                            if (closestVisibleCreature == null)
                            {
                                creatureInputValue = 0;
                            }
                            else
                            {
                                float angleFromClosestVisibleCreature = Globals.GetAngleBetweenF(creature.MX, creature.MY, closestVisibleCreature.MX, closestVisibleCreature.MY);
                                float distanceFromForwardAngle = Globals.AngleDifference(creature.ForwardAngle, Globals.Angle180RangeTo360Range(angleFromClosestVisibleCreature));

                                //The mapping of the value to the output is reversed so that relative to the forward angle, the more anti-clockwise a creature is,
                                //the lower the output will be.
                                creatureInputValue = Globals.Map(distanceFromForwardAngle, -creature.FieldOfView / 2, creature.FieldOfView / 2, 1, -1);
                            }
                            break;

                        case CreatureInput.AngleToClosestVisibleFood:
                            closestVisibleFood ??= visibleFoodOrderedByDistance.FirstOrDefault();

                            if (closestVisibleFood == null)
                            {
                                creatureInputValue = 0;
                            }
                            else
                            {
                                float angleFromClosestVisibleFood = Globals.GetAngleBetweenF(creature.MX, creature.MY, closestVisibleFood.MX, closestVisibleFood.MY);
                                float distanceFromForwardAngle = Globals.AngleDifference(creature.ForwardAngle, Globals.Angle180RangeTo360Range(angleFromClosestVisibleFood));

                                //The mapping of the value to the output is reversed so that relative to the forward angle, the more anti-clockwise a creature is,
                                //the lower the output will be.
                                creatureInputValue = Globals.Map(distanceFromForwardAngle, -creature.FieldOfView / 2, creature.FieldOfView / 2, 1, -1);
                            }
                            break;

                        case CreatureInput.ProximityToClosestVisibleCreature:
                            closestVisibleCreature ??= visibleCreaturesOrderedByDistance.FirstOrDefault();

                            if (closestVisibleCreature == null)
                            {
                                creatureInputValue = 0;
                            }
                            else
                            {
                                float distanceFromClosestVisibleCreature = Globals.GetDistanceFrom(creature.MX, creature.MY, closestVisibleCreature.MX, closestVisibleCreature.MY);

                                creatureInputValue = creature.SightRange == 0 ? 1 : 1 - (distanceFromClosestVisibleCreature / creature.SightRange);
                            }
                            break;

                        case CreatureInput.ProximityToClosestVisibleFood:
                            closestVisibleFood ??= visibleFoodOrderedByDistance.FirstOrDefault();

                            if (closestVisibleFood == null)
                            {
                                creatureInputValue = 0;
                            }
                            else
                            {
                                float distanceFromClosestVisibleFood = Globals.GetDistanceFrom(creature.MX, creature.MY, closestVisibleFood.MX, closestVisibleFood.MY);

                                creatureInputValue = creature.SightRange == 0 ? 1 : 1 - (distanceFromClosestVisibleFood / creature.SightRange);
                            }
                            break;

                        case CreatureInput.ClosestVisibleFoodEnergyPercentage:
                            closestVisibleFood ??= visibleFoodOrderedByDistance.FirstOrDefault();

                            if (closestVisibleFood == null)
                            {
                                creatureInputValue = 0;
                            }
                            else
                            {
                                creatureInputValue = closestVisibleFood.MaxEnergy == 0 ? 0 : closestVisibleFood.Energy / closestVisibleFood.MaxEnergy;
                            }
                            break;

                        case CreatureInput.ClosestVisibleFoodNutrientPercentage:
                            closestVisibleFood ??= visibleFoodOrderedByDistance.FirstOrDefault();

                            if (closestVisibleFood == null)
                            {
                                creatureInputValue = 0;
                            }
                            else
                            {
                                creatureInputValue = closestVisibleFood.MaxNutrients == 0 ? 0 : closestVisibleFood.Nutrients / closestVisibleFood.MaxNutrients;
                            }
                            break;

                        case CreatureInput.ClosestVisibleCreatureEnergyPercentage:
                            closestVisibleCreature ??= visibleCreaturesOrderedByDistance.FirstOrDefault();

                            if (closestVisibleCreature == null)
                            {
                                creatureInputValue = 0;
                            }
                            else
                            {
                                creatureInputValue = closestVisibleCreature.MaxEnergy == 0 ? 0 : closestVisibleCreature.Energy / closestVisibleCreature.MaxEnergy;
                            }
                            break;

                        case CreatureInput.ClosestVisibleCreatureAgePercentage:
                            closestVisibleCreature ??= visibleCreaturesOrderedByDistance.FirstOrDefault();

                            if (closestVisibleCreature == null)
                            {
                                creatureInputValue = 0;
                            }
                            else
                            {
                                creatureInputValue = closestVisibleCreature.MaxAge == 0 ? 0 : (float)closestVisibleCreature.Age / closestVisibleCreature.MaxAge;
                            }
                            break;

                        case CreatureInput.ClosestVisibleCreatureHealthPercentage:
                            closestVisibleCreature ??= visibleCreaturesOrderedByDistance.FirstOrDefault();

                            if (closestVisibleCreature == null)
                            {
                                creatureInputValue = 0;
                            }
                            else
                            {
                                creatureInputValue = closestVisibleCreature.MaxHealthPoints == 0 ? 0 : closestVisibleCreature.HealthPoints / closestVisibleCreature.MaxHealthPoints;
                            }
                            break;

                        case CreatureInput.AggressionPercentage:
                            int attemptedAttacksAndEats = creature.AttemptedAttacksCount + creature.AttemptedEatsCount;
                            creatureInputValue = attemptedAttacksAndEats == 0 ? 0 : (float)creature.AttemptedAttacksCount / attemptedAttacksAndEats;
                            break;

                        case CreatureInput.SuccessfulAttackPercentage:
                            creatureInputValue = creature.InitiatedAttacksCount == 0 ? 0 : (float)creature.SuccessfulAttacksCount / creature.InitiatedAttacksCount;
                            break;

                        case CreatureInput.SizeToClosestVisibleCreatureSize:
                            closestVisibleCreature ??= visibleCreaturesOrderedByDistance.FirstOrDefault();

                            if (closestVisibleCreature == null)
                            {
                                creatureInputValue = 1;
                            }
                            else if (creature.Size == closestVisibleCreature.Size)
                            {
                                creatureInputValue = 1;
                            }
                            else
                            {
                                creatureInputValue = closestVisibleCreature.Size == 0 ? 1 : creature.Size / closestVisibleCreature.Size;
                            }
                            break;

                        case CreatureInput.ClosestVisibleCreatureSizeToSize:
                            closestVisibleCreature ??= visibleCreaturesOrderedByDistance.FirstOrDefault();

                            if (closestVisibleCreature == null)
                            {
                                creatureInputValue = 0;
                            }
                            else if (creature.Size == closestVisibleCreature.Size)
                            {
                                creatureInputValue = 1;
                            }
                            else
                            {
                                creatureInputValue = creature.Size == 0 ? 1 : closestVisibleCreature.Size / creature.Size;
                            }
                            break;

                        case CreatureInput.AnyVisibleCreatures:
                            creatureInputValue = visibleCreatures.Count == 0 ? 0 : 1;
                            break;

                        case CreatureInput.AnyVisibleFood:
                            creatureInputValue = visibleFood.Count == 0 ? 0 : 1;
                            break;

                        default:
                            throw new NotImplementedException($"{nameof(CreatureInput)} '{creatureInput}' has not been implemented.");
                    }
                }

                if (visibleCreaturesOrderedByDistanceList != null)
                {
                    visibleCreaturesOrderedByDistance = visibleCreaturesOrderedByDistanceList;
                }

                if (visibleFoodOrderedByDistanceList != null)
                {
                    visibleFoodOrderedByDistance = visibleFoodOrderedByDistanceList;
                }

                creatureInputValues[creatureInput] = creatureInputValue;
            }

            return creatureInputValues;
        }
        private float GetCreatureVisibleArea(TCreature creature)
        {
            return (MathF.PI * creature.SightRange * creature.SightRange) * (creature.FieldOfView / 360);
        }
        private float GetCircleArea(float radius)
        {
            return MathF.PI * radius * radius;
        }
        #endregion
    }
}
