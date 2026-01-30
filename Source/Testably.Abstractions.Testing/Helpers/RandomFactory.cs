using System;
using Testably.Abstractions.Helpers;
using Testably.Abstractions.RandomSystem;

namespace Testably.Abstractions.Testing.Helpers;

internal static class RandomFactory
{
	private static readonly Random Global = new();

	/// <inheritdoc cref="IRandomFactory.Shared" />
	[field: ThreadStatic]
	public static IRandom Shared
		=> field ??= CreateThreadSafeRandomWrapper();

	/// <summary>
	///     <see href="https://andrewlock.net/building-a-thread-safe-random-implementation-for-dotnet-framework/" />
	/// </summary>
	private static RandomWrapper CreateThreadSafeRandomWrapper()
	{
		int seed;
		lock (Global)
		{
			seed = Global.Next();
		}

		return new RandomWrapper(new Random(seed));
	}
}
