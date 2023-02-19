using System.Collections.Generic;
using System.Threading;
using Testably.Abstractions.TimeSystem;

namespace Testably.Abstractions.Tests.TimeSystem;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class TimerTests<TTimeSystem>
	: TimeSystemTestBase<TTimeSystem>
	where TTimeSystem : ITimeSystem
{
	[SkippableFact]
	public void New()
	{
		int count = 0;
		var ms = new ManualResetEventSlim();
		using ITimer timer = TimeSystem.Timer.New(_ =>
		{
			count++;
			if (count > 1)
			{
				ms.Set();
			}
		}, null, 50, 50);

		ms.Wait(1000).Should().BeTrue();
		count.Should().BeGreaterOrEqualTo(2);
	}
}
