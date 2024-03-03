using MaceEvolve.Core;
using MaceEvolve.Core.Models;
using System;
using System.Collections.Generic;
using System.Drawing;

namespace MaceEvolve.WinForms.Models
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

            int offSpringRed = (int)Globals.Clamp(CreatureOffspringColor.R * (creature.AttemptedEatsCount == 0 ? 1 : (double)creature.AttemptedAttacksCount / creature.AttemptedEatsCount), 0, 175);
            int offSpringBlue = (int)Globals.Clamp(CreatureOffspringColor.B * (creature.AttemptedAttacksCount == 0 ? 1 : (double)creature.AttemptedEatsCount / creature.AttemptedAttacksCount), 0, 200);

            foreach (var creatureOffSpring in offspring)
            {
                creatureOffSpring.Color = Color.FromArgb(offSpringRed, Globals.Map(offSpringRed + offSpringBlue, 0, 375, 100, 50), offSpringBlue);
            }

            return offspring;
        }
        #endregion
    }
}
