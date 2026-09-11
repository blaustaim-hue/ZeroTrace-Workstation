using ZeroTrace.Core.Models;
namespace ZeroTrace.Remediation;
public sealed record RemediationPlan(DateTimeOffset CreatedAt, IReadOnlyList<RemediationAction> Actions);
