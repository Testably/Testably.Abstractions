using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Testably.Abstractions.AccessControl.Tests.TestHelpers;

[AttributeUsage(AttributeTargets.Class)]
public class WindowsOnlyFileSystemTestsAttribute : FileSystemTestsAttribute
{
	public override async IAsyncEnumerable<Func<Task<FileSystemTestData>>> GetTypedDataRowsAsync(
		DataGeneratorMetadata dataGeneratorMetadata)
	{
		if (!RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
		{
			yield break;
		}

		await foreach (Func<Task<FileSystemTestData>> baseDataRow in base.GetTypedDataRowsAsync(
			dataGeneratorMetadata))
		{
			yield return baseDataRow;
		}
	}
}
