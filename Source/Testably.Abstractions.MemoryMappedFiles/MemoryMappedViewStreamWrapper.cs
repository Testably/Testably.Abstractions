using System.IO.MemoryMappedFiles;

namespace Testably.Abstractions;

internal sealed class MemoryMappedViewStreamWrapper(MemoryMappedViewStream instance)
	: MemoryMappedFileSystemViewStream(instance)
{
	/// <inheritdoc cref="MemoryMappedFileSystemViewStream.Capacity" />
	public override long Capacity
		=> instance.Capacity;

	/// <inheritdoc cref="MemoryMappedFileSystemViewStream.PointerOffset" />
	public override long PointerOffset
		=> instance.PointerOffset;
}
