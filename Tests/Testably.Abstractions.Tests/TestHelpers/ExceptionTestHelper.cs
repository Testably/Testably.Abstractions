namespace Testably.Abstractions.Tests.TestHelpers;

internal static class ExceptionTestHelper
{
	/// <summary>
	///     Various test types for exception tests.
	/// </summary>
	[Flags]
	internal enum TestTypes
	{
		/// <summary>
		///     The provided parameter is <see langword="null" />.
		/// </summary>
		Null = 1,

		/// <summary>
		///     The provided parameter is <see cref="string.Empty" />.
		/// </summary>
		Empty = 2,

		/// <summary>
		///     The provided parameter is <see cref="string.Empty" />.
		/// </summary>
		Whitespace = 4,

		/// <summary>
		///     The provided parameter contains invalid path characters.
		/// </summary>
		InvalidPath = 8,

		NullOrEmpty = Null | Empty,
		NullOrInvalidPath = Null | InvalidPath,
		All = Null | Empty | Whitespace | InvalidPath,
		AllExceptWhitespace = All & ~Whitespace,
		AllExceptInvalidPath = All & ~InvalidPath
	}

	/// <summary>
	///     Determines the <see cref="TestTypes" /> according to the provided <paramref name="value" />.
	/// </summary>
	internal static TestTypes ToTestType(this string? value)
	{
		if (value == null)
		{
			return TestTypes.Null;
		}

		if (value == "")
		{
			return TestTypes.Empty;
		}

		if (value.TrimEnd() == "")
		{
			return TestTypes.Whitespace;
		}

		return TestTypes.InvalidPath;
	}
}