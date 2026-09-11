using Microsoft.Win32;
using System.Runtime.Versioning;
using ZeroTrace.Core.Contracts;
using ZeroTrace.Core.Models;

namespace ZeroTrace.Diagnostics.System;

[SupportedOSPlatform("windows")]
public sealed class WindowsProxyPostureDiagnostic : IDiagnosticCheck
{
    public string Name => "Windows Proxy Posture";

    public Task<DiagnosticResult> RunAsync(CancellationToken cancellationToken = default)
    {
        var started = DateTimeOffset.UtcNow;
        cancellationToken.ThrowIfCancellationRequested();
        var evidence = new List<Evidence>();
        var findings = new List<Finding>();
        var now = DateTimeOffset.UtcNow;

        try
        {
            using var internetSettings = Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Internet Settings");

            var proxyEnabled = Convert.ToInt32(internetSettings?.GetValue("ProxyEnable") ?? 0) != 0;
            var proxyServer = internetSettings?.GetValue("ProxyServer")?.ToString() ?? "NotConfigured";
            var autoConfigUrl = internetSettings?.GetValue("AutoConfigURL")?.ToString() ?? "NotConfigured";

            evidence.Add(new("Proxy", "ProxyEnabled", proxyEnabled.ToString(), EvidenceState.Info, now));
            evidence.Add(new("Proxy", "ProxyServer", proxyServer, EvidenceState.Info, now));
            evidence.Add(new("Proxy", "AutoConfigURL", autoConfigUrl, EvidenceState.Info, now));
        }
        catch
        {
            evidence.Add(new("Proxy", "PostureRead", "ReadFailed", EvidenceState.Info, now));
        }

        evidence.Add(new("Proxy", "MutationPolicy", "ReadOnlyDiagnostic", EvidenceState.Pass, now));

        return Task.FromResult(new DiagnosticResult(
            Name, started, DateTimeOffset.UtcNow, true, evidence, findings));
    }
}
