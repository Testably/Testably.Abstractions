using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Testably.Abstractions.Compression.Tests.TestHelpers;

internal class MemoryStreamMock : MemoryStream
{
	public MemoryStreamMock(bool canWrite = true, bool canRead = true)
	{
		CanWrite = canWrite;
		CanRead = canRead;
	}

	public override bool CanRead { get; }

	public override bool CanWrite { get; }
}
