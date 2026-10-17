using System;
namespace Soenneker.OpenSeaChest.Util;

/// <summary>Controls native executable lookup and process execution.</summary>
public sealed class OpenSeaChestOptions
{
    /// <summary>An optional directory containing the native executables. Defaults to the bundled platform directory.</summary>
    public string? ExecutableDirectory { get; set; }

    /// <summary>Maximum duration of each command. Defaults to two minutes; null permits an unlimited wait.</summary>
    public TimeSpan? Timeout { get; set; } = TimeSpan.FromMinutes(2);

    /// <summary>Maximum firmware-update duration. Null (the default) avoids interrupting flashing with an automatic timeout.</summary>
    public TimeSpan? FirmwareTimeout { get; set; }

    /// <summary>Whether to log process output. Defaults to false because output can contain device identifiers.</summary>
    public bool Log { get; set; }
}
