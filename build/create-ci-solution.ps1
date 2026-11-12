[CmdletBinding()]
param(
    [ValidateNotNullOrEmpty()]
    [string]$Framework = 'net472',

    [ValidateNotNullOrEmpty()]
    [string]$SolutionPath = 'FclEx.slnx',

    [ValidateNotNullOrEmpty()]
    [string]$OutputPath = "artifacts/ci/Tests.$Framework.slnx"
)

$ErrorActionPreference = 'Stop'
$repositoryRoot = Split-Path $PSScriptRoot -Parent
$sourcePath = [IO.Path]::GetFullPath($SolutionPath, $repositoryRoot)
$sourceDirectory = Split-Path $sourcePath -Parent
$destination = [IO.Path]::GetFullPath($OutputPath, $repositoryRoot)
if ($destination -eq $sourcePath) {
    throw 'The output path must differ from the source solution.'
}

[xml]$sourceSolution = Get-Content -LiteralPath $sourcePath -Raw
$selectedProjects = @($sourceSolution.SelectNodes('//Project') | ForEach-Object {
    $projectPath = [IO.Path]::GetFullPath($_.GetAttribute('Path'), $sourceDirectory)
    $repositoryRelativePath = [IO.Path]::GetRelativePath($repositoryRoot, $projectPath).Replace('\', '/')
    if (-not $repositoryRelativePath.StartsWith('test/')) { return }

    $evaluation = & dotnet msbuild $projectPath -nologo -property:Configuration=Release `
        '-getProperty:TargetFrameworks,TargetFramework,IsTestProject' 2>&1
    if ($LASTEXITCODE -ne 0) {
        throw "Failed to evaluate ${repositoryRelativePath}:`n$($evaluation -join [Environment]::NewLine)"
    }

    $properties = ($evaluation -join [Environment]::NewLine | ConvertFrom-Json).Properties
    $frameworks = @("$($properties.TargetFrameworks);$($properties.TargetFramework)".Split(';') |
        ForEach-Object { $_.Trim() } | Where-Object { $_ })
    if ($properties.IsTestProject -eq 'true' -and $Framework -in $frameworks) {
        $projectPath
    }
}) | Sort-Object -Unique

if ($selectedProjects.Count -eq 0) {
    throw "No test projects in $SolutionPath support $Framework on this host."
}

$destinationDirectory = Split-Path $destination -Parent
New-Item -ItemType Directory -Path $destinationDirectory -Force | Out-Null
[xml]$solution = '<Solution />'
foreach ($projectPath in $selectedProjects) {
    $project = $solution.CreateElement('Project')
    $relativePath = [IO.Path]::GetRelativePath($destinationDirectory, $projectPath).Replace('\', '/')
    $project.SetAttribute('Path', $relativePath)
    [void]$solution.DocumentElement.AppendChild($project)
}

$settings = [Xml.XmlWriterSettings]::new()
$settings.Encoding = [Text.UTF8Encoding]::new($false)
$settings.Indent = $true
$settings.NewLineChars = [Environment]::NewLine
$writer = [Xml.XmlWriter]::Create($destination, $settings)
try {
    $solution.Save($writer)
}
finally {
    $writer.Dispose()
}
Write-Host "Created $Framework solution with $($selectedProjects.Count) test projects: $destination"
