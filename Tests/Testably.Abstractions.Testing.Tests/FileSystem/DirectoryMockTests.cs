using System.IO;
using Testably.Abstractions.Testing.FileSystem;

namespace Testably.Abstractions.Testing.Tests.FileSystem;

public class DirectoryMockTests
{
	[Test]
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
			.WithMessage().Containing($"'{sut.FullName}'").And
			.WithHResult(-2147024891);
	}

	[Test]
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
			.WithMessage().Containing($"'{sut.FullName}'").And
			.WithHResult(-2147024891);
	}

	[Test]
	public async Task EnumerateDirectories_UnauthorizedSubdirectory_ShouldBeListed()
	{
		Skip.IfNot(Test.RunsOnWindows);

		MockFileSystem fileSystem = new();
		fileSystem.Initialize()
			.WithSubdirectory("root").Initialized(d => d
				.WithSubdirectory("foo")
				.WithSubdirectory("bar"));
		fileSystem.WithAccessControlStrategy(
			new DefaultAccessControlStrategy((p, _)
				=> !p.EndsWith("foo", StringComparison.Ordinal)));

		string[] result = fileSystem.Directory.GetDirectories("root");

		await That(result).IsEqualTo([
			fileSystem.Path.Combine("root", "foo"),
			fileSystem.Path.Combine("root", "bar"),
		]).InAnyOrder();
	}

#if FEATURE_FILESYSTEM_ENUMERATION_OPTIONS
	[Test]
	public async Task
		EnumerateFiles_RecursiveWithIgnoreInaccessible_ShouldSkipContentOfUnauthorizedSubdirectory()
	{
		Skip.IfNot(Test.RunsOnWindows);

		MockFileSystem fileSystem = new();
		fileSystem.Initialize()
			.WithSubdirectory("root").Initialized(d => d
				.WithSubdirectory("foo").Initialized(f => f.WithFile("denied.txt"))
				.WithSubdirectory("bar").Initialized(b => b.WithFile("allowed.txt")));
		fileSystem.WithAccessControlStrategy(
			new DefaultAccessControlStrategy((p, _)
				=> !p.EndsWith("foo", StringComparison.Ordinal)));

		string[] result = fileSystem.Directory.GetFiles("root", "*", new EnumerationOptions
		{
			RecurseSubdirectories = true,
			IgnoreInaccessible = true,
		});

		await That(result).IsEqualTo([fileSystem.Path.Combine("root", "bar", "allowed.txt")]);
	}
#endif

	[Test]
	public async Task
		EnumerateFiles_RecursiveWithUnauthorizedSubdirectory_ShouldThrowUnauthorizedAccessException()
	{
		Skip.IfNot(Test.RunsOnWindows);

		MockFileSystem fileSystem = new();
		fileSystem.Initialize()
			.WithSubdirectory("root").Initialized(d => d
				.WithSubdirectory("foo"));
		fileSystem.WithAccessControlStrategy(
			new DefaultAccessControlStrategy((p, _)
				=> !p.EndsWith("foo", StringComparison.Ordinal)));

		void Act() =>
			_ = fileSystem.Directory.GetFiles("root", "*", SearchOption.AllDirectories);

		await That(Act).Throws<UnauthorizedAccessException>()
			.WithMessage().Containing($"'{fileSystem.Path.GetFullPath(fileSystem.Path.Combine("root", "foo"))}'").And
			.WithHResult(-2147024891);
	}

	[Test]
	public async Task EnumerateFiles_UnauthorizedFile_ShouldBeListedButNotReadable()
	{
		Skip.IfNot(Test.RunsOnWindows);

		MockFileSystem fileSystem = new();
		fileSystem.Initialize()
			.WithSubdirectory("root").Initialized(d => d
				.WithFile("myFile1.txt")
				.WithFile("myFile2.txt"));
		fileSystem.WithAccessControlStrategy(
			new DefaultAccessControlStrategy((p, _)
				=> !p.EndsWith("myFile2.txt", StringComparison.Ordinal)));

		string[] result = fileSystem.Directory.GetFiles("root");

		void Act() =>
			_ = fileSystem.File.ReadAllBytes(fileSystem.Path.Combine("root", "myFile2.txt"));

		await That(result).IsEqualTo([
			fileSystem.Path.Combine("root", "myFile1.txt"),
			fileSystem.Path.Combine("root", "myFile2.txt"),
		]).InAnyOrder();
		await That(Act).Throws<UnauthorizedAccessException>();
	}

	[Test]
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
			.WithMessage().Containing($"'{sut.FullName}'").And
			.WithHResult(-2147024891);
	}
}
