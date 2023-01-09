$ErrorActionPreference = "Stop"

$mode = if ($args[0] -eq 'Release') { "Release" } else { "Debug" }
$isProd = if ($args[1] -eq 'prod') { $true } else { $false }
$restore = if ($args[2] -eq 'no-restore') { $false } else { $true }

Write-Output "mode = $mode, isProd = $isProd, restore = $restore"

$root = Split-Path -Parent $MyInvocation.MyCommand.Definition
$slnPath = [io.path]::combine($root, "..")
$testDirs = (
  [io.path]::combine($slnPath, "src\FclEx\test"),
  [io.path]::combine($slnPath, "src\FclEx.Abp\test")
)

$excludeInNonLocal = @()

$projects = $testDirs | ForEach-Object { Get-ChildItem -Path $_ -Include *.csproj -Recurse } | Where-Object { $isProd -eq $false -or ($isProd -and ($excludeInNonLocal -notcontains $_.Basename)) }

foreach ($path in $projects) { 
  $command = 'dotnet test $path --nologo -v q -c $mode'
  if($restore -eq $false) {
      $command = $command + " --no-restore"
  }
  Invoke-Expression $command
  
  if ($Lastexitcode -ne 0) {
    throw "failed with exit code $LastExitCode"
  }
}