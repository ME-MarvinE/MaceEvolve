using MaceEvolve.Core.Enums;
using MaceEvolve.Core.Models;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace MaceEvolve.Core.Interfaces
{
    public interface IStep<TCreature, TFood, TTree>  where TCreature : ICreature where TFood : IFood, new() where TTree : ITree<TFood>
    {
        #region Properties
        ConcurrentBag<TCreature> Creatures { get; set; }
        ConcurrentBag<TFood> Food { get; set; }
        ConcurrentBag<TTree> Trees { get; set; }
        Rectangle WorldBounds { get; set; }
        float ConnectionWeightBound { get; set; }
        MinMaxVal<int> CreatureConnectionsMinMax { get; set; }
        MinMaxVal<int> TreeSizeMinMax { get; set; }
        int MaxTreeAmount { get; set; }
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
        TFood TreeGrowFood(TTree tree);
        TTree TreeReproduce(TTree tree);

        void ExecuteActions(IEnumerable<StepAction<TCreature>> stepActions);
        void UpdateTrees(int maxTreeAmount, int maxFoodAmount);
        IDictionary<TCreature, IDictionary<CreatureInput, float>> GenerateCreaturesInputValues(IDictionary<TCreature, IEnumerable<CreatureInput>> creatureToCreatureInputsDict);
        #endregion
    }
}