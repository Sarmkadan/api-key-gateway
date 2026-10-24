// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using ApiKeyGateway.Benchmarks;
using BenchmarkDotNet.Running;

var switcher = BenchmarkSwitcher.FromAssembly(System.Reflection.Assembly.GetExecutingAssembly());

if (args.Length == 0)
{
    switcher.RunAll();
}
else
{
    switcher.Run(args);
}