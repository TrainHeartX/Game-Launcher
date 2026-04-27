# Estado Actual del Proyecto: GameLauncher
**Última actualización:** 2026-04-27
**Versión:** 1.5.0-beta

Este documento detalla la arquitectura técnica, las funcionalidades implementadas y el estado de estabilidad del prototipo actual de GameLauncher.

## 1. Arquitectura del Sistema
El proyecto está construido bajo una arquitectura multicapa (Clean Architecture / DDD Lite) utilizando **.NET 8** y **WPF**.

- **GameLauncher.Core**: Contiene las entidades de dominio (`Game`, `Platform`, `Emulator`, `Playlist`) y las interfaces de servicios. Define la lógica de negocio pura.
- **GameLauncher.Data**: Implementación de la persistencia basada en XML. Utiliza un sistema de serialización personalizado para garantizar compatibilidad del 100% con los archivos `.xml` de LaunchBox.
- **GameLauncher.Infrastructure**: Servicios de infraestructura como el lanzador de procesos (`EmulatorLauncher`), el tracker de estadísticas (`StatisticsTracker`), y servicios multimedia.
- **GameLauncher.Desktop**: Interfaz de administración estilo catálogo clásico.
- **GameLauncher.BigScreen**: Interfaz tipo consola optimizada para mando y visualización a distancia.

## 2. Capa de Datos y Compatibilidad
- **Sincronización Bidireccional**: Capaz de leer y escribir en la carpeta `Data` de LaunchBox sin corromper metadatos originales.
- **Preservación de Datos**: Soporta campos extendidos mediante `[XmlAnyElement]`, lo que permite que GameLauncher respete nodos XML que aún no procesa (como configuraciones de controles específicas).
- **Caché Inteligente**: `GameCacheManager` utiliza `FileSystemWatcher` para invalidar datos solo cuando los archivos físicos cambian, optimizando el rendimiento en colecciones de miles de juegos.

## 3. Funcionalidades Desktop
- **Explorador Triple Panel**: Navegación por plataformas/playlists, rejilla de juegos y panel de detalles.
- **Búsqueda Global**: Filtrado instantáneo a través de toda la biblioteca.
- **Gestión de Estadísticas**: Seguimiento de tiempo de juego y contador de sesiones.
- **Control de Lanzamiento**: Minimización automática de la UI al ejecutar juegos.
- **Barra de Herramientas**: Control total sobre ordenamiento (Título, Fecha de Lanzamiento, Fecha de Modificación, Rating) y dirección.

## 4. Funcionalidades BigScreen
- **Experiencia Inmersiva**: Animaciones de transición fluidas (Fade, Slide, Scale) configurables.
- **Modos de Vista**: 
  - *Wheel*: Carrusel horizontal de portadas con efectos de iluminación.
  - *TextList*: Lista vertical virtualizada para navegación rápida en listas masivas.
- **Attract Mode**: Activación automática de vídeos a pantalla completa tras periodos de inactividad, con overlay de información dinámico.
- **Feedback Visual**: Pantalla de carga integrada ("Lanzando...") que mantiene al usuario informado durante la apertura del emulador.
- **Multimedia**: Música de fondo aleatoria por plataforma y efectos de sonido en la navegación.
- **Control Total**: Soporte para mandos XInput y atajos de teclado completos para todas las funciones de gestión.

## 5. Estabilidad y QA
Tras el sprint de estabilización (v1.5.0), se han resuelto los siguientes puntos críticos:
- **Ejecución de Juegos**: Fallback automático a `UseShellExecute` para garantizar que emuladores como RetroArch o MAME funcionen incluso mediante asociaciones de archivos.
- **Rendimiento de Playlists**: Optimización de carga que elimina el lag en listas inteligentes o manuales grandes.
- **Persistencia XML**: Corrección de bugs de serialización que causaban pérdida de aplicaciones adicionales.
- **Interacción**: Flujo de minimización/restauración de ventanas pulido para una transición "transparente" entre el launcher y el juego.

## 6. Estado Técnico
- **Solution Status**: Compila sin errores ni warnings.
- **Tests**: Suite de pruebas unitarias cubriendo el 100% de la lógica de parsing XML y servicios de infraestructura críticos.
- **Dependencies**: 
  - `CommunityToolkit.Mvvm` (8.2+)
  - `LibVLCSharp` (para video playback)
  - `XInputDotNetPure` (controladores)

---
*Este documento es la referencia base para el desarrollo de futuras versiones y la paridad de características con LaunchBox/BigBox.*
