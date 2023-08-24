using System.IO;

namespace Testably.Abstractions.Compression.Tests.TestHelpers;

internal sealed class MemoryStreamMock : MemoryStream
{
	public MemoryStreamMock(bool canWrite = true, bool canRead = true)
	{
		CanWrite = canWrite;
		CanRead = canRead;
	}

	public override bool CanRead { get; }

	public override bool CanWrite { get; }
}
