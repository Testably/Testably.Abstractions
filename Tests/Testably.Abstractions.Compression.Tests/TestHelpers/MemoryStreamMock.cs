#if FEATURE_COMPRESSION_STREAM
using System.IO;

namespace Testably.Abstractions.Compression.Tests.TestHelpers;

internal sealed class MemoryStreamMock : MemoryStream
{
	public override bool CanRead { get; }

	public override bool CanWrite { get; }

	public MemoryStreamMock(bool canWrite = true, bool canRead = true)
	{
		CanWrite = canWrite;
		CanRead = canRead;
	}
}
#endif
