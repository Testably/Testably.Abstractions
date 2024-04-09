using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using Testably.Abstractions.Tests.SourceGenerator.Model;

namespace Testably.Abstractions.Tests.SourceGenerator;

internal sealed class SyntaxReceiver : ISyntaxReceiver
{
	private readonly Dictionary<string, List<ClassModel>>
		_classModels = new(StringComparer.Ordinal);

	public SyntaxReceiver(IEnumerable<ClassGeneratorBase> classGenerators)
	{
		foreach (ClassGeneratorBase classGenerator in classGenerators)
		{
			_classModels.Add(classGenerator.Marker, []);
		}
	}

	#region ISyntaxReceiver Members

	/// <inheritdoc cref="ISyntaxReceiver.OnVisitSyntaxNode(SyntaxNode)" />
	public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
	{
		if (syntaxNode is ClassDeclarationSyntax classDeclarationSyntax)
		{
			string? marker = classDeclarationSyntax.BaseList?.Types.FirstOrDefault()
				?.ToString();

			if (marker != null &&
			    _classModels.TryGetValue(marker, out List<ClassModel>? models))
			{
				models!.Add(
					ClassModel.FromClassDeclarationSyntax(classDeclarationSyntax));
			}
		}
	}

	#endregion

	public List<ClassModel> GetClassModels(string marker)
	{
		if (_classModels.TryGetValue(marker, out List<ClassModel>? classModels))
		{
			return classModels!;
		}

		return [];
	}
}
