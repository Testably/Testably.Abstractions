using aweXpect.Core;
using aweXpect.Core.Constraints;
using aweXpect.Core.Extending;
using aweXpect.Core.Sources;
using aweXpect.Delegates;
using aweXpect.Formatting;
using aweXpect.Results;
using System;
using System.IO;
using System.Text;

namespace Testably.Abstractions.TestHelpers;

public static class AssertionHelpers
{
	/// <summary>
	///     Adds the <paramref name="expectations" /> to the <paramref name="result" /> only when the
	///     <paramref name="condition" /> is <see langword="true" />.
	/// </summary>
	public static AndOrResult<TType, TThat, TSelf> AndIf<TType, TThat, TSelf>(
		this AndOrResult<TType, TThat, TSelf> result,
		bool condition,
		Func<TThat, Expectation> expectations)
		where TSelf : AndOrResult<TType, TThat, TSelf>
	{
		if (condition)
		{
			expectations(result.And);
		}

		return result;
	}

	/// <inheritdoc cref="AndIf{TType, TThat, TSelf}" />
	public static ThatDelegateThrows<TException> AndIf<TException>(
		this ThatDelegateThrows<TException> result,
		bool condition,
		Func<IThatDelegateThrows<TException>, Expectation> expectations)
		where TException : Exception?
	{
		if (condition)
		{
			expectations(result);
		}

		return result;
	}

	/// <summary>
	///     Verifies that the <paramref name="delegate" /> throws either a <see cref="FileNotFoundException" /> or
	///     a <see cref="DirectoryNotFoundException" /> with their corresponding HResult values.
	/// </summary>
	public static ExpectationResult ThrowsAFileOrDirectoryNotFoundException(
		this ThatDelegate.WithoutValue @delegate)
		=> new(@delegate.Get().ExpectationBuilder.AddConstraint((it, grammars)
				=> new DelegateIsNotNullConstraint(it, grammars))
			.ForWhich<DelegateValue, Exception?>(d => d.Exception)
			.AddConstraint((it, grammars)
				=> new ThrowsAFileOrDirectoryNotFoundExceptionConstraint(it, grammars)));

	private sealed class DelegateIsNotNullConstraint(
		string it,
		ExpectationGrammars grammars)
		: ConstraintResult(grammars),
			IValueConstraint<DelegateValue>
	{
		private DelegateValue? _actual;

		#region IValueConstraint<DelegateValue> Members

		public override void AppendExpectation(StringBuilder stringBuilder,
			string? indentation = null)
		{
			// Do nothing
		}

		public ConstraintResult IsMetBy(DelegateValue value)
		{
			_actual = value;
			if (value.IsNull)
			{
				Outcome = Outcome.Failure;
				return this;
			}

			Outcome = Outcome.Success;
			return this;
		}

		#endregion

		public override void AppendResult(StringBuilder stringBuilder, string? indentation = null)
		{
			if (_actual?.IsNull != false)
			{
				stringBuilder.Append(it).Append(" was <null>");
			}
		}

		protected override void NegateInPlace()
		{
		}

		public override bool TryGetStoredValue<TValue>(out TValue? value) where TValue : default
		{
			value = default;
			return false;
		}
	}

	private sealed class ThrowsAFileOrDirectoryNotFoundExceptionConstraint(
		string it,
		ExpectationGrammars grammars)
		: ConstraintResult(grammars),
			IValueConstraint<Exception?>
	{
		private Exception? _actual;
		private bool _doCheckThrow = true;

		#region IValueConstraint<Exception?> Members

		public override void AppendExpectation(StringBuilder stringBuilder,
			string? indentation = null)
		{
			if (!_doCheckThrow)
			{
				stringBuilder.Append("does not throw any exception");
			}
			else
			{
				stringBuilder.Append(
					"throws a FileNotFoundException or a DirectoryNotFoundException with correct HResult");
			}
		}

		/// <inheritdoc />
		public ConstraintResult IsMetBy(Exception? value)
		{
			_actual = value;

			if (!_doCheckThrow)
			{
				FurtherProcessingStrategy = FurtherProcessingStrategy.IgnoreCompletely;
				Outcome = value is null ? Outcome.Success : Outcome.Failure;
				return this;
			}

			if (value is null)
			{
				FurtherProcessingStrategy = FurtherProcessingStrategy.IgnoreResult;
			}

			if ((value is FileNotFoundException fileNotFoundException &&
			     fileNotFoundException.HResult == -2147024894) ||
			    (value is DirectoryNotFoundException directoryNotFoundException &&
			     directoryNotFoundException.HResult == -2147024893))
			{
				Outcome = Outcome.Success;
				return this;
			}

			Outcome = Outcome.Failure;
			return this;
		}

		#endregion

		public override void AppendResult(StringBuilder stringBuilder, string? indentation = null)
		{
			if (_doCheckThrow && _actual is null)
			{
				stringBuilder.Append(it).Append(" did not throw any exception");
			}
			else if (_actual is not null)
			{
				stringBuilder.Append(it).Append(" did throw ");
				stringBuilder.Append(FormatForMessage(_actual));
			}
		}

		protected override void NegateInPlace()
			=> _doCheckThrow = !_doCheckThrow;

		public override bool TryGetStoredValue<TValue>(out TValue? value) where TValue : default
		{
			if (_actual is TValue typedValue)
			{
				value = typedValue;
				return true;
			}

			value = default;
			return typeof(TValue).IsAssignableFrom(typeof(DirectoryNotFoundException));
		}

		private static string FormatForMessage(Exception exception)
		{
			string message = PrependAOrAn(Format.Formatter.Format(exception.GetType()));
			if (!string.IsNullOrEmpty(exception.Message))
			{
				message += ":" + Environment.NewLine + Indent(exception.Message);
			}

			return message;

			static string? Indent(string? value, string? indentation = "  ",
				bool indentFirstLine = true)
			{
				if (value == null)
				{
					return null;
				}

				if (string.IsNullOrEmpty(indentation))
				{
					return value;
				}

				return (indentFirstLine ? indentation : "") +
#if NET8_0_OR_GREATER
					value.Replace("\n", $"\n{indentation}", StringComparison.Ordinal);
#else
					value.Replace("\n", $"\n{indentation}");
#endif
			}

			static string PrependAOrAn(string value)
			{
				char[] vocals = ['a', 'e', 'i', 'o', 'u', 'A', 'E', 'I', 'O', 'U',];
#if NET8_0_OR_GREATER
				if (value.Length > 0 && vocals.Contains(value[0]))
#else
				if (value.Length > 0 && vocals.Contains($"{value[0]}", StringComparison.Ordinal))
#endif
				{
					return $"an {value}";
				}

				return $"a {value}";
			}
		}
	}
}
