using aweXpect;
using System;
using System.Threading.Tasks;
using Testably.Abstractions.Testing;
using Xunit;

namespace Testably.Abstractions.Examples.Configuration.FileSystemConfiguration.Tests;

public class InterceptionTests
{
	/// <summary>
	///     Intercepting allows callbacks to be invoked before the change in the file system is performed.
	/// </summary>
	[Fact]
	public async Task Intercept()
	{
		Exception customException = new ApplicationException("bar");
		MockFileSystem fileSystem = new();
		fileSystem.Intercept.Creating(FileSystemTypes.File,
			_ => throw customException);

		fileSystem.Directory.CreateDirectory("foo");

		Exception? exception = Record.Exception(() =>
		{
			fileSystem.File.Create("foo/bar.txt");
		});

		await Expect.That(exception).IsSameAs(customException);
		await Expect.That(fileSystem.File.Exists("foo/bar.txt")).IsFalse();
	}
}
