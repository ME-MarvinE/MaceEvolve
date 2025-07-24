using MaceEvolve.Core.Enums;
using MaceEvolve.Core.Interfaces;

namespace MaceEvolve.Core.Models
{
    public class Creature : LivingGameObject, ICreature
    {
        #region Fields
        private float _forwardAngle;
        private float _fieldOfView = 112.5f;
        private float _moveEffort = 1f;
        private float _minMoveEffort;
        private float _maxMoveEffort = 2;
        #endregion

        #region Constructors
        public Creature()
        {
            Type = GameObjectType.Creature;
            MaxEnergy = 150;
            MaxNutrients = 30;
            MaxAge = 8000;
            MaxHealthPoints = 100;
            Energy = 100;
            Nutrients = 30;
            HealthPoints = 90;
        }

        #endregion

        #region Properties
        public NeuralNetwork Brain { get; set; }
        public byte[] Genetics { get; set; }
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
        public float Speed
        {
            get
            {
                return Size * 0.275f;
            }
        }
        public float SightRange { get; set; } = 100;
        public int FoodEaten { get; set; }
        public int AttemptedAttacksCount { get; set; }
        public int InitiatedAttacksCount { get; set; }
        public int SuccessfulAttacksCount { get; set; }
        public int AttacksEvadedCount { get; set; }
        public int AttemptedEatsCount { get; set; }
        public float EnergyRequiredToReproduce { get; set; } = 50;
        public float NutrientsRequiredToReproduce { get; set; } = 100;
        public int TimesReproduced { get; set; }
        public int MaxOffspringPerReproduction { get; set; }
        public int OffspringBrainMutationAttempts { get; set; } = 1;
        public float OffspringBrainMutationChance { get; set; } = 1 / 3f;
        public float EnergyPerEat { get; set; }
        public float NutrientsPerEat { get; set; }
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
    }
}
