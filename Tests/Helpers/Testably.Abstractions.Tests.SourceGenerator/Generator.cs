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
	#region IIncrementalGenerator Members

	/// <inheritdoc cref="IIncrementalGenerator.Initialize(IncrementalGeneratorInitializationContext)" />
	public void Initialize(IncrementalGeneratorInitializationContext context)
	{
		// Add the marker attribute to the compilation
		context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
			"MarkerAttributes.g.cs",
			SourceText.From(SourceGenerationHelper.GenerateMarkerAttributes(), Encoding.UTF8)));

		// Add the marker attribute to the compilation
		context.RegisterPostInitializationOutput(ctx => ctx.AddSource(
			"CollectionFixtures.g.cs",
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
		string[] exclusions =
		[
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
			// generate the source code and add it to the output
			string result = SourceGenerationHelper.GenerateTestClasses(model.Value);
			// Create a separate partial class file for each test class
			context.AddSource(fileName, SourceText.From(result, Encoding.UTF8));
		}
	}

	private static ClassModel? GetClassModelToGenerate(ClassModelType type,
		SemanticModel semanticModel, ClassDeclarationSyntax classDeclarationSyntax)
	{
		// Get the semantic representation of the enum syntax
		if (semanticModel.GetDeclaredSymbol(classDeclarationSyntax) is not INamedTypeSymbol
			classSymbol)
		{
			// something went wrong
			return null;
		}

		return new ClassModel(type, GetNamespace(classDeclarationSyntax), classSymbol.Name);
	}

	/// <summary>
	///     Determine the namespace the class/enum/struct is declared in, if any
	/// </summary>
	private static string GetNamespace(BaseTypeDeclarationSyntax syntax)
	{
		// If we don't have a namespace at all we'll return an empty string
		// This accounts for the "default namespace" case
		string @namespace = string.Empty;

		// Get the containing syntax node for the type declaration
		// (could be a nested type, for example)
		SyntaxNode? potentialNamespaceParent = syntax.Parent;

		// Keep moving "out" of nested classes etc until we get to a namespace
		// or until we run out of parents
		while (potentialNamespaceParent != null &&
		       potentialNamespaceParent is not NamespaceDeclarationSyntax
		       && potentialNamespaceParent is not FileScopedNamespaceDeclarationSyntax)
		{
			potentialNamespaceParent = potentialNamespaceParent.Parent;
		}

		// Build up the final namespace by looping until we no longer have a namespace declaration
		if (potentialNamespaceParent is BaseNamespaceDeclarationSyntax namespaceParent)
		{
			// We have a namespace. Use that as the type
			@namespace = namespaceParent.Name.ToString();

			// Keep moving "out" of the namespace declarations until we 
			// run out of nested namespace declarations
			while (true)
			{
				if (namespaceParent.Parent is not NamespaceDeclarationSyntax parent)
				{
					break;
				}

				// Add the outer namespace as a prefix to the final namespace
				@namespace = $"{namespaceParent.Name}.{@namespace}";
				namespaceParent = parent;
			}
		}

		// return the final namespace
		return @namespace;
	}

	private static ClassModel? GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
	{
		// we know the node is a ClassDeclarationSyntax thanks to IsSyntaxTargetForGeneration
		ClassDeclarationSyntax? classDeclarationSyntax = (ClassDeclarationSyntax)context.Node;

		// loop through all the attributes on the method
		foreach (AttributeListSyntax attributeListSyntax in classDeclarationSyntax.AttributeLists)
		{
			foreach (AttributeSyntax attributeSyntax in attributeListSyntax.Attributes)
			{
				if (context.SemanticModel.GetSymbolInfo(attributeSyntax).Symbol is not IMethodSymbol
					attributeSymbol)
				{
					// weird, we couldn't get the symbol, ignore it
					continue;
				}

				INamedTypeSymbol attributeContainingTypeSymbol = attributeSymbol.ContainingType;
				string fullName = attributeContainingTypeSymbol.ToDisplayString();

				return fullName switch
				{
					"Testably.Abstractions.TestHelpers.FileSystemTestsAttribute" =>
						GetClassModelToGenerate(ClassModelType.FileSystem,
							context.SemanticModel, classDeclarationSyntax),
					"Testably.Abstractions.TestHelpers.TimeSystemTestsAttribute" =>
						GetClassModelToGenerate(ClassModelType.TimeSystem,
							context.SemanticModel, classDeclarationSyntax),
					"Testably.Abstractions.TestHelpers.RandomSystemTestsAttribute" =>
						GetClassModelToGenerate(ClassModelType.RandomSystem,
							context.SemanticModel, classDeclarationSyntax),
					_ => null,
				};
			}
		}

		return null;
	}

	private static bool IsSyntaxTargetForGeneration(SyntaxNode syntaxNode)
	{
		return syntaxNode is ClassDeclarationSyntax { AttributeLists.Count: > 0 };
	}
}
