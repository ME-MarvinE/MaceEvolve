namespace MaceEvolve.Core.Interfaces
{
    public interface IFood : IGameObject
    {
        int Servings { get; set; }
        float EnergyPerServing { get; set; }
        float ServingDigestionCost { get; set; }
        float NutrientsPerServing { get; set; }
    }
}
