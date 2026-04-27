# Fase 4: Aplicación BigScreen MVP - Progreso

**Estado**: 🔄 EN PROGRESO (3/8 tareas completadas)
**Fecha inicio**: 8 de Febrero 2026
**Última actualización**: 8 de Febrero 2026 - 03:30

---

## Objetivo de la Fase 4

Crear una aplicación fullscreen para TV con control por gamepad que permita:
- Navegación completa por gamepad (Xbox/compatible)
- Interfaz tipo "wheel" para plataformas y juegos
- Video playback de gameplay
- Transiciones suaves entre vistas
- Experiencia optimizada para pantallas grandes (TV)

---

## ✅ Componentes Completados

### 1. Configuración del Proyecto (Tarea #17)

**Archivo modificado**: `GameLauncher.BigScreen.csproj`

**Referencias de proyectos agregadas**:
- `GameLauncher.Core` - Modelos y lógica de negocio
- `GameLauncher.Data` - Parser XML de LaunchBox
- `GameLauncher.Infrastructure` - Servicios (EmulatorLauncher, StatisticsTracker, etc.)
- `GameLauncher.UI.Shared` - Controles compartidos

**NuGet packages agregados**:
```xml
<PackageReference Include="CommunityToolkit.Mvvm" Version="8.2.2" />
<PackageReference Include="Microsoft.Xaml.Behaviors.Wpf" Version="1.1.77" />
<PackageReference Include="LibVLCSharp.WPF" Version="3.8.5" />
<PackageReference Include="XInputDotNetPure" Version="1.0.0" />
```

**Estructura de carpetas creada**:
```
GameLauncher.BigScreen/
├── Input/           # Controlador de gamepad
├── Navigation/      # Servicio de navegación
├── ViewModels/      # ViewModels de vistas
├── Views/           # Vistas XAML
└── Transitions/     # Sistema de transiciones
```

**Resultado**: ✅ Compilación exitosa

---

### 2. GamepadController con XInput (Tarea #18)

**Archivo creado**: `Input/GamepadController.cs` (310 líneas)

**Características implementadas**:

#### Polling a 60 FPS
```csharp
private readonly DispatcherTimer _pollTimer;
// Timer a 16.67ms (60 FPS)
_pollTimer = new DispatcherTimer
{
    Interval = TimeSpan.FromMilliseconds(1000.0 / 60.0)
};
```

#### Detección de Botones
- **A Button** → `SelectPressed` (Confirmar/Seleccionar)
- **B Button** → `BackPressed` (Volver/Cancelar)
- **X Button** → `DetailsPressed` (Detalles/Info)
- **Y Button** → `OptionsPressed` (Opciones/Menú)
- **Start/Back** → Eventos genéricos

#### Navegación con D-Pad
```csharp
public event Action? NavigateUp;
public event Action? NavigateDown;
public event Action? NavigateLeft;
public event Action? NavigateRight;
```

Detección directa desde `GamePadState.DPad`:
```csharp
if (_currentState.DPad.Up == ButtonState.Pressed &&
    _previousState.DPad.Up == ButtonState.Released)
{
    NavigateUp?.Invoke();
}
```

#### Navegación con ThumbStick Izquierdo
- Dead zone configurable (0.3 por defecto)
- Detección de transiciones (no estaba presionado → ahora sí)
- Evita inputs repetidos

#### Triggers
- **Right Trigger (> 0.5)** → `PlayPressed` (Lanzar juego)
- **Left Trigger (> 0.5)** → `FavoritePressed` (Marcar favorito)

#### Eventos Disponibles
```csharp
// Botones
public event Action<GamepadButton>? ButtonPressed;
public event Action<GamepadButton>? ButtonReleased;

// Navegación
public event Action? NavigateUp;
public event Action? NavigateDown;
public event Action? NavigateLeft;
public event Action? NavigateRight;

// Acciones
public event Action? SelectPressed;      // A
public event Action? BackPressed;        // B
public event Action? DetailsPressed;     // X
public event Action? OptionsPressed;     // Y
public event Action? PlayPressed;        // RT
public event Action? FavoritePressed;    // LT
```

#### Uso del Controlador
```csharp
var gamepad = new GamepadController(PlayerIndex.One);

gamepad.SelectPressed += () =>
{
    // Usuario presionó A (Seleccionar)
};

gamepad.NavigateRight += () =>
{
    // Usuario navegó a la derecha
};

gamepad.Start();  // Iniciar polling
```

**Resultado**: ✅ Compilación exitosa, 0 errores

---

### 3. Sistema de Navegación Stack-Based (Tarea #19)

**Archivo creado**: `Navigation/BigScreenNavigationService.cs` (270 líneas)

**Arquitectura**:
```
Stack de Navegación:
┌─────────────────────┐
│  GameDetailsView    │ ← Vista actual (Peek)
├─────────────────────┤
│  HorizontalWheelView│
├─────────────────────┤
│  PlatformFiltersView│ ← Vista raíz
└─────────────────────┘

GoBack() → Pop y navega a la vista anterior
```

**Características**:

