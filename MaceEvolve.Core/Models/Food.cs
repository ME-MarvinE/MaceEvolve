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
        public float Energy { get; set; }
        public float MaxEnergy { get; set; }
        public float Nutrients { get; set; }
        public float MaxNutrients { get; set; }
        public int Servings { get; set; } = 1;
        public float ServingDigestionCost { get; set; } = 0.05f;
        #endregion
    }
}
