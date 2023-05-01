using MaceEvolve.Core.Models;

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
        float SightRange { get; set; }
        float Metabolism { get; set; }
        int FoodEaten { get; set; }
        bool IsDead { get; set; }
        public float Nutrients { get; set; }
        public float MaxNutrients { get; set; }
        public float EnergyRequiredToReproduce { get; set; }
        public float NutrientsRequiredToReproduce { get; set; }
        public int TimesReproduced { get; set; }
        public int MaxOffspringPerReproduction { get; set; }
        public int OffspringBrainMutationAttempts { get; set; }
        public float OffspringBrainMutationChance { get; set; }
        public float EnergyPerEat { get; set; }
        public float NutrientsPerEat { get; set; }
        public int Age { get; set; }
        public int MaxAge { get; set; }
        #endregion

        #region Methods
        void Die();
        bool IsWithinSight(IGameObject gameObject);
        #endregion
    }
}
