namespace Testably.Abstractions.Tests.TestHelpers;

internal static class ExceptionTestHelper
{
	[Flags]
	internal enum TestTypes
	{
		Null,
		Empty,
		InvalidPath,
		All = Null | Empty | InvalidPath
	}

	internal static TestTypes ToTestType(this string? parameter)
	{
		if (parameter == null)
		{
			return TestTypes.Null;
		}

		if (parameter == "")
		{
			return TestTypes.Empty;
		}

		return TestTypes.InvalidPath;
	}
}