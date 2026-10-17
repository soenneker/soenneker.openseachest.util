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
}
