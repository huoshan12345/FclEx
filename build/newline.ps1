Set-Location $PSScriptRoot

$excludedPattern = '\\(bin|obj|bld|Backup|_UpgradeReport_Files|Debug|Release|ipch|TestResults|node_modules|dist)\\'

Get-ChildItem ..\ -Recurse -File |
Where-Object {
  $_.FullName -notmatch $excludedPattern
} |
Where-Object {
  $_.Extension -in ".cs", ".ts", ".js", ".json", ".md", ".yml", ".yaml"
} |
ForEach-Object {
  $content = Get-Content $_.FullName -Raw
  $new_content = $content -replace "`r`n", "`n"
  $new_content = $new_content -replace "`n", "`r`n"
  if ($new_content -ne $content) {
    Write-Host "Updating file: $($_.FullName)"
    [System.IO.File]::WriteAllText($_.FullName, $new_content)
  }
}