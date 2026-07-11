using System.IO.MemoryMappedFiles;

namespace Testably.Abstractions;

internal sealed class MemoryMappedViewStreamWrapper : MemoryMappedFileSystemViewStream
{
	private readonly MemoryMappedViewStream _instance;

	public MemoryMappedViewStreamWrapper(MemoryMappedViewStream instance)
		: base(instance)
	{
		_instance = instance;
	}

	/// <inheritdoc cref="MemoryMappedFileSystemViewStream.PointerOffset" />
	public override long PointerOffset
		=> _instance.PointerOffset;

	/// <inheritdoc cref="MemoryMappedFileSystemViewStream.Capacity" />
	public override long Capacity
		=> _instance.Capacity;
}
