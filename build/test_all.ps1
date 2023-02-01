$ErrorActionPreference = "Stop"

$mode = if ($args[0] -eq 'Release') { "Release" } else { "Debug" }
$restore = if ($args[1] -eq 'no-restore') { $false } else { $true }
$isGithub = $Env:GITHUB_ACTION
Write-Output "mode = $mode, isGithub = $isGithub, restore = $restore"

$root = Split-Path -Parent $MyInvocation.MyCommand.Definition
$slnPath = [io.path]::combine($root, "..")
$testDirs = (
  [io.path]::combine($slnPath, "src\FclEx\test"),
  [io.path]::combine($slnPath, "src\FclEx.Abp\test")
)

$onlyWin = ("FclEx.Wmi.Test")

$projects = $testDirs | ForEach-Object { Get-ChildItem -Path $_ -Include *.csproj -Recurse } `
| Where-Object { $isGithub -eq $false -or ( ($IsWindows -and $onlyWin -contains $_.Basename) -or ($IsWindows -eq $false -and $onlyWin -notcontains $_.Basename) ) }


foreach ($path in $projects) { 
  $command = 'dotnet test $path --nologo -v q -c $mode'
  if ($restore -eq $false) {
    $command = $command + " --no-restore"
  }
  Invoke-Expression $command
  
  if ($Lastexitcode -ne 0) {
    throw "failed with exit code $LastExitCode"
  }
}