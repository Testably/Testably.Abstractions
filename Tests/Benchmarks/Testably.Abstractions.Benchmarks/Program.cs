﻿using BenchmarkDotNet.Running;

namespace Testably.Abstractions.Benchmarks;

internal static class Program
{
	public static void Main(string[] args)
	{
		BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
	}
}
