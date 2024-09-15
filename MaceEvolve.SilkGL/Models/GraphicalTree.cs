using MaceEvolve.Core.Interfaces;
using MaceEvolve.Core.Models;
using System.Drawing;

namespace MaceEvolve.SilkGL.Models
{
    public class GraphicalTree : GraphicalTree<GraphicalFood>
    {
    }
    public class GraphicalTree<TFood> : Tree<TFood> where TFood : IFood, new()
    {
        public Color Color { get; set; }
    }
}
