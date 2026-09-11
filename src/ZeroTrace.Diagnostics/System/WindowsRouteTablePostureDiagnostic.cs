using System.Diagnostics;
using System.Runtime.Versioning;
using ZeroTrace.Core.Contracts;
using ZeroTrace.Core.Models;

namespace ZeroTrace.Diagnostics.System;

[SupportedOSPlatform("windows")]
public sealed class WindowsRouteTablePostureDiagnostic : IDiagnosticCheck
{
    public string Name => "Windows Route Table Posture";

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
                    FileName = "route.exe",
                    Arguments = "print",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            process.Start();
            var output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
            await process.WaitForExitAsync(cancellationToken);

            var lines = output.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            evidence.Add(new("RouteTablePosture", "OutputLines", lines.Length.ToString(), EvidenceState.Info, now));
            evidence.Add(new("RouteTablePosture", "ReadStatus",
                process.ExitCode == 0 ? "ReadSucceeded" : "ReadFailed", EvidenceState.Info, now));
        }
        catch
        {
            evidence.Add(new("RouteTablePosture", "ReadStatus", "ReadFailed", EvidenceState.Info, now));
        }

        evidence.Add(new("RouteTablePosture", "MutationPolicy", "ReadOnlyDiagnostic", EvidenceState.Pass, now));

        return new DiagnosticResult(Name, started, DateTimeOffset.UtcNow, true, evidence, findings);
    }
}
