using MaceEvolve.Core.Models;
using System.Collections.Generic;
using System.Drawing;

namespace MaceEvolve.WinForms.Models
{
    public class GraphicalGameHost<TStep, TCreature, TFood> : GameHost<TStep, TCreature, TFood> where TCreature : GraphicalCreature, new() where TFood : GraphicalFood, new() where TStep : GraphicalStep<TCreature, TFood>, new()
    {
        public Color CreatureOffspringColor { get; set; } = Color.Yellow;
        public override TStep CreateStep(IEnumerable<TCreature> creatures, IEnumerable<TFood> food)
        {
            TStep step = base.CreateStep(creatures, food);

            step.CreatureOffspringColor = CreatureOffspringColor;

            return step;
        }
    }
}
