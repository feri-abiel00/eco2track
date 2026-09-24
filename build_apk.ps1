# ==============================================================================
# TerraStep Android APK Build Pipeline
# Automates compiling, packaging, signing, and verifying TerraStep.apk
# ==============================================================================

$ErrorActionPreference = "Continue"

$projectRoot = Split-Path -Parent $MyInvocation.MyCommand.Definition
if (-not $projectRoot) {
    $projectRoot = (Get-Location).Path
}

$androidDir   = Join-Path $projectRoot "src\android"
$binDir       = Join-Path $projectRoot "bin\android"
$objDir       = Join-Path $binDir "obj"
$downloadsDir = Join-Path $projectRoot "downloads"

# Target APK output paths
$finalApkRoot      = Join-Path $projectRoot "TerraStep.apk"
$finalApkDownloads = Join-Path $downloadsDir "TerraStep.apk"

# Toolchain paths
$env:JAVA_HOME = "C:\Program Files\Android\Android Studio\jbr"
$env:Path = "$($env:JAVA_HOME)\bin;" + $env:Path
$buildTools  = "C:\Users\Windows\AppData\Local\Android\Sdk\build-tools\36.0.0"
$platformJar = "C:\Users\Windows\AppData\Local\Android\Sdk\platforms\android-37.0\android.jar"

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "   TerraStep Android APK Build Pipeline" -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

# Ensure output directories exist
if (Test-Path $binDir) {
    Remove-Item -Recurse -Force $binDir
}
New-Item -ItemType Directory -Path $binDir -Force | Out-Null
New-Item -ItemType Directory -Path $objDir -Force | Out-Null
New-Item -ItemType Directory -Path $downloadsDir -Force | Out-Null

# 0. Sync fresh web assets to Android assets folder (Only MP3, ECO2Track, Mood, Chemistry, and Physics - No front page)
Write-Host "`n[1/8] Synchronizing web application assets (5 core modules only)..." -ForegroundColor Yellow
$assetSourceFiles = @(
    "mp3.html",
    "ECO2Track.html",
    "kalkulator_kimia (4).html",
    "kalkulator_kimia.html",
    "Mood.html",
    "Physics.html",
    "logo.png",
    "firebase-config.js",
    "ECO₂Track0.png"
)

$assetsTarget = Join-Path $androidDir "assets"
if (Test-Path $assetsTarget) {
    Remove-Item -Recurse -Force $assetsTarget
}
New-Item -ItemType Directory -Path $assetsTarget -Force | Out-Null

foreach ($item in $assetSourceFiles) {
    $srcPath = Join-Path $projectRoot $item
    if (Test-Path $srcPath) {
        Copy-Item -Path $srcPath -Destination $assetsTarget -Force
    }
}
Write-Host "Assets synced successfully (front page index.html excluded)." -ForegroundColor Green

# 1. Compile Android resources
Write-Host "`n[2/8] Compiling resources with aapt2..." -ForegroundColor Yellow
$resZip = Join-Path $binDir "resources.zip"
& "$buildTools\aapt2.exe" compile --dir (Join-Path $androidDir "res") -o $resZip
if ($LASTEXITCODE -ne 0) { throw "aapt2 compile failed with exit code $LASTEXITCODE" }

# 2. Link Android resources and generate R.java
Write-Host "`n[3/8] Linking resources and packaging initial APK..." -ForegroundColor Yellow
$unalignedApk = Join-Path $binDir "unaligned.apk"
$manifestPath = Join-Path $androidDir "AndroidManifest.xml"
$srcDir       = Join-Path $androidDir "src"

& "$buildTools\aapt2.exe" link `
    -o $unalignedApk `
    -I $platformJar `
    --manifest $manifestPath `
    --java $srcDir `
    -A $assetsTarget `
    $resZip `
    --auto-add-overlay
if ($LASTEXITCODE -ne 0) { throw "aapt2 link failed with exit code $LASTEXITCODE" }

# 3. Compile Java sources
Write-Host "`n[4/8] Compiling Java classes with javac..." -ForegroundColor Yellow
$javaFiles = Get-ChildItem -Path $srcDir -Recurse -Filter "*.java" | ForEach-Object { $_.FullName }
& javac -source 8 -target 8 -cp $platformJar -d $objDir $javaFiles
if ($LASTEXITCODE -ne 0) { throw "javac failed with exit code $LASTEXITCODE" }

# 4. Convert classes to Dalvik Executable (classes.dex) using D8
Write-Host "`n[5/8] Converting bytecode to classes.dex with d8..." -ForegroundColor Yellow
$classFiles = Get-ChildItem -Path $objDir -Recurse -Filter "*.class" | ForEach-Object { $_.FullName }
& "$buildTools\d8.bat" --lib $platformJar --output $binDir $classFiles
if ($LASTEXITCODE -ne 0) { throw "d8 failed with exit code $LASTEXITCODE" }

# 5. Add classes.dex into unaligned.apk
Write-Host "`n[6/8] Packaging classes.dex into APK..." -ForegroundColor Yellow
Push-Location $binDir
try {
    & "$env:JAVA_HOME\bin\jar.exe" -uf $unalignedApk classes.dex
} finally {
    Pop-Location
}
if ($LASTEXITCODE -ne 0) { throw "jar -uf failed with exit code $LASTEXITCODE" }

# 6. Zipalign APK (4-byte alignment)
Write-Host "`n[7/8] Aligning APK with zipalign..." -ForegroundColor Yellow
$alignedApk = Join-Path $binDir "aligned.apk"
& "$buildTools\zipalign.exe" -p -f -v 4 $unalignedApk $alignedApk | Out-Null
if ($LASTEXITCODE -ne 0) { throw "zipalign failed with exit code $LASTEXITCODE" }

# 7. Generate keystore & Sign APK
Write-Host "`n[8/8] Signing APK with apksigner..." -ForegroundColor Yellow
$keystorePath = Join-Path $binDir "terrastep-release.keystore"
cmd.exe /c "keytool -genkeypair -noprompt -keystore ""$keystorePath"" -alias terrastepkey -keyalg RSA -keysize 2048 -validity 10000 -storepass terrastep2026 -keypass terrastep2026 -dname ""CN=TerraStep, OU=Engineering, O=SMA Unggul Del, L=Laguboti, ST=North Sumatra, C=ID""" 2>$null

& "$buildTools\apksigner.bat" sign `
    --ks $keystorePath `
    --ks-pass pass:terrastep2026 `
    --ks-key-alias terrastepkey `
    --key-pass pass:terrastep2026 `
    --out $finalApkRoot `
    $alignedApk

if ($LASTEXITCODE -ne 0) { throw "apksigner failed with exit code $LASTEXITCODE" }

# Copy to downloads folder as well
Copy-Item -Path $finalApkRoot -Destination $finalApkDownloads -Force

# Verify APK
Write-Host "`nVerifying final APK signatures:" -ForegroundColor Cyan
& "$buildTools\apksigner.bat" verify --verbose $finalApkRoot

$apkSize = (Get-Item $finalApkRoot).Length / 1KB
Write-Host "`n==========================================================" -ForegroundColor Green
Write-Host "   BUILD SUCCESSFUL!" -ForegroundColor Green
Write-Host "   Generated: $finalApkRoot ($([Math]::Round($apkSize, 2)) KB)" -ForegroundColor Green
Write-Host "   Mirrored:  $finalApkDownloads" -ForegroundColor Green
Write-Host "==========================================================" -ForegroundColor Green
