using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using GameLauncher.Android.Services;
using GameLauncher.Core.Models;

namespace GameLauncher.Android.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private readonly AndroidImportService _importService;
        private readonly AndroidEmulatorLauncher _emulatorLauncher;

        public ObservableCollection<AndroidExportedGame> Games { get; set; } = new ObservableCollection<AndroidExportedGame>();

        private string _ipAddress = "192.168.1.";
        public string IpAddress
        {
            get => _ipAddress;
            set { _ipAddress = value; OnPropertyChanged(); }
        }

        private string _statusText = "Listo";
        public string StatusText
        {
            get => _statusText;
            set { _statusText = value; OnPropertyChanged(); }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set { _isBusy = value; OnPropertyChanged(); }
        }

        public ICommand ImportCommand { get; }
        public ICommand PlayCommand { get; }

        public MainViewModel()
        {
            _importService = new AndroidImportService();
            _emulatorLauncher = new AndroidEmulatorLauncher();

            ImportCommand = new Command(async () => await ImportLibraryAsync());
            PlayCommand = new Command<AndroidExportedGame>(async (g) => await PlayGameAsync(g));

            LoadExistingLibraryAsync();
        }

        private async void LoadExistingLibraryAsync()
        {
            var jsonPath = Path.Combine(FileSystem.AppDataDirectory, "ImportedLibrary", "AndroidLibrary.json");
            if (File.Exists(jsonPath))
            {
                var json = await File.ReadAllTextAsync(jsonPath);
                var manifest = JsonSerializer.Deserialize<AndroidLibraryManifest>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (manifest != null && manifest.Games != null)
                {
                    Games.Clear();
                    foreach (var g in manifest.Games) Games.Add(g);
                    StatusText = $"{Games.Count} juegos cargados de la biblioteca local.";
                }
            }
        }

        private async Task ImportLibraryAsync()
        {
            if (string.IsNullOrWhiteSpace(IpAddress))
            {
                StatusText = "Ingresa la IP de tu PC";
                return;
            }

            IsBusy = true;
            try
            {
                var manifest = await _importService.ImportFromLocalServerAsync(IpAddress, msg => 
                {
                    MainThread.BeginInvokeOnMainThread(() => StatusText = msg);
                });

                if (manifest != null && manifest.Games != null)
                {
                    Games.Clear();
                    foreach (var g in manifest.Games) Games.Add(g);
                    StatusText = $"¡Importación exitosa! {Games.Count} juegos sincronizados.";
                }
            }
            catch (Exception ex)
            {
                StatusText = $"Error: {ex.Message}";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task PlayGameAsync(AndroidExportedGame game)
        {
            if (game == null) return;

            if (!game.IsRetroArch)
            {
                await Application.Current.MainPage.DisplayAlert("Sólo PC", "Este juego no utiliza RetroArch y solo se puede jugar en tu computadora.", "OK");
                return;
            }

            var libraryDir = Path.Combine(FileSystem.AppDataDirectory, "ImportedLibrary");
            var result = await _emulatorLauncher.LaunchGameAsync(game, libraryDir);

            if (!result.Success)
            {
                await Application.Current.MainPage.DisplayAlert("Error", result.Message, "OK");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
