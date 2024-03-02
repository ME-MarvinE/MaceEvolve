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
        public ConcurrentDictionary<TCreature, float> CreatureToCachedAreaDict { get; } = new ConcurrentDictionary<TCreature, float>();
        public ConcurrentDictionary<TFood, float> FoodToCachedAreaDict { get; } = new ConcurrentDictionary<TFood, float>();
        #endregion

        #region Methods
        public bool CreatureTryEat(TCreature creature)
        {
            IEnumerable<TFood> visibleFoodOrderedByDistance = VisibleFoodDict[creature].OrderBy(x => Globals.GetDistanceFrom(creature.X, creature.Y, x.X, x.Y));

            IFood closestFood = visibleFoodOrderedByDistance.FirstOrDefault();

            if (closestFood?.Mass > 0 && Globals.GetDistanceFrom(creature.MX, creature.MY, closestFood.MX, closestFood.MY) <= (closestFood.Size + creature.Size) / 2)
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
                newCreature.Mass = creature.MassRequiredToReproduce;
                newCreature.MassRequiredToReproduce = creature.MassRequiredToReproduce;
                newCreature.MaxEnergy = creature.MaxEnergy;
                newCreature.Size = creature.Size;
                newCreature.Speed = creature.Speed;
                newCreature.Metabolism = creature.Metabolism;
                newCreature.MoveCost = creature.MoveCost;
                newCreature.AttackCost = creature.AttackCost;
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
                newCreature.HealthPoints = creature.MaxHealthPoints * 0.9f;
                newCreature.MaxHealthPoints = creature.MaxHealthPoints;
                newCreature.NaturalHealInterval = creature.NaturalHealInterval;
                newCreature.NaturalHealHealthPoints = creature.MaxHealthPoints * 0.05f;


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
                    creature.Y += creature.Speed;
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
                    creature.Y -= creature.Speed;
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
                    creature.X += creature.Speed;
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
                    creature.X -= creature.Speed;
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
            creature.X += MathF.Cos(Globals.AngleToRadians(angle)) * creature.Speed;
            creature.Y += MathF.Sin(Globals.AngleToRadians(angle)) * creature.Speed;
            LimitCreatureBounds(creature, WorldBounds, LoopWorldBounds);
            creature.Energy -= creature.MoveCost;
        }
        public void CreatureDoNothing()
        {

        }
        public bool CreatureTryAttack(TCreature creature)
        {
            IEnumerable<TCreature> visibleCreaturesOrderedByDistance = VisibleCreaturesDict[creature].OrderBy(x => Globals.GetDistanceFrom(creature.X, creature.Y, x.X, x.Y));
            TCreature closestCreature = visibleCreaturesOrderedByDistance.FirstOrDefault();

            bool creatureSuccessfullyAttacked;

            if (closestCreature?.Energy > 0 && Globals.GetDistanceFrom(creature.MX, creature.MY, closestCreature.MX, closestCreature.MY) < (closestCreature.Size + creature.Size) / 2)
            {
                float percentOfTargetSize = closestCreature.Size == 0 ? 1 : creature.Size / closestCreature.Size;
                float energyToTake = Math.Min(closestCreature.Energy, (closestCreature.MaxEnergy / 8) * percentOfTargetSize);
                float massToTake = Math.Min(closestCreature.Mass, (closestCreature.Mass / 8) * percentOfTargetSize);
                float healthToTake = Math.Min(closestCreature.HealthPoints, closestCreature.MaxHealthPoints * percentOfTargetSize);
                float nutrientsToTake = Math.Min(closestCreature.Nutrients, (closestCreature.Nutrients / 8) * percentOfTargetSize);

                closestCreature.Energy -= energyToTake;
                creature.Energy += energyToTake;
                closestCreature.Nutrients -= nutrientsToTake;
                creature.Nutrients += nutrientsToTake;
                closestCreature.Mass -= massToTake;
                creature.Mass += massToTake;
                closestCreature.HealthPoints -= healthToTake;
                creature.TimesAttackedSuccessfully += 1;

                creatureSuccessfullyAttacked = true;
            }
            else
            {
                creatureSuccessfullyAttacked = false;
            }

            creature.Energy -= creature.AttackCost;

            return creatureSuccessfullyAttacked;
        }
        public void CreatureTurnLeft(TCreature creature)
        {
            CreatureTurn(creature, -90);
        }
        public void CreatureTurnRight(TCreature creature)
        {
            CreatureTurn(creature, 90);
        }
        public void CreatureTurnBackwards(TCreature creature)
        {
            CreatureTurn(creature, 180);
        }
        private void CreatureTurn(TCreature creature, float angle)
        {
            creature.ForwardAngle += angle;
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

                        case CreatureAction.TurnBackwards:
                            CreatureTurnBackwards(stepAction.Creature);
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
            TCreature closestCreatureToLeft = null;
            TCreature closestCreatureToRight = null;
            TCreature closestCreatureToFront = null;
            TCreature closestCreatureToBack = null;
            TFood closestFoodToLeft = null;
            TFood closestFoodToRight = null;
            TFood closestFoodToFront = null;
            TFood closestFoodToBack = null;
            float? visibleArea = null;

            foreach (var creatureInput in creatureInputs)
            {
                float creatureInputValue;

                if (!creatureInputValues.TryGetValue(creatureInput, out creatureInputValue))
                {
                    switch (creatureInput)
                    {
                        case CreatureInput.PercentMaxEnergy:
                            creatureInputValue = creature.Energy / creature.MaxEnergy;
                            break;

                        case CreatureInput.CreatureToLeftProximity:
                            closestCreatureToLeft ??= visibleCreaturesOrderedByDistance.FirstOrDefault(x => x.MX <= creature.MX);

                            if (closestCreatureToLeft == null)
                            {
                                creatureInputValue = 0;
                            }
                            else
                            {
                                float distanceFromClosestCreatureToLeft = Globals.GetDistanceFrom(creature.MX, creature.MY, closestCreatureToLeft.MX, closestCreatureToLeft.MY);

                                creatureInputValue = 1 - (distanceFromClosestCreatureToLeft / creature.SightRange);
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
                                float distanceFromClosestCreatureToRight = Globals.GetDistanceFrom(creature.MX, creature.MY, closestCreatureToRight.MX, closestCreatureToRight.MY);

                                creatureInputValue = 1 - (distanceFromClosestCreatureToRight / creature.SightRange);
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
                                float distanceFromClosestCreatureToFront = Globals.GetDistanceFrom(creature.MX, creature.MY, closestCreatureToFront.MX, closestCreatureToFront.MY);

                                creatureInputValue = 1 - (distanceFromClosestCreatureToFront / creature.SightRange);
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
                                float distanceFromClosestCreatureToBack = Globals.GetDistanceFrom(creature.MX, creature.MY, closestCreatureToBack.MX, closestCreatureToBack.MY);

                                creatureInputValue = 1 - (distanceFromClosestCreatureToBack / creature.SightRange);
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
                                float distanceFromClosestFoodToLeft = Globals.GetDistanceFrom(creature.MX, creature.MY, closestFoodToLeft.MX, closestFoodToLeft.MY);

                                creatureInputValue = 1 - (distanceFromClosestFoodToLeft / creature.SightRange);
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
                                creatureInputValue = closestFoodToLeft.Energy / closestFoodToLeft.MaxEnergy;
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
                                creatureInputValue = closestFoodToLeft.Nutrients / closestFoodToLeft.MaxNutrients;
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
                                float distanceFromClosestFoodToRight = Globals.GetDistanceFrom(creature.MX, creature.MY, closestFoodToRight.MX, closestFoodToRight.MY);

                                creatureInputValue = 1 - (distanceFromClosestFoodToRight / creature.SightRange);
                            }
                            break;

                        case CreatureInput.FoodToRightPercentMaxEnergy:
                            closestFoodToRight ??= visibleFoodOrderedByDistance.FirstOrDefault(x => x.MX >= creature.MX);

                            if (closestFoodToRight == null)
                            {
                                creatureInputValue = 0;
                            }
                            else
                            {
                                creatureInputValue = closestFoodToRight.Energy / closestFoodToRight.MaxEnergy;
                            }
                            break;

                        case CreatureInput.FoodToRightPercentMaxNutrients:
                            closestFoodToRight ??= visibleFoodOrderedByDistance.FirstOrDefault(x => x.MX >= creature.MX);

                            if (closestFoodToRight == null)
                            {
                                creatureInputValue = 0;
                            }
                            else
                            {
                                creatureInputValue = closestFoodToRight.Nutrients / closestFoodToRight.MaxNutrients;
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
                                float distanceFromClosestFoodToFront = Globals.GetDistanceFrom(creature.MX, creature.MY, closestFoodToFront.MX, closestFoodToFront.MY);

                                creatureInputValue = 1 - (distanceFromClosestFoodToFront / creature.SightRange);
                            }
                            break;

                        case CreatureInput.FoodToFrontPercentMaxEnergy:
                            closestFoodToFront ??= visibleFoodOrderedByDistance.FirstOrDefault(x => x.MY <= creature.MY);

                            if (closestFoodToFront == null)
                            {
                                creatureInputValue = 0;
                            }
                            else
                            {
                                creatureInputValue = closestFoodToFront.Energy / closestFoodToFront.MaxEnergy;
                            }
                            break;

                        case CreatureInput.FoodToFrontPercentMaxNutrients:
                            closestFoodToFront ??= visibleFoodOrderedByDistance.FirstOrDefault(x => x.MY <= creature.MY);

                            if (closestFoodToFront == null)
                            {
                                creatureInputValue = 0;
                            }
                            else
                            {
                                creatureInputValue = closestFoodToFront.Nutrients / closestFoodToFront.MaxNutrients;
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
                                float distanceFromClosestFoodToBack = Globals.GetDistanceFrom(creature.MX, creature.MY, closestFoodToBack.MX, closestFoodToBack.MY);

                                creatureInputValue = 1 - (distanceFromClosestFoodToBack / creature.SightRange);
                            }

                            break;

                        case CreatureInput.FoodToBackPercentMaxEnergy:
                            closestFoodToBack ??= visibleFoodOrderedByDistance.FirstOrDefault(x => x.MY >= creature.MY);

                            if (closestFoodToBack == null)
                            {
                                creatureInputValue = 0;
                            }
                            else
                            {
                                creatureInputValue = closestFoodToBack.Energy / closestFoodToBack.MaxEnergy;
                            }

                            break;

                        case CreatureInput.FoodToBackPercentMaxNutrients:
                            closestFoodToBack ??= visibleFoodOrderedByDistance.FirstOrDefault(x => x.MY >= creature.MY);

                            if (closestFoodToBack == null)
                            {
                                creatureInputValue = 0;
                            }
                            else
                            {
                                creatureInputValue = closestFoodToBack.Nutrients / closestFoodToBack.MaxNutrients;
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
                            creatureInputValue = creature.Nutrients / creature.NutrientsRequiredToReproduce;
                            break;

                        case CreatureInput.PercentEnergyRequiredToReproduce:
                            creatureInputValue = creature.Energy / creature.EnergyRequiredToReproduce;
                            break;

                        case CreatureInput.PercentMaxAge:
                            creatureInputValue = (float)creature.Age / creature.MaxAge;
                            break;
                        case CreatureInput.PercentMaxHealth:
                            creatureInputValue = creature.HealthPoints / creature.MaxHealthPoints;
                            break;

                        case CreatureInput.WillNaturallyHeal:
                            creatureInputValue = (float)creature.StepsSinceLastNaturalHeal / creature.NaturalHealInterval;
                            break;

                        case CreatureInput.PercentMassRequiredToReproduce:
                            creatureInputValue = creature.Mass / creature.MassRequiredToReproduce;
                            break;

                        case CreatureInput.VisibleAreaCreatureDensity:
                            visibleCreaturesOrderedByDistanceList ??= visibleCreaturesOrderedByDistance.ToList();
                            visibleArea ??= GetCreatureVisibleArea(creature);

                            if (visibleArea == 0)
                            {
                                creatureInputValue = 0;
                            }
                            else
                            {
                                float creaturesArea = 0;

                                foreach (var visibleCreature in visibleCreaturesOrderedByDistanceList)
                                {
                                    float visibleCreatureArea = CreatureToCachedAreaDict.GetOrAdd(visibleCreature, (x) => GetCircleArea(x.Size / 2));
                                    creaturesArea += visibleCreatureArea;
                                }

                                creatureInputValue = creaturesArea >= visibleArea ? 1 : creaturesArea / visibleArea.Value;
                            }
                            break;

                        case CreatureInput.VisibleAreaFoodDensity:
                            visibleFoodOrderedByDistanceList ??= visibleFoodOrderedByDistance.ToList();
                            visibleArea ??= GetCreatureVisibleArea(creature);

                            if (visibleArea == 0)
                            {
                                creatureInputValue = 0;
                            }
                            else
                            {
                                float foodArea = 0;

                                foreach (var vFood in visibleFoodOrderedByDistanceList)
                                {
                                    float visibleFoodArea = FoodToCachedAreaDict.GetOrAdd(vFood, (x) => GetCircleArea(x.Size / 2));
                                    foodArea += visibleFoodArea;
                                }

                                creatureInputValue = foodArea >= visibleArea ? 1 : foodArea / visibleArea.Value;
                            }
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
