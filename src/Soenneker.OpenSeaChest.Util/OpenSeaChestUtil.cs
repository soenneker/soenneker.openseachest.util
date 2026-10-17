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
namespace Soenneker.OpenSeaChest.Util;

public sealed class OpenSeaChestUtil(IProcessUtil processUtil, IOptions<OpenSeaChestOptions> options) : IOpenSeaChestUtil
{
    public string GetExecutablePath(OpenSeaChestTool tool)
    {
        ArgumentNullException.ThrowIfNull(tool);
        // Enum-like values can be constructed by consumers; never treat an arbitrary value as a path.
        if (!OpenSeaChestTool.List.Contains(tool))
            throw new ArgumentOutOfRangeException(nameof(tool));
        string os = OperatingSystem.IsWindows() ? "win" : OperatingSystem.IsLinux() ? "linux"
            : throw new PlatformNotSupportedException("Bundled openSeaChest tools support Windows and Linux.");
        string architecture = RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.X64 => "x64",
            Architecture.Arm64 => "arm64",
            _ => throw new PlatformNotSupportedException("Bundled openSeaChest tools support x64 and ARM64.")
        };
        string directory = options.Value.ExecutableDirectory ?? Path.Combine(AppContext.BaseDirectory, "Resources", "OpenSeaChest", $"{os}-{architecture}");
        string path = Path.GetFullPath(Path.Combine(directory, "openSeaChest_" + tool.Value + (OperatingSystem.IsWindows() ? ".exe" : "")));
        if (!File.Exists(path))
            throw new FileNotFoundException("The openSeaChest executable is missing. Install Soenneker.OpenSeaChest.Binaries or configure ExecutableDirectory.", path);
        return path;
    }

    public ValueTask<List<string>> Execute(OpenSeaChestTool tool, IReadOnlyList<string> arguments, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(arguments);
        cancellationToken.ThrowIfCancellationRequested();
        string commandLine = ProcessArguments.Join(arguments);
        string executable = GetExecutablePath(tool);
        if (OperatingSystem.IsLinux())
        {
            UnixFileMode mode = File.GetUnixFileMode(executable);
            if ((mode & UnixFileMode.UserExecute) == 0)
                File.SetUnixFileMode(executable, mode | UnixFileMode.UserExecute);
        }
        return processUtil.Start(executable, workingDirectory: Path.GetDirectoryName(executable), arguments: commandLine,
            timeout: options.Value.Timeout, log: options.Value.Log, cancellationToken: cancellationToken);
    }

    public ValueTask<List<string>> GetVersion(CancellationToken cancellationToken = default) => Execute(OpenSeaChestTool.Basics, ["--version"], cancellationToken);
    public ValueTask<List<string>> Scan(CancellationToken cancellationToken = default) => Execute(OpenSeaChestTool.Basics, ["--scan"], cancellationToken);
    public ValueTask<List<string>> GetDeviceInfo(string device, CancellationToken cancellationToken = default) => DeviceCommand(OpenSeaChestTool.Info, device, ["--deviceInfo"], cancellationToken);
    public ValueTask<List<string>> CheckSmartHealth(string device, CancellationToken cancellationToken = default) => DeviceCommand(OpenSeaChestTool.SMART, device, ["--smartCheck"], cancellationToken);
    public ValueTask<List<string>> GetSmartAttributes(string device, CancellationToken cancellationToken = default) => DeviceCommand(OpenSeaChestTool.SMART, device, ["--smartAttributes", "hybrid"], cancellationToken);
    public ValueTask<List<string>> GetPowerMode(string device, CancellationToken cancellationToken = default) => DeviceCommand(OpenSeaChestTool.PowerControl, device, ["--checkPowerMode"], cancellationToken);
    public ValueTask<List<string>> GetPowerSettings(string device, CancellationToken cancellationToken = default) => DeviceCommand(OpenSeaChestTool.PowerControl, device, ["--showEPCSettings"], cancellationToken);

    private ValueTask<List<string>> DeviceCommand(OpenSeaChestTool tool, string device, string[] arguments, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(device);
        if (device.StartsWith('-') || device.IndexOfAny(['\0', '\r', '\n']) >= 0)
            throw new ArgumentException("A device identifier, not a command option, is required.", nameof(device));
        return Execute(tool, ["--device", device, .. arguments], cancellationToken);
    }
}
