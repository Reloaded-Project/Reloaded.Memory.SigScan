using System.Collections.Generic;
using Reloaded.Memory.Sigscan;
using Xunit;

namespace Reloaded.Memory.SigScan.Tests;

public class MultithreadScannerTests : ScannerTestBase
{
    [Fact]
    public void Cached_DoesNotRemoveDuplicates()
    {
        var scanner = new Scanner(_data);
        var multiplePatterns = new List<string>()
        {
            "7A BB",
            "9F AB",
            "7A BB"
        };

        var results = scanner.FindPatternsCached(multiplePatterns);
        Assert.Equal(results[0], results[2]);
    }
}