using System.Runtime.Versioning;
using ZeroTrace.Core.Contracts;
using ZeroTrace.Core.Models;

namespace ZeroTrace.Diagnostics.System;

[SupportedOSPlatform("windows")]
public sealed class SystemUptimeDiagnostic : IDiagnosticCheck
{
    public string Name => "System Uptime";

    public Task<DiagnosticResult> RunAsync(CancellationToken cancellationToken = default)
    {
        var started = DateTimeOffset.UtcNow;
        cancellationToken.ThrowIfCancellationRequested();

        var now = DateTimeOffset.UtcNow;
        var uptime = TimeSpan.FromMilliseconds(Environment.TickCount64);
        var bootTime = now - uptime;

        var evidence = new List<Evidence>
        {
            new("System", "Uptime", uptime.ToString(@"d\.hh\:mm\:ss"), EvidenceState.Info, now),
            new("System", "EstimatedBootTimeUtc", bootTime.UtcDateTime.ToString("O"), EvidenceState.Info, now)
        };

        var findings = new List<Finding>();

        if (uptime >= TimeSpan.FromDays(14))
        {
            findings.Add(new Finding(
                "SYS-UPTIME-001",
                "System has been running for an extended period",
                $"Windows uptime is approximately {Math.Floor(uptime.TotalDays)} days.",
                "Info",
                70,
                evidence,
                "Consider a planned restart when troubleshooting performance, update, driver, or service issues. On managed devices, follow organizational maintenance and change-control policy."));
        }

        return Task.FromResult(new DiagnosticResult(
            Name,
            started,
            DateTimeOffset.UtcNow,
            true,
            evidence,
            findings));
    }
}
