using MaceEvolve.Core.Enums;
using MaceEvolve.Core.Models;
using System.Collections.Generic;

namespace MaceEvolve.Core.Interfaces
{
    public interface ICreature : IGameObject
    {
        #region Properties
        NeuralNetwork Brain { get; set; }
        double MoveCost { get; set; }
        double Energy { get; set; }
        double MaxEnergy { get; set; }
        double Speed { get; set; }
        int SightRange { get; set; }
        double Metabolism { get; set; }
        int FoodEaten { get; set; }
        bool IsDead { get; set; }
        #endregion

        #region Methods
        void ExecuteAction(CreatureAction creatureAction, CreatureStepInfo stepInfo);
        IEnumerable<T> GetVisibleGameObjects<T>(IEnumerable<T> gameObjects) where T : IGameObject;
        void Die();
        void Live(EnvironmentInfo currentEnvironmentInfo);
        CreatureStepInfo CreateStepInfo(EnvironmentInfo environmentInfo);
        void UpdateInputValues(CreatureStepInfo stepInfo);
        T Reproduce<T>(IList<T> parents, List<CreatureInput> inputs, List<CreatureAction> actions, double nodeBiasMaxVariancePercentage, double connectionWeightMaxVariancePercentage, double connectionWeightBound) where T : ICreature, new();
        #endregion

        #region CreatureValues
        //x values map from 0 to 1.
        double PercentMaxEnergy();
        double HorizontalProximityToCreature(CreatureStepInfo stepInfo);
        double VerticalProximityToCreature(CreatureStepInfo stepInfo);
        double HorizontalProximityToFood(CreatureStepInfo stepInfo);
        double VerticalProximityToFood(CreatureStepInfo stepInfo);
        double DistanceFromTopWorldBound(CreatureStepInfo stepInfo);
        double DistanceFromLeftWorldBound(CreatureStepInfo stepInfo);
        double RandomInput();
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
