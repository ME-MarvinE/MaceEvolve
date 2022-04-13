using MaceEvolve.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaceEvolve.Models
{
    public class ProcessNode : Node
    {
        public double ConnectionWeight { get; set; } = 1;
        public List<ProcessNode> Inputs { get; set; } = new List<ProcessNode>();
        public Creature StartNodeCreature { get; set; }
        public CreatureValue StartNodeValue { get; set; }
        public bool IsStartNode { get; set; }
        public int Depth
        {
            get
            {
                int NewDepth = 0;

                foreach (ProcessNode ProcessNode in Inputs)
                {
                    if (ProcessNode.Depth + 1 > NewDepth)
                    {
                        NewDepth = ProcessNode.Depth + 1;
                    }
                }

                return NewDepth;
            }
        }
        public double GetValue()
        {
            double ReturnValue = 0;

            if (IsStartNode)
            {
                if (StartNodeCreature == null)
                {
                    throw new InvalidOperationException("No creature specified for startnode.");
                }

                switch (StartNodeValue)
                {
                    case CreatureValue.ProximityToFood:
                        ReturnValue = Creature.ProximityToFood(StartNodeCreature) * ConnectionWeight;
                        break;

                    case CreatureValue.PercentMaxEnergy:
                        ReturnValue = Creature.PercentMaxEnergy(StartNodeCreature) * ConnectionWeight;
                        break;

                    default:
                        throw new NotImplementedException();
                }
            }
            else
            {
                if (Inputs.Count == 0)
                {
                    throw new InvalidOperationException("No inputs specified for non-startnode.");
                }

                foreach (ProcessNode ProcessNode in Inputs)
                {
                    ReturnValue += ProcessNode.GetValue() * ProcessNode.ConnectionWeight;
                }
            }


            return Globals.Sigmoid(ReturnValue);
        }
    }
}
