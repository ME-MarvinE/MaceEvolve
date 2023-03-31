using MaceEvolve.Core.Enums;
using MaceEvolve.Core.Extensions;
using MaceEvolve.Core.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace MaceEvolve.Core.Models
{
    public class Step<TCreature, TFood> : IStep<TCreature, TFood> where TCreature : ICreature where TFood : IFood
    {
        #region Properties
        public Queue<StepAction<TCreature>> RequestedActions { get; set; } = new Queue<StepAction<TCreature>>();
        public Dictionary<TCreature, List<NeuralNetworkStepNodeInfo>> CreaturesBrainOutput { get; set; } = new Dictionary<TCreature, List<NeuralNetworkStepNodeInfo>>();
        public List<TCreature> Creatures { get; set; }
        public List<TFood> Food { get; set; }
        public IRectangle WorldBounds { get; set; }
        private Dictionary<TCreature, CreatureStepInfo<TCreature, TFood>> CreatureStepInfos { get; } = new Dictionary<TCreature, CreatureStepInfo<TCreature, TFood>>();
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
            if (!CreatureStepInfos.TryGetValue(creature, out CreatureStepInfo<TCreature, TFood> stepInfo))
            {
                CreatureStepInfos.Add(creature, new CreatureStepInfo<TCreature, TFood>());
            }

            CreatureStepInfos[creature].VisibleCreatures ??= Creatures.Where(x => creature.IsWithinSight(x) && (ICreature)x != (ICreature)creature).ToList();

            return CreatureStepInfos[creature].VisibleCreatures;
        }
        public IEnumerable<TFood> GetVisibleFood(TCreature creature)
        {
            if (!CreatureStepInfos.TryGetValue(creature, out CreatureStepInfo<TCreature, TFood> stepInfo))
            {
                CreatureStepInfos.Add(creature, new CreatureStepInfo<TCreature, TFood>());
            }

            CreatureStepInfos[creature].VisibleFood ??= Food.Where(x => creature.IsWithinSight(x)).ToList();

            return CreatureStepInfos[creature].VisibleFood;
        }
        public IEnumerable<TCreature> GetVisibleCreaturesOrderedByDistance(TCreature creature)
        {
            if (!CreatureStepInfos.TryGetValue(creature, out CreatureStepInfo<TCreature, TFood> stepInfo))
            {
                CreatureStepInfos.Add(creature, new CreatureStepInfo<TCreature, TFood>());
            }

            CreatureStepInfos[creature].VisibleCreaturesOrderedByDistance ??= GetVisibleCreatures(creature).OrderBy(x => Globals.GetDistanceFrom(creature.X, creature.Y, x.X, x.Y)).ToList();

            return CreatureStepInfos[creature].VisibleCreaturesOrderedByDistance;
        }
        public IEnumerable<TFood> GetVisibleFoodOrderedByDistance(TCreature creature)
        {
            if (!CreatureStepInfos.TryGetValue(creature, out CreatureStepInfo<TCreature, TFood> stepInfo))
            {
                CreatureStepInfos.Add(creature, new CreatureStepInfo<TCreature, TFood>());
            }

            CreatureStepInfos[creature].VisibleFoodOrderedByDistance ??= GetVisibleFood(creature).OrderBy(x => Globals.GetDistanceFrom(creature.X, creature.Y, x.X, x.Y)).ToList();

            return CreatureStepInfos[creature].VisibleFoodOrderedByDistance;
        }
        public float PercentMaxEnergy(TCreature creature)
        {
            return Globals.Map(creature.Energy, 0, creature.MaxEnergy, 0, 1);
        }
        public float ProximityToCreatureToLeft(TCreature creature)
        {
            ICreature closestCreature = GetVisibleCreaturesOrderedByDistance(creature).FirstOrDefault(x => x.MX <= creature.MX);

            if (closestCreature == null)
            {
                return 0;
            }

            float distanceFromClosestCreatureToLeft = Globals.GetDistanceFrom(creature.MX, creature.MY, closestCreature.MX, creature.MY);

            return Globals.Map(distanceFromClosestCreatureToLeft, 0, creature.SightRange, 1, 0);
        }
        public float ProximityToCreatureToRight(TCreature creature)
        {
            ICreature closestCreature = GetVisibleCreaturesOrderedByDistance(creature).FirstOrDefault(x => x.MX >= creature.MX);

            if (closestCreature == null)
            {
                return 0;
            }

            float distanceFromClosestCreatureToRight = Globals.GetDistanceFrom(creature.MX, creature.MY, closestCreature.MX, creature.MY);

            return Globals.Map(distanceFromClosestCreatureToRight, 0, creature.SightRange, 1, 0);
        }
        public float ProximityToCreatureToFront(TCreature creature)
        {
            ICreature closestCreature = GetVisibleCreaturesOrderedByDistance(creature).FirstOrDefault(x => x.MY <= creature.MY);

            if (closestCreature == null)
            {
                return 0;
            }

            float distanceFromClosestCreatureToFront = Globals.GetDistanceFrom(creature.MX, creature.MY, creature.MX, closestCreature.MY);

            return Globals.Map(distanceFromClosestCreatureToFront, 0, creature.SightRange, 1, 0);
        }
        public float ProximityToCreatureToBack(TCreature creature)
        {
            ICreature closestCreature = GetVisibleCreaturesOrderedByDistance(creature).FirstOrDefault(x => x.MY >= creature.MY);

            if (closestCreature == null)
            {
                return 0;
            }

            float distanceFromClosestCreatureToBack = Globals.GetDistanceFrom(creature.MX, creature.MY, creature.MX, closestCreature.MY);

            return Globals.Map(distanceFromClosestCreatureToBack, 0, creature.SightRange, 1, 0);
        }
        public float ProximityToFoodToLeft(TCreature creature)
        {
            IFood closestFood = GetVisibleFoodOrderedByDistance(creature).FirstOrDefault(x => x.MX <= creature.MX);

            if (closestFood == null)
            {
                return 0;
            }

            float distanceFromClosestFoodToLeft = Globals.GetDistanceFrom(creature.MX, creature.MY, closestFood.MX, creature.MY);

            return Globals.Map(distanceFromClosestFoodToLeft, 0, creature.SightRange, 1, 0);
        }
        public float ProximityToFoodToRight(TCreature creature)
        {
            IFood closestFood = GetVisibleFoodOrderedByDistance(creature).FirstOrDefault(x => x.MX >= creature.MX);

            if (closestFood == null)
            {
                return 0;
            }

            float distanceFromClosestFoodToRight = Globals.GetDistanceFrom(creature.MX, creature.MY, closestFood.MX, creature.MY);

            return Globals.Map(distanceFromClosestFoodToRight, 0, creature.SightRange, 1, 0);
        }
        public float ProximityToFoodToFront(TCreature creature)
        {
            IFood closestFood = GetVisibleFoodOrderedByDistance(creature).FirstOrDefault(x => x.MY <= creature.MY);

            if (closestFood == null)
            {
                return 0;
            }

            float distanceFromClosestFoodToFront = Globals.GetDistanceFrom(creature.MX, creature.MY, creature.MX, closestFood.MY);

            return Globals.Map(distanceFromClosestFoodToFront, 0, creature.SightRange, 1, 0);
        }
        public float ProximityToFoodToBack(TCreature creature)
        {
            IFood closestFood = GetVisibleFoodOrderedByDistance(creature).FirstOrDefault(x => x.MY >= creature.MY);

            if (closestFood == null)
            {
                return 0;
            }

            float distanceFromClosestFoodToBack = Globals.GetDistanceFrom(creature.MX, creature.MY, creature.MX, closestFood.MY);

            return Globals.Map(distanceFromClosestFoodToBack, 0, creature.SightRange, 1, 0);
        }
        public float DistanceFromTopWorldBound(TCreature creature)
        {
            return Globals.Map(creature.Y, WorldBounds.Y, WorldBounds.Y + WorldBounds.Height, 0, 1);
        }
        public float DistanceFromLeftWorldBound(TCreature creature)
        {
            return Globals.Map(creature.X, WorldBounds.X, WorldBounds.X + WorldBounds.Width, 0, 1);
        }
        public float RandomInput()
        {
            return Globals.Random.NextFloat();
        }

        public bool CreatureTryEat(TCreature creature)
        {
            IEnumerable<TFood> VisibleFoodOrderedByDistance = GetVisibleFoodOrderedByDistance(creature);

            IFood closestFood = VisibleFoodOrderedByDistance.FirstOrDefault();

            if (closestFood != null && closestFood.Servings > 0 && Globals.GetDistanceFrom(creature.MX, creature.MY, closestFood.MX, closestFood.MY) < creature.Size / 2)
            {
                creature.Energy -= closestFood.ServingDigestionCost;
                closestFood.Servings -= 1;
                creature.FoodEaten += 1;
                creature.Energy += closestFood.EnergyPerServing;

                return true;
            }
            else
            {
                return false;
            }
        }
        public void CreatureMoveForwards(TCreature creature)
        {
            creature.Y -= creature.Speed;
            if (creature.MY < WorldBounds.Y)
            {
                creature.Y += creature.Speed;
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
                creature.Y -= creature.Speed;
                //Y -= WorldBounds.WorldBounds.Height;
            }
            creature.Energy -= creature.MoveCost;
        }
        public void CreatureMoveLeft(TCreature creature)
        {
            creature.X -= creature.Speed;
            if (creature.MX < WorldBounds.X)
            {
                creature.X += creature.Speed;
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
                creature.X -= creature.Speed;
                //X -= WorldBounds.WorldBounds.Width;
            }
            creature.Energy -= creature.MoveCost;
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
        public float GenerateCreatureInputValue(CreatureInput creatureInput, TCreature creature)
        {
            switch (creatureInput)
            {
                case CreatureInput.PercentMaxEnergy:
                    return PercentMaxEnergy(creature);

                case CreatureInput.ProximityToCreatureToLeft:
                    return ProximityToCreatureToLeft(creature);

                case CreatureInput.ProximityToCreatureToRight:
                    return ProximityToCreatureToRight(creature);

                case CreatureInput.ProximityToCreatureToFront:
                    return ProximityToCreatureToFront(creature);

                case CreatureInput.ProximityToCreatureToBack:
                    return ProximityToCreatureToBack(creature);

                case CreatureInput.ProximityToFoodToLeft:
                    return ProximityToFoodToLeft(creature);

                case CreatureInput.ProximityToFoodToRight:
                    return ProximityToFoodToRight(creature);

                case CreatureInput.ProximityToFoodToFront:
                    return ProximityToFoodToFront(creature);

                case CreatureInput.ProximityToFoodToBack:
                    return ProximityToFoodToBack(creature);

                case CreatureInput.DistanceFromTopWorldBound:
                    return DistanceFromTopWorldBound(creature);

                case CreatureInput.DistanceFromLeftWorldBound:
                    return DistanceFromLeftWorldBound(creature);

                case CreatureInput.RandomInput:
                    return RandomInput();

                default:
                    throw new NotImplementedException($"{nameof(CreatureInput)} '{creatureInput}' has not been implemented.");
            }
        }
        #endregion
    }
}
