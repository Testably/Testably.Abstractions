using System;
using Testably.Abstractions.Helpers;

namespace Testably.Abstractions.RandomSystem;

internal sealed class GuidWrapper : GuidSystemBase
{
	internal GuidWrapper(RealRandomSystem randomSystem) : base(randomSystem)
	{
	}

	/// <inheritdoc cref="IGuid.NewGuid()" />
	public override Guid NewGuid()
		=> Guid.NewGuid();

#if FEATURE_GUID_V7
	/// <inheritdoc cref="IGuid.CreateVersion7()" />
	public override Guid CreateVersion7()
		=> Guid.CreateVersion7();
#endif

#if FEATURE_GUID_V7
	/// <inheritdoc cref="IGuid.CreateVersion7(DateTimeOffset)" />
	public override Guid CreateVersion7(DateTimeOffset timestamp)
		=> Guid.CreateVersion7(timestamp);
#endif
}
