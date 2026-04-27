using CommunityToolkit.Mvvm.ComponentModel;

namespace GameLauncher.BigScreen.Models;

/// <summary>
/// Model representing disk/drive information for the System Info view.
/// </summary>
public partial class DiskInfo : ObservableObject
{
    [ObservableProperty]
    private string _letter = string.Empty;

    [ObservableProperty]
    private double _totalGB;

    [ObservableProperty]
    private double _freeGB;

    [ObservableProperty]
    private int _usagePercent;

    public DiskInfo(string letter, long totalBytes, long freeBytes)
    {
        Letter = letter;
        TotalGB = Math.Round(totalBytes / (1024.0 * 1024.0 * 1024.0), 2);
        FreeGB = Math.Round(freeBytes / (1024.0 * 1024.0 * 1024.0), 2);
        UsagePercent = totalBytes > 0 
            ? (int)Math.Round(((totalBytes - freeBytes) / (double)totalBytes) * 100) 
            : 0;
    }
}
