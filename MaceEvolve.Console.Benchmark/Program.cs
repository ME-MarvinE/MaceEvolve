using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

namespace MaceEvolve.Console.Benchmark
{
    [SimpleJob(iterationCount: 1)]
    public class Program
    {
        static void Main(string[] args)
        {
            BenchmarkRunner.Run<BenchmarkClass>();
        }
        public class BenchmarkClass
        {
            [Benchmark]
            public void BenchmarkSimulation()
            {
                var program = new Console.Program();

                for (int i = 0; i < 300; i++)
                {
                    program.UpdateSimulation();
                }
            }
        }
    }
}
