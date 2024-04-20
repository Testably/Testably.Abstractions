namespace Testably.Abstractions.Tests.FileSystem.Path;

// ReSharper disable once PartialTypeWithSinglePart
public abstract partial class IsPathRootedTests<TFileSystem>
	: FileSystemTestBase<TFileSystem>
	where TFileSystem : IFileSystem
{
	[SkippableTheory]
	[MemberData(nameof(TestData))]
	public void IsPathRooted_ShouldReturnDefaultValue(string path, TestOS isRootedOn)
	{
		bool result = FileSystem.Path.IsPathRooted(path);

		result.Should().Be(Test.RunsOn(isRootedOn));
	}

#if FEATURE_SPAN
	[SkippableTheory]
	[MemberData(nameof(TestData))]
	public void IsPathRooted_Span_ShouldReturnDefaultValue(string path, TestOS isRootedOn)
	{
		bool result = FileSystem.Path.IsPathRooted(path.AsSpan());

		result.Should().Be(Test.RunsOn(isRootedOn));
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
