using System;
using Testably.Abstractions.Helpers;
using Testably.Abstractions.RandomSystem;

namespace Testably.Abstractions.Testing.Internal;

internal static class RandomFactory
{
	private static readonly Random Global = new();
	[ThreadStatic] private static IRandom? _shared;

	#region IRandomFactory Members

	/// <inheritdoc cref="IRandomFactory.Shared" />
	public static IRandom Shared
	{
		get
		{
			if (_shared is null)
			{
				_shared = CreateThreadSafeRandomWrapper();
			}

			return _shared;
		}
	}

	#endregion

	/// <summary>
	///     <see href="https://andrewlock.net/building-a-thread-safe-random-implementation-for-dotnet-framework/" />
	/// </summary>
	private static IRandom CreateThreadSafeRandomWrapper()
	{
		int seed;
		lock (Global)
		{
			seed = Global.Next();
		}

		return new RandomWrapper(new Random(seed));
	}
}