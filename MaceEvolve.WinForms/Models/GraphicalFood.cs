using MaceEvolve.Core.Models;
using MaceEvolve.WinForms.Interfaces;
using System.Drawing;

namespace MaceEvolve.WinForms.Models
{
    public class GraphicalFood : Food, IGraphical
    {
        public Color Color { get; set; }
    }
}
