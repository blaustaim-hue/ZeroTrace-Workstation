using ZeroTrace.Core.Models;
namespace ZeroTrace.Remediation;
public sealed class RemediationPlanner
{
 public RemediationPlan Build(IEnumerable<RemediationAction> actions) => new(DateTimeOffset.UtcNow, actions.ToArray());
 // Execution is deliberately excluded from Foundation.
}
