using Microsoft.Extensions.DependencyInjection;

namespace Testably.Abstractions.Tests;

public class DependencyInjectionTests
{
	[Fact]
	[Trait(nameof(Testing), nameof(IServiceProvider))]
	public void FileSystem_ShouldHaveDefaultConstructorForDependencyInjection()
	{
		ServiceCollection services = new();
		services.AddSingleton<IFileSystem, Abstractions.FileSystem>();
		ServiceProvider serviceProvider = services.BuildServiceProvider();

		IFileSystem result = serviceProvider.GetRequiredService<IFileSystem>();

		result.Should().BeOfType<Abstractions.FileSystem>();
	}

	[Fact]
	[Trait(nameof(Testing), nameof(IServiceProvider))]
	public void RandomSystem_ShouldHaveDefaultConstructorForDependencyInjection()
	{
		ServiceCollection services = new();
		services.AddSingleton<IRandomSystem, Abstractions.RandomSystem>();
		ServiceProvider serviceProvider = services.BuildServiceProvider();

		IRandomSystem result = serviceProvider.GetRequiredService<IRandomSystem>();

		result.Should().BeOfType<Abstractions.RandomSystem>();
	}

	[Fact]
	[Trait(nameof(Testing), nameof(IServiceProvider))]
	public void TimeSystem_ShouldHaveDefaultConstructorForDependencyInjection()
	{
		ServiceCollection services = new();
		services.AddSingleton<ITimeSystem, Abstractions.TimeSystem>();
		ServiceProvider serviceProvider = services.BuildServiceProvider();

		ITimeSystem result = serviceProvider.GetRequiredService<ITimeSystem>();

		result.Should().BeOfType<Abstractions.TimeSystem>();
	}
}