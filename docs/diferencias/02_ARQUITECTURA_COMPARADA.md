# 02 — ARQUITECTURA COMPARADA: GameLauncher vs LaunchBox

## 2.1 Stack Tecnológico

| Aspecto | LaunchBox 13.6 | GameLauncher |
|---------|----------------|-------------|
| **Lenguaje** | C# | C# 12 |
| **Framework** | .NET 6.0 | .NET 8.0 |
| **UI** | WPF | WPF |
| **Patrón MVVM** | Caliburn.Micro | CommunityToolkit.Mvvm |
| **Persistencia** | XML (primario) + SQLite (caché) | XML puro |
| **Multimedia** | VLC embebido | MediaElement WPF nativo |
| **Compresión** | 7-Zip (externo) | SharpCompress (embebido) |
| **Logging** | Propio | System.Diagnostics.Debug |
| **DI** | Manual / Caliburn | Manual (new) |
| **Async** | async/await + Task | async/await + Task |
| **Threading** | Task.Run + Dispatcher | Task.Run + Dispatcher |

---

## 2.2 Estructura de Proyectos

### LaunchBox (estimada desde binarios)
```
LaunchBox.exe              → Stub launcher (22 KB)
Core/LaunchBox.exe         → WPF Desktop app (~19 MB DLL)
Core/BigBox.exe            → WPF BigBox app
Core/Unbroken.LaunchBox.Windows.dll  → Core principal (61 MB)
Core/Unbroken.LaunchBox.dll          → Lógica compartida (2.2 MB)
Core/Unbroken.LaunchBox.Plugins.dll  → API pública plugins (51 KB)
```

### GameLauncher (código fuente completo)
```
src/Core/GameLauncher.Core           → Modelos de dominio
src/Core/GameLauncher.Data           → Acceso XML
src/Core/GameLauncher.Infrastructure → Servicios de negocio
src/UI/GameLauncher.Desktop          → App escritorio WPF
src/UI/GameLauncher.BigScreen        → App BigScreen WPF
src/UI/GameLauncher.UI.Shared        → Componentes compartidos
```

**Similitud arquitectónica:** Alta. GameLauncher sigue el mismo patrón de separación Desktop/BigScreen que LaunchBox.

---

## 2.3 Patrón MVVM

### LaunchBox
- Usa **Caliburn.Micro** como framework MVVM
- Convención de nombres (naming convention) para binding automático
- `IScreen`, `IConductor` para navegación
- `EventAggregator` para comunicación entre VMs

### GameLauncher
- Usa **CommunityToolkit.Mvvm** (Microsoft, open-source, moderno)
- Atributos `[ObservableProperty]`, `[RelayCommand]` con source generators
- `INavigationAware` propio para navegación BigScreen
- Sin bus de eventos (comunicación directa entre servicios)

**Impacto:** El patrón es equivalente pero la implementación es diferente. Los ViewModels de GameLauncher son más verbosos en algunos casos pero más explícitos.

---

## 2.4 Sistema de Navegación

### LaunchBox BigBox — Navegación
- Frame/Page WPF con transiciones XAML configurables
- Temas XAML que definen las vistas y transiciones
- Caliburn.Micro como conductor de pantallas

### GameLauncher BigScreen — Navegación
```csharp
// NavigationService propio
public interface INavigationAware {
    void OnNavigatedTo(object? parameter);
}

// MainWindow.xaml.cs maneja Frame WPF
private Frame? NavigationFrame;
```

- Frame WPF con `Navigate(Page/UserControl)`
- Stack de navegación para "back"
- Sin transiciones animadas (TODO importante)

---

## 2.5 Sistema de Datos

### LaunchBox
```
XML → SQLite cache → In-memory cache
      (para búsquedas    (para UI rápido)
       y count rápido)
```
LaunchBox mantiene una base de datos SQLite como caché para operaciones de búsqueda y conteo rápido. Los XMLs son la fuente de verdad.

### GameLauncher
```
XML → GameCacheManager (in-memory) → UI
            ↓
       JSON cache (disco, rutas de imágenes)
```
GameLauncher no usa SQLite. Todo es en memoria con un caché JSON para rutas de imágenes.

**Impacto en rendimiento:** Con 18,900 ROMs de Arcade, GameLauncher carga todo en RAM. Para plataformas grandes podría ser lento. LaunchBox usa SQLite para filtrado eficiente.

---

