using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;

namespace GameLauncher.BigScreen.Models;

/// <summary>
/// Model representing game scan results for a specific drive.
/// Tracks ROM/ISO files and PC executables found during scanning.
/// </summary>
public partial class DiskGameScan : ObservableObject
{
    [ObservableProperty]
    private string _driveLetter = string.Empty;

    [ObservableProperty]
    private int _romCount;

    [ObservableProperty]
    private int _pcGameCount;

    [ObservableProperty]
    private Dictionary<string, int> _romsByConsole = new();

    [ObservableProperty]
    private int _totalFound;

    [ObservableProperty]
    private bool _isScanning;

    public DiskGameScan(string driveLetter)
    {
        DriveLetter = driveLetter;
    }
}
