using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using GameLauncher.BigScreen.Input;
using GameLauncher.BigScreen.Navigation;
using GameLauncher.BigScreen.ViewModels;
using GameLauncher.BigScreen.Views;
using GameLauncher.Data.Cache;
using GameLauncher.Data.Xml;
using GameLauncher.Infrastructure.Services;
using Microsoft.Win32;

namespace GameLauncher.BigScreen;

/// <summary>
/// Lógica de la aplicación BigScreen (modo TV/fullscreen).
/// Configura dependency injection, gamepad y navegación.
/// </summary>
public partial class App : Application
{
    private const string LAUNCHBOX_PATH_KEY = "LaunchBoxPath";
    private GamepadController? _gamepadController;
    private BigScreenNavigationService? _navigationService;
    private IPlatformManager? _platformManager;
    private IGameManager? _gameManager;
    private XmlDataContext? _dataContext;
    private GameCacheManager? _cacheManager;
    private IEmulatorLauncher? _emulatorLauncher;
    private IStatisticsTracker? _statisticsTracker;
    private IPlaylistManager? _playlistManager;
    private string _lastGamepadEvent = "(ninguno)";

    // Previous stick positions for threshold-crossing navigation (non-PlatformFilters views)
    private float _prevStickLeftX, _prevStickLeftY;
    private const float StickDeadZone = 0.3f;
    private const float ScrollSpeed = 30f;

    public App()
    {
        DispatcherUnhandledException += App_DispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
        TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
    }

