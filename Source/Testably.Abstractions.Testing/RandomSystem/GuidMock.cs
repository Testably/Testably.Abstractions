using System;
using Testably.Abstractions.Helpers;
using Testably.Abstractions.RandomSystem;

namespace Testably.Abstractions.Testing.RandomSystem;

internal sealed class GuidMock : GuidSystemBase
{
	private readonly MockRandomSystem _mockRandomSystem;

	internal GuidMock(MockRandomSystem randomSystem) : base(randomSystem)
	{
		_mockRandomSystem = randomSystem;
	}

	/// <inheritdoc cref="IGuid.NewGuid()" />
	public override Guid NewGuid()
		=> _mockRandomSystem.RandomProvider.GetGuid();

#if FEATURE_GUID_V7
	/// <inheritdoc cref="IGuid.CreateVersion7()" />
	public override Guid CreateVersion7()
		=> _mockRandomSystem.RandomProvider.GetGuid();
#endif

#if FEATURE_GUID_V7
	/// <inheritdoc cref="IGuid.CreateVersion7(DateTimeOffset)" />
	public override Guid CreateVersion7(DateTimeOffset timestamp)
		=> _mockRandomSystem.RandomProvider.GetGuid();
#endif
}
