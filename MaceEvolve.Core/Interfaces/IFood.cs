﻿namespace MaceEvolve.Core.Interfaces
{
    public interface IFood : IGameObject
    {
        int Servings { get; set; }
        int EnergyPerServing { get; set; }
        float ServingDigestionCost { get; set; }
    }
}