using System.Net.NetworkInformation;
using System.Runtime.Versioning;
using ZeroTrace.Core.Contracts;
using ZeroTrace.Core.Models;

namespace ZeroTrace.Diagnostics.System;

[SupportedOSPlatform("windows")]
public sealed class WindowsGatewayPostureDiagnostic : IDiagnosticCheck
{
    public string Name => "Windows Default Gateway Posture";

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
                var gateways = adapter.GetIPProperties().GatewayAddresses
                    .Select(g => g.Address.ToString())
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .ToArray();

                evidence.Add(new(
                    "GatewayPosture",
                    adapter.Name,
                    gateways.Length == 0 ? "NoDefaultGatewayReported" : string.Join(", ", gateways),
                    EvidenceState.Info,
                    now));
            }
        }
        catch
        {
            evidence.Add(new("GatewayPosture", "PostureRead", "ReadFailed", EvidenceState.Info, now));
        }

        evidence.Add(new("GatewayPosture", "MutationPolicy", "ReadOnlyDiagnostic", EvidenceState.Pass, now));

        return Task.FromResult(new DiagnosticResult(
            Name, started, DateTimeOffset.UtcNow, true, evidence, findings));
    }
}
