using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Json;
// ReSharper disable once RedundantUsingDirective
using System;
// ReSharper disable once RedundantUsingDirective
using System.Collections.Generic;

namespace Build;

/// <summary>
///     Pure logic for transforming BenchmarkDotNet reports into the body posted as a PR comment, compared to the
///     results of <c>main</c> from the same runner.
/// </summary>
static class BenchmarkReport
{
	public const string Heading = "## :rocket: Benchmark Results";

	public const double TimeChangeThreshold = 0.10;

	/// <summary>
	///     Leaves room for the summaries within the limit of a GitHub comment.
	/// </summary>
	public const int MaxRawResultsLength = 45000;

	const string BaselineLibrary = "Testably";
	const string Worse = "🔴";
	const string Better = "🟢";

	// ReSharper disable once UnusedMember.Global
	public static readonly string[] DefaultColumnsToRemove = ["RatioSD", "Gen0", "Gen1", "Gen2",];

	/// <summary>
	///     The order of the tabs on the benchmarks page of the docs site.
	/// </summary>
	static readonly string[] BenchmarkOrder =
	[
		"CompleteTestBenchmarks", "InitializationBenchmarks", "MockCreationBenchmarks", "FileRoundTripBenchmarks",
		"FileInfoBenchmarks", "FileStreamBenchmarks", "DirectoryTreeBenchmarks", "EnumerationBenchmarks",
	];

	/// <param name="mainCommitUrl">The link to the commit of <c>main</c> that ran, or <see langword="null" /> when it is unknown.</param>
	public static string BuildBody(IReadOnlyList<BenchmarkReportFile> files, string? mainCommitUrl,
		string[] columnsToRemove)
	{
		files = files
			.OrderBy(file => Array.IndexOf(BenchmarkOrder, file.Name) is var index and >= 0 ? index : int.MaxValue)
			.ThenBy(file => file.Name, StringComparer.Ordinal)
			.ToArray();
		List<string> memoryRows = new();
		List<string> timeRows = new();
		var comparedBenchmarks = 0;
		List<string> newBenchmarks = new();
		List<string> removedBenchmarks = new();
		foreach (var file in files.Where(file => file.MainMeasurementsJson != null))
		{
			var measurements = ReadMeasurements(file.MeasurementsJson);
			var mainMeasurements = ReadMeasurements(file.MainMeasurementsJson!);
			var benchmarks = measurements
				.Where(pair => pair.Value.ContainsKey(BaselineLibrary))
				.Select(pair => pair.Key)
				.Order(StringComparer.Ordinal)
				.ToArray();
			removedBenchmarks.AddRange(mainMeasurements.Keys.Except(benchmarks).Order(StringComparer.Ordinal));
			foreach (var name in benchmarks)
			{
				var current = measurements[name][BaselineLibrary];
				if (!mainMeasurements.TryGetValue(name, out var mainLibraries) ||
					!mainLibraries.TryGetValue(BaselineLibrary, out var main))
				{
					newBenchmarks.Add(name);
					continue;
				}

				if (current.Allocated != main.Allocated)
				{
					memoryRows.Add(
						$"| `{name}` | {FormatBytes(main.Allocated)} | {FormatBytes(current.Allocated)} | {FormatMemoryChange(main.Allocated, current.Allocated)} |");
				}

				var timeChange = current.Mean / main.Mean - 1;
				if (Math.Abs(timeChange) > TimeChangeThreshold)
				{
					timeRows.Add(
						$"| `{name}` | {FormatTime(main.Mean)} | {FormatTime(current.Mean)} | {(timeChange > 0 ? Worse : Better)} {FormatPercent(timeChange)} |");
				}

				comparedBenchmarks++;
			}
		}

		StringBuilder sb = new();
		sb.AppendLine(Heading);
		var filesWithoutMain = files.Where(file => file.MainMeasurementsJson == null).ToArray();
		var compared = filesWithoutMain.Length < files.Count;
		if (compared)
		{
			sb.AppendLine(
				$"Compared to {(mainCommitUrl == null ? "`main`" : $"[`main`]({mainCommitUrl})")}, which ran on the same runner. Times that change by more than {FormatPercent(TimeChangeThreshold).TrimStart('+')} are marked.");
		}

		if (filesWithoutMain.Length > 0)
		{
			sb.AppendLine(
				$"⚠️ There are no results of `main` for {string.Join(", ", filesWithoutMain.Select(file => $"`{file.Name}`"))}, so they are not compared.");
		}

		sb.AppendLine();
		if (compared && memoryRows.Count == 0 && timeRows.Count == 0)
		{
			sb.AppendLine($"✅ No changes in memory or time ({comparedBenchmarks} benchmarks)");
			sb.AppendLine();
		}

		AppendChangeTable(sb, "Memory", memoryRows);
		AppendChangeTable(sb, "Time", timeRows);
		if (newBenchmarks.Count > 0)
		{
			sb.AppendLine($"New, without results on `main`: {string.Join(", ", newBenchmarks.Select(name => $"`{name}`"))}");
			sb.AppendLine();
		}

		if (removedBenchmarks.Count > 0)
		{
			sb.AppendLine($"Removed: {string.Join(", ", removedBenchmarks.Select(name => $"`{name}`"))}");
			sb.AppendLine();
		}

		var rawResultsLength = 0;
		foreach (var file in files)
		{
			var rawResults = CreateRawResults(file.Name, file.ReportLines, columnsToRemove);
			// The comparison takes precedence over the raw results when the comment would get too long.
			rawResultsLength += rawResults.Length;
			sb.AppendLine(rawResultsLength <= MaxRawResultsLength
				? rawResults
				: $"_The raw results of `{file.Name}` do not fit into this comment, see the artifact `Benchmarks-{file.Name}` of the workflow run._");
			sb.AppendLine();
		}

		return sb.ToString();
	}

