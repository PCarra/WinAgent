using System;
using System.Management;


class DriveEncryptionChecker
{
    public static string GetDriveEncryption()
    {
        try{
            string namespacePath = @"root\CIMv2\Security\MicrosoftVolumeEncryption";
            string query = "SELECT DriveLetter, ProtectionStatus FROM Win32_EncryptableVolume";

            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(namespacePath, query))
            using (ManagementObjectCollection results = searcher.Get())
            {
                foreach (ManagementObject volume in results)
                {
                    string driveLetter = volume["DriveLetter"]?.ToString() ?? "Unknown";
                    uint protectionStatus = (uint)(volume["ProtectionStatus"] ?? 0);

                    string statusMessage = protectionStatus switch
                    {
                        1 => $"{driveLetter}: Encryption is ON.",
                        2 => $"{driveLetter}; Encryption is PAUSED.",
                        _ => $"{driveLetter}: Encryption is OFF."
                    };

                    return statusMessage;
                }
            }

            return "No encryptable volumes found.";
        }
        catch (Exception ex)
        {
            return $"Error checking drive encryption status: {ex.Message}";
        }
    }

    public static string GetDriveEncryptionStatus()
    {
        try
        {
            string namespacePath = @"root\CIMv2\Security\MicrosoftVolumeEncryption";
            string query = "SELECT DriveLetter, ProtectionStatus, EncryptionMethod FROM Win32_EncryptableVolume";

            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher(namespacePath, query))
            using (ManagementObjectCollection results = searcher.Get())
            {
                if (results.Count == 0)
                {
                    return "No encryptable volumes found.";
                }

                string statusReport = "";

                foreach (ManagementObject volume in results)
                {
                    string driveLetter = volume["DriveLetter"]?.ToString() ?? "Unknown";
                    uint protectionStatus = (uint)(volume["ProtectionStatus"] ?? 0);
                    uint encryptionMethod = (uint)(volume["EncryptionMethod"] ?? 0);

                    string encryptionStatus = protectionStatus switch
                    {
                        1 when encryptionMethod != 0 => $"{driveLetter}: Fully Encrypted",
                        1 => $"{driveLetter}: Partially Encrypted",
                        0 => $"{driveLetter}: Not Encrypted",
                        2 => $"{driveLetter}: Encryption Paused",
                        _ => $"{driveLetter}: Unknown Encryption Status"
                    };

                    statusReport += encryptionStatus + Environment.NewLine;
                }

                return statusReport.Trim();
            }
        }
        catch (Exception ex)
        {
            return $"Error retrieving encryption status: {ex.Message}";
        }
    }
}