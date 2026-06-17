using MaceEvolve.Core.Enums;
using MaceEvolve.Core.Models;
using System.Collections.Concurrent;

namespace MaceEvolve.Core.Interfaces
{
    public interface IPlant<TFood> : ILivingGameObject where TFood : IFood, new()
    {
        float AgeRequiredToReproduce { get; set; }
        float EnergyRequiredToReproduce { get; set; }
        float MassRequiredToReproduce { get; set; }
        float NutrientsRequiredToReproduce { get; set; }
        float AgeRequiredToCreateFood { get; set; }
        float ChanceToGrowFood { get; set; }
        float ChanceToDropFood { get; set; }
        float ChanceToReproduce { get; set; }
        float EnergyPerEat { get; set; }
        MinMaxVal<float> FoodEnergyMinMax { get; set; }
        ConcurrentDictionary<int, int> FoodIdToAgeDict { get; }
        MinMaxVal<float> FoodMassMinMax { get; set; }
        MinMaxVal<float> FoodNutrientsMinMax { get; set; }
        MinMaxVal<float> FoodSizeMinMax { get; set; }
        MinMaxVal<int> FoodSeedsMinMax { get; set; }
        ConcurrentDictionary<int, TFood> IdToFoodDict { get; }
        int MaxFoodAge { get; set; }
        float NutrientsPerEat { get; set; }
        float PhotosynthesisEfficency { get; set; }
        float ChanceToGrow { get; set; }
        int TimesDroppedFood { get; set; }
        int TimesFoodWithered { get; set; }
        PlantStage PlantStage { get; set; }
        bool HasRoots { get; set; }
        bool IsSeed { get; set; }
        bool HasStem { get; set; }
        bool HasLeaves { get; set; }
        bool HasSeedlingLeaves { get; set; }
        bool HasSideShoots { get; set; }
        bool HasBranches { get; set; }
        void Grow();
    }
}