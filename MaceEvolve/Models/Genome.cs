using MaceEvolve.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MaceEvolve.Models
{
    public class Genome
    {
        #region Properties
        protected static Random _Random { get; }
        public Dictionary<CreatureOutput, ProcessNode> Nodes { get; set; }
        public static List<CreatureValue> CreatureInputs { get; }
        public static List<CreatureOutput> CreatureOutputs { get; }
        public static Dictionary<CreatureValue, double> DefaultGenes { get; }
        public Dictionary<CreatureValue, double> Genes { get; }
        public static int MinWeight { get; } = 0;
        public static int MaxWeight { get; } = 100;
        public static int MaxNodeDepth { get; } = 3;
        public static int MaxNodeBreadth { get; } = 16;
        #endregion

        #region Constructors
        static Genome()
        {
            _Random = new Random();
            CreatureInputs = Enum.GetValues(typeof(CreatureValue)).Cast<CreatureValue>().ToList();
            CreatureOutputs = Enum.GetValues(typeof(CreatureOutput)).Cast<CreatureOutput>().ToList();

            DefaultGenes = new Dictionary<CreatureValue, double>();
            foreach (CreatureValue Input in CreatureInputs)
            {
                DefaultGenes.Add(Input, _Random.NextDouble());
            }
        }
        public Genome()
            : this(new Dictionary<CreatureValue, double>(DefaultGenes))
        {
        }
        public Genome(Dictionary<CreatureValue, double> Genes)
        {
            this.Genes = Genes;
        }
        #endregion

        #region Methods
        public static int ClampToRange(int Num, int Min, int Max)
        {
            if (Num < Min)
            {
                return Min;
            }
            else if (Num > Max)
            {
                return Max;
            }
            else
            {
                return Num;
            }
        }
        public static void RandomizeGenes(Dictionary<CreatureValue, double> Genes)
        {
            foreach (var Gene in Genes)
            {
                Genes[Gene.Key] = _Random.Next(MaxWeight + 1);
            }
        }
        public static Dictionary<CreatureValue, double> GetRandomizedGenes()
        {
            return DefaultGenes.ToDictionary(x => x.Key, x => _Random.NextDouble());
        }
        public static Dictionary<CreatureValue, double> Mutate(Dictionary<CreatureValue, double> Genes, double MutationChance, double MutationSeverity)
        {
            return new Dictionary<CreatureValue, double>(Genes);
        }
        //public static Dictionary<CreatureOutput, ProcessNode> GetRandomizedNodes(Creature Creature)
        //{

        //    Dictionary<CreatureOutput, ProcessNode> RandomizedNodes = new Dictionary<CreatureOutput, ProcessNode>();

        //    int RandomOutputNodeCount = _Random.Next(1, CreatureOutputs.Count + 1);
        //    for (int i = 0; i < RandomOutputNodeCount; i++)
        //    {
        //        CreatureOutput RandomCreatureOutput = CreatureOutputs[_Random.Next(CreatureOutputs.Count + 1)];
        //        ProcessNode OutputNode = new ProcessNode()
        //        {
        //            ConnectionWeight = GetRandomWeight(),
        //            StartNodeCreature = Creature
        //        };

        //        OutputNode.Inputs.AddRange(GetRandomNodeLayer(1, MaxNodeBreadth + 1));

        //        ////int RandomNodeDepth = _Random.Next(1, MaxNodeDepth + 1);
        //        //int RandomNodeBreadth = _Random.Next(1, MaxNodeBreadth + 1);

        //        //for (int j = 0; j < RandomNodeBreadth; j++)
        //        //{
        //        //    //bool CreateNode = _Random.Next(0, 2) == 1;
        //        //    ProcessNode NewNode = new ProcessNode()
        //        //    {
        //        //        ConnectionWeight = GetRandomWeight(),
        //        //        StartNodeCreature = Creature
        //        //    };
        //        //}

        //        int AllowedDepth = OutputNode.Layers - OutputNode.Depth;
        //        if (AllowedDepth > 0)
        //        {
        //            foreach (ProcessNode ProcessNode in OutputNode.Inputs)
        //            {
        //                OutputNode.Inputs.AddRange(GetRandomNodeLayer(0, MaxNodeBreadth + 1));
        //            }
        //        }
        //    }

        //}
        public static double GetRandomWeight()
        {
            int Bound = 5;
            bool IsNegative = _Random.Next(0, 2) == 1;
            return _Random.NextDouble() * (IsNegative ? Bound : -Bound);
        }
        public static List<ProcessNode> GenerateRandomLayers(int MinBreadth, int MaxBreadth, int Layers, Creature Creature)
        {
            List<ProcessNode> ProcessNodes = new List<ProcessNode>();

            bool IsStartLayer = Layers == 1;

            ProcessNodes.AddRange(GetRandomNodeLayer(MinBreadth, MaxBreadth + 1));

            foreach (ProcessNode ProcessNode in ProcessNodes)
            {
                if (IsStartLayer)
                {
                    ProcessNode.IsStartNode = true;
                    ProcessNode.StartNodeCreature = Creature;
                    ProcessNode.StartNodeValue = GetRandomCreatureInput();
                }
                else
                {
                    ProcessNode.Inputs.AddRange(GenerateRandomLayers(MinBreadth, MaxBreadth, Layers - 1, Creature));
                }
            }

            return ProcessNodes;
        }
        public static List<ProcessNode> GetRandomNodeLayer(int MinBreadth, int MaxBreadth, LayerType LayerType, Creature InputCreature)
        {
            List<ProcessNode> ProcessNodes = new List<ProcessNode>();

            int RandomNodeBreadth = _Random.Next(MinBreadth, MaxBreadth + 1);

            for (int i = 0; i < RandomNodeBreadth; i++)
            {
                ProcessNodes.Add(
                    new ProcessNode()
                    {
                        ConnectionWeight = GetRandomWeight()
                    });
            }

            switch (LayerType)
            {
                case LayerType.Input:
                    foreach (ProcessNode ProcessNode in ProcessNodes)
                    {
                        ProcessNode.IsStartNode = true;
                        ProcessNode.StartNodeCreature = InputCreature;
                        ProcessNode.StartNodeValue = GetRandomCreatureInput();
                    }
                    break;
                case LayerType.Output:
                    foreach (ProcessNode ProcessNode in ProcessNodes)
                    {

                    }
                    break;
                case LayerType.Process:

                    break;
                default:
                    throw new NotImplementedException();
            }

            foreach (ProcessNode ProcessNode in ProcessNodes)
            {
                if (IsStartLayer)
                {
                    ProcessNode.IsStartNode = true;
                    ProcessNode.StartNodeCreature = Creature;
                    ProcessNode.StartNodeValue = GetRandomCreatureInput();
                }
                else
                {
                    ProcessNode.Inputs.AddRange(GenerateRandomLayers(MinBreadth, MaxBreadth, Layers - 1, Creature));
                }
            }

            return ProcessNodes;
        }
        //public static void AddLayerToInputs(List<ProcessNode> Inputs, ProcessNode ParentNode = null)
        //{
        //    if (ParentNode == null)
        //    {
        //        foreach (ProcessNode ChildNode in Inputs)
        //        {
        //            if (Inputs == null)
        //            {
        //                bool MoreDepth = _Random.Next(0, 2) == 1;
        //                if (MoreDepth)
        //                {
        //                    Inputs.AddRange(GetRandomNodeLayer(1, ));
        //                }
        //            }
        //            else
        //            {

        //            }
        //        }
        //    }
        //}
        public static CreatureOutput GetRandomCreatureOutput()
        {
            return CreatureOutputs[_Random.Next(CreatureOutputs.Count)];
        }
        public static CreatureValue GetRandomCreatureInput()
        {
            return CreatureInputs[_Random.Next(CreatureInputs.Count)];
        }
        #endregion
    }
}
