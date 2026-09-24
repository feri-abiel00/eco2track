$desktop = [Environment]::GetFolderPath('Desktop')
$wsh = New-Object -ComObject WScript.Shell
$root = (Get-Location).Path

$shortcutPath = Join-Path $desktop 'TerraStep.lnk'
$shortcut = $wsh.CreateShortcut($shortcutPath)
$shortcut.TargetPath = Join-Path $root 'TerraStep.exe'
$shortcut.Arguments = '--launch'
$shortcut.WorkingDirectory = $root
$shortcut.IconLocation = "$root\app.ico,0"
$shortcut.Description = 'TerraStep - ECO2Track Virtual MP3 Ecosystem'
$shortcut.Save()

$ecoPath = Join-Path $desktop 'ECO2Track.lnk'
$eco = $wsh.CreateShortcut($ecoPath)
$eco.TargetPath = Join-Path $root 'TerraStep.exe'
$eco.Arguments = '--launch'
$eco.WorkingDirectory = $root
$eco.IconLocation = "$root\app.ico,0"
$eco.Description = 'ECO2Track - Multi-Module Virtual MP3 Ecosystem'
$eco.Save()

Write-Host "Desktop shortcuts successfully created:"
Get-ChildItem $desktop -Filter "*TerraStep*" | Select-Object Name, FullName, Length
Get-ChildItem $desktop -Filter "*ECO2Track*" | Select-Object Name, FullName, Length
