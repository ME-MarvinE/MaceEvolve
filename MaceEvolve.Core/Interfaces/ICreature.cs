using MaceEvolve.Core.Models;

namespace MaceEvolve.Core.Interfaces
{
    public interface ICreature : ILivingGameObject
    {
        #region Properties
        NeuralNetwork Brain { get; set; }
        byte[] Genetics { get; set; }
        float MoveCost { get; }
        float AttackCost { get; }
        float DefendCost { get; }
        float Speed { get; }
        float SightRange { get; set; }
        int FoodEaten { get; set; }
        int AttemptedAttacksCount { get; set; }
        int InitiatedAttacksCount { get; set; }
        int SuccessfulAttacksCount { get; set; }
        int AttacksEvadedCount { get; set; }
        int AttemptedEatsCount { get; set; }
        public float EnergyRequiredToReproduce { get; set; }
        public float NutrientsRequiredToReproduce { get; set; }
        public int TimesReproduced { get; set; }
        public int MaxOffspringPerReproduction { get; set; }
        public int OffspringBrainMutationAttempts { get; set; }
        public float OffspringBrainMutationChance { get; set; }
        public float EnergyPerEat { get; set; }
        public float NutrientsPerEat { get; set; }
        public float MassRequiredToReproduce { get; set; }
        public float ForwardAngle { get; set; }
        public float FieldOfView { get; set; }
        public float MoveEffort { get; set; }
        #endregion
    }
}
