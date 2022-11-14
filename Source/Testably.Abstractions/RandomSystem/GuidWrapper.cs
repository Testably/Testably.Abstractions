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
}
