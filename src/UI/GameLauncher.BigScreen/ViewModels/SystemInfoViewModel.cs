using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GameLauncher.BigScreen.Models;

namespace GameLauncher.BigScreen.ViewModels;

/// <summary>
/// ViewModel for the System Info view.
/// Displays hardware information, disk storage, and scans for game files.
/// </summary>
public partial class SystemInfoViewModel : ObservableObject
{
    private readonly string[] _sections = { "Hardware", "Almacenamiento", "Juegos Detectados" };
    private readonly string[] _targetDrives = { "A:", "F:", "G:", "H:" };

    [ObservableProperty]
    private int _currentSectionIndex = 0;

    [ObservableProperty]
    private string _currentSectionTitle = "Hardware";

    // Hardware properties
    [ObservableProperty]
    private string _cpuName = "Detectando...";

    [ObservableProperty]
    private string _gpuName = "Detectando...";
    
    [ObservableProperty]
    private string _gpuVram = "Detectando...";

    [ObservableProperty]
    private string _ramTotal = "Detectando...";

    [ObservableProperty]
    private string _osVersion = "Detectando...";

    [ObservableProperty]
    private string _screenResolution = "Detectando...";

    // Disk storage
    [ObservableProperty]
    private ObservableCollection<DiskInfo> _disks = new();

    // Game detection
    [ObservableProperty]
    private ObservableCollection<DiskGameScan> _scannedDisks = new();

    [ObservableProperty]
    private bool _isScanningGames = false;

    [ObservableProperty]
    private string _scanStatus = "";

    public async Task InitializeAsync()
    {
        await LoadHardwareInfoAsync();
        await LoadDiskInfoAsync();
        _ = ScanGamesAsync(); // Start scan but don't await
    }

    public void PreviousSection()
    {
        if (CurrentSectionIndex > 0)
        {
            CurrentSectionIndex--;
            CurrentSectionTitle = _sections[CurrentSectionIndex];
        }
    }

    public void NextSection()
    {
        if (CurrentSectionIndex < _sections.Length - 1)
        {
            CurrentSectionIndex++;
            CurrentSectionTitle = _sections[CurrentSectionIndex];
        }
    }

    private async Task LoadHardwareInfoAsync()
    {
        await Task.Run(() =>
        {
            try
            {
                // CPU detection via WMI
                using (var searcher = new ManagementObjectSearcher("SELECT Name FROM Win32_Processor"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        CpuName = obj["Name"]?.ToString()?.Trim() ?? "Desconocido";
                        break; // Get first processor
                    }
                }

                // GPU detection via WMI
                using (var searcher = new ManagementObjectSearcher("SELECT Name, AdapterRAM FROM Win32_VideoController"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        var gpuNameRaw = obj["Name"]?.ToString()?.Trim();
                        if (!string.IsNullOrEmpty(gpuNameRaw))
                        {
                            GpuName = gpuNameRaw;
                             // Extract VRAM
                            if (obj["AdapterRAM"] != null)
                            {
                                long vramBytes = Convert.ToInt64(obj["AdapterRAM"]);
                                double vramGb = vramBytes / (1024.0 * 1024.0 * 1024.0);
                                GpuVram = $"{Math.Round(vramGb, 1)} GB";
                            }
                            else
                            {
                                GpuVram = "N/A";
                            }
                            break; // Get primary GPU
                        }
                    }
                }

                // RAM detection via WMI
                long totalRamBytes = 0;
                using (var searcher = new ManagementObjectSearcher("SELECT Capacity FROM Win32_PhysicalMemory"))
                {
                    foreach (ManagementObject obj in searcher.Get())
                    {
                        if (obj["Capacity"] != null)
                        {
                            totalRamBytes += Convert.ToInt64(obj["Capacity"]);
                        }
                    }
                }
                double totalRamGB = totalRamBytes / (1024.0 * 1024.0 * 1024.0);
                RamTotal = $"{Math.Round(totalRamGB)} GB";

                // OS version
                OsVersion = $"{RuntimeInformation.OSDescription}";

                // Screen resolution
                Application.Current.Dispatcher.Invoke(() =>
                {
                    var width = SystemParameters.PrimaryScreenWidth;
                    var height = SystemParameters.PrimaryScreenHeight;
                    ScreenResolution = $"{width} x {height}";
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SystemInfoVM] Hardware detection error: {ex.Message}");
                CpuName = "Error al detectar";
                GpuName = "Error al detectar";
                RamTotal = "Error al detectar";
                OsVersion = "Error al detectar";
                ScreenResolution = "Error al detectar";
            }
        });
    }

    private async Task LoadDiskInfoAsync()
    {
        await Task.Run(() =>
        {
            try
            {
                var allDrives = DriveInfo.GetDrives();
                var targetedDrives = allDrives.Where(d => 
                    _targetDrives.Contains(d.Name.TrimEnd('\\')) && d.IsReady).ToList();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    Disks.Clear();
                    foreach (var drive in targetedDrives)
                    {
                        var diskInfo = new DiskInfo(
                            drive.Name.TrimEnd('\\'),
                            drive.TotalSize,
                            drive.AvailableFreeSpace
                        );
                        Disks.Add(diskInfo);
                    }
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SystemInfoVM] Disk detection error: {ex.Message}");
            }
        });
    }