	static void AppendChangeTable(StringBuilder sb, string title, List<string> rows)
	{
		if (rows.Count == 0)
		{
			return;
		}

		sb.AppendLine($"**{title}**");
		sb.AppendLine();
		sb.AppendLine("| Benchmark | main | PR | Δ |");
		sb.AppendLine("|---|--:|--:|--:|");
		rows.ForEach(row => sb.AppendLine(row));
		sb.AppendLine();
	}

	/// <summary>
	///     Reads the measurements of each library per benchmark from a full BenchmarkDotNet JSON report, keyed like the
	///     charts of the benchmark page.
	/// </summary>
	internal static Dictionary<string, Dictionary<string, BenchmarkMeasurement>> ReadMeasurements(string json)
	{
		Dictionary<string, Dictionary<string, BenchmarkMeasurement>> result = new();
		using var document = JsonDocument.Parse(json);
		foreach (var benchmark in document.RootElement.GetProperty("Benchmarks").EnumerateArray())
		{
			var method = benchmark.GetProperty("Method").GetString()!;
			var index = method.LastIndexOf('_');
			if (index <= 0)
			{
				continue;
			}

			var parameters = benchmark.TryGetProperty("Parameters", out var p) ? p.GetString() : null;
			var scenario = method.Substring(0, index);
			var name = string.IsNullOrWhiteSpace(parameters) ? scenario : $"{scenario} ({parameters})";
			var allocated = benchmark.GetProperty("Metrics").EnumerateArray()
				.Where(metric => metric.GetProperty("Descriptor").GetProperty("Id").GetString() == "Allocated Memory")
				.Select(metric => metric.GetProperty("Value").GetDouble())
				.FirstOrDefault();
			if (!result.TryGetValue(name, out var libraries))
			{
				libraries = new Dictionary<string, BenchmarkMeasurement>();
				result[name] = libraries;
			}

			libraries[method.Substring(index + 1)] = new BenchmarkMeasurement(
				benchmark.GetProperty("Statistics").GetProperty("Mean").GetDouble(), allocated);
		}

		return result;
	}

	internal static string FormatPercent(double relativeChange)
		=> (relativeChange * 100).ToString("+0;-0;±0", CultureInfo.InvariantCulture) + "%";

	internal static string FormatBytes(double bytes)
		=> bytes.ToString("N0", CultureInfo.InvariantCulture) + " B";

	internal static string FormatMemoryChange(double main, double current)
	{
		var difference = current - main;
		var change =
			$"{(difference > 0 ? Worse : Better)} {difference.ToString("+#,0;-#,0", CultureInfo.InvariantCulture)} B";
		return main == 0 ? change : $"{change} ({FormatPercent(current / main - 1)})";
	}

