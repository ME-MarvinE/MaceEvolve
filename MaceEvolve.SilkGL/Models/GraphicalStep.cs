using MaceEvolve.Core.Models;
using System.Drawing;
using System.Collections.Generic;
using System;
using MaceEvolve.Core;

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

            int offSpringRed = (int)Globals.Clamp(CreatureOffspringColor.R * (creature.FoodEaten == 0 ? 1 : (double)creature.SuccessfulAttackCount / creature.FoodEaten), 0, 175);
            int offSpringBlue = (int)Globals.Clamp(CreatureOffspringColor.B * (creature.SuccessfulAttackCount == 0 ? 1 : (double)creature.FoodEaten / creature.SuccessfulAttackCount), 0, 200);

            foreach (var creatureOffSpring in offspring)
            {
                creatureOffSpring.Color = Color.FromArgb(offSpringRed, Globals.Map(offSpringRed + offSpringBlue, 0, 375, 100, 50), offSpringBlue);
            }

            return offspring;
        }
        #endregion
    }
}
