using System.Diagnostics;
using System.Runtime.Versioning;
using ZeroTrace.Core.Contracts;
using ZeroTrace.Core.Models;

namespace ZeroTrace.Diagnostics.System;

[SupportedOSPlatform("windows")]
public sealed class WindowsDnsCachePostureDiagnostic : IDiagnosticCheck
{
    public string Name => "Windows DNS Cache Posture";

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
                    FileName = "ipconfig.exe",
                    Arguments = "/displaydns",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
            await process.WaitForExitAsync(cancellationToken);

            var recordCount = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Count(line => line.Contains("Record Name", StringComparison.OrdinalIgnoreCase));

            evidence.Add(new("DnsCachePosture", "CachedRecordCount", recordCount.ToString(), EvidenceState.Info, now));
            evidence.Add(new("DnsCachePosture", "ReadStatus",
                process.ExitCode == 0 ? "ReadSucceeded" : "ReadFailed", EvidenceState.Info, now));
        }
        catch
        {
            evidence.Add(new("DnsCachePosture", "ReadStatus", "ReadFailed", EvidenceState.Info, now));
        }

        evidence.Add(new("DnsCachePosture", "MutationPolicy", "ReadOnlyDiagnostic", EvidenceState.Pass, now));

        return new DiagnosticResult(
            Name, started, DateTimeOffset.UtcNow, true, evidence, findings);
    }
}
