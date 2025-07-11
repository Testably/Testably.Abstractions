using Testably.Abstractions.RandomSystem;
using Testably.Abstractions.Testing.Helpers;

namespace Testably.Abstractions.Testing.Tests.Helpers;

public class FileSystemExtensionsTests
{
	[Theory]
	[AutoData]
	public async Task GetMoveLocation_LocationNotUnderSource_ShouldThrowNotSupportedException(
		string location, string source, string destination)
	{
		MockFileSystem sut = new();

		void Act()
		{
			sut.GetMoveLocation(
				sut.Storage.GetLocation(location),
				sut.Storage.GetLocation(source),
				sut.Storage.GetLocation(destination));
		}

		await That(Act).ThrowsExactly<NotSupportedException>()
			.Whose(x => x.Message, it => it
				.Contains($"'{sut.Path.GetFullPath(location)}'").And
				.Contains($"'{sut.Path.GetFullPath(source)}'"));
	}

	[Fact]
	public async Task RandomOrDefault_WithMockFileSystem_ShouldUseRandomFromRandomSystem()
	{
		MockFileSystem fileSystem = new();
		IFileSystem sut = fileSystem;

		IRandom result = sut.RandomOrDefault();

		await That(result).IsEqualTo(fileSystem.RandomSystem.Random.Shared);
	}

	[Fact]
	public async Task RandomOrDefault_WithRealFileSystem_ShouldUseSharedRandom()
	{
		RealFileSystem fileSystem = new();
		IFileSystem sut = fileSystem;

		IRandom result = sut.RandomOrDefault();

		await That(result).IsEqualTo(RandomFactory.Shared);
	}
}
