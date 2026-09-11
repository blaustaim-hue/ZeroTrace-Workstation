namespace ZeroTrace.Core.Models;
public sealed record DiagnosticResult(string DiagnosticName, DateTimeOffset StartedAt, DateTimeOffset CompletedAt, bool Success, IReadOnlyList<Evidence> Evidence, IReadOnlyList<Finding> Findings);
