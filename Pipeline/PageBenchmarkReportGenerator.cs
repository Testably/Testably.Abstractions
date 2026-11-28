#nullable disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Serilog;

// ReSharper disable CollectionNeverQueried.Local
// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Local

namespace Build;

/// <summary>
///     Aggregates BenchmarkDotNet JSON reports into the <c>data.js</c> / <c>limited-data.js</c> files consumed
///     by the documentation site (<see href="https://docs.testably.org/Abstractions/benchmarks" />).
/// </summary>
/// <remarks>
///     Mirrors the generator of Mockolate, so that the docs site reads both the same way. Chart keys include the
///     BenchmarkDotNet parameter sets (e.g. <c>EnumerateAll (N=100)</c>), so that each <c>[Params]</c> combination
///     becomes its own chart.
/// </remarks>
public class PageBenchmarkReportGenerator
{
	private const string FilePrefix = "window.BENCHMARK_DATA = ";

	internal const string BaselineLibrary = "Testably";

	private static readonly JsonSerializerOptions BenchmarkSerializerOptions = new()
	{
		WriteIndented = true,
	};

	public static (string FullContent, string LimitedContent) Append(CommitInfo commitInfo,
		string currentFileContent, List<string> benchmarkReportsContents, int limit)
	{
		if (!currentFileContent.StartsWith(FilePrefix))
		{
			throw new NotSupportedException($"The benchmark data file is incorrect (does not start with {FilePrefix})");
		}

		PageReportData pageReport =
			JsonSerializer.Deserialize<PageReportData>(currentFileContent.Substring(FilePrefix.Length));

		if (pageReport.Values.Any(r => r.Commits.Any(c => c.Sha == commitInfo.Sha)))
		{
			Log.Warning(
				"The benchmark already has data for {Sha}: {Message} by {Author} on {Date}",
				commitInfo.Sha, commitInfo.Message, commitInfo.Author, commitInfo.Date);
			return (null, null);
		}

		Log.Debug(
			"Updating benchmark report for {Sha}: {Message} by {Author} on {Date}",
			commitInfo.Sha, commitInfo.Message, commitInfo.Author, commitInfo.Date);

		foreach (string benchmarkReportContent in benchmarkReportsContents)
		{
			BenchmarkReport benchmarkReport = JsonSerializer.Deserialize<BenchmarkReport>(benchmarkReportContent);
			if (!pageReport.Append(commitInfo, benchmarkReport))
			{
				throw new NotSupportedException("The new benchmark data is incorrect");
			}
		}

		string newFileContent =
			$"{FilePrefix}{JsonSerializer.Serialize(pageReport, BenchmarkSerializerOptions)}";
		string limitedFileContent =
			$"{FilePrefix}{JsonSerializer.Serialize(pageReport.Limit(limit), BenchmarkSerializerOptions)}";
		return (newFileContent, limitedFileContent);
	}

	internal sealed class PageReportData : Dictionary<string, PageReport>
	{
		public bool Append(CommitInfo commitInfo, BenchmarkReport benchmarkReport)
		{
			HashSet<string> chartsTouchedByBaseline = new();
			foreach (BenchmarkReport.Benchmark benchmark in benchmarkReport.Benchmarks)
			{
				if (!Append(commitInfo, benchmark, chartsTouchedByBaseline))
				{
					return false;
				}
			}

			return true;
		}

		private bool Append(CommitInfo commitInfo, BenchmarkReport.Benchmark benchmark,
			HashSet<string> chartsTouchedByBaseline)
		{
			if (!ParseMethod(benchmark.Method, out string scenario, out string library))
			{
				return false;
			}

			if (!IsIncluded(library))
			{
				return true;
			}

			string chartKey = BuildChartKey(scenario, benchmark.Parameters);
			if (!TryGetValue(chartKey, out PageReport pageReport))
			{
				pageReport = new PageReport();
				this[chartKey] = pageReport;
			}

			if (library == BaselineLibrary && chartsTouchedByBaseline.Add(chartKey))
			{
				pageReport.Commits.Add(commitInfo);
				pageReport.Labels.Add(commitInfo.Sha.Substring(0, 8));
			}

			AppendTimeDataset(benchmark, pageReport, library);
			AppendMemoryDataset(benchmark, pageReport, library);

			return true;
		}

		private static string BuildChartKey(string scenario, string parameters)
			=> string.IsNullOrWhiteSpace(parameters) ? scenario : $"{scenario} ({parameters})";

