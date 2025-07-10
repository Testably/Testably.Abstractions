using aweXpect.Core;
using aweXpect.Core.Constraints;
using aweXpect.Formatting;
using aweXpect.Results;
using System.Runtime.CompilerServices;
using System.Text;
using Testably.Abstractions.Testing.Statistics;

namespace Testably.Abstractions.Testing.Tests.TestHelpers;

public static class StatisticsTestHelpers
{
	public static ExpectationResult<IStatistics> OnlyContainsMethodCall(
		this IThat<IStatistics> statistics, string name)
	{
		ExpectationBuilder expectationBuilder =
			((IExpectThat<IStatistics>)statistics).ExpectationBuilder;
		return new ExpectationResult<IStatistics>(expectationBuilder.AddConstraint((it, grammars)
			=> new OnlyContainsMethodCallConstraint(it, grammars, name, p => p.Length == 0)));
	}

	public static ExpectationResult<IStatistics> OnlyContainsMethodCall<T1>(
		this IThat<IStatistics> statistics, string name,
		T1 parameter1)
	{
		ExpectationBuilder expectationBuilder =
			((IExpectThat<IStatistics>)statistics).ExpectationBuilder;
		return new ExpectationResult<IStatistics>(expectationBuilder.AddConstraint((it, grammars)
			=> new OnlyContainsMethodCallConstraint(it, grammars, name,
				p => p.Length == 1 && p[0].Is(parameter1))));
	}

#if FEATURE_SPAN
	public static ExpectationResult<IStatistics> OnlyContainsMethodCall<T1>(this IThat<IStatistics> statistics, string name,
		ReadOnlySpan<T1> parameter1)
	{
		ParameterDescription.SpanParameterDescription<T1> parameter1Values = new(parameter1);
		ExpectationBuilder expectationBuilder =
 ((IExpectThat<IStatistics>)statistics).ExpectationBuilder;
		return new ExpectationResult<IStatistics>(expectationBuilder.AddConstraint((it, grammars)
			=> new OnlyContainsMethodCallConstraint(it, grammars, name, p => p.Length == 1 && p[0].Is(parameter1Values))));
	}
#endif

#if FEATURE_SPAN
	public static ExpectationResult<IStatistics> OnlyContainsMethodCall<T1>(this IThat<IStatistics> statistics, string name,
		Span<T1> parameter1)
	{
		ParameterDescription.SpanParameterDescription<T1> parameter1Values = new(parameter1);
		ExpectationBuilder expectationBuilder =
 ((IExpectThat<IStatistics>)statistics).ExpectationBuilder;
		return new ExpectationResult<IStatistics>(expectationBuilder.AddConstraint((it, grammars)
			=> new OnlyContainsMethodCallConstraint(it, grammars, name, p => p.Length == 1 && p[0].Is(parameter1Values))));
	}
#endif

	public static ExpectationResult<IStatistics> OnlyContainsMethodCall<T1>(
		this IThat<IStatistics> statistics, string name,
		T1[] parameter1)
	{
		ExpectationBuilder expectationBuilder =
			((IExpectThat<IStatistics>)statistics).ExpectationBuilder;
		return new ExpectationResult<IStatistics>(expectationBuilder.AddConstraint((it, grammars)
			=> new OnlyContainsMethodCallConstraint(it, grammars, name,
				p => p.Length == 1 && p[0].Is(parameter1))));
	}

