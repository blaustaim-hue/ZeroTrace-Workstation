using System.Net.NetworkInformation;
using ZeroTrace.Core.Contracts;
using ZeroTrace.Core.Models;

namespace ZeroTrace.Diagnostics.Network;

public sealed class NetworkInventoryDiagnostic : IDiagnosticCheck
{
    public string Name => "Network Inventory";

    public Task<DiagnosticResult> RunAsync(CancellationToken cancellationToken = default)
    {
        var started = DateTimeOffset.UtcNow;
        var evidence = new List<Evidence>();

        foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
        {
            var ip = nic.GetIPProperties();

            evidence.Add(new("Network", $"{nic.Name}:Status", nic.OperationalStatus.ToString(), EvidenceState.Info, DateTimeOffset.UtcNow));
            evidence.Add(new("Network", $"{nic.Name}:Type", nic.NetworkInterfaceType.ToString(), EvidenceState.Info, DateTimeOffset.UtcNow));
            evidence.Add(new("Network", $"{nic.Name}:Speed", nic.Speed.ToString(), EvidenceState.Info, DateTimeOffset.UtcNow));

            foreach (var addr in ip.UnicastAddresses)
                evidence.Add(new("Network", $"{nic.Name}:IP", addr.Address.ToString(), EvidenceState.Info, DateTimeOffset.UtcNow));

            foreach (var dns in ip.DnsAddresses)
                evidence.Add(new("Network", $"{nic.Name}:DNS", dns.ToString(), EvidenceState.Info, DateTimeOffset.UtcNow));

            foreach (var gw in ip.GatewayAddresses)
                evidence.Add(new("Network", $"{nic.Name}:Gateway", gw.Address.ToString(), EvidenceState.Info, DateTimeOffset.UtcNow));
        }

        return Task.FromResult(new DiagnosticResult(
            Name,
            started,
            DateTimeOffset.UtcNow,
            true,
            evidence,
            Array.Empty<Finding>()));
    }
}
