using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.Versioning;
using ZeroTrace.Core.Contracts;
using ZeroTrace.Core.Models;

namespace ZeroTrace.Diagnostics.System;

[SupportedOSPlatform("windows")]
public sealed class WindowsIpConfigurationPostureDiagnostic : IDiagnosticCheck
{
    public string Name => "Windows IP Configuration Posture";

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
                var properties = adapter.GetIPProperties();

                var ipv4 = properties.UnicastAddresses
                    .Where(x => x.Address.AddressFamily == AddressFamily.InterNetwork)
                    .Select(x => x.Address.ToString())
                    .ToArray();

                var gateways = properties.GatewayAddresses
                    .Where(x => x.Address.AddressFamily == AddressFamily.InterNetwork)
                    .Select(x => x.Address.ToString())
                    .ToArray();

                evidence.Add(new("IPConfiguration", $"{adapter.Name}.IPv4",
                    ipv4.Length == 0 ? "None" : string.Join(", ", ipv4), EvidenceState.Info, now));
                evidence.Add(new("IPConfiguration", $"{adapter.Name}.Gateway",
                    gateways.Length == 0 ? "None" : string.Join(", ", gateways), EvidenceState.Info, now));
            }
        }
        catch
        {
            evidence.Add(new("IPConfiguration", "PostureRead", "ReadFailed", EvidenceState.Info, now));
        }

        evidence.Add(new("IPConfiguration", "MutationPolicy", "ReadOnlyDiagnostic", EvidenceState.Pass, now));

        return Task.FromResult(new DiagnosticResult(
            Name, started, DateTimeOffset.UtcNow, true, evidence, findings));
    }
}
