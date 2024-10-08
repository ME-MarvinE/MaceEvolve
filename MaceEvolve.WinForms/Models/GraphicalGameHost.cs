﻿using MaceEvolve.Core;
using MaceEvolve.Core.Models;
using System.Collections.Generic;
using System.Drawing;

namespace MaceEvolve.WinForms.Models
{
    public class GraphicalGameHost<TStep, TCreature, TFood, TTree> : GameHost<TStep, TCreature, TFood, TTree> where TCreature : GraphicalCreature, new() where TFood : GraphicalFood, new() where TStep : GraphicalStep<TCreature, TFood, TTree>, new() where TTree : GraphicalTree<TFood>, new()
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
        public override TTree CreateTreeWithRandomLocation()
        {
            TTree tree = base.CreateTreeWithRandomLocation();
            tree.Color = Color.FromArgb(50, 30, 170, 0);

            return tree;
        }
    }
}
