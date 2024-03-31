#if FEATURE_COMPRESSION_STREAM
using System.IO;

namespace Testably.Abstractions.Compression.Tests.TestHelpers;

internal sealed class MemoryStreamMock(bool canWrite = true, bool canRead = true) : MemoryStream
{
	public override bool CanRead { get; } = canRead;

	public override bool CanWrite { get; } = canWrite;
}
#endif
