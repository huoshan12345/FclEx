$Env:DOTNET_SYSTEM_NET_HTTP_USESOCKETSHTTPHANDLER = 0
$ErrorActionPreference = "Stop"

. "./share.ps1"

# Go back to the pack folder
Set-Location $packFolder
Remove-Item *.nupkg

$ver_path = ".\pkg.version"
$ver = Get-Content -Path $ver_path
$key = $Env:MYGET_APIKEY
$myget = "https://www.myget.org/F/huoshan12345/api/v2/package"

if ([string]::IsNullOrEmpty($key)){
	throw "the api key is empty"
}
if ([string]::IsNullOrEmpty($ver)){
	throw "the version is empty"
}

Write-Output "Packing..."
foreach($path in $projectPaths) { 
    & dotnet clean $path -v q
    & dotnet pack $path --nologo -c Release --include-symbols -v q --output $packFolder -p:PackageVersion=$ver
	if ($Lastexitcode -ne 0)	{
		throw "failed with exit code $LastExitCode"
	}
	Write-Output "Packed $($path)"
}

Write-Output "Packing finished."


$files = Get-ChildItem ./*.nupkg

Write-Output "Uploading..."
foreach ($file in $files) {
	& dotnet nuget push $file -k $key --source $myget -t 50
	if ($Lastexitcode -ne 0) {
		throw "failed with exit code $LastExitCode"
	}
}

Write-Output "Uploading finished."