$rootDir = [io.path]::combine($PSScriptRoot, "..")
$settingsDir = [io.path]::combine($rootDir, "test", "FclEx.Core.Tests")
$encryptedPath = [io.path]::combine($settingsDir, "appsettings.encrypted.json")
$decryptedPath = [io.path]::combine($settingsDir, "appsettings.decrypted.json")

$content = sops -e $decryptedPath | Out-String

[System.IO.File]::WriteAllText($encryptedPath, $content, [System.Text.UTF8Encoding]::new($false))