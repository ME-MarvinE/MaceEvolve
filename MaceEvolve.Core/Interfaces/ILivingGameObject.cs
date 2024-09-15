using MaceEvolve.Core.Interfaces;

namespace MaceEvolve.Core.Models
{
    public interface ILivingGameObject : IGameObject
    {
        int Age { get; set; }
        float Energy { get; set; }
        float HealthPoints { get; set; }
        bool IsDead { get; set; }
        int MaxAge { get; set; }
        float MaxEnergy { get; set; }
        float MaxHealthPoints { get; set; }
        float MaxNutrients { get; set; }
        float Metabolism { get; }
        float NaturalHealHealthPoints { get; set; }
        int NaturalHealInterval { get; set; }
        float Nutrients { get; set; }
        int StepsSinceLastNaturalHeal { get; set; }
        void Die();
    }
}