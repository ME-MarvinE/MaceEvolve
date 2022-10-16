using MaceEvolve.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaceEvolve.Models
{
    public class NeuralNetworkStepInfo
    {
        public int NodeId { get; set; }
        public virtual NodeType NodeType { get; set; }
        public double Bias { get; set; }
        public CreatureInput? CreatureInput { get; set; }
        public CreatureAction? CreatureAction { get; set; }
        public double PreviousOutput { get; set; }
        public int FailedEvaluationCount { get; set; }
        public int PreviousValueUsedCount { get; set; }
        public List<Connection> ConnectionsFrom { get; set; } = new List<Connection>();
        public List<Connection> ConnectionsTo { get; set; } = new List<Connection>();
    }
}
