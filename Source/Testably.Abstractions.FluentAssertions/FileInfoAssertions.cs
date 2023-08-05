namespace Testably.Abstractions.FluentAssertions;

/// <summary>
///     Assertions on <see cref="IFileInfo" />.
/// </summary>
public class FileInfoAssertions :
	ReferenceTypeAssertions<IFileInfo, FileInfoAssertions>
{
	/// <inheritdoc cref="ReferenceTypeAssertions{TSubject,TAssertions}.Identifier" />
	protected override string Identifier => "file";

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
				"Expected {context:file} '{0}' not to be read-only {reason}, but it was.",
				_ => Subject.Name);

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
				"Expected {context:file} '{0}' to be read-only {reason}, but it was not.",
				_ => Subject.Name);

		return new AndConstraint<FileInfoAssertions>(this);
	}
}
