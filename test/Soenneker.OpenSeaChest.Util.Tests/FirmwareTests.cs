using System;
using System.IO;
using System.Threading.Tasks;
using Soenneker.OpenSeaChest.Util.Abstract;
using Soenneker.Tests.HostedUnit;
using Soenneker.Utils.File.Abstract;
namespace Soenneker.OpenSeaChest.Util.Tests;

[ClassDataSource<Host>(Shared = SharedType.PerTestSession)]
public sealed class FirmwareTests(Host host) : HostedUnitTest(host)
{
    [Test]
    public async Task RejectsAllDevices()
    {
        var util = Resolve<IOpenSeaChestUtil>(true);
        try { await util.UpgradeFirmware("all", "unused.bin"); }
        catch (ArgumentException exception) when (exception.ParamName == "device") { return; }
        throw new InvalidOperationException("Expected rejection of the all-devices selector.");
    }

    [Test]
    public async Task RejectsMissingImage()
    {
        var util = Resolve<IOpenSeaChestUtil>(true);
        string path = Path.Combine(Path.GetTempPath(), $"missing-firmware-{Guid.NewGuid()}.bin");
        try { await util.UpgradeFirmware("PD0", path); }
        catch (FileNotFoundException exception) when (exception.FileName == path) { return; }
        throw new InvalidOperationException("Expected rejection of the missing firmware image.");
    }

    [Test]
    public async Task RejectsEmptyImage()
    {
        var util = Resolve<IOpenSeaChestUtil>(true);
        var files = Resolve<IFileUtil>(true);
        string path = Path.Combine(Path.GetTempPath(), $"empty-firmware-{Guid.NewGuid()}.bin");
        await files.Write(path, Array.Empty<byte>());
        try
        {
            try { await util.UpgradeFirmware("PD0", path); }
            catch (ArgumentException exception) when (exception.ParamName == "firmwarePath") { return; }
            throw new InvalidOperationException("Expected rejection of the empty firmware image.");
        }
        finally { await files.DeleteIfExists(path); }
    }
}
