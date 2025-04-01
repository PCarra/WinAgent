using System;
using System.Net;
using System.Net.Sockets;
using System.Management;
using Microsoft.Win32;
using System.Net.NetworkInformation;
public class HostInfoChecker
{

     // Function to get the Local IP Address of the host
     public static string GetLocalIPAddress()
     {
          try
          {
               var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces()
                   .Where(n => n.OperationalStatus == OperationalStatus.Up &&
                               n.NetworkInterfaceType != NetworkInterfaceType.Loopback &&
                               n.NetworkInterfaceType != NetworkInterfaceType.Tunnel);

               foreach (var netInterface in networkInterfaces)
               {
                    var ipProps = netInterface.GetIPProperties();
                    foreach (var ip in ipProps.UnicastAddresses)
                    {
                         if (ip.Address.AddressFamily == AddressFamily.InterNetwork &&
                             !ip.Address.ToString().StartsWith("169.254")) // Exclude APIPA
                         {
                              return ip.Address.ToString();
                         }
                    }
               }

               throw new Exception("No valid IPv4 address found.");
          }
          catch (Exception ex)
          {
               Console.WriteLine($"Error: {ex.Message}");
               return "Unknown";
          }
     }

     public static string GetOSAutoUpdateSetting()
        {
          string autoUpdateSetting;

          try
          {
               // Check if the registry key exists
               using (RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Policies\Microsoft\Windows\WindowsUpdate\AU"))
               {
                    if (key == null)
                    {
                         // Key does not exist, use default setting
                         autoUpdateSetting = "Off";
                    }
                    else
                    {
                         // Get the value of NoAutoUpdate
                         object noAutoUpdateValue = key.GetValue("NoAutoUpdate");

                         if (noAutoUpdateValue is int noAutoUpdate)
                         {
                              // Convert the value to "Off" or "On"
                              autoUpdateSetting = noAutoUpdate == 1 ? "Off" : "On";
                         }
                         else
                         {
                              // Value is not set properly, default to "On"
                              autoUpdateSetting = "On";
                         }
                    }
               }
          }
          catch (Exception ex)
          {
               // Log or handle exception as needed
               Console.WriteLine($"Error retrieving Windows Auto Update setting: {ex.Message}");
               autoUpdateSetting = "Unknown";
          }

          return autoUpdateSetting;
        }

         public static string GetOSVersionAndEdition()
        {
            try
            {
                // Get OS version
                string osVersion = Environment.OSVersion.Version.ToString();

                // Query WMI for OS Edition
                string query = "SELECT Caption, OSArchitecture FROM Win32_OperatingSystem";
                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(query))
                using (ManagementObjectCollection results = searcher.Get())
                {
                    foreach (ManagementObject obj in results)
                    {
                        string caption = obj["Caption"]?.ToString() ?? "Unknown OS";
                        string architecture = obj["OSArchitecture"]?.ToString() ?? "Unknown Architecture";

                        return $"{caption} ({architecture}), Version: {osVersion}";
                    }
                }

                return $"OS Version: {osVersion}, Edition: Unknown";
            }
            catch (Exception ex)
            {
                return $"Error retrieving OS information: {ex.Message}";
            }
        }
}