$desktop = [Environment]::GetFolderPath('Desktop')
$wsh = New-Object -ComObject WScript.Shell
$root = (Get-Location).Path


# 1. Create TerraStep Desktop Shortcut
$shortcutPath = Join-Path $desktop 'TerraStep.lnk'
$shortcut = $wsh.CreateShortcut($shortcutPath)
$shortcut.TargetPath = Join-Path $root 'TerraStep.exe'
$shortcut.Arguments = '--launch'
$shortcut.WorkingDirectory = $root
$shortcut.IconLocation = "$root\app.ico,0"
$shortcut.Description = 'TerraStep - ECO2Track Virtual MP3 Ecosystem'
$shortcut.Save()

# 2. Remove obsolete ECO2Track shortcut if present
$ecoPath = Join-Path $desktop 'ECO2Track.lnk'
if (Test-Path $ecoPath) {
    Remove-Item -Force $ecoPath
}

Write-Host "Desktop shortcut successfully updated (TerraStep only):"
Get-ChildItem $desktop -Filter "*TerraStep*" | Select-Object Name, FullName, Length

