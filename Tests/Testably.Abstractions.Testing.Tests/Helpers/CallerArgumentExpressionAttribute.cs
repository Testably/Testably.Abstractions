#if !NET6_0_OR_GREATER
using System.Diagnostics.CodeAnalysis;

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable once CheckNamespace
namespace System.Runtime.CompilerServices
{
	/// <summary>
	///     Indicates that a parameter captures the expression passed for another parameter as a string.
	/// </summary>
	[AttributeUsage(AttributeTargets.Parameter)]
	[ExcludeFromCodeCoverage]
	internal sealed class CallerArgumentExpressionAttribute : Attribute
	{
		/// <summary>
		///     Gets the name of the parameter whose expression should be captured as a string.
		/// </summary>
		public string ParameterName { get; }

		/// <summary>
		///     Initializes a new instance of the <see cref="CallerArgumentExpressionAttribute" /> class.
		/// </summary>
		/// <param name="parameterName">The name of the parameter whose expression should be captured as a string.</param>
		public CallerArgumentExpressionAttribute(string parameterName)
		{
			ParameterName = parameterName;
		}
	}
}

#endif
