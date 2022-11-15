using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using Testably.Abstractions.Tests.SourceGenerator.ClassGenerators;
using Testably.Abstractions.Tests.SourceGenerator.Model;

namespace Testably.Abstractions.Tests.SourceGenerator;

/// <summary>
///     This generator creates concrete classes in `Testably.Abstractions.Tests` for abstract partial classes, so that the
///     defined tests run against a real and a mocked system. This works for:<br />
///     - `IFileSystem`<br />
///     - `IRandomSystem`<br />
///     - `ITimeSystem`
/// </summary>
[Generator]
public sealed class Generator : ISourceGenerator
{
	private readonly List<ClassGeneratorBase> _classGenerators = new()
	{
		new FileSystemClassGenerator(),
		new RandomSystemClassGenerator(),
		new TimeSystemClassGenerator()
	};

	private SyntaxReceiver? _syntaxReceiver;

	#region ISourceGenerator Members

	/// <inheritdoc cref="ISourceGenerator.Execute(GeneratorExecutionContext)" />
	public void Execute(GeneratorExecutionContext context)
	{
		if (_syntaxReceiver == null)
		{
			return;
		}

		foreach (ClassGeneratorBase classGenerator in _classGenerators)
		{
			foreach (ClassModel classModel in _syntaxReceiver
				.GetClassModels(classGenerator.Marker))
			{
				classGenerator.GenerateClass(context, classModel);
			}
		}
	}

	/// <inheritdoc cref="ISourceGenerator.Initialize(GeneratorInitializationContext)" />
	public void Initialize(GeneratorInitializationContext context)
	{
		_syntaxReceiver = new SyntaxReceiver(_classGenerators);
		context.RegisterForSyntaxNotifications(() => _syntaxReceiver);
	}

	#endregion
}
