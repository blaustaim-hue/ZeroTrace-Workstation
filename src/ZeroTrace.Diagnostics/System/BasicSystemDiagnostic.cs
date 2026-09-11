using ZeroTrace.Core.Contracts;
using ZeroTrace.Core.Models;
namespace ZeroTrace.Diagnostics.System;
public sealed class BasicSystemDiagnostic : IDiagnosticCheck
{
 public string Name => "Basic System Diagnostic";
 public Task<DiagnosticResult> RunAsync(CancellationToken cancellationToken = default)
 {
  var started = DateTimeOffset.UtcNow;
  var evidence = new List<Evidence>
  {
   new("System","MachineName",Environment.MachineName,EvidenceState.Info,DateTimeOffset.UtcNow),
   new("System","OSVersion",Environment.OSVersion.VersionString,EvidenceState.Info,DateTimeOffset.UtcNow),
   new("System","64BitOS",Environment.Is64BitOperatingSystem.ToString(),EvidenceState.Info,DateTimeOffset.UtcNow),
   new("System","ProcessorCount",Environment.ProcessorCount.ToString(),EvidenceState.Info,DateTimeOffset.UtcNow)
  };
  return Task.FromResult(new DiagnosticResult(Name,started,DateTimeOffset.UtcNow,true,evidence,Array.Empty<Finding>()));
 }
}
