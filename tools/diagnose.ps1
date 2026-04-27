
Write-Output "--- EXES ---"
Get-ChildItem -Path "H:\GameLauncher" -Filter "*.exe" -Recurse | Select-Object FullName, LastWriteTime

Write-Output "--- APP.XAML ---"
Get-Content "H:\GameLauncher\src\UI\GameLauncher.BigScreen\App.xaml" | Select-String "StartupUri"

Write-Output "--- SHORTCUTS ---"
$sh = New-Object -ComObject WScript.Shell
try { 
    $s = $sh.CreateShortcut('H:\GameLauncher\GameLauncher BigScreen.lnk')
    "BigScreen Target: " + $s.TargetPath 
} catch { 
    "BigScreen Error: " + $_ 
}

try { 
    $s = $sh.CreateShortcut('H:\GameLauncher\GameLauncher Desktop.lnk')
    "Desktop Target: " + $s.TargetPath 
} catch { 
    "Desktop Error: " + $_ 
}
