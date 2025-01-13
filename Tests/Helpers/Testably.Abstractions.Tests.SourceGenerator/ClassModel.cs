namespace Testably.Abstractions.Tests.SourceGenerator;

internal readonly record struct ClassModel
{
	public string Name { get; }
	public string Namespace { get; }
	public ClassModelType Type { get; }

	internal ClassModel(ClassModelType type, string @namespace, string name)
	{
		Type = type;
		Namespace = @namespace;
		Name = name;
	}
}
