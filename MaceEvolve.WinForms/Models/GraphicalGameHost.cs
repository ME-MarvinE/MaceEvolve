using MaceEvolve.Core;
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
        public override void NextStep(bool gatherInfoForAllCreatures = false)
        {
            CurrentStep.CreatureOffspringColor = CreatureOffspringColor;
            base.NextStep(gatherInfoForAllCreatures);
        }
        public override TFood CreateFoodWithRandomLocation()
        {
            TFood food = base.CreateFoodWithRandomLocation();
            int foodG = (int)Globals.Map(food.Nutrients, 0, MaxFoodNutrients, 32, 255);

            food.Color = Color.FromArgb(0, foodG, 0);

            return food;
        }
    }
}
