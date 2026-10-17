using System;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using Soenneker.OpenSeaChest.Util.Abstract;
using Soenneker.Tests.HostedUnit;
namespace Soenneker.OpenSeaChest.Util.Tests;

[ClassDataSource<Host>(Shared = SharedType.PerTestSession)]
public sealed class OpenSeaChestUtilTests : HostedUnitTest
{
    private readonly IOpenSeaChestUtil _util;

    public OpenSeaChestUtilTests(Host host) : base(host) => _util = Resolve<IOpenSeaChestUtil>(true);

    private static void RequireNativeExecution()
    {
        if (OperatingSystem.IsWindows())
        {
            using WindowsIdentity identity = WindowsIdentity.GetCurrent();
            Skip.Unless(new WindowsPrincipal(identity).IsInRole(WindowsBuiltInRole.Administrator),
                "The upstream Windows executable manifest requires an elevated test process, including for help/version.");
        }
    }

    [Test]
    public async Task AllBundledToolsExist()
    {
        foreach (OpenSeaChestTool tool in OpenSeaChestTool.List)
            await _util.GetExecutablePath(tool);
    }
    [Test]
    public async Task BundledExecutableReportsVersion()
    {
        RequireNativeExecution();
        var output = await _util.GetVersion();
        if (!output.Any(line => line.Contains("openSeaChest", StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException("Native version output was not captured.");
    }

    [Test]
    public async Task BundledSmartToolReportsHelp()
    {
        RequireNativeExecution();
        var output = await _util.Execute(OpenSeaChestTool.SMART, ["--help"]);
        if (!output.Any(line => line.Contains("smartCheck", StringComparison.Ordinal)))
            throw new InvalidOperationException("SMART help output was not captured.");
    }

    [Test]
    public async Task CancelledCommandDoesNotStart()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        try { await _util.GetVersion(cancellation.Token); }
        catch (OperationCanceledException) { return; }
        throw new InvalidOperationException("Expected cancellation.");
    }

    [Test]
    public async Task DeviceOptionCannotBeUsedAsDeviceIdentifier()
    {
        try { await _util.GetDeviceInfo("--scan"); }
        catch (ArgumentException) { return; }
        throw new InvalidOperationException("Expected invalid device rejection.");
    }
}

