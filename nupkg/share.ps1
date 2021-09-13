# Paths
$packFolder = (Get-Item -Path "./" -Verbose).FullName
$slnPath = Join-Path $packFolder "../"
$srcPaths = @(
([io.path]::combine($slnPath, "src"))
)

# List of projects
$projectNames = (
"FclEx",
"FclEx.Http",
"FclEx.Xunit",
"FclEx.Serilog",
"FclEx.Wmi"
)

$projectPaths = new-object 'System.Collections.Generic.List[string]'
foreach($srcPath in $srcPaths) {
	$items = Get-ChildItem -Path $srcPath -Include *.csproj -File -Recurse  | 
	Where-Object { $projectNames -contains $_.BaseName }  | % { $_.FullName }
	$projectPaths.AddRange( ([string[]]$items) )
}