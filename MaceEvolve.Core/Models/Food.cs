﻿using MaceEvolve.Core.Interfaces;

namespace MaceEvolve.Core.Models
{
    public class Food : GameObject, IFood
    {
        #region Properties
        public int Servings { get; set; } = 1;
        public int EnergyPerServing { get; set; } = 10;
        public float ServingDigestionCost { get; set; } = 0.05f;
        #endregion
    }
}
