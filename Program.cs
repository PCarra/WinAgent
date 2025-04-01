using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.ServiceProcess;
using WindowsAgentService.Helpers;
using System.Net.Http.Headers;

namespace WindowsAgent
{
     public class AgentService : ServiceBase
     {
          private Timer timer;
          private const int TimerIntervalInMinutes = 3; // Minutes
          //public static string ClientId = "REPLACE_WITH_CLIENT_ID";
          //public static string ClientSecret = "REPLACE_WITH_CLIENT_SECRET";
          public static string ClientId = "agent123";
          public static string ClientSecret = "supersecret";

          public AgentService()
          {
               InitializeComponent();  // Ensuring components are initialized
          }

          private void InitializeComponent()
          {
               this.ServiceName = "WindowsAgentService";
          }

          protected override void OnStart(string[] args)
          {
               Logger.LogInfo("Starting C# Agent Service...");
               timer = new Timer(async _ => await RunAgent(), null, TimeSpan.Zero, TimeSpan.FromMinutes(TimerIntervalInMinutes));
               Logger.LogInfo("After timer");
          }

          protected override void OnStop()
          {
               timer?.Dispose();
               Logger.LogInfo("C# Agent Service stopped.");
          }

          private async Task RunAgent()
          {
               Logger.LogInfo("In RunAgent");
               try
               {

                    Logger.LogInfo("Collecting system data...");
                    var systemData = await GetSystemData();
                    GetBandwidthTxRx(systemData);

                    var jsonPayload = CreateNestedJsonPayload(systemData);
                    Logger.LogInfo("Data collected:\n" + jsonPayload);

                    string baseApiUrl = "http://portal2.securitybasecamp.com/api";

                    // Step 1: Check if host exists
                    string checkHostUrl = $"{baseApiUrl}/get-host-id/?hostname={systemData.Hostname}";
                    int? hostId = await CheckHostExists(checkHostUrl);

                    if (hostId.HasValue)
                    {
                         // Step 2: Update existing host
                         string updateUrl = $"{baseApiUrl}/update/{hostId}/";
                         await SendDataToAPI(updateUrl, jsonPayload, "PUT");
                         Logger.LogInfo($"Host {systemData.Hostname} updated successfully.");
                    }
                    else
                    {
                         // Step 3: Create new host
                         string createUrl = $"{baseApiUrl}/create/";
                         await SendDataToAPI(createUrl, jsonPayload, "POST");
                         Logger.LogInfo($"Host {systemData.Hostname} created successfully.");
                    }
               }
               catch (Exception ex)
               {
                    Logger.LogInfo($"An error occurred while running the agent: {ex.Message}");
               }
          }

          public static async Task<SystemData> GetSystemData()
          {
               // Get system data asynchronously
               return await Task.FromResult(new SystemData
               {
                    Hostname = Environment.MachineName,
                    IpAddress = HostInfoChecker.GetLocalIPAddress(),
                    OSVersion = Environment.OSVersion.ToString(),
                    Organization = "1",
                    BwRx = "0",
                    BwTx = "0",
                    CpuUsage = HostPerfChecker.GetCpuUsage(),
                    FreeDiskSpace = HostPerfChecker.GetFreeDriveSpace("C"),
                    FreeRam = HostPerfChecker.GetFreeRam(),
                    TotalMemory = HostPerfChecker.GetTotalMemory(),
                    Uptime = HostPerfChecker.GetSystemUptime(),
                    Alert = "False",
                    AvName = AntivirusChecker.GetActiveAntivirusName(),
                    AvSignatureStatus = AntivirusChecker.GetSignatureStatus(),
                    DriveEncryption = DriveEncryptionChecker.GetDriveEncryption(),
                    DriveEncryptionStatus = DriveEncryptionChecker.GetDriveEncryptionStatus(),
                    FirewallName = FirewallChecker.GetFirewallName(),
                    FirewallState = FirewallChecker.GetFirewallState(),
                    LoggedOnAsAdmin = UserSessionChecker.GetLoggedInAdminUsers(),
                    LoggedOnUser = UserSessionChecker.GetLoggedInUsers(),
                    OsAutoUpdateSetting = HostInfoChecker.GetOSAutoUpdateSetting(),
                    OsVersionEdition = HostInfoChecker.GetOSVersionAndEdition(),
                    PasswordComplexity = PasswordPolicyChecker.GetPasswordComplexityRequirements(),
                    PasswordMaxAge = PasswordPolicyChecker.GetMaximumPasswordAge(),
                    PasswordMinLength = PasswordPolicyChecker.GetMinimumPasswordLength(),
                    RdpStatus = RdpChecker.GetRdpStatus(),
                    ScreenLockTimeout = ScreenLockChecker.GetScreenLockTimeout(),
                    WifiSecurity = WiFiInfo.GetWiFiSecurityType(),
                    WifiSsid = WiFiInfo.GetWiFiSSID(),
                    Timestamp = DateTime.UtcNow
               });
          }

          public static void GetBandwidthTxRx(SystemData systemData)
          {
               // Get the network bandwidth data and update systemData object
               var usage = HostPerfChecker.GetBandwidthUsage();
               if (usage == null)
               {
                    Logger.LogInfo("No valid network usage data found.");
                    return;
               }

               systemData.BwRx = usage.Value.BytesReceived.ToString();
               systemData.BwTx = usage.Value.BytesSent.ToString();
          }

