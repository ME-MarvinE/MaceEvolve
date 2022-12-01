using System;
using System.Collections.Generic;

namespace MaceEvolve.Core.Models
{
    public class EnvironmentInfo
    {
        #region Properties
        public IReadOnlyList<Food> ExistingFood { get; }
        public IReadOnlyList<Creature> ExistingCreatures { get; }
        public Rectangle WorldBounds { get; }
        #endregion

        #region Constructors
        public EnvironmentInfo(IReadOnlyList<Creature> existingCreatures, IReadOnlyList<Food> existingFood, Rectangle worldBounds)
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
