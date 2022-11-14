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

	public static AndWhichConstraint<ObjectAssertions, TException>
		BeException<TException>(this ObjectAssertions objectAssertions,
			string? messageContains = null,
			int? hResult = null,
			string? paramName = null,
			string because = "", params object[] becauseArgs)
		where TException : Exception
	{
		bool success = Execute.Assertion
			.ForCondition(objectAssertions.Subject is not null)
			.BecauseOf(because, becauseArgs)
			.WithDefaultIdentifier("type")
			.FailWith("Expected {context} to be {0}{reason}, but found <null>.",
				"FileNotFoundException or DirectoryNotFoundException");
		TException? typedSubject = null;
		if (success)
		{
			if (objectAssertions.Subject is TException exception)
			{
				typedSubject = exception;
				if (messageContains != null)
				{
					Execute.Assertion
						.ForCondition(exception.Message.Contains(messageContains))
						.BecauseOf(because, becauseArgs)
						.WithDefaultIdentifier("type")
						.FailWith(
							"Expected {context} to have a message containing {0}{reason}, but found {1}.",
							messageContains,
							exception.Message);
				}

				if (hResult != null)
				{
					Execute.Assertion
						.ForCondition(exception.HResult == hResult)
						.BecauseOf(because, becauseArgs)
						.WithDefaultIdentifier("type")
						.FailWith(
							"Expected {context} to have HResult set to {0}{reason}, but found {1}.",
							hResult,
							exception.HResult);
				}

				if (paramName != null)
				{
					if (exception is ArgumentException argumentException)
					{
						Execute.Assertion
							.ForCondition(argumentException.ParamName == paramName)
							.BecauseOf(because, becauseArgs)
							.WithDefaultIdentifier("type")
							.FailWith(
								"Expected {context} to have ParamName set to {0}{reason}, but found {1}.",
								paramName,
								argumentException.ParamName);
					}
					else
					{
						Execute.Assertion
							.BecauseOf(because, becauseArgs)
							.WithDefaultIdentifier("type")
							.FailWith(
								"Expected {context} to be {0} with ParamName set to {0}{reason}, but it is no ArgumentException.",
								typeof(TException),
								paramName);
					}
				}
			}
			else
			{
				Execute.Assertion
					.BecauseOf(because, becauseArgs)
					.WithDefaultIdentifier("type")
					.FailWith("Expected {context} to be {0}{reason}, but found {1}.",
						typeof(TException),
						objectAssertions.Subject!.GetType());
			}
		}

		return new AndWhichConstraint<ObjectAssertions, TException>(objectAssertions,
			typedSubject!);
	}
}
