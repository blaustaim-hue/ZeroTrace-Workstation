using ZeroTrace.Core.Models;
namespace ZeroTrace.Core.Contracts;
public interface IDiagnosticCheck { string Name { get; } Task<DiagnosticResult> RunAsync(CancellationToken cancellationToken = default); }
