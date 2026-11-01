#if FEATURE_FILESYSTEM_HARDLINK
using System.IO;

namespace Testably.Abstractions.Tests.FileSystem.File;

[FileSystemTests]
public class CreateHardLinkTests(FileSystemTestData testData) : FileSystemTestBase(testData)
{
	[Test]
	[AutoArguments]
	public async Task CreateHardLink_CopyOntoLinkOfSource_ShouldThrowIOException(
		string path, string pathToTarget)
	{
		FileSystem.File.WriteAllText(pathToTarget, "foo");
		FileSystem.File.CreateHardLink(path, pathToTarget);

		void Act()
		{
			FileSystem.File.Copy(pathToTarget, path, true);
		}

		await That(Act).Throws<IOException>();
		await That(FileSystem.File.ReadAllText(path)).IsEqualTo("foo");
	}

	[Test]
	[AutoArguments]
	public async Task CreateHardLink_CopyOverwritingLink_ShouldOnlyChangeOtherLinkOnLinuxAndWindows(
		string path, string pathToTarget, string source)
	{
		FileSystem.File.WriteAllText(pathToTarget, "foo");
		FileSystem.File.CreateHardLink(path, pathToTarget);
		FileSystem.File.WriteAllText(source, "bar");

		FileSystem.File.Copy(source, path, true);

		await That(FileSystem.File.ReadAllText(path)).IsEqualTo("bar");
		await That(FileSystem.File.ReadAllText(pathToTarget))
			.IsEqualTo(Test.RunsOnMac ? "foo" : "bar")
			.Because("macOS clones the source to a new file instead of overwriting the destination");
	}

	[Test]
	[AutoArguments]
	public async Task CreateHardLink_MovingOntoOtherLinkOfSameFile_OnUnix_ShouldKeepBothLinks(
		string path, string pathToTarget)
	{
		Skip.If(Test.RunsOnWindows, "rename() does nothing for two links to the same file.");

		FileSystem.File.WriteAllText(pathToTarget, "foo");
		FileSystem.File.CreateHardLink(path, pathToTarget);

		FileSystem.File.Move(pathToTarget, path);

		await That(FileSystem.File.Exists(pathToTarget)).IsTrue();
		await That(FileSystem.File.ReadAllText(path)).IsEqualTo("foo");
	}

	[Test]
	[AutoArguments]
	public async Task CreateHardLink_MovingOntoOtherLinkOfSameFile_OnWindows_ShouldRemoveSource(
		string path, string pathToTarget)
	{
		Skip.IfNot(Test.RunsOnWindows, "rename() does nothing for two links to the same file.");

		FileSystem.File.WriteAllText(pathToTarget, "foo");
		FileSystem.File.CreateHardLink(path, pathToTarget);

		FileSystem.File.Move(pathToTarget, path);

		await That(FileSystem.File.Exists(pathToTarget)).IsFalse();
		await That(FileSystem.File.ReadAllText(path)).IsEqualTo("foo");
	}

	[Test]
	[AutoArguments]
	public async Task CreateHardLink_ReplacingOtherLinkOfSameFile_ShouldThrowIOException(
		string path, string pathToTarget)
	{
		FileSystem.File.WriteAllText(pathToTarget, "foo");
		FileSystem.File.CreateHardLink(path, pathToTarget);

		void Act()
		{
			FileSystem.File.Replace(pathToTarget, path, null);
		}

		await That(Act).Throws<IOException>()
			.WithHResult(Test.RunsOnWindows ? -2147024864 : -2146232800);
		await That(FileSystem.File.Exists(pathToTarget)).IsTrue();
		await That(FileSystem.File.Exists(path)).IsTrue();
	}

	[Test]
	[AutoArguments]
	public async Task CreateHardLink_TargetWithColon_ShouldThrowFileNotFoundException(
		string path)
	{
		void Act()
		{
			FileSystem.File.CreateHardLink(path, "foo:bar");
		}

		await That(Act).Throws<FileNotFoundException>()
			.WithMessage().Containing($"'{path}'").And
			.WithHResult(-2147024894);
	}

	[Test]
	[AutoArguments]
	public async Task CreateHardLink_TargetInMissingDirectory_ShouldThrowCorrectException(
		string path, string missingDirectory, string fileName)
	{
		string pathToTarget = FileSystem.Path.Combine(missingDirectory, fileName);

		void Act()
		{
			FileSystem.File.CreateHardLink(path, pathToTarget);
		}

		if (Test.RunsOnWindows)
		{
			await That(Act).Throws<DirectoryNotFoundException>()
				.WithMessage().Containing($"'{path}'").And
				.WithHResult(-2147024893);
		}
		else
		{
			await That(Act).Throws<FileNotFoundException>()
				.WithMessage().Containing($"'{path}'").And
				.WithHResult(-2147024894);
		}
	}