	public static ExpectationResult<IStatistics> OnlyContainsMethodCall<T1, T2>(
		this IThat<IStatistics> statistics, string name,
		T1 parameter1, T2 parameter2)
	{
		ExpectationBuilder expectationBuilder =
			((IExpectThat<IStatistics>)statistics).ExpectationBuilder;
		return new ExpectationResult<IStatistics>(expectationBuilder.AddConstraint((it, grammars)
			=> new OnlyContainsMethodCallConstraint(it, grammars, name,
				p => p.Length == 2 && p[0].Is(parameter1) && p[1].Is(parameter2))));
	}

#if FEATURE_SPAN
	public static ExpectationResult<IStatistics> OnlyContainsMethodCall<T1, T2>(this IThat<IStatistics> statistics, string name,
		T1 parameter1, ReadOnlySpan<T2> parameter2)
	{
		ParameterDescription.SpanParameterDescription<T2> parameter2Values = new(parameter2);
		ExpectationBuilder expectationBuilder =
 ((IExpectThat<IStatistics>)statistics).ExpectationBuilder;
		return new ExpectationResult<IStatistics>(expectationBuilder.AddConstraint((it, grammars)
			=> new OnlyContainsMethodCallConstraint(it, grammars, name, p => p.Length == 2 && p[0].Is(parameter1) && p[1].Is(parameter2Values))));
	}
#endif

#if FEATURE_SPAN
	public static ExpectationResult<IStatistics> OnlyContainsMethodCall<T1, T2>(this IThat<IStatistics> statistics, string name,
		ReadOnlySpan<T1> parameter1, ReadOnlySpan<T2> parameter2)
	{
		ParameterDescription.SpanParameterDescription<T1> parameter1Values = new(parameter1);
		ParameterDescription.SpanParameterDescription<T2> parameter2Values = new(parameter2);
		ExpectationBuilder expectationBuilder =
 ((IExpectThat<IStatistics>)statistics).ExpectationBuilder;
		return new ExpectationResult<IStatistics>(expectationBuilder.AddConstraint((it, grammars)
			=> new OnlyContainsMethodCallConstraint(it, grammars, name, p => p.Length == 2 && p[0].Is(parameter1Values) && p[1].Is(parameter2Values))));
	}
#endif

	public static ExpectationResult<IStatistics> OnlyContainsMethodCall<T1, T2, T3>(
		this IThat<IStatistics> statistics, string name,
		T1 parameter1, T2 parameter2, T3 parameter3)
	{
		ExpectationBuilder expectationBuilder =
			((IExpectThat<IStatistics>)statistics).ExpectationBuilder;
		return new ExpectationResult<IStatistics>(expectationBuilder.AddConstraint((it, grammars)
			=> new OnlyContainsMethodCallConstraint(it, grammars, name,
				p => p.Length == 3 && p[0].Is(parameter1) && p[1].Is(parameter2) &&
				     p[2].Is(parameter3))));
	}

#if FEATURE_SPAN
	public static ExpectationResult<IStatistics> OnlyContainsMethodCall<T1, T2, T3>(this IThat<IStatistics> statistics, string name,
		ReadOnlySpan<T1> parameter1, ReadOnlySpan<T2> parameter2, ReadOnlySpan<T3> parameter3)
	{
		ParameterDescription.SpanParameterDescription<T1> parameter1Values = new(parameter1);
		ParameterDescription.SpanParameterDescription<T2> parameter2Values = new(parameter2);
		ParameterDescription.SpanParameterDescription<T3> parameter3Values = new(parameter3);
		ExpectationBuilder expectationBuilder =
 ((IExpectThat<IStatistics>)statistics).ExpectationBuilder;
		return new ExpectationResult<IStatistics>(expectationBuilder.AddConstraint((it, grammars)
			=> new OnlyContainsMethodCallConstraint(it, grammars, name, p => p.Length == 3 && p[0].Is(parameter1Values) && p[1].Is(parameter2Values) && p[2].Is(parameter3Values))));
	}
#endif

#if FEATURE_SPAN
	public static ExpectationResult<IStatistics> OnlyContainsMethodCall<T1, T2, T3>(this IThat<IStatistics> statistics, string name,
		T1 parameter1, ReadOnlySpan<T2> parameter2, T3 parameter3)
	{
		ParameterDescription.SpanParameterDescription<T2> parameter2Values = new(parameter2);
		ExpectationBuilder expectationBuilder =
 ((IExpectThat<IStatistics>)statistics).ExpectationBuilder;
		return new ExpectationResult<IStatistics>(expectationBuilder.AddConstraint((it, grammars)
			=> new OnlyContainsMethodCallConstraint(it, grammars, name, p => p.Length == 3 && p[0].Is(parameter1) && p[1].Is(parameter2Values) && p[2].Is(parameter3))));
	}
#endif

