using MaceEvolve.Core.Models;

namespace MaceEvolve.Core.Interfaces
{
    public interface IGameObject
    {
        double X { get; set; }
        double Y { get; set; }
        double MX { get; }
        double MY { get; }
        double Size { get; set; }
        Rectangle Rectangle { get; set; }
    }
}
