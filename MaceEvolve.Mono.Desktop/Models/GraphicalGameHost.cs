using MaceEvolve.Core.Models;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace MaceEvolve.Mono.Desktop.Models
{
    public class GraphicalGameHost<TStep, TCreature, TFood> : GameHost<TStep, TCreature, TFood> where TCreature : GraphicalCreature, new() where TFood : GraphicalFood, new() where TStep : GraphicalStep<TCreature, TFood>, new()
    {
        public Color CreatureOffspringColor { get; set; } = Color.Yellow;
        public override TStep CreateStep(List<TCreature> creatures, List<TFood> food)
        {
            TStep step = base.CreateStep(creatures, food);

            step.CreatureOffspringColor = CreatureOffspringColor;

            return step;
        }
    }
}
