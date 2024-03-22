﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Testably.Abstractions.Tests.SourceGenerator.Model;

namespace Testably.Abstractions.Tests.SourceGenerator;

internal abstract class ClassGeneratorBase
{
	/// <summary>
	///     The marker of the base class to trigger generation of concrete test classes.
	/// </summary>
	public abstract string Marker { get; }

	public void GenerateClass(GeneratorExecutionContext context,
		ClassModel @class)
	{
		StringBuilder sourceBuilder = GetSourceBuilder();
		GenerateSource(sourceBuilder, @class);
		string fileName = CreateFileName(@class);
		context.AddSource(fileName,
			SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));
	}

	private string CreateFileName(ClassModel classToGenerate)
	{
		string? fileNamePrefix = ExtractFileNamePrefixFromMarker(Marker);
		List<string> namespacePrefixes =
		[
			"Testably.Abstractions.Tests.FileSystem.",
			"Testably.Abstractions."
		];

		string? namespacePrefix = namespacePrefixes
			.FirstOrDefault(classToGenerate.Namespace.StartsWith);
		if (namespacePrefix != null)
		{
			string? @namespace = classToGenerate.Namespace
				.Substring(namespacePrefix.Length);
			return $"{fileNamePrefix}{@namespace}.{classToGenerate.Name}.cs";
		}

		return $"{fileNamePrefix}{classToGenerate.Name}.cs";
	}

	/// <summary>
	///     Heuristic to generate a filename prefix depending on the <see cref="Marker" /> from the class generator.
	/// </summary>
	private static string ExtractFileNamePrefixFromMarker(string marker)
	{
		int genericIndex = marker.IndexOf("<", StringComparison.OrdinalIgnoreCase);
		if (genericIndex > 0)
		{
			marker = marker.Substring(0, genericIndex);
		}

		if (marker.EndsWith("TestBase", StringComparison.Ordinal))
		{
			marker = marker.Substring(0, marker.Length - "TestBase".Length);
		}

		return $"{marker}.";
	}

	/// <summary>
	///     Append the source necessary for the <paramref name="class" /> to the <paramref name="sourceBuilder" />.
	/// </summary>
	protected abstract void GenerateSource(
		StringBuilder sourceBuilder, ClassModel @class);

	private StringBuilder GetSourceBuilder()
		=> new(
			@"//------------------------------------------------------------------------------
// <auto-generated>
//   This code was generated by ""Testably.Abstractions.Tests.SourceGenerator"".
// 
//   Changes to this file may cause incorrect behavior
//   and will be lost if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
");
}
