using System;
using System.Management;
using Microsoft.Win32;

public class PasswordPolicyChecker
{
    public static string GetPasswordComplexityRequirements()
    {
        try
        {
            // Open the registry key where password policy is stored
            string keyPath = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\PasswordPolicy";
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(keyPath))
            {
                if (key != null)
                {
                    object passwordComplexity = key.GetValue("PasswordComplexity");
                    if (passwordComplexity != null)
                    {
                        // Check if password complexity is enabled (1 = enabled, 0 = disabled)
                        bool isComplexityEnabled = (int)passwordComplexity == 1;
                        return isComplexityEnabled ? "Password complexity is enabled." : "Password complexity is not required.";
                    }
                    else
                    {
                        return "Password complexity setting not found in registry.";
                    }
                }
                else
                {
                    return "Password policy registry key not found.";
                }
            }
        }
        catch (Exception ex)
        {
            return $"Error retrieving password complexity requirements: {ex.Message}";
        }
    }

    public static string GetMaximumPasswordAge()
    {
        try
        {
            // Open the registry key where password policy is stored
            string keyPath = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\PasswordPolicy";
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(keyPath))
            {
                if (key != null)
                {
                    object maxPasswordAge = key.GetValue("MaximumPasswordAge");
                    if (maxPasswordAge != null)
                    {
                        int maxAgeDays = (int)maxPasswordAge;
                        return maxAgeDays == 0
                            ? "No maximum password age is set."
                            : $"The maximum password age is {maxAgeDays} days.";
                    }
                    else
                    {
                        return "Maximum password age not found in registry.";
                    }
                }
                else
                {
                    return "Password policy registry key not found.";
                }
            }
        }
        catch (Exception ex)
        {
            return $"Error retrieving maximum password age: {ex.Message}";
        }
    }

     public static string GetMinimumPasswordLength()
    {
        try
        {
            // Open the registry key where password policy is stored
            string keyPath = @"SOFTWARE\Microsoft\Windows NT\CurrentVersion\PasswordPolicy";
            using (RegistryKey key = Registry.LocalMachine.OpenSubKey(keyPath))
            {
                if (key != null)
                {
                    object minPasswordLength = key.GetValue("MinimumPasswordLength");
                    if (minPasswordLength != null)
                    {
                        int minLength = (int)minPasswordLength;
                        return $"The minimum password length is {minLength} characters.";
                    }
                    else
                    {
                        return "Minimum password length setting not found in registry.";
                    }
                }
                else
                {
                    return "Password policy registry key not found.";
                }
            }
        }
        catch (Exception ex)
        {
            return $"Error retrieving minimum password length: {ex.Message}";
        }
    }
}
