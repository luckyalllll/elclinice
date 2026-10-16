using System.Diagnostics;
using System.IO;

namespace SystemGuardian;

public static class RepairService
{
    public static List<RepairAction> GetRepairActions()
    {
        return new List<RepairAction>
        {
            new() { Name = "System File Check", Description = "Scan and repair protected Windows files.", Command = "sfc.exe", Arguments = "/scannow" },
            new() { Name = "DISM Repair", Description = "Restore the Windows image health.", Command = "dism.exe", Arguments = "/Online /Cleanup-Image /RestoreHealth" },
            new() { Name = "Flush DNS", Description = "Clear stale DNS cache entries.", Command = "ipconfig.exe", Arguments = "/flushdns" },
            new() { Name = "Reset Winsock", Description = "Reset network socket configuration. A restart may be required.", Command = "netsh.exe", Arguments = "winsock reset" }
        };
    }

    public static void RunRepair(RepairAction repair)
    {
        ArgumentNullException.ThrowIfNull(repair);
        var startInfo = new ProcessStartInfo
        {
            FileName = Path.Combine(Environment.SystemDirectory, repair.Command),
            Arguments = repair.Arguments,
            UseShellExecute = true,
            Verb = "runas"
        };

        if (Process.Start(startInfo) is null)
        {
            throw new InvalidOperationException($"Windows did not start the repair process '{repair.Command}'.");
        }
    }
}
