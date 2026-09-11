using ZeroTrace.Core.Models;
using ZeroTrace.Enterprise;
namespace ZeroTrace.Safety;
public sealed class SafetyPolicy
{
 public SafetyDecision Evaluate(RemediationAction action, EnterpriseMode mode)
 {
  if (action.Risk == RiskLevel.Red) return new(false, action.Risk, "RED actions are blocked by default.");
  if (mode == EnterpriseMode.EnterpriseSafe && action.Risk == RiskLevel.Amber) return new(false, action.Risk, "AMBER action requires enterprise review.");
  return new(true, action.Risk, "Action is eligible for explicit user approval.");
 }
}
