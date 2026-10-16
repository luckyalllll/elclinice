using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;

namespace SystemGuardian;

public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;

    public MainWindow()
    {
        InitializeComponent();
        _viewModel = new MainViewModel();
        DataContext = _viewModel;
    }

    private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        await LoadSystemDataAsync("Dashboard loaded. Driver and update status are not assessed.");
    }

    private async void AnalyzeSystem_Click(object sender, RoutedEventArgs e)
    {
        await LoadSystemDataAsync("System details refreshed. Driver and update status are not assessed.");
    }

    private async Task LoadSystemDataAsync(string successMessage)
    {
        SetBusy(true);
        StatusText.Text = "Analyzing system and measuring cleanup candidates...";

        try
        {
            var healthTask = Task.Run(SystemInfoService.GetSystemHealth);
            var cleanupTask = Task.Run(CleanupService.GetRecommendedCleanupItems);
            var startupTask = Task.Run(StartupService.GetStartupEntries);
            var driverTask = Task.Run(SystemInfoService.GetDriverStatus);
            await Task.WhenAll(healthTask, cleanupTask, startupTask, driverTask);

            _viewModel.SystemHealth = await healthTask;
            ReplaceItems(_viewModel.CleanupItems, await cleanupTask);
            ReplaceItems(_viewModel.StartupEntries, await startupTask);
            ReplaceItems(_viewModel.DriverStatusItems, await driverTask);
            _viewModel.RefreshCleanupSummary();
            StatusText.Text = successMessage;
        }
        catch (Exception ex)
        {
            StatusText.Text = $"System analysis failed: {ex.Message}";
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async void FreshCleanup_Click(object sender, RoutedEventArgs e)
    {
        var selected = _viewModel.CleanupItems.Where(item => item.IsSelected).ToList();
        if (selected.Count == 0)
        {
            StatusText.Text = "Select one or more cleanup categories first. Nothing was deleted.";
            return;
        }

        var categoryNames = string.Join(Environment.NewLine, selected.Select(item => $"• {item.Name} ({item.Size})"));
        var confirmation = MessageBox.Show(
            $"This permanently deletes files inside the selected temporary/cache folders. Files in use may be skipped.{Environment.NewLine}{Environment.NewLine}{categoryNames}{Environment.NewLine}{Environment.NewLine}Continue?",
            "Confirm cleanup",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);
        if (confirmation != MessageBoxResult.Yes)
        {
            StatusText.Text = "Cleanup cancelled. No files were deleted.";
            return;
        }

        SetBusy(true);
        StatusText.Text = "Cleaning selected temporary/cache folders...";
        try
        {
            var result = await Task.Run(() => CleanupService.CleanSelectedItems(selected));
            ReplaceItems(_viewModel.CleanupItems, await Task.Run(CleanupService.GetRecommendedCleanupItems));
            _viewModel.RefreshCleanupSummary();
            var outcome = $"Removed {result.RemovedFiles} files ({FormatBytes(result.RemovedBytes)}).";
            StatusText.Text = result.FailedEntries == 0
                ? outcome
                : $"{outcome} {result.FailedEntries} item(s) could not be removed. First affected path: {result.Errors.FirstOrDefault() ?? "path unavailable"}.";
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Cleanup failed: {ex.Message}";
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void RunRepair_Click(object sender, RoutedEventArgs e)
    {
        var repair = RepairListView.SelectedItem as RepairAction;
        if (repair is null)
        {
            StatusText.Text = "Select a repair action first.";
            return;
        }

        var confirmation = MessageBox.Show(
            $"Run {repair.Name}?{Environment.NewLine}{repair.Description}{Environment.NewLine}{Environment.NewLine}This action may request administrator access.",
            "Confirm repair action",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);
        if (confirmation != MessageBoxResult.Yes)
        {
            StatusText.Text = "Repair action cancelled.";
            return;
        }

        try
        {
            RepairService.RunRepair(repair);
            StatusText.Text = $"Started: {repair.Name}. A User Account Control prompt may appear for elevated access.";
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Repair launch failed: {ex.Message}";
        }
    }

    private void OpenDeviceManager_Click(object sender, RoutedEventArgs e)
    {
        OpenSystemTarget("devmgmt.msc", "Device Manager");
    }

    private void OpenWindowsUpdate_Click(object sender, RoutedEventArgs e)
    {
        OpenSystemTarget("ms-settings:windowsupdate", "Windows Update settings");
    }

    private void OpenStartupSettings_Click(object sender, RoutedEventArgs e)
    {
        OpenSystemTarget("ms-settings:startupapps", "Startup apps settings");
    }

    private void OpenSystemTarget(string target, string description)
    {
        try
        {
            Process.Start(new ProcessStartInfo(target) { UseShellExecute = true });
            StatusText.Text = $"Opened {description}. Review its status there; System Guardian does not install drivers or updates.";
        }
        catch (Exception ex)
        {
            StatusText.Text = $"Could not open {description}: {ex.Message}";
        }
    }

    private static void ReplaceItems<T>(ObservableCollection<T> destination, IEnumerable<T> source)
    {
        destination.Clear();
        foreach (var item in source)
        {
            destination.Add(item);
        }
    }

    private void SetBusy(bool isBusy)
    {
        AnalyzeButton.IsEnabled = !isBusy;
        CleanButton.IsEnabled = !isBusy;
        RepairButton.IsEnabled = !isBusy;
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
