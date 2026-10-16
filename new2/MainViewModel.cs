using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace SystemGuardian;

public class MainViewModel : INotifyPropertyChanged
{
    private SystemHealthModel _systemHealth = new();
    private string _cleanupSummary = "Ready to scan for temporary system clutter.";

    public event PropertyChangedEventHandler? PropertyChanged;

    public SystemHealthModel SystemHealth
    {
        get => _systemHealth;
        set => SetProperty(ref _systemHealth, value);
    }

    public ObservableCollection<CleanupItem> CleanupItems { get; } = new();

    public ObservableCollection<StartupEntry> StartupEntries { get; } = new();

    public ObservableCollection<DriverStatusItem> DriverStatusItems { get; } = new();

    public ObservableCollection<RepairAction> RepairActions { get; } = new(RepairService.GetRepairActions());

    public string CleanupSummary
    {
        get => _cleanupSummary;
        private set => SetProperty(ref _cleanupSummary, value);
    }

    public void RefreshCleanupSummary()
    {
        var totalSize = CleanupItems.Sum(item => item.SizeBytes);
        var scanWarningCount = CleanupItems.Sum(item => item.ScanWarnings);
        var suffix = scanWarningCount == 0 ? string.Empty : $" Size estimate is partial in {scanWarningCount} location(s).";
        CleanupSummary = $"Detected {CleanupItems.Count} cleanup categories totaling {FormatBytes(totalSize)}.{suffix}";
    }

    private void SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (Equals(field, value))
        {
            return;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
