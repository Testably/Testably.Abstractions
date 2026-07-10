using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Testably.Abstractions.Testing;

namespace Testably.Abstractions.TestHelpers;

[AttributeUsage(AttributeTargets.Class)]
public class TimeSystemTestsAttribute(bool disableAutoAdvance = false)
	: TypedDataSourceAttribute<TimeSystemTestData>
{
	public override async IAsyncEnumerable<Func<Task<TimeSystemTestData>>> GetTypedDataRowsAsync(
		DataGeneratorMetadata dataGeneratorMetadata)
	{
		await Task.CompletedTask;
		yield return () =>
		{
			DateTime now = DateTime.UtcNow;
			return Task.FromResult(
				new TimeSystemTestData(now, new MockTimeSystem(TimeProviderFactory.Use(now), o =>
				{
					if (disableAutoAdvance)
					{
						o.DisableAutoAdvance();
					}

					return o;
				})));
		};
		yield return () => Task.FromResult(
			new TimeSystemTestData(DateTime.UtcNow, new RealTimeSystem()));
	}
}
