using MaceEvolve.Core.Models;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace MaceEvolve.Core.Interfaces
{
    public interface IStepResult<TCreature> where TCreature : class, ICreature
    {
        ConcurrentQueue<StepAction<TCreature>> CalculatedActions { get; }
        ConcurrentDictionary<TCreature, List<NeuralNetworkStepNodeInfo>> CreaturesBrainOutputs { get; }
    }
}
