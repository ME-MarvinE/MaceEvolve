using MaceEvolve.Core.Enums;
using MaceEvolve.Core.Interfaces;

namespace MaceEvolve.Core.Models
{
    public class StepAction<TCreature> where TCreature : ICreature
    {
        public TCreature Creature { get; set; }
        public CreatureAction Action { get; set; }
    }
}
