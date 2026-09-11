namespace ZeroTrace.Core.Models;
public sealed record Evidence(string Source, string Key, string Value, EvidenceState State, DateTimeOffset ObservedAt);
