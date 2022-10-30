﻿using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Collections.Generic;
using System.Text;

namespace Testably.Abstractions.Tests.SourceGenerator;

[Generator]
public class TestGenerator : ISourceGenerator
{
	private SyntaxReceiver? _receiver;

	#region ISourceGenerator Members

	/// <inheritdoc cref="ISourceGenerator.Execute(GeneratorExecutionContext)" />
	public void Execute(GeneratorExecutionContext context)
	{
		if (_receiver == null)
		{
			return;
		}

		foreach (ClassToGenerate classToGenerate in _receiver.ClassesToGenerate)
		{
			CreateMockFileSystemClass(context, classToGenerate);
		}
	}

	/// <inheritdoc cref="ISourceGenerator.Initialize(GeneratorInitializationContext)" />
	public void Initialize(GeneratorInitializationContext context)
	{
		_receiver = new SyntaxReceiver();
		context.RegisterForSyntaxNotifications(() => _receiver);
	}

	#endregion

	private static void CreateMockFileSystemClass(GeneratorExecutionContext context,
	                                              ClassToGenerate classToGenerate)
	{
		StringBuilder? sourceBuilder = new(@$"//------------------------------------------------------------------------------
// <auto-generated>
//   This code was generated by ""Testably.Abstractions.Tests.SourceGenerator"".
// 
//   Changes to this file may cause incorrect behavior
//   and will be lost if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using Testably.Abstractions.Testing.FileSystemInitializer;
using Testably.Abstractions.Tests.TestHelpers;
using Xunit.Abstractions;

namespace {classToGenerate.Namespace};

public abstract partial class {classToGenerate.Name}<TFileSystem>
{{
	protected {classToGenerate.Name}(TFileSystem fileSystem, ITimeSystem timeSystem)
		: base(fileSystem, timeSystem)
	{{
	}}
}}

// ReSharper disable once UnusedMember.Global
public sealed class MockFileSystemTests : {classToGenerate.Name}<MockFileSystem>
{{
	/// <inheritdoc cref=""{classToGenerate.Name}{{TFileSystem}}.BasePath"" />
	public override string BasePath => _directoryCleaner.BasePath;

	private readonly IDirectoryCleaner _directoryCleaner;

	public MockFileSystemTests() : this(new MockFileSystem())
	{{
	}}

	private MockFileSystemTests(MockFileSystem mockFileSystem) : base(
		mockFileSystem,
		mockFileSystem.TimeSystem)
	{{
		_directoryCleaner = FileSystem
		   .SetCurrentDirectoryToEmptyTemporaryDirectory();
	}}

	/// <inheritdoc cref=""IDisposable.Dispose()"" />
	public void Dispose()
		=> _directoryCleaner.Dispose();
}}

#if !DEBUG || !DISABLE_TESTS_REALFILESYSTEM

// ReSharper disable once UnusedMember.Global
[Collection(nameof(DriveInfoFactory.RealFileSystemTests))]
public sealed class RealFileSystemTests : {classToGenerate.Name}<RealFileSystem>
{{
	/// <inheritdoc cref=""{classToGenerate.Name}{{TFileSystem}}.BasePath"" />
	public override string BasePath => _directoryCleaner.BasePath;

	private readonly IDirectoryCleaner _directoryCleaner;

	public RealFileSystemTests(ITestOutputHelper testOutputHelper)
		: base(new RealFileSystem(), new RealTimeSystem())
	{{
		_directoryCleaner = FileSystem
		   .SetCurrentDirectoryToEmptyTemporaryDirectory(testOutputHelper.WriteLine);
	}}

	/// <inheritdoc cref=""IDisposable.Dispose()"" />
	public void Dispose()
		=> _directoryCleaner.Dispose();
}}
#endif");
		string? fileName = CreateClassNamePrefix(classToGenerate) + ".cs";
		context.AddSource(fileName,
			SourceText.From(sourceBuilder.ToString(), Encoding.UTF8));
	}

	private static string CreateClassNamePrefix(ClassToGenerate classToGenerate, bool prefixNamespace = false)
	{
		if (prefixNamespace && classToGenerate.Namespace.StartsWith("Testably.Abstractions.Tests.FileSystem."))
		{
			var prefixLength = "Testably.Abstractions.Tests.FileSystem.".Length;
			return $"{classToGenerate.Namespace.Substring(prefixLength)}.{classToGenerate.Name}";
		}

		return classToGenerate.Name;
	}

	private class SyntaxReceiver : ISyntaxReceiver
	{
		public readonly List<ClassToGenerate> ClassesToGenerate = new();

		/// <summary>
		///     The marker of the base class to trigger generation of files.
		/// </summary>
		private static readonly string ClassMarker = "FileSystemTestBase<TFileSystem>";

		#region ISyntaxReceiver Members

		/// <inheritdoc cref="ISyntaxReceiver.OnVisitSyntaxNode(SyntaxNode)" />
		public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
		{
			if (syntaxNode is ClassDeclarationSyntax classDeclarationSyntax)
			{
				BaseTypeSyntax? baseType =
					classDeclarationSyntax.BaseList?.Types.FirstOrDefault();
				if (baseType?.ToString() == ClassMarker)
				{
					var classToGenerate = ClassToGenerate
					   .FromClassDeclarationSyntax(classDeclarationSyntax);
					ClassesToGenerate.Add(classToGenerate);
				}
			}
		}

		#endregion
	}
}