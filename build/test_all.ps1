$ErrorActionPreference = "Stop"

$mode = if ($args[0] -eq 'Release') { "Release" } else { "Debug" }
$disableOSCheck = $args[1] -eq 'true'
$isGithub = [string]::IsNullOrEmpty($Env:GITHUB_ACTION) -eq $false
Write-Output "mode = $mode, isGithub = $isGithub"

$rootDir = [io.path]::combine($MyInvocation.MyCommand.Definition, "..", "..")
$slnDir = [io.path]::combine($rootDir, "src")

$testDirs = (
  [io.path]::combine($slnDir, "FclEx", "test"),
  [io.path]::combine($slnDir, "FclEx.Abp", "test")
)

$onlyWin = ("FclEx.Wmi.Tests")

$projects = $testDirs | ForEach-Object { Get-ChildItem -Path $_ -Include *.csproj -Recurse } `
| Where-Object { $isGithub -eq $false -or $disableOSCheck -or ( ($IsWindows -and $onlyWin -contains $_.Basename) -or ($IsWindows -eq $false -and $onlyWin -notcontains $_.Basename) ) }


$result = [ordered]@{}
foreach ($project in $projects) {
  Write-Output "Testing $($project.Basename)"
  $command = 'dotnet test $project.FullName --nologo -c $mode -v n --property:WarningLevel=0 /clp:ErrorsOnly'
  Invoke-Expression $command
  $success = $Lastexitcode -eq 0
  $result.Add($project.Name, $success)
}

$failed = $result.GetEnumerator() | Where-Object { $_.Value -ne $true } | Join-String -Property Name -Separator ', '
  
if ($failed) {
  throw "Failed projects: $failed"
}

Write-Output "Testing finished."