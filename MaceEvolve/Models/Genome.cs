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
        public static List<CreatureInputType> CreatureInputs { get; }
        public static Dictionary<CreatureInputType, double> DefaultGenes { get; }
        public Dictionary<CreatureInputType, double> Genes { get; }
        public static int MinWeight { get; } = 0;
        public static int MaxWeight { get; } = 100;
        #endregion

        #region Constructors
        static Genome()
        {
            _Random = new Random();
            CreatureInputs = Enum.GetValues(typeof(CreatureInputType)).Cast<CreatureInputType>().ToList();

            DefaultGenes = new Dictionary<CreatureInputType, double>();
            foreach (CreatureInputType Input in CreatureInputs)
            {
                DefaultGenes.Add(Input, _Random.NextDouble());
            }
        }
        public Genome()
            : this(new Dictionary<CreatureInputType, double>(DefaultGenes))
        {
        }
        public Genome(Dictionary<CreatureInputType, double> Genes)
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
        public static void RandomizeGenes(Dictionary<CreatureInputType, double> Genes)
        {
            foreach (var Gene in Genes)
            {
                Genes[Gene.Key] = _Random.Next(MaxWeight + 1);
            }
        }
        public static Dictionary<CreatureInputType, double> GetRandomizedGenes()
        {
            return DefaultGenes.ToDictionary(x => x.Key, x => _Random.NextDouble());
        }
        public static Dictionary<CreatureInputType, double> Mutate(Dictionary<CreatureInputType, double> Genes, double MutationChance, double MutationSeverity)
        {
            return new Dictionary<CreatureInputType, double>(Genes);
        }
        #endregion
    }
}
