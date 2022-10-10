## Reloaded-II Shared Library

[For information on consuming Shared Libraries, see [Dependency Injection in Reloaded II](https://reloaded-project.github.io/Reloaded-II/DependencyInjection_Consumer/).]

[![NuGet](https://img.shields.io/nuget/v/Reloaded.Memory.SigScan.ReloadedII.Interfaces)](https://www.nuget.org/packages/Reloaded.Memory.SigScan.ReloadedII.Interfaces)

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
[Binary by ismail abdurrasyid from Noun Project](https://thenounproject.com/browse/icons/term/binary/).
