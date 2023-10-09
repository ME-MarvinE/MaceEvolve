using MaceEvolve.Core.Enums;
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
        #region Properties
        public ConcurrentBag<TCreature> Creatures { get; set; }
        public ConcurrentBag<TFood> Food { get; set; }
        public Rectangle WorldBounds { get; set; }
        public int MinCreatureConnections { get; set; } = 4;
        public int MaxCreatureConnections { get; set; } = 128;
        public int MaxCreatureProcessNodes { get; set; } = 3;
        public float ConnectionWeightBound { get; set; } = 4;
        public bool LoopWorldBounds { get; set; }
        public ConcurrentDictionary<TCreature, List<TCreature>> VisibleCreaturesDict { get; } = new ConcurrentDictionary<TCreature, List<TCreature>>();
        public ConcurrentDictionary<TCreature, List<TFood>> VisibleFoodDict { get; } = new ConcurrentDictionary<TCreature, List<TFood>>();
        #endregion

        #region Methods
        public bool CreatureTryEat(TCreature creature)
        {
            IEnumerable<TFood> visibleFoodOrderedByDistance = VisibleFoodDict[creature].OrderBy(x => Globals.GetDistanceFrom(creature.X, creature.Y, x.X, x.Y));

            IFood closestFood = visibleFoodOrderedByDistance.FirstOrDefault();

            if (closestFood?.Energy > 0 && Globals.GetDistanceFrom(creature.MX, creature.MY, closestFood.MX, closestFood.MY) < (closestFood.Size + creature.Size) / 2)
            {
                float energyToTake = Math.Min(creature.EnergyPerEat, closestFood.Energy);
                float nutrientsToTake = Math.Min(creature.NutrientsPerEat, closestFood.Nutrients);

                closestFood.Energy -= energyToTake;
                creature.Energy += energyToTake;
                closestFood.Nutrients -= nutrientsToTake;
                creature.Nutrients += nutrientsToTake;
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
                newCreature.MaxAge = creature.MaxAge;
                newCreature.SightRange = creature.SightRange;
                newCreature.MaxOffspringPerReproduction = creature.MaxOffspringPerReproduction;
                newCreature.MaxEnergy = creature.MaxEnergy;
                newCreature.Energy = creature.EnergyRequiredToReproduce / 2;
                newCreature.MaxNutrients = creature.MaxNutrients;
                newCreature.Nutrients = creature.NutrientsRequiredToReproduce / 2;
                newCreature.NutrientsRequiredToReproduce = creature.NutrientsRequiredToReproduce;
                newCreature.EnergyRequiredToReproduce = creature.EnergyRequiredToReproduce;
                newCreature.OffspringBrainMutationAttempts = creature.OffspringBrainMutationAttempts;
                newCreature.EnergyPerEat = creature.EnergyPerEat;
                newCreature.NutrientsPerEat = creature.NutrientsPerEat;
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
                        minCreatureConnections: MinCreatureConnections,
                        maxCreatureConnections: MaxCreatureConnections,
                        maxCreatureProcessNodes: MaxCreatureProcessNodes,
                        connectionWeightBound: ConnectionWeightBound);
                }

                creature.Energy -= creature.EnergyRequiredToReproduce;
                creature.Nutrients -= creature.NutrientsRequiredToReproduce;
                creature.TimesReproduced += 1;

                offSpring.Add(newCreature);
            }

            return offSpring;
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

            List<TCreature> visibleCreatures = VisibleCreaturesDict[creature];
            IEnumerable<TCreature> visibleCreaturesOrderedByDistance = visibleCreatures.OrderBy(x => Globals.GetDistanceFrom(creature.MX, creature.MY, x.MX, x.MY));
            List<TFood> visibleFood = VisibleFoodDict[creature];
            IEnumerable<TFood> visibleFoodOrderedByDistance = visibleFood.OrderBy(x => Globals.GetDistanceFrom(creature.MX, creature.MY, x.MX, x.MY));
            TCreature closestCreatureToLeft = null;
            TCreature closestCreatureToRight = null;
            TCreature closestCreatureToFront = null;
            TCreature closestCreatureToBack = null;
            TFood closestFoodToLeft = null;
            TFood closestFoodToRight = null;
            TFood closestFoodToFront = null;
            TFood closestFoodToBack = null;

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

                        case CreatureInput.CreatureToLeftProximity:
                            closestCreatureToLeft ??= visibleCreaturesOrderedByDistance.FirstOrDefault(x => x.MX <= creature.MX);

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

                        case CreatureInput.CreatureToRightProximity:
                            closestCreatureToRight ??= visibleCreaturesOrderedByDistance.FirstOrDefault(x => x.MX >= creature.MX);

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

                        case CreatureInput.CreatureToFrontProximity:
                            closestCreatureToFront ??= visibleCreaturesOrderedByDistance.FirstOrDefault(x => x.MY <= creature.MY);

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

                        case CreatureInput.CreatureToBackProximity:
                            closestCreatureToBack ??= visibleCreaturesOrderedByDistance.FirstOrDefault(x => x.MY >= creature.MY);

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

                        case CreatureInput.FoodToLeftProximity:
                            closestFoodToLeft ??= visibleFoodOrderedByDistance.FirstOrDefault(x => x.MX <= creature.MX);

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

                        case CreatureInput.FoodToLeftPercentMaxEnergy:
                            closestFoodToLeft ??= visibleFoodOrderedByDistance.FirstOrDefault(x => x.MX <= creature.MX);

                            if (closestFoodToLeft == null)
                            {
                                creatureInputValue = 0;
                            }
                            else
                            {
                                creatureInputValue = Globals.Map(closestFoodToLeft.Energy, 0, closestFoodToLeft.MaxEnergy, 0, 1);
                            }
                            break;

                        case CreatureInput.FoodToLeftPercentMaxNutrients:
                            closestFoodToLeft ??= visibleFoodOrderedByDistance.FirstOrDefault(x => x.MX <= creature.MX);

                            if (closestFoodToLeft == null)
                            {
                                creatureInputValue = 0;
                            }
                            else
                            {
                                creatureInputValue = Globals.Map(closestFoodToLeft.Nutrients, 0, closestFoodToLeft.MaxNutrients, 0, 1);
                            }
                            break;

                        case CreatureInput.FoodToRightProximity:
                            closestFoodToRight ??= visibleFoodOrderedByDistance.FirstOrDefault(x => x.MX >= creature.MX);

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

                        case CreatureInput.FoodToRightPercentMaxEnergy:
                            closestFoodToRight ??= visibleFoodOrderedByDistance.FirstOrDefault(x => x.MX <= creature.MX);

                            if (closestFoodToRight == null)
                            {
                                creatureInputValue = 0;
                            }
                            else
                            {
                                creatureInputValue = Globals.Map(closestFoodToRight.Energy, 0, closestFoodToRight.MaxEnergy, 0, 1);
                            }
                            break;

                        case CreatureInput.FoodToRightPercentMaxNutrients:
                            closestFoodToRight ??= visibleFoodOrderedByDistance.FirstOrDefault(x => x.MX <= creature.MX);

                            if (closestFoodToRight == null)
                            {
                                creatureInputValue = 0;
                            }
                            else
                            {
                                creatureInputValue = Globals.Map(closestFoodToRight.Nutrients, 0, closestFoodToRight.MaxNutrients, 0, 1);
                            }
                            break;

                        case CreatureInput.FoodToFrontProximity:
                            closestFoodToFront ??= visibleFoodOrderedByDistance.FirstOrDefault(x => x.MY <= creature.MY);

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

                        case CreatureInput.FoodToFrontPercentMaxEnergy:
                            closestFoodToFront ??= visibleFoodOrderedByDistance.FirstOrDefault(x => x.MX <= creature.MX);

                            if (closestFoodToFront == null)
                            {
                                creatureInputValue = 0;
                            }
                            else
                            {
                                creatureInputValue = Globals.Map(closestFoodToFront.Energy, 0, closestFoodToFront.MaxEnergy, 0, 1);
                            }
                            break;

                        case CreatureInput.FoodToFrontPercentMaxNutrients:
                            closestFoodToFront ??= visibleFoodOrderedByDistance.FirstOrDefault(x => x.MX <= creature.MX);

                            if (closestFoodToFront == null)
                            {
                                creatureInputValue = 0;
                            }
                            else
                            {
                                creatureInputValue = Globals.Map(closestFoodToFront.Nutrients, 0, closestFoodToFront.MaxNutrients, 0, 1);
                            }
                            break;

                        case CreatureInput.FoodToBackProximity:
                            closestFoodToBack ??= visibleFoodOrderedByDistance.FirstOrDefault(x => x.MY >= creature.MY);

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

                        case CreatureInput.FoodToBackPercentMaxEnergy:
                            closestFoodToBack ??= visibleFoodOrderedByDistance.FirstOrDefault(x => x.MX <= creature.MX);

                            if (closestFoodToBack == null)
                            {
                                creatureInputValue = 0;
                            }
                            else
                            {
                                creatureInputValue = Globals.Map(closestFoodToBack.Energy, 0, closestFoodToBack.MaxEnergy, 0, 1);
                            }

                            break;

                        case CreatureInput.FoodToBackPercentMaxNutrients:
                            closestFoodToBack ??= visibleFoodOrderedByDistance.FirstOrDefault(x => x.MX <= creature.MX);

                            if (closestFoodToBack == null)
                            {
                                creatureInputValue = 0;
                            }
                            else
                            {
                                creatureInputValue = Globals.Map(closestFoodToBack.Nutrients, 0, closestFoodToBack.MaxNutrients, 0, 1);
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

                        case CreatureInput.PercentMaxAge:
                            creatureInputValue = Globals.Map((float)creature.Age, 0, creature.MaxAge, 0, 1);
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
