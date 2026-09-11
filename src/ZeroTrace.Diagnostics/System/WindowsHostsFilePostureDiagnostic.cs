using System.Runtime.Versioning;
using ZeroTrace.Core.Contracts;
using ZeroTrace.Core.Models;

namespace ZeroTrace.Diagnostics.System;

[SupportedOSPlatform("windows")]
public sealed class WindowsHostsFilePostureDiagnostic : IDiagnosticCheck
{
    public string Name => "Windows Hosts File Posture";

    public Task<DiagnosticResult> RunAsync(CancellationToken cancellationToken = default)
    {
        var started = DateTimeOffset.UtcNow;
        cancellationToken.ThrowIfCancellationRequested();
        var evidence = new List<Evidence>();
        var findings = new List<Finding>();
        var now = DateTimeOffset.UtcNow;

        try
        {
            var path = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.System),
                @"drivers\etc\hosts");

            var exists = File.Exists(path);
            evidence.Add(new("HostsFile", "Exists", exists.ToString(), exists ? EvidenceState.Pass : EvidenceState.Info, now));

            if (exists)
            {
                var info = new FileInfo(path);
                evidence.Add(new("HostsFile", "LengthBytes", info.Length.ToString(), EvidenceState.Info, now));
                evidence.Add(new("HostsFile", "LastWriteTimeUtc", info.LastWriteTimeUtc.ToString("O"), EvidenceState.Info, now));

                var activeEntries = File.ReadLines(path)
                    .Count(line =>
                    {
                        var trimmed = line.Trim();
                        return trimmed.Length > 0 && !trimmed.StartsWith("#");
                    });

                evidence.Add(new("HostsFile", "ActiveEntryCount", activeEntries.ToString(), EvidenceState.Info, now));
            }
        }
        catch
        {
            evidence.Add(new("HostsFile", "PostureRead", "ReadFailed", EvidenceState.Info, now));
        }

        evidence.Add(new("HostsFile", "MutationPolicy", "ReadOnlyDiagnostic", EvidenceState.Pass, now));

        return Task.FromResult(new DiagnosticResult(
            Name, started, DateTimeOffset.UtcNow, true, evidence, findings));
    }
}
