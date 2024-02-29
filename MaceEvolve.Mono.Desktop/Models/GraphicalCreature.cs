using MaceEvolve.Core.Models;
using Microsoft.Xna.Framework;

namespace MaceEvolve.Mono.Desktop.Models
{
    public class GraphicalCreature : Creature
    {
        public Color Color { get; set; }
        public override void Die()
        {
            base.Die();
            Color = new Color(165, 41, 41);
        }
    }
}
