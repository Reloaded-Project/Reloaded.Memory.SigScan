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

# Table of Contents

- [Introduction](#introduction)
- [Functionality](#functionality)
    - [Features](#features)
        - [Fast, Very Fast](#fast-very-fast)
        - [Simple to Use](#simple-to-use)
    - [Future Features](#future-features)
        - [Wildcards smaller than 1 byte.](#wildcards-smaller-than-1-byte)
- [Sample Benchmarks](#sample-benchmarks)
    - [Configuration](#configuration)
    - [Small Signatures (1-8 bytes)](#small-signatures-1-8-bytes)
    - [Long Signatures (8+ bytes)](#long-signatures-8-bytes)
    - [Worst Case Scenario](#worst-case-scenario)
    - [Other Benchmarks](#other-benchmarks)

# Introduction

`Reloaded.Memory.SigScan`, is a pattern matching library for scanning byte signatures. It is biased towards ease of use and most importantly speed, achieving a staggering 2GB/s on a single processor thread in non-vectorised searches and over 10GB/s on vectorised searches.

# Functionality

### Features

#### Fast, Very Fast

Speed is the primary reason this repository exists.

The current non-vectorized implementation, at the time of writing can reach upwards of 2000MB/s on a single thread of a modern processor.  
The current vectorized implementation breaks the 10000MB/s barrier.  

Signature scanning is most often used in malware analysis (AV Software) and game hacking, however both of those are by far the domain of native C/C++ code. There exists very little on the managed .NET front, this library was written as I was not able to find a .NET signature scanning library that focuses on performance.

#### Simple to Use

**Search the code of the current process**:
```csharp
var thisProcess = Process.GetCurrentProcess();
var scanner     = new Scanner(thisProcess, thisProcess.MainModule);
int offset      = scanner.FindPattern("04 25 12 ?? ?? E5 E3");
```

**Search an offset in a byte array:**
```csharp
var scanner     = new Scanner(data);
int offset      = scanner.FindPattern("02 11 25 AB");
```

Patterns are specified as hex values, without prefix and delimited by spaces.
`??` represents a wildcard, i.e. any byte can be present in the place of the wildcard.

### Future Features
Here is a short list of features which this pattern matching library does NOT support at this current moment in time.

#### Wildcards smaller than 1 byte.
The current implementation of the algorithm can support bit masks for wildcards without any performance deficit. That said, at the current moment in time, the string format parser (in `PatternScanInstructionSet`) only supports byte wildcards. 

This feature doesn't currently exist as I see no necessity for it, yet.

If you are interested in such a feature feel free to provide a pull request with support for different formats.

# Sample Benchmarks
All benchmarks were performed on a machine running a Core i7 4790k @ 4.5GHz (OC) on a single thread.

Memory configuration is 1866MHz with CL9 timings.

The provided "Speed" column is an effective measure of pattern matching speed in MB/s.

### Configuration
```
BenchmarkDotNet=v0.13.1, OS=Windows 10.0.19044.1586 (21H2)
Intel Core i7-4790K CPU 4.00GHz (Haswell), 1 CPU, 8 logical and 4 physical cores
.NET SDK=6.0.200
  [Host] : .NET 5.0.15 (5.0.1522.11506), X64 RyuJIT
```

### Small Signatures (1-8 bytes)

For any inputs shorter than 8 bytes, the speed is approximately 2050MB/s on a single core.

This speed does not differ depending on whether there are any wildcards used, so long the input is between 1 and 8 bytes.

```csharp
[Benchmark]
public int Compiled()
{
    var result = _scannerFromFile.CompiledFindPattern("A0 4E ?? ?? 0E ED");
    return result.Offset;
}

[Benchmark]
public int Simple()
{
    var result = _scannerFromFile.SimpleFindPattern("A0 4E ?? ?? 0E ED");
    return result.Offset;
}
```
```
|   Method |       Mean |    Error |   StdDev |              Speed |
|--------- |-----------:|---------:|---------:|------------------- |
|      Avx |   266.0 us |  1.01 us |  0.79 us | 11826.019431013981 |
|      Sse |   382.6 us |  2.53 us |  2.24 us |  8222.161830886607 |
| Compiled | 1,563.1 us | 10.80 us | 10.10 us |  2012.485989510929 |
|   Simple | 3,431.8 us | 29.68 us | 24.78 us |  916.6368017423626 |
```

### Long Signatures (8+ bytes)

For any inputs longer than 8 bytes, the speed matches the short signature speed at ~2050MB/s on a single core. 

There is a reduction in speed that is parallel to the amount of times the first 8 bytes match, however with any real data (PE file, executable, binary file) it is completely indistinguishable.

```csharp
[Benchmark]
public int Compiled()
{
    var result = _scannerFromFile.CompiledFindPattern("9F 43 ?? ?? 43 4F 99 ?? ?? 48"");
    return result.Offset;
}

[Benchmark]
public int Simple()
{
    var result = _scannerFromFile.SimpleFindPattern("9F 43 ?? ?? 43 4F 99 ?? ?? 48"");
    return result.Offset;
}
```

```
|   Method |       Mean |    Error |   StdDev |              Speed |
|--------- |-----------:|---------:|---------:|------------------- |
|      Avx |   271.1 us |  1.78 us |  1.66 us | 11604.550844524128 |
|      Sse |   384.4 us |  1.57 us |  1.47 us |  8184.029436222898 |
| Compiled | 1,565.4 us | 11.37 us | 10.08 us |  2009.473376177141 |
|   Simple | 3,271.7 us | 15.61 us | 14.60 us |  961.4902217649164 |
```

### Other Benchmarks
For other benchmarks, please see the `Reloaded.Memory.Sigscan.Benchmark` project in this repository.

To run the benchmarks yourself, simply run `Reloaded.Memory.Sigscan.Benchmark` after compiling in `Release` mode.

`Reloaded.Memory.Sigscan.Benchmark` is powered by [BenchmarkDotNet](https://github.com/dotnet/BenchmarkDotNet).

### Acknowledgements

Vectorised implementations of Reloaded.Memory.Sigscan are based off of a modified version of LazySIMD by [uberhalit](https://github.com/uberhalit).
