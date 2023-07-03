using MaceEvolve.Core.Interfaces;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace MaceEvolve.Core.Models
{
    public class StepResult<TCreature> : IStepResult<TCreature> where TCreature : class, ICreature
    {
        public ConcurrentDictionary<TCreature, List<NeuralNetworkStepNodeInfo>> CreaturesBrainOutputs { get; }
        public ConcurrentQueue<StepAction<TCreature>> CalculatedActions { get; }
        public StepResult(ConcurrentQueue<StepAction<TCreature>> calculatedActions, ConcurrentDictionary<TCreature, List<NeuralNetworkStepNodeInfo>> creaturesNodeOutputs = null)
        {
            CalculatedActions = calculatedActions;
            CreaturesBrainOutputs = creaturesNodeOutputs;
        }
    }
}
