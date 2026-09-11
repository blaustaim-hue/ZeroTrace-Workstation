namespace ZeroTrace.Core.Models;

public sealed record EnterpriseIndicators(
    bool IsDomainJoined,
    string? DomainName,
    bool IsAzureAdJoined,
    bool HasGroupPolicyArtifacts,
    bool HasMdmArtifacts,
    IReadOnlyList<string> Evidence);
