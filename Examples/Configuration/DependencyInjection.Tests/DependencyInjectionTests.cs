using aweXpect;
using Microsoft.Extensions.DependencyInjection;
using System.IO.Abstractions;
using System.Threading.Tasks;
using Xunit;

namespace Testably.Abstractions.Examples.Configuration.DependencyInjection.Tests;

public class DependencyInjectionTests
{
	[Fact]
	public async Task
		DependencyInjection_Microsoft_ShouldAllowRegistrationAndCreationOfInstances()
	{
		ServiceProvider services = new ServiceCollection()
			.AddSingleton<IFileSystem, RealFileSystem>()
			.AddSingleton<IRandomSystem, RealRandomSystem>()
			.AddSingleton<ITimeSystem, RealTimeSystem>()
			.BuildServiceProvider();

		await Expect.That(services.GetService<IFileSystem>()).Is<RealFileSystem>();
		await Expect.That(services.GetService<IRandomSystem>()).Is<RealRandomSystem>();
		await Expect.That(services.GetService<ITimeSystem>()).Is<RealTimeSystem>();
	}
}
