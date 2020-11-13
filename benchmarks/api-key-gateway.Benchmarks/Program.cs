// =============================================================================
// Author: Vladyslav Zaiets | https://sarmkadan.com
// CTO & Software Architect
// =============================================================================

using BenchmarkDotNet.Running;
using ApiKeyGateway.Benchmarks;

var switcher = BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly);

if (args.Length == 0)
    switcher.RunAll();
else
    switcher.Run(args);
