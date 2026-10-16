$script:SystemGuardianProject = Join-Path $PSScriptRoot 'SystemGuardian.csproj'

function Test-SystemGuardianProject {
    if (-not (Test-Path -LiteralPath $script:SystemGuardianProject -PathType Leaf)) {
        throw "System Guardian project file was not found: $script:SystemGuardianProject"
    }
}

function Invoke-SystemGuardianDotNet {
    param(
        [Parameter(Mandatory)]
        [string[]] $Arguments
    )

    Test-SystemGuardianProject
    & dotnet @Arguments
    if ($LASTEXITCODE -ne 0) {
        throw "dotnet command failed with exit code $LASTEXITCODE."
    }
}

function Get-ProjectShortcuts {
    [pscustomobject]@{
        Project     = 'System Guardian'
        Shortcut    = 'sg-restore'
        Description = 'Restore NuGet dependencies.'
    }
    [pscustomobject]@{
        Project     = 'System Guardian'
        Shortcut    = 'sg-build'
        Description = 'Build the WPF app in Release configuration.'
    }
    [pscustomobject]@{
        Project     = 'System Guardian'
        Shortcut    = 'sg-run'
        Description = 'Build and run the WPF app.'
    }
    [pscustomobject]@{
        Project     = 'System Guardian'
        Shortcut    = 'sg-publish'
        Description = 'Publish a self-contained Windows x64 app.'
    }
    [pscustomobject]@{
        Project     = 'System Guardian'
        Shortcut    = 'sg-open'
        Description = 'Open the project folder in File Explorer.'
    }
}

function Invoke-SystemGuardianRestore {
    Invoke-SystemGuardianDotNet -Arguments @('restore', $script:SystemGuardianProject)
}

function Invoke-SystemGuardianBuild {
    Invoke-SystemGuardianDotNet -Arguments @('build', $script:SystemGuardianProject, '--configuration', 'Release')
}

function Invoke-SystemGuardianRun {
    Invoke-SystemGuardianDotNet -Arguments @('run', '--project', $script:SystemGuardianProject)
}

function Publish-SystemGuardian {
    Invoke-SystemGuardianDotNet -Arguments @(
        'publish',
        $script:SystemGuardianProject,
        '--configuration', 'Release',
        '--runtime', 'win-x64',
        '--self-contained', 'true'
    )
}

function Open-SystemGuardianProject {
    Test-SystemGuardianProject
    Start-Process -FilePath (Split-Path -Parent $script:SystemGuardianProject)
}

Set-Alias -Name sg-restore -Value Invoke-SystemGuardianRestore
Set-Alias -Name sg-build -Value Invoke-SystemGuardianBuild
Set-Alias -Name sg-run -Value Invoke-SystemGuardianRun
Set-Alias -Name sg-publish -Value Publish-SystemGuardian
Set-Alias -Name sg-open -Value Open-SystemGuardianProject

Export-ModuleMember -Function @(
    'Get-ProjectShortcuts',
    'Invoke-SystemGuardianRestore',
    'Invoke-SystemGuardianBuild',
    'Invoke-SystemGuardianRun',
    'Publish-SystemGuardian',
    'Open-SystemGuardianProject'
) -Alias @('sg-restore', 'sg-build', 'sg-run', 'sg-publish', 'sg-open')
