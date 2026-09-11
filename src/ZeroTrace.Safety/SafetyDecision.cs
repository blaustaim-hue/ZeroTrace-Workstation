using ZeroTrace.Core.Models;
namespace ZeroTrace.Safety;
public sealed record SafetyDecision(bool Allowed, RiskLevel Risk, string Reason);
