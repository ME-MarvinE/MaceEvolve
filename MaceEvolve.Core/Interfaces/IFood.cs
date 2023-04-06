namespace MaceEvolve.Core.Interfaces
{
    public interface IFood : IGameObject
    {
        float Energy { get; set; }
        float MaxEnergy { get; set; }
        float Nutrients { get; set; }
        float MaxNutrients { get; set; }
    }
}