## 2.6 Sistema de Imágenes

### LaunchBox
- Prioridad configurable de tipos de imagen (en `Settings.xml`)
- Thumbnails pre-generados en diferentes resoluciones
- Lazy loading con placeholders
- Soporte de imágenes 3D renderizadas

### GameLauncher
```csharp
// GamesWheelViewModel.cs — ResolveCoverImagePath()
string[] imageTypeFolders = {
    "Box - Front",
    "Box - Front - Reconstructed",
    "Fanart - Box - Front",
    "Steam Poster",
    "Epic Games Poster",
    "GOG Poster",
    "Origin Poster",
    "Box - 3D",
    "Banner",
    "Clear Logo"
};
// Busca en disco con sufijos -01, -02, -03, ""
// Extensiones: .png, .jpg, .jpeg, .gif, .bmp
```

- Prioridad de imágenes hardcodeada (no configurable desde Settings.xml)
- Lazy loading por lotes (batch de 30/50)
- `BitmapImage` con `DecodePixelWidth=250` para thumbnails eficientes
- Caché JSON de rutas en disco
- Caché en memoria por plataforma

**Diferencia clave:** La prioridad de imágenes debería leerla de `Settings.xml` (campo `DefaultImageGroup` y `ImageTypeSettings`), no estar hardcodeada.

---

## 2.7 Sistema de Video

### LaunchBox
- VLC embebido como engine de video
- `VideoPlaybackEngine: VLC` en Settings
- Background videos, auto-play configurable
- Timeout de inicio configurable

### GameLauncher
```csharp
// VideoPathResolver.cs
// Busca en: Videos/{Platform}/Video Snap/
//           Videos/{Platform}/Theme Video/
// MediaElement WPF nativo (sin VLC)
```

- Usa `MediaElement` nativo de WPF (solo formatos Windows Media)
- Delay de 500ms antes de reproducir al seleccionar juego
- Solo busca "Video Snap" y "Theme Video"
- No soporta todos los formatos que soporta VLC (mkv, avi, etc. pueden fallar)

**Recomendación:** Integrar LibVLCSharp (VLC.NET) para máxima compatibilidad.

---

## 2.8 Sistema de Servicios (Dependency Injection)

### LaunchBox (estimado)
- Framework DI interno o Caliburn IoC
- Resolución por interfaz

### GameLauncher
```csharp
// App.xaml.cs — Composición manual
var dataContext = new XmlDataContext(dataPath);
var cacheManager = new GameCacheManager(dataContext);
var emulatorLauncher = new EmulatorLauncher(dataContext, launchBoxPath);
var statisticsTracker = new StatisticsTracker(dataContext);
var gameManager = new GameManager(dataContext, cacheManager, statisticsTracker);
var platformManager = new PlatformManager(dataContext, statisticsTracker, cacheManager);
var playlistManager = new PlaylistManager(dataContext, gameManager);
var settingsManager = new SettingsManager(dataContext);
```

Composición manual sin container DI. Es simple y funcional para este tamaño de proyecto.

---

## 2.9 Interfaces de Servicio

```
IEmulatorLauncher      → Lanzar juegos, matar proceso
IGameManager           → CRUD de juegos, búsquedas
IPlatformManager       → Plataformas, árbol de navegación
IPlaylistManager       → Playlists y sus juegos
IStatisticsTracker     → Estadísticas de juego
ISettingsManager       → Settings.xml y BigBoxSettings.xml
```

Todas las interfaces están definidas. Permite testing y sustitución de implementaciones.

---

## 2.10 Diagrama de Dependencias

```
BigScreen App                    Desktop App
     │                                │
     ├─ PlatformFiltersViewModel      ├─ MainViewModel
     ├─ GamesWheelViewModel           ├─ GameViewModel
     ├─ GameDetailsViewModel          └─ StatisticsViewModel
     └─ GameEditorViewModel
                │                          │
                └──────────────────────────┘
                               │
                    Infrastructure Layer
                    ├─ EmulatorLauncher
                    ├─ GameManager
                    ├─ PlatformManager
                    ├─ PlaylistManager
                    ├─ StatisticsTracker
                    └─ SettingsManager
                               │
                      Data Layer
                      ├─ XmlDataContext
                      └─ GameCacheManager
                               │
                        Core Layer
                        ├─ Game.cs
                        ├─ Platform.cs
                        ├─ Emulator.cs
                        └─ ... (20 modelos)
```
