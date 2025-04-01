using System;
using System.Diagnostics;
using System.Management;
using System.Net.NetworkInformation;

public class HostPerfChecker
{
    public struct NetworkUsage
    {
        public string BytesSent { get; set; }
        public string BytesReceived { get; set; }
    }
    public static NetworkUsage? GetBandwidthUsage()
    {
        try
        {
            // Get all network interfaces
            var networkInterfaces = NetworkInterface.GetAllNetworkInterfaces();
            NetworkInterface primaryInterface = null;

            foreach (var networkInterface in networkInterfaces)
            {
                // Log the interface being checked
                //Console.WriteLine($"Checking Interface: {networkInterface.Name}");
                //Console.WriteLine($" - Type: {networkInterface.NetworkInterfaceType}");
                //Console.WriteLine($" - Status: {networkInterface.OperationalStatus}");
                //Console.WriteLine($" - Description: {networkInterface.Description}");

                // Skip interfaces that are not operational, or are loopback/tunnel
                if (networkInterface.OperationalStatus != OperationalStatus.Up)
                {
                    //Console.WriteLine(" - Skipped: Interface is not operational.");
                    continue;
                }

                if (networkInterface.NetworkInterfaceType == NetworkInterfaceType.Loopback ||
                    networkInterface.NetworkInterfaceType == NetworkInterfaceType.Tunnel)
                {
                    //Console.WriteLine(" - Skipped: Interface is loopback or tunnel.");
                    continue;
                }

                // Get IP properties safely
                var ipProperties = networkInterface.GetIPProperties();
                if (ipProperties?.GatewayAddresses != null &&
                    ipProperties.GatewayAddresses.Any(g => g.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork))
                {
                    //Console.WriteLine($" - Selected: Interface has a valid gateway.");
                    primaryInterface = networkInterface;

                    // Prefer Wi-Fi interfaces
                    if (networkInterface.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                    {
                        //Console.WriteLine(" - Priority: This is a Wi-Fi interface.");
                        break;
                    }
                }
                else
                {
                    //Console.WriteLine(" - Skipped: No valid gateway found.");
                    continue;
                }
            }

            if (primaryInterface == null)
            {
                //Console.WriteLine("No active network interface with a default gateway found.");
                return null;
            }

            // Display primary interface details
            //Console.WriteLine($"Primary Interface: {primaryInterface.Name}");
            //Console.WriteLine($"Description: {primaryInterface.Description}");
            //Console.WriteLine($"Interface Type: {primaryInterface.NetworkInterfaceType}");
            //Console.WriteLine($"Speed: {primaryInterface.Speed / 1_000_000} Mbps");

            // Get statistics
            var stats = primaryInterface.GetIPStatistics();
            //Console.WriteLine("Data Usage Statistics:");
            //Console.WriteLine($"Bytes Sent: {stats.BytesSent}");
            //Console.WriteLine($"Bytes Received: {stats.BytesReceived}");
            return new NetworkUsage
            {
                BytesSent = stats.BytesSent.ToString(),
                BytesReceived = stats.BytesReceived.ToString()
            };
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            return null;
        }
    }

    // Function to get free Ram
    public static string GetFreeRam()
    {
        var availableMemoryCounter = new PerformanceCounter("Memory", "Available MBytes");
        float availableMemory = availableMemoryCounter.NextValue();
        //returned in MB
        return string.Format("{0}", availableMemory);
    }

    // Function to get total Ram
    public static string GetTotalMemory()
    {
        try
        {
            using (var searcher = new ManagementObjectSearcher("SELECT TotalVisibleMemorySize FROM Win32_OperatingSystem"))
            {
                foreach (ManagementObject obj in searcher.Get())
                {
                    //value in MB
                    return (Convert.ToInt64(obj["TotalVisibleMemorySize"]) / 1024).ToString() + " MB";
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Memory Error: {ex.Message}");
        }
        return "Unknown";
    }

    // Function to get the total disk space for specified drive
    public static string GetFreeDriveSpace(string driveLetter)
    {
        if (!driveLetter.EndsWith(":"))
        {
            driveLetter += ":";
        }

        DriveInfo drive = new DriveInfo(driveLetter);

        if (!drive.IsReady)
        {
            throw new InvalidOperationException($"The drive {driveLetter} is not ready.");
        }

        long freeSpace = drive.TotalFreeSpace;
        //returned in Gigabytes
        return $"{freeSpace / (1024.0 * 1024.0 * 1024.0):F2}";
    }

    // Retrieve current CPU Usage
    public static string GetCpuUsage()
    {
        try
        {
            using (PerformanceCounter cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total"))
            {
                cpuCounter.NextValue(); // First read is always 0
                System.Threading.Thread.Sleep(1000); // Wait for accurate reading
               
                return cpuCounter.NextValue().ToString("0.00");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"CPU Error: {ex.Message}");
        }
        return "Unknown";
    }

    //Retrieve the system uptime
    public static string GetSystemUptime()
    {
        TimeSpan uptime = TimeSpan.FromMilliseconds(Environment.TickCount64);
        return $"{uptime.Days} {uptime.Hours}:{uptime.Minutes}:{uptime.Seconds}";
    }
}