using MaceEvolve.Core.Interfaces;
using System;
using System.Collections.Generic;

namespace MaceEvolve.Core.Models
{
    public class EnvironmentInfo
    {
        #region Properties
        public IReadOnlyList<IFood> ExistingFood { get; }
        public IReadOnlyList<ICreature> ExistingCreatures { get; }
        public IRectangle WorldBounds { get; }
        #endregion

        #region Constructors
        public EnvironmentInfo(IReadOnlyList<ICreature> existingCreatures, IReadOnlyList<IFood> existingFood, IRectangle worldBounds)
        {
            if (existingCreatures == null) { throw new ArgumentNullException(nameof(existingCreatures)); }
            if (existingFood == null) { throw new ArgumentNullException(nameof(existingFood)); }

            ExistingCreatures = existingCreatures;
            ExistingFood = existingFood;
            WorldBounds = worldBounds;
        }
        #endregion
    }
}
