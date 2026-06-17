using MaceEvolve.Core.Enums;
using MaceEvolve.Core.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MaceEvolve.Core.Models
{
    public class Plant : Plant<Food>
    {
    }
    public class Plant<TFood> : LivingGameObject, IPlant<TFood> where TFood : IFood, new()
    {
        #region Properties
        public ConcurrentDictionary<int, TFood> IdToFoodDict { get; } = new ConcurrentDictionary<int, TFood>();
        public ConcurrentDictionary<int, int> FoodIdToAgeDict { get; } = new ConcurrentDictionary<int, int>();
        public PlantStage PlantStage { get; set; }
        public bool HasRoots { get; set; }
        public bool IsSeed { get; set; }
        public bool HasStem { get; set; }
        public bool HasLeaves { get; set; }
        public bool HasSeedlingLeaves { get; set; }
        public bool HasSideShoots { get; set; }
        public bool HasBranches { get; set; }
        public MinMaxVal<float> FoodSizeMinMax { get; set; } = MinMaxVal.Create(5f, 12);
        public MinMaxVal<float> FoodMassMinMax { get; set; } = MinMaxVal.Create(4f, 12);
        public MinMaxVal<float> FoodEnergyMinMax { get; set; } = MinMaxVal.Create(150f, 300);
        public MinMaxVal<float> FoodNutrientsMinMax { get; set; } = MinMaxVal.Create(10f, 50);
        public MinMaxVal<int> FoodSeedsMinMax { get; set; } = MinMaxVal.Create(0, 2);
        public float ChanceToGrowFood { get; set; } = 0.04f;
        public float ChanceToDropFood { get; set; } = 0.01f;
        public float ChanceToReproduce { get; set; } = 0.01f;
        public int MaxFoodAge { get; set; } = 100;
        public int TimesDroppedFood { get; set; }
        public int TimesFoodWithered { get; set; }
        public float TimesReproduced { get; set; }
        public float EnergyPerEat { get; set; }
        public float NutrientsPerEat { get; set; }
        public float AgeRequiredToReproduce { get; set; }
        public float AgeRequiredToCreateFood { get; set; }
        public float EnergyRequiredToReproduce { get; set; } = 1000;
        public float MassRequiredToReproduce { get; set; } = float.MaxValue;/* 40;*/
        public float NutrientsRequiredToReproduce { get; set; } = 100;
        public float PhotosynthesisEfficency { get; set; } = 0.25f;
        public float ChanceToGrow { get; set; } = 0.001f;
        #endregion

        #region Constructors
        public Plant()
        {
            Type = GameObjectType.Plant;
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

        #region Methods
        public void Grow()
        {
            switch (PlantStage)
            {
                case PlantStage.Seed:
                    SetSizeCentered(Size * 1.1f);
                    HasRoots = true;
                    PlantStage = PlantStage.RootedSeed;
                    break;

                case PlantStage.RootedSeed:
                    SetSizeCentered(Size * 1.1f);
                    HasStem = true;
                    PlantStage = PlantStage.Shoot;
                    break;

                case PlantStage.Shoot:
                    SetSizeCentered(Size * 1.1f);
                    HasSeedlingLeaves = true;
                    PlantStage = PlantStage.Sprout;
                    break;

                case PlantStage.Sprout:
                    SetSizeCentered(Size * 1.3f);
                    HasSeedlingLeaves = false;
                    HasSideShoots = true;
                    PlantStage = PlantStage.YoungPlant;
                    break;

                case PlantStage.YoungPlant:
                    SetSizeCentered(Size * 1.5f);
                    HasLeaves = true;
                    PlantStage = PlantStage.MaturePlant;
                    break;

                case PlantStage.MaturePlant:
                    SetSizeCentered(Size * 1.5f);
                    HasBranches = true;
                    PlantStage = PlantStage.VeryMaturePlant;
                    break;
            }
        }
        //public void Grow()
        //{
        //    if (!PlantStructures.TryGetValue(PlantStructure.Roots, out bool hasRoots))
        //    {
        //        PlantStructures.TryUpdate(PlantStructure.Roots, true, false);
        //        return;
        //    }

        //    if (!PlantStructures.TryGetValue(PlantStructure.Stem, out bool hasStem))
        //    {
        //        PlantStructures.TryUpdate(PlantStructure.Stem, true, false);
        //        return;
        //    }

        //    if (!PlantStructures.TryGetValue(PlantStructure.Leaves, out bool hasLeaves))
        //    {
        //        PlantStructures.TryUpdate(PlantStructure.Leaves, true, false);
        //        PlantStructures.TryUpdate(PlantStructure.Shell, false, true);
        //        return;
        //    }

        //    if (!PlantStructures.TryGetValue(PlantStructure.Flowers, out bool hasFlowers))
        //    {
        //        PlantStructures.TryUpdate(PlantStructure.Flowers, true, false);
        //        return;
        //    }

        //    if (!PlantStructures.TryGetValue(PlantStructure.Branches, out bool hasBranches))
        //    {
        //        PlantStructures.TryUpdate(PlantStructure.Branches, true, false);
        //        return;
        //    }
        //}
        #endregion
    }
}
