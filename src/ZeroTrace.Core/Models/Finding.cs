namespace ZeroTrace.Core.Models;
public sealed record Finding(string Id, string Title, string Description, string Severity, int Confidence, IReadOnlyList<Evidence> Evidence, string? SuggestedNextAction = null);
