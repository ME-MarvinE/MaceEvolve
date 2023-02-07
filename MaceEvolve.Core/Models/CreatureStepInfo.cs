using MaceEvolve.Core.Interfaces;
using System.Collections.Generic;

namespace MaceEvolve.Core.Models
{
    public class CreatureStepInfo<TCreature, TFood> where TCreature : ICreature where TFood : IFood
    {
        #region Properties
        public IEnumerable<TFood> VisibleFood { get; set; }
        public IEnumerable<TCreature> VisibleCreatures { get; set; }
        public IEnumerable<TFood> VisibleFoodOrderedByDistance { get; set; }
        public IEnumerable<TCreature> VisibleCreaturesOrderedByDistance { get; set; }
        #endregion
    }
}
