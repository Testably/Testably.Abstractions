using Microsoft.Extensions.DependencyInjection;

namespace Testably.Abstractions.Tests.Testing;

public class DependencyInjectionTests
{
	[Fact]
	[Trait(nameof(Testing), nameof(IServiceProvider))]
	public void FileSystem_ShouldHaveDefaultConstructorForDependencyInjection()
	{
		ServiceCollection services = new();
		services.AddSingleton<IFileSystem, FileSystem>();
		ServiceProvider serviceProvider = services.BuildServiceProvider();

		IFileSystem result = serviceProvider.GetRequiredService<IFileSystem>();

		result.Should().BeOfType<FileSystem>();
	}

	[Fact]
	[Trait(nameof(Testing), nameof(IServiceProvider))]
	public void RandomSystem_ShouldHaveDefaultConstructorForDependencyInjection()
	{
		ServiceCollection services = new();
		services.AddSingleton<IRandomSystem, RandomSystem>();
		ServiceProvider serviceProvider = services.BuildServiceProvider();

		IRandomSystem result = serviceProvider.GetRequiredService<IRandomSystem>();

		result.Should().BeOfType<RandomSystem>();
	}

	[Fact]
	[Trait(nameof(Testing), nameof(IServiceProvider))]
	public void TimeSystem_ShouldHaveDefaultConstructorForDependencyInjection()
	{
		ServiceCollection services = new();
		services.AddSingleton<ITimeSystem, TimeSystem>();
		ServiceProvider serviceProvider = services.BuildServiceProvider();

		ITimeSystem result = serviceProvider.GetRequiredService<ITimeSystem>();

		result.Should().BeOfType<TimeSystem>();
	}
}