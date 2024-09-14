using MaceEvolve.Core.Enums;
using MaceEvolve.Core.Interfaces;

namespace MaceEvolve.Core.Models
{
    public class Food : GameObject, IFood
    {
        #region Fields
        private float _energy;
        private float _nutrients;
        #endregion

        #region Constructors
        public Food()
        {
            Type = GameObjectType.Food;
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
        public float MaxEnergy { get; set; }
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
        public float MaxNutrients { get; set; }
        public int Servings { get; set; } = 1;
        public float ServingDigestionCost { get; set; } = 0.05f;
        #endregion
    }
}
