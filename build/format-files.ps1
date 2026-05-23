Set-Location $PSScriptRoot

$currentScriptPath = [System.IO.Path]::GetFullPath($PSCommandPath)

$excludedPattern = '\\(bin|obj|bld|Backup|_UpgradeReport_Files|Debug|Release|ipch|TestResults|node_modules|dist)\\'

$includedExtensions = @(
  ".cs",
  ".ts",
  ".js",
  ".json",
  ".md",
  ".yml",
  ".yaml",
  ".csproj",
  ".ps1",
  ".props",
  ".targets",
  ".DotSettings"
)

function Test-Utf8NoBom {
  param(
    [Parameter(Mandatory)]
    [string] $Path
  )

  $bytes = [System.IO.File]::ReadAllBytes($Path)

  # UTF-8 BOM: EF BB BF
  if ($bytes.Length -ge 3 -and
      $bytes[0] -eq 0xEF -and
      $bytes[1] -eq 0xBB -and
      $bytes[2] -eq 0xBF) {
    return $false
  }

  try {
    $utf8Strict = [System.Text.UTF8Encoding]::new($false, $true)
    $null = $utf8Strict.GetString($bytes)
    return $true
  }
  catch {
    return $false
  }
}

function Write-Utf8NoBom {
  param(
    [Parameter(Mandatory)]
    [string] $Path,

    [Parameter(Mandatory)]
    [string] $Content
  )

  [System.IO.File]::WriteAllText(
    $Path,
    $Content,
    [System.Text.UTF8Encoding]::new($false)
  )
}

function Format-TextFile {
  param(
    [Parameter(Mandatory)]
    [string] $Path
  )

  $content = Get-Content $Path -Raw

  $newContent = $content -replace "`r`n", "`n"
  $newContent = $newContent -replace "`n", "`r`n"

  if ($newContent -ne $content) {
    Write-Host "Updating line endings: $Path"
    Write-Utf8NoBom $Path $newContent
    return
  }

  if (-not (Test-Utf8NoBom $Path)) {
    Write-Host "Updating encoding: $Path"
    Write-Utf8NoBom $Path $content
  }
}

Get-ChildItem ..\ -Recurse -File |
Where-Object {
  [System.IO.Path]::GetFullPath($_.FullName) -ne $currentScriptPath
} |
Where-Object {
  $_.FullName -notmatch $excludedPattern
} |
Where-Object {
  $_.Extension -in $includedExtensions
} |
ForEach-Object {
  Format-TextFile $_.FullName
}

Write-Output "Formating text files finished."
Read-Host