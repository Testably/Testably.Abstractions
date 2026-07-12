using System;
using System.Collections.Generic;

namespace Testably.Abstractions;

/// <summary>
///     A copy-on-write view backing that emulates the page privatization of real memory-mapped
///     files: reads pass through to the shared backing until this view writes to a page; the
///     written page is copied into private memory at that moment and is from then on isolated
///     from the shared backing in both directions.
/// </summary>
internal sealed class CopyOnWriteViewBacking(MemoryMappedViewBacking shared, long capacity)
	: MemoryMappedViewBacking
{
	/// <summary>
	///     The granularity at which written ranges are privatized, matching the 4096-byte pages
	///     of the operating systems backing the real memory-mapped file.
	/// </summary>
	private const int PageSize = 4096;

#if NET9_0_OR_GREATER
	private readonly System.Threading.Lock _lock = new();
#else
	private readonly object _lock = new();
#endif
	private readonly Dictionary<long, byte[]> _privatePages = new();

	/// <inheritdoc cref="MemoryMappedViewBacking.Flush()" />
	public override void Flush()
	{
		// Copy-on-write changes are never persisted to the underlying file.
	}

	/// <inheritdoc cref="MemoryMappedViewBacking.ReadAt(long, byte[], int, int)" />
	public override void ReadAt(long position, byte[] buffer, int offset, int count)
	{
		lock (_lock)
		{
			if (_privatePages.Count == 0)
			{
				// No page has been privatized yet, so the whole range comes from the shared
				// backing in one call instead of one call per 4096-byte page.
				shared.ReadAt(position, buffer, offset, count);
				return;
			}

			int read = 0;
			while (read < count)
			{
				long pageIndex = (position + read) / PageSize;
				int offsetInPage = (int)((position + read) % PageSize);
				int chunk = Math.Min(count - read, PageSize - offsetInPage);
				if (_privatePages.TryGetValue(pageIndex, out byte[]? page))
				{
					Array.Copy(page, offsetInPage, buffer, offset + read, chunk);
				}
				else
				{
					shared.ReadAt(position + read, buffer, offset + read, chunk);
				}

				read += chunk;
			}
		}
	}

	/// <inheritdoc cref="MemoryMappedViewBacking.WriteAt(long, byte[], int, int)" />
	public override void WriteAt(long position, byte[] buffer, int offset, int count)
	{
		lock (_lock)
		{
			int written = 0;
			while (written < count)
			{
				long pageIndex = (position + written) / PageSize;
				int offsetInPage = (int)((position + written) % PageSize);
				int chunk = Math.Min(count - written, PageSize - offsetInPage);
				byte[] page = GetOrCreatePrivatePage(pageIndex);
				Array.Copy(buffer, offset + written, page, offsetInPage, chunk);
				written += chunk;
			}
		}
	}

	private byte[] GetOrCreatePrivatePage(long pageIndex)
	{
		if (!_privatePages.TryGetValue(pageIndex, out byte[]? page))
		{
			page = new byte[PageSize];
			long pageStart = pageIndex * PageSize;
			int available = (int)Math.Min(PageSize, capacity - pageStart);
			shared.ReadAt(pageStart, page, 0, available);
			_privatePages[pageIndex] = page;
		}

		return page;
	}
}
