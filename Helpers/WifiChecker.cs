using System;
using System.Diagnostics;
using System.Linq;

public class WiFiInfo
{
     public static string GetWiFiSSID()
     {
          try
          {
               string netshOutput = ExecuteNetshCommand("netsh wlan show interfaces");
               string ssid = ParseNetshOutput(netshOutput, "Profile");
               return string.IsNullOrEmpty(ssid) ? "Wired Connection" : ssid;
          }
          catch (Exception ex)
          {
               Console.WriteLine($"Error retrieving WiFi SSID: {ex.Message}");
               return "Error";
          }
     }

     public static string GetWiFiSecurityType()
     {
          try
          {
               string netshOutput = ExecuteNetshCommand("netsh wlan show interfaces");
               string ssid = ParseNetshOutput(netshOutput, "Profile");
               if (string.IsNullOrEmpty(ssid))
               {
                    return "N/A"; // No wireless connection
               }

               string securityType = ParseNetshOutput(netshOutput, "Authentication");
               return securityType ?? "Unknown";
          }
          catch (Exception ex)
          {
               Console.WriteLine($"Error retrieving WiFi Security Type: {ex.Message}");
               return "Error";
          }
     }

     private static string ExecuteNetshCommand(string command)
     {
          ProcessStartInfo psi = new ProcessStartInfo
          {
               FileName = "cmd.exe",
               Arguments = $"/C {command}",
               RedirectStandardOutput = true,
               UseShellExecute = false,
               CreateNoWindow = true
          };

          using (Process process = new Process { StartInfo = psi })
          {
               process.Start();
               string output = process.StandardOutput.ReadToEnd();
               process.WaitForExit();
               return output;
          }
     }

     private static string ParseNetshOutput(string output, string key)
     {
          // Split output into lines and find the one containing the key
          var line = output.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries)
                           .FirstOrDefault(l => l.Trim().StartsWith(key, StringComparison.OrdinalIgnoreCase));

          if (line != null)
          {
               // Split the line by ':' and return the value
               string[] parts = line.Split(new[] { ':' }, 2, StringSplitOptions.RemoveEmptyEntries);
               if (parts.Length > 1)
               {
                    return parts[1].Trim();
               }
          }

          return null;
     }
}