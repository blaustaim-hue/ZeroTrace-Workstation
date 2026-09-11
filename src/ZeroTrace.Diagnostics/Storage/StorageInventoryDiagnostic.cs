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
        cancellationToken.ThrowIfCancellationRequested();

        var evidence = new List<Evidence>();
        var findings = new List<Finding>();

        foreach (var drive in DriveInfo.GetDrives().Where(d => d.IsReady))
        {
            cancellationToken.ThrowIfCancellationRequested();

            var total = drive.TotalSize;
            var free = drive.AvailableFreeSpace;
            var freePercent = total > 0 ? Math.Round((double)free / total * 100, 2) : 0;
            var now = DateTimeOffset.UtcNow;

            var capacityState = freePercent < 5
                ? EvidenceState.Fail
                : freePercent < 15
                    ? EvidenceState.Warning
                    : EvidenceState.Pass;

            var driveEvidence = new List<Evidence>
            {
                new("Storage", $"{drive.Name}:Format", drive.DriveFormat, EvidenceState.Info, now),
                new("Storage", $"{drive.Name}:Type", drive.DriveType.ToString(), EvidenceState.Info, now),
                new("Storage", $"{drive.Name}:TotalBytes", total.ToString(), EvidenceState.Info, now),
                new("Storage", $"{drive.Name}:FreeBytes", free.ToString(), capacityState, now),
                new("Storage", $"{drive.Name}:FreePercent", freePercent.ToString("F2"), capacityState, now)
            };

            evidence.AddRange(driveEvidence);

            if (freePercent < 5)
            {
                findings.Add(new Finding(
                    $"STO-{NormalizeDriveId(drive.Name)}-001",
                    $"Critically low disk space on {drive.Name}",
                    $"Only {freePercent:F2}% of {drive.Name} is available.",
                    "Critical",
                    98,
                    driveEvidence,
                    "Review large files, temporary data and application storage before performing any cleanup."));
            }
            else if (freePercent < 15)
            {
                findings.Add(new Finding(
                    $"STO-{NormalizeDriveId(drive.Name)}-002",
                    $"Low disk space on {drive.Name}",
                    $"Only {freePercent:F2}% of {drive.Name} is available.",
                    "Warning",
                    95,
                    driveEvidence,
                    "Review storage usage and identify safe cleanup candidates; do not remove corporate or system-managed data automatically."));
            }
        }

        return Task.FromResult(new DiagnosticResult(
            Name,
            started,
            DateTimeOffset.UtcNow,
            true,
            evidence,
            findings));
    }

    private static string NormalizeDriveId(string driveName)
    {
        var id = new string(driveName.Where(char.IsLetterOrDigit).ToArray());
        return string.IsNullOrWhiteSpace(id) ? "DRIVE" : id.ToUpperInvariant();
    }
}
