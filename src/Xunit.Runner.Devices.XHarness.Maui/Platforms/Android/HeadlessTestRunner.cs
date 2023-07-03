﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.OS;
using Microsoft.DotNet.XHarness.XHarnesss.Common;
using Microsoft.DotNet.XHarness.XHarnesss.Xunit;

namespace Xunit.Runner.Devices.XHarness.Maui
{
	class HeadlessXHarness : AndroidApplicationEntryPoint
	{
		readonly HeadlessRunnerOptions _runnerOptions;
		readonly RunnerOptions _options;
		readonly string _resultsPath;

		public HeadlessXHarness(HeadlessRunnerOptions runnerOptions, RunnerOptions options)
		{
			_runnerOptions = runnerOptions;
			_options = options;

			var cache = Application.Context.CacheDir!.AbsolutePath;
			_resultsPath = Path.Combine(cache, _runnerOptions.TestResultsFilename);
		}

		protected override bool LogExcludedTests => true;

		public override TextWriter? Logger => null;

		public override string TestsResultsFinalPath => _resultsPath;

		protected override int? MaxParallelThreads => System.Environment.ProcessorCount;

		protected override IDevice Device { get; } = new TestDevice();

		protected override IEnumerable<TestAssemblyInfo> GetTestAssemblies() =>
			_options.Assemblies
				.Distinct()
				.Select(assembly =>
				{
					// Android needs this file to "exist" but it uses the assembly actually.
					var path = Path.Combine(Application.Context.CacheDir!.AbsolutePath, assembly.GetName().Name + ".dll");
					if (!File.Exists(path))
						File.Create(path).Close();

					return new TestAssemblyInfo(assembly, path);
				});

		protected override void TerminateWithSuccess() { }

		protected override XHarness GetXHarness(LogWriter logWriter)
		{
			var testRunner = base.GetXHarness(logWriter);
			if (_options.SkipCategories?.Count > 0)
				testRunner.SkipCategories(_options.SkipCategories);
			return testRunner;
		}

		public async Task<Bundle> RunTestsAsync()
		{
			var bundle = new Bundle();

			TestsCompleted += OnTestsCompleted;

			await RunAsync();

			TestsCompleted -= OnTestsCompleted;

			if (File.Exists(TestsResultsFinalPath))
				bundle.PutString("test-results-path", TestsResultsFinalPath);

			if (bundle.GetLong("return-code", -1) == -1)
				bundle.PutLong("return-code", 1);

			return bundle;

			void OnTestsCompleted(object? sender, TestRunResult results)
			{
				var message =
					$"Tests run: {results.ExecutedTests} " +
					$"Passed: {results.PassedTests} " +
					$"Inconclusive: {results.InconclusiveTests} " +
					$"Failed: {results.FailedTests} " +
					$"Ignored: {results.SkippedTests}";

				bundle.PutString("test-execution-summary", message);

				bundle.PutLong("return-code", results.FailedTests == 0 ? 0 : 1);
			}
		}
	}
}