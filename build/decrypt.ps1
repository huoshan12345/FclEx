$rootDir = [io.path]::combine($PSScriptRoot, "..")
$settingsDir = [io.path]::combine($rootDir, "test", "FclEx.Core.Tests")
$encryptedPath = [io.path]::combine($settingsDir, "appsettings.encrypted.json")
$decryptedPath = [io.path]::combine($settingsDir, "appsettings.decrypted.json")

$content = sops -d $encryptedPath | Out-String

[System.IO.File]::WriteAllText($decryptedPath, $content, [System.Text.UTF8Encoding]::new($false))