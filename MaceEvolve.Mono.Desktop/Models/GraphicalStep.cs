using MaceEvolve.Core;
using MaceEvolve.Core.Models;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace MaceEvolve.Mono.Desktop.Models
{
    public class GraphicalStep<TCreature, TFood, TPlant> : Step<TCreature, TFood, TPlant> where TCreature : GraphicalCreature, new() where TFood : GraphicalFood, new() where TPlant : GraphicalPlant<TFood>, new()
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

                creatureOffSpring.Color = new Color(offSpringRed, Globals.Map(offSpringRed + offSpringBlue, 0, 375, 100, 50), offSpringBlue);
            }

            return offspring;
        }
        public override TFood PlantGrowFood(TPlant plant)
        {
            TFood food = base.PlantGrowFood(plant);

            if (food == null)
            {
                return null;
            }

            int foodG = (int)Globals.Map(food.Nutrients, 10, 50, 32, 255);

            food.Color = new Color(0, foodG, 0);

            return food;
        }
        public override TPlant CreatePlant()
        {
            TPlant newPlant = base.CreatePlant();

            newPlant.Color = new Color(30, 170, 0, 50);

            return newPlant;
        }
        #endregion
    }
}
