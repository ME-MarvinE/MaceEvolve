using MaceEvolve.Core.Models;
using System.Collections.Generic;
using System.Drawing;
using CoreGlobals = MaceEvolve.Core.Globals;

namespace MaceEvolve.SilkGL.Models
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

            int offSpringRed = (int)CoreGlobals.Clamp(CreatureOffspringColor.R * (creature.AttemptedEatsCount == 0 ? 1 : (double)creature.AttemptedAttacksCount / creature.AttemptedEatsCount), 0, 175);
            int offSpringBlue = (int)CoreGlobals.Clamp(CreatureOffspringColor.B * (creature.AttemptedAttacksCount == 0 ? 1 : (double)creature.AttemptedEatsCount / creature.AttemptedAttacksCount), 0, 200);

            foreach (var creatureOffSpring in offspring)
            {
                creatureOffSpring.Color = Color.FromArgb(offSpringRed, CoreGlobals.Map(offSpringRed + offSpringBlue, 0, 375, 100, 50), offSpringBlue);
            }

            return offspring;
        }
        #endregion
    }
}