	[Test]
	[AutoArguments]
	public async Task CreateHardLink_TargetWithNullCharacter_ShouldThrowArgumentException(
		string path)
	{
		void Act()
		{
			FileSystem.File.CreateHardLink(path, "foo\0bar");
		}

		await That(Act).Throws<ArgumentException>()
			.WithParamName("pathToTarget").And
			.WithHResult(-2147024809);
	}

	[Test]
	[AutoArguments]
	public async Task CreateHardLink_MovingDirectoryOntoDirectoryWithLink_ShouldKeepBothLinks(
		string source, string destination)
	{
		FileSystem.Directory.CreateDirectory(source);
		FileSystem.Directory.CreateDirectory(destination);
		string pathToTarget = FileSystem.Path.Combine(source, "file.txt");
		string path = FileSystem.Path.Combine(destination, "file.txt");
		FileSystem.File.WriteAllText(pathToTarget, "foo");
		FileSystem.File.CreateHardLink(path, pathToTarget);

		void Act()
		{
			FileSystem.Directory.Move(source, destination);
		}

		await That(Act).Throws<IOException>();
		await That(FileSystem.File.ReadAllText(pathToTarget)).IsEqualTo("foo");
		await That(FileSystem.File.ReadAllText(path)).IsEqualTo("foo");
	}

	[Test]
	[Arguments("foo|bar")]
	[Arguments("foo<bar")]
	[Arguments("foo>bar")]
	[Arguments("foo\"bar")]
	public async Task CreateHardLink_TargetWithInvalidCharacter_OnWindows_ShouldThrowIOException(
		string pathToTarget)
	{
		Skip.IfNot(Test.RunsOnWindows, "Only Windows has invalid path characters besides '\\0'.");

		string path = "link.txt";

		void Act()
		{
			FileSystem.File.CreateHardLink(path, pathToTarget);
		}

		await That(Act).Throws<IOException>()
			.WithMessage().Containing($"'{path}'").And
			.WithHResult(-2147024773);
	}

	[Test]
	[Arguments("foo|bar")]
	[Arguments("foo<bar")]
	[Arguments("foo>bar")]
	[Arguments("foo\"bar")]
	public async Task CreateHardLink_PathWithInvalidCharacter_OnWindows_ShouldThrowIOException(
		string path)
	{
		Skip.IfNot(Test.RunsOnWindows, "Only Windows has invalid path characters besides '\\0'.");

		string pathToTarget = "target.txt";
		FileSystem.File.WriteAllText(pathToTarget, "foo");

		void Act()
		{
			FileSystem.File.CreateHardLink(path, pathToTarget);
		}

		await That(Act).Throws<IOException>()
			.WithMessage().Containing(path).And
			.WithHResult(-2147024773);
	}

	[Test]
	[AutoArguments]
	public async Task CreateHardLink_MovingLink_ShouldKeepSharingContent(
		string path, string pathToTarget, string newPath)
	{
		FileSystem.File.WriteAllText(pathToTarget, "foo");
		FileSystem.File.CreateHardLink(path, pathToTarget);

		FileSystem.File.Move(path, newPath);
		FileSystem.File.AppendAllText(newPath, "bar");

		await That(FileSystem.File.Exists(path)).IsFalse();
		await That(FileSystem.File.ReadAllText(pathToTarget)).IsEqualTo("foobar");
	}

	[Test]
	[AutoArguments]
	public async Task CreateHardLink_ReplacingLink_ShouldKeepContentOfOtherLink(
		string path, string pathToTarget, string source)
	{
		FileSystem.File.WriteAllText(pathToTarget, "foo");
		FileSystem.File.CreateHardLink(path, pathToTarget);
		FileSystem.File.WriteAllText(source, "bar");

		FileSystem.File.Replace(source, path, null);

		await That(FileSystem.File.ReadAllText(path)).IsEqualTo("bar");
		await That(FileSystem.File.ReadAllText(pathToTarget)).IsEqualTo("foo");
	}

	[Test]
	[AutoArguments]
	public async Task CreateHardLink_PathExistsAndTargetIsDirectory_OnLinux_ShouldThrowIOException(
		string path, string pathToTarget)
	{
		Skip.IfNot(Test.RunsOnLinux, "Linux checks the new name before the type of the target.");

		FileSystem.Directory.CreateDirectory(pathToTarget);
		FileSystem.File.WriteAllText(path, "foo");

		void Act()
		{
			FileSystem.File.CreateHardLink(path, pathToTarget);
		}

		await That(Act).Throws<IOException>().WithHResult(17);
	}

