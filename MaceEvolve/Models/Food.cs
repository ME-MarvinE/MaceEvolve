namespace MaceEvolve.Models
{
    public abstract class Food : GameObject
    {
        #region Properties
        public int Servings { get; set; } = 1;
        public int EnergyPerServing { get; set; } = 10;
        public double ServingDigestionCost { get; set; } = 0.05;
        #endregion
    }
}