	public static ExpectationResult<IStatistics> OnlyContainsMethodCall<T1, T2, T3, T4>(
		this IThat<IStatistics> statistics, string name,
		T1 parameter1, T2 parameter2, T3 parameter3, T4 parameter4)
	{
		ExpectationBuilder expectationBuilder =
			((IExpectThat<IStatistics>)statistics).ExpectationBuilder;
		return new ExpectationResult<IStatistics>(expectationBuilder.AddConstraint((it, grammars)
			=> new OnlyContainsMethodCallConstraint(it, grammars, name,
				p => p.Length == 4 && p[0].Is(parameter1) && p[1].Is(parameter2) &&
				     p[2].Is(parameter3) && p[3].Is(parameter4))));
	}

#if FEATURE_SPAN
	public static ExpectationResult<IStatistics> OnlyContainsMethodCall<T1, T2, T3, T4>(this IThat<IStatistics> statistics, string name,
		ReadOnlySpan<T1> parameter1, ReadOnlySpan<T2> parameter2, ReadOnlySpan<T3> parameter3, ReadOnlySpan<T4> parameter4)
	{
		ParameterDescription.SpanParameterDescription<T1> parameter1Values = new(parameter1);
		ParameterDescription.SpanParameterDescription<T2> parameter2Values = new(parameter2);
		ParameterDescription.SpanParameterDescription<T3> parameter3Values = new(parameter3);
		ParameterDescription.SpanParameterDescription<T4> parameter4Values = new(parameter4);
		ExpectationBuilder expectationBuilder =
 ((IExpectThat<IStatistics>)statistics).ExpectationBuilder;
		return new ExpectationResult<IStatistics>(expectationBuilder.AddConstraint((it, grammars)
			=> new OnlyContainsMethodCallConstraint(it, grammars, name, p => p.Length == 4 && p[0].Is(parameter1Values) && p[1].Is(parameter2Values) && p[2].Is(parameter3Values) && p[3].Is(parameter4Values))));
	}
#endif

#if FEATURE_SPAN
	public static ExpectationResult<IStatistics> OnlyContainsMethodCall<T1, T2, T3, T4>(this IThat<IStatistics> statistics, string name,
		ReadOnlySpan<T1> parameter1, ReadOnlySpan<T2> parameter2, Span<T3> parameter3, T4 parameter4)
	{
		ParameterDescription.SpanParameterDescription<T1> parameter1Values = new(parameter1);
		ParameterDescription.SpanParameterDescription<T2> parameter2Values = new(parameter2);
		ParameterDescription.SpanParameterDescription<T3> parameter3Values = new(parameter3);
		ExpectationBuilder expectationBuilder =
 ((IExpectThat<IStatistics>)statistics).ExpectationBuilder;
		return new ExpectationResult<IStatistics>(expectationBuilder.AddConstraint((it, grammars)
			=> new OnlyContainsMethodCallConstraint(it, grammars, name, p => p.Length == 4 && p[0].Is(parameter1Values) && p[1].Is(parameter2Values) && p[2].Is(parameter3Values) && p[3].Is(parameter4))));
	}
#endif

