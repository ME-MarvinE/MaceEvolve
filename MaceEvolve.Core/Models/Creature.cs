using MaceEvolve.Core.Interfaces;
using System;

namespace MaceEvolve.Core.Models
{
    public class Creature : GameObject, ICreature
    {
        #region Fields
        private float _energy = 100;
        private float _nutrients = 30;
        private float _healthPoints = 90;
        private float _forwardAngle;
        private float _fieldOfView = 112.5f;
        #endregion

        #region Properties
        public NeuralNetwork Brain { get; set; }
        public float MoveCost { get; set; } = 0.25f;
        public float AttackCost { get; set; } = 0.25f;
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
        public float Speed { get; set; } = 1;
        public float SightRange { get; set; } = 200;
        public float Metabolism { get; set; } = 0.1f;
        public int FoodEaten { get; set; }
        public int AttackCount { get; set; }
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
        public float EnergyRequiredToReproduce { get; set; } = 50;
        public float NutrientsRequiredToReproduce { get; set; } = 100;
        public int TimesReproduced { get; set; }
        public int MaxOffspringPerReproduction { get; set; }
        public int OffspringBrainMutationAttempts { get; set; } = 1;
        public float OffspringBrainMutationChance { get; set; } = 1 / 3f;
        public float EnergyPerEat { get; set; }
        public float NutrientsPerEat { get; set; }
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
        public float MassRequiredToReproduce { get; set; }
        public float ForwardAngle
        {
            get
            {
                return _forwardAngle;
            }
            set
            {
                if (value < 0)
                {
                    _forwardAngle = 360 + value % 360;
                }
                else if (value > 359)
                {
                    _forwardAngle = value % 360;
                }
                else
                {
                    _forwardAngle = value;
                }
            }
        }
        public float FieldOfView
        {
            get
            {
                return _fieldOfView;
            }
            set
            {
                if (value < 0)
                {
                    _fieldOfView = Math.Abs(value) % 360;
                }
                else if (value > 359)
                {
                    _fieldOfView = value % 360;
                }
                else
                {
                    _fieldOfView = value;
                }
            }
        }

        //public int StomachSize { get; set; } = 5;
        //public List<food> StomachContents { get; set; } = 5;
        //public float DigestionRate = 0.1;
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
