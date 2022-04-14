using MaceEvolve.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaceEvolve.Models
{
    public class ProcessNode
    {
        public double ConnectionWeight { get; set; } = 1;
        public List<ProcessNode> Inputs { get; set; } = new List<ProcessNode>();
        public CreatureValue InputNodeCreatureInput { get; set; }
        public CreatureOutput OutputNodeCreatureOutput { get; set; }
        public int HighestDepth
        {
            get
            {
                int NewDepth = 0;

                foreach (ProcessNode ProcessNode in Inputs)
                {
                    if (ProcessNode.HighestDepth + 1 > NewDepth)
                    {
                        NewDepth = ProcessNode.HighestDepth + 1;
                    }
                }

                return NewDepth;
            }
        }
        public int Breadth
        {
            get
            {
                return Inputs.Count;
            }
        }
        public NodeType NodeType { get; set; }
        public double? InputNodeValue { get; set; } 
        public double GetValue()
        {
            double ReturnValue = 0;

            switch (NodeType)
            {
                case NodeType.Input:
                    if (InputNodeValue == null)
                    {
                        throw new InvalidOperationException("No input value given for input node.");
                    }
                    ReturnValue = InputNodeValue.Value * ConnectionWeight;
                    break;
                case NodeType.Process:
                case NodeType.Output:
                    if (Inputs.Count == 0)
                    {
                        throw new InvalidOperationException("No inputs specified for node.");
                    }

                    foreach (ProcessNode ProcessNode in Inputs)
                    {
                        ReturnValue += ProcessNode.GetValue() * ProcessNode.ConnectionWeight;
                    }
                    break;
                default:
                    throw new NotImplementedException();
            }
            return Globals.Sigmoid(ReturnValue);
        }
    }
}
