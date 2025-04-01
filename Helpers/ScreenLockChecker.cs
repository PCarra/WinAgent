using Microsoft.Win32;

class ScreenLockChecker
{
    public static string GetScreenLockTimeout()
    {
        try
        {
            // Registry path where screen saver settings are stored
            string keyPath = @"Control Panel\Desktop";
            using (RegistryKey key = Registry.CurrentUser.OpenSubKey(keyPath))
            {
                if (key != null)
                {
                    // Get the value of ScreenSaveTimeOut
                    object timeoutValue = key.GetValue("ScreenSaveTimeOut");
                    if (timeoutValue != null)
                    {
                        int timeout = (int)timeoutValue;
                        return $"The screen lock timeout is {timeout} seconds.";
                    }
                    else
                    {
                        return "Screen lock timeout setting not found in registry.";
                    }
                }
                else
                {
                    return "Registry key for screen lock timeout not found.";
                }
            }
        }
        catch (Exception ex)
        {
            return $"Error retrieving screen lock timeout: {ex.Message}";
        }
    }
}