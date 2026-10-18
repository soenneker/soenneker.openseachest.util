using System;
using System.IO;
namespace Soenneker.OpenSeaChest.Util.Tests;

public sealed class DriveOutputParserTests
{
    [Test]
    public void ScanPreservesHandlesWithLongModelsAndMissingVendor()
    {
        string header = $"{"Vendor",-8} {"Handle",-12} {"Model Number",-23} {"Serial Number",-22} FwRev";
        var devices = DriveOutputParser.ParseScan([
            "openSeaChest_Basics Version: 26.03.2", header,
            $"{"NVMe",-8} {"PD0",-12} {"A model with spaces longer than its column",-23} {"serial0",-22} FW01",
            $"{"",-8} {"/dev/nvme0n1",-12} {"Another model",-23} {"serial1",-22} FW02"]);
        if (devices.Count != 2 || devices[0].Handle != "PD0" || devices[1].Handle != "/dev/nvme0n1" || devices[1].Vendor is not null)
            throw new Exception("Scan handles were not parsed correctly.");
    }

    [Test]
    public void ParsesSataIdentityAndExactCapacity()
    {
        var drive = DriveOutputParser.ParseDevice("PD0", "ATA", [
            "Model Number: Model with spaces", "Serial Number: ABC 123", "Firmware Revision: SN04",
            "MaxLBA: 1953525167", "Logical Sector Size (B): 512", "Physical Sector Size (B): 4096", "interface type: IDE/ATA"]);
        if (drive.ModelNumber != "Model with spaces" || drive.SerialNumber != "ABC 123" || drive.FirmwareRevision != "SN04"
            || drive.CapacityBytes != 1000204886016UL || drive.LogicalBlockSizeBytes != 512 || drive.InterfaceType != "IDE/ATA")
            throw new Exception("SATA information was not parsed correctly.");
        if (drive.Details["Physical Sector Size (B)"] != "4096")
            throw new Exception("Additional fields were lost.");
    }

    [Test]
    public void ParsesNvmeNamespaceCapacityWithoutOffByOne()
    {
        var drive = DriveOutputParser.ParseDevice("/dev/nvme0n1", "NVMe", [
            "NVMe Controller Information:", "Model Number: Example NVMe", "Firmware Revision: FW:02",
            "Namespace Capacity (LBAs): 1000", "Logical Block Size (B): 4096"]);
        if (drive.CapacityBytes != 4096000UL || drive.FirmwareRevision != "FW:02" || drive.SerialNumber is not null)
            throw new Exception("NVMe information was not parsed correctly.");
    }

    [Test]
    public void EmptyScanIsExplicit()
    {
        if (DriveOutputParser.ParseScan(["No devices found"]).Count != 0)
            throw new Exception("Expected no devices.");
        try { DriveOutputParser.ParseScan(["Some new unsupported output format"]); }
        catch (InvalidDataException) { return; }
        throw new Exception("Unknown output must not look like an empty system.");
    }

    [Test]
    public void PermissionErrorsAreNotEmptyResults()
    {
        try { DriveOutputParser.ParseScan(["Permission to access all devices was denied. Please check your permissions and try again."]); }
        catch (UnauthorizedAccessException) { return; }
        throw new Exception("Expected an access error.");
    }

    [Test]
    public void RepeatedSectionsCannotOverwritePrimaryIdentity()
    {
        var drive = DriveOutputParser.ParseDevice("PD0", "ATA", [
            "Model Number: Primary", "Firmware Revision: PrimaryFW", "Model Number: Secondary", "Firmware Revision: SecondaryFW"]);
        if (drive.ModelNumber != "Primary" || drive.FirmwareRevision != "PrimaryFW" || drive.RawDeviceInfo.Count != 4)
            throw new Exception("Repeated section handling changed the primary identity.");
    }

    [Test]
    public void UnrecognizedDeviceOutputFails()
    {
        try { DriveOutputParser.ParseDevice("PD0", null, ["openSeaChest_Info Version: 26.03.2", "ERROR: failed to get device information"]); }
        catch (InvalidDataException) { return; }
        throw new Exception("Expected missing device information to fail.");
    }
}
