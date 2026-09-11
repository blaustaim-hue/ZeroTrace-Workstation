using System.Diagnostics;
using System.Runtime.Versioning;
using ZeroTrace.Core.Contracts;
using ZeroTrace.Core.Models;

namespace ZeroTrace.Diagnostics.System;

[SupportedOSPlatform("windows")]
public sealed class WindowsDefenderStatusPostureDiagnostic : IDiagnosticCheck
{
    public string Name => "Windows Defender Status Posture";

    public async Task<DiagnosticResult> RunAsync(CancellationToken cancellationToken = default)
    {
        var started = DateTimeOffset.UtcNow;
        var evidence = new List<Evidence>();
        var findings = new List<Finding>();
        var now = DateTimeOffset.UtcNow;

        try
        {
            using var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = "-NoProfile -NonInteractive -Command \"Get-MpComputerStatus | Select-Object AMServiceEnabled,AntivirusEnabled,AntispywareEnabled,RealTimeProtectionEnabled,BehaviorMonitorEnabled,IoavProtectionEnabled,NISEnabled | ConvertTo-Json -Compress\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
            await process.WaitForExitAsync(cancellationToken);

            evidence.Add(new("DefenderStatusPosture", "StatusReadable",
                process.ExitCode == 0 && !string.IsNullOrWhiteSpace(output) ? "True" : "False",
                EvidenceState.Info, now));
            evidence.Add(new("DefenderStatusPosture", "StatusSnapshot",
                string.IsNullOrWhiteSpace(output) ? "Unavailable" : output.Trim(),
                EvidenceState.Info, now));
        }
        catch
        {
            evidence.Add(new("DefenderStatusPosture", "StatusReadable", "False", EvidenceState.Info, now));
        }

        evidence.Add(new("DefenderStatusPosture", "MutationPolicy", "ReadOnlyDiagnostic", EvidenceState.Pass, now));
        return new DiagnosticResult(Name, started, DateTimeOffset.UtcNow, true, evidence, findings);
    }
}
