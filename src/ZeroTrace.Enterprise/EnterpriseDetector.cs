using Microsoft.Win32;
using System.Runtime.Versioning;
using ZeroTrace.Core.Models;

namespace ZeroTrace.Enterprise;

[SupportedOSPlatform("windows")]
public sealed class EnterpriseDetector
{
    public EnterpriseIndicators Discover()
    {
        var evidence = new List<string>();

        string? domainName = null;
        var isDomainJoined = false;

        try
        {
            using var tcpip = Registry.LocalMachine.OpenSubKey(
                @"SYSTEM\CurrentControlSet\Services\Tcpip\Parameters");

            domainName = tcpip?.GetValue("Domain") as string;

            if (!string.IsNullOrWhiteSpace(domainName))
            {
                isDomainJoined = true;
                evidence.Add($"Domain configuration detected: {domainName}");
            }
            else
            {
                evidence.Add("Domain configuration not detected.");
            }
        }
        catch (Exception ex)
        {
            evidence.Add($"Domain discovery unavailable: {ex.GetType().Name}");
        }

        var isAzureAdJoined = false;
        try
        {
            using var joinInfo = Registry.LocalMachine.OpenSubKey(
                @"SYSTEM\CurrentControlSet\Control\CloudDomainJoin\JoinInfo");

            isAzureAdJoined = joinInfo?.GetSubKeyNames().Length > 0;
            evidence.Add(isAzureAdJoined
                ? "Microsoft Entra/Azure AD join artifacts detected."
                : "Microsoft Entra/Azure AD join artifacts not detected.");
        }
        catch (Exception ex)
        {
            evidence.Add($"Entra discovery unavailable: {ex.GetType().Name}");
        }

        var hasGroupPolicyArtifacts = false;
        try
        {
            using var gpo = Registry.LocalMachine.OpenSubKey(
                @"SOFTWARE\Microsoft\Windows\CurrentVersion\Group Policy");

            hasGroupPolicyArtifacts = gpo is not null;
            evidence.Add(hasGroupPolicyArtifacts
                ? "Group Policy registry artifacts detected."
                : "Group Policy registry artifacts not detected.");
        }
        catch (Exception ex)
        {
            evidence.Add($"Group Policy discovery unavailable: {ex.GetType().Name}");
        }

        var hasMdmArtifacts = false;
        try
        {
            using var mdm = Registry.LocalMachine.OpenSubKey(
                @"SOFTWARE\Microsoft\Enrollments");

            hasMdmArtifacts = mdm?.GetSubKeyNames().Length > 0;
            evidence.Add(hasMdmArtifacts
                ? "MDM enrollment artifacts detected."
                : "MDM enrollment artifacts not detected.");
        }
        catch (Exception ex)
        {
            evidence.Add($"MDM discovery unavailable: {ex.GetType().Name}");
        }

        return new EnterpriseIndicators(
            isDomainJoined,
            domainName,
            isAzureAdJoined,
            hasGroupPolicyArtifacts,
            hasMdmArtifacts,
            evidence);
    }

    public EnterpriseMode Detect()
    {
        var indicators = Discover();

        if (indicators.IsDomainJoined ||
            indicators.IsAzureAdJoined ||
            indicators.HasMdmArtifacts)
        {
            return EnterpriseMode.EnterpriseSafe;
        }

        return EnterpriseMode.Personal;
    }
}
