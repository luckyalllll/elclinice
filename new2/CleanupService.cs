using System.IO;

namespace SystemGuardian;

public static class CleanupService
{
    public static List<CleanupItem> GetRecommendedCleanupItems()
    {
        var items = new List<CleanupItem>
        {
            CreateItem("Temp Files", Path.GetTempPath()),
            CreateItem("Internet Cache", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "Windows", "INetCache"))
        };

        return items.Where(item => !string.IsNullOrWhiteSpace(item.Path) && Directory.Exists(item.Path)).ToList();
    }

    public static CleanupResult CleanSelectedItems(IEnumerable<CleanupItem> items)
    {
        var result = new CleanupResult();

        foreach (var item in items)
        {
            if (!item.IsSelected || !IsAllowedPath(item.Path))
            {
                continue;
            }

            try
            {
                var directory = new DirectoryInfo(item.Path);
                if ((directory.Attributes & FileAttributes.ReparsePoint) != 0)
                {
                    AddError(result, item.Path);
                    continue;
                }

                foreach (var entry in directory.EnumerateFileSystemInfos())
                {
                    DeleteEntry(entry, result);
                }
            }
            catch (IOException)
            {
                AddError(result, item.Path);
            }
            catch (UnauthorizedAccessException)
            {
                AddError(result, item.Path);
            }
        }

        return result;
    }

    private static void DeleteEntry(FileSystemInfo entry, CleanupResult result)
    {
        var pending = new Stack<(FileSystemInfo Entry, bool Visited)>();
        pending.Push((entry, false));

        while (pending.Count > 0)
        {
            var (current, visited) = pending.Pop();
            try
            {
                if ((current.Attributes & FileAttributes.ReparsePoint) != 0)
                {
                    current.Delete();
                }
                else if (current is DirectoryInfo directory)
                {
                    if (visited)
                    {
                        directory.Delete();
                    }
                    else
                    {
                        pending.Push((directory, true));
                        foreach (var child in directory.EnumerateFileSystemInfos())
                        {
                            pending.Push((child, false));
                        }
                    }
                }
                else if (current is FileInfo file)
                {
                    var length = file.Length;
                    file.Delete();
                    result.RemovedBytes += length;
                    result.RemovedFiles++;
                }
            }
            catch (IOException)
            {
                AddError(result, current.FullName);
            }
            catch (UnauthorizedAccessException)
            {
                AddError(result, current.FullName);
            }
        }
    }

    private static CleanupItem CreateItem(string name, string path)
    {
        var size = 0L;
        var scanWarnings = 0;
        if (Directory.Exists(path))
        {
            size = GetDirectorySize(new DirectoryInfo(path), out scanWarnings);
        }

        return new CleanupItem
        {
            Name = name,
            Path = path,
            Size = scanWarnings > 0 ? $"~{FormatBytes(size)} (partial)" : FormatBytes(size),
            SizeBytes = size,
            ScanWarnings = scanWarnings,
            IsSelected = false
        };
    }

    private static long GetDirectorySize(DirectoryInfo directory, out int scanWarnings)
    {
        long size = 0;
        scanWarnings = 0;
        var pending = new Stack<DirectoryInfo>();
        pending.Push(directory);

        while (pending.Count > 0)
        {
            var current = pending.Pop();
            try
            {
                foreach (var entry in current.EnumerateFileSystemInfos())
                {
                    try
                    {
                        if ((entry.Attributes & FileAttributes.ReparsePoint) != 0)
                        {
                            continue;
                        }

                        if (entry is DirectoryInfo childDirectory)
                        {
                            pending.Push(childDirectory);
                        }
                        else if (entry is FileInfo file)
                        {
                            size += file.Length;
                        }
                    }
                    catch (IOException)
                    {
                        scanWarnings++;
                    }
                    catch (UnauthorizedAccessException)
                    {
                        scanWarnings++;
                    }
                }
            }
            catch (IOException)
            {
                scanWarnings++;
            }
            catch (UnauthorizedAccessException)
            {
                scanWarnings++;
            }
        }

        return size;
    }

    private static bool IsAllowedPath(string path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        var allowedPaths = new[]
        {
            Path.GetTempPath(),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Microsoft", "Windows", "INetCache")
        };

        try
        {
            return allowedPaths.Any(allowed =>
                string.Equals(
                    Path.GetFullPath(path).TrimEnd(Path.DirectorySeparatorChar),
                    Path.GetFullPath(allowed).TrimEnd(Path.DirectorySeparatorChar),
                    StringComparison.OrdinalIgnoreCase));
        }
        catch (ArgumentException)
        {
            return false;
        }
        catch (NotSupportedException)
        {
            return false;
        }
        catch (PathTooLongException)
        {
            return false;
        }
    }

    private static void AddError(CleanupResult result, string path)
    {
        result.FailedEntries++;
        if (result.Errors.Count < 5)
        {
            result.Errors.Add(path);
        }
    }

    private static string FormatBytes(long bytes)
    {
        var units = new[] { "B", "KB", "MB", "GB", "TB" };
        double size = bytes;
        int unitIndex = 0;

        while (size >= 1024 && unitIndex < units.Length - 1)
        {
            size /= 1024;
            unitIndex++;
        }

        return $"{size:0.##} {units[unitIndex]}";
    }
}
