using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GameLauncher.BigScreen.Models;
using GameLauncher.BigScreen.Services;
using System.Linq;

namespace GameLauncher.BigScreen.ViewModels;

public partial class SourcesViewModel : ObservableObject
{
    private readonly List<IGameSourceService> _services;
    private IGameSourceService _currentService;
    private readonly RequirementsAnalyzer _analyzer;
    
    [ObservableProperty]
    private ObservableCollection<GameSourceItem> _latestGames = new();

    [ObservableProperty]
    private GameSourceItem? _selectedGame;

    [ObservableProperty]
    private GameSourceDetail? _selectedGameDetails;

    [ObservableProperty]
    private CompatibilityVerification? _compatibility;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private bool _isLoadingDetails;

    [ObservableProperty]
    private string _statusMessage = "Listo";

    [ObservableProperty]
    private bool _showDetails; 

    [ObservableProperty]
    private string _currentSourceName;

    public SourcesViewModel()
    {
        // Register sources
        _services = new List<IGameSourceService>
        {
            new BlizzBoyGamesService(),
            new PiviGamesService()
        };
        // Default to first, but allows change
        _currentService = _services[0];
        _currentSourceName = _currentService.SourceName;

        var sysInfo = new SystemInfoViewModel();
        _ = sysInfo.InitializeAsync(); 
        _analyzer = new RequirementsAnalyzer(sysInfo);
    }

    public void SelectSource(string sourceName)
    {
        var service = _services.FirstOrDefault(s => s.SourceName == sourceName);
        if (service != null)
        {
            _currentService = service;
            CurrentSourceName = _currentService.SourceName;
        }
    }

    [RelayCommand]
    public async Task SwitchSourceAsync()
    {
        var index = _services.IndexOf(_currentService);
        index++;
        if (index >= _services.Count) index = 0;
        
        _currentService = _services[index];
        CurrentSourceName = _currentService.SourceName;
        
        await InitializeAsync();
    }

    [RelayCommand]
    public void GoBackToList()
    {
        ShowDetails = false;
        SelectedGameDetails = null;
        Compatibility = null;
    }

    [RelayCommand]
    public async Task OpenDetailsAsync()
    {
        if (SelectedGame == null) return;

        ShowDetails = true;
        IsLoadingDetails = true;
        
        // Fetch full details using current service
        var details = await _currentService.GetGameDetailsAsync(SelectedGame.Url);
        details.Title = SelectedGame.Title;
        details.ImageUrl = SelectedGame.ImageUrl;
        
        SelectedGameDetails = details;

        // Run Verification
        var minReq = SelectedGameDetails.Requirements.FirstOrDefault(r => r.Label.Contains("Mínimos"));
        if (minReq != null)
        {
            Compatibility = _analyzer.Verify(minReq);
        }

        IsLoadingDetails = false;
    }

    public async Task InitializeAsync()
    {
        IsLoading = true;
        StatusMessage = $"Obteniendo juegos de {CurrentSourceName}...";
        LatestGames.Clear();
        SelectedGame = null;

        var games = await _currentService.GetLatestGamesAsync();
        
        foreach (var game in games)
        {
            LatestGames.Add(game);
        }

        if (LatestGames.Any())
        {
            SelectedGame = LatestGames.First();
            StatusMessage = $"Se encontraron {LatestGames.Count} juegos";
        }
        else
        {
            StatusMessage = "No se encontraron juegos o hubo un error";
        }

        IsLoading = false;
    }
    
    [RelayCommand]
    public void OpenInBrowser()
    {
        if (SelectedGame != null && !string.IsNullOrEmpty(SelectedGame.Url))
        {
            OpenLink(SelectedGame.Url);
        }
    }

    [RelayCommand]
    public void OpenLink(string url)
    {
        if (string.IsNullOrEmpty(url)) return;
        try
        {
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = url,
                UseShellExecute = true
            };
            System.Diagnostics.Process.Start(psi);
        }
        catch { }
    }
}
