<div align="center">
	<h1>Project Reloaded: Signature Scanning</h1>
	<img src="https://i.imgur.com/BjPn7rU.png" width="150" align="center" />
	<br/> <br/>
	<strong><i>It's fast enough. I think...</i></strong>
	<br/> <br/>
	<!-- Coverage -->
	<a href="https://codecov.io/gh/Reloaded-Project/Reloaded.Memory.SigScan">
		<img src="https://codecov.io/gh/Reloaded-Project/Reloaded.Memory.SigScan/branch/master/graph/badge.svg" alt="Coverage" />
	</a>
	<!-- NuGet -->
	<a href="https://www.nuget.org/packages/Reloaded.Memory.SigScan">
		<img src="https://img.shields.io/nuget/v/Reloaded.Memory.SigScan.svg" alt="NuGet" />
	</a>
	<!-- Build Status -->
	<a href="https://github.com/Reloaded-Project/Reloaded.Memory.SigScan/actions/workflows/build-and-publish.yml">
		<img src="https://img.shields.io/github/workflow/status/Reloaded-Project/Reloaded.Memory.SigScan/Build%20and%20Publish" alt="Build Status" />
	</a>
</div>

- [Introduction](#introduction)
  - [Usage](#usage)
  - [Reference Benchmarks](#reference-benchmarks)
    - [Implementation Comparison](#implementation-comparison)
    - [Small Data](#small-data)
    - [Big Data](#big-data)
    - [Other Benchmarks](#other-benchmarks)
  - [Reloaded-II Shared Library](#reloaded-ii-shared-library)
  - [Acknowledgements](#acknowledgements)

# Introduction

`Reloaded.Memory.SigScan`, is a pattern matching library for scanning byte signatures.  

Signature scanning is often used in malware analysis (AV Software) and game hacking.  
Usually both of those are the domain of native C/C++ code and at the time of writing this library, there existed very little on the managed .NET front that focuses on performance.  

`Reloaded.Memory.SigScan` is biased towards ease of use and most importantly speed, achieving a staggering > 5GB/s on a in non-vectorised searches and over 30GB/s on vectorised searches on a single thread.

## Usage

Patterns are specified as hex values, without prefix and delimited by spaces.  
`??` represents a wildcard, i.e. any byte can be present in the place of the wildcard.  

**Search the code of the current process**:
```csharp
// `FindPattern` will automatically use the most 
// efficient scanning method available on the current machine.  
var thisProcess = Process.GetCurrentProcess();
var scanner     = new Scanner(thisProcess, thisProcess.MainModule);
int offset      = scanner.FindPattern("04 25 12 ?? ?? E5 E3");
```

**Search an offset in a byte array:**
```csharp
var scanner     = new Scanner(data);
int offset      = scanner.FindPattern("02 11 25 AB");
```

**Scan for multiple signatures (multithreaded)**
```csharp
var scanner = new Scanner(data);
var multiplePatterns = new string[]
{
    "7A BB",
    "9F AB",
    "7A BB"
};

// Note: Use FindPatternsCached if your list of 
// patterns might contain duplicates.
var results = scanner.FindPatterns(multiplePatterns);
```

## Reference Benchmarks

All benchmarks were performed on a machine running:  
- Core i7 4790k (4.4GHz OC)  
- DDR3 1866MHz RAM (CL9 timings)  
- .NET 5 Runtime  

The provided "Speed" column is an effective measure of pattern matching speed in MB/s.  

- `NumItems`: Number of signatures scanned.  
- `ST`: Single Threaded.  (baseline reference)
- `MT`: Multi Threaded.  
- `LB`: Load Balancing On.  
- `Compiled`: Non-Vectorized implementation.  

### Implementation Comparison

Scanning a small data (3MiB) with known signature at end of data.  

```
|   Method |       Mean |    Error |   StdDev | Ratio | Speed (MB/s) |
|--------- |-----------:|---------:|---------:|------:|------------- |
|      Avx |   230.8 us |  2.72 us |  2.41 us |  0.14 |     13631.33 |
|      Sse |   288.6 us |  2.45 us |  2.05 us |  0.18 |     10900.74 |
| Compiled | 1,596.8 us | 14.54 us | 13.60 us |  1.00 |      1970.06 |
|   Simple | 3,087.1 us | 14.83 us | 13.87 us |  1.93 |      1019.00 |
```

### Small Data

Scanning a small data (3MiB) with random pre-selected 12 byte signatures.  

```
|         Method | NumItems | Speed (MB/s) |
|--------------- |--------- |------------- |
|      Random_ST |        1 |     13618.03 |
| Random_MT_NoLB |        4 |     26394.96 |
|   Random_MT_LB |        4 |     26326.17 |
|   Random_MT_LB |       16 |     46032.68 |
| Random_MT_NoLB |       16 |     50459.84 |
|   Random_MT_LB |       64 |     56748.39 |
| Random_MT_NoLB |       64 |     57970.04 |
|   Random_MT_LB |      256 |     63418.04 |
| Random_MT_NoLB |      256 |     63746.84 |
```

### Big Data

Scanning a random big data (200MiB) with random pre-selected 12 byte signatures.  

```
|             Method | NumItems | Speed (MB/s) |
|------------------- |--------- |------------- |
| Random_ST_Compiled |        1 |      5339.74 |
|          Random_ST |        1 |     30025.36 |
|     Random_MT_NoLB |        4 |     44669.77 |
|       Random_MT_LB |        4 |     44572.28 |
|       Random_MT_LB |       16 |     60354.57 |
|     Random_MT_NoLB |       16 |     64928.73 |
|       Random_MT_LB |       64 |     59824.99 |
|     Random_MT_NoLB |       64 |     60502.48 |
|     Random_MT_NoLB |      256 |     55931.01 |
|       Random_MT_LB |      256 |     56763.69 |
```

Speed loss in cases of >16 items suggests increased cache misses.  
Might be worth investigating in a future version.  

### Other Benchmarks
For other benchmarks, please see the `Reloaded.Memory.Sigscan.Benchmark` project in this repository.

To run the benchmarks yourself, simply run `Reloaded.Memory.Sigscan.Benchmark` after compiling in `Release` mode.

`Reloaded.Memory.Sigscan.Benchmark` is powered by [BenchmarkDotNet](https://github.com/dotnet/BenchmarkDotNet).

## Reloaded-II Shared Library

[For information on consuming Shared Libraries, see [Dependency Injection in Reloaded II](https://reloaded-project.github.io/Reloaded-II/DependencyInjection_Consumer/).]

<a href="https://www.nuget.org/packages/Reloaded.Memory.SigScan.ReloadedII.Interfaces">
    <img alt="Nuget" src="https://img.shields.io/nuget/v/Reloaded.Memory.SigScan.ReloadedII.Interfaces">
</a>

`Reloaded.Memory.SigScan.ReloadedII.Interfaces` [(NuGet)](https://www.nuget.org/packages/Reloaded.Memory.SigScan.ReloadedII.Interfaces) exposes the following APIs:  
- `IStartupScanner`: Queues signature scans for batch parallel scanning.   
- `IScannerFactory`: Creates `Scanner` instances.  

It is highly recommended that you use `IStartupScanner` in your mods.  
Running signature scans in parallel provides very significant gains to startup time.  

Scans submitted to this class are performed in the order they are submitted.  
There is no need to worry about conflicts/race conditions.  

Example:  
```csharp
void Start() 
{
    // ... code omitted
    _modLoader.GetController<IStartupScanner>().TryGetTarget(out var startupScanner);
    startupScanner.AddMainModuleScan("C3", OnMainModuleScan);
}

private void OnMainModuleScan(PatternScanResult result)
{
    _logger.WriteLine($"Found `ret` at: {result.Offset}");
}
```

If you need additional APIs in `IStartupScanner`, such as scanning custom/different ranges, please let me know. For now the API only handles the common use case.

## Acknowledgements

Vectorised implementations of `Reloaded.Memory.Sigscan` are based off of a modified version of `LazySIMD` by [uberhalit](https://github.com/uberhalit).
