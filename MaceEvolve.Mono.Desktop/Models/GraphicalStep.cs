using MaceEvolve.Core.Models;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace MaceEvolve.Mono.Desktop.Models
{
    public class GraphicalStep<TCreature, TFood> : Step<TCreature, TFood> where TCreature : GraphicalCreature, new() where TFood : GraphicalFood
    {
        #region Properties
        public Color CreatureOffspringColor { get; set; }
        #endregion

        #region Methods
        public override IList<TCreature> CreatureTryReproduce(TCreature creature)
        {
            IList<TCreature> offspring = base.CreatureTryReproduce(creature);

            foreach (var creatureOffSpring in offspring)
            {
                creatureOffSpring.Color = CreatureOffspringColor;
            }

            return offspring;
        }
        #endregion
    }
}
