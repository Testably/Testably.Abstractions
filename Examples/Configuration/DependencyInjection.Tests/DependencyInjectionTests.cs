using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Testably.Abstractions;
using Xunit;

namespace DependencyInjection.Tests;

public class DependencyInjectionTests
{
	[Fact]
	public void DependencyInjection_Microsoft_ShouldAllowRegistrationAndCreationOfInstances()
	{
		ServiceProvider services = new ServiceCollection()
			.AddSingleton<IFileSystem, FileSystem>()
			.AddSingleton<IRandomSystem, RandomSystem>()
			.AddSingleton<ITimeSystem, TimeSystem>()
			.BuildServiceProvider();

		services.GetService<IFileSystem>().Should().BeOfType<FileSystem>();
		services.GetService<IRandomSystem>().Should().BeOfType<RandomSystem>();
		services.GetService<ITimeSystem>().Should().BeOfType<TimeSystem>();
	}
}