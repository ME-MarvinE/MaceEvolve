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
        public static List<CreatureValue> CreatureInputs { get; }
        public static Dictionary<CreatureValue, double> DefaultGenes { get; }
        public Dictionary<CreatureValue, double> Genes { get; }
        public static int MinWeight { get; } = 0;
        public static int MaxWeight { get; } = 100;
        #endregion

        #region Constructors
        static Genome()
        {
            _Random = new Random();
            CreatureInputs = Enum.GetValues(typeof(CreatureValue)).Cast<CreatureValue>().ToList();

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
        #endregion
    }
}
