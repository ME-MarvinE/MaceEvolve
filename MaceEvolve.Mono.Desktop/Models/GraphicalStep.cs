using MaceEvolve.Core;
using MaceEvolve.Core.Models;
using Microsoft.Xna.Framework;
using System;
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

            int offSpringRed = (int)Globals.Clamp(CreatureOffspringColor.R * (creature.FoodEaten == 0 ? 1 : (double)creature.AttackCount / creature.FoodEaten), 0, 175);
            int offSpringBlue = (int)Globals.Clamp(CreatureOffspringColor.B * (creature.AttackCount == 0 ? 1 : (double)creature.FoodEaten / creature.AttackCount), 0, 200);

            foreach (var creatureOffSpring in offspring)
            {

                creatureOffSpring.Color = new Color(offSpringRed, Globals.Map(offSpringRed + offSpringBlue, 0, 375, 100, 50), offSpringBlue);
            }

            return offspring;
        }
        #endregion
    }
}
