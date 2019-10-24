$reg = [regex]"(?<=<Version>)(\d+)\.(\d+)\.(\d+)(?=</Version>)";
$callback = {
	$m = $args[0]
	$major = [int]$m.Groups[1].Value
	$minor = [int]$m.Groups[2].Value
	$build = [int]$m.Groups[3].Value
	$ver = $major * 100 + $minor * 10 + $build
	$ver++

	$digits = New-Object System.Collections.Generic.List[int]
	$number = $ver
	For ($i = 3; $i -gt 0; $i--) {
		$digit = $number % 10
		$number = ($number - $digit) / 10;
		$digits.Add($digit);
	}
	$digits.Reverse()
	$str = [system.String]::Join(".", $digits)
	$str
	#"2.3.0"
}
$path = ".\version.props"
(Get-Content -Path $path) | % { $reg.Replace($_, $callback) } | Set-Content $path

Write-Output "Finished. Press any key to exit."
Read-Host