    private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        MessageBox.Show($"Error Fatal (Dispatcher):\n{e.Exception.Message}\n\n{e.Exception.StackTrace}", "Error Fatal", MessageBoxButton.OK, MessageBoxImage.Error);
        e.Handled = true; // Prevent crash if possible
    }

    private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        var ex = e.ExceptionObject as Exception;
        MessageBox.Show($"Error Fatal (Domain):\n{ex?.Message ?? "Unknown Error"}\n\n{ex?.StackTrace}", "Error Fatal", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
    {
        MessageBox.Show($"Error Fatal (Task):\n{e.Exception.Message}\n\n{e.Exception.StackTrace}", "Error Fatal", MessageBoxButton.OK, MessageBoxImage.Error);
        e.SetObserved();
    }

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

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
            GameLauncher.Core.Helpers.VideoPathResolver.LaunchBoxPath = launchBoxPath;

            // Inicializar servicios (compartidos con Desktop)
            _dataContext = new XmlDataContext(dataPath);
            _cacheManager = new GameCacheManager(_dataContext, dataPath);
            _emulatorLauncher = new EmulatorLauncher(_dataContext, launchBoxPath);
            _statisticsTracker = new StatisticsTracker(_dataContext, _cacheManager);
            _gameManager = new GameManager(_dataContext, _cacheManager);
            _platformManager = new PlatformManager(_dataContext, _statisticsTracker, _cacheManager);
            _playlistManager = new PlaylistManager(_dataContext);
            var settingsManager = new SettingsManager(_dataContext);

            // Cargar configuración de BigBox
            var bigBoxSettings = settingsManager.LoadBigBoxSettingsAsync().GetAwaiter().GetResult();

            // Crear ventana principal en modo fullscreen
            var mainWindow = new MainWindow
            {
                WindowState = WindowState.Maximized,
                WindowStyle = WindowStyle.None,  // Sin bordes ni barra de título
                ResizeMode = ResizeMode.NoResize,
                Topmost = false,  // TODO: Leer de bigBoxSettings.AlwaysOnTop cuando se implemente
                Title = "GameLauncher BigScreen"
            };

            // IMPORTANTE: Esperar a que la ventana se cargue antes de inicializar servicios de UI
            mainWindow.Loaded += async (s, args) =>
            {
                // Inicializar gamepad (opcional, funciona sin él)
                mainWindow.SetSplashStatus("Inicializando controles...");
                var gamepadStatus = TryInitializeGamepad();
                mainWindow.SetSplashStatus(gamepadStatus);

                await System.Threading.Tasks.Task.Delay(1000);

                // Inicializar navegación (siempre)
                try
                {
                    var navigationFrame = mainWindow.FindName("NavigationFrame") as Frame;
                    if (navigationFrame != null && _platformManager != null && _gameManager != null)
                    {
                        _navigationService = new BigScreenNavigationService(navigationFrame);

                        mainWindow.SetSplashStatus("Preparando interfaz...");

                        // Navigate to HomeView instead of PlatformFiltersView
                        var homeVM = CreateHomeViewModel(navigationFrame);
                        var homeView = new Views.HomeView { DataContext = homeVM };
                        navigationFrame.Navigate(homeView);

                        // Dismiss splash after navigation is ready
                        mainWindow.SetSplashStatus("Listo!");
                        await System.Threading.Tasks.Task.Delay(500);
                        mainWindow.DismissSplash();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Error al inicializar navegación:\n\n{ex.Message}",
                        "Error",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            };

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

    protected override void OnExit(ExitEventArgs e)
    {
        _gamepadController?.Stop();
        _gamepadController?.Dispose();

        base.OnExit(e);
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

        // 2. Intentar detectar automáticamente
        string? autoDetectedPath = TryAutoDetectLaunchBox();
        if (!string.IsNullOrEmpty(autoDetectedPath))
        {
            SavePath(autoDetectedPath);
            return autoDetectedPath;
        }

        // 3. En BigScreen, si no se encuentra, no preguntar al usuario
        // (modo TV no tiene mouse/teclado fácilmente disponible)
        // En su lugar, mostrar mensaje de error y cerrar
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
            @"E:\LaunchBox",
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

    /// <summary>
    /// Método separado para que si XInput DLL no existe, el JIT falle solo aquí
    /// y no afecte al resto de la inicialización.
    /// </summary>
    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.NoInlining)]
    private string TryInitializeGamepad()
    {
        try
        {
            _gamepadController = new GamepadController(); // Auto-detect player index

            // Navegación (D-Pad)
            _gamepadController.NavigateUp += OnGamepadNavigateUp;
            _gamepadController.NavigateDown += OnGamepadNavigateDown;
            _gamepadController.NavigateLeft += OnGamepadNavigateLeft;
            _gamepadController.NavigateRight += OnGamepadNavigateRight;

            // Sticks analógicos (scroll continuo)
            _gamepadController.LeftStickAxis += OnLeftStickAxis;
            _gamepadController.RightStickAxis += OnRightStickAxis;

            // Acciones (B=Select, Y=Back, X=Images, A=Manage, RT=Play, LB/RB=Pages)
            _gamepadController.SelectPressed += OnGamepadSelect;
            _gamepadController.BackPressed += OnGamepadBack;
            _gamepadController.ImagesPressed += OnGamepadImages;
            _gamepadController.ManagePressed += OnGamepadManage;
            _gamepadController.PlayPressed += OnGamepadPlay;
            _gamepadController.PageUpPressed += OnGamepadPageUp;
            _gamepadController.PageDownPressed += OnGamepadPageDown;
            _gamepadController.KillComboPressed += OnGamepadKillCombo;

            _gamepadController.Start();

            var diag = _gamepadController.GetDiagnosticInfo();
            System.Diagnostics.Debug.WriteLine($"[App] Gamepad init OK:\n{diag}");

            if (_gamepadController.IsConnected)
                return $"Gamepad OK (index {_gamepadController.PlayerIndex})";
            else
                return $"Gamepad NO detectado.\n{diag}";
        }
        catch (DllNotFoundException)
        {
            _gamepadController = null;
            return "ERROR: xinput1_4.dll no encontrado";
        }
        catch (Exception ex)
        {
            _gamepadController = null;
            return $"ERROR gamepad: {ex.Message}";
        }
    }

    private void TrackEvent(string eventName)
    {
        _lastGamepadEvent = $"{eventName} [{DateTime.Now:HH:mm:ss.fff}]";
        System.Diagnostics.Debug.WriteLine($"[Gamepad] {_lastGamepadEvent}");
    }

    private object? GetCurrentViewModel()
    {
        if (MainWindow is not MainWindow mainWin)
        {
            System.Diagnostics.Debug.WriteLine("[App] GetCurrentViewModel: MainWindow is null or wrong type");
            return null;
        }
        var frame = mainWin.FindName("NavigationFrame") as System.Windows.Controls.Frame;
        if (frame == null)
        {
            System.Diagnostics.Debug.WriteLine("[App] GetCurrentViewModel: NavigationFrame not found");
            return null;
        }
        var content = frame.Content;
        System.Diagnostics.Debug.WriteLine($"[App] GetCurrentViewModel: frame.Content type = {content?.GetType().Name ?? "null"}");
        var page = content as System.Windows.Controls.Page;
        if (page == null && content != null)
        {
            // Si el content no es Page, intentar obtener DataContext directamente del FrameworkElement
            if (content is System.Windows.FrameworkElement fe)
            {
                System.Diagnostics.Debug.WriteLine($"[App] GetCurrentViewModel: using FrameworkElement.DataContext, VM type = {fe.DataContext?.GetType().Name ?? "null"}");
                return fe.DataContext;
            }
            System.Diagnostics.Debug.WriteLine("[App] GetCurrentViewModel: content is not Page nor FrameworkElement");
            return null;
        }
        System.Diagnostics.Debug.WriteLine($"[App] GetCurrentViewModel: VM type = {page?.DataContext?.GetType().Name ?? "null"}");
        return page?.DataContext;
    }

    // ── Helper: detecta si hay un overlay abierto en GamesWheel ──

    private bool IsGamesWheelOverlayActive(GamesWheelViewModel gvm)
        => gvm.ShowingManageMenu || gvm.ShowingGallery || gvm.ShowingZoomedImage || gvm.ShowingEditor;

    // ── Navegación D-Pad / Stick ──

    private void OnGamepadNavigateUp()
    {
        TrackEvent("D-Pad Up");
        var vm = GetCurrentViewModel();
        if (vm is PlatformFiltersViewModel platformVM)
            platformVM.NavigateUpCommand.Execute(null);
        else if (vm is GamesWheelViewModel gamesVM)
        {
            if (gamesVM.ShowingEditor)
                gamesVM.EditorNavigateUp();
            else if (gamesVM.ShowingManageMenu)
                gamesVM.ManageNavigateUp();
            else if (gamesVM.ShowingGallery)
                gamesVM.GalleryNavigate(0, -1);
            else
                gamesVM.NavigateLeftCommand.Execute(null);
        }
    }

    private void OnGamepadNavigateDown()
    {
        TrackEvent("D-Pad Down");
        var vm = GetCurrentViewModel();
        if (vm is PlatformFiltersViewModel platformVM)
            platformVM.NavigateDownCommand.Execute(null);
        else if (vm is GamesWheelViewModel gamesVM)
        {
            if (gamesVM.ShowingEditor)
                gamesVM.EditorNavigateDown();
            else if (gamesVM.ShowingManageMenu)
                gamesVM.ManageNavigateDown();
            else if (gamesVM.ShowingGallery)
                gamesVM.GalleryNavigate(0, 1);
            else
                gamesVM.NavigateRightCommand.Execute(null);
        }
    }

    private void OnGamepadNavigateLeft()
    {
        TrackEvent("D-Pad Left");
        var vm = GetCurrentViewModel();
        if (vm is HomeViewModel homeVM)
            homeVM.NavigateLeftCommand.Execute(null);
        else if (vm is PlatformFiltersViewModel)
        {
            GetPlatformFiltersView()?.SeekVideo(-5);
        }
        else if (vm is GamesWheelViewModel gamesVM)
        {
            if (gamesVM.ShowingEditor)
                gamesVM.EditorNavigateLeft();
            else if (gamesVM.ShowingManageMenu) return;
            else if (gamesVM.ShowingGallery)
                gamesVM.GalleryNavigate(-1, 0);
            else
                gamesVM.NavigateLeftCommand.Execute(null);
        }
    }

    private void OnGamepadNavigateRight()
    {
        TrackEvent("D-Pad Right");
        var vm = GetCurrentViewModel();
        if (vm is HomeViewModel homeVM)
            homeVM.NavigateRightCommand.Execute(null);
        else if (vm is PlatformFiltersViewModel)
        {
            GetPlatformFiltersView()?.SeekVideo(5);
        }
        else if (vm is GamesWheelViewModel gamesVM)
        {
            if (gamesVM.ShowingEditor)
                gamesVM.EditorNavigateRight();
            else if (gamesVM.ShowingManageMenu) return;
            else if (gamesVM.ShowingGallery)
                gamesVM.GalleryNavigate(1, 0);
            else
                gamesVM.NavigateRightCommand.Execute(null);
        }
    }

    // ── Sticks analógicos (scroll continuo) ──

    private void OnLeftStickAxis(float x, float y)
    {
        var vm = GetCurrentViewModel();
        if (vm is PlatformFiltersViewModel)
        {
            // Left stick Y → scroll description (negative Y = push down = scroll down)
            if (Math.Abs(y) > StickDeadZone)
                GetPlatformFiltersView()?.ScrollDescription(-y * ScrollSpeed);
        }
        else
        {
            // For other views: replicate threshold-crossing → navigate
            if (y > StickDeadZone && _prevStickLeftY <= StickDeadZone)
                OnGamepadNavigateUp();
            if (y < -StickDeadZone && _prevStickLeftY >= -StickDeadZone)
                OnGamepadNavigateDown();
            if (x < -StickDeadZone && _prevStickLeftX >= -StickDeadZone)
                OnGamepadNavigateLeft();
            if (x > StickDeadZone && _prevStickLeftX <= StickDeadZone)
                OnGamepadNavigateRight();
        }
        _prevStickLeftX = x;
        _prevStickLeftY = y;
    }

    private void OnRightStickAxis(float x, float y)
    {
        var vm = GetCurrentViewModel();
        if (vm is PlatformFiltersViewModel)
        {
            // Right stick X → scroll game covers horizontally (positive X = push right = scroll right)
            if (Math.Abs(x) > StickDeadZone)
                GetPlatformFiltersView()?.ScrollCovers(x * ScrollSpeed);
        }
    }

    private Views.PlatformFiltersView? GetPlatformFiltersView()
    {
        if (MainWindow is not MainWindow mainWin) return null;
        var frame = mainWin.FindName("NavigationFrame") as System.Windows.Controls.Frame;
        return frame?.Content as Views.PlatformFiltersView;
    }

    // ── B = Seleccionar / Confirmar ──

    private void OnGamepadSelect()
    {
        TrackEvent("B (Seleccionar)");
        var vm = GetCurrentViewModel();
        if (vm is HomeViewModel homeVM)
            _ = homeVM.NavigateToSelectedCardCommand.ExecuteAsync(null);
        else if (vm is PlatformFiltersViewModel platformVM)
            platformVM.SelectItemCommand.Execute(null);
        else if (vm is GamesWheelViewModel gamesVM)
        {
            if (gamesVM.ShowingEditor)
                gamesVM.EditorConfirm();
            else if (gamesVM.ShowingManageMenu)
                gamesVM.ManageToggleSelected();
            else if (gamesVM.ShowingGallery)
                gamesVM.GallerySelect();
            else
                gamesVM.ShowDetailsCommand.Execute(null);
        }
    }

    // ── Y = Volver atrás ──

    private void OnGamepadBack()
    {
        TrackEvent("Y (Volver)");
        var vm = GetCurrentViewModel();

        if (vm is GamesWheelViewModel gamesVM)
        {
            if (gamesVM.ShowingEditor)
            {
                gamesVM.EditorCancelField();
                return;
            }
            if (gamesVM.ShowingManageMenu)
            {
                gamesVM.CloseManageMenu();
                return;
            }
            if (gamesVM.ShowingGallery || gamesVM.ShowingZoomedImage)
            {
                gamesVM.GalleryBack();
                return;
            }
        }

        if (vm is PlatformFiltersViewModel platformVM && platformVM.CanGoBack)
        {
            platformVM.GoBackCommand.Execute(null);
            return;
        }

        if (MainWindow is MainWindow mainWin)
        {
            var frame = mainWin.FindName("NavigationFrame") as System.Windows.Controls.Frame;
            if (frame != null && frame.CanGoBack)
            {
                frame.GoBack();
            }
        }
    }

    // ── X = Ver imágenes del juego ──

    private void OnGamepadImages()
    {
        TrackEvent("X (Imagenes)");
        var vm = GetCurrentViewModel();
        if (vm is GamesWheelViewModel gamesVM && !IsGamesWheelOverlayActive(gamesVM))
            gamesVM.ShowImagesCommand.Execute(null);
        else if (vm is GameDetailsViewModel detailsVM)
            detailsVM.ShowImagesCommand.Execute(null);
    }

    // ── A = Gestionar juego (menu de etiquetas) ──

    private void OnGamepadManage()
    {
        TrackEvent("A (Gestionar)");
        var vm = GetCurrentViewModel();
        if (vm is GamesWheelViewModel gamesVM)
        {
            if (gamesVM.ShowingEditor)
                _ = gamesVM.EditorSaveAsync();
            else if (!IsGamesWheelOverlayActive(gamesVM))
                gamesVM.OpenManageMenuCommand.Execute(null);
        }
        else if (vm is GameDetailsViewModel detailsVM)
            detailsVM.ToggleFavoriteCommand.Execute(null);
    }

    // ── RT = Ejecutar juego ──

    private void OnGamepadPlay()
    {
        TrackEvent("RT (Ejecutar)");
        var vm = GetCurrentViewModel();
        if (vm is GamesWheelViewModel gamesVM)
            gamesVM.LaunchGameCommand.Execute(null);
        else if (vm is GameDetailsViewModel detailsVM)
            detailsVM.LaunchGameCommand.Execute(null);
    }

    // ── Select+Start = Matar emulador/juego en ejecución ──

    private void OnGamepadKillCombo()
    {
        TrackEvent("Select+Start (Kill)");
        _emulatorLauncher?.KillCurrentProcess();
    }

    // ── LB/RB = Página arriba/abajo ──

    private void OnGamepadPageUp()
    {
        TrackEvent("LB (Pag Arriba)");
        var vm = GetCurrentViewModel();
        if (vm is SystemInfoViewModel systemVM)
            systemVM.PreviousSection();
        else if (vm is PlatformFiltersViewModel platformVM)
            platformVM.PageUpCommand.Execute(null);
        else if (vm is GamesWheelViewModel gamesVM)
        {
            if (gamesVM.ShowingEditor)
                gamesVM.EditorPreviousSection();
            else
                gamesVM.PageLeftCommand.Execute(null);
        }
    }

    private void OnGamepadPageDown()
    {
        TrackEvent("RB (Pag Abajo)");
        var vm = GetCurrentViewModel();
        if (vm is SystemInfoViewModel systemVM)
            systemVM.NextSection();
        else if (vm is PlatformFiltersViewModel platformVM)
            platformVM.PageDownCommand.Execute(null);
        else if (vm is GamesWheelViewModel gamesVM)
        {
            if (gamesVM.ShowingEditor)
                gamesVM.EditorNextSection();
            else
                gamesVM.PageRightCommand.Execute(null);
        }
    }

    public GamesWheelViewModel CreateGamesWheelViewModel()
    {
        if (_cacheManager == null || _emulatorLauncher == null || _statisticsTracker == null || _gameManager == null || _dataContext == null)
            throw new InvalidOperationException("Services not initialized");

        return new GamesWheelViewModel(_cacheManager, _emulatorLauncher, _statisticsTracker, _gameManager, _dataContext);
    }

    public GameDetailsViewModel CreateGameDetailsViewModel()
    {
        if (_emulatorLauncher == null || _statisticsTracker == null)
            throw new InvalidOperationException("Services not initialized");

        return new GameDetailsViewModel(_emulatorLauncher, _statisticsTracker);
    }

    public PlatformFiltersViewModel CreatePlatformFiltersViewModel()
    {
        if (_platformManager == null || _gameManager == null)
            throw new InvalidOperationException("Services not initialized");

        return new PlatformFiltersViewModel(_platformManager, _gameManager, _playlistManager);
    }

    public HomeViewModel CreateHomeViewModel(Frame navigationFrame)
    {
        if (_platformManager == null || _gameManager == null)
            throw new InvalidOperationException("Services not initialized");

        return new HomeViewModel(navigationFrame, _platformManager, _gameManager, _playlistManager);
    }

    public SystemInfoViewModel CreateSystemInfoViewModel()
    {
        return new SystemInfoViewModel();
    }

}

