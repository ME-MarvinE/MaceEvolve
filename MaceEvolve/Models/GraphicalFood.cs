using MaceEvolve.Core.Models;
using MaceEvolve.Interfaces;
using System.Drawing;

namespace MaceEvolve.Models
{
    public class GraphicalFood : Food, IGraphical
    {
        public Color Color { get; set; }
    }
}
