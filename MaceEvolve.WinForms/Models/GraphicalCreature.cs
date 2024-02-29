using MaceEvolve.Core.Models;
using System.Drawing;

namespace MaceEvolve.WinForms.Models
{
    public class GraphicalCreature : Creature
    {
        public Color Color { get; set; }
        public override void Die()
        {
            base.Die();
            Color = Color.FromArgb(165, 41, 41);
        }
    }
}
