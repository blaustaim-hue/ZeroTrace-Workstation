using System.Runtime.Versioning;
using ZeroTrace.Core.Contracts;
using ZeroTrace.Core.Models;

namespace ZeroTrace.Enterprise;

[SupportedOSPlatform("windows")]
public sealed class EnterpriseEnvironmentDiagnostic : IDiagnosticCheck
{
    public string Name => "Enterprise Environment";

    public Task<DiagnosticResult> RunAsync(CancellationToken cancellationToken = default)
    {
        var started = DateTimeOffset.UtcNow;
        cancellationToken.ThrowIfCancellationRequested();

        var detector = new EnterpriseDetector();
        var indicators = detector.Discover();
        var mode = detector.Detect();
        var now = DateTimeOffset.UtcNow;

        var evidence = new List<Evidence>
        {
            new("Enterprise", "IsDomainJoined", indicators.IsDomainJoined.ToString(), EvidenceState.Info, now),
            new("Enterprise", "DomainName", indicators.DomainName ?? "NotDetected", EvidenceState.Info, now),
            new("Enterprise", "IsAzureAdJoined", indicators.IsAzureAdJoined.ToString(), EvidenceState.Info, now),
            new("Enterprise", "HasGroupPolicyArtifacts", indicators.HasGroupPolicyArtifacts.ToString(), EvidenceState.Info, now),
            new("Enterprise", "HasMdmArtifacts", indicators.HasMdmArtifacts.ToString(), EvidenceState.Info, now),
            new("Enterprise", "RecommendedMode", mode.ToString(), EvidenceState.Info, now)
        };

        foreach (var item in indicators.Evidence)
        {
            evidence.Add(new(
                "Enterprise",
                "DiscoveryEvidence",
                item,
                EvidenceState.Info,
                now));
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
