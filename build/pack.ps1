$ErrorActionPreference = "Stop"

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

# List of projects
$projectNames = (
  "FclEx",
  "FclEx.Http",
  "FclEx.Xunit",
  "FclEx.Wmi",
  "FclEx.Serilog",

  "ServiceStack.OrmLite.Custom",
  "ServiceStack.OrmLite.MySql.Custom",
  "ServiceStack.OrmLite.PostgreSQL.Custom",
  "ServiceStack.OrmLite.Sqlite.Custom",
  "ServiceStack.OrmLite.SqlServer.Custom",
  "ServiceStack.OrmLite.Oracle.Custom",

  "FclEx.Abp.OrmLite",
  "FclEx.Abp.RabbitMQ",
  "FclEx.Abp",
  "FclEx.Abp.RedisCache",
  "FclEx.Abp.AspNetCore",
  "FclEx.Abp.Xunit"
)

$srcDir = ([io.path]::combine($root, "..", "src"))

$projects = Get-ChildItem -Path $srcDir -Include *.csproj -Recurse | Where-Object { $projectNames -Contains $_.Basename } 

foreach ($path in $projects) { 
  Write-Output "Packing $path"
  & dotnet clean $path --nologo -v q
  & dotnet pack $path --nologo -v q -c Release --include-symbols --output $root -p:PackageVersion=$ver
  if ($Lastexitcode -ne 0)	{
    throw "failed with exit code $LastExitCode"
  }
}

Write-Output "Packing finished."


$files = Get-ChildItem $pkgPath

Write-Output "Uploading..."
foreach ($file in $files) {
  Write-Output "Uploading $file"
  & dotnet nuget push $file -k $key --source $myget -t 50
  if ($Lastexitcode -ne 0) {
    throw "failed with exit code $LastExitCode"
  }
}

Write-Output "Uploading finished."