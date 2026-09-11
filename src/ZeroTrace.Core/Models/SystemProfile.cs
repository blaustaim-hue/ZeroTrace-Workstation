namespace ZeroTrace.Core.Models;

public sealed record SystemProfile(
    string MachineName,
    string OsDescription,
    string OsArchitecture,
    string ProcessArchitecture,
    string FrameworkDescription,
    int ProcessorCount,
    long WorkingSetBytes,
    TimeSpan Uptime);
