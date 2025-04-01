using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Runtime.InteropServices;

public class UserSessionChecker
{
    // P/Invoke declarations
    [DllImport("wtsapi32.dll", SetLastError = true)]
    private static extern bool WTSQuerySessionInformation(
        IntPtr hServer,
        int sessionId,
        WTSInfoClass wtsInfoClass,
        out IntPtr ppBuffer,
        out uint pBytesReturned);

    [DllImport("wtsapi32.dll", SetLastError = true)]
    private static extern void WTSFreeMemory(IntPtr pMemory);

    [DllImport("wtsapi32.dll", SetLastError = true)]
    private static extern IntPtr WTSOpenServer(string pServerName);

    [DllImport("wtsapi32.dll", SetLastError = true)]
    private static extern void WTSCloseServer(IntPtr hServer);

    [DllImport("wtsapi32.dll", SetLastError = true)]
    private static extern bool WTSEnumerateSessions(
        IntPtr hServer,
        int reserved,
        int version,
        out IntPtr ppSessionInfo,
        out int pCount);

    [StructLayout(LayoutKind.Sequential)]
    private struct WTS_SESSION_INFO
    {
        public int SessionId;
        public string pWinStationName;
        public WTS_CONNECTSTATE_CLASS State;
    }

    private enum WTS_CONNECTSTATE_CLASS
    {
        Active,
        Connected,
        ConnectQuery,
        Shadow,
        Disconnected,
        Idle,
        Listen,
        Reset,
        Down,
        Init
    }

    private enum WTSInfoClass
    {
        WTSUserName = 5,
        WTSDomainName = 7
    }

    // Method to enumerate logged-in users
    public static string GetLoggedInUsers()
    {
        List<string> users = new List<string>();
        IntPtr serverHandle = WTSOpenServer(Environment.MachineName);
        IntPtr sessionInfoPtr = IntPtr.Zero;
        int sessionCount = 0;

        try
        {
            if (WTSEnumerateSessions(serverHandle, 0, 1, out sessionInfoPtr, out sessionCount))
            {
                int dataSize = Marshal.SizeOf(typeof(WTS_SESSION_INFO));
                IntPtr currentSession = sessionInfoPtr;

                for (int i = 0; i < sessionCount; i++)
                {
                    WTS_SESSION_INFO sessionInfo = Marshal.PtrToStructure<WTS_SESSION_INFO>(currentSession);

                    IntPtr buffer;
                    uint bytesReturned;

                    if (sessionInfo.State == WTS_CONNECTSTATE_CLASS.Active)
                    {
                        if (WTSQuerySessionInformation(serverHandle, sessionInfo.SessionId, WTSInfoClass.WTSUserName, out buffer, out bytesReturned))
                        {
                            string userName = Marshal.PtrToStringAnsi(buffer);
                            if (!string.IsNullOrWhiteSpace(userName))
                            {
                                users.Add(userName);
                            }
                            WTSFreeMemory(buffer);
                        }
                    }

                    currentSession = IntPtr.Add(currentSession, dataSize);
                }
            }
        }
        finally
        {
            WTSFreeMemory(sessionInfoPtr);
            WTSCloseServer(serverHandle);
        }

        return users.Count > 0 ? string.Join(", ", users) : "No users are currently logged in.";
    }

    // Method to check if a user is an admin
    public static string GetLoggedInAdminUsers()
    {
        List<string> adminUsers = new List<string>();
        IntPtr serverHandle = WTSOpenServer(Environment.MachineName);
        IntPtr sessionInfoPtr = IntPtr.Zero;
        int sessionCount = 0;

        try
        {
            if (WTSEnumerateSessions(serverHandle, 0, 1, out sessionInfoPtr, out sessionCount))
            {
                int dataSize = Marshal.SizeOf(typeof(WTS_SESSION_INFO));
                IntPtr currentSession = sessionInfoPtr;

                for (int i = 0; i < sessionCount; i++)
                {
                    WTS_SESSION_INFO sessionInfo = Marshal.PtrToStructure<WTS_SESSION_INFO>(currentSession);

                    if (sessionInfo.State == WTS_CONNECTSTATE_CLASS.Active)
                    {
                        IntPtr buffer;
                        uint bytesReturned;

                        if (WTSQuerySessionInformation(serverHandle, sessionInfo.SessionId, WTSInfoClass.WTSUserName, out buffer, out bytesReturned))
                        {
                            string userName = Marshal.PtrToStringAnsi(buffer);
                            if (!string.IsNullOrWhiteSpace(userName) && IsUserAdmin(userName))
                            {
                                adminUsers.Add(userName);
                            }
                            WTSFreeMemory(buffer);
                        }
                    }

                    currentSession = IntPtr.Add(currentSession, dataSize);
                }
            }
        }
        finally
        {
            WTSFreeMemory(sessionInfoPtr);
            WTSCloseServer(serverHandle);
        }

        // Return the logged-in admin users as a single string or a fallback message
        return adminUsers.Count > 0 ? string.Join(", ", adminUsers) : "No administrators are currently logged in.";
    }
    private static bool IsUserAdmin(string userName)
    {
        using (PrincipalContext context = new PrincipalContext(ContextType.Machine))
        {
            GroupPrincipal administratorsGroup = GroupPrincipal.FindByIdentity(context, "Administrators");
            if (administratorsGroup != null)
            {
                foreach (Principal member in administratorsGroup.Members)
                {
                    if (member is UserPrincipal userPrincipal && userPrincipal.SamAccountName.Equals(userName, StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }
}
