using Soenneker.Gen.EnumValues;
namespace Soenneker.OpenSeaChest.Util;

/// <summary>Identifies a bundled openSeaChest executable.</summary>
[EnumValue<string>]
public sealed partial class OpenSeaChestTool
{
    /// <summary>The openSeaChest_Basics executable.</summary>
    public static readonly OpenSeaChestTool Basics = new(nameof(Basics));

    /// <summary>The openSeaChest_Configure executable.</summary>
    public static readonly OpenSeaChestTool Configure = new(nameof(Configure));

    /// <summary>The openSeaChest_Defect executable.</summary>
    public static readonly OpenSeaChestTool Defect = new(nameof(Defect));

    /// <summary>The openSeaChest_Erase executable.</summary>
    public static readonly OpenSeaChestTool Erase = new(nameof(Erase));

    /// <summary>The openSeaChest_Firmware executable.</summary>
    public static readonly OpenSeaChestTool Firmware = new(nameof(Firmware));

    /// <summary>The openSeaChest_Format executable.</summary>
    public static readonly OpenSeaChestTool Format = new(nameof(Format));

    /// <summary>The openSeaChest_GenericTests executable.</summary>
    public static readonly OpenSeaChestTool GenericTests = new(nameof(GenericTests));

    /// <summary>The openSeaChest_Info executable.</summary>
    public static readonly OpenSeaChestTool Info = new(nameof(Info));

    /// <summary>The openSeaChest_Logs executable.</summary>
    public static readonly OpenSeaChestTool Logs = new(nameof(Logs));

    /// <summary>The openSeaChest_NVMe executable.</summary>
    public static readonly OpenSeaChestTool NVMe = new(nameof(NVMe));

    /// <summary>The openSeaChest_PassthroughTest executable.</summary>
    public static readonly OpenSeaChestTool PassthroughTest = new(nameof(PassthroughTest));

    /// <summary>The openSeaChest_PowerControl executable.</summary>
    public static readonly OpenSeaChestTool PowerControl = new(nameof(PowerControl));

    /// <summary>The openSeaChest_Raw executable.</summary>
    public static readonly OpenSeaChestTool Raw = new(nameof(Raw));

    /// <summary>The openSeaChest_Reservations executable.</summary>
    public static readonly OpenSeaChestTool Reservations = new(nameof(Reservations));

    /// <summary>The openSeaChest_Security executable.</summary>
    public static readonly OpenSeaChestTool Security = new(nameof(Security));

    /// <summary>The openSeaChest_SMART executable.</summary>
    public static readonly OpenSeaChestTool SMART = new(nameof(SMART));

    /// <summary>The openSeaChest_ZBD executable.</summary>
    public static readonly OpenSeaChestTool ZBD = new(nameof(ZBD));
}
