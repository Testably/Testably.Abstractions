namespace Testably.Abstractions.FluentAssertions;

/// <summary>
///     Assertions on <see cref="IDirectoryInfo" />.
/// </summary>
public class DirectoryInfoAssertions :
	ReferenceTypeAssertions<IDirectoryInfo, DirectoryInfoAssertions>
{
	/// <inheritdoc cref="ReferenceTypeAssertions{TSubject,TAssertions}.Identifier" />
	protected override string Identifier => "directoryinfo";

	internal DirectoryInfoAssertions(IDirectoryInfo instance)
		: base(instance)
	{
	}

	/// <summary>
	///     Asserts that the current directory has at least one file which matches the <paramref name="searchPattern" />.
	/// </summary>
	public AndConstraint<DirectoryInfoAssertions> HaveFile(
		string searchPattern = "*", string because = "", params object[] becauseArgs)
	{
		Execute.Assertion
			.BecauseOf(because, becauseArgs)
			.ForCondition(!string.IsNullOrEmpty(searchPattern))
			.FailWith(
				"You can't assert a file exist in the directory if you don't pass a proper name")
			.Then
			.Given(() => Subject.GetFiles(searchPattern))
			.ForCondition(fileInfos => fileInfos.Length > 0)
			.FailWith(
				"Expected {context:directoryinfo} to contain at least one file matching {0}{reason}, but none was found.",
				_ => searchPattern);

		return new AndConstraint<DirectoryInfoAssertions>(this);
	}
}
