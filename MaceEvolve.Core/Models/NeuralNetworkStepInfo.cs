using MaceEvolve.Core.Enums;
using System.Collections.Generic;

namespace MaceEvolve.Core.Models
{
    public class NeuralNetworkStepNodeInfo
    {
        public int NodeId { get; set; }
        public virtual NodeType NodeType { get; set; }
        public float Bias { get; set; }
        public CreatureInput? CreatureInput { get; set; }
        public CreatureAction? CreatureAction { get; set; }
        public float PreviousOutput { get; set; }
        public List<Connection> ConnectionsFrom { get; set; } = new List<Connection>();
        public List<Connection> ConnectionsTo { get; set; } = new List<Connection>();
        public List<Connection> Connections { get; set; } = new List<Connection>();
    }
}
