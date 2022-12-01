using MaceEvolve.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MaceEvolve.Core.Models
{
    public class Genome
    {
        #region Properties
        protected static Random random { get; }
        public static List<CreatureInput> CreatureInputs { get; }
        public static Dictionary<CreatureInput, double> DefaultGenes { get; }
        public Dictionary<CreatureInput, double> Genes { get; }
        public static int MinWeight { get; } = 0;
        public static int MaxWeight { get; } = 100;
        #endregion

        #region Constructors
        static Genome()
        {
            random = new Random();
            CreatureInputs = Enum.GetValues(typeof(CreatureInput)).Cast<CreatureInput>().ToList();

            DefaultGenes = new Dictionary<CreatureInput, double>();
            foreach (CreatureInput Input in CreatureInputs)
            {
                DefaultGenes.Add(Input, random.NextDouble());
            }
        }
        public Genome()
            : this(new Dictionary<CreatureInput, double>(DefaultGenes))
        {
        }
        public Genome(Dictionary<CreatureInput, double> genes)
        {
            Genes = genes;
        }
        #endregion

        #region Methods
        public static int ClampToRange(int num, int min, int max)
        {
            if (num < min)
            {
                return min;
            }
            else if (num > max)
            {
                return max;
            }
            else
            {
                return num;
            }
        }
        public static void RandomizeGenes(Dictionary<CreatureInput, double> genes)
        {
            foreach (var gene in genes)
            {
                genes[gene.Key] = random.Next(MaxWeight + 1);
            }
        }
        public static Dictionary<CreatureInput, double> GetRandomizedGenes()
        {
            return DefaultGenes.ToDictionary(x => x.Key, x => random.NextDouble());
        }
        public static Dictionary<CreatureInput, double> Mutate(Dictionary<CreatureInput, double> genes, double mutationChance, double mutationSeverity)
        {
            return new Dictionary<CreatureInput, double>(genes);
        }
        #endregion
    }
}