		private static void AppendMemoryDataset(BenchmarkReport.Benchmark benchmark, PageReport pageReport,
			string library)
		{
			PageReport.Dataset memoryDataset = pageReport.Datasets.FirstOrDefault(x
				=> x.Label.StartsWith(library, StringComparison.OrdinalIgnoreCase) && x.YAxisId == "y1");
			if (memoryDataset == null)
			{
				memoryDataset = new PageReport.Dataset
				{
					Label = $"{library} memory",
					Unit = "b",
					PointStyle = "triangle",
					BorderDash = [5, 5,],
					YAxisId = "y1",
					BackgroundColor = GetColor(library),
					BorderColor = GetColor(library),
					Data = new List<double>(),
				};
				pageReport.Datasets.Add(memoryDataset);
			}

			memoryDataset.Data.Add(benchmark.Metrics
				.Where(x => x.Descriptor.Id == "Allocated Memory")
				.Select(x => x.Value)
				.FirstOrDefault(double.NaN));
		}

		private static void AppendTimeDataset(BenchmarkReport.Benchmark benchmark, PageReport pageReport, string library)
		{
			PageReport.Dataset timeDataset = pageReport.Datasets.FirstOrDefault(x
				=> x.Label.StartsWith(library, StringComparison.OrdinalIgnoreCase) && x.YAxisId == "y");
			if (timeDataset == null)
			{
				timeDataset = new PageReport.Dataset
				{
					Label = $"{library} time",
					Unit = "ns",
					PointStyle = "circle",
					YAxisId = "y",
					BackgroundColor = GetColor(library),
					BorderColor = GetColor(library),
					Data = new List<double>(),
				};
				pageReport.Datasets.Add(timeDataset);
			}

			timeDataset.Data.Add(benchmark.Statistics.Mean);
		}

		private static bool IsIncluded(string library)
			=> library is "Testably" or "SystemIOAbstractions";

		private static string GetColor(string library)
			=> library switch
			{
				"Testably" => "#63A2AC",
				"SystemIOAbstractions" => "#A052B0",
				_ => "#888888",
			};

		private static bool ParseMethod(string method, out string scenario, out string library)
		{
			int index = method.LastIndexOf('_');
			if (index <= 0)
			{
				scenario = null;
				library = null;
				return false;
			}

			scenario = method.Substring(0, index);
			library = method.Substring(index + 1);
			return true;
		}

		public PageReportData Limit(int limit)
		{
			PageReportData pageReportData = new();
			foreach ((string key, PageReport pageReport) in this)
			{
				pageReportData[key] = pageReport.Limit(limit);
			}

			return pageReportData;
		}
	}

	public class CommitInfo(string sha, string author, string date, string message)
	{
		[JsonPropertyName("sha")] public string Sha { get; } = sha;
		[JsonPropertyName("author")] public string Author { get; } = author;
		[JsonPropertyName("date")] public string Date { get; } = date;
		[JsonPropertyName("message")] public string Message { get; } = message;
	}

	internal sealed class PageReport
	{
		[JsonPropertyName("commits")] public List<CommitInfo> Commits { get; init; } = new();
		[JsonPropertyName("labels")] public List<string> Labels { get; init; } = new();

		[JsonPropertyName("datasets")] public List<Dataset> Datasets { get; init; } = new();

		public PageReport Limit(int limit)
			=> new()
			{
				Commits = Commits.TakeLast(limit).ToList(),
				Labels = Labels.TakeLast(limit).ToList(),
				Datasets = Datasets.Select(dataset => dataset.Limit(limit)).ToList(),
			};

		public class Dataset
		{
			[JsonPropertyName("label")] public string Label { get; init; }
			[JsonPropertyName("unit")] public string Unit { get; set; }

			[JsonPropertyName("data")] public List<double> Data { get; init; }

			[JsonPropertyName("borderColor")] public string BorderColor { get; set; }

			[JsonPropertyName("backgroundColor")] public string BackgroundColor { get; set; }

			[JsonPropertyName("yAxisID")] public string YAxisId { get; init; }

			[JsonPropertyName("borderDash")] public int[] BorderDash { get; set; } = [];

			[JsonPropertyName("pointStyle")] public string PointStyle { get; set; }

			public Dataset Limit(int limit)
				=> new()
				{
					Label = Label,
					Unit = Unit,
					Data = Data.TakeLast(limit).ToList(),
					BorderColor = BorderColor,
					BackgroundColor = BackgroundColor,
					YAxisId = YAxisId,
					BorderDash = BorderDash,
					PointStyle = PointStyle,
				};
		}
	}

	internal sealed class BenchmarkReport
	{
		public Benchmark[] Benchmarks { get; init; }

		public class Benchmark
		{
			public string Type { get; init; }
			public string Method { get; init; }
			public string Parameters { get; init; }
			public BenchmarkStatistics Statistics { get; init; }
			public BenchmarkMetrics[] Metrics { get; init; }
		}

		public class BenchmarkStatistics
		{
			public double Mean { get; init; }
		}

		public class BenchmarkMetrics
		{
			public double Value { get; init; }
			public BenchmarkMetricDescriptor Descriptor { get; init; }
		}

		public class BenchmarkMetricDescriptor
		{
			public string Id { get; init; }
			public string DisplayName { get; init; }
			public string Unit { get; init; }
		}
	}
}