          public static string CreateNestedJsonPayload(SystemData systemData)
          {
               var payload = new
               {
                    host = new
                    {
                         hostname = systemData.Hostname,
                         ip_address = systemData.IpAddress,
                         os = systemData.OSVersion,
                         organization = systemData.Organization
                    },
                    system = new
                    {
                         bandwidth_rx = systemData.BwRx,
                         bandwidth_tx = systemData.BwTx,
                         cpu_utilization = systemData.CpuUsage,
                         disk_utilization = systemData.FreeDiskSpace,
                         ram_utilization = systemData.FreeRam,
                         uptime = systemData.Uptime,
                         host = systemData.Hostname
                    },
                    security = new
                    {
                         alert = systemData.Alert,
                         antivirus_name = systemData.AvName,
                         antivirus_signature_status = systemData.AvSignatureStatus,
                         drive_encryption = systemData.DriveEncryption,
                         drive_encryption_status = systemData.DriveEncryptionStatus,
                         firewall_name = systemData.FirewallName,
                         firewall_state = systemData.FirewallState,
                         logged_on_as_admin = systemData.LoggedOnAsAdmin,
                         logged_on_user = systemData.LoggedOnUser,
                         os_auto_update_setting = systemData.OsAutoUpdateSetting,
                         os_update_status = "Up-to-date",
                         os_version_edition = systemData.OsVersionEdition,
                         password_complexity = systemData.PasswordComplexity,
                         password_max_age = systemData.PasswordMaxAge,
                         password_minimum_length = systemData.PasswordMinLength,
                         rdp_status = systemData.RdpStatus,
                         screen_lock_timeout = systemData.ScreenLockTimeout,
                         wifi_security = systemData.WifiSecurity,
                         wifi_ssid = systemData.WifiSsid,
                         host = systemData.Hostname
                    }
               };

               return JsonSerializer.Serialize(payload, new JsonSerializerOptions { WriteIndented = true });
          }


          public static async Task<int?> CheckHostExists(string url)
          {
               try
               {
                    using (var client = new HttpClient())
                    {

                         HttpResponseMessage response = await client.GetAsync(url);
                         if (response.IsSuccessStatusCode)
                         {
                              var jsonResponse = await response.Content.ReadAsStringAsync();
                              var result = JsonSerializer.Deserialize<Dictionary<string, int>>(jsonResponse);
                              return result.ContainsKey("host_id") ? result["host_id"] : (int?)null;
                         }
                         else
                         {
                              Logger.LogError($"Failed to check host. Status Code: {response.StatusCode}");
                              return null;
                         }
                    }
               }
               catch (Exception ex)
               {
                    Logger.LogError($"Error in CheckHostExists: {ex.Message}");
                    return null;
               }
          }

          private static async Task SendDataToAPI(string url, string jsonPayload, string method)
          {
               try
               {

                    using (var client = new HttpClient())
                    {

                         HttpContent content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                         HttpResponseMessage response;

                         if (method == "POST")
                              response = await client.PostAsync(url, content);
                         else if (method == "PUT")
                              response = await client.PutAsync(url, content);
                         else
                              throw new ArgumentException("Invalid HTTP method specified.");

                         string responseContent = await response.Content.ReadAsStringAsync();
                         Logger.LogInfo($"Response Status Code: {response.StatusCode}");
                         Logger.LogInfo($"Response Content:\n{responseContent}");

                         if (!response.IsSuccessStatusCode)
                         {
                              Logger.LogError($"Failed to send data. Status Code: {response.StatusCode}");
                              Logger.LogError($"Response Content: {responseContent}");
                              throw new Exception($"Failed to send data. Status Code: {response.StatusCode}");
                         }
                    }
               }
               catch (Exception ex)
               {
                    Logger.LogError($"Error in SendDataToAPI: {ex.Message}");
               }
          }

          public static void Main(string[] args)
          {
               ServiceBase.Run(new AgentService());
          }
     }

     public class SystemData
     {
          public string Hostname { get; set; }
          public string IpAddress { get; set; }
          public string OSVersion { get; set; }
          public string Organization { get; set; }
          public string BwRx { get; set; }
          public string BwTx { get; set; }
          public string CpuUsage { get; set; }
          public string FreeDiskSpace { get; set; }
          public string FreeRam { get; set; }
          public string TotalMemory { get; set; }
          public string Uptime { get; set; }
          public string Alert { get; set; }
          public string AvName { get; set; }
          public string AvSignatureStatus { get; set; }
          public string DriveEncryption { get; set; }
          public string DriveEncryptionStatus { get; set; }
          public string FirewallName { get; set; }
          public string FirewallState { get; set; }
          public string LoggedOnAsAdmin { get; set; }
          public string LoggedOnUser { get; set; }
          public string OsAutoUpdateSetting { get; set; }
          public string OsVersionEdition { get; set; }
          public string PasswordComplexity { get; set; }
          public string PasswordMaxAge { get; set; }
          public string PasswordMinLength { get; set; }
          public string RdpStatus { get; set; }
          public string ScreenLockTimeout { get; set; }
          public string WifiSecurity { get; set; }
          public string WifiSsid { get; set; }
          public DateTime Timestamp { get; set; }
     }
}