	internal static string FormatTime(double nanoseconds)
		=> nanoseconds switch
		{
			< 1_000 => nanoseconds.ToString("0.0", CultureInfo.InvariantCulture) + " ns",
			< 1_000_000 => (nanoseconds / 1_000).ToString("0.00", CultureInfo.InvariantCulture) + " μs",
			_ => (nanoseconds / 1_000_000).ToString("0.00", CultureInfo.InvariantCulture) + " ms",
		};

	internal static string CreateRawResults(string name, string[] reportLines, string[] columnsToRemove)
	{
		StringBuilder sb = new();
		sb.AppendLine("<details>");
		sb.AppendLine($"<summary>{name}</summary>");
		sb.AppendLine();
		BenchmarkTableParser parser = new(columnsToRemove);
		List<TableRow> tableBuffer = new();
		// The environment, like the BenchmarkDotNet version and the hardware, follows the results.
		StringBuilder environment = new();
		var inCodeBlock = false;
		foreach (var line in reportLines)
		{
			if (line.StartsWith("```"))
			{
				FlushTableBuffer(sb, tableBuffer);
				inCodeBlock = !inCodeBlock;
				parser.Reset();
				continue;
			}

			if (inCodeBlock)
			{
				environment.AppendLine(line);
				continue;
			}

			if (!parser.TryConsume(line, out var rowIndex, out _, out var filteredTokens))
			{
				FlushTableBuffer(sb, tableBuffer);
				sb.AppendLine(line);
				continue;
			}

			tableBuffer.Add(new TableRow(rowIndex, filteredTokens));
		}

		FlushTableBuffer(sb, tableBuffer);
		if (environment.Length > 0)
		{
			sb.AppendLine();
			sb.AppendLine("<pre>");
			sb.Append(environment);
			sb.AppendLine("</pre>");
		}

		sb.AppendLine();
		sb.AppendLine("</details>");
		return sb.ToString();
	}

	internal static void FlushTableBuffer(StringBuilder sb, List<TableRow> tableBuffer)
	{
		if (tableBuffer.Count == 0) return;

		var commonPrefix = FindCommonRowPrefix(tableBuffer);
		var headerRow = tableBuffer[0].RowIndex == 0 ? tableBuffer[0] : null;

		foreach (var row in tableBuffer)
		{
			if (row.RowIndex >= BenchmarkTableParser.DataRowStartIndex && row.Tokens.Length == 0)
			{
				if (headerRow != null && headerRow.Tokens.Length > 0)
				{
					var empty = new string[headerRow.Tokens.Length];
					Array.Fill(empty, string.Empty);
					sb.AppendLine(JoinTokens(empty));
				}

				continue;
			}

			var displayLine = JoinTokens(ApplyHeaderAndPrefixStripping(row, commonPrefix));

			var isBaselineRow = row.RowIndex >= BenchmarkTableParser.DataRowStartIndex
								&& row.Tokens[0].EndsWith("_" + BaselineLibrary, StringComparison.OrdinalIgnoreCase);

			if (isBaselineRow)
				MakeLineBold(sb, displayLine);
			else
				sb.AppendLine(displayLine);
		}

		tableBuffer.Clear();
	}

	internal static string[] ApplyHeaderAndPrefixStripping(TableRow row, string? commonPrefix)
	{
		if (commonPrefix == null || row.Tokens.Length == 0) return row.Tokens;

		var result = (string[])row.Tokens.Clone();
		if (row.RowIndex == 0)
			result[0] = commonPrefix.TrimEnd('_');
		else if (row.RowIndex >= BenchmarkTableParser.DataRowStartIndex
				 && result[0].StartsWith(commonPrefix, StringComparison.Ordinal))
			result[0] = result[0].Substring(commonPrefix.Length);

		return result;
	}

	internal static string? FindCommonRowPrefix(List<TableRow> rows)
	{
		string? prefix = null;
		foreach (var row in rows)
		{
			if (row.RowIndex < BenchmarkTableParser.DataRowStartIndex) continue;

			if (row.Tokens.Length == 0) continue;

			var name = row.Tokens[0];
			var underscoreIdx = name.IndexOf('_');
			if (underscoreIdx <= 0) return null;

			var thisPrefix = name.Substring(0, underscoreIdx + 1);
			if (prefix == null)
				prefix = thisPrefix;
			else if (!string.Equals(prefix, thisPrefix, StringComparison.Ordinal)) return null;
		}

		return prefix;
	}

