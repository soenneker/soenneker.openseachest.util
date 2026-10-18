using System;
using System.Collections.Generic;

namespace Soenneker.OpenSeaChest.Util.Dtos;

/// <summary>A storage device discovered by openSeaChest. Unreported properties remain null.</summary>
public sealed class OpenSeaChestDrive
{
    /// <summary>Failure reading this device's details. Only Handle and scan Vendor are retained when set; never use this entry for firmware operations.</summary>
    public Exception? DiscoveryError { get; init; }
    /// <summary>The device handle to pass to device operations, including UpgradeFirmware.</summary>
    public required string Handle { get; init; }
    /// <summary>Vendor reported by the device or scan.</summary>
    public string? Vendor { get; init; }
    /// <summary>Full model number reported by device information.</summary>
    public string? ModelNumber { get; init; }
    /// <summary>Device serial number.</summary>
    public string? SerialNumber { get; init; }
    /// <summary>Currently reported firmware revision.</summary>
    public string? FirmwareRevision { get; init; }
    /// <summary>Interface type, when explicitly reported by openSeaChest.</summary>
    public string? InterfaceType { get; init; }
    /// <summary>Capacity calculated from the reported block count and block size, when available.</summary>
    public ulong? CapacityBytes { get; init; }
    /// <summary>Logical block size in bytes, when reported.</summary>
    public uint? LogicalBlockSizeBytes { get; init; }
    /// <summary>Original named device-information fields, including units in their keys. The first occurrence is retained.</summary>
    public required IReadOnlyDictionary<string, string> Details { get; init; }
    /// <summary>Unmodified per-device output, including fields without values and repeated sections.</summary>
    public required IReadOnlyList<string> RawDeviceInfo { get; init; }
}
