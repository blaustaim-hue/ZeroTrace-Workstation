using System.Diagnostics;
using System.Runtime.Versioning;
using ZeroTrace.Core.Contracts;
using ZeroTrace.Core.Models;

namespace ZeroTrace.Diagnostics.System;

[SupportedOSPlatform("windows")]
public sealed class WindowsArpPostureDiagnostic : IDiagnosticCheck
{
    public string Name => "Windows ARP Posture";

    public async Task<DiagnosticResult> RunAsync(CancellationToken cancellationToken = default)
    {
        var started = DateTimeOffset.UtcNow;
        cancellationToken.ThrowIfCancellationRequested();
        var evidence = new List<Evidence>();
        var findings = new List<Finding>();
        var now = DateTimeOffset.UtcNow;

        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "arp.exe",
                    Arguments = "-a",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
            await process.WaitForExitAsync(cancellationToken);

            var entries = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Count(line => line.Contains("dynamic", StringComparison.OrdinalIgnoreCase) ||
                               line.Contains("static", StringComparison.OrdinalIgnoreCase));

            evidence.Add(new("ArpPosture", "NeighborEntries", entries.ToString(), EvidenceState.Info, now));
            evidence.Add(new("ArpPosture", "ReadStatus",
                process.ExitCode == 0 ? "ReadSucceeded" : "ReadFailed", EvidenceState.Info, now));
        }
        catch
        {
            evidence.Add(new("ArpPosture", "ReadStatus", "ReadFailed", EvidenceState.Info, now));
        }

        evidence.Add(new("ArpPosture", "MutationPolicy", "ReadOnlyDiagnostic", EvidenceState.Pass, now));

        return new DiagnosticResult(Name, started, DateTimeOffset.UtcNow, true, evidence, findings);
    }
}
