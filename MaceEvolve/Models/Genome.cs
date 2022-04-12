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
        public static List<GeneType> GeneTypes { get; }
        public static Dictionary<GeneType, int> DefaultGenes { get; }
        public Dictionary<GeneType, int> Genes { get; }
        public static int MinWeight { get; } = 0;
        public static int MaxWeight { get; } = 100;
        #endregion

        #region Constructors
        static Genome()
        {
            _Random = new Random();
            GeneTypes = Enum.GetValues(typeof(GeneType)).Cast<GeneType>().ToList();

            DefaultGenes = new Dictionary<GeneType, int>();
            foreach (GeneType GeneType in GeneTypes)
            {
                DefaultGenes.Add(GeneType, MinWeight);
            }

            DefaultGenes[GeneType.MoveForward] = MaxWeight;
        }
        public Genome()
            : this(new Dictionary<GeneType, int>(DefaultGenes))
        {
        }
        public Genome(Dictionary<GeneType, int> Genes)
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
        public static void RandomizeGenes(Dictionary<GeneType, int> Genes)
        {
            foreach (var Gene in Genes)
            {
                Genes[Gene.Key] = _Random.Next(MaxWeight + 1);
            }
        }
        public static Dictionary<GeneType, int> GetRandomizedGenes()
        {
            return DefaultGenes.ToDictionary(x => x.Key, x => _Random.Next(MaxWeight + 1));
        }
        public static Dictionary<GeneType, int> Mutate(Dictionary<GeneType, int> Genes, double MutationChance, double MutationSeverity)
        {
            return new Dictionary<GeneType, int>(Genes);
        }
        #endregion
    }
}
