using Testably.Abstractions.Testing.FileSystem;

namespace Testably.Abstractions.Testing.Tests.FileSystem;

public class DirectoryMockTests
{
	[Fact]
	public async Task
		EnumerateDirectories_UnauthorizedParentAccess_ShouldThrowUnauthorizedAccessExceptionImmediately()
	{
		Skip.IfNot(Test.RunsOnWindows);

		string path = "foo";
		MockFileSystem fileSystem = new();
		IDirectoryInfo sut = fileSystem.Directory.CreateDirectory(path);
		fileSystem.WithAccessControlStrategy(
			new DefaultAccessControlStrategy((p, _)
				=> !p.EndsWith(path, StringComparison.Ordinal)));

		void Act() =>
			_ = fileSystem.Directory.EnumerateDirectories(path);

		await That(Act).Throws<UnauthorizedAccessException>()
			.WithMessageContaining($"'{sut.FullName}'").And
			.WithHResult(-2147024891);
	}

	[Fact]
	public async Task
		EnumerateFiles_UnauthorizedParentAccess_ShouldThrowUnauthorizedAccessExceptionImmediately()
	{
		Skip.IfNot(Test.RunsOnWindows);

		string path = "foo";
		MockFileSystem fileSystem = new();
		IDirectoryInfo sut = fileSystem.Directory.CreateDirectory(path);
		fileSystem.WithAccessControlStrategy(
			new DefaultAccessControlStrategy((p, _)
				=> !p.EndsWith(path, StringComparison.Ordinal)));

		void Act() =>
			_ = fileSystem.Directory.EnumerateFiles(path);

		await That(Act).Throws<UnauthorizedAccessException>()
			.WithMessageContaining($"'{sut.FullName}'").And
			.WithHResult(-2147024891);
	}

	[Fact]
	public async Task
		EnumerateFileSystemEntries_UnauthorizedParentAccess_ShouldThrowUnauthorizedAccessExceptionImmediately()
	{
		Skip.IfNot(Test.RunsOnWindows);

		string path = "foo";
		MockFileSystem fileSystem = new();
		IDirectoryInfo sut = fileSystem.Directory.CreateDirectory(path);
		fileSystem.WithAccessControlStrategy(
			new DefaultAccessControlStrategy((p, _)
				=> !p.EndsWith(path, StringComparison.Ordinal)));

		void Act() =>
			_ = fileSystem.Directory.EnumerateFileSystemEntries(path);

		await That(Act).Throws<UnauthorizedAccessException>()
			.WithMessageContaining($"'{sut.FullName}'").And
			.WithHResult(-2147024891);
	}
}
