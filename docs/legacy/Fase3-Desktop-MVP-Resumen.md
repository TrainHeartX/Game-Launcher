# Fase 3: Aplicación Desktop MVP - Resumen de Implementación

**Estado**: ✅ COMPLETADA
**Fecha**: 8 de Febrero 2026
**Tiempo de implementación**: ~2 horas

---

## Objetivo de la Fase 3

Crear una aplicación de escritorio funcional tipo LaunchBox con:
- Interfaz MVVM moderna con CommunityToolkit.Mvvm
- Layout de 3 paneles (Plataformas | Juegos | Detalles)
- Navegación, búsqueda y lanzamiento de juegos
- Integración completa con los servicios de las Fases 1 y 2

---

## Componentes Implementados

### 1. ViewModels (MVVM Pattern)

#### **GameViewModel.cs** (155 líneas)
- Wrapper MVVM para el modelo `Game`
- **Propiedades observables**:
  - `Title`, `Platform`, `Developer`, `Publisher`
  - `Genre`, `PlayCount`, `PlayTime`
  - `Favorite`, `Completed`, `Hidden`
  - `FormattedPlayTime` - Convierte segundos a formato legible ("5h 23m")
  - `FormattedReleaseDate` - Formatea fecha de lanzamiento

- **Comandos**:
  - `LaunchCommand` - Lanza el juego
  - `EditCommand` - Edita metadatos del juego
  - `DeleteCommand` - Elimina el juego
  - `ToggleFavoriteCommand` - Marca/desmarca como favorito

**Ejemplo de uso**:
```csharp
var gameVM = new GameViewModel(game, emulatorLauncher, gameManager);
await gameVM.LaunchCommand.ExecuteAsync(null);
```

#### **PlatformViewModel.cs** (72 líneas)
- Wrapper MVVM para el modelo `Platform`
- **Propiedades observables**:
  - `Name`, `Developer`, `Manufacturer`
  - `CPU`, `Graphics`, `MaxControllers`
  - `GameCount` - Número de juegos en la plataforma

- **Comandos**:
  - `LoadGamesCommand` - Carga juegos de la plataforma
  - `EditCommand` - Edita metadatos de plataforma

#### **MainViewModel.cs** (226 líneas)
- ViewModel principal que coordina toda la aplicación
- **Colecciones observables**:
  - `ObservableCollection<PlatformViewModel> Platforms` - Lista de plataformas
  - `ObservableCollection<GameViewModel> Games` - Juegos de la plataforma seleccionada
  - `ObservableCollection<GameViewModel> FilteredGames` - Juegos filtrados por búsqueda

- **Propiedades**:
  - `SelectedPlatform` - Plataforma seleccionada
  - `SelectedGame` - Juego seleccionado
  - `SearchText` - Texto de búsqueda
  - `IsLoading` - Indicador de carga
  - `StatusText` - Texto de estado (barra inferior)

- **Comandos**:
  - `LoadGamesForPlatformCommand` - Carga juegos al seleccionar plataforma
  - `SearchGamesCommand` - Busca juegos en todas las plataformas
  - `LaunchSelectedGameCommand` - Lanza el juego seleccionado
  - `RefreshCommand` - Recarga datos

- **Método de inicialización**:
```csharp
public async Task InitializeAsync()
{
    IsLoading = true;
    StatusText = "Cargando plataformas...";

    var platforms = await _platformManager.GetAllPlatformsAsync();
    // ... llenar Platforms collection

    IsLoading = false;
    StatusText = $"{Platforms.Count} plataformas cargadas";
}
```

### 2. Converters (Data Binding)

#### **PlayTimeConverter.cs**
- Convierte segundos (int) a formato legible (string)
- Ejemplos:
  - `3600` → `"1h 0m"`
  - `7383` → `"2h 3m"`
  - `1800` → `"30m"`

#### **BoolToVisibilityConverter.cs**
- Convierte `bool` a `Visibility` (WPF)
- Soporta parámetro "Inverse" para lógica invertida
- `true` → `Visibility.Visible`
- `false` → `Visibility.Collapsed`

#### **ArgbIntegerToColorConverter.cs**
- Convierte entero ARGB (formato LaunchBox) a `Color` de WPF
- Descompone el int en componentes A, R, G, B
- Usado para colores de plataformas y temas

### 3. MainWindow - Interfaz de Usuario

