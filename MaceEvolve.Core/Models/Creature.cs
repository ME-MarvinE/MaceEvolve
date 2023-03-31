using MaceEvolve.Core.Enums;
using MaceEvolve.Core.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace MaceEvolve.Core.Models
{
    public class Creature : GameObject, ICreature
    {
        #region Fields
        private float _energy;
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
        public int SightRange { get; set; } = 200;
        public float Metabolism { get; set; } = 0.1f;
        public int FoodEaten { get; set; }
        public bool IsDead { get; set; }
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
