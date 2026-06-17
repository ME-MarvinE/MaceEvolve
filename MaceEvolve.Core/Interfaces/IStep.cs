using MaceEvolve.Core.Enums;
using MaceEvolve.Core.Models;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace MaceEvolve.Core.Interfaces
{
    public interface IStep<TCreature, TFood, TPlant>  where TCreature : ICreature where TFood : IFood, new() where TPlant : IPlant<TFood>
    {
        #region Properties
        ConcurrentBag<TCreature> Creatures { get; set; }
        ConcurrentBag<TFood> Food { get; set; }
        ConcurrentBag<TPlant> Plants { get; set; }
        Rectangle WorldBounds { get; set; }
        float ConnectionWeightBound { get; set; }
        MinMaxVal<int> CreatureConnectionsMinMax { get; set; }
        MinMaxVal<int> PlantSizeMinMax { get; set; }
        int MaxCreatureProcessNodes { get; set; }
        bool LoopWorldBounds { get; set; }
        ConcurrentDictionary<TCreature, List<TCreature>> VisibleCreaturesDict { get; }
        ConcurrentDictionary<TCreature, List<TFood>> VisibleFoodDict { get; }
        ConcurrentDictionary<TCreature, float> CreatureToCachedAreaDict { get; }
        ConcurrentDictionary<TFood, float> FoodToCachedAreaDict { get; }
        #endregion

        #region Methods
        void CreatureMoveBackwards(TCreature creature);
        void CreatureMoveForwards(TCreature creature);
        void CreatureMoveLeft(TCreature creature);
        void CreatureMoveRight(TCreature creature);
        bool? CreatureTryEat(TCreature creature);
        bool? CreatureTryAttack(TCreature creature);
        void CreatureTurnLeft(TCreature creature);
        void CreatureTurnRight(TCreature creature);
        IList<TCreature> CreatureTryReproduce(TCreature creature);
        void CreatureDoNothing();
        TFood PlantGrowFood(TPlant plant);
        TPlant PlantReproduce(TPlant plant);

        void ExecuteActions(IEnumerable<StepAction<TCreature>> stepActions);
        void UpdatePlants();
        IDictionary<TCreature, IDictionary<CreatureInput, float>> GenerateCreaturesInputValues(IDictionary<TCreature, IEnumerable<CreatureInput>> creatureToCreatureInputsDict);
        int GetNumberOfChildrenThatCanBeCreated(TCreature creature, IEnumerable<TCreature> otherParents = null);
        IList<TCreature> CreateChildren(TCreature mainParent, int? numberOfChildrenToCreate = null, IEnumerable<TCreature> otherParents = null);
        #endregion
    }
}