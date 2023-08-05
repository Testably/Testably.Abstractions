namespace Testably.Abstractions.FluentAssertions;

/// <summary>
///     Assertions on <see cref="IFileInfo" />.
/// </summary>
public class FileInfoAssertions :
	ReferenceTypeAssertions<IFileInfo, FileInfoAssertions>
{
	/// <inheritdoc cref="ReferenceTypeAssertions{TSubject,TAssertions}.Identifier" />
	protected override string Identifier => "fileinfo";

	internal FileInfoAssertions(IFileInfo instance)
		: base(instance)
	{
	}

	/// <summary>
	///     Asserts that the current file is not read-only.
	/// </summary>
	public AndConstraint<FileInfoAssertions> IsNotReadOnly(
		string because = "", params object[] becauseArgs)
	{
		Execute.Assertion
			.BecauseOf(because, becauseArgs)
			.Given(() => Subject)
			.ForCondition(fileInfo => !fileInfo.IsReadOnly)
			.FailWith(
				"Expected {context:fileinfo} not to be read-only {reason}, but it was.");

		return new AndConstraint<FileInfoAssertions>(this);
	}

	/// <summary>
	///     Asserts that the current file is read-only.
	/// </summary>
	public AndConstraint<FileInfoAssertions> IsReadOnly(
		string because = "", params object[] becauseArgs)
	{
		Execute.Assertion
			.BecauseOf(because, becauseArgs)
			.Given(() => Subject)
			.ForCondition(fileInfo => fileInfo.IsReadOnly)
			.FailWith(
				"Expected {context:fileinfo} to be read-only {reason}, but it was not.");

		return new AndConstraint<FileInfoAssertions>(this);
	}
}
