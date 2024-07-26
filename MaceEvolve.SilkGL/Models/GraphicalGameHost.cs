using MaceEvolve.Core.Models;
using System.Drawing;
using CoreGlobals = MaceEvolve.Core.Globals;

namespace MaceEvolve.SilkGL.Models
{
    public class GraphicalGameHost<TStep, TCreature, TFood> : GameHost<TStep, TCreature, TFood> where TCreature : GraphicalCreature, new() where TFood : GraphicalFood, new() where TStep : GraphicalStep<TCreature, TFood>, new()
    {
        public Color CreatureOffspringColor { get; set; } = Color.Yellow;
        public override TFood CreateFoodWithRandomLocation()
        {
            TFood food = base.CreateFoodWithRandomLocation();
            int foodG = (int)CoreGlobals.Map(food.Nutrients, FoodNutrientsMinMax.Min, FoodNutrientsMinMax.Max, 32, 255);

            food.Color = Color.FromArgb(0, foodG, 0);

            return food;
        }
    }
}
