using MaceEvolve.Core.Enums;
using MaceEvolve.Core.Extensions;
using MaceEvolve.Core.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace MaceEvolve.Core.Models
{
    public class Step<TCreature, TFood> where TCreature : ICreature where TFood : IFood
    {
        #region Properties
        public Queue<StepAction<TCreature>> RequestedActions = new Queue<StepAction<TCreature>>();
        public List<TCreature> Creatures { get; }
        public List<TFood> Food { get; }
        public IRectangle WorldBounds { get; }
        private Dictionary<TCreature, CreatureStepInfo<TCreature, TFood>> CreatureStepInfos { get; } = new Dictionary<TCreature, CreatureStepInfo<TCreature, TFood>>();
        #endregion

        #region Constructors
        public Step(List<TCreature> creatures, List<TFood> food, IRectangle worldBounds)
        {
            Creatures = creatures;
            Food = food;
            WorldBounds = worldBounds;

            foreach (var creature in Creatures)
            {
                CreatureStepInfos.Add(creature, new CreatureStepInfo<TCreature, TFood>());
            }
        }
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
                throw new ArgumentException($"The {nameof(Creature)} is not present in the step.", nameof(creature));
            }

            stepInfo.VisibleCreatures ??= Creatures.Where(x => creature.IsWithinSight(x) && (ICreature)x != (ICreature)creature).ToList();

            return stepInfo.VisibleCreatures;
        }
        public IEnumerable<TFood> GetVisibleFood(TCreature creature)
        {
            if (!CreatureStepInfos.TryGetValue(creature, out CreatureStepInfo<TCreature, TFood> stepInfo))
            {
                throw new ArgumentException($"The {nameof(Creature)} is not present in the step.", nameof(creature));
            }

            stepInfo.VisibleFood ??= Food.Where(x => creature.IsWithinSight(x)).ToList();

            return stepInfo.VisibleFood;
        }
        public IEnumerable<TCreature> GetVisibleCreaturesOrderedByDistance(TCreature creature)
        {
            if (!CreatureStepInfos.TryGetValue(creature, out CreatureStepInfo<TCreature, TFood> stepInfo))
            {
                throw new ArgumentException($"The {nameof(Creature)} is not present in the step.", nameof(creature));
            }

            stepInfo.VisibleCreaturesOrderedByDistance ??= GetVisibleCreatures(creature).OrderBy(x => Globals.GetDistanceFrom(creature.X, creature.Y, x.X, x.Y)).ToList();

            return stepInfo.VisibleCreaturesOrderedByDistance;
        }
        public IEnumerable<TFood> GetVisibleFoodOrderedByDistance(TCreature creature)
        {
            if (!CreatureStepInfos.TryGetValue(creature, out CreatureStepInfo<TCreature, TFood> stepInfo))
            {
                throw new ArgumentException($"The {nameof(Creature)} is not present in the step.", nameof(creature));
            }

            stepInfo.VisibleFoodOrderedByDistance ??= GetVisibleFood(creature).OrderBy(x => Globals.GetDistanceFrom(creature.X, creature.Y, x.X, x.Y)).ToList();

            return stepInfo.VisibleFoodOrderedByDistance;
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
        public void UpdateCreatureInputValues(TCreature creature)
        {
            creature.Brain.UpdateInputValue(CreatureInput.PercentMaxEnergy, PercentMaxEnergy(creature));
            creature.Brain.UpdateInputValue(CreatureInput.ProximityToCreatureToLeft, ProximityToCreatureToLeft(creature));
            creature.Brain.UpdateInputValue(CreatureInput.ProximityToCreatureToRight, ProximityToCreatureToRight(creature));
            creature.Brain.UpdateInputValue(CreatureInput.ProximityToCreatureToFront, ProximityToCreatureToFront(creature));
            creature.Brain.UpdateInputValue(CreatureInput.ProximityToCreatureToBack, ProximityToCreatureToBack(creature));
            creature.Brain.UpdateInputValue(CreatureInput.ProximityToFoodToLeft, ProximityToFoodToLeft(creature));
            creature.Brain.UpdateInputValue(CreatureInput.ProximityToFoodToRight, ProximityToFoodToRight(creature));
            creature.Brain.UpdateInputValue(CreatureInput.ProximityToFoodToFront, ProximityToFoodToFront(creature));
            creature.Brain.UpdateInputValue(CreatureInput.ProximityToFoodToBack, ProximityToFoodToBack(creature));
            creature.Brain.UpdateInputValue(CreatureInput.DistanceFromTopWorldBound, DistanceFromTopWorldBound(creature));
            creature.Brain.UpdateInputValue(CreatureInput.DistanceFromLeftWorldBound, DistanceFromLeftWorldBound(creature));
            creature.Brain.UpdateInputValue(CreatureInput.RandomInput, RandomInput());
        }
        public static void ExecuteActions<T1, T2>(IEnumerable<StepAction<T1>> stepActions, Step<T1, T2> step) where T1 : ICreature where T2 : IFood
        {
            foreach (var stepAction in stepActions)
            {
                if (!stepAction.Creature.IsDead)
                {
                    switch (stepAction.Action)
                    {
                        case CreatureAction.MoveForward:
                            step.CreatureMoveForwards(stepAction.Creature);
                            break;

                        case CreatureAction.MoveBackward:
                            step.CreatureMoveBackwards(stepAction.Creature);

                            break;

                        case CreatureAction.MoveLeft:
                            step.CreatureMoveLeft(stepAction.Creature);
                            break;

                        case CreatureAction.MoveRight:
                            step.CreatureMoveRight(stepAction.Creature);
                            break;

                        case CreatureAction.TryEat:
                            step.CreatureTryEat(stepAction.Creature);
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
        #endregion
    }
}
