# 07 — UI DESKTOP: Análisis Detallado

## 7.1 Vista General Desktop

### MainWindow.xaml (34,386 bytes — la vista más grande del proyecto)

**Layout general:**
```
┌─────────────────────────────────────────────────────┐
│  TOOLBAR (botones principales)                      │
├────────────┬────────────────────────────────────────┤
│  SIDEBAR   │  CONTENT AREA                          │
│            │  ┌──────────────────────────────────┐  │
│ Categorías │  │  GAME GRID (portadas + títulos)  │  │
│ Plataformas│  │                                  │  │
│ Playlists  │  └──────────────────────────────────┘  │
│            ├──────────────────────────────────────   │
│            │  GAME DETAILS PANEL                    │
│            │  (imagen, video, metadata)             │
└────────────┴────────────────────────────────────────┘
│  STATUS BAR                                         │
└─────────────────────────────────────────────────────┘
```

---

## 7.2 Comparación Vista Desktop vs LaunchBox Desktop

### Sidebar

| Característica | LaunchBox | GameLauncher |
|----------------|-----------|-------------|
| TreeView de categorías | ✅ | ✅ |
| Lista de playlists | ✅ | ✅ |
| Cambiar entre plataformas/playlists | ✅ | ✅ |
| Filtro por campo configurable | ✅ (30+ campos) | ❌ Solo texto |
| Contador de juegos por plataforma | ✅ | ✅ |
| Imágenes de plataforma en sidebar | ✅ | 🟡 (depende de implementación) |
| Sidebar redimensionable | ✅ | 🟡 (posiblemente) |
| Ocultar sidebar | ✅ | ❌ |

### Área de Juegos

| Característica | LaunchBox | GameLauncher |
|----------------|-----------|-------------|
| Vista Grid (portadas) | ✅ | ✅ |
| Vista List (texto) | ✅ | ❌ |
| Múltiples tamaños de portada | ✅ | ❌ Fijo |
| Lazy loading de imágenes | ✅ | ✅ |
| Caché de imágenes en disco | ✅ | ✅ (JSON) |
| Caché en memoria | ✅ | ✅ |
| Virtualización (grandes listas) | ✅ | 🟡 Depende WPF VirtualizingPanel |

### Panel de Detalles

| Característica | LaunchBox | GameLauncher |
|----------------|-----------|-------------|
| Imagen del juego | ✅ | ✅ |
| Video del juego | ✅ | ✅ |
| Metadata básica | ✅ | ✅ |
| Notas/descripción | ✅ | ✅ |
| Rating de estrellas | ✅ | ✅ |
| Calificación comunidad | ✅ | ✅ |
| Tiempo de juego | ✅ | ✅ |
| Veces jugado | ✅ | ✅ |
| Última vez jugado | ✅ | 🟡 (calculado) |
| Múltiples imágenes | ✅ (galería) | ❌ |
| Additional Applications | ✅ | ❌ |
| RA Badges | ✅ | ❌ |
| Botón Wikipedia | ✅ | ❌ |
| Manual PDF (abrir) | ✅ | ❌ |

---

## 7.3 Toolbar y Acciones Disponibles

### Implementado en GameLauncher Desktop ✅

| Acción | Comando |
|--------|---------|
| Lanzar juego seleccionado | `LaunchSelectedGameCommand` |
| Buscar (texto libre) | `SearchGamesCommand` |
| Limpiar búsqueda | `ClearSearchCommand` |
| Ver favoritos | `ShowFavoritesCommand` |
| Ver recientes | `ShowRecentlyPlayedCommand` |
| Ver completados | `ShowCompletedCommand` |
| Toggle favorito | `ToggleFavoriteCommand` |
| Ver plataformas | `ShowPlatformsCommand` |
| Ver playlists | `ShowPlaylistsCommand` |
| Cargar juegos de plataforma | `LoadGamesForPlatformCommand` |
| Cargar juegos de playlist | `LoadGamesForPlaylistCommand` |

### Faltante en GameLauncher Desktop ❌

| Acción | En LaunchBox |
|--------|-------------|
| Importar ROMs | Tools > Import ROMs |
| Descargar metadata | Tools > Download Metadata |
| Editar juego | Right-click > Edit |
| Editar plataforma | Tools > Manage Platforms |
| Editar emuladores | Tools > Manage Emulators |
| Crear/editar playlist | Tools > Manage Playlists |
| Ordenar por múltiples criterios | Column headers / Sort menu |
| Filtrar por género, developer, etc. | Sidebar field filter |
| Exportar a Android | Tools > Android |
| Opciones/configuración | Tools > Options |
| Buscar en LaunchBox DB | Tools > Search |
| Ver estadísticas globales | Tools > Statistics |

