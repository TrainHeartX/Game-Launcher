# Changelog

Todos los cambios notables de este proyecto serán documentados en este archivo.

El formato está basado en [Keep a Changelog](https://keepachangelog.com/es-ES/1.0.0/),
y este proyecto adhiere a [Semantic Versioning](https://semver.org/lang/es/).

## [1.5.0-beta] - 2026-04-27

### Añadido
- **BigScreen: Modo Attract (Atracción)**
  - Protector de pantalla automático con vídeos de juegos aleatorios.
  - Overlay de información con título, plataforma y metadatos.
  - Efecto visual de Scanlines para un look retro.
  - Timer de inactividad de alta precisión sincronizado con el input del usuario.
- **BigScreen: Vista de Lista (TextList)**
  - Nuevo modo de visualización optimizado con ListView virtualizado.
  - Navegación ultra-fluida para bibliotecas masivas.
  - Alternancia entre modo Wheel y modo Lista mediante tecla `Tab`.
- **BigScreen: Sistema de Feedback de Lanzamiento**
  - Pantalla de carga animada ("Lanzando...") al iniciar un emulador.
  - Animación pulsante de mando de juego con efectos de resplandor.
  - Información clara sobre el estado de la aplicación mientras el juego corre.
- **BigScreen: Atajos de Teclado Completos**
  - `S`: Cambiar campo de ordenamiento.
  - `D`: Cambiar dirección de orden (Asc/Desc).
  - `Q`: Ciclar filtros rápidos (Favoritos, Completados, etc.).
  - `Tab`: Cambiar modo de vista (Wheel/Lista).
  - `A`: Menú de gestión / Editar juego.
  - `I`: Galería de imágenes.
- **Desktop: Toolbar de Control**
  - Nueva barra de herramientas superior con selectores de Orden y Filtro.
  - Acceso rápido a configuraciones de visualización.

### Cambiado
- **Gestión de Ventanas al Jugar**
  - Tanto en Desktop como en BigScreen, la ventana principal se minimiza automáticamente al lanzar un juego para liberar recursos y evitar distracciones.
  - Restauración y activación automática del foco al cerrar el emulador.
- **Optimización de Playlists**
  - Implementación de batch lookup O(n+m) para la carga de juegos en listas de reproducción.
  - Eliminación de retardos y pantallas negras al cargar playlists grandes como "Assassin's Creed".
- **Comportamiento del Audio**
  - Integración inteligente del `BackgroundMusicService` para pausar la música de fondo durante el juego y reanudarla al volver.

### Corregido
- **BUG-01**: Corrección del nombre del comando de lanzamiento (LaunchGameCommand) para compatibilidad con MVVM Toolkit 8.x.
- **BUG-04**: Manejo robusto de juegos sin ID asignado en `StatisticsTracker` (fallback a Título+Plataforma).
- **BUG-06**: Solucionada la pérdida de nodos `<AdditionalApplication>` al guardar cambios en los XML de plataformas.
- **BUG-08**: Soporte mejorado para emuladores basados en asociaciones de archivos mediante fallback automático a `UseShellExecute=true`.
- **BUG-09**: Eliminado el error de argumentos con doble espacio al construir líneas de comandos complejas.
- **BUG-11**: Reactividad de la interfaz corregida para el sistema de calificación por estrellas (`StarRating`).
- **BUG-12**: Prioridad de imágenes de portada corregida (la imagen principal sin sufijo tiene prioridad #1).
- **BUG-13**: Precisión del timer de inactividad para el modo Attract corregida para medir tiempo real transcurrido.

---

## [1.0.0-beta] - 2026-02-08

### Añadido

#### Fase 1: Fundamentos de Datos
- **Modelos de Datos Completos**
  - `Game.cs` con 100+ propiedades de metadata
  - `Platform.cs` con especificaciones técnicas completas
  - `Emulator.cs` con configuración de emuladores
  - `EmulatorPlatform.cs` para mapeo emulador-plataforma
  - `Settings.cs` con 289+ opciones de configuración Desktop
  - `BigBoxSettings.cs` con 523+ opciones de configuración BigScreen
  - `Playlist.cs` para playlists manuales y auto-generadas

- **Parser XML Compatible con LaunchBox**
  - `XmlDataContext.cs` para lectura/escritura de XML
  - Garantía de compatibilidad 100% con formato LaunchBox
  - UTF-8 con BOM encoding
  - Preservación exacta de estructura XML
  - Soporte para todos los archivos XML de LaunchBox

- **Sistema de Caché**
  - `GameCacheManager.cs` para caché en memoria
  - FileSystemWatcher para invalidación automática
  - Thread-safe con ReaderWriterLockSlim
  - Optimización para archivos XML grandes (Arcade.xml 18.9 MB)

- **Tests de Compatibilidad**
  - 6/6 tests de Data pasando
  - Tests de round-trip para validar preservación XML
  - Tests de manejo de archivos vacíos y faltantes

#### Fase 2: Lógica de Negocio
- **EmulatorLauncher**
  - Lanzamiento de juegos con emuladores
  - Construcción de línea de comandos con parámetros
  - Validación de rutas y archivos
  - Tracking de tiempo de juego

- **StatisticsTracker**
  - Tracking de sesiones de juego
  - Actualización de PlayCount, PlayTime, LastPlayed
  - Estadísticas agregadas por plataforma

- **GameManager**
  - CRUD completo para juegos
  - Auto-generación de GUIDs
  - Búsqueda multi-campo
  - Búsqueda en todas las plataformas

- **PlatformManager**
  - Gestión de plataformas
  - Agrupación por categoría
  - Estadísticas de plataforma

- **SettingsManager**
  - Carga/guardado de Settings.xml
  - Carga/guardado de BigBoxSettings.xml
  - Generación de configuración por defecto

- **GamePersistenceService**
  - Persistencia automática de cambios en XML
  - Actualización de DateModified
  - Sincronización con archivos LaunchBox

- **Tests de Servicios**
  - 20/20 tests de Core pasando
  - Cobertura completa de todos los servicios

#### Fase 3: Desktop MVP
- **Framework MVVM**
  - Integración de CommunityToolkit.Mvvm
  - ViewModels con ObservableObject
  - RelayCommand para comandos

- **MainWindow**
  - Layout de 3 paneles (Filtros | Juegos | Detalles)
  - GridSplitters redimensionables
  - Navegación por plataformas

- **ViewModels**
  - `MainViewModel.cs` - ViewModel principal
  - `GameViewModel.cs` - Wrapper de Game
  - `PlatformViewModel.cs` - Wrapper de Platform
  - `StatisticsViewModel.cs` - Estadísticas avanzadas

- **Comandos**
  - LaunchGameCommand
  - SearchGamesCommand
  - FilterByPlatformCommand
  - ToggleFavoriteCommand

#### Fase 4: BigScreen MVP
- **GamepadController**
  - Polling a 60 FPS con XInputDotNetPure
  - Soporte para Xbox 360/One/Series controllers
  - Eventos para botones y navegación
  - Mapeo configurable desde BigBoxSettings.xml

- **BigScreenNavigationService**
  - Navegación basada en stack (push/pop)
  - Soporte para INavigationAware ViewModels
  - Estado de navegación persistente
  - Integración con Frame de WPF

- **Vistas BigScreen**
  - `PlatformFiltersView.xaml` - Lista de plataformas
  - `GamesWheelView.xaml` - Wheel horizontal de juegos
  - `GameDetailsView.xaml` - Detalles del juego

- **ViewModels BigScreen**
  - `PlatformFiltersViewModel.cs`
  - `GamesWheelViewModel.cs`
  - `GameDetailsViewModel.cs`
  - Comandos de navegación integrados

- **TransitionPresenter**
  - Fade transitions
  - Slide transitions (horizontal/vertical)
  - Rotate transitions
  - Scale transitions
  - Configurable desde BigBoxSettings.xml

- **Integración de Navegación**
  - Frame-based navigation en MainWindow
  - Factory methods en App.xaml.cs para ViewModels
  - Conexión de gamepad con ViewModels
  - Navegación fluida entre vistas

#### Fase 5: Sistema de Temas
- **ThemeLoader**
  - Carga dinámica de ResourceDictionaries
  - Soporte para temas personalizados
  - Estructura de carpetas para temas
  - Tema Default incluido

- **Estructura de Temas**
  - Theme.xaml para estilos
  - Carpetas para Fonts, Images, Sounds
  - Compatibilidad básica con temas de LaunchBox

#### Fase 6: Estadísticas Avanzadas
- **Panel de Estadísticas**
  - Tiempo total de juego
  - Total de juegos jugados
  - Total de plataformas activas
  - Top 10 juegos más jugados
  - Top plataformas con tiempo de juego

- **StatisticsView**
  - Diseño con cards de resumen
  - Listas de top games y platforms
  - Binding a ViewModel
  - Actualización en tiempo real

#### Fase 7: Documentación
- **README.md completo**
  - Documentación de instalación
  - Guía de uso para Desktop y BigScreen
  - Arquitectura del proyecto
  - Troubleshooting
  - Roadmap

- **CHANGELOG.md**
  - Registro detallado de cambios
  - Formato Keep a Changelog

- **FAQ.md** (próximamente)
  - Preguntas frecuentes
  - Solución de problemas comunes

- **MANUAL_USUARIO.md** (próximamente)
  - Manual detallado de usuario
  - Capturas de pantalla
  - Guías paso a paso

### Características Técnicas

- **.NET 8.0** - Framework LTS más reciente
- **C# 12** - Características modernas del lenguaje
- **WPF/XAML** - UI nativa de Windows
- **CommunityToolkit.Mvvm** - Framework MVVM moderno
- **XInputDotNetPure** - Soporte de gamepad Xbox
- **NUnit** - Framework de testing
- **FluentAssertions** - Assertions fluent
- **Moq** - Mocking framework

### Compatibilidad

- **Windows 10/11** (x64)
- **LaunchBox** - Compatibilidad 100% con XML
- **Emuladores** - Todos los emuladores compatibles con LaunchBox
- **Gamepads** - Xbox 360, Xbox One, Xbox Series (XInput)

### Tests

- **26/26 tests pasando** ✅
  - 6 tests de Data (XML parsing)
  - 20 tests de Core (servicios)
- **Cobertura de código**: >80%
- **Tests de integración**: Round-trip XML validation

### Conocidos Issues

- **Task #14**: Controles personalizados Desktop (GameGridView, GameDetailsPanel, PlatformTreeView) pendientes de implementación
- **XInputDotNetPure Warning**: Warning esperado de compatibilidad .NET Framework (no afecta funcionalidad)
- **LibVLCSharp**: Reproducción de video en BigScreen pendiente de integración
- **LiveCharts**: Gráficas interactivas en estadísticas pendientes

### Notas de Migración

#### Desde LaunchBox
No requiere migración. GameLauncher lee y escribe directamente los archivos XML de LaunchBox. Ambas aplicaciones pueden usarse en paralelo sin conflictos.

#### Primera Configuración
1. Ejecutar GameLauncher.Desktop.exe o GameLauncher.BigScreen.exe
2. Seleccionar carpeta raíz de LaunchBox cuando se solicite
3. La configuración se guarda automáticamente en configuración local

### Roadmap

#### v1.1 (Futuro Próximo)
- [ ] Completar Task #14: Controles personalizados Desktop
- [ ] Integrar LibVLCSharp para video playback
- [ ] Integrar LiveCharts para gráficas de estadísticas
- [ ] Exportación de estadísticas a CSV/Excel
- [ ] Editor de metadata integrado
- [ ] Instalador con Inno Setup

#### v1.2 (Futuro)
- [ ] Soporte de plugins
- [ ] Scraping automático de metadata
- [ ] Playlists avanzadas
- [ ] Temas avanzados con editor visual

#### v2.0 (Futuro Lejano)
- [ ] Soporte multi-plataforma (Linux/macOS con Avalonia)
- [ ] Servidor web para acceso remoto
- [ ] Aplicación móvil companion
- [ ] Cloud sync de estadísticas

---

## [Unreleased]

### En Desarrollo
- Instalador completo (Inno Setup)
- Manual de usuario detallado con capturas
- FAQ extendido

### En Evaluación
- Soporte para DirectInput gamepads (más allá de XInput)
- Integración con Discord Rich Presence
- Soporte para ROMs comprimidas (ZIP, 7Z)
- Base de datos SQLite opcional para performance

---

**Formato del Changelog**:
- **Añadido** - para nuevas características
- **Cambiado** - para cambios en funcionalidad existente
- **Obsoleto** - para características que serán removidas
- **Removido** - para características removidas
- **Corregido** - para bug fixes
- **Seguridad** - para vulnerabilidades de seguridad

**Versionado Semántico**:
- **MAJOR** - Cambios incompatibles en API
- **MINOR** - Nueva funcionalidad compatible hacia atrás
- **PATCH** - Bug fixes compatibles hacia atrás
