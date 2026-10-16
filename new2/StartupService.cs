namespace SystemGuardian;

public static class StartupService
{
    public static List<StartupEntry> GetStartupEntries()
    {
        var entries = new List<StartupEntry>();
        ReadRunKey(Microsoft.Win32.Registry.CurrentUser, @"Software\Microsoft\Windows\CurrentVersion\Run", "Current user", entries);
        ReadRunKey(Microsoft.Win32.Registry.LocalMachine, @"Software\Microsoft\Windows\CurrentVersion\Run", "All users", entries);
        ReadStartupFolder(Environment.GetFolderPath(Environment.SpecialFolder.Startup), "Current user startup folder", entries);
        ReadStartupFolder(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartup), "All users startup folder", entries);
        return entries;
    }

    private static void ReadRunKey(
        Microsoft.Win32.RegistryKey hive,
        string subKey,
        string source,
        ICollection<StartupEntry> entries)
    {
        using var key = hive.OpenSubKey(subKey);
        if (key is null)
        {
            return;
        }

        foreach (var name in key.GetValueNames())
        {
            var value = key.GetValue(name)?.ToString();
            if (!string.IsNullOrWhiteSpace(value))
            {
                entries.Add(new StartupEntry
                {
                    Name = name,
                    Path = value,
                    Impact = "Unknown",
                    Source = source
                });
            }
        }
    }

    private static void ReadStartupFolder(string folder, string source, ICollection<StartupEntry> entries)
    {
        if (!Directory.Exists(folder))
        {
            return;
        }

        foreach (var file in Directory.EnumerateFiles(folder))
        {
            entries.Add(new StartupEntry
            {
                Name = Path.GetFileNameWithoutExtension(file),
                Path = file,
                Impact = "Unknown",
                Source = source
            });
        }
    }
}
