using System;
using System.Management;
using System.ServiceProcess;
using System.ComponentModel;

public class FirewallChecker
{

     [Flags]
     public enum ProductState
     {
          Off = 0x0000,
          On = 0x1000,
          Snoozed = 0x2000,
          Expired = 0x3000
     }

     [Flags]
     public enum SignatureStatus
     {
          UpToDate = 0x00,
          OutOfDate = 0x10
     }

     [Flags]
     public enum ProductFlags
     {
          ProductState = 0xF000
     }

     public class ServiceControllerException : Exception
     {
          public ServiceControllerException() { }

          public ServiceControllerException(string message)
              : base(message) { }

          public ServiceControllerException(string message, Exception innerException)
              : base(message, innerException) { }
     }
     // Function to get the Firewall Name
     public static string GetFirewallName()
     {
          try
          {
               // Query the WMI namespace for Firewall Products
               string query = "SELECT * FROM FirewallProduct";
               ManagementObjectSearcher searcher = new ManagementObjectSearcher(@"\\.\root\SecurityCenter2", query);

               foreach (ManagementObject obj in searcher.Get())
               {
                    // If a Firewall Product is found, return its name
                    return obj["displayName"]?.ToString() ?? "Unknown Firewall";
               }

               // If no products are found, check Windows Firewall service
               return "Windows Firewall";
          }
          catch
          {
               return "Error retrieving Firewall Name";
          }
     }

     // Function to get the Firewall State
     public static string GetFirewallState()
     {
          try
          {
               // Attempt to get the Firewall state using WMI
               var searcher = new ManagementObjectSearcher("root\\SecurityCenter2", "SELECT * FROM FirewallProduct");
               var infos = searcher.Get();

               if (infos.Count == 0)
               {
                    // Fallback to checking the Windows Firewall service state
                    using (ServiceController firewallService = new ServiceController("mpssvc"))
                    {
                         if (firewallService.Status == ServiceControllerStatus.Running)
                         {
                              return "On (Windows Firewall)";
                         }
                         else
                         {
                              return "Off (Windows Firewall)";
                         }
                    }
               }
               else
               {
                    // Iterate through FirewallProduct objects
                    foreach (ManagementObject info in infos)
                    {
                         uint state = (uint)info["productState"];
                         ProductState productState = (ProductState)(state & (uint)ProductFlags.ProductState);

                         if (productState == ProductState.On)
                         {
                              return $"{info["displayName"]} (On)";
                         }
                    }

                    return "Off (Third-party Firewall)";
               }
          }
          catch (ManagementException ex)
          {
               LogError("WMI ManagementException occurred while retrieving the firewall state.", ex);
               return "Error retrieving Firewall State: ManagementException";
          }
          catch (ServiceControllerException ex)
          {
               LogError("Error occurred while checking Windows Firewall service state.", ex);
               return "Error retrieving Firewall State: ServiceControllerException";
          }
          catch (UnauthorizedAccessException ex)
          {
               LogError("UnauthorizedAccessException: Insufficient privileges to query firewall state.", ex);
               return "Error retrieving Firewall State: Insufficient privileges";
          }
          catch (Exception ex)
          {
               LogError("An unexpected error occurred while retrieving the firewall state.", ex);
               return "Error retrieving Firewall State: Unexpected error";
          }
     }

     /// <summary>
     /// Logs error details for debugging.
     /// </summary>
     /// <param name="message">Custom error message.</param>
     /// <param name="exception">The exception to log.</param>
     private static void LogError(string message, Exception exception)
     {
          // Here you can use a logging framework like NLog, Serilog, etc., or a simple Console.WriteLine
          Console.WriteLine($"{DateTime.Now}: {message}");
          Console.WriteLine($"Exception Type: {exception.GetType().Name}");
          Console.WriteLine($"Message: {exception.Message}");
          Console.WriteLine($"Stack Trace: {exception.StackTrace}");
     }


}
