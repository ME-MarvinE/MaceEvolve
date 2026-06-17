using MaceEvolve.Core.Interfaces;
using MaceEvolve.Core.Models;
using System.Drawing;

namespace MaceEvolve.WinForms.Models
{
    public class GraphicalPlant : GraphicalPlant<GraphicalFood>
    {
    }
    public class GraphicalPlant<TFood> : Plant<TFood> where TFood : IFood, new()
    {
        public Color Color { get; set; }
    }
}
