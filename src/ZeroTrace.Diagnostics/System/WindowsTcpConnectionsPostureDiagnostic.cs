using System.Net.NetworkInformation;
using System.Runtime.Versioning;
using ZeroTrace.Core.Contracts;
using ZeroTrace.Core.Models;

namespace ZeroTrace.Diagnostics.System;

[SupportedOSPlatform("windows")]
public sealed class WindowsTcpConnectionsPostureDiagnostic : IDiagnosticCheck
{
    public string Name => "Windows TCP Connections Posture";

    public Task<DiagnosticResult> RunAsync(CancellationToken cancellationToken = default)
    {
        var started = DateTimeOffset.UtcNow;
        cancellationToken.ThrowIfCancellationRequested();
        var evidence = new List<Evidence>();
        var findings = new List<Finding>();
        var now = DateTimeOffset.UtcNow;

        try
        {
            var connections = IPGlobalProperties.GetIPGlobalProperties().GetActiveTcpConnections();
            var established = connections.Count(c => c.State == TcpState.Established);
            var listening = connections.Count(c => c.State == TcpState.Listen);
            var timeWait = connections.Count(c => c.State == TcpState.TimeWait);

            evidence.Add(new("TcpConnectionsPosture", "TotalConnections", connections.Length.ToString(), EvidenceState.Info, now));
            evidence.Add(new("TcpConnectionsPosture", "Established", established.ToString(), EvidenceState.Info, now));
            evidence.Add(new("TcpConnectionsPosture", "Listening", listening.ToString(), EvidenceState.Info, now));
            evidence.Add(new("TcpConnectionsPosture", "TimeWait", timeWait.ToString(), EvidenceState.Info, now));
        }
        catch
        {
            evidence.Add(new("TcpConnectionsPosture", "ReadStatus", "ReadFailed", EvidenceState.Info, now));
        }

        evidence.Add(new("TcpConnectionsPosture", "MutationPolicy", "ReadOnlyDiagnostic", EvidenceState.Pass, now));

        return Task.FromResult(new DiagnosticResult(
            Name, started, DateTimeOffset.UtcNow, true, evidence, findings));
    }
}
