using System.Net.NetworkInformation;
using System.Runtime.Versioning;
using ZeroTrace.Core.Contracts;
using ZeroTrace.Core.Models;

namespace ZeroTrace.Diagnostics.System;

[SupportedOSPlatform("windows")]
public sealed class WindowsListeningPortsPostureDiagnostic : IDiagnosticCheck
{
    public string Name => "Windows Listening Ports Posture";

    public Task<DiagnosticResult> RunAsync(CancellationToken cancellationToken = default)
    {
        var started = DateTimeOffset.UtcNow;
        cancellationToken.ThrowIfCancellationRequested();
        var evidence = new List<Evidence>();
        var findings = new List<Finding>();
        var now = DateTimeOffset.UtcNow;

        try
        {
            var properties = IPGlobalProperties.GetIPGlobalProperties();
            var tcp = properties.GetActiveTcpListeners();
            var udp = properties.GetActiveUdpListeners();

            evidence.Add(new("ListeningPortsPosture", "TcpListeners", tcp.Length.ToString(), EvidenceState.Info, now));
            evidence.Add(new("ListeningPortsPosture", "UdpListeners", udp.Length.ToString(), EvidenceState.Info, now));
            evidence.Add(new("ListeningPortsPosture", "TotalListeners", (tcp.Length + udp.Length).ToString(), EvidenceState.Info, now));
        }
        catch
        {
            evidence.Add(new("ListeningPortsPosture", "ReadStatus", "ReadFailed", EvidenceState.Info, now));
        }

        evidence.Add(new("ListeningPortsPosture", "MutationPolicy", "ReadOnlyDiagnostic", EvidenceState.Pass, now));

        return Task.FromResult(new DiagnosticResult(
            Name, started, DateTimeOffset.UtcNow, true, evidence, findings));
    }
}
