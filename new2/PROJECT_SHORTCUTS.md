# Project shortcuts

This workspace includes a small PowerShell module for common System Guardian
development tasks. From the workspace folder, import it once per PowerShell
session:

```powershell
Import-Module .\ProjectShortcuts.psm1
```

Then use the short commands:

| Shortcut | Action |
| --- | --- |
| `sg-restore` | Restore NuGet dependencies |
| `sg-build` | Build the app in Release configuration |
| `sg-run` | Run the WPF app |
| `sg-publish` | Publish a self-contained Windows x64 build |
| `sg-open` | Open the project folder in File Explorer |

To see the catalog in PowerShell, run `Get-ProjectShortcuts`. The module
resolves `SystemGuardian.csproj` relative to its own location, so the commands
work even when the current directory changes after import.
