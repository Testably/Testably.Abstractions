using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Testably.Abstractions.Testing;

namespace Testably.Abstractions.TestHelpers;

[AttributeUsage(AttributeTargets.Class)]
public class RandomSystemTestsAttribute : TypedDataSourceAttribute<RandomSystemTestData>
{
	public override async IAsyncEnumerable<Func<Task<RandomSystemTestData>>> GetTypedDataRowsAsync(
		DataGeneratorMetadata dataGeneratorMetadata)
	{
		await Task.CompletedTask;
		yield return () => Task.FromResult(
			new RandomSystemTestData(new MockRandomSystem()));
		yield return () => Task.FromResult(
			new RandomSystemTestData(new RealRandomSystem()));
	}
}