	[Test]
	[AutoArguments]
	public async Task
		CreateHardLink_PathExistsAndTargetIsDirectory_ShouldThrowUnauthorizedAccessException(
			string path, string pathToTarget)
	{
		Skip.If(Test.RunsOnLinux, "Linux checks the new name before the type of the target.");

		FileSystem.Directory.CreateDirectory(pathToTarget);
		FileSystem.File.WriteAllText(path, "foo");

		void Act()
		{
			FileSystem.File.CreateHardLink(path, pathToTarget);
		}

		await That(Act).Throws<UnauthorizedAccessException>().WithHResult(-2147024891);
	}

	[Test]
	[AutoArguments]
	public async Task CreateHardLink_WritingThroughStreamOfDeletedLink_ShouldChangeOtherLink(
		string path, string pathToTarget)
	{
		FileSystem.File.WriteAllText(pathToTarget, "foo");
		FileSystem.File.CreateHardLink(path, pathToTarget);

		using (FileSystemStream stream = FileSystem.File.Open(path, FileMode.Open,
			FileAccess.Write, FileShare.ReadWrite | FileShare.Delete))
		{
			FileSystem.File.Delete(path);
			stream.Write("bar"u8.ToArray(), 0, 3);
		}

		await That(FileSystem.File.Exists(path)).IsFalse();
		await That(FileSystem.File.ReadAllText(pathToTarget)).IsEqualTo("bar");
	}

	[Test]
	[AutoArguments]
	public async Task CreateHardLink_DeletingTarget_ShouldKeepContentOfLink(
		string path, string pathToTarget, string contents)
	{
		FileSystem.File.WriteAllText(pathToTarget, contents);
		FileSystem.File.CreateHardLink(path, pathToTarget);

		FileSystem.File.Delete(pathToTarget);

		await That(FileSystem.File.Exists(pathToTarget)).IsFalse();
		await That(FileSystem.File.ReadAllText(path)).IsEqualTo(contents);
	}

	[Test]
	[AutoArguments]
	public async Task CreateHardLink_EmptyPath_ShouldThrowArgumentException(string pathToTarget)
	{
		FileSystem.File.WriteAllText(pathToTarget, null);

		void Act()
		{
			FileSystem.File.CreateHardLink("", pathToTarget);
		}

		await That(Act).Throws<ArgumentException>().WithParamName("path");
	}

	[Test]
	[AutoArguments]
	public async Task CreateHardLink_EmptyPathToTarget_ShouldThrowArgumentException(string path)
	{
		void Act()
		{
			FileSystem.File.CreateHardLink(path, "");
		}

		await That(Act).Throws<ArgumentException>().WithParamName("pathToTarget");
	}

	[Test]
	[AutoArguments]
	public async Task CreateHardLink_FileOpenedWithoutSharingViaLink_ShouldBlockTarget(
		string path, string pathToTarget)
	{
		FileSystem.File.WriteAllText(pathToTarget, "foo");
		FileSystem.File.CreateHardLink(path, pathToTarget);
		using FileSystemStream stream = FileSystem.File.Open(path, FileMode.Open,
			FileAccess.Read, FileShare.None);

		void Act()
		{
			using FileSystemStream _ = FileSystem.File.OpenRead(pathToTarget);
		}

		await That(Act).Throws<IOException>()
			.Because("both paths refer to the same file, which is opened without sharing");
	}

	[Test]
	[AutoArguments]
	public async Task CreateHardLink_MissingParentDirectory_ShouldThrowDirectoryNotFoundException(
		string missingDirectory, string fileName, string pathToTarget)
	{
		FileSystem.File.WriteAllText(pathToTarget, null);
		string path = FileSystem.Path.Combine(missingDirectory, fileName);

		void Act()
		{
			FileSystem.File.CreateHardLink(path, pathToTarget);
		}

		await That(Act).Throws<DirectoryNotFoundException>()
			.WithMessage().Containing($"'{path}'").And
			.WithHResult(-2147024893);
	}

	[Test]
	[AutoArguments]
	public async Task CreateHardLink_NullPath_ShouldThrowArgumentNullException(string pathToTarget)
	{
		void Act()
		{
			FileSystem.File.CreateHardLink(null!, pathToTarget);
		}

		await That(Act).Throws<ArgumentNullException>().WithParamName("path");
	}

	[Test]
	[AutoArguments]
	public async Task CreateHardLink_NullPathToTarget_ShouldThrowArgumentNullException(
		string path)
	{
		void Act()
		{
			FileSystem.File.CreateHardLink(path, null!);
		}

		await That(Act).Throws<ArgumentNullException>().WithParamName("pathToTarget");
	}

