namespace MaceEvolve.Core.Interfaces
{
    public interface IGameObject
    {
        float X { get; set; }
        float Y { get; set; }
        float MX { get; }
        float MY { get; }
        float Size { get; set; }
    }
}