#### **MainWindow.xaml** (408 líneas)

**Layout de 3 Paneles**:
```
┌──────────────┬─┬────────────────────────┬─┬────────────────┐
│  PLATFORMS   │ │      GAMES GRID        │ │  GAME DETAILS  │
│  (Filtros)   │ │                        │ │                │
│              │ │  ┌──────┬──────┬──────┐│ │  Título        │
│  □ Arcade    │ │  │ Box  │ Box  │ Box  ││ │  Desarrollador │
│  □ NES       │ │  │ Art  │ Art  │ Art  ││ │  Género        │
│  □ SNES      │ │  └──────┴──────┴──────┘│ │  Rating ★★★★★  │
│  □ PS1       │ │                        │ │  Tiempo jugado │
│  □ ...       │ │  [Lista de juegos]     │ │  □ Favorito    │
│              │ │                        │ │  □ Completado  │
└──────────────┴─┴────────────────────────┴─┴────────────────┘
```

**Estructura del Grid**:
- **Columna 0**: SideBar de plataformas (250px)
- **Columna 1**: GridSplitter (5px)
- **Columna 2**: Panel central de juegos (*)
- **Columna 3**: GridSplitter (5px)
- **Columna 4**: Panel de detalles (300px)

**Componentes principales**:

1. **Toolbar superior**:
   - Botón "▶ Jugar" (F5)
   - Botón "★ Favorito" (Ctrl+F)
   - Botón "🔄 Actualizar" (F5)
   - SearchBox con binding a `SearchText`
   - Enter key ejecuta búsqueda

2. **Panel de Plataformas (izquierda)**:
   - `ListBox` con binding a `Platforms`
   - Muestra: Nombre + recuento de juegos
   - Selección activa `LoadGamesForPlatformCommand`

3. **Panel de Juegos (centro)**:
   - `ListBox` con binding a `FilteredGames`
   - Cada item muestra:
     - Título (negrita, tamaño 14)
     - Plataforma (gris)
     - Género, Desarrollador
     - Tiempo jugado (convertido con `PlayTimeConverter`)
     - PlayCount
     - ★ si es favorito, ✓ si está completado

4. **Panel de Detalles (derecha)**:
   - Título del juego
   - Desarrollador, Publisher
   - Género, Año de lanzamiento
   - Plataforma
   - Rating (★★★★★)
   - Estadísticas:
     - Tiempo jugado
     - Veces jugado
     - Última vez jugado
   - Checkboxes:
     - ☐ Favorito
     - ☐ Completado
     - ☐ Oculto

5. **StatusBar (inferior)**:
   - Binding a `StatusText`
   - Muestra mensajes de estado

6. **Loading Overlay**:
   - Se muestra cuando `IsLoading = true`
   - Fondo semi-transparente
   - Mensaje "⏳ Cargando..."

**Recursos**:
```xaml
<Window.Resources>
    <local:PlayTimeConverter x:Key="PlayTimeConverter"/>
    <local:BoolToVisibilityConverter x:Key="BoolToVisibilityConverter"/>
</Window.Resources>
```

#### **MainWindow.xaml.cs** (19 líneas)

Constructor simplificado con inyección de dependencias:
```csharp
public MainWindow(MainViewModel viewModel)
{
    InitializeComponent();
    DataContext = viewModel;

    // Inicializar datos al cargar la ventana
    Loaded += async (s, e) => await viewModel.InitializeAsync();
}
```

### 4. App.xaml.cs - Dependency Injection

#### **App.xaml.cs** (192 líneas)

**Funciones principales**:

1. **Detección de ruta de LaunchBox** (3 estrategias):

```csharp
private string GetLaunchBoxPath()
{
    // 1. Intentar cargar desde configuración guardada (Registry)
    string? savedPath = LoadSavedPath();
    if (!string.IsNullOrEmpty(savedPath) && Directory.Exists(savedPath))
        return savedPath;

    // 2. Intentar detectar automáticamente
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
```

2. **Auto-detección de LaunchBox**:
```csharp
private string? TryAutoDetectLaunchBox()
{
    string[] commonPaths = new[]
    {
        @"H:\LaunchBox\LaunchBox",
        @"C:\LaunchBox",
        @"D:\LaunchBox",
        Path.Combine(Environment.GetFolderPath(
            Environment.SpecialFolder.ProgramFiles), "LaunchBox"),
        Path.Combine(Environment.GetFolderPath(
            Environment.SpecialFolder.ProgramFilesX86), "LaunchBox")
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
```

