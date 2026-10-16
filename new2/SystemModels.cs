using System.ComponentModel;

namespace SystemGuardian;

public class SystemHealthModel
{
    public string DeviceName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string WindowsVersion { get; set; } = string.Empty;
    public string CpuName { get; set; } = string.Empty;
    public string TotalMemory { get; set; } = string.Empty;
    public string UsedMemory { get; set; } = string.Empty;
    public string MemorySummary { get; set; } = string.Empty;
    public string AvailableDisk { get; set; } = string.Empty;
    public string Uptime { get; set; } = string.Empty;
    public string HealthSummary { get; set; } = string.Empty;
    public string DriverStatus { get; set; } = string.Empty;
    public string UpdateStatus { get; set; } = string.Empty;
}

public class DriverStatusItem
{
    public string Name { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Details { get; set; } = string.Empty;
}

public class StartupEntry
{
    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string Impact { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
}

public class CleanupItem : INotifyPropertyChanged
{
    private bool _isSelected;

    public event PropertyChangedEventHandler? PropertyChanged;

    public string Name { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
    public string Size { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public int ScanWarnings { get; set; }

    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected == value)
            {
                return;
            }

            _isSelected = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsSelected)));
        }
    }
}

public class CleanupResult
{
    public long RemovedBytes { get; set; }
    public int RemovedFiles { get; set; }
    public int FailedEntries { get; set; }
    public List<string> Errors { get; } = new();
}

public class RepairAction
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Command { get; set; } = string.Empty;
    public string Arguments { get; set; } = string.Empty;
}
