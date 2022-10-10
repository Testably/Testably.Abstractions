using System;

namespace Testably.Abstractions;

public sealed partial class RandomSystem
{
	private sealed class RandomFactory : IRandomSystem.IRandomFactory
	{
		private static readonly Random Global = new();
		private IRandomSystem.IRandom? _shared;

		internal RandomFactory(RandomSystem timeSystem)
		{
			RandomSystem = timeSystem;
		}

		#region IRandomFactory Members

		/// <inheritdoc cref="IRandomSystem.IRandomSystemExtensionPoint.RandomSystem" />
		public IRandomSystem RandomSystem { get; }

		/// <inheritdoc cref="IRandomSystem.IRandomFactory.Shared" />
		public IRandomSystem.IRandom Shared
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

		/// <inheritdoc cref="IRandomSystem.IRandomFactory.New()" />
		public IRandomSystem.IRandom New()
			=> CreateThreadSafeRandomWrapper();

		/// <inheritdoc cref="IRandomSystem.IRandomFactory.New(int)" />
		public IRandomSystem.IRandom New(int seed)
			=> new RandomWrapper(new Random(seed));

		#endregion

		/// <summary>
		///     <see href="https://andrewlock.net/building-a-thread-safe-random-implementation-for-dotnet-framework/" />
		/// </summary>
		private static IRandomSystem.IRandom CreateThreadSafeRandomWrapper()
		{
			int seed;
			lock (Global)
			{
				seed = Global.Next();
			}

			return new RandomWrapper(new Random(seed));
		}
	}
}