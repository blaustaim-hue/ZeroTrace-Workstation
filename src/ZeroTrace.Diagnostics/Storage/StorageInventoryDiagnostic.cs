using System.IO;
using ZeroTrace.Core.Contracts;
using ZeroTrace.Core.Models;

namespace ZeroTrace.Diagnostics.Storage;

public sealed class StorageInventoryDiagnostic : IDiagnosticCheck
{
    public string Name => "Storage Inventory";

    public Task<DiagnosticResult> RunAsync(CancellationToken cancellationToken = default)
    {
        var started = DateTimeOffset.UtcNow;
        var evidence = new List<Evidence>();

        foreach (var drive in DriveInfo.GetDrives().Where(d => d.IsReady))
        {
            var total = drive.TotalSize;
            var free = drive.AvailableFreeSpace;
            var freePercent = total > 0 ? Math.Round((double)free / total * 100, 2) : 0;

            evidence.Add(new("Storage", $"{drive.Name}:Format", drive.DriveFormat, EvidenceState.Info, DateTimeOffset.UtcNow));
            evidence.Add(new("Storage", $"{drive.Name}:Type", drive.DriveType.ToString(), EvidenceState.Info, DateTimeOffset.UtcNow));
            evidence.Add(new("Storage", $"{drive.Name}:TotalBytes", total.ToString(), EvidenceState.Info, DateTimeOffset.UtcNow));
            evidence.Add(new("Storage", $"{drive.Name}:FreeBytes", free.ToString(), EvidenceState.Info, DateTimeOffset.UtcNow));
            evidence.Add(new("Storage", $"{drive.Name}:FreePercent", freePercent.ToString("F2"), EvidenceState.Info, DateTimeOffset.UtcNow));
        }

        return Task.FromResult(new DiagnosticResult(
            Name,
            started,
            DateTimeOffset.UtcNow,
            true,
            evidence,
            Array.Empty<Finding>()));
    }
}
