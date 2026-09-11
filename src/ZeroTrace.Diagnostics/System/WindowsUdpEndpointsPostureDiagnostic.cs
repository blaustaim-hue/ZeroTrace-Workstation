using System.Net.NetworkInformation;
using System.Runtime.Versioning;
using ZeroTrace.Core.Contracts;
using ZeroTrace.Core.Models;

namespace ZeroTrace.Diagnostics.System;

[SupportedOSPlatform("windows")]
public sealed class WindowsUdpEndpointsPostureDiagnostic : IDiagnosticCheck
{
    public string Name => "Windows UDP Endpoints Posture";

    public Task<DiagnosticResult> RunAsync(CancellationToken cancellationToken = default)
    {
        var started = DateTimeOffset.UtcNow;
        cancellationToken.ThrowIfCancellationRequested();
        var evidence = new List<Evidence>();
        var findings = new List<Finding>();
        var now = DateTimeOffset.UtcNow;

        try
        {
            var endpoints = IPGlobalProperties.GetIPGlobalProperties().GetActiveUdpListeners();
            var loopback = endpoints.Count(e => System.Net.IPAddress.IsLoopback(e.Address));
            var wildcard = endpoints.Count(e => e.Address.Equals(System.Net.IPAddress.Any) ||
                                                e.Address.Equals(System.Net.IPAddress.IPv6Any));

            evidence.Add(new("UdpEndpointsPosture", "ActiveUdpListeners", endpoints.Length.ToString(), EvidenceState.Info, now));
            evidence.Add(new("UdpEndpointsPosture", "LoopbackListeners", loopback.ToString(), EvidenceState.Info, now));
            evidence.Add(new("UdpEndpointsPosture", "WildcardListeners", wildcard.ToString(), EvidenceState.Info, now));
        }
        catch
        {
            evidence.Add(new("UdpEndpointsPosture", "ReadStatus", "ReadFailed", EvidenceState.Info, now));
        }

        evidence.Add(new("UdpEndpointsPosture", "MutationPolicy", "ReadOnlyDiagnostic", EvidenceState.Pass, now));

        return Task.FromResult(new DiagnosticResult(
            Name, started, DateTimeOffset.UtcNow, true, evidence, findings));
    }
}
