using MaceEvolve.Core.Enums;
using MaceEvolve.Core.Interfaces;
using System.Collections.Generic;

namespace MaceEvolve.Core.Models
{
    public class StepAction<TCreature> where TCreature : ICreature
    {
        public TCreature Creature { get; set; }
        public CreatureAction Action { get; set; }
        public Dictionary<CreatureAction, float> CreatureActionToOutputValueDict { get; set; }
    }
}
