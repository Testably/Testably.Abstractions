using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Testably.Abstractions;
using Xunit;

namespace DependencyInjection.Tests;

public class DependencyInjectionTests
{
	[Fact]
	public void
		DependencyInjection_Microsoft_ShouldAllowRegistrationAndCreationOfInstances()
	{
		ServiceProvider services = new ServiceCollection()
			.AddSingleton<IFileSystem, RealFileSystem>()
			.AddSingleton<IRandomSystem, RealRandomSystem>()
			.AddSingleton<ITimeSystem, RealTimeSystem>()
			.BuildServiceProvider();

		services.GetService<IFileSystem>().Should().BeOfType<RealFileSystem>();
		services.GetService<IRandomSystem>().Should().BeOfType<RealRandomSystem>();
		services.GetService<ITimeSystem>().Should().BeOfType<RealTimeSystem>();
	}
}
