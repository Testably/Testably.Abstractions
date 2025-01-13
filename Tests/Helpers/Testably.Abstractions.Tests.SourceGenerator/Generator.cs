using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System;
using System.Collections.Generic;
using System.Text;

namespace Testably.Abstractions.Tests.SourceGenerator;

/// <summary>
///     This generator creates concrete classes in `Testably.Abstractions.Tests` for abstract partial classes, so that the
///     defined tests run against a real and a mocked system. This works for:<br />
///     - `IFileSystem`<br />
///     - `IRandomSystem`<br />
///     - `ITimeSystem`
/// </summary>
[Generator]
public sealed class Generator : IIncrementalGenerator
{
	private static readonly HashSet<string> GeneratedFiles = [];

	#region IIncrementalGenerator Members

	/// <inheritdoc cref="IIncrementalGenerator.Initialize(IncrementalGeneratorInitializationContext)" />
	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		// Add the marker attribute to the compilation
		context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
			"XunitCollectionFixtures.g.cs", 
			SourceText.From(SourceGenerationHelper.GenerateCollectionFixtures(), Encoding.UTF8)));
		
		// Do a simple filter for enums
		IncrementalValuesProvider<ClassModel?> classesToGenerate = context.SyntaxProvider
			.CreateSyntaxProvider(
				predicate: static (s, _)
					=> IsSyntaxTargetForGeneration(s), // select enums with attributes
				transform: static (ctx, _)
					=> GetSemanticTargetForGeneration(
						ctx)) // select enums with the [EnumExtensions] attribute and extract details
			.Where(static m => m is not null); // Filter out errors that we don't care about

		context.RegisterSourceOutput(classesToGenerate,
			static (context, source) => Execute(source, context));
	}

	#endregion

	private static string CreateFileName(ClassModel model)
	{
		string? prefix = model.Namespace + ".";
		string[] exclusions = [
			"Testably.Abstractions.Tests.",
			"Testably.Abstractions.",
		];
		foreach (string? exclusion in exclusions)
		{
			if (prefix.StartsWith(exclusion, StringComparison.Ordinal))
			{
				prefix = prefix.Substring(exclusion.Length);
			}
		}

		return $"{prefix}{model.Name}.g.cs";
	}

	private static void Execute(ClassModel? model, SourceProductionContext context)
	{
		if (model is not null)
		{
			string? fileName = CreateFileName(model.Value);
			if (!GeneratedFiles.Add(fileName))
			{
				return;
			}

			// generate the source code and add it to the output
			string result = SourceGenerationHelper.GenerateTestClasses(model.Value);
			// Create a separate partial class file for each test class
			context.AddSource(fileName, SourceText.From(result, Encoding.UTF8));
		}
	}

	private static ClassModel? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
	{
		// we know the node is a ClassDeclarationSyntax thanks to IsSyntaxTargetForGeneration
		ClassDeclarationSyntax? classDeclarationSyntax = (ClassDeclarationSyntax)context.Node;

		ISymbol? typeSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax);
		if (typeSymbol is not INamedTypeSymbol namedTypeSymbol)
		{
			return null;
		}

		INamedTypeSymbol? parentTypeSymbol = namedTypeSymbol.BaseType;
		if (parentTypeSymbol is null)
		{
			return null;
		}

		if (!string.Equals(parentTypeSymbol.ContainingNamespace.ToString(),
			"Testably.Abstractions.TestHelpers", StringComparison.Ordinal))
		{
			return null;
		}

		if (parentTypeSymbol.IsGenericType &&
		    string.Equals(parentTypeSymbol.Name, "FileSystemTestBase", StringComparison.Ordinal))
		{
			return new ClassModel(ClassModelType.FileSystem, typeSymbol.ContainingNamespace.ToString(), typeSymbol.Name);
		}

		if (parentTypeSymbol.IsGenericType &&
		    string.Equals(parentTypeSymbol.Name, "TimeSystemTestBase", StringComparison.Ordinal))
		{
			return new ClassModel(ClassModelType.TimeSystem, typeSymbol.ContainingNamespace.ToString(), typeSymbol.Name);
		}

		if (parentTypeSymbol.IsGenericType &&
		    string.Equals(parentTypeSymbol.Name, "RandomSystemTestBase", StringComparison.Ordinal))
		{
			return new ClassModel(ClassModelType.RandomSystem, typeSymbol.ContainingNamespace.ToString(), typeSymbol.Name);
		}

		return null;
	}

	private static bool IsSyntaxTargetForGeneration(SyntaxNode syntaxNode)
	{
		return syntaxNode is ClassDeclarationSyntax;
	}
}
