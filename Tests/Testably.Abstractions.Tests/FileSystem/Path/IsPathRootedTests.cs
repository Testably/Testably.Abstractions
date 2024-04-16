namespace Testably.Abstractions.Tests.FileSystem.Path;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class IsPathRootedTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[MemberData(nameof(TestData))]
	public void IsPathRooted_ShouldReturnDefaultValue(string path, TestOs isRootedOn)
	{
		bool result = FileSystem.Path.IsPathRooted(path);

		result.Should().Be(Test.RunsOn(isRootedOn));
	}

#if FEATURE_SPAN
	[SkippableTheory]
	[MemberData(nameof(TestData))]
	public void IsPathRooted_Span_ShouldReturnDefaultValue(string path, TestOs isRootedOn)
	{
		bool result = FileSystem.Path.IsPathRooted(path.AsSpan());

		result.Should().Be(Test.RunsOn(isRootedOn));
	}
#endif

	#region Helpers

	public static TheoryData<string, TestOs> TestData()
	{
		return new TheoryData<string, TestOs>
		{
			{
				"", TestOs.None
			},
			{
				"/", TestOs.All
			},
			{
				@"\", TestOs.Windows
			},
			{
				"/foo", TestOs.All
			},
			{
				@"\foo", TestOs.Windows
			},
			{
				"foo/bar", TestOs.None
			},
			{
				"a:", TestOs.Windows
			},
			{
				"z:", TestOs.Windows
			},
			{
				"A:", TestOs.Windows
			},
			{
				"Z:", TestOs.Windows
			},
			{
				"@:", TestOs.Framework
			},
			{
				"[:", TestOs.Framework
			},
			{
				"`:", TestOs.Framework
			},
			{
				"{:", TestOs.Framework
			},
		};
	}

	#endregion
}
