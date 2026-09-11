using System.Net.NetworkInformation;
using System.Runtime.Versioning;
using ZeroTrace.Core.Contracts;
using ZeroTrace.Core.Models;

namespace ZeroTrace.Diagnostics.System;

[SupportedOSPlatform("windows")]
public sealed class WindowsNetworkAdapterPostureDiagnostic : IDiagnosticCheck
{
    public string Name => "Windows Network Adapter Posture";

    public Task<DiagnosticResult> RunAsync(CancellationToken cancellationToken = default)
    {
        var started = DateTimeOffset.UtcNow;
        cancellationToken.ThrowIfCancellationRequested();
        var evidence = new List<Evidence>();
        var findings = new List<Finding>();
        var now = DateTimeOffset.UtcNow;

        try
        {
            var adapters = NetworkInterface.GetAllNetworkInterfaces();
            evidence.Add(new("NetworkAdapter", "AdapterCount", adapters.Length.ToString(), EvidenceState.Info, now));

            var active = adapters.Where(a => a.OperationalStatus == OperationalStatus.Up).ToArray();
            evidence.Add(new("NetworkAdapter", "ActiveAdapterCount", active.Length.ToString(), EvidenceState.Info, now));

            foreach (var adapter in active.Take(10))
            {
                cancellationToken.ThrowIfCancellationRequested();
                evidence.Add(new(
                    "NetworkAdapter",
                    adapter.Name,
                    $"{adapter.NetworkInterfaceType}; {adapter.Speed} bps; {adapter.OperationalStatus}",
                    EvidenceState.Info,
                    now));
            }
        }
        catch
        {
            evidence.Add(new("NetworkAdapter", "PostureRead", "ReadFailed", EvidenceState.Info, now));
        }

        evidence.Add(new("NetworkAdapter", "MutationPolicy", "ReadOnlyDiagnostic", EvidenceState.Pass, now));

        return Task.FromResult(new DiagnosticResult(
            Name, started, DateTimeOffset.UtcNow, true, evidence, findings));
    }
}
