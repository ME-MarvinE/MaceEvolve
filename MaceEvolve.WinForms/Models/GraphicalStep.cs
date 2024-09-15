using MaceEvolve.Core;
using MaceEvolve.Core.Models;
using System.Collections.Generic;
using System.Drawing;

namespace MaceEvolve.WinForms.Models
{
    public class GraphicalStep<TCreature, TFood, TTree> : Step<TCreature, TFood, TTree> where TCreature : GraphicalCreature, new() where TFood : GraphicalFood, new() where TTree : GraphicalTree<TFood>, new()
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
        public override TFood TreeGrowFood(TTree tree)
        {
            TFood food = base.TreeGrowFood(tree);

            if (food == null)
            {
                return null;
            }

            int foodG = (int)Globals.Map(food.Nutrients, 10, 50, 32, 255);

            food.Color = Color.FromArgb(0, foodG, 0);

            return food;
        }
        public override TTree CreateTree()
        {
            TTree newTree = base.CreateTree();

            newTree.Color = Color.FromArgb(50, 30, 170, 0);

            return newTree;
        }
        #endregion
    }
}