    private async Task ScanGamesAsync()
    {
        IsScanningGames = true;
        ScanStatus = "Escaneando discos...";

        await Task.Run(() =>
        {
            try
            {
                var romExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
                {
                    ".nes", ".smc", ".sfc", ".gba", ".gbc", ".gb", ".n64", ".z64",
                    ".nds", ".3ds", ".iso", ".bin", ".cue", ".chd", ".gcm", ".nsp",
                    ".xci", ".wbfs", ".wad", ".pbp", ".pkg"
                };

                var extensionToConsole = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    { ".nes", "NES" }, { ".smc", "SNES" }, { ".sfc", "SNES" },
                    { ".gba", "GBA" }, { ".gbc", "GBC" }, { ".gb", "GB" },
                    { ".n64", "N64" }, { ".z64", "N64" },
                    { ".nds", "NDS" }, { ".3ds", "3DS" },
                    { ".iso", "ISO/PS1/PSP" }, { ".bin", "PS1/Sega CD" }, { ".cue", "PS1/Sega CD" },
                    { ".chd", "PS1/Dreamcast" }, { ".gcm", "GameCube" },
                    { ".nsp", "Switch" }, { ".xci", "Switch" },
                    { ".wbfs", "Wii" }, { ".wad", "Wii" },
                    { ".pbp", "PSP" }, { ".pkg", "PS3" }
                };

                var allDrives = DriveInfo.GetDrives();
                var targetedDrives = allDrives.Where(d =>
                    _targetDrives.Contains(d.Name.TrimEnd('\\')) && d.IsReady).ToList();

                foreach (var drive in targetedDrives)
                {
                    DiskGameScan scanResultRef = null;
                    
                    Application.Current.Dispatcher.Invoke(() => 
                    {
                        var scanResult = new DiskGameScan(drive.Name.TrimEnd('\\'))
                        {
                            IsScanning = true
                        };
                        ScannedDisks.Add(scanResult);
                        scanResultRef = scanResult; // Capture reference safely on UI thread
                        ScanStatus = $"Escaneando {drive.Name}...";
                    });
                    
                    try
                    {
                        var romsByConsole = new Dictionary<string, int>();
                        int romCount = 0;
                        int pcGameCount = 0;

                        // Scan drive recursively up to depth 3
                        ScanDirectory(drive.RootDirectory.FullName, 0, 3, romExtensions, extensionToConsole,
                            ref romCount, ref pcGameCount, romsByConsole);

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            if (scanResultRef != null) 
                            { 
                                scanResultRef.RomCount = romCount;
                                scanResultRef.PcGameCount = pcGameCount;
                                scanResultRef.RomsByConsole = romsByConsole;
                                scanResultRef.TotalFound = romCount + pcGameCount;
                                scanResultRef.IsScanning = false;
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine($"[SystemInfoVM] Scan error for {drive.Name}: {ex.Message}");
                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            if (scanResultRef != null) scanResultRef.IsScanning = false;
                        });
                    }
                }

                Application.Current.Dispatcher.Invoke(() => ScanStatus = "Escaneo completo");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"[SystemInfoVM] Game scan error: {ex.Message}");
                Application.Current.Dispatcher.Invoke(() => ScanStatus = "Error en escaneo");
            }
            finally
            {
                Application.Current.Dispatcher.Invoke(() => IsScanningGames = false);
            }
        });
    }

    private void ScanDirectory(
        string path,
        int currentDepth,
        int maxDepth,
        HashSet<string> romExtensions,
        Dictionary<string, string> extensionToConsole,
        ref int romCount,
        ref int pcGameCount,
        Dictionary<string, int> romsByConsole)
    {
        if (currentDepth > maxDepth)
            return;

        try
        {
            // Skip Windows system directories
            var dirName = Path.GetFileName(path);
            if (dirName.Equals("Windows", StringComparison.OrdinalIgnoreCase) ||
                dirName.Equals("Program Files", StringComparison.OrdinalIgnoreCase) ||
                dirName.Equals("Program Files (x86)", StringComparison.OrdinalIgnoreCase) ||
                dirName.Equals("ProgramData", StringComparison.OrdinalIgnoreCase) ||
                dirName.StartsWith("$", StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            // Scan files in current directory
            foreach (var file in Directory.GetFiles(path))
            {
                var ext = Path.GetExtension(file);
                
                if (romExtensions.Contains(ext))
                {
                    romCount++;
                    if (extensionToConsole.TryGetValue(ext, out var console))
                    {
                        if (!romsByConsole.ContainsKey(console))
                            romsByConsole[console] = 0;
                        romsByConsole[console]++;
                    }
                }
                else if (ext.Equals(".exe", StringComparison.OrdinalIgnoreCase))
                {
                    // Simple heuristic: count .exe files (excluding ones in obvious system dirs)
                    pcGameCount++;
                }
            }

            // Recurse into subdirectories
            foreach (var subDir in Directory.GetDirectories(path))
            {
                ScanDirectory(subDir, currentDepth + 1, maxDepth, romExtensions, extensionToConsole,
                    ref romCount, ref pcGameCount, romsByConsole);
            }
        }
        catch (UnauthorizedAccessException)
        {
            // Skip directories we don't have access to
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[SystemInfoVM] Error scanning {path}: {ex.Message}");
        }
    }
}
