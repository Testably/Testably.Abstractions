using FluentAssertions.Execution;
using FluentAssertions.Primitives;
using System.IO;

namespace Testably.Abstractions.Tests.TestHelpers;

internal static class AssertionHelpers
{
	public static AndWhichConstraint<ObjectAssertions, IOException?>
		BeFileOrDirectoryNotFoundException(this ObjectAssertions objectAssertions,
		                                   string because = "", params object[] becauseArgs)
	{
		bool success = Execute.Assertion
		                      .ForCondition(objectAssertions.Subject is not null)
		                      .BecauseOf(because, becauseArgs)
		                      .WithDefaultIdentifier("type")
		                      .FailWith("Expected {context} to be {0}{reason}, but found <null>.",
			                       "FileNotFoundException or DirectoryNotFoundException");
		IOException? typedSubject = null;
		if (success)
		{
			if (objectAssertions.Subject is FileNotFoundException fileNotFoundException)
			{
				typedSubject = fileNotFoundException;
				Execute.Assertion
				       .ForCondition(fileNotFoundException.HResult == -2147024894)
				       .BecauseOf(because, becauseArgs)
				       .WithDefaultIdentifier("type")
				       .FailWith(
					        "Expected {context} to have HResult set to {0}{reason}, but found {1}.",
					        -2147024894,
					        fileNotFoundException.HResult);
			}
			else if (objectAssertions.Subject is DirectoryNotFoundException
				directoryNotFoundException)
			{
				typedSubject = directoryNotFoundException;
				Execute.Assertion
				       .ForCondition(directoryNotFoundException.HResult == -2147024893)
				       .BecauseOf(because, becauseArgs)
				       .WithDefaultIdentifier("type")
				       .FailWith(
					        "Expected {context} to have HResult set to {0}{reason}, but found {1}.",
					        -2147024893,
					        directoryNotFoundException.HResult);
			}
			else
			{
				Execute.Assertion
				       .BecauseOf(because, becauseArgs)
				       .WithDefaultIdentifier("type")
				       .FailWith("Expected {context} to be {0}{reason}, but found {1}.",
					        "FileNotFoundException or DirectoryNotFoundException",
					        objectAssertions.Subject!.GetType());
			}
		}

		return new AndWhichConstraint<ObjectAssertions, IOException?>(objectAssertions,
			typedSubject);
	}
}