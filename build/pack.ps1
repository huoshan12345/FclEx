$ErrorActionPreference = "Stop"

$disableOSCheck = $args[0] -eq 'true'
$isGithub = [string]::IsNullOrEmpty($Env:GITHUB_ACTION) -eq $false
Write-Output "isGithub = $isGithub"

$buildDir = [io.path]::combine($MyInvocation.MyCommand.Definition, "..")
$rootDir = [io.path]::combine($buildDir, "..")
$slnDir = [io.path]::combine($rootDir, "src")

$pkgPath = ([io.path]::combine($buildDir, "*.nupkg"))
Remove-Item $pkgPath

$ver_path = ([io.path]::combine($buildDir, "pkg.version"))
$ver = Get-Content -Path $ver_path
$key = $Env:MYGET_APIKEY
$myget = "https://www.myget.org/F/huoshan12345/api/v2/package"

if ([string]::IsNullOrEmpty($key)) {
  throw "the api key is empty"
}
if ([string]::IsNullOrEmpty($ver)) {
  throw "the version is empty"
}

$srcDirs = (
  [io.path]::combine($slnDir, "FclEx", "src"),
  [io.path]::combine($slnDir, "FclEx.Abp", "src")
)

$onlyWin = ("FclEx.Wmi")

$projects = $srcDirs | ForEach-Object { Get-ChildItem -Path $_ -Include *.csproj -Recurse } `
| Where-Object { $isGithub -eq $false -or $disableOSCheck -or ( ($IsWindows -and $onlyWin -contains $_.Basename) -or ($IsWindows -eq $false -and $onlyWin -notcontains $_.Basename) ) }

foreach ($project in $projects) { 
  Write-Output "Packing $($project.Basename)"
  Set-Location -Path $($project.DirectoryName)

  dotnet clean --nologo -v q

  $command = 'dotnet pack --nologo -v q -c Release --include-symbols --output $buildDir -p:PackageVersion=$ver'
  Invoke-Expression $command
  
  if ($Lastexitcode -ne 0)	{
    throw "failed with exit code $LastExitCode"
  }
}

Write-Output "Packing finished."

if ($isGithub) {
  Write-Output "Uploading..."

  $files = Get-ChildItem $pkgPath
  foreach ($file in $files) {
    Write-Output "Uploading $($file.Basename)"
    & dotnet nuget push $file -k $key --source $myget -t 50
    if ($Lastexitcode -ne 0) {
      throw "failed with exit code $LastExitCode"
    }
  }

  Write-Output "Uploading finished."
}
else {
  foreach ($project in $projects) {
    Write-Output "Removing $($project.Basename) from nuget cache"
    $packageLocalDir = [io.path]::combine( $env:USERPROFILE, ".nuget", "packages", $project.Basename.ToLower(), $ver);
    Remove-Item $packageLocalDir -Recurse -Force -ErrorAction SilentlyContinue
  }
}