using MaceEvolve.Core.Enums;
using MaceEvolve.Core.Models;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace MaceEvolve.Core.Interfaces
{
    public interface IStep<TCreature, TFood>  where TCreature : ICreature where TFood : IFood
    {
        #region Properties
        ConcurrentBag<TCreature> Creatures { get; set; }
        ConcurrentDictionary<TCreature, List<NeuralNetworkStepNodeInfo>> CreaturesBrainOutput { get; set; }
        ConcurrentBag<TFood> Food { get; set; }
        ConcurrentQueue<StepAction<TCreature>> RequestedActions { get; set; }
        Rectangle WorldBounds { get; set; }
        float ConnectionWeightBound { get; set; }
        int MaxCreatureConnections { get; set; }
        int MaxCreatureProcessNodes { get; set; }
        int MinCreatureConnections { get; set; }
        bool LoopWorldBounds { get; set; }
        #endregion

        #region Methods
        void CreatureMoveBackwards(TCreature creature);
        void CreatureMoveForwards(TCreature creature);
        void CreatureMoveLeft(TCreature creature);
        void CreatureMoveRight(TCreature creature);
        void CreatureMoveTowardsClosestFood(TCreature creature);
        bool CreatureTryEat(TCreature creature);
        IList<TCreature> CreatureTryReproduce(TCreature creature);
        void ExecuteActions(IEnumerable<StepAction<TCreature>> stepActions);
        Dictionary<CreatureInput, float> GenerateCreatureInputValues(IEnumerable<CreatureInput> creatureInput, TCreature creature);
        IEnumerable<TCreature> GetVisibleCreatures(TCreature creature);
        IEnumerable<TCreature> GetVisibleCreaturesOrderedByDistance(TCreature creature, IEnumerable<TCreature> visibleCreatures);
        IEnumerable<TFood> GetVisibleFood(TCreature creature);
        IEnumerable<TFood> GetVisibleFoodOrderedByDistance(TCreature creature, IEnumerable<TFood> visibleFood);
        void QueueAction(StepAction<TCreature> stepAction);
        void QueueAction(TCreature creature, CreatureAction creatureAction);
        #endregion
    }
}