	[Test]
	[AutoArguments]
	public async Task CreateHardLink_PathAlreadyExists_ShouldThrowIOException(
		string path, string pathToTarget)
	{
		FileSystem.File.WriteAllText(pathToTarget, null);
		FileSystem.File.WriteAllText(path, "foo");

		void Act()
		{
			FileSystem.File.CreateHardLink(path, pathToTarget);
		}

		await That(Act).Throws<IOException>()
			.WithMessage().Containing($"'{path}'").And
			.WithHResult(Test.RunsOnWindows ? -2147024713 : 17);
		await That(FileSystem.File.ReadAllText(path)).IsEqualTo("foo");
	}

	[Test]
	[AutoArguments]
	public async Task CreateHardLink_PathIsExistingDirectory_ShouldThrowIOException(
		string path, string pathToTarget)
	{
		FileSystem.File.WriteAllText(pathToTarget, null);
		FileSystem.Directory.CreateDirectory(path);

		void Act()
		{
			FileSystem.File.CreateHardLink(path, pathToTarget);
		}

		await That(Act).Throws<IOException>()
			.WithMessage().Containing($"'{path}'").And
			.WithHResult(Test.RunsOnWindows ? -2147024713 : 17);
	}

	[Test]
	[AutoArguments]
	public async Task CreateHardLink_ShouldReturnExistingFileWithoutLinkTarget(
		string path, string pathToTarget)
	{
		FileSystem.File.WriteAllText(pathToTarget, "foo");

		IFileSystemInfo result = FileSystem.File.CreateHardLink(path, pathToTarget);

		await That(result).Is<IFileInfo>();
		await That(result.Exists).IsTrue();
		await That(result.FullName).IsEqualTo(FileSystem.Path.GetFullPath(path));
		await That(result.LinkTarget).IsNull()
			.Because("a hard link is no reparse point that refers to another path");
		await That(result.Attributes).DoesNotHaveFlag(FileAttributes.ReparsePoint);
		await That(FileSystem.File.ResolveLinkTarget(path, false)).IsNull();
	}

	[Test]
	[AutoArguments]
	public async Task CreateHardLink_ShouldShareAttributes(string path, string pathToTarget)
	{
		FileSystem.File.WriteAllText(pathToTarget, null);
		FileSystem.File.CreateHardLink(path, pathToTarget);

		FileSystem.File.SetAttributes(path, FileAttributes.ReadOnly);
		FileAttributes result = FileSystem.File.GetAttributes(pathToTarget);
		FileSystem.File.SetAttributes(path, FileAttributes.Normal);

		await That(result).HasFlag(FileAttributes.ReadOnly);
	}

	[Test]
	[AutoArguments]
	public async Task CreateHardLink_ShouldShareContent(
		string path, string pathToTarget, string contents1, string contents2)
	{
		FileSystem.File.WriteAllText(pathToTarget, contents1);

		FileSystem.File.CreateHardLink(path, pathToTarget);
		string initialContent = FileSystem.File.ReadAllText(path);
		FileSystem.File.AppendAllText(path, contents2);

		await That(initialContent).IsEqualTo(contents1);
		await That(FileSystem.File.ReadAllText(pathToTarget)).IsEqualTo(contents1 + contents2);
	}

	[Test]
	[AutoArguments]
	public async Task CreateHardLink_ShouldShareLastWriteTime(string path, string pathToTarget)
	{
		DateTime lastWriteTime = new(2019, 1, 1, 12, 0, 0, DateTimeKind.Utc);
		FileSystem.File.WriteAllText(pathToTarget, null);
		FileSystem.File.CreateHardLink(path, pathToTarget);

		FileSystem.File.SetLastWriteTimeUtc(path, lastWriteTime);

		await That(FileSystem.File.GetLastWriteTimeUtc(pathToTarget)).IsEqualTo(lastWriteTime);
	}

	[Test]
	[AutoArguments]
	public async Task CreateHardLink_TargetIsDirectory_ShouldThrowUnauthorizedAccessException(
		string path, string pathToTarget)
	{
		FileSystem.Directory.CreateDirectory(pathToTarget);

		void Act()
		{
			FileSystem.File.CreateHardLink(path, pathToTarget);
		}

		await That(Act).Throws<UnauthorizedAccessException>()
			.WithMessage().Containing($"'{path}'").And
			.WithHResult(-2147024891);
		await That(FileSystem.File.Exists(path)).IsFalse();
	}

	[Test]
	[AutoArguments]
	public async Task CreateHardLink_TargetMissing_ShouldThrowFileNotFoundException(
		string path, string pathToTarget)
	{
		void Act()
		{
			FileSystem.File.CreateHardLink(path, pathToTarget);
		}

		await That(Act).Throws<FileNotFoundException>()
			.WithMessage().Containing($"'{path}'").And
			.WithHResult(-2147024894);
		await That(FileSystem.File.Exists(path)).IsFalse();
	}
}
#endif
