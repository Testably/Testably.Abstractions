﻿using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;
using System;
using System.IO;

namespace Testably.Abstractions.TestHelpers;

public static class AssertionHelpers
{
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

				AssertExceptionMessage(exception, messageContains, because, becauseArgs);
				AssertExceptionHResult(exception, hResult, because, becauseArgs);
				AssertExceptionParamName(exception, paramName, because, becauseArgs);
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

	private static void AssertExceptionHResult<TException>(TException exception,
		int? hResult,
		string because, object[] becauseArgs) where TException : Exception
	{
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
	}

	private static void AssertExceptionMessage<TException>(TException exception,
		string? messageContains,
		string because, object[] becauseArgs) where TException : Exception
	{
		if (messageContains != null)
		{
			#pragma warning disable MA0074 // Avoid implicit culture-sensitive methods
			#pragma warning disable MA0001 // Use an overload of 'Contains' that has a StringComparison parameter
			Execute.Assertion
				.ForCondition(exception.Message.Contains(messageContains))
				.BecauseOf(because, becauseArgs)
				.WithDefaultIdentifier("type")
				.FailWith(
					"Expected {context} to have a message containing {0}{reason}, but found {1}.",
					messageContains,
					exception.Message);
			#pragma warning restore MA0001
			#pragma warning restore MA0074
		}
	}

	private static void AssertExceptionParamName<TException>(TException exception,
		string? paramName,
		string because, object[] becauseArgs) where TException : Exception
	{
		if (paramName != null)
		{
			if (exception is ArgumentException argumentException)
			{
				Execute.Assertion
					.ForCondition(string.Equals(
						argumentException.ParamName,
						paramName,
						StringComparison.Ordinal))
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
}
