using aweXpect.Core;
using aweXpect.Core.Constraints;
using aweXpect.Delegates;
using aweXpect.Options;
using aweXpect.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Testably.Abstractions.Tests.TestHelpers;
/// <summary>
/// TODO: This class should be deleted after a aweXpect update
/// </summary>
internal static class TemporaryExtensions
{

	/// <summary>
	///     Verifies that the thrown exception has a message that contains the <paramref name="expected" /> pattern.
	/// </summary>
	public static StringEqualityResult<TException, ThatDelegateThrows<TException>> WithMessageContaining<TException>(
		this ThatDelegateThrows<TException> source,
		string? expected)
		where TException : Exception?
	{
		StringEqualityOptions options = new();
		return new StringEqualityResult<TException, ThatDelegateThrows<TException>>(
			source.ExpectationBuilder.AddConstraint((it, grammars)
				=> new HasMessageContainingConstraint(
					source.ExpectationBuilder,
					it,
					grammars | ExpectationGrammars.Active | ExpectationGrammars.Nested,
					expected,
					options)),
			source,
			options);
	}

	private sealed class HasMessageContainingConstraint(
		ExpectationBuilder expectationBuilder,
		string it,
		ExpectationGrammars grammars,
		string? expected,
		StringEqualityOptions options)
		: ConstraintResult.WithValue<Exception?>(grammars),
			IValueConstraint<Exception?>
	{
		public ConstraintResult IsMetBy(Exception? actual)
		{
			Actual = actual;
			options.AsWildcard();
			Outcome = expected is null || options.AreConsideredEqual(actual?.Message, $"*{expected}*")
				? Outcome.Success
				: Outcome.Failure;
			if (Outcome == Outcome.Failure)
			{
				expectationBuilder.UpdateContexts(contexts => contexts
					.Add(new ResultContext("Message", actual?.Message)));
			}

			return this;
		}

		protected override void AppendNormalExpectation(StringBuilder stringBuilder, string? indentation = null)
		{
			ExpectationGrammars equalityGrammars = Grammars;
			if (Grammars.HasFlag(ExpectationGrammars.Active))
			{
				stringBuilder.Append("with Message containing ");
				equalityGrammars &= ~ExpectationGrammars.Active;
			}
			else if (Grammars.HasFlag(ExpectationGrammars.Nested))
			{
				stringBuilder.Append("Message contains ");
			}
			else
			{
				stringBuilder.Append("contains Message ");
			}

			stringBuilder.Append(options.GetExpectation(expected, equalityGrammars));
		}

		protected override void AppendNormalResult(StringBuilder stringBuilder, string? indentation = null)
			=> stringBuilder.Append(options.GetExtendedFailure(it, Grammars, Actual?.Message, expected));

		protected override void AppendNegatedExpectation(StringBuilder stringBuilder, string? indentation = null)
		{
			ExpectationGrammars equalityGrammars = Grammars;
			if (Grammars.HasFlag(ExpectationGrammars.Active))
			{
				stringBuilder.Append("with Message containing ");
				equalityGrammars &= ~ExpectationGrammars.Active;
			}
			else if (Grammars.HasFlag(ExpectationGrammars.Nested))
			{
				stringBuilder.Append("Message contains ");
			}
			else
			{
				stringBuilder.Append("contains Message ");
			}

			stringBuilder.Append(options.GetExpectation(expected, equalityGrammars));
		}

		protected override void AppendNegatedResult(StringBuilder stringBuilder, string? indentation = null)
			=> AppendNormalResult(stringBuilder, indentation);
	}
}
