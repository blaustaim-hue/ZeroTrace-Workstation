using System.Net.NetworkInformation;
using System.Net.Sockets;
using ZeroTrace.Core.Contracts;
using ZeroTrace.Core.Models;

namespace ZeroTrace.Diagnostics.Network;

public sealed class NetworkInventoryDiagnostic : IDiagnosticCheck
{
    public string Name => "Network Inventory";

    public Task<DiagnosticResult> RunAsync(CancellationToken cancellationToken = default)
    {
        var started = DateTimeOffset.UtcNow;
        cancellationToken.ThrowIfCancellationRequested();

        var evidence = new List<Evidence>();
        var findings = new List<Finding>();
        var activeUsableAdapters = 0;

        foreach (var nic in NetworkInterface.GetAllNetworkInterfaces())
        {
            cancellationToken.ThrowIfCancellationRequested();

            var now = DateTimeOffset.UtcNow;
            var ip = nic.GetIPProperties();
            var isLoopback = nic.NetworkInterfaceType == NetworkInterfaceType.Loopback;
            var isUp = nic.OperationalStatus == OperationalStatus.Up;
            var hasIpv4 = ip.UnicastAddresses.Any(a =>
                a.Address.AddressFamily == AddressFamily.InterNetwork &&
                !IPAddressIsAutomaticPrivate(a.Address));

            if (!isLoopback && isUp && hasIpv4)
                activeUsableAdapters++;

            evidence.Add(new("Network", $"{nic.Name}:Status", nic.OperationalStatus.ToString(),
                isUp ? EvidenceState.Pass : EvidenceState.Info, now));
            evidence.Add(new("Network", $"{nic.Name}:Type", nic.NetworkInterfaceType.ToString(), EvidenceState.Info, now));
            evidence.Add(new("Network", $"{nic.Name}:Speed", nic.Speed.ToString(), EvidenceState.Info, now));

            foreach (var addr in ip.UnicastAddresses)
                evidence.Add(new("Network", $"{nic.Name}:IP", addr.Address.ToString(), EvidenceState.Info, now));

            foreach (var dns in ip.DnsAddresses)
                evidence.Add(new("Network", $"{nic.Name}:DNS", dns.ToString(), EvidenceState.Info, now));

            foreach (var gw in ip.GatewayAddresses)
                evidence.Add(new("Network", $"{nic.Name}:Gateway", gw.Address.ToString(), EvidenceState.Info, now));
        }

        var summaryState = activeUsableAdapters > 0 ? EvidenceState.Pass : EvidenceState.Warning;
        var summary = new Evidence(
            "Network",
            "ActiveUsableAdapters",
            activeUsableAdapters.ToString(),
            summaryState,
            DateTimeOffset.UtcNow);
        evidence.Add(summary);

        if (activeUsableAdapters == 0)
        {
            findings.Add(new Finding(
                "NET-001",
                "No active IPv4 network adapter detected",
                "ZeroTrace did not detect an active non-loopback adapter with a usable IPv4 address.",
                "Warning",
                90,
                new[] { summary },
                "Check adapter status, cabling or Wi-Fi connectivity, IP configuration and corporate network requirements before changing settings."));
        }

        return Task.FromResult(new DiagnosticResult(
            Name,
            started,
            DateTimeOffset.UtcNow,
            true,
            evidence,
            findings));
    }

    private static bool IPAddressIsAutomaticPrivate(global::System.Net.IPAddress address)
    {
        var bytes = address.GetAddressBytes();
        return bytes.Length == 4 && bytes[0] == 169 && bytes[1] == 254;
    }
}
