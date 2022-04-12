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
        public List<ProcessNode> Inputs { get; set; }
        public Creature StartNodeCreature { get; set; }
        public CreatureValue StartNodeValue { get; set; }
        public bool IsStartNode { get; set; }
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
                        ReturnValue = Creature.ProximityToFood(StartNodeCreature);
                        break;

                    case CreatureValue.PercentMaxEnergy:
                        ReturnValue = Creature.PercentMaxEnergy(StartNodeCreature);
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
