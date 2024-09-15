using MaceEvolve.Core.Models;
using System.Drawing;
using CoreGlobals = MaceEvolve.Core.Globals;

namespace MaceEvolve.SilkGL.Models
{
    public class GraphicalGameHost<TStep, TCreature, TFood, TTree> : GameHost<TStep, TCreature, TFood, TTree> where TCreature : GraphicalCreature, new() where TFood : GraphicalFood, new() where TStep : GraphicalStep<TCreature, TFood, TTree>, new() where TTree : GraphicalTree<TFood>, new()
    {
        public Color CreatureOffspringColor { get; set; } = Color.Yellow;
        public override TFood CreateFoodWithRandomLocation()
        {
            TFood food = base.CreateFoodWithRandomLocation();
            int foodG = (int)CoreGlobals.Map(food.Nutrients, FoodNutrientsMinMax.Min, FoodNutrientsMinMax.Max, 32, 255);

            food.Color = Color.FromArgb(0, foodG, 0);

            return food;
        }
        public override TTree CreateTreeWithRandomLocation()
        {
            TTree tree = base.CreateTreeWithRandomLocation();
            tree.Color = Color.FromArgb(50, 30, 170, 0);

            return tree;
        }
    }
}
