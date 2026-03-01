using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Testably.Abstractions.Testing;
using TimeProvider = Testably.Abstractions.Testing.TimeProvider;

namespace Testably.Abstractions.TestHelpers;

[AttributeUsage(AttributeTargets.Class)]
public class TimeSystemTestsAttribute : TypedDataSourceAttribute<TimeSystemTestData>
{
	public override async IAsyncEnumerable<Func<Task<TimeSystemTestData>>> GetTypedDataRowsAsync(
		DataGeneratorMetadata dataGeneratorMetadata)
	{
		await Task.CompletedTask;
		yield return () => Task.FromResult(
			new TimeSystemTestData(new MockTimeSystem(TimeProvider.Now())));
		yield return () => Task.FromResult(
			new TimeSystemTestData(new RealTimeSystem()));
	}
}
