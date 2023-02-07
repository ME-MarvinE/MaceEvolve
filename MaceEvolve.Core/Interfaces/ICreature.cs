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
        void Die();
        bool IsWithinSight(IGameObject gameObject);
        T Reproduce<T>(IList<T> parents, List<CreatureInput> inputs, List<CreatureAction> actions, float nodeBiasMaxVariancePercentage, float connectionWeightMaxVariancePercentage, float connectionWeightBound) where T : ICreature, new();
        #endregion
    }
}
