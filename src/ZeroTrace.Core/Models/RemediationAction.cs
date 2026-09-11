namespace ZeroTrace.Core.Models;
public sealed record RemediationAction(string Id, string Title, string Description, RiskLevel Risk, bool RequiresElevation, bool Reversible, string EnterprisePolicy);
