using System.Net.NetworkInformation;
using System.Runtime.Versioning;
using ZeroTrace.Core.Contracts;
using ZeroTrace.Core.Models;

namespace ZeroTrace.Diagnostics.System;

[SupportedOSPlatform("windows")]
public sealed class WindowsDhcpPostureDiagnostic : IDiagnosticCheck
{
    public string Name => "Windows DHCP Posture";

    public Task<DiagnosticResult> RunAsync(CancellationToken cancellationToken = default)
    {
        var started = DateTimeOffset.UtcNow;
        cancellationToken.ThrowIfCancellationRequested();
        var evidence = new List<Evidence>();
        var findings = new List<Finding>();
        var now = DateTimeOffset.UtcNow;

        try
        {
            foreach (var adapter in NetworkInterface.GetAllNetworkInterfaces()
                         .Where(a => a.OperationalStatus == OperationalStatus.Up)
                         .Take(10))
            {
                cancellationToken.ThrowIfCancellationRequested();
                var props = adapter.GetIPProperties();
                var dhcpServers = props.DhcpServerAddresses.Select(x => x.ToString()).ToArray();

                evidence.Add(new(
                    "DhcpPosture",
                    adapter.Name,
                    dhcpServers.Length == 0 ? "NoDhcpServerReported" : string.Join(", ", dhcpServers),
                    EvidenceState.Info,
                    now));
            }
        }
        catch
        {
            evidence.Add(new("DhcpPosture", "PostureRead", "ReadFailed", EvidenceState.Info, now));
        }

        evidence.Add(new("DhcpPosture", "MutationPolicy", "ReadOnlyDiagnostic", EvidenceState.Pass, now));

        return Task.FromResult(new DiagnosticResult(
            Name, started, DateTimeOffset.UtcNow, true, evidence, findings));
    }
}
