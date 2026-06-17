using MaceEvolve.Core;
using MaceEvolve.Core.Models;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace MaceEvolve.WinForms.Models
{
    public class GraphicalGameHost<TStep, TCreature, TFood, TPlant> : GameHost<TStep, TCreature, TFood, TPlant> where TCreature : GraphicalCreature, new() where TFood : GraphicalFood, new() where TStep : GraphicalStep<TCreature, TFood, TPlant>, new() where TPlant : GraphicalPlant<TFood>, new()
    {
        public Color CreatureOffspringColor { get; set; } = Color.Yellow;
        public override StepResult<TCreature> NextStep(IEnumerable<StepAction<TCreature>> actionsToExecute, bool gatherBestCreatureInfo, bool gatherSelectedCreatureInfo, bool gatherAliveCreatureInfo, bool gatherDeadCreatureInfo)
        {
            CurrentStep.CreatureOffspringColor = CreatureOffspringColor;
            return base.NextStep(actionsToExecute, gatherBestCreatureInfo, gatherSelectedCreatureInfo, gatherDeadCreatureInfo, gatherAliveCreatureInfo);
        }
        public override TFood CreateFoodWithRandomLocation()
        {
            TFood food = base.CreateFoodWithRandomLocation();
            int foodG = (int)Globals.Map(food.Nutrients, FoodNutrientsMinMax.Min, FoodNutrientsMinMax.Max, 32, 255);

            food.Color = Color.FromArgb(0, foodG, 0);

            return food;
        }
        public override TPlant CreatePlantWithRandomLocation()
        {
            TPlant plant = base.CreatePlantWithRandomLocation();
            plant.Color = Color.FromArgb(50, 30, 170, 0);

            return plant;
        }
        public override List<TCreature> GenerateCreatures()
        {
            return GenerateCreatures(base.GenerateCreatures()).ToList();
        }
        public IEnumerable<TCreature> GenerateCreatures(IEnumerable<TCreature> creaturesToCovert)
        {
            IEnumerable<TCreature> creatures = creaturesToCovert ?? GenerateCreatures();

            foreach (var creature in creatures)
            {
                creature.Color = Color.FromArgb(255, 64, 64, MaceRandom.Current.Next(256));

                if (creature.Genetics == null)
                {
                    byte[] genetics = new byte[CreatureGeneticDepthBytes];
                    MaceRandom.Current.NextBytes(genetics);
                    creature.Genetics = genetics;
                }
            }

            return creatures;
        }
        public override List<TFood> GenerateFood()
        {
            return GenerateFood(base.GenerateFood()).ToList();
        }
        public IEnumerable<TFood> GenerateFood(IEnumerable<TFood> foodToConvert)
        {
            IEnumerable<TFood> foodList = foodToConvert ?? GenerateFood();

            foreach (var food in foodList)
            {
                int foodG = (int)Globals.Map(food.Nutrients, FoodNutrientsMinMax.Min, FoodNutrientsMinMax.Max, 32, 255);

                food.Color = Color.FromArgb(0, foodG, 0);
            }

            return foodList;
        }
        public override List<TPlant> GeneratePlants()
        {
            return GeneratePlants(base.GeneratePlants()).ToList();
        }
        public IEnumerable<TPlant> GeneratePlants(IEnumerable<TPlant> plantsToConvert)
        {
            IEnumerable<TPlant> plantList = plantsToConvert ?? GeneratePlants();

            foreach (var plant in plantList)
            {
                plant.Color = Color.FromArgb(50, 30, 170, 0);
            }

            return plantList;
        }
        public override List<TCreature> CreateNewGenerationCreatures(IEnumerable<TCreature> sourceCreatures, bool sexual = false)
        {
            return GenerateCreatures(base.CreateNewGenerationCreatures(sourceCreatures, sexual)).ToList();
        }
    }
}
