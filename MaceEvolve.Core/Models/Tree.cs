using MaceEvolve.Core.Enums;
using MaceEvolve.Core.Interfaces;
using System.Collections.Concurrent;

namespace MaceEvolve.Core.Models
{
    public class Tree : Tree<Food>
    {
    }
    public class Tree<TFood> : LivingGameObject, ITree<TFood> where TFood : IFood, new()
    {
        #region Properties
        public ConcurrentDictionary<int, TFood> IdToFoodDict { get; } = new ConcurrentDictionary<int, TFood>();
        public ConcurrentDictionary<int, int> FoodIdToAgeDict { get; } = new ConcurrentDictionary<int, int>();
        public MinMaxVal<float> FoodSizeMinMax { get; set; } = MinMaxVal.Create(5f, 12);
        public MinMaxVal<float> FoodMassMinMax { get; set; } = MinMaxVal.Create(4f, 12);
        public MinMaxVal<float> FoodEnergyMinMax { get; set; } = MinMaxVal.Create(150f, 300);
        public MinMaxVal<float> FoodNutrientsMinMax { get; set; } = MinMaxVal.Create(10f, 50);
        public float ChanceToGrowFood { get; set; } = 0.04f;
        public float ChanceToDropFood { get; set; } = 0.01f;
        public float ChanceToReproduce { get; set; } = 0.01f;
        public int MaxFoodAmount { get; set; } = 5;
        public int MaxFoodAge { get; set; } = 100;
        public int TimesDroppedFood { get; set; }
        public int TimesFoodWithered { get; set; }
        public float TimesReproduced { get; set; }
        public float EnergyPerEat { get; set; }
        public float NutrientsPerEat { get; set; }
        public float AgeRequiredToReproduce { get; set; }
        public float AgeRequiredToCreateFood { get; set; }
        public float EnergyRequiredToReproduce { get; set; } = 1000;
        public float MassRequiredToReproduce { get; set; } = 40;
        public float NutrientsRequiredToReproduce { get; set; } = 100;
        public float PhotosynthesisEfficency { get; set; } = 0.25f;
        #endregion

        #region Constructors
        public Tree()
        {
            Type = GameObjectType.Tree;
            IdToFoodDict = new ConcurrentDictionary<int, TFood>();
            MaxEnergy = 4000;
            MaxNutrients = 500;
            MaxAge = 32000;
            MaxHealthPoints = 1000;
            Energy = 300;
            Nutrients = 100;
            HealthPoints = 900;
            Mass = 50;
            AgeRequiredToReproduce = MaxAge / 4;
            AgeRequiredToCreateFood = MaxAge / 4;
        }
        #endregion
    }
}
