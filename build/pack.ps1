$ErrorActionPreference = "Stop"

$restore = if ($args[0] -eq 'no-restore') { $false } else { $true }
$isGithub = [bool]$Env:GITHUB_ACTION

$root = Split-Path -Parent $MyInvocation.MyCommand.Definition

$pkgPath = ([io.path]::combine($root, "*.nupkg"))
Remove-Item $pkgPath

$ver_path = Join-Path $root "pkg.version"
$ver = Get-Content -Path $ver_path
$key = $Env:MYGET_APIKEY
$myget = "https://www.myget.org/F/huoshan12345/api/v2/package"

if ([string]::IsNullOrEmpty($key)) {
  throw "the api key is empty"
}
if ([string]::IsNullOrEmpty($ver)) {
  throw "the version is empty"
}

$onlyWin = ("FclEx.Wmi")

$srcDir = ([io.path]::combine($root, "..", "src"))

$projects = Get-ChildItem -Path $srcDir -Include *.csproj -Recurse `
| Where-Object { $_.Basename.EndsWith('.Tests') -eq $false -and $_.Basename.EndsWith('.Benchmarks') -eq $false } `
| Where-Object { $isGithub -eq $false -or ( ($IsWindows -and $onlyWin -contains $_.Basename) -or ($IsWindows -eq $false -and $onlyWin -notcontains $_.Basename) ) }

foreach ($path in $projects) { 
  Write-Output "Packing $($path.Basename)"
  Set-Location -Path $($path.DirectoryName)

  dotnet clean --nologo -v q

  $command = 'dotnet pack --nologo -v q -c Release --include-symbols --output $root -p:PackageVersion=$ver'
  if ($restore -eq $false) {
    $command = $command + " --no-restore"
  }
  Invoke-Expression $command
  
  if ($Lastexitcode -ne 0)	{
    throw "failed with exit code $LastExitCode"
  }
}

Write-Output "Packing finished."
Read-Host

$files = Get-ChildItem $pkgPath

Write-Output "Uploading..."
foreach ($file in $files) {
  Write-Output "Uploading $($file.Basename)"
  & dotnet nuget push $file -k $key --source $myget -t 50
  if ($Lastexitcode -ne 0) {
    throw "failed with exit code $LastExitCode"
  }
}

Write-Output "Uploading finished."