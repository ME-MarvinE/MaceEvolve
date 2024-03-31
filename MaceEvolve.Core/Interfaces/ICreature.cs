using MaceEvolve.Core.Models;

namespace MaceEvolve.Core.Interfaces
{
    public interface ICreature : IGameObject
    {
        #region Properties
        NeuralNetwork Brain { get; set; }
        float MoveCost { get; set; }
        float AttackCost { get; set; }
        float DefendCost { get; set; }
        float Energy { get; set; }
        float MaxEnergy { get; set; }
        float Speed { get; }
        float SightRange { get; set; }
        float Metabolism { get; }
        int FoodEaten { get; set; }
        int AttemptedAttacksCount { get; set; }
        int InitiatedAttacksCount { get; set; }
        int SuccessfulAttacksCount { get; set; }
        int AttacksEvadedCount { get; set; }
        int AttemptedEatsCount { get; set; }
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
        public float HealthPoints { get; set; }
        public float MaxHealthPoints { get; set; }
        public int NaturalHealInterval { get; set; }
        public float NaturalHealHealthPoints { get; set; }
        public int StepsSinceLastNaturalHeal { get; set; }
        public float MassRequiredToReproduce { get; set; }
        public float ForwardAngle { get; set; }
        public float FieldOfView { get; set; }
        public float MoveEffort { get; set; }
        #endregion

        #region Methods
        void Die();
        #endregion
    }
}
