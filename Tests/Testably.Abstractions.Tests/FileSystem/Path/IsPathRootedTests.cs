namespace Testably.Abstractions.Tests.FileSystem.Path;

[FileSystemTests]
public partial class IsPathRootedTests
{
	[Theory]
	[MemberData(nameof(TestData))]
	public async Task IsPathRooted_ShouldReturnDefaultValue(string path, TestOS isRootedOn)
	{
		bool result = FileSystem.Path.IsPathRooted(path);

		await That(result).IsEqualTo(Test.RunsOn(isRootedOn));
	}

#if FEATURE_SPAN
	[Theory]
	[MemberData(nameof(TestData))]
	public async Task IsPathRooted_Span_ShouldReturnDefaultValue(string path, TestOS isRootedOn)
	{
		bool result = FileSystem.Path.IsPathRooted(path.AsSpan());

		await That(result).IsEqualTo(Test.RunsOn(isRootedOn));
	}
#endif

	#region Helpers

	public static TheoryData<string, TestOS> TestData()
	{
		return new TheoryData<string, TestOS>
		{
			{
				"", TestOS.None
			},
			{
				"/", TestOS.All
			},
			{
				@"\", TestOS.Windows | TestOS.Framework
			},
			{
				"/foo", TestOS.All
			},
			{
				@"\foo", TestOS.Windows | TestOS.Framework
			},
			{
				"foo/bar", TestOS.None
			},
			{
				"a:", TestOS.Windows | TestOS.Framework
			},
			{
				"z:", TestOS.Windows | TestOS.Framework
			},
			{
				"A:", TestOS.Windows | TestOS.Framework
			},
			{
				"Z:", TestOS.Windows | TestOS.Framework
			},
			{
				"@:", TestOS.Framework
			},
			{
				"[:", TestOS.Framework
			},
			{
				"`:", TestOS.Framework
			},
			{
				"{:", TestOS.Framework
			},
		};
	}

	#endregion
}
