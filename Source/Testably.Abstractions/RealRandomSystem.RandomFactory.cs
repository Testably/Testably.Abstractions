using System;
using Testably.Abstractions.Helpers;

namespace Testably.Abstractions;

public sealed partial class RealRandomSystem
{
	private sealed class RandomFactory : IRandomFactory
	{
		private static readonly Random Global = new();
		private IRandom? _shared;

		internal RandomFactory(RealRandomSystem timeSystem)
		{
			RandomSystem = timeSystem;
		}

		#region IRandomFactory Members

		/// <inheritdoc cref="IRandomSystemExtensionPoint.RandomSystem" />
		public IRandomSystem RandomSystem { get; }

		/// <inheritdoc cref="IRandomFactory.Shared" />
		public IRandom Shared
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
}