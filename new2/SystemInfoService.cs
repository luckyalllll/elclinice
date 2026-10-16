using Microsoft.VisualBasic.Devices;

namespace SystemGuardian;

public static class SystemInfoService
{
    public static SystemHealthModel GetSystemHealth()
    {
        var computerInfo = new ComputerInfo();
        var totalMemory = computerInfo.TotalPhysicalMemory;
        var availableMemory = computerInfo.AvailablePhysicalMemory;
        var usedMemory = totalMemory - availableMemory;

        var drive = DriveInfo.GetDrives().FirstOrDefault(d => d.IsReady && d.DriveType == DriveType.Fixed)
            ?? DriveInfo.GetDrives().FirstOrDefault(d => d.IsReady);

        var uptime = TimeSpan.FromMilliseconds(Environment.TickCount64);
        var osName = GetWindowsVersion();

        return new SystemHealthModel
        {
            DeviceName = Environment.MachineName,
            UserName = Environment.UserName,
            WindowsVersion = osName,
            CpuName = GetCpuName(),
            TotalMemory = FormatBytes(totalMemory),
            UsedMemory = FormatBytes(usedMemory),
            MemorySummary = $"{FormatBytes(usedMemory)} used / {FormatBytes(totalMemory)} total",
            AvailableDisk = drive is null ? "Unknown" : $"{FormatBytes(drive.AvailableFreeSpace)} free",
            Uptime = FormatDuration(uptime),
            DriverStatus = "Not checked",
            UpdateStatus = "Review in Settings",
            HealthSummary = "System details are informational. Driver health and Windows Update status have not been checked by this app."
        };
    }

    public static List<DriverStatusItem> GetDriverStatus()
    {
        return new List<DriverStatusItem>
        {
            new() { Name = "Device Manager", Status = "Not checked", Details = "Open Device Manager to review device status and driver warnings." }
        };
    }

    private static string GetCpuName()
    {
        using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"Hardware\Description\System\CentralProcessor\0");
        return key?.GetValue("ProcessorNameString") as string ?? "Unknown CPU";
    }

    private static string GetWindowsVersion()
    {
        using var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows NT\CurrentVersion");
        var productName = key?.GetValue("ProductName") as string;
        var displayVersion = key?.GetValue("DisplayVersion") as string;
        var build = key?.GetValue("CurrentBuildNumber") as string;

        if (string.IsNullOrWhiteSpace(productName))
        {
            return Environment.OSVersion.VersionString;
        }

        var version = string.Join(" ", new[] { productName, displayVersion }.Where(value => !string.IsNullOrWhiteSpace(value)));
        return string.IsNullOrWhiteSpace(build) ? version : $"{version} (Build {build})";
    }

    private static string FormatDuration(TimeSpan timeSpan)
    {
        if (timeSpan.TotalDays >= 1)
        {
            return $"{(int)timeSpan.TotalDays}d {timeSpan.Hours}h {timeSpan.Minutes}m";
        }

        if (timeSpan.TotalHours >= 1)
        {
            return $"{(int)timeSpan.TotalHours}h {timeSpan.Minutes}m";
        }

        return $"{timeSpan.Minutes}m {timeSpan.Seconds}s";
    }

    private static string FormatBytes(long bytes)
    {
        var units = new[] { "B", "KB", "MB", "GB", "TB" };
        double size = bytes;
        var unitIndex = 0;

        while (size >= 1024 && unitIndex < units.Length - 1)
        {
            size /= 1024;
            unitIndex++;
        }

        return $"{size:0.##} {units[unitIndex]}";
    }
}
