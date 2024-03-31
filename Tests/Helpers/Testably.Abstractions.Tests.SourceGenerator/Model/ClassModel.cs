﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Testably.Abstractions.Tests.SourceGenerator.Model;

internal sealed class ClassModel
{
	public string Name { get; }
	public string Namespace { get; }

	private ClassModel(string @namespace, string name)
	{
		Namespace = @namespace;
		Name = name;
	}

	internal static ClassModel FromClassDeclarationSyntax(
		ClassDeclarationSyntax classDeclarationSyntax)
	{
		string @namespace = GetNamespace(classDeclarationSyntax);
		string name = classDeclarationSyntax.Identifier.ToString();
		return new ClassModel(@namespace, name);
	}

	/// <summary>
	///     Get the namespace of a <paramref name="syntaxNode" />.
	/// </summary>
	/// <remarks>
	///     see
	///     <see
	///         href="https://andrewlock.net/creating-a-source-generator-part-5-finding-a-type-declarations-namespace-and-type-hierarchy/" />
	/// </remarks>
	private static string GetNamespace(SyntaxNode syntaxNode)
	{
		// If we don't have a namespace at all we'll return an empty string
		// This accounts for the "default namespace" case
		string nameSpace = string.Empty;

		// Get the containing syntax node for the type declaration
		// (could be a nested type, for example)
		SyntaxNode? potentialNamespaceParent = syntaxNode.Parent;

		// Keep moving "out" of nested classes until we get to a namespace
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
			nameSpace = namespaceParent.Name.ToString();

			// Keep moving "out" of the namespace declarations until we 
			// run out of nested namespace declarations
			while (true)
			{
				if (namespaceParent.Parent is not NamespaceDeclarationSyntax parent)
				{
					break;
				}

				// Add the outer namespace as a prefix to the final namespace
				nameSpace = $"{namespaceParent.Name}.{nameSpace}";
				namespaceParent = parent;
			}
		}

		// return the final namespace
		return nameSpace;
	}
}
