using MaceEvolve.Core.Interfaces;
using System.Collections.Generic;

namespace MaceEvolve.Core.Models
{
    public class CreatureStepInfo
    {
        #region Properties
        public EnvironmentInfo EnvironmentInfo { get; set; }
        public IReadOnlyList<Food> VisibleFood { get; set; }
        public IReadOnlyList<ICreature> VisibleCreatures { get; set; }
        public IReadOnlyList<Food> VisibleFoodOrderedByDistance { get; set; }
        public IReadOnlyList<ICreature> VisibleCreaturesOrderedByDistance { get; set; }
        #endregion
    }
}
