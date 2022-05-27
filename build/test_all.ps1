$ErrorActionPreference = "Stop"

$mode = if ($args[0] -eq 'Release') { "Release" } else { "Debug" }
$isProd = if ($args[1] -eq 'prod') { $true } else { $false }

Write-Output "mode = $mode, isProd = $isProd"

$root = Split-Path -Parent $MyInvocation.MyCommand.Definition
$slnPath = [io.path]::combine($root, "..")
$testDirs = (
  [io.path]::combine($slnPath, "src\FclEx\test"),
  [io.path]::combine($slnPath, "src\FclEx.Abp\test")
)

$excludeInNonLocal = (
  "FclEx.Abp.RabbitMQ.Test",
  "FclEx.Abp.RedisCache.Test"
)

$projects = $testDirs  | ForEach-Object { Get-ChildItem -Path $_ -Include *.csproj -Recurse } | Where-Object { $isProd -eq $false -or ($isProd -and ($excludeInNonLocal -notcontains $_.Basename)) }

foreach ($path in $projects) { 
  & dotnet test $path --nologo -v q -c $mode
  if ($Lastexitcode -ne 0) {
    throw "failed with exit code $LastExitCode"
  }
}