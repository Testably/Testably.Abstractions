using System;
using Testably.Abstractions.Helpers;

namespace Testably.Abstractions.Testing;

public sealed partial class MockRandomSystem
{
	private sealed class GuidMock : GuidSystemBase
	{
		private readonly MockRandomSystem _randomSystemMock;

		internal GuidMock(MockRandomSystem randomSystem) : base(randomSystem)
		{
			_randomSystemMock = randomSystem;
		}

		/// <inheritdoc cref="IGuid.NewGuid()" />
		public override Guid NewGuid()
			=> _randomSystemMock.RandomProvider.GetGuid();
	}
}