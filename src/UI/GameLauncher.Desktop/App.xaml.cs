using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using GameLauncher.Data.Cache;
using GameLauncher.Data.Xml;
using GameLauncher.Desktop.ViewModels;
using GameLauncher.Infrastructure.Services;
using Microsoft.Win32;

namespace GameLauncher.Desktop;

/// <summary>
/// Lógica de la aplicación principal.
/// Configura la inyección de dependencias y los servicios.
/// </summary>
public partial class App : Application
{
    private const string LAUNCHBOX_PATH_KEY = "LaunchBoxPath";

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Manejar excepciones no controladas
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        DispatcherUnhandledException += OnDispatcherUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;

        try
        {
            // Obtener la ruta de LaunchBox
            string launchBoxPath = GetLaunchBoxPath();

            if (string.IsNullOrEmpty(launchBoxPath))
            {
                MessageBox.Show(
                    "No se pudo encontrar la ruta de LaunchBox.\nLa aplicación se cerrará.",
                    "Error de Configuración",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                Shutdown();
                return;
            }

            // Resolver ruta de datos local (XMLs copiados al proyecto)
            string dataPath = GameLauncher.Core.Helpers.DataPathResolver.FindProjectDataPath()
                ?? Path.Combine(launchBoxPath, "Data");

            // Media (imágenes, videos) sigue en LaunchBox
            GameLauncher.Desktop.ViewModels.GameViewModel.LaunchBoxPath = launchBoxPath;
            GameLauncher.Core.Helpers.VideoPathResolver.LaunchBoxPath = launchBoxPath;

            // Inicializar servicios
            var dataContext = new XmlDataContext(dataPath);
            var cacheManager = new GameCacheManager(dataContext, dataPath);

            var emulatorLauncher = new EmulatorLauncher(dataContext, launchBoxPath);
            var statisticsTracker = new StatisticsTracker(dataContext, cacheManager);
            var gameManager = new GameManager(dataContext, cacheManager);
            var platformManager = new PlatformManager(dataContext, statisticsTracker, cacheManager);
            var settingsManager = new SettingsManager(dataContext);
            var playlistManager = new PlaylistManager(dataContext);
            var exportService = new AndroidExportService(dataContext, launchBoxPath);
            var syncServer = new LocalSyncServer();

            // Crear ViewModel principal
            var mainViewModel = new MainViewModel(
                dataContext,
                cacheManager,
                emulatorLauncher,
                statisticsTracker,
                gameManager,
                platformManager,
                playlistManager,
                exportService,
                syncServer);

            // Crear y mostrar la ventana principal
            var mainWindow = new MainWindow(mainViewModel);
            mainWindow.Show();
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Error al iniciar la aplicación:\n\n{ex.Message}",
                "Error Fatal",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown();
        }
    }

    private string GetLaunchBoxPath()
    {
        // 1. Intentar cargar desde configuración guardada
        string? savedPath = LoadSavedPath();
        if (!string.IsNullOrEmpty(savedPath) && Directory.Exists(savedPath))
        {
            var dataDir = Path.Combine(savedPath, "Data");
            if (Directory.Exists(dataDir))
                return savedPath;
        }

        // 2. Intentar detectar automáticamente (buscar en ubicaciones comunes)
        string? autoDetectedPath = TryAutoDetectLaunchBox();
        if (!string.IsNullOrEmpty(autoDetectedPath))
        {
            SavePath(autoDetectedPath);
            return autoDetectedPath;
        }

        // 3. Preguntar al usuario
        string? userSelectedPath = AskUserForPath();
        if (!string.IsNullOrEmpty(userSelectedPath))
        {
            SavePath(userSelectedPath);
            return userSelectedPath;
        }

        return string.Empty;
    }

    private string? TryAutoDetectLaunchBox()
    {
        // Buscar en ubicaciones comunes
        string[] commonPaths = new[]
        {
            @"H:\LaunchBox\LaunchBox",
            @"C:\LaunchBox",
            @"D:\LaunchBox",
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "LaunchBox"),
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "LaunchBox")
        };

        foreach (var path in commonPaths)
        {
            if (Directory.Exists(path))
            {
                var dataDir = Path.Combine(path, "Data");
                if (Directory.Exists(dataDir))
                    return path;
            }
        }

        return null;
    }

    private string? AskUserForPath()
    {
        var result = MessageBox.Show(
            "No se pudo detectar LaunchBox automáticamente.\n\n" +
            "¿Deseas seleccionar manualmente la carpeta de LaunchBox?",
            "Seleccionar LaunchBox",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            var dialog = new OpenFolderDialog
            {
                Title = "Selecciona la carpeta raíz de LaunchBox",
                InitialDirectory = "C:\\"
            };

            if (dialog.ShowDialog() == true)
            {
                string selectedPath = dialog.FolderName;
                var dataDir = Path.Combine(selectedPath, "Data");

                if (Directory.Exists(dataDir))
                    return selectedPath;

                MessageBox.Show(
                    "La carpeta seleccionada no parece ser una instalación válida de LaunchBox.\n" +
                    "Debe contener una subcarpeta 'Data'.",
                    "Carpeta Inválida",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        return null;
    }

    private string? LoadSavedPath()
    {
        try
        {
            using var key = Registry.CurrentUser.CreateSubKey(@"Software\GameLauncher");
            return key?.GetValue(LAUNCHBOX_PATH_KEY) as string;
        }
        catch
        {
            return null;
        }
    }

    private void SavePath(string path)
    {
        try
        {
            using var key = Registry.CurrentUser.CreateSubKey(@"Software\GameLauncher");
            key?.SetValue(LAUNCHBOX_PATH_KEY, path);
        }
        catch
        {
            // Ignorar errores al guardar
        }
    }

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var ex = e.ExceptionObject as Exception;
        MessageBox.Show(
            $"Error no controlado:\n\n{ex?.Message}\n\nStack Trace:\n{ex?.StackTrace}",
            "Error Fatal",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
    }

    private void OnDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        MessageBox.Show(
            $"Error en UI Thread:\n\n{e.Exception.Message}\n\nStack Trace:\n{e.Exception.StackTrace}",
            "Error de Interfaz",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
        e.Handled = true; // Prevent application from crashing
    }

    private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        MessageBox.Show(
            $"Error en Task:\n\n{e.Exception.InnerException?.Message}\n\nStack Trace:\n{e.Exception.InnerException?.StackTrace}",
            "Error Asíncrono",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
        e.SetObserved(); // Prevent application from crashing
    }
}
