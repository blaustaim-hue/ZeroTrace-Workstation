using Microsoft.Win32;
using System.Runtime.Versioning;
using ZeroTrace.Core.Contracts;
using ZeroTrace.Core.Models;

namespace ZeroTrace.Diagnostics.System;

[SupportedOSPlatform("windows")]
public sealed class WindowsDnsPostureDiagnostic : IDiagnosticCheck
{
    public string Name => "Windows DNS Posture";

    public Task<DiagnosticResult> RunAsync(CancellationToken cancellationToken = default)
    {
        var started = DateTimeOffset.UtcNow;
        cancellationToken.ThrowIfCancellationRequested();
        var evidence = new List<Evidence>();
        var findings = new List<Finding>();
        var now = DateTimeOffset.UtcNow;

        try
        {
            using var parameters = Registry.LocalMachine.OpenSubKey(
                @"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters");

            var hostName = parameters?.GetValue("Hostname")?.ToString() ?? "Unavailable";
            var domain = parameters?.GetValue("Domain")?.ToString() ?? "Unavailable";
            var nameServer = parameters?.GetValue("NameServer")?.ToString() ?? "Automatic/DHCP";

            evidence.Add(new("DNS", "HostName", hostName, EvidenceState.Info, now));
            evidence.Add(new("DNS", "Domain", domain, EvidenceState.Info, now));
            evidence.Add(new("DNS", "ConfiguredNameServer", nameServer, EvidenceState.Info, now));
        }
        catch
        {
            evidence.Add(new("DNS", "PostureRead", "ReadFailed", EvidenceState.Info, now));
        }

        evidence.Add(new("DNS", "MutationPolicy", "ReadOnlyDiagnostic", EvidenceState.Pass, now));

        return Task.FromResult(new DiagnosticResult(
            Name, started, DateTimeOffset.UtcNow, true, evidence, findings));
    }
}
