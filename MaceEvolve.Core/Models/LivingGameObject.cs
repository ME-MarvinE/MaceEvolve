using MaceEvolve.Core.Enums;
using System;

namespace MaceEvolve.Core.Models
{
    public class LivingGameObject : GameObject, ILivingGameObject
    {
        #region Fields
        private float _energy = 100;
        private float _nutrients = 30;
        private float _healthPoints = 90;
        private float _cachedMetabolism;
        private float _cachedMass;
        #endregion

        #region Constructors
        public LivingGameObject()
        {
            Type = GameObjectType.Unknown;
        }
        #endregion

        #region Properties
        public float Energy
        {
            get
            {
                return _energy;
            }
            set
            {
                if (value < 0)
                {
                    _energy = 0;
                }
                else if (value > MaxEnergy)
                {
                    _energy = MaxEnergy;
                }
                else
                {
                    _energy = value;
                }
            }
        }
        public float MaxEnergy { get; set; } = 150;
        public float Metabolism
        {
            get
            {
                if (Mass != _cachedMass)
                {
                    _cachedMass = Mass;
                    _cachedMetabolism = MathF.Pow(Mass, 0.75f) * 0.00075f;
                }

                return _cachedMetabolism;
            }
        }
        public bool IsDead { get; set; }
        public float Nutrients
        {
            get
            {
                return _nutrients;
            }
            set
            {
                if (value < 0)
                {
                    _nutrients = 0;
                }
                else if (value > MaxNutrients)
                {
                    _nutrients = MaxNutrients;
                }
                else
                {
                    _nutrients = value;
                }
            }
        }
        public float MaxNutrients { get; set; } = 30;
        public int MaxAge { get; set; } = 8000;
        public int Age { get; set; }
        public float HealthPoints
        {
            get
            {
                return _healthPoints;
            }
            set
            {
                if (value < 0)
                {
                    _healthPoints = 0;
                }
                else if (value > MaxHealthPoints)
                {
                    _healthPoints = MaxHealthPoints;
                }
                else
                {
                    _healthPoints = value;
                }
            }
        }
        public float MaxHealthPoints { get; set; } = 100;
        public int NaturalHealInterval { get; set; }
        public float NaturalHealHealthPoints { get; set; }
        public int StepsSinceLastNaturalHeal { get; set; }
        #endregion

        #region Methods
        public virtual void Die()
        {
            IsDead = true;
            Energy = 0;
            HealthPoints = 0;
        }
        #endregion
    }
}
