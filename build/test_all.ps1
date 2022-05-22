# Paths
$root = Split-Path -Parent $MyInvocation.MyCommand.Definition
$slnPath = Join-Path $root "../"
$srcPaths = @(
([io.path]::combine($slnPath, "test"))
)

$projectPaths = new-object 'System.Collections.Generic.List[string]'
foreach($srcPath in $srcPaths) {
	$items = Get-ChildItem -Path $srcPath -Include *.csproj -File -Recurse | % { $_.FullName }
	$projectPaths.AddRange( ([string[]]$items) )
}

foreach($path in $projectPaths) { 
    & dotnet test $path -v q
	if ($Lastexitcode -ne 0)	{
		throw "failed with exit code $LastExitCode"
	}
}