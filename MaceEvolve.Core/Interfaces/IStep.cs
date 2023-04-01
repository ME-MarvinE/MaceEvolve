using MaceEvolve.Core.Enums;
using MaceEvolve.Core.Models;
using System.Collections.Generic;

namespace MaceEvolve.Core.Interfaces
{
    public interface IStep<TCreature, TFood>  where TCreature : ICreature where TFood : IFood
    {
        #region Properties
        List<TCreature> Creatures { get; set; }
        Dictionary<TCreature, List<NeuralNetworkStepNodeInfo>> CreaturesBrainOutput { get; set; }
        List<TFood> Food { get; set; }
        Queue<StepAction<TCreature>> RequestedActions { get; set; }
        IRectangle WorldBounds { get; set; }
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
        float DistanceFromLeftWorldBound(TCreature creature);
        float DistanceFromTopWorldBound(TCreature creature);
        void ExecuteActions(IEnumerable<StepAction<TCreature>> stepActions);
        float GenerateCreatureInputValue(CreatureInput creatureInput, TCreature creature);
        IEnumerable<TCreature> GetVisibleCreatures(TCreature creature);
        IEnumerable<TCreature> GetVisibleCreaturesOrderedByDistance(TCreature creature);
        IEnumerable<TFood> GetVisibleFood(TCreature creature);
        IEnumerable<TFood> GetVisibleFoodOrderedByDistance(TCreature creature);
        float PercentEnergyRequiredToReproduce(TCreature creature);
        float PercentMaxEnergy(TCreature creature);
        float PercentNutrientsRequiredToReproduce(TCreature creature);
        float ProximityToCreatureToBack(TCreature creature);
        float ProximityToCreatureToFront(TCreature creature);
        float ProximityToCreatureToLeft(TCreature creature);
        float ProximityToCreatureToRight(TCreature creature);
        float ProximityToFoodToBack(TCreature creature);
        float ProximityToFoodToFront(TCreature creature);
        float ProximityToFoodToLeft(TCreature creature);
        float ProximityToFoodToRight(TCreature creature);
        void QueueAction(StepAction<TCreature> stepAction);
        void QueueAction(TCreature creature, CreatureAction creatureAction);
        float RandomInput();
        #endregion
    }
}