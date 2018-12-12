# Paths
$packFolder = (Get-Item -Path "./" -Verbose).FullName
$slnPath = Join-Path $packFolder "../"
$srcPath = Join-Path $slnPath "src"

# List of projects
$projects = (
"FclEx",
"FclEx.Http",
"FclEx.Image",
"FclEx.Npoi"
)

Set-Location $slnPath
foreach($project in $projects) {    
    $projectFolder = Join-Path $srcPath $project	
	$outputDir = New-Object System.IO.DirectoryInfo -ArgumentList @($projectFolder)
	$l = $outputDir.GetFiles("*.csproj", [System.IO.SearchOption]::TopDirectoryOnly);
	$reg =  [regex]"(?<=<Version>)(\d+)\.(\d+)\.(\d+)(?=</Version>)";
	$callback = {
		$m = $args[0]
		$major = [int]$m.Groups[1].Value
		$minor = [int]$m.Groups[2].Value
		$build = [int]$m.Groups[3].Value
		$ver = $major * 100 + $minor * 10 + $build
		$ver++
		$major = ($ver - $ver % 100) / 100
		$minor = ($ver % 100 - $ver % 10) / 10
		$build = $ver % 10

		"$major.$minor.$build"
		#"1.5.0"
	}
	foreach ($i in $l){
		$path = $i.FullName
        (Get-Content -Path $path) | % { $reg.Replace($_, $callback) } | Set-Content $path
	}
}

# Go back to the pack folder
Set-Location $packFolder

Write-Output "Finished. Press any key to exit."
Read-Host