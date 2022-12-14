using MaceEvolve.Core.Enums;
using MaceEvolve.Core.Models;
using System.Collections.Generic;

namespace MaceEvolve.Core.Interfaces
{
    public interface ICreature : IGameObject
    {
        #region Properties
        NeuralNetwork Brain { get; set; }
        float MoveCost { get; set; }
        float Energy { get; set; }
        float MaxEnergy { get; set; }
        float Speed { get; set; }
        int SightRange { get; set; }
        float Metabolism { get; set; }
        int FoodEaten { get; set; }
        bool IsDead { get; set; }
        #endregion

        #region Methods
        void ExecuteAction(CreatureAction creatureAction, CreatureStepInfo stepInfo);
        void Die();
        void Live(EnvironmentInfo currentEnvironmentInfo);
        CreatureStepInfo CreateStepInfo(EnvironmentInfo environmentInfo);
        void UpdateInputValues(CreatureStepInfo stepInfo);
        T Reproduce<T>(IList<T> parents, List<CreatureInput> inputs, List<CreatureAction> actions, float nodeBiasMaxVariancePercentage, float connectionWeightMaxVariancePercentage, float connectionWeightBound) where T : ICreature, new();
        #endregion

        #region CreatureValues
        //x values map from 0 to 1.
        float PercentMaxEnergy();
        float ProximityToCreatureToLeft(CreatureStepInfo stepInfo);
        float ProximityToCreatureToRight(CreatureStepInfo stepInfo);
        float ProximityToCreatureToFront(CreatureStepInfo stepInfo);
        float ProximityToCreatureToBack(CreatureStepInfo stepInfo);
        float ProximityToFoodToLeft(CreatureStepInfo stepInfo);
        float ProximityToFoodToRight(CreatureStepInfo stepInfo);
        float ProximityToFoodToFront(CreatureStepInfo stepInfo);
        float ProximityToFoodToBack(CreatureStepInfo stepInfo);
        float DistanceFromTopWorldBound(CreatureStepInfo stepInfo);
        float DistanceFromLeftWorldBound(CreatureStepInfo stepInfo);
        float RandomInput();
        #endregion

        #region Actions
        bool TryEatFoodInRange(CreatureStepInfo stepInfo);
        void MoveForward(CreatureStepInfo stepInfo);
        void MoveBackward(CreatureStepInfo stepInfo);
        void MoveLeft(CreatureStepInfo stepInfo);
        void MoveRight(CreatureStepInfo stepInfo);
        void MoveTowardsClosestFood(CreatureStepInfo stepInfo);
        #endregion
    }
}
