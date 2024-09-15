using MaceEvolve.Core;
using MaceEvolve.Core.Interfaces;
using MaceEvolve.Core.Models;
using Microsoft.Xna.Framework;
using System.Collections.Generic;

namespace MaceEvolve.Mono.Desktop.Models
{
    public class GraphicalGameHost<TStep, TCreature, TFood, TTree> : GameHost<TStep, TCreature, TFood, TTree> where TCreature : GraphicalCreature, new() where TFood : GraphicalFood, new() where TStep : GraphicalStep<TCreature, TFood, TTree>, new() where TTree : GraphicalTree<TFood>, new()
    {
        public Color CreatureOffspringColor { get; set; } = Color.Yellow;
        public override TFood CreateFoodWithRandomLocation()
        {
            TFood food = base.CreateFoodWithRandomLocation();
            int foodG = (int)Globals.Map(food.Nutrients, FoodNutrientsMinMax.Min, FoodNutrientsMinMax.Max, 32, 255);

            food.Color = new Color(0, foodG, 0);

            return food;
        }
        public override TTree CreateTreeWithRandomLocation()
        {
            TTree tree = base.CreateTreeWithRandomLocation();
            tree.Color = new Color(30, 170, 0, 50);

            return tree;
        }
    }
}