---

## 7.4 Sistema de Búsqueda Desktop

### Implementado ✅

```csharp
// MainViewModel.cs — ApplyFilter()
var filtered = string.IsNullOrWhiteSpace(SearchText)
    ? Games
    : Games.Where(g =>
        g.Title.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
        (g.Developer?.Contains(SearchText, ...) ?? false) ||
        (g.Genre?.Contains(SearchText, ...) ?? false));
```

**Búsqueda:** Solo en plataforma actualmente cargada. No busca entre todas las plataformas a la vez.

**Cuando SearchText no es vacío:** `SearchGamesCommand` llama a `IGameManager.SearchGamesAsync()` que sí busca en la plataforma seleccionada.

### En LaunchBox

LaunchBox tiene búsqueda global que busca en **todas las plataformas simultáneamente** usando SQLite como índice.

**Diferencia crítica:** GameLauncher sin SQLite no puede hacer búsqueda cross-platform eficiente sin cargar todas las plataformas en memoria.

**Solución posible:** `GameCacheManager` ya carga todas las plataformas en background. La búsqueda global podría hacerse sobre el caché en memoria.

---

## 7.5 GameViewModel.cs (10,588 bytes)

**Lo que expone para la UI:**

```csharp
public class GameViewModel : ObservableObject {
    // Propiedades del modelo
    string ID, Title, Platform, Developer, Publisher
    string Genre, Series, Region, Rating, PlayMode
    DateTime? ReleaseDate; int ReleaseYear
    string? Notes, WikipediaURL
    bool Favorite, Completed, Broken, Installed, Portable, Hide
    int PlayCount, StarRating
    float StarRatingFloat, CommunityStarRating
    long PlayTime; string FormattedPlayTime
    DateTime? LastPlayed
    
    // Propiedades calculadas de UI
    BitmapImage? CoverImage
    string? ResolvedImagePath
    Uri? GameVideoUri
    bool HasVideo
    string StatusText  // "Favorito | Completado | Roto"
    
    // Comandos
    LaunchCommand, ToggleFavoriteCommand, ToggleCompletedCommand
    
    // Métodos
    ResolveCoverImagePath() → string?
    ResolveVideoPath() → string?
    RefreshStats()         → void
}
```

---

## 7.6 StatisticsViewModel.cs (4,769 bytes)

Muestra estadísticas de plataforma:

```
- Total de juegos en la plataforma
- Juegos completados / % completado
- Total de tiempo de juego
- Total de sesiones de juego
- Juego más jugado (nombre + tiempo)
```

---

## 7.7 Caché de Imágenes Desktop

### Implementación Actual

**Caché en memoria:** `Dictionary<string, List<GameViewModel>>` — Una entrada por plataforma

**Caché en disco (JSON):**
```
AppDir/cache/images/{PlatformName}.json
{
  "game-guid-1": "H:\\LaunchBox\\Images\\SNES\\Box - Front\\Super Mario World-01.png",
  "game-guid-2": null,
  ...
}
```

**Flujo de carga:**
```
1. ¿Está en caché memoria? → Usar directamente
2. Cargar juegos desde XML
3. ¿Existe caché JSON en disco? → Cargar rutas, crear BitmapImages
4. Sin caché JSON → Buscar rutas en disco (lento), guardar JSON nuevo
5. Guardar en caché memoria
```

**Ventaja:** Primera carga lenta, cargas posteriores muy rápidas.

**Problema potencial:** El caché JSON se invalida si se mueven/agregan imágenes. No hay lógica de invalidación de caché.

---

## 7.8 Tareas Pendientes Desktop

```
🔴 CRÍTICO:
  [ ] Búsqueda cross-platform (buscar en todas las plataformas)
  [ ] Editor de juego (click derecho > editar metadatos)
  [ ] Ordenamiento configurable (por columna)

🟠 IMPORTANTE:
  [ ] Vista de lista (text list) alternativa al grid de portadas
  [ ] Filtros múltiples en sidebar (género, dev, rating, etc.)
  [ ] Gestor de playlists (crear, editar, agregar juegos)
  [ ] Configuración de la aplicación (opciones)
  [ ] Invalidación inteligente del caché de imágenes

🟡 DESEADO:
  [ ] Editor de plataformas
  [ ] Editor de emuladores
  [ ] Tamaño de portadas configurable
  [ ] Columnas configurables en vista de lista
  [ ] Estadísticas globales (todas las plataformas)
  [ ] Exportar lista de juegos (CSV, HTML)
```
