namespace ZeroTrace.Core.Models;

public sealed record StorageDeviceInfo(
    string Name,
    string DriveFormat,
    string DriveType,
    long TotalBytes,
    long FreeBytes,
    double FreePercent);
