using aweXpect;
using System.IO;
using System.Threading.Tasks;
using Testably.Abstractions.Testing;
using Xunit;

namespace Testably.Abstractions.Examples.AccessControlLists.Tests;

public class AccessControlListTests
{
	#region Test Setup

	public MockFileSystem FileSystem { get; }

	public AccessControlListTests()
	{
		FileSystem = new MockFileSystem();
	}

	#endregion

	[Theory]
	[InlineData("granted", "denied")]
	public async Task ReadAllText_DeniedPath_ShouldThrowIOException(string grantedPath,
		string deniedPath)
	{
		FileSystem.File.WriteAllText(grantedPath, "foo");
		FileSystem.File.WriteAllText(deniedPath, "bar");
		FileSystem.WithAccessControlStrategy(
			new CustomAccessControlStrategy(path => path.Contains(grantedPath)));

		string result = FileSystem.File.ReadAllText(grantedPath);
		await Expect.That(result).IsEqualTo("foo");

		void Act()
		{
			FileSystem.File.ReadAllText(deniedPath);
		}

		await Expect.That(Act).Throws<IOException>()
			.WithMessage(
				$"Access to the path '{FileSystem.Path.GetFullPath(deniedPath)}' is denied.");
	}
}
