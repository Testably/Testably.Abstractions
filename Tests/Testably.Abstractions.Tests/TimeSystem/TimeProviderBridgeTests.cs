#if FEATURE_TIMEPROVIDER
using System.Threading;

namespace Testably.Abstractions.Tests.TimeSystem;

[TimeSystemTests]
public class TimeProviderBridgeTests(TimeSystemTestData testData) : TimeSystemTestBase(testData)
{
	[Test]
	public async Task ToTimeProvider_ThrowingCallback_ShouldKeepFiring()
	{
		System.TimeProvider provider = TimeSystem.ToTimeProvider();
		int count = 0;
		using ManualResetEventSlim ms = new();

		using System.Threading.ITimer timer = provider.CreateTimer(
			_ =>
			{
				// ReSharper disable once AccessToDisposedClosure
				if (Interlocked.Increment(ref count) >= 2)
				{
					// ReSharper disable once AccessToDisposedClosure
					ms.Set();
				}

				throw new InvalidOperationException("callback failure");
			},
			null,
			TimeSpan.Zero,
			TimeSpan.FromMilliseconds(50));

		await That(ms.Wait(ExpectSuccess, CancellationToken)).IsTrue();
		await That(count).IsGreaterThanOrEqualTo(2);
	}
}
#endif