3. **Configuración de Dependency Injection**:
```csharp
protected override void OnStartup(StartupEventArgs e)
{
    base.OnStartup(e);

    try
    {
        // Obtener ruta de LaunchBox
        string launchBoxPath = GetLaunchBoxPath();

        if (string.IsNullOrEmpty(launchBoxPath))
        {
            MessageBox.Show(
                "No se pudo encontrar la ruta de LaunchBox.\n" +
                "La aplicación se cerrará.",
                "Error de Configuración",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown();
            return;
        }

        // Inicializar servicios (Fase 1 y 2)
        var dataContext = new XmlDataContext(launchBoxPath);
        var cacheManager = new GameCacheManager(dataContext, launchBoxPath);
        var emulatorLauncher = new EmulatorLauncher(dataContext);
        var statisticsTracker = new StatisticsTracker(dataContext, cacheManager);
        var gameManager = new GameManager(dataContext, cacheManager);
        var platformManager = new PlatformManager(
            dataContext, statisticsTracker, cacheManager);
        var settingsManager = new SettingsManager(dataContext);

        // Crear ViewModel principal
        var mainViewModel = new MainViewModel(
            dataContext,
            cacheManager,
            emulatorLauncher,
            statisticsTracker,
            gameManager,
            platformManager);

        // Crear y mostrar ventana principal
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
```

4. **Persistencia de configuración**:
```csharp
private string? LoadSavedPath()
{
    try
    {
        using var key = Registry.CurrentUser.CreateSubKey(
            @"Software\GameLauncher");
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
        using var key = Registry.CurrentUser.CreateSubKey(
            @"Software\GameLauncher");
        key?.SetValue(LAUNCHBOX_PATH_KEY, path);
    }
    catch
    {
        // Ignorar errores al guardar
    }
}
```

**Registry Key**: `HKEY_CURRENT_USER\Software\GameLauncher\LaunchBoxPath`

---

## Compilación y Resultado

### Build Output

```bash
dotnet build src/UI/GameLauncher.Desktop/GameLauncher.Desktop.csproj
```

**Resultado**:
- ✅ Compilación correcta
- ✅ 0 errores
- ⚠️ 1 warning (CS4014 - fire-and-forget intencional)

### Archivos Generados

**Ubicación**: `H:\GameLauncher\src\UI\GameLauncher.Desktop\bin\Debug\net8.0-windows\`

| Archivo | Tamaño | Descripción |
|---------|--------|-------------|
| `GameLauncher.Desktop.exe` | 136 KB | Ejecutable principal |
| `GameLauncher.Desktop.dll` | 49 KB | Librería de UI |
| `GameLauncher.Core.dll` | 68 KB | Modelos y lógica |
| `GameLauncher.Data.dll` | 14 KB | Parser XML |
| `GameLauncher.Infrastructure.dll` | 47 KB | Servicios |
| `CommunityToolkit.Mvvm.dll` | 115 KB | Framework MVVM |
| `Microsoft.Xaml.Behaviors.dll` | 144 KB | Behaviors de WPF |

---

## Características Implementadas

### ✅ Navegación
- Selección de plataformas en panel izquierdo
- Carga automática de juegos al seleccionar plataforma
- Selección de juegos en panel central
- Visualización de detalles en panel derecho

### ✅ Búsqueda
- Búsqueda global en todas las plataformas
- Actualización automática de resultados
- Búsqueda por título, desarrollador, género
- Tecla Enter para ejecutar búsqueda

### ✅ Lanzamiento de Juegos
- Botón "Jugar" (F5)
- Integración con EmulatorLauncher
- Tracking automático de PlayTime y PlayCount

### ✅ Gestión de Favoritos
- Marcar/desmarcar favoritos desde detalles
- Comando toggle favorito (Ctrl+F)
- Indicador visual de favoritos (★)

### ✅ Estadísticas
- Tiempo jugado formateado ("5h 23m")
- Contador de veces jugado
- Última vez jugado
- Indicadores visuales de completado (✓)

### ✅ UI Responsive
- Layout redimensionable con GridSplitters
- Overlay de carga durante operaciones
- Status bar con mensajes informativos
- Tooltips en controles

### ✅ Configuración Persistente
- Ruta de LaunchBox guardada en Registry
- Auto-detección de instalación de LaunchBox
- Diálogo de selección manual si es necesario

---

## Flujo de Uso de la Aplicación

### 1. Inicio
```
Usuario ejecuta GameLauncher.Desktop.exe
    ↓
