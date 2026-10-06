[CmdletBinding()]
param(
    [switch]$BuildInstaller,
    # Used by the Installer publish profile after the SDK has published the app.
    [switch]$SkipAppPublish
)

$ErrorActionPreference = 'Stop'
$repositoryRoot = Split-Path $PSScriptRoot -Parent
$publishDirectory = [IO.Path]::GetFullPath((Join-Path $repositoryRoot 'artifacts/publish/win-x64'))
$expectedRoot = [IO.Path]::GetFullPath((Join-Path $repositoryRoot 'artifacts')) + [IO.Path]::DirectorySeparatorChar
if (-not $publishDirectory.StartsWith($expectedRoot, [StringComparison]::OrdinalIgnoreCase)) {
    throw "Publish directory is outside artifacts: $publishDirectory"
}

if ($SkipAppPublish -and -not (Test-Path -LiteralPath (Join-Path $publishDirectory 'AudioSwitch.exe'))) {
    throw 'The application must be published before completing the installer payload.'
}

if (-not $SkipAppPublish -and (Test-Path -LiteralPath $publishDirectory)) {
    Remove-Item -LiteralPath $publishDirectory -Recurse -Force
}

$projects = if ($SkipAppPublish) { @('AudioSwitch.Cli') } else { @('AudioSwitch', 'AudioSwitch.Cli') }
foreach ($project in $projects) {
    $projectPath = Join-Path $repositoryRoot "$project/$project.csproj"
    & dotnet publish $projectPath -c Release -r win-x64 --self-contained false -o $publishDirectory
    if ($LASTEXITCODE -ne 0) {
        throw "Publishing $project failed with exit code $LASTEXITCODE."
    }
}

Copy-Item -LiteralPath (Join-Path $repositoryRoot 'LICENSE') -Destination $publishDirectory
Write-Host "Published AudioSwitch to $publishDirectory"

if ($BuildInstaller) {
    $compiler = Get-Command ISCC.exe -ErrorAction SilentlyContinue
    $compilerPath = if ($compiler) { $compiler.Source } else { Join-Path ${env:ProgramFiles(x86)} 'Inno Setup 6/ISCC.exe' }
    if (-not (Test-Path -LiteralPath $compilerPath)) {
        throw 'Install Inno Setup 6.3 or later, or add ISCC.exe to PATH.'
    }

    & $compilerPath (Join-Path $repositoryRoot 'Setup/Setup.iss')
    if ($LASTEXITCODE -ne 0) {
        throw "Installer compilation failed with exit code $LASTEXITCODE."
    }
}