#### Navegación con Stack
```csharp
private readonly Stack<NavigationEntry> _navigationStack = new();

public void NavigateTo(Type viewType, object? parameter = null)
{
    // Crear vista
    var view = Activator.CreateInstance(viewType) as Page;

    // Empujar al stack
    var entry = new NavigationEntry(viewType, view, parameter);
    _navigationStack.Push(entry);

    // Navegar en Frame
    _frame.Navigate(view);
}

public bool GoBack()
{
    if (!CanGoBack) return false;

    // Pop vista actual
    _navigationStack.Pop();

    // Navegar a vista anterior
    var previousEntry = _navigationStack.Peek();
    _frame.Navigate(previousEntry.View);

    return true;
}
```

#### Parámetros de Navegación
```csharp
// Navegar con parámetro
navigationService.NavigateTo(typeof(GamesView), platformName: "Arcade");

// En la vista:
public class GamesViewModel : INavigationAware
{
    public void OnNavigatedTo(object? parameter)
    {
        if (parameter is string platformName)
        {
            LoadGames(platformName);
        }
    }
}
```

#### Interfaz INavigationAware
```csharp
public interface INavigationAware
{
    void OnNavigatedTo(object? parameter);      // Al llegar a la vista
    void OnNavigatedFrom();                      // Al salir de la vista
    void OnNavigatedBack(object? parameter);     // Al volver con GoBack()
}
```

#### Eventos de Navegación
```csharp
public event EventHandler<NavigatedEventArgs>? Navigated;
public event EventHandler<NavigatingEventArgs>? Navigating;

// Uso:
navigationService.Navigating += (s, e) =>
{
    // Antes de navegar, se puede cancelar
    if (!UserConfirmed)
        e.Cancel = true;
};

navigationService.Navigated += (s, e) =>
{
    // Después de navegar
    Console.WriteLine($"Navegado a: {e.Entry.ViewType.Name}");
};
```

#### Métodos Adicionales
```csharp
// Navegar a raíz (limpia stack y navega)
public void NavigateToRoot(Type viewType, object? parameter = null);

// Limpiar todo el historial
public void ClearHistory();

// Navegar con ViewModel específico
public void NavigateTo<TView>(object? viewModel = null, object? parameter = null)
    where TView : Page;
```

#### Flujo de Navegación Típico
```
Usuario en PlatformFiltersView
    ↓ [Selecciona "Arcade", presiona A]
NavigateTo(typeof(GamesView), "Arcade")
    ↓ [Stack: PlatformFiltersView → GamesView]
Usuario en GamesView (muestra juegos de Arcade)
    ↓ [Selecciona "Street Fighter II", presiona A]
NavigateTo(typeof(GameDetailsView), gameId)
    ↓ [Stack: PlatformFiltersView → GamesView → GameDetailsView]
Usuario presiona B
    ↓
GoBack()
    ↓ [Stack: PlatformFiltersView → GamesView]
Usuario de vuelta en GamesView
```

**Resultado**: ✅ Compilación exitosa, 0 errores

---

## 🔄 Componentes Pendientes

### 4. PlatformWheel2FiltersView (Tarea #20)
**Vista de selección de plataformas con wheel vertical**

Características a implementar:
- ListBox vertical con plataformas
- Animación de "wheel" (items que entran/salen)
- Plataforma seleccionada centrada y más grande
- Background dinámico (imagen de plataforma)
- Navegación con gamepad (Up/Down)
- Binding a `PlatformViewModel` collection

Diseño visual:
```
┌────────────────────────────────────┐
│                                    │
│         Super Nintendo             │ ← Item arriba (pequeño, semi-transparente)
│                                    │
│    ┌──────────────────────┐       │
│    │   NINTENDO 64        │       │ ← Item seleccionado (grande, centrado)
│    │   147 juegos         │       │
│    └──────────────────────┘       │
│                                    │
│         PlayStation                │ ← Item abajo (pequeño, semi-transparente)
│                                    │
└────────────────────────────────────┘
Background: Imagen de la plataforma seleccionada (blur)
```

---

### 5. HorizontalWheel3GamesView (Tarea #21)
**Vista de selección de juegos con wheel horizontal**

Características a implementar:
- ItemsControl horizontal con 3 juegos visibles
- Juego central más grande
- Box art de juegos (Front box)
- Navegación con gamepad (Left/Right)
- Información del juego seleccionado en overlay
- Binding a `GameViewModel` collection

Diseño visual:
```
┌──────────────────────────────────────────────────────────┐
│                                                          │
│   ┌────────┐      ┌────────────┐      ┌────────┐      │
│   │        │      │            │      │        │      │
│   │ Game 1 │      │  GAME 2    │      │ Game 3 │      │
│   │        │      │  (Selected)│      │        │      │
│   └────────┘      │            │      └────────┘      │
│   pequeño         │  Grande    │      pequeño         │
│                   └────────────┘                       │
│                                                          │
│   Street Fighter II                                     │
│   Capcom • 1991 • Arcade • Fighting                    │
│   ★★★★☆ • Played 47 times • 23h 15m                   │
└──────────────────────────────────────────────────────────┘
```

---

### 6. GameDetailsView con Video (Tarea #22)
**Vista de detalles del juego con video gameplay**

