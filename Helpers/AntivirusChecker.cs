using System;
using System.Management;
using System.Diagnostics;
using System.DirectoryServices.Protocols;
using WindowsAgentService.Helpers;

class AntivirusChecker
{
    public static string GetActiveAntivirusName()
    {
        string command = "Get-MpComputerStatus | Select-Object -ExpandProperty AntivirusEnabled";
        using (Process powerShellProcess = new Process())
        {
            powerShellProcess.StartInfo.FileName = "powershell";
            powerShellProcess.StartInfo.Arguments = $"-Command \"{command}\"";
            powerShellProcess.StartInfo.RedirectStandardOutput = true;
            powerShellProcess.StartInfo.RedirectStandardError = true;
            powerShellProcess.StartInfo.UseShellExecute = false;
            powerShellProcess.StartInfo.CreateNoWindow = true;

            powerShellProcess.Start();

            string result = powerShellProcess.StandardOutput.ReadToEnd().Trim();
            string error = powerShellProcess.StandardError.ReadToEnd().Trim();

            powerShellProcess.WaitForExit();

            if (!string.IsNullOrWhiteSpace(error))
            {
                Logger.LogError($"PowerShell Error: {error}");
                throw new InvalidOperationException($"PowerShell Error: {error}");
            }
            //if defender then result will be true for this command
            else if (result == "True")
            {
                //Console.WriteLine("Defender Found!");
                //Console.WriteLine(result);
                return "Windows Defender";
            }
            //if not query the display name from Antivirus Product entry in root\SecurityCenter2
            else if (string.IsNullOrWhiteSpace(result))
            {
                string query = "SELECT displayName FROM AntivirusProduct";
                string namespacePath = @"\\.\root\SecurityCenter2";

                using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(namespacePath, query))
                using (ManagementObjectCollection results = searcher.Get())
                {
                    foreach (ManagementObject obj in results)
                    {
                        result = obj["displayName"]?.ToString() ?? "Unknown Antivirus";
                        return result;
                    }
                }

                return "No Active Antivirus Found";
            }

            return result.Trim();
        }
    }

    public static string GetSignatureStatus()
    {
        if (GetActiveAntivirusName() == "Windows Defender")
        {
            string command = "Get-MpComputerStatus | Select-Object -ExpandProperty AntivirusSignatureLastUpdated";
            using (Process powerShellProcess = new Process())
            {
                powerShellProcess.StartInfo.FileName = "powershell";
                powerShellProcess.StartInfo.Arguments = $"-Command \"{command}\"";
                powerShellProcess.StartInfo.RedirectStandardOutput = true;
                powerShellProcess.StartInfo.RedirectStandardError = true;
                powerShellProcess.StartInfo.UseShellExecute = false;
                powerShellProcess.StartInfo.CreateNoWindow = true;

                powerShellProcess.Start();

                string result = powerShellProcess.StandardOutput.ReadToEnd().Trim();
                string error = powerShellProcess.StandardError.ReadToEnd().Trim();

                powerShellProcess.WaitForExit();

                if (!string.IsNullOrWhiteSpace(error))
                {
                    Logger.LogError($"PowerShell Error: {error}");
                    throw new InvalidOperationException($"PowerShell Error: {error}");
                }
                else
                {
                    //Console.WriteLine("Defender Found!");
                    Console.WriteLine(result);
                    return result.Trim();
                }
            }
        }
        else
        {
            string query = "SELECT displayName FROM AntivirusProduct";
            string namespacePath = @"\\.\root\SecurityCenter2";

            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(namespacePath, query))
            using (ManagementObjectCollection results = searcher.Get())
            {
                foreach (ManagementObject obj in results)
                {
                    string result = obj["displayName"]?.ToString() ?? "Unknown Antivirus";
                    return result;
                }
            }
            
            return "No Active Antivirus Found";
        }
        return "Null";
    }
}