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
        private float _moveEffort = 1f;
        private float _minMoveEffort;
        private float _maxMoveEffort = 2;
        #endregion

        #region Properties
        public NeuralNetwork Brain { get; set; }
        public float MoveCost
        {
            get
            {
                return Size * 0.005f;
            }
        }
        public float AttackCost
        {
            get
            {
                return Size * 0.5f;
            }
        }
        public float DefendCost
        {
            get
            {
                return Size * 0.4f;
            }
        }
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
        public float Speed
        {
            get
            {
                return Size * 0.275f;
            }
        }
        public float SightRange { get; set; } = 100;
        public float Metabolism
        {
            get
            {
                return Size * 0.01f;
            }
        }
        public int FoodEaten { get; set; }
        public int AttemptedAttacksCount { get; set; }
        public int InitiatedAttacksCount { get; set; }
        public int SuccessfulAttacksCount { get; set; }
        public int AttacksEvadedCount { get; set; }
        public int AttemptedEatsCount { get; set; }
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
                _forwardAngle = Globals.ToAngle(value);
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
                _fieldOfView = Globals.ToAngle(value);
            }
        }
        public float MoveEffort
        {
            get
            {
                return _moveEffort;
            }
            set
            {
                if (value < _minMoveEffort)
                {
                    _moveEffort = _minMoveEffort;
                }
                else if (value > _maxMoveEffort)
                {
                    _moveEffort = _maxMoveEffort;
                }
                else
                {
                    _moveEffort = value;
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
