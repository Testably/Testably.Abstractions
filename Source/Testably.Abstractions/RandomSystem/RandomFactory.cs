using System;
using Testably.Abstractions.Helpers;

namespace Testably.Abstractions.RandomSystem;

internal sealed class RandomFactory : IRandomFactory
{
	[ThreadStatic] private static IRandom? _shared;
	private static readonly Random Global = new();

	internal RandomFactory(RealRandomSystem timeSystem)
	{
		RandomSystem = timeSystem;
	}

	#region IRandomFactory Members

	/// <inheritdoc cref="IRandomSystemEntity.RandomSystem" />
	public IRandomSystem RandomSystem { get; }

	/// <inheritdoc cref="IRandomFactory.Shared" />
	public IRandom Shared
		=> _shared ??= CreateThreadSafeRandomWrapper();

	/// <inheritdoc cref="IRandomFactory.New()" />
	public IRandom New()
		=> CreateThreadSafeRandomWrapper();

	/// <inheritdoc cref="IRandomFactory.New(int)" />
	public IRandom New(int seed)
		=> new RandomWrapper(new Random(seed));

	#endregion

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