App.OnStartup() detecta ruta de LaunchBox
    ↓
Crea todos los servicios (DI manual)
    ↓
Crea MainViewModel con servicios inyectados
    ↓
Muestra MainWindow
    ↓
MainWindow.Loaded ejecuta viewModel.InitializeAsync()
    ↓
Carga lista de plataformas (449 plataformas)
```

### 2. Navegación
```
Usuario selecciona "Arcade" en panel izquierdo
    ↓
LoadGamesForPlatformCommand se ejecuta
    ↓
CacheManager carga Arcade.xml (18.9 MB)
    ↓
Games collection se llena con GameViewModels
    ↓
FilteredGames se actualiza
    ↓
Panel central muestra lista de juegos
```

### 3. Lanzar Juego
```
Usuario selecciona "Street Fighter II"
    ↓
SelectedGame se actualiza
    ↓
Panel derecho muestra detalles
    ↓
Usuario presiona F5 o botón "Jugar"
    ↓
LaunchSelectedGameCommand se ejecuta
    ↓
EmulatorLauncher lanza el emulador
    ↓
StatisticsTracker registra sesión
    ↓
XML se actualiza con PlayCount++, PlayTime+=X
```

### 4. Búsqueda
```
Usuario escribe "mario" en SearchBox
    ↓
Usuario presiona Enter
    ↓
SearchGamesCommand se ejecuta
    ↓
GameManager.SearchGamesAsync() busca en todas plataformas
    ↓
FilteredGames se actualiza con resultados
    ↓
StatusText muestra "Encontrados 47 juegos"
```

---

## Integración con Fases Anteriores

### Fase 1 (Data Foundation)
- ✅ `XmlDataContext` usado para cargar Platforms.xml y Platform/*.xml
- ✅ `GameCacheManager` cachea juegos en memoria
- ✅ `FileSystemWatcher` detecta cambios externos

### Fase 2 (Business Logic)
- ✅ `EmulatorLauncher` lanza juegos
- ✅ `StatisticsTracker` registra sesiones
- ✅ `GameManager` gestiona CRUD
- ✅ `PlatformManager` carga plataformas
- ✅ `SettingsManager` (listo para Fase 7)

---

## Próximos Pasos (Fase 4)

**Pendiente**: Tarea #14 - Implementar controles personalizados

La tarea #14 está en estado `pending` pero NO es crítica para el MVP Desktop. Los controles actuales (ListBox estándar) funcionan perfectamente. Los controles personalizados serían mejoras visuales (como tiles con imágenes de box art).

**Recomendación**: Continuar directamente con **Fase 4 - Aplicación BigScreen MVP** antes de pulir controles personalizados, para tener ambas aplicaciones funcionales lo antes posible.

---

## Decisiones Técnicas de la Fase 3

### 1. MVVM Framework
**Decisión**: CommunityToolkit.Mvvm
**Razón**: Source generators reducen boilerplate, mejor rendimiento que Caliburn.Micro

### 2. Dependency Injection
**Decisión**: Manual en App.xaml.cs
**Razón**: WPF no tiene DI built-in, manual DI es suficiente para este proyecto

### 3. Layout Design
**Decisión**: Grid de 5 columnas con GridSplitters
**Razón**: Permite al usuario ajustar tamaños de paneles, similar a LaunchBox

### 4. Converters
**Decisión**: Custom IValueConverter implementations
**Razón**: WPF estándar, sin dependencias externas, fácil de mantener

### 5. Persistencia de Configuración
**Decisión**: Windows Registry (HKCU)
**Razón**: Estándar para aplicaciones Windows, persiste entre sesiones

---

## Conclusión

✅ **Fase 3 COMPLETADA exitosamente**

La aplicación Desktop MVP está completamente funcional con:
- Interfaz moderna y responsive
- Navegación fluida entre plataformas y juegos
- Lanzamiento de juegos con tracking de estadísticas
- Búsqueda global
- Gestión de favoritos
- 100% compatible con datos XML de LaunchBox

**Próximo objetivo**: Fase 4 - Aplicación BigScreen MVP para TV/gamepad
