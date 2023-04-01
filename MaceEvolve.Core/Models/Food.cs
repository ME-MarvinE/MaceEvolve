using MaceEvolve.Core.Interfaces;

namespace MaceEvolve.Core.Models
{
    public class Food : GameObject, IFood
    {
        #region Properties
        public int Servings { get; set; } = 1;
        public float EnergyPerServing { get; set; } = 10;
        public float ServingDigestionCost { get; set; } = 0.05f;
        public float NutrientsPerServing { get; set; } = 50;
        #endregion
    }
}