	internal static string JoinTokens(string[] tokens)
	{
		StringBuilder sb = new();
		sb.Append('|');
		foreach (var token in tokens)
		{
			sb.Append(' ');
			sb.Append(token);
			sb.Append(" |");
		}

		return sb.ToString();
	}

	internal static int[] DetermineDroppedColumnIndices(string headerLine, string[] columnsToRemove)
	{
		var tokens = SplitTableRow(headerLine);
		List<int> indices = new();
		for (var i = 0; i < tokens.Length; i++)
			foreach (var columnName in columnsToRemove)
				if (string.Equals(tokens[i], columnName, StringComparison.OrdinalIgnoreCase))
				{
					indices.Add(i);
					break;
				}

		return indices.ToArray();
	}

	internal static string RemoveColumns(string line, int[] droppedColumnIndices)
	{
		if (droppedColumnIndices.Length == 0) return line;

		var tokens = SplitTableRow(line);
		StringBuilder result = new();
		result.Append('|');
		for (var i = 0; i < tokens.Length; i++)
		{
			if (Array.IndexOf(droppedColumnIndices, i) >= 0) continue;

			result.Append(' ');
			result.Append(tokens[i]);
			result.Append(" |");
		}

		return result.ToString();
	}

	internal static string StripMarkdownBold(string token)
	{
		if (token.Length >= 4 && token.StartsWith("**", StringComparison.Ordinal)
							  && token.EndsWith("**", StringComparison.Ordinal))
			return token.Substring(2, token.Length - 4);

		return token;
	}

	internal static string[] SplitTableRow(string line)
	{
		var parts = line.Split('|', StringSplitOptions.TrimEntries);
		if (parts.Length < 2) return [];

		var tokens = new string[parts.Length - 2];
		Array.Copy(parts, 1, tokens, 0, tokens.Length);
		return tokens;
	}

	internal static void MakeLineBold(StringBuilder sb, string line)
	{
		var tokens = line.Split("|", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
		sb.Append('|');
		foreach (var token in tokens)
		{
			sb.Append(" **");
			sb.Append(token);
			sb.Append("** |");
		}

		sb.AppendLine();
	}
}

/// <param name="Name">The benchmark class, which is also the suffix of its workflow artifact.</param>
/// <param name="MainMeasurementsJson">The report of <c>main</c> from the same runner, or <see langword="null" /> when it is missing.</param>
sealed record BenchmarkReportFile(string Name, string[] ReportLines, string MeasurementsJson, string? MainMeasurementsJson);

sealed record TableRow(int RowIndex, string[] Tokens);

sealed record BenchmarkMeasurement(double Mean, double Allocated);

/// <summary>
///     Parses the markdown tables produced by BenchmarkDotNet's GitHub exporter.
///     Tables are framed by a header row (index 0), a separator row like <c>|---|---|</c> (index 1), and one or more
///     data rows (index >= <see cref="DataRowStartIndex" />). The parser auto-resets on the first non-table line that
///     follows a table, so a single instance can walk a document that contains multiple tables.
/// </summary>
sealed class BenchmarkTableParser
{
	public const int DataRowStartIndex = 2;

	// ReSharper disable InconsistentNaming
	readonly string[] _columnsToRemove;
	int[]? _droppedColumnIndices;
	int _nextRowIndex;
	// ReSharper restore InconsistentNaming

	public BenchmarkTableParser(string[] columnsToRemove)
	{
		_columnsToRemove = columnsToRemove;
	}

	public bool TryConsume(string line, out int rowIndex,
		[NotNullWhen(true)] out string? filteredLine,
		[NotNullWhen(true)] out string[]? tokens)
	{
		rowIndex = -1;
		if (!line.StartsWith('|') || !line.EndsWith('|'))
		{
			if (_nextRowIndex > 0) Reset();

			filteredLine = null;
			tokens = null;
			return false;
		}

		_droppedColumnIndices ??= BenchmarkReport.DetermineDroppedColumnIndices(line, _columnsToRemove);
		filteredLine = BenchmarkReport.RemoveColumns(line, _droppedColumnIndices);
		tokens = filteredLine.Split('|', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
		for (var i = 0; i < tokens.Length; i++) tokens[i] = BenchmarkReport.StripMarkdownBold(tokens[i]);

		rowIndex = _nextRowIndex;
		_nextRowIndex++;
		return true;
	}

	public void Reset()
	{
		_droppedColumnIndices = null;
		_nextRowIndex = 0;
	}
}
