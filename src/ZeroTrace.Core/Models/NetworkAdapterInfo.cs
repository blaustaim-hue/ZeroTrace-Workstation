namespace ZeroTrace.Core.Models;

public sealed record NetworkAdapterInfo(
    string Name,
    string Description,
    string OperationalStatus,
    string InterfaceType,
    long SpeedBitsPerSecond,
    IReadOnlyList<string> UnicastAddresses,
    IReadOnlyList<string> DnsAddresses,
    IReadOnlyList<string> GatewayAddresses);
