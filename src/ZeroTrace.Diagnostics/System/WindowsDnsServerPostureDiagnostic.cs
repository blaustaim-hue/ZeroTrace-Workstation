using System.Net.NetworkInformation;
using System.Runtime.Versioning;
using ZeroTrace.Core.Contracts;
using ZeroTrace.Core.Models;

namespace ZeroTrace.Diagnostics.System;

[SupportedOSPlatform("windows")]
public sealed class WindowsDnsServerPostureDiagnostic : IDiagnosticCheck
{
    public string Name => "Windows DNS Server Posture";

    public Task<DiagnosticResult> RunAsync(CancellationToken cancellationToken = default)
    {
        var started = DateTimeOffset.UtcNow;
        cancellationToken.ThrowIfCancellationRequested();
        var evidence = new List<Evidence>();
        var findings = new List<Finding>();
        var now = DateTimeOffset.UtcNow;

        try
        {
            var adapters = NetworkInterface.GetAllNetworkInterfaces()
                .Where(a => a.OperationalStatus == OperationalStatus.Up);

            foreach (var adapter in adapters.Take(10))
            {
                cancellationToken.ThrowIfCancellationRequested();
                var dnsServers = adapter.GetIPProperties().DnsAddresses
                    .Select(x => x.ToString())
                    .ToArray();

                evidence.Add(new(
                    "DnsServer",
                    adapter.Name,
                    dnsServers.Length == 0 ? "None" : string.Join(", ", dnsServers),
                    EvidenceState.Info,
                    now));
            }
        }
        catch
        {
            evidence.Add(new("DnsServer", "PostureRead", "ReadFailed", EvidenceState.Info, now));
        }

        evidence.Add(new("DnsServer", "MutationPolicy", "ReadOnlyDiagnostic", EvidenceState.Pass, now));

        return Task.FromResult(new DiagnosticResult(
            Name, started, DateTimeOffset.UtcNow, true, evidence, findings));
    }
}