	public static ExpectationResult<IStatistics> OnlyContainsMethodCall<T1, T2, T3, T4, T5>(
		this IThat<IStatistics> statistics, string name,
		T1 parameter1, T2 parameter2, T3 parameter3, T4 parameter4, T5 parameter5)
	{
		ExpectationBuilder expectationBuilder =
			((IExpectThat<IStatistics>)statistics).ExpectationBuilder;
		return new ExpectationResult<IStatistics>(expectationBuilder.AddConstraint((it, grammars)
			=> new OnlyContainsMethodCallConstraint(it, grammars, name,
				p => p.Length == 5 && p[0].Is(parameter1) && p[1].Is(parameter2) &&
				     p[2].Is(parameter3) && p[3].Is(parameter4) && p[4].Is(parameter5))));
	}

#if FEATURE_SPAN
	public static ExpectationResult<IStatistics> OnlyContainsMethodCall<T1, T2, T3, T4, T5>(this IThat<IStatistics> statistics, string name,
		ReadOnlySpan<T1> parameter1, ReadOnlySpan<T2> parameter2, ReadOnlySpan<T3> parameter3, Span<T4> parameter4, T5 parameter5)
	{
		ParameterDescription.SpanParameterDescription<T1> parameter1Values = new(parameter1);
		ParameterDescription.SpanParameterDescription<T2> parameter2Values = new(parameter2);
		ParameterDescription.SpanParameterDescription<T3> parameter3Values = new(parameter3);
		ParameterDescription.SpanParameterDescription<T4> parameter4Values = new(parameter4);
		ExpectationBuilder expectationBuilder =
 ((IExpectThat<IStatistics>)statistics).ExpectationBuilder;
		return new ExpectationResult<IStatistics>(expectationBuilder.AddConstraint((it, grammars)
			=> new OnlyContainsMethodCallConstraint(it, grammars, name, p => p.Length == 5 && p[0].Is(parameter1Values) && p[1].Is(parameter2Values) && p[2].Is(parameter3Values) && p[3].Is(parameter4Values) && p[4].Is(parameter5))));
	}
#endif

	public static ExpectationResult<IStatistics> OnlyContainsMethodCall<T1, T2, T3, T4, T5, T6>(
		this IThat<IStatistics> statistics, string name,
		T1 parameter1, T2 parameter2, T3 parameter3, T4 parameter4, T5 parameter5, T6 parameter6)
	{
		ExpectationBuilder expectationBuilder =
			((IExpectThat<IStatistics>)statistics).ExpectationBuilder;
		return new ExpectationResult<IStatistics>(expectationBuilder.AddConstraint((it, grammars)
			=> new OnlyContainsMethodCallConstraint(it, grammars, name,
				p => p.Length == 6 && p[0].Is(parameter1) && p[1].Is(parameter2) &&
				     p[2].Is(parameter3) && p[3].Is(parameter4) && p[4].Is(parameter5) &&
				     p[5].Is(parameter6))));
	}

	public static ExpectationResult<IStatistics> OnlyContainsPropertyGetAccess(
		this IThat<IStatistics> statistics, string name)
	{
		ExpectationBuilder expectationBuilder =
			((IExpectThat<IStatistics>)statistics).ExpectationBuilder;
		return new ExpectationResult<IStatistics>(expectationBuilder.AddConstraint((it, grammars)
			=> new OnlyContainsPropertyAccessConstraint(it, grammars, name, PropertyAccess.Get)));
	}

	public static ExpectationResult<IStatistics> OnlyContainsPropertySetAccess(
		this IThat<IStatistics> statistics, string name)
	{
		ExpectationBuilder expectationBuilder =
			((IExpectThat<IStatistics>)statistics).ExpectationBuilder;
		return new ExpectationResult<IStatistics>(expectationBuilder.AddConstraint((it, grammars)
			=> new OnlyContainsPropertyAccessConstraint(it, grammars, name, PropertyAccess.Set)));
	}

