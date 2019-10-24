$Env:DOTNET_SYSTEM_NET_HTTP_USESOCKETSHTTPHANDLER = 0
$ErrorActionPreference = "Stop"

. "./share.ps1"

# Go back to the pack folder
Set-Location $packFolder
Remove-Item *.nupkg

foreach($path in $projectPaths) { 
    & dotnet pack $path -c Release --no-restore --include-symbols -v q --output $packFolder
	if ($Lastexitcode -ne 0)	{
		throw "failed with exit code $LastExitCode"
	}
}

$PSGallerySourceUri = 'https://www.myget.org/F/huoshan12345/api/v2/package'
$APIKey = 'fbc0486a-55ff-4760-b246-bef3e0ee952d'

$files = Get-ChildItem ./*.nupkg
foreach ($file in $files) {
	& dotnet nuget push $file -k $APIKey -s $PSGallerySourceUri --timeout 50
	if ($Lastexitcode -ne 0) {
		throw "failed with exit code $LastExitCode"
	}
}

Write-Output "Finished. Press any key to exit."
Read-Host