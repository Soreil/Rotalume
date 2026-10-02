# Emulator revision comparison

This host uses BenchmarkDotNet's native exporters. The Visual Studio diagnostics package, `CPUUsageDiagnoser`, and `VSDiagnosticsExporter` are intentionally not used because their installed version is incompatible with BenchmarkDotNet 0.16. No Visual Studio CPU trace is collected by this command.

Run from the repository root:

```powershell
dotnet run --project BenchmarkSuite1/BenchmarkSuite1.csproj -c Release -- --filter '*EmulatorRevisionBenchmarks*' --jitTieringMode Skip --exporters json csv --artifacts './BenchmarkDotNet.Artifacts/revision-comparison'
```

Both revisions run on the same .NET runtime, with the same dmg-acid2 ROM, 120-frame startup warmup and 120-frame measured emulated duration. There are graphics-enabled and graphics-disabled cases, two process launches, four warmup iterations and twelve measurement iterations per launch. The frame sink excludes frontend/UI rendering and FPS throttling. Results describe emulator-core throughput, not end-to-end WinUI performance.

`--jitTieringMode Skip` skips BenchmarkDotNet 0.16's extra automatic JIT stage, which repeatedly ran this per-iteration-setup workload without reaching measurements. It does not disable runtime tiered compilation or remove the configured warmup iterations. Both revisions use the same setting; results should be interpreted with the reported variability.

`Current` references the workspace emulator. `Previous` references an aliased assembly built from commit `b65d812`. Do not assume `Current` remains commit `a941ed6` after further source changes.

The baseline files are local, ignored artifacts. To reconstruct them from the repository root:

```powershell
$baseline = Join-Path $PWD '.vs/performance-baseline/b65d812'
New-Item -ItemType Directory -Force $baseline | Out-Null
git archive b65d812 --format=zip --output="$baseline/source.zip" emulator .editorconfig
Expand-Archive "$baseline/source.zip" "$baseline/source" -Force
dotnet build "$baseline/source/emulator/emulator.csproj" -c Release -p:AssemblyName=emulator.PreviousCommit
```

The ROM must be present at `Tests/rom/dmg-acid2/dmg-acid2.gb`. Benchmark setup and cleanup validate LCD enable and produced frames. Keep the workload/setup unchanged when comparing results; the diagnostics workaround only removes the incompatible integration and enables standard command-line selection/export.

## Observed comparison

The native-exporter run completed on .NET 11.0.0-rc.1.26425.128 with BenchmarkDotNet 0.16.0-preview.2 (AMD Ryzen 9 7950X). Current source was `a941ed6`; the baseline was `b65d812`. Each operation measures 120 frames' worth of emulated time, not 120 wall-clock frames.

| Method   | GraphicsEnabled | Mean      | Error    | StdDev   | Ratio | RatioSD |
|--------- |---------------- |----------:|---------:|---------:|------:|--------:|
| Current  | False           |  37.55 ms | 0.481 ms | 0.608 ms |  0.99 |    0.02 |
| Previous | False           |  37.87 ms | 0.259 ms | 0.327 ms |  1.00 |    0.00 |
| Current  | True            | 106.59 ms | 1.159 ms | 1.465 ms |  0.98 |    0.02 |
| Previous | True            | 108.75 ms | 1.112 ms | 1.406 ms |  1.00 |    0.00 |

Elapsed-time differences based on unrounded means: graphics disabled -0.85%; graphics enabled -1.98%. No material regression is observed in this workload. Do not treat these small differences as a definitive speedup: the confidence intervals overlap, the graphics-disabled iterations trigger BenchmarkDotNet's minimum-iteration-time warning, and this is a single-ROM core-only workload with the extra automatic JIT stage skipped. Error is the half-width of the 99.9% confidence interval.

CSV, GitHub Markdown, HTML and full JSON reports are written under `BenchmarkDotNet.Artifacts/revision-comparison/results/`. These are ordinary BenchmarkDotNet timing reports, not Visual Studio profiling traces.