	private sealed class OnlyContainsPropertyAccessConstraint(
		string it,
		ExpectationGrammars grammars,
		string name,
		PropertyAccess propertyAccess)
		: ConstraintResult.WithNotNullValue<IStatistics>(it, grammars),
			IValueConstraint<IStatistics>
	{
		#region IValueConstraint<IStatistics> Members

		public ConstraintResult IsMetBy(IStatistics actual)
		{
			Actual = actual;
			Outcome = actual.Properties.Length == 1 &&
			          string.Equals(actual.Properties[0].Name, name, StringComparison.Ordinal) &&
			          actual.Properties[0].Access == propertyAccess
				? Outcome.Success
				: Outcome.Failure;
			return this;
		}

		#endregion

		protected override void AppendNegatedExpectation(StringBuilder stringBuilder,
			string? indentation = null)
		{
			stringBuilder.Append("not only accesses ").Append(name).Append(" via ")
				.Append(propertyAccess);
		}

		protected override void AppendNegatedResult(StringBuilder stringBuilder,
			string? indentation = null)
			=> stringBuilder.Append(It).Append(" did");

		protected override void AppendNormalExpectation(StringBuilder stringBuilder,
			string? indentation = null)
		{
			stringBuilder.Append("only accesses ").Append(name).Append(" via ")
				.Append(propertyAccess);
		}

		protected override void AppendNormalResult(StringBuilder stringBuilder,
			string? indentation = null)
		{
			if (Actual == null)
			{
				stringBuilder.Append(It).Append(" was <null>");
			}
			else if (Actual.Properties.Length == 0)
			{
				stringBuilder.Append(It).Append(" contained no property access");
			}
			else if (Actual.Properties.Length == 1)
			{
				stringBuilder.Append(It)
					.Append(" did not contain the expected property access, but ");
				Format.Formatter.Format(stringBuilder, Actual.Properties[0]);
			}
			else
			{
				stringBuilder.Append(It).Append(" contained more than one property access:");
				Format.Formatter.Format(stringBuilder, Actual.Properties);
			}
		}
	}

	private sealed class OnlyContainsMethodCallConstraint(
		string it,
		ExpectationGrammars grammars,
		string methodName,
		Func<ParameterDescription[], bool> parameterVerification,
		[CallerArgumentExpression("parameterVerification")]
		string doNotPopulateThisValue = "")
		: ConstraintResult.WithNotNullValue<IStatistics>(it, grammars),
			IValueConstraint<IStatistics>
	{
		private readonly string _methodName = methodName;

		private readonly Func<ParameterDescription[], bool> _parameterVerification =
			parameterVerification;

		private readonly string ParameterDescription = doNotPopulateThisValue;

		#region IValueConstraint<IStatistics> Members

		public ConstraintResult IsMetBy(IStatistics actual)
		{
			Actual = actual;
			Outcome = actual.Methods.Length == 1 &&
			          string.Equals(actual.Methods[0].Name, _methodName,
				          StringComparison.Ordinal) &&
			          _parameterVerification(actual.Methods[0].Parameters)
				? Outcome.Success
				: Outcome.Failure;
			return this;
		}

		#endregion

		protected override void AppendNegatedExpectation(StringBuilder stringBuilder,
			string? indentation = null)
		{
			stringBuilder.Append("does not only contain a single method ").Append(_methodName)
				.Append(" with ").Append(ParameterDescription);
		}

		protected override void AppendNegatedResult(StringBuilder stringBuilder,
			string? indentation = null)
			=> stringBuilder.Append(It).Append(" did");

		protected override void AppendNormalExpectation(StringBuilder stringBuilder,
			string? indentation = null)
		{
			stringBuilder.Append("only contains a single method ").Append(_methodName)
				.Append(" with ").Append(ParameterDescription);
		}

		protected override void AppendNormalResult(StringBuilder stringBuilder,
			string? indentation = null)
		{
			if (Actual == null)
			{
				stringBuilder.Append(It).Append(" was <null>");
			}
			else if (Actual.Methods.Length == 0)
			{
				stringBuilder.Append(It).Append(" contained no method call");
			}
			else if (Actual.Methods.Length == 1)
			{
				stringBuilder.Append(It).Append(" did not contain the expected method call, but ");
				Format.Formatter.Format(stringBuilder, Actual.Methods[0]);
			}
			else
			{
				stringBuilder.Append(It).Append(" contained more than one method call:");
				Format.Formatter.Format(stringBuilder, Actual.Methods);
			}
		}
	}
}
