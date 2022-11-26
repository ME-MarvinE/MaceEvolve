using System.Collections.Generic;

namespace MaceEvolve.Models
{
    public class CreatureStepInfo
    {
        #region Properties
        public IReadOnlyList<Food> ExistingFood { get; set; }
        public IReadOnlyList<Creature> ExistingCreatures { get; set; }
        public IReadOnlyList<Food> VisibleFood { get; set; }
        public IReadOnlyList<Creature> VisibleCreatures { get; set; }
        public IReadOnlyList<Food> VisibleFoodOrderedByDistance { get; set; }
        public IReadOnlyList<Creature> VisibleCreaturesOrderedByDistance { get; set; }
        #endregion
    }
}
