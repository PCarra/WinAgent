using System;
using Microsoft.Win32;

public class RdpChecker
{
    public static string GetRdpStatus()
    {
        try
        {
            // Path to the registry key that controls RDP settings
            string keyPath = @"SYSTEM\CurrentControlSet\Control\Terminal Server";
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(keyPath))
            {
                if (key != null)
                {
                    // Get the value of fDenyTSConnections
                    object rdpStatus = key.GetValue("fDenyTSConnections");
                    if (rdpStatus != null)
                    {
                        int status = (int)rdpStatus;
                        if (status == 0)
                        {
                            return "RDP is enabled on this host.";
                        }
                        else
                        {
                            return "RDP is disabled on this host.";
                        }
                    }
                    else
                    {
                        return "RDP status setting not found in registry.";
                    }
                }
                else
                {
                    return "Registry key for RDP status not found.";
                }
            }
        }
        catch (Exception ex)
        {
            return $"Error retrieving RDP status: {ex.Message}";
        }
    }
}
