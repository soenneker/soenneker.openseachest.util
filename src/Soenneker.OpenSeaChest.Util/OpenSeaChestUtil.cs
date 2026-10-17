using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Soenneker.OpenSeaChest.Util.Abstract;
using Soenneker.Utils.Process.Abstract;
using Soenneker.Utils.File.Abstract;
using Soenneker.Utils.Runtime;
namespace Soenneker.OpenSeaChest.Util;

public sealed class OpenSeaChestUtil(IProcessUtil processUtil, IFileUtil fileUtil, IOptions<OpenSeaChestOptions> options) : IOpenSeaChestUtil
{
    public async ValueTask<string> GetExecutablePath(OpenSeaChestTool tool, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(tool);
        // Enum-like values can be constructed by consumers; never treat an arbitrary value as a path.
        if (!OpenSeaChestTool.List.Contains(tool))
            throw new ArgumentOutOfRangeException(nameof(tool));
        string os = RuntimeUtil.IsWindows() ? "win" : RuntimeUtil.IsLinux() ? "linux"
            : throw new PlatformNotSupportedException("Bundled openSeaChest tools support Windows and Linux.");
        string architecture = RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.X64 => "x64",
            Architecture.Arm64 => "arm64",
            _ => throw new PlatformNotSupportedException("Bundled openSeaChest tools support x64 and ARM64.")
        };
        string directory = options.Value.ExecutableDirectory ?? Path.Combine(AppContext.BaseDirectory, "Resources", "OpenSeaChest", $"{os}-{architecture}");
        string path = Path.GetFullPath(Path.Combine(directory, "openSeaChest_" + tool.Value + (RuntimeUtil.IsWindows() ? ".exe" : "")));
        if (!await fileUtil.Exists(path, cancellationToken).ConfigureAwait(false))
            throw new FileNotFoundException("The openSeaChest executable is missing. Install Soenneker.OpenSeaChest.Binaries or configure ExecutableDirectory.", path);
        return path;
    }

    public ValueTask<List<string>> Execute(OpenSeaChestTool tool, IReadOnlyList<string> arguments, CancellationToken cancellationToken = default)
        => ExecuteCore(tool, arguments, options.Value.Timeout, cancellationToken);

    private async ValueTask<List<string>> ExecuteCore(OpenSeaChestTool tool, IReadOnlyList<string> arguments, TimeSpan? timeout, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(arguments);
        cancellationToken.ThrowIfCancellationRequested();
        string commandLine = ProcessArguments.Join(arguments);
        string executable = await GetExecutablePath(tool, cancellationToken).ConfigureAwait(false);

        if (RuntimeUtil.IsLinux())
        {
            // IFileUtil does not expose Unix modes. RuntimeUtil guards these Linux-only calls.
#pragma warning disable CA1416
            UnixFileMode mode = File.GetUnixFileMode(executable);
            if ((mode & UnixFileMode.UserExecute) == 0)
                File.SetUnixFileMode(executable, mode | UnixFileMode.UserExecute);
#pragma warning restore CA1416
        }

        return await processUtil.Start(executable, workingDirectory: Path.GetDirectoryName(executable), arguments: commandLine,
            timeout: timeout, log: options.Value.Log, cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public ValueTask<List<string>> GetVersion(CancellationToken cancellationToken = default) => Execute(OpenSeaChestTool.Basics, ["--version"], cancellationToken);
    public ValueTask<List<string>> Scan(CancellationToken cancellationToken = default) => Execute(OpenSeaChestTool.Basics, ["--scan"], cancellationToken);
    public ValueTask<List<string>> GetDeviceInfo(string device, CancellationToken cancellationToken = default) => DeviceCommand(OpenSeaChestTool.Info, device, ["--deviceInfo"], cancellationToken);
    public ValueTask<List<string>> CheckSmartHealth(string device, CancellationToken cancellationToken = default) => DeviceCommand(OpenSeaChestTool.SMART, device, ["--smartCheck"], cancellationToken);
    public ValueTask<List<string>> GetSmartAttributes(string device, CancellationToken cancellationToken = default) => DeviceCommand(OpenSeaChestTool.SMART, device, ["--smartAttributes", "hybrid"], cancellationToken);
    public ValueTask<List<string>> GetPowerMode(string device, CancellationToken cancellationToken = default) => DeviceCommand(OpenSeaChestTool.PowerControl, device, ["--checkPowerMode"], cancellationToken);
    public ValueTask<List<string>> GetPowerSettings(string device, CancellationToken cancellationToken = default) => DeviceCommand(OpenSeaChestTool.PowerControl, device, ["--showEPCSettings"], cancellationToken);

    public async ValueTask<List<string>> UpgradeFirmware(string device, string firmwarePath, CancellationToken cancellationToken = default)
    {
        ValidateDevice(device);
        if (device.Trim().Equals("all", StringComparison.OrdinalIgnoreCase) || device.IndexOfAny(['*', '?']) >= 0)
            throw new ArgumentException("Firmware updates require one specific device.", nameof(device));
        ArgumentException.ThrowIfNullOrWhiteSpace(firmwarePath);
        cancellationToken.ThrowIfCancellationRequested();
        string path = Path.GetFullPath(firmwarePath);
        if (!await fileUtil.Exists(path, cancellationToken).ConfigureAwait(false))
            throw new FileNotFoundException("The firmware image does not exist.", path);
        await using (FileStream stream = fileUtil.OpenRead(path))
        {
            if (stream.Length == 0)
                throw new ArgumentException("The firmware image is empty.", nameof(firmwarePath));
        }
        return await ExecuteCore(OpenSeaChestTool.Firmware, ["--device", device, "--downloadFW", path, "--downloadMode", "auto"],
            options.Value.FirmwareTimeout, cancellationToken).ConfigureAwait(false);
    }
    private ValueTask<List<string>> DeviceCommand(OpenSeaChestTool tool, string device, string[] arguments, CancellationToken cancellationToken)
    {
        ValidateDevice(device);
        return Execute(tool, ["--device", device, .. arguments], cancellationToken);
    }

    private static void ValidateDevice(string device)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(device);
        if (device.StartsWith('-') || device.IndexOfAny(['\0', '\r', '\n']) >= 0)
            throw new ArgumentException("A device identifier, not a command option, is required.", nameof(device));
    }
}