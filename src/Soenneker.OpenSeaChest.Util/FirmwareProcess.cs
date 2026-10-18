using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Soenneker.Utils.Process.Abstract;
using Soenneker.Utils.Process.Dtos;

namespace Soenneker.OpenSeaChest.Util;

internal static class FirmwareProcess
{
    internal static async Task<List<string>> Run(IProcessUtil processes, string executable, string arguments,
        TimeSpan? timeout, bool log, CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(token);
        if (timeout is { } duration) deadline.CancelAfter(duration);
        var output = new ConcurrentQueue<string>();
        using System.Diagnostics.Process process = await processes.StartDetached(new ProcessStartDto
        {
            FileName = executable, Arguments = arguments, WorkingDirectory = Path.GetDirectoryName(executable),
            RedirectStandardOutput = true, RedirectStandardError = true, Log = log,
            OutputCallback = output.Enqueue, ErrorCallback = output.Enqueue
        }, deadline.Token).ConfigureAwait(false) ?? throw new IOException("Could not start firmware tool.");
        await process.WaitForExitAsync().ConfigureAwait(false);
        token.ThrowIfCancellationRequested();
        if (deadline.IsCancellationRequested) throw new TimeoutException("Firmware tool exceeded its configured timeout.");
        // Firmware-specific documented completion statuses, not generic process success codes.
        if (process.ExitCode is not (0 or 32 or 33 or 34 or 38))
            throw new InvalidOperationException($"Firmware tool exited with code {process.ExitCode}." + Environment.NewLine + string.Join(Environment.NewLine, output));
        return new List<string>(output);
    }
}
