using System.Collections.Generic;

namespace Testably.Abstractions.Tests.FileSystem.Path;

[FileSystemTests]
public class IsPathRootedTests(FileSystemTestData testData) : FileSystemTestBase(testData)
{
	[Test]
	[MethodDataSource(nameof(TestData))]
	public async Task IsPathRooted_ShouldReturnDefaultValue(string path, TestOS isRootedOn)
	{
		bool result = FileSystem.Path.IsPathRooted(path);

		await That(result).IsEqualTo(Test.RunsOn(isRootedOn));
	}

#if FEATURE_SPAN
	[Test]
	[MethodDataSource(nameof(TestData))]
	public async Task IsPathRooted_Span_ShouldReturnDefaultValue(string path, TestOS isRootedOn)
	{
		bool result = FileSystem.Path.IsPathRooted(path.AsSpan());

		await That(result).IsEqualTo(Test.RunsOn(isRootedOn));
	}
#endif

	#region Helpers

	public static IEnumerable<(string, TestOS)> TestData()
	{
		yield return ("", TestOS.None);
		yield return ("/", TestOS.All);
		yield return (@"\", TestOS.Windows | TestOS.Framework);
		yield return ("/foo", TestOS.All);
		yield return (@"\foo", TestOS.Windows | TestOS.Framework);
		yield return ("foo/bar", TestOS.None);
		yield return ("a:", TestOS.Windows | TestOS.Framework);
		yield return ("z:", TestOS.Windows | TestOS.Framework);
		yield return ("A:", TestOS.Windows | TestOS.Framework);
		yield return ("Z:", TestOS.Windows | TestOS.Framework);
		yield return ("@:", TestOS.Framework);
		yield return ("[:", TestOS.Framework);
		yield return ("`:", TestOS.Framework);
		yield return ("{:", TestOS.Framework);
	}

	#endregion
}
