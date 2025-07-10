using aweXpect.Core;
using aweXpect.Core.Constraints;
using aweXpect.Core.Sources;
using aweXpect.Delegates;
using aweXpect.Formatting;
using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using static aweXpect.Delegates.ThatDelegate;

namespace Testably.Abstractions.TestHelpers;

public static class AssertionHelpers
{
	/// <summary>
	///     Verifies that the <paramref name="delegate"/> throws either a <see cref="FileNotFoundException"/> or
	///     a <see cref="DirectoryNotFoundException"/> with their corresponding HResult values.
	/// </summary>
	public static ThatDelegateThrows<Exception> ThrowsAFileOrDirectoryNotFoundException(this ThatDelegate @delegate)
	{
		var throwOptions = new ThatDelegate.ThrowsOption();
		return new ThatDelegateThrows<Exception>(
			@delegate.ExpectationBuilder.AddConstraint((it, grammars) => new DelegateIsNotNullWithinTimeoutConstraint(it, grammars, throwOptions))
				.ForWhich<DelegateValue, Exception?>(d => d.Exception)
				.AddConstraint((it, grammars) => new ThrowsAFileOrDirectoryNotFoundExceptionConstraint(it, grammars, throwOptions))
				.And(" "), throwOptions);
	}

	private sealed class DelegateIsNotNullWithinTimeoutConstraint(
		string it,
		ExpectationGrammars grammars,
		ThrowsOption options)
		: ConstraintResult(grammars),
			IValueConstraint<DelegateValue>
	{
		private DelegateValue? _actual;

		public ConstraintResult IsMetBy(DelegateValue value)
		{
			_actual = value;
			if (value.IsNull)
			{
				Outcome = Outcome.Failure;
				return this;
			}

			if (options.ExecutionTimeOptions is not null &&
				!options.ExecutionTimeOptions.IsWithinLimit(value.Duration))
			{
				Outcome = Outcome.Failure;
				return this;
			}

			Outcome = Outcome.Success;
			return this;
		}

		public override void AppendExpectation(StringBuilder stringBuilder, string? indentation = null)
		{
			// Do nothing
		}

		public override void AppendResult(StringBuilder stringBuilder, string? indentation = null)
		{
			if (_actual?.IsNull != false)
			{
				stringBuilder.Append(it).Append(" was <null>");
			}
			else if (options.ExecutionTimeOptions is not null)
			{
				stringBuilder.Append(it).Append(" took ");
				options.ExecutionTimeOptions.AppendFailureResult(stringBuilder, _actual.Duration);
			}
		}

		public override bool TryGetValue<TValue>([NotNullWhen(true)] out TValue? value) where TValue : default
		{
			value = default;
			return false;
		}

		public override ConstraintResult Negate()
			=> this;
	}

	private sealed class ThrowsAFileOrDirectoryNotFoundExceptionConstraint(
		string it,
		ExpectationGrammars grammars,
		ThatDelegate.ThrowsOption throwOptions)
		: ConstraintResult(grammars),
			IValueConstraint<Exception?>
	{
		private Exception? _actual;

		/// <inheritdoc />
		public ConstraintResult IsMetBy(Exception? value)
		{
			_actual = value;

			if (!throwOptions.DoCheckThrow)
			{
				FurtherProcessingStrategy = FurtherProcessingStrategy.IgnoreCompletely;
				Outcome = value is null ? Outcome.Success : Outcome.Failure;
				return this;
			}

			if (value is null)
			{
				FurtherProcessingStrategy = FurtherProcessingStrategy.IgnoreResult;
			}
			else if (value is FileNotFoundException fileNotFoundException && fileNotFoundException.HResult == -2147024894)
			{
				Outcome = Outcome.Success;
				return this;
			}
			else if (value is DirectoryNotFoundException directoryNotFoundException && directoryNotFoundException.HResult == -2147024893)
			{
				Outcome = Outcome.Success;
				return this;
			}

			Outcome = Outcome.Failure;
			return this;
		}

		public override void AppendExpectation(StringBuilder stringBuilder, string? indentation = null)
		{
			if (!throwOptions.DoCheckThrow)
			{
				stringBuilder.Append("does not throw any exception");
			}
			else
			{
				stringBuilder.Append("throws a FileNotFoundException or a DirectoryNotFoundException with correct HResult");
			}

			if (throwOptions.ExecutionTimeOptions is not null)
			{
				stringBuilder.Append(' ');
				throwOptions.ExecutionTimeOptions.AppendTo(stringBuilder, "in ");
			}
		}

		public override void AppendResult(StringBuilder stringBuilder, string? indentation = null)
		{
			if (throwOptions.DoCheckThrow && _actual is null)
			{
				stringBuilder.Append(it).Append(" did not throw any exception");
			}
			else if (_actual is not null)
			{
				stringBuilder.Append(it).Append(" did throw ");
				stringBuilder.Append(FormatForMessage(_actual));
			}
		}

		public override bool TryGetValue<TValue>([NotNullWhen(true)] out TValue? value) where TValue : default
		{
			if (_actual is TValue typedValue)
			{
				value = typedValue;
				return true;
			}

			value = default;
			return typeof(TValue).IsAssignableFrom(typeof(DirectoryNotFoundException));
		}

		public override ConstraintResult Negate()
		{
			throwOptions.DoCheckThrow = !throwOptions.DoCheckThrow;
			return this;
		}

		private static string FormatForMessage(Exception exception)
		{
			string message = PrependAOrAn(Format.Formatter.Format(exception.GetType()));
			if (!string.IsNullOrEmpty(exception.Message))
			{
				message += ":" + Environment.NewLine + Indent(exception.Message);
			}

			return message;

			[return: NotNullIfNotNull(nameof(value))]
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

				return (indentFirstLine ? indentation : "")
					   + value.Replace("\n", $"\n{indentation}");
			}

			static string PrependAOrAn(string value)
			{
				char[] vocals = ['a', 'e', 'i', 'o', 'u', 'A', 'E', 'I', 'O', 'U',];
				if (value.Length > 0 && vocals.Contains(value[0]))
				{
					return $"an {value}";
				}

				return $"a {value}";
			}
		}
	}
}
