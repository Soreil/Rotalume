using BenchmarkDotNet.Running;

namespace BenchmarkSuite1;

internal class Program
{
    private static void Main(string[] args)
    {
        _ = BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
    }
}
