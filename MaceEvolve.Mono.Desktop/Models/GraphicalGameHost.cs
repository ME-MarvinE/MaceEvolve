using MaceEvolve.Core;
using MaceEvolve.Core.Models;
using Microsoft.Xna.Framework;

namespace MaceEvolve.Mono.Desktop.Models
{
    public class GraphicalGameHost<TStep, TCreature, TFood, TPlant> : GameHost<TStep, TCreature, TFood, TPlant> where TCreature : GraphicalCreature, new() where TFood : GraphicalFood, new() where TStep : GraphicalStep<TCreature, TFood, TPlant>, new() where TPlant : GraphicalPlant<TFood>, new()
    {
        public Color CreatureOffspringColor { get; set; } = Color.Yellow;
        public override TFood CreateFoodWithRandomLocation()
        {
            TFood food = base.CreateFoodWithRandomLocation();
            int foodG = (int)Globals.Map(food.Nutrients, FoodNutrientsMinMax.Min, FoodNutrientsMinMax.Max, 32, 255);

            food.Color = new Color(0, foodG, 0);

            return food;
        }
        public override TPlant CreatePlantWithRandomLocation()
        {
            TPlant plant = base.CreatePlantWithRandomLocation();
            plant.Color = new Color(30, 170, 0, 50);

            return plant;
        }
    }
}
