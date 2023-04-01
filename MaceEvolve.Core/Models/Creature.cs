using MaceEvolve.Core.Interfaces;

namespace MaceEvolve.Core.Models
{
    public class Creature : GameObject, ICreature
    {
        #region Fields
        private float _energy = 100;
        #endregion

        #region Properties
        public NeuralNetwork Brain { get; set; }
        public float MoveCost { get; set; } = 0.5f;
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
        public bool IsDead { get; set; }
        public float Nutrients { get; set; } = 30;
        public float EnergyRequiredToReproduce { get; set; } = 50;
        public float NutrientsRequiredToReproduce { get; set; } = 100;
        public int TimesReproduced { get; set; }
        public int MaxOffspringPerReproduction { get; set; }
        public int OffspringBrainMutationAttempts { get; set; } = 1;
        public float OffspringBrainMutationChance { get; set; } = 1 / 3f;

        //public int StomachSize { get; set; } = 5;
        //public List<food> StomachContents { get; set; } = 5;
        //public float DigestionRate = 0.1;
        #endregion

        #region Methods
        public bool IsWithinSight(IGameObject gameObject)
        {
            return Globals.GetDistanceFrom(X, Y, gameObject.X, gameObject.Y) <= SightRange;
        }
        public void Die()
        {
            IsDead = true;
            Energy = 0;
        }
        #endregion
    }
}
