using System;
using Testably.Abstractions.Helpers;

namespace Testably.Abstractions;

public sealed partial class RealRandomSystem
{
	private sealed class GuidWrapper : GuidSystemBase
	{
		internal GuidWrapper(RealRandomSystem randomSystem) : base(randomSystem)
		{
		}

		/// <inheritdoc cref="IGuid.NewGuid()" />
		public override Guid NewGuid()
			=> System.Guid.NewGuid();
	}
}