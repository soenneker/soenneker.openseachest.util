using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Soenneker.OpenSeaChest.Util.Dtos;
using Soenneker.OpenSeaChest.Util.Abstract;
using Soenneker.Tests.HostedUnit;

namespace Soenneker.OpenSeaChest.Util.Tests;

// Native adapter-boundary tests use the real registered discovery service and parser.
[ClassDataSource<Host>(Shared = SharedType.PerTestSession)]
public sealed class DriveDiscoveryTests(Host host) : HostedUnitTest(host)
{
    private static List<string> Scan() => [
        "Vendor   Handle       Model Number            Serial Number          FwRev",
        "ATA      PD0          ST16000NM001G-2KK103    SERIAL0                SN04",
        "         PD56",
        "NVMe     PD55         Samsung SSD 970 EVO Plus 2TB SERIAL55             2B2QEXM7",
        "ATA      PD57         ST18000NM000J-2TV103    SERIAL57               SN02"
    ];

    private static List<string> Info(string handle) => [
        "Model Number: ST16000NM001G-2KK103", $"Serial Number: SERIAL{handle}",
        "Firmware Revision: SN04", "Logical Sector Size (B): 512", "MaxLBA: 999"
    ];

    [Test]
    public async Task BlankVendorHandleFailureDoesNotHideFollowingDrives()
    {
        var visited = new List<string>();
        List<OpenSeaChestDrive> results = await ((OpenSeaChestUtil)Resolve<IOpenSeaChestUtil>(true)).GetDrives(
            _ => Task.FromResult(Scan()), (handle, _) =>
            {
                visited.Add(handle);
                if (handle == "PD56") throw new InvalidOperationException("Could not open handle to PhysicalDrive56");
                return Task.FromResult(Info(handle));
            });
        if (!visited.SequenceEqual(new[] { "PD0", "PD56", "PD55", "PD57" }) || results.Count != 4)
            throw new Exception("Discovery stopped or parsed an incorrect handle.");
        if (results.Count(result => result.DiscoveryError is null) != 3 || results[1].DiscoveryError is null ||
            results[1].ModelNumber is not null || results[1].SerialNumber is not null ||
            results[0].LogicalBlockSizeBytes != 512 || results[0].CapacityBytes != 512000)
            throw new Exception("Failed device retained usable identity or successful details were lost.");

    }

    [Test]
    public async Task MalformedDetailsAndAccessErrorsAreIsolated()
    {
        var results = await ((OpenSeaChestUtil)Resolve<IOpenSeaChestUtil>(true)).GetDrives(_ => Task.FromResult(Scan()), (handle, _) =>
        {
            if (handle == "PD0") return Task.FromResult(new List<string> { "Not device information" });
            if (handle == "PD56") throw new UnauthorizedAccessException("Access denied");
            if (handle == "PD55") throw new TimeoutException("Device timed out");
            return Task.FromResult(Info(handle));
        });
        if (results.Take(3).Any(r => r.DiscoveryError is null || r.ModelNumber is not null) || results[3].DiscoveryError is not null)
            throw new Exception("Individual detail failures were not isolated.");
    }

    [Test]
    public async Task ScanFailurePropagatesWithoutReadingDevices()
    {
        bool called = false;
        try
        {
            await ((OpenSeaChestUtil)Resolve<IOpenSeaChestUtil>(true)).GetDrives(_ => throw new IOException("Scan failed"),
                (_, _) => { called = true; return Task.FromResult(Info("PD0")); });
        }
        catch (IOException) when (!called) { return; }
        throw new Exception("Global scan failure was hidden.");
    }

    [Test]
    public async Task MalformedScanIsNotReportedAsAnEmptySystem()
    {
        try
        {
            await ((OpenSeaChestUtil)Resolve<IOpenSeaChestUtil>(true)).GetDrives(_ => Task.FromResult(new List<string> { "unexpected output" }),
                (handle, _) => Task.FromResult(Info(handle)));
        }
        catch (InvalidDataException) { return; }
        throw new Exception("Malformed scan was accepted.");
    }

    [Test]
    public async Task CancellationDuringDetailsStopsDiscovery()
    {
        using var cancellation = new CancellationTokenSource();
        int calls = 0;
        try
        {
            await ((OpenSeaChestUtil)Resolve<IOpenSeaChestUtil>(true)).GetDrives(_ => Task.FromResult(Scan()), (handle, token) =>
            {
                calls++;
                cancellation.Cancel();
                token.ThrowIfCancellationRequested();
                return Task.FromResult(Info(handle));
            }, cancellation.Token);
        }
        catch (OperationCanceledException) when (calls == 1) { return; }
        throw new Exception("Cancellation was swallowed or another device was queried.");
    }

    [Test]
    public async Task PreCancelledDiscoveryDoesNotStartScan()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        bool called = false;
        try
        {
            await ((OpenSeaChestUtil)Resolve<IOpenSeaChestUtil>(true)).GetDrives(_ => { called = true; return Task.FromResult(Scan()); },
                (handle, _) => Task.FromResult(Info(handle)), cancellation.Token);
        }
        catch (OperationCanceledException) when (!called) { return; }
        throw new Exception("Cancelled discovery started native work.");
    }
}
