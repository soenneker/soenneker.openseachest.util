using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
namespace Soenneker.OpenSeaChest.Util.Abstract;

/// <summary>
/// Runs openSeaChest using the bundled native tools and Soenneker.Utils.Process.
/// Device operations require the caller's process to have the appropriate administrator/root permissions.
/// Output is returned as text; nonzero exits, timeouts, and cancellation propagate from the process utility.
/// </summary>
public interface IOpenSeaChestUtil
{
    /// <summary>Resolves the executable for the current operating system and process architecture.</summary>
    ValueTask<string> GetExecutablePath(OpenSeaChestTool tool, CancellationToken cancellationToken = default);

    /// <summary>
    /// Executes a tool with individually escaped arguments, without a shell or automatic elevation.
    /// This general entry point also permits modifying commands; callers are responsible for their arguments.
    /// </summary>
    ValueTask<List<string>> Execute(OpenSeaChestTool tool, IReadOnlyList<string> arguments, CancellationToken cancellationToken = default);

    /// <summary>Returns the Basics executable's version without accessing a device.</summary>
    ValueTask<List<string>> GetVersion(CancellationToken cancellationToken = default);

    /// <summary>Scans for accessible storage devices.</summary>
    ValueTask<List<string>> Scan(CancellationToken cancellationToken = default);

    /// <summary>Reads information about a device, such as PD0 on Windows or /dev/sg0 on Linux.</summary>
    ValueTask<List<string>> GetDeviceInfo(string device, CancellationToken cancellationToken = default);

    /// <summary>Checks SMART health for the specified device.</summary>
    ValueTask<List<string>> CheckSmartHealth(string device, CancellationToken cancellationToken = default);

    /// <summary>Reads SATA SMART attributes using the upstream hybrid display format.</summary>
    ValueTask<List<string>> GetSmartAttributes(string device, CancellationToken cancellationToken = default);

    /// <summary>Reads the current power mode for the specified device.</summary>
    ValueTask<List<string>> GetPowerMode(string device, CancellationToken cancellationToken = default);

    /// <summary>Reads extended power condition settings for the specified device.</summary>
    ValueTask<List<string>> GetPowerSettings(string device, CancellationToken cancellationToken = default);
    /// <summary>Updates one drive using a local firmware image and openSeaChest's automatic download/activation mode.</summary>
    /// <param name="device">A specific device handle, such as PD0 or /dev/sg0. The all selector and wildcards are rejected.</param>
    /// <param name="firmwarePath">Path to a nonempty manufacturer-supplied firmware image matching the drive.</param>
    /// <param name="cancellationToken">Cancels execution. Cancellation terminates the tool and can interrupt flashing.</param>
    /// <returns>Native command output, including any activation or power-cycle instructions.</returns>
    /// <remarks>
    /// Requires administrator/root permissions. File validation does not establish hardware compatibility.
    /// Uses FirmwareTimeout (unlimited by default), rather than the ordinary command timeout.
    /// Firmware changes can affect all namespaces/LUNs on the selected physical drive.
    /// </remarks>
    ValueTask<List<string>> UpgradeFirmware(string device, string firmwarePath, CancellationToken cancellationToken = default);
}