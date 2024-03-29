﻿using MaceEvolve.Core.Enums;
using MaceEvolve.Core.Models;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace MaceEvolve.Core.Interfaces
{
    public interface IStep<TCreature, TFood>  where TCreature : ICreature where TFood : IFood
    {
        #region Properties
        ConcurrentBag<TCreature> Creatures { get; set; }
        ConcurrentBag<TFood> Food { get; set; }
        Rectangle WorldBounds { get; set; }
        float ConnectionWeightBound { get; set; }
        MinMaxVal<int> CreatureConnectionsMinMax { get; set; }
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
        void ExecuteActions(IEnumerable<StepAction<TCreature>> stepActions);
        Dictionary<CreatureInput, float> GenerateCreatureInputValues(IEnumerable<CreatureInput> creatureInput, TCreature creature);
        #endregion
    }
}