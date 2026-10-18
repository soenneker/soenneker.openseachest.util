using System;
using System.Threading.Tasks;
using Soenneker.Tests.HostedUnit;
using Soenneker.Utils.Process.Abstract;

namespace Soenneker.OpenSeaChest.Util.Tests;

[ClassDataSource<Host>(Shared = SharedType.PerTestSession)]
public sealed class FirmwareProcessTests(Host host) : HostedUnitTest(host)
{
    [Test]
    [Arguments(0, true)]
    [Arguments(32, true)]
    [Arguments(33, true)]
    [Arguments(34, true)]
    [Arguments(38, true)]
    [Arguments(3, false)]
    [Arguments(35, false)]
    [Arguments(37, false)]
    [Arguments(39, false)]
    public async Task PreservesOutputAndChecksCompletionCode(int code, bool success)
    {
        string executable = OperatingSystem.IsWindows() ? Environment.GetEnvironmentVariable("COMSPEC")! : "/bin/sh";
        string arguments = OperatingSystem.IsWindows() ? $"/d /c \"echo firmware-result & exit /b {code}\"" : $"-c \"echo firmware-result; exit {code}\"";
        try
        {
            var output = await FirmwareProcess.Run(Resolve<IProcessUtil>(true), executable, arguments, TimeSpan.FromSeconds(10), false, default);
            if (!success || !output.Exists(line => line.Contains("firmware-result"))) throw new Exception("Incorrect completion or lost output.");
        }
        catch (InvalidOperationException) when (!success) { }
    }
}