Características a implementar:
- Layout de detalles completo
- Video playback con LibVLCSharp
- Galería de screenshots
- Información completa (desarrollador, género, rating, etc.)
- Botones: Jugar, Favorito, Volver
- Navegación con gamepad

Diseño visual:
```
┌──────────────────────────────────────────────────────────┐
│  ┌──────────────────┐  Street Fighter II                │
│  │                  │  Capcom • 1991                     │
│  │   VIDEO          │  Genre: Fighting                   │
│  │   GAMEPLAY       │  Platform: Arcade                  │
│  │                  │  Rating: ★★★★★                    │
│  └──────────────────┘                                    │
│                                                          │
│  Description:                                            │
│  The world warrior tournament...                        │
│                                                          │
│  Statistics:                                             │
│  • Played 47 times                                       │
│  • Total time: 23h 15m                                   │
│  • Last played: 2026-02-07                               │
│                                                          │
│  [▶ Jugar]  [★ Favorito]  [← Volver]                   │
└──────────────────────────────────────────────────────────┘
```

---

### 7. Sistema de Transiciones (Tarea #23)
**TransitionPresenter para animaciones entre vistas**

Características a implementar:
- Tipos de transición:
  - **Fade**: Desvanecimiento
  - **Slide**: Deslizamiento (horizontal/vertical)
  - **Rotate**: Rotación 3D
  - **Scale**: Escalado
- Duración configurable (desde BigBoxSettings.xml)
- Usar Storyboards de WPF
- Eventos de inicio/fin de transición

Ejemplo de uso:
```csharp
<transitions:TransitionPresenter
    TransitionType="SlideHorizontal"
    Duration="0:0:0.3"
    Content="{Binding CurrentView}" />
```

---

### 8. Configurar App.xaml.cs para BigScreen (Tarea #24)
**Configuración de la aplicación BigScreen**

Características a implementar:
- Fullscreen al inicio:
  ```csharp
  WindowState = WindowState.Maximized;
  WindowStyle = WindowStyle.None;  // Sin bordes
  ```
- Cargar BigBoxSettings.xml
- Dependency injection de servicios
- Inicializar GamepadController y conectar eventos
- Crear BigScreenNavigationService
- Navegar a vista inicial (PlatformFiltersView)
- Manejar eventos del gamepad globalmente:
  ```csharp
  gamepad.BackPressed += () =>
  {
      if (navigationService.CanGoBack)
          navigationService.GoBack();
  };
  ```

---

## Próximos Pasos

**Orden recomendado de implementación**:

1. **Tarea #24** (App.xaml.cs) - Configurar la aplicación base
2. **Tarea #20** (PlatformWheel2FiltersView) - Vista de plataformas (más simple)
3. **Tarea #21** (HorizontalWheel3GamesView) - Vista de juegos
4. **Tarea #22** (GameDetailsView) - Vista de detalles con video
5. **Tarea #23** (Transiciones) - Pulir con animaciones

Después de completar estas 5 tareas, la **Fase 4 estará completa** y tendremos una aplicación BigScreen funcional.

---

## Estado de Compilación

```bash
cd /h/GameLauncher
dotnet build src/UI/GameLauncher.BigScreen/GameLauncher.BigScreen.csproj
```

**Resultado actual**: ✅ Compilación correcta, 0 errores, 3 warnings (XInputDotNetPure compatibilidad)

---

## Archivos Creados en Fase 4

| Archivo | Líneas | Descripción |
|---------|--------|-------------|
| `GameLauncher.BigScreen.csproj` | 23 | Configuración del proyecto |
| `Input/GamepadController.cs` | 310 | Controlador de gamepad con XInput |
| `Navigation/BigScreenNavigationService.cs` | 270 | Servicio de navegación stack-based |
| **Total** | **603** | 3 archivos creados |

---

## Decisiones Técnicas

### XInput vs DirectInput
**Decisión**: XInputDotNetPure
**Razón**: API simple, soporte nativo de Xbox controllers, polling fácil
**Trade-off**: Solo soporta controllers Xbox-compatible (pero son los más comunes)

### Navegación Stack vs State Machine
**Decisión**: Stack-based navigation
**Razón**: Historial natural con GoBack(), fácil de implementar, predecible

### Frame vs Custom ContentPresenter
**Decisión**: WPF Frame
**Razón**: Soporte nativo de navegación, compatible con Page, integración con historial

### LibVLCSharp vs MediaElement
**Decisión**: LibVLCSharp
**Razón**: Mismo que LaunchBox, soporta todos los formatos, mejor rendimiento

---

## Conclusión Parcial

✅ **Fundamentos de BigScreen completados**:
- Proyecto configurado con todas las dependencias
- GamepadController funcional con polling a 60 FPS
- Sistema de navegación robusto con stack y eventos

🔄 **Pendiente**:
- Vistas XAML (PlatformFilters, GamesWheel, GameDetails)
- Sistema de transiciones
- Configuración de App.xaml.cs

**Progreso**: 37.5% (3/8 tareas)

**Tiempo estimado restante**: ~2-3 horas para completar las 5 tareas pendientes
