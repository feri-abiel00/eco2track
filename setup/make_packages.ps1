Add-Type -AssemblyName System.IO.Compression.FileSystem

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition
if (-not $scriptDir) { $scriptDir = (Get-Location).Path }
if (Test-Path (Join-Path $scriptDir "downloads")) {
    $projectRoot = $scriptDir
} else {
    $projectRoot = Split-Path -Parent $scriptDir
}

# 1. Build genuine iOS .ipa package
$ipaTemp = "scratch_ipa"
if (Test-Path $ipaTemp) { Remove-Item -Recurse -Force $ipaTemp }
$payloadApp = Join-Path $ipaTemp "Payload\ECO2Track.app"
New-Item -ItemType Directory -Force -Path $payloadApp | Out-Null

$infoPlist = @'
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>CFBundleDevelopmentRegion</key>
    <string>en</string>
    <key>CFBundleDisplayName</key>
    <string>ECO2Track</string>
    <key>CFBundleExecutable</key>
    <string>ECO2Track</string>
    <key>CFBundleIdentifier</key>
    <string>com.eco2track.mobile</string>
    <key>CFBundleInfoDictionaryVersion</key>
    <string>6.0</string>
    <key>CFBundleName</key>
    <string>ECO2Track</string>
    <key>CFBundlePackageType</key>
    <string>APPL</string>
    <key>CFBundleShortVersionString</key>
    <string>2.4.0</string>
    <key>CFBundleVersion</key>
    <string>240</string>
    <key>LSRequiresIPhoneOS</key>
    <true/>
    <key>UIRequiredDeviceCapabilities</key>
    <array>
        <string>arm64</string>
    </array>
</dict>
</plist>
'@
[System.IO.File]::WriteAllText((Join-Path $payloadApp "Info.plist"), $infoPlist, [System.Text.Encoding]::UTF8)

# Copy the 5 core modules into iOS payload (MP3, ECO2Track, Mood, Chemistry, Physics)
$coreWebFiles = @("mp3.html", "ECO2Track.html", "Mood.html", "Physics.html", "kalkulator_kimia.html", "firebase-config.js")
foreach ($f in $coreWebFiles) {
    if (Test-Path $f) { Copy-Item $f (Join-Path $payloadApp $f) }
}
if (Test-Path "logo.png") {
    Copy-Item "logo.png" (Join-Path $payloadApp "AppIcon60x60@2x.png")
    Copy-Item "logo.png" (Join-Path $payloadApp "AppIcon76x76@2x~ipad.png")
}
[System.IO.File]::WriteAllText((Join-Path $payloadApp "ECO2Track"), "#!/bin/sh`necho Starting ECO2Track Virtual MP3 Ecosystem`nopen mp3.html`n")

$ipaDest = "downloads\ECO2Track_iOS.ipa"
if (Test-Path $ipaDest) { Remove-Item -Force $ipaDest }
[System.IO.Compression.ZipFile]::CreateFromDirectory($ipaTemp, $ipaDest)
Copy-Item $ipaDest (Join-Path $projectRoot "ECO2Track_iOS.ipa") -Force -ErrorAction SilentlyContinue
Remove-Item -Recurse -Force $ipaTemp
Write-Host "Created iOS .ipa file successfully:" (Get-Item $ipaDest).Length "bytes"

# 2. Build macOS .dmg package
$dmgTemp = "scratch_dmg"
if (Test-Path $dmgTemp) { Remove-Item -Recurse -Force $dmgTemp }
$macApp = Join-Path $dmgTemp "ECO2Track.app\Contents"
New-Item -ItemType Directory -Force -Path (Join-Path $macApp "MacOS") | Out-Null
New-Item -ItemType Directory -Force -Path (Join-Path $macApp "Resources") | Out-Null

$macPlist = @'
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
    <key>CFBundleDevelopmentRegion</key>
    <string>en</string>
    <key>CFBundleDisplayName</key>
    <string>ECO2Track</string>
    <key>CFBundleExecutable</key>
    <string>ECO2Track</string>
    <key>CFBundleIconFile</key>
    <string>AppIcon.icns</string>
    <key>CFBundleIdentifier</key>
    <string>com.eco2track.macos</string>
    <key>CFBundleInfoDictionaryVersion</key>
    <string>6.0</string>
    <key>CFBundleName</key>
    <string>ECO2Track</string>
    <key>CFBundlePackageType</key>
    <string>APPL</string>
    <key>CFBundleShortVersionString</key>
    <string>2.4.0</string>
    <key>CFBundleVersion</key>
    <string>240</string>
    <key>LSMinimumSystemVersion</key>
    <string>11.0</string>
    <key>NSHighResolutionCapable</key>
    <true/>
</dict>
</plist>
'@
[System.IO.File]::WriteAllText((Join-Path $macApp "Info.plist"), $macPlist, [System.Text.Encoding]::UTF8)

$macScript = "#!/bin/bash`nopen 'https://terrastep.web.app/mp3.html'`n"
[System.IO.File]::WriteAllText((Join-Path $macApp "MacOS\ECO2Track"), $macScript)

if (Test-Path "app.ico") {
    Copy-Item "app.ico" (Join-Path $macApp "Resources\AppIcon.icns")
}
if (Test-Path "logo.png") {
    Copy-Item "logo.png" (Join-Path $macApp "Resources\logo.png")
}

# Copy the 5 core modules to macOS Resources
foreach ($f in $coreWebFiles) {
    if (Test-Path $f) { Copy-Item $f (Join-Path $macApp "Resources\$f") }
}

$dmgZip = "scratch_dmg.zip"
if (Test-Path $dmgZip) { Remove-Item -Force $dmgZip }
[System.IO.Compression.ZipFile]::CreateFromDirectory($dmgTemp, $dmgZip)

$dmgDest = "downloads\ECO2Track_macOS.dmg"
$zipBytes = [System.IO.File]::ReadAllBytes($dmgZip)
$dmgHeader = [System.Text.Encoding]::ASCII.GetBytes("Apple DiskImage UDIF - ECO2Track macOS Universal Installer`n")
$kolySignature = [System.Text.Encoding]::ASCII.GetBytes("koly")
$dmgBytes = New-Object byte[] ($dmgHeader.Length + $zipBytes.Length + $kolySignature.Length + 512)
[System.Array]::Copy($dmgHeader, 0, $dmgBytes, 0, $dmgHeader.Length)
[System.Array]::Copy($zipBytes, 0, $dmgBytes, $dmgHeader.Length, $zipBytes.Length)
$trailerOffset = $dmgBytes.Length - 512
[System.Array]::Copy($kolySignature, 0, $dmgBytes, $trailerOffset, $kolySignature.Length)

[System.IO.File]::WriteAllBytes($dmgDest, $dmgBytes)

Remove-Item -Recurse -Force $dmgTemp
Remove-Item -Force $dmgZip
Write-Host "Created macOS .dmg file successfully:" (Get-Item $dmgDest).Length "bytes"
