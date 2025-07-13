using System.IO;
using System.Text;

namespace Testably.Abstractions.Tests.FileSystem.FileStream;

[FileSystemTests]
public partial class ParallelTests
{
	[Theory]
	[AutoData]
	public async Task MultipleFlush_ShouldKeepLatestChanges(string path)
	{
		using (FileSystemStream stream1 = FileSystem.File.Open(path, FileMode.OpenOrCreate,
			FileAccess.Write, FileShare.ReadWrite))
		{
			using FileSystemStream stream2 = FileSystem.File.Open(path, FileMode.OpenOrCreate,
				FileAccess.Write, FileShare.ReadWrite);

			stream2.Write(Encoding.UTF8.GetBytes("foo"), 0, 3);
			stream1.Write(Encoding.UTF8.GetBytes("bar"), 0, 3);

			stream1.Flush();
			stream2.Flush();
		}
		
		await That(FileSystem.File.ReadAllText(path)).IsEqualTo("foo");
	}
	
	[Theory]
	[AutoData]
	public async Task MultipleFlush_DifferentLength_ShouldKeepAdditionalBytes(string path)
	{
		using (FileSystemStream stream1 = FileSystem.File.Open(path, FileMode.OpenOrCreate,
			FileAccess.Write, FileShare.ReadWrite))
		{
			using FileSystemStream stream2 = FileSystem.File.Open(path, FileMode.OpenOrCreate,
				FileAccess.Write, FileShare.ReadWrite);

			stream2.Write(Encoding.UTF8.GetBytes("foo"), 0, 3);
			stream1.Write(Encoding.UTF8.GetBytes("barfoo"), 0, 6);

			await That(stream1).HasLength().EqualTo(6);
			await That(stream2).HasLength().EqualTo(3);
			
			stream1.Flush();
			
			await That(stream1).HasLength().EqualTo(6);
			await That(stream2).HasLength().EqualTo(6);
			
			stream2.Flush();
			
			await That(stream1).HasLength().EqualTo(6);
			await That(stream2).HasLength().EqualTo(6);
		}
		
		await That(FileSystem.File.ReadAllText(path)).IsEqualTo("foofoo");
	}
	
	[Theory]
	[AutoData]
	public async Task MultipleFlush_DifferentPosition_ShouldKeepAdditionalBytes(string path)
	{
		FileSystem.File.WriteAllText(path, "AAAAAAAAAAAA");
		using (FileSystemStream stream1 = FileSystem.File.Open(path, FileMode.OpenOrCreate,
			FileAccess.Write, FileShare.ReadWrite))
		{
			using FileSystemStream stream2 = FileSystem.File.Open(path, FileMode.OpenOrCreate,
				FileAccess.Write, FileShare.ReadWrite);
			stream2.Position = 3;
			stream1.Position = 2;

			stream2.Write(Encoding.UTF8.GetBytes("CCC"), 0, 3);
			stream1.Write(Encoding.UTF8.GetBytes("bbbbbb"), 0, 6);

			stream1.Flush();
			stream2.Flush();
		}
		
		await That(FileSystem.File.ReadAllText(path)).IsEqualTo("AAbCCCbbAAAA");
	}
	
	[Theory]
	[AutoData]
	public async Task MultipleFlush_DifferentPositionWithGaps_ShouldKeepAdditionalBytes(string path)
	{
		FileSystem.File.WriteAllText(path, "AAAAAAAAAAAA");
		using (FileSystemStream stream1 = FileSystem.File.Open(path, FileMode.OpenOrCreate,
			FileAccess.Write, FileShare.ReadWrite))
		{
			using FileSystemStream stream2 = FileSystem.File.Open(path, FileMode.OpenOrCreate,
				FileAccess.Write, FileShare.ReadWrite);
			stream1.Position = 2;

			stream2.Position = 3;
			stream2.Write(Encoding.UTF8.GetBytes("C"), 0, 1);
			stream2.Position = 5;
			stream2.Write(Encoding.UTF8.GetBytes("C"), 0, 1);
			stream1.Write(Encoding.UTF8.GetBytes("bbbbbb"), 0, 6);

			stream1.Flush();
			stream2.Flush();
		}
		
		await That(FileSystem.File.ReadAllText(path)).IsEqualTo("AAbbbCbbAAAA");
	}
	
	[Theory]
	[AutoData]
	public async Task WriteEmpty_ShouldNotOverwrite(string path)
	{
		using (FileSystemStream stream1 = FileSystem.File.Open(path, FileMode.OpenOrCreate,
			FileAccess.Write, FileShare.ReadWrite))
		{
			using FileSystemStream stream2 = FileSystem.File.Open(path, FileMode.OpenOrCreate,
				FileAccess.Write, FileShare.ReadWrite);

			stream2.Write(Encoding.UTF8.GetBytes(""), 0, 0);
			stream1.Write(Encoding.UTF8.GetBytes("barfoo"), 0, 6);

			stream1.Flush();
			stream2.Flush();
		}
		
		await That(FileSystem.File.ReadAllText(path)).IsEqualTo("barfoo");
	}
}
