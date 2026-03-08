using System.IO;
using Testably.Abstractions.Testing.Helpers;

namespace Testably.Abstractions.Testing.Tests.Helpers;

public class PathHelperTests
{
	[Test]
	public async Task
		EnsureValidFormat_WithWhiteSpaceAndIncludeIsEmptyCheck_ShouldThrowArgumentException()
	{
		string whiteSpace = " ";
		MockFileSystem fileSystem = new();

		void Act()
		{
			whiteSpace.EnsureValidFormat(fileSystem, "foo", true);
		}

		await That(Act).ThrowsExactly<ArgumentException>().WithHResult(-2147024809);
	}

	[Test]
	[AutoArguments]
	public async Task GetFullPathOrWhiteSpace_NormalPath_ShouldReturnFullPath(string path)
	{
		MockFileSystem fileSystem = new();
		fileSystem.Initialize();
		string expectedPath = fileSystem.Path.GetFullPath(path);

		string result = path.GetFullPathOrWhiteSpace(fileSystem);

		await That(result).IsEqualTo(expectedPath);
	}

	[Test]
	public async Task GetFullPathOrWhiteSpace_Null_ShouldReturnEmptyString()
	{
		MockFileSystem fileSystem = new();
		string? sut = null;

		string result = sut.GetFullPathOrWhiteSpace(fileSystem);

		await That(result).IsEqualTo("");
	}

	[Test]
	[Arguments("  ")]
	[Arguments("\t")]
	public async Task GetFullPathOrWhiteSpace_WhiteSpace_ShouldReturnPath(string path)
	{
		MockFileSystem fileSystem = new();

		string result = path.GetFullPathOrWhiteSpace(fileSystem);

		await That(result).IsEqualTo(path);
	}

	[Test]
	[AutoArguments]
	public async Task IsUncPath_AltDirectorySeparatorChar_ShouldReturnTrue(string path)
	{
		string prefix = new(Path.AltDirectorySeparatorChar, 2);
		path = prefix + path;

		bool result = path.IsUncPath(new MockFileSystem());

		await That(result).IsTrue();
	}

	[Test]
	[AutoArguments]
	public async Task IsUncPath_DirectorySeparatorChar_ShouldReturnTrue(string path)
	{
		string prefix = new(Path.DirectorySeparatorChar, 2);
		path = prefix + path;

		bool result = path.IsUncPath(new MockFileSystem());

		await That(result).IsTrue();
	}

	[Test]
	[AutoArguments]
	public async Task IsUncPath_MixedDirectorySeparatorChars_ShouldReturnFalse(string path)
	{
		Skip.IfNot(Test.RunsOnWindows,
			"Mac and Linux don't have distinctive directory separator chars.");

		path = $"{Path.AltDirectorySeparatorChar}{Path.DirectorySeparatorChar}{path}";

		bool result = path.IsUncPath(new MockFileSystem());

		await That(result).IsFalse();
	}

	[Test]
	public async Task IsUncPath_Null_ShouldReturnFalse()
	{
		string? path = null;

		bool result = path!.IsUncPath(new MockFileSystem());

		await That(result).IsFalse();
	}

	[Test]
	public async Task
		ThrowCommonExceptionsIfPathIsInvalid_StartWithNull_ShouldThrowArgumentException()
	{
		string path = "\0foo";

		void Act()
		{
			path.EnsureValidFormat(new MockFileSystem());
		}

		await That(Act).ThrowsExactly<ArgumentException>().WithMessage($"*'{path}'*").AsWildcard();
	}

#if CAN_SIMULATE_OTHER_OS
	[Test]
	[Arguments('|')]
	[Arguments((char)1)]
	[Arguments((char)31)]
	public async Task ThrowCommonExceptionsIfPathIsInvalid_WithInvalidCharacters(
		char invalidChar)
	{
		MockFileSystem fileSystem = new(o => o
			.SimulatingOperatingSystem(SimulationMode.Windows));
		string path = invalidChar + "path";

		void Act()
		{
			path.EnsureValidFormat(fileSystem);
		}

		await That(Act).ThrowsExactly<IOException>().WithMessage($"*{path}*").AsWildcard();
	}
#endif

	[Test]
	public async Task
		ThrowCommonExceptionsIfPathToTargetIsInvalid_NullCharacter_ShouldThrowArgumentException()
	{
		MockFileSystem fileSystem = new();
		string path = "path-with\0 invalid character";

		void Act()
		{
			path.ThrowCommonExceptionsIfPathToTargetIsInvalid(fileSystem);
		}

		await That(Act).ThrowsExactly<ArgumentException>().WithMessage($"*'{path}'*").AsWildcard();
	}
}
