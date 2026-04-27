# 04 — FUNCIONALIDADES IMPLEMENTADAS

## 4.1 Capa de Datos — ✅ Casi Completo

### XmlDataContext (28,672 bytes — 755 líneas)

**Operaciones disponibles:**

| Operación | Estado |
|-----------|--------|
| Cargar plataformas | ✅ |
| Cargar categorías de plataforma | ✅ |
| Guardar plataformas | ✅ |
| Cargar juegos por plataforma | ✅ |
| Guardar juegos por plataforma | ✅ |
| Carga optimizada de plataforma (todos los tipos a la vez) | ✅ |
| Cargar/Guardar AdditionalApplications | ✅ |
| Cargar/Guardar CustomFields | ✅ |
| Cargar/Guardar GameControllerSupport | ✅ |
| Cargar/Guardar AlternateNames | ✅ |
| Cargar emuladores y mappings | ✅ |
| Guardar emuladores y mappings | ✅ |
| Cargar Parents (jerarquía) | ✅ |
| Guardar Parents | ✅ |
| Listar playlists | ✅ |
| Cargar/Guardar playlists | ✅ |
| Cargar GameControllers | ✅ |
| Cargar InputBindings | ✅ |
| Cargar/Guardar ListCache | ✅ |
| Cargar/Guardar ImportBlacklist | ✅ |
| Cargar Settings | ✅ |
| Guardar Settings | ✅ |
| Cargar BigBoxSettings | ✅ |
| Guardar BigBoxSettings | ✅ |

---

## 4.2 Lanzamiento de Juegos — ✅ Sólido

### EmulatorLauncher.cs (515 líneas)

**Características implementadas:**

1. **Resolución de emulador por juego:**
   - Primero busca el emulador específico del juego (`Game.Emulator` GUID)
   - Si no tiene asignado, busca el emulador por defecto para la plataforma
   - Fallback a lanzamiento directo para ejecutables Windows

2. **Construcción de línea de comando:**
   ```
   Placeholders soportados:
   {rom}      → Ruta completa del ROM (con comillas si tiene espacios)
   {romraw}   → Ruta completa sin comillas
   {rompath}  → Directorio del ROM
   {romfile}  → Nombre del archivo ROM con extensión
   {romname}  → Nombre del archivo ROM sin extensión
   {emudir}   → Directorio del emulador
   {emupath}  → Ruta completa del emulador
   {platform} → Nombre de la plataforma
   {title}    → Título del juego
   ```

3. **Extracción automática de ROMs comprimidas:**
   - Detecta archivos `.zip`, `.rar`, `.7z`
   - Extrae a directorio temporal
   - Busca el ROM exacto dentro del archivo
   - Limpia el directorio temporal al terminar

4. **Lanzamiento directo** (sin emulador):
   - `.exe`, `.bat`, `.cmd`, `.lnk`, `.url`

5. **Medición de tiempo de juego:**
   - Registra `startTime` y `endTime`
   - Calcula `PlayTimeSeconds` exacto

6. **Kill de proceso:**
   - `KillCurrentProcess()` mata el árbol completo de procesos

7. **Validación previa:**
   - `CanLaunchGameAsync()` verifica existencia del ROM y emulador antes de lanzar

---

## 4.3 Árbol de Navegación — ✅ Completo

### PlatformManager.GetNavigationTreeAsync()

Construye exactamente la misma jerarquía que LaunchBox lee de `Parents.xml`:

```
Categoría Raíz
  └── Subcategoría
        ├── Plataforma
        │     └── [juegos]
        └── Playlist
              └── [juegos de la playlist]
```

**Características:**
- Lee `Parents.xml` para construir la jerarquía
- Lee `Platforms.xml` para metadata de plataformas
- Lee `PlatformCategory` de `Platforms.xml`
- Lee todas las playlists de `Playlists/`
- Plataformas sin categoría van a "Sin Categoría" automáticamente
- Ordenamiento recursivo de nodos
- Conteo de juegos por plataforma en paralelo

---

## 4.4 UI BigScreen — 🟡 Parcial pero Funcional

### Vistas disponibles en BigScreen:

| Vista | Estado | Descripción |
|-------|--------|-------------|
| `HomeView` | ✅ | Pantalla de inicio |
| `PlatformFiltersView` | ✅ | Navegación drill-down por categorías |
| `GamesWheelView` | ✅ | Rueda horizontal de juegos |
| `GameDetailsView` | ✅ | Detalles completos de juego |
| `SystemInfoView` | ✅ | Información del sistema |
| `SourcesView` | ✅ | Vista de fuentes |

### Características BigScreen implementadas:

1. **Navegación con gamepad:**
   - D-Pad navegación en todas las vistas
   - Botón A: Abrir/Seleccionar
   - Botón B: Atrás
   - Botón X: Galería de imágenes
   - Botón Y: Favorito
   - Select+Start: Matar proceso

2. **Galería de imágenes:**
   - Muestra todos los tipos de imagen disponibles
   - Zoom con botón B
   - Navegación 2D con D-pad

3. **Editor de metadatos del juego:**
   - Edita: Title, Platform, Developer, Publisher, Genre, Year, Region
   - Edita: Rating, PlayMode, Series, Status, ReleaseType, MaxPlayers, Notes
   - Edita: Favorite, Completed, Broken, Installed
   - Navega por secciones con L/R bumpers

4. **Menú de gestión rápida:**
   - Toggle: Instalado, Favorito, Completado, Roto
   - Rating de 1-5 estrellas
   - Acceso al editor de metadatos

5. **Vista previa de Sagas (Playlists):**
   - Logo de la saga
   - Lista de juegos con portadas
   - Año de rango
   - Estado de completado
   - Metadata de Notes (status, fecha revisión, juegos faltantes)

6. **Resolución de imágenes:**
   - Portadas de juegos con fallback a 9 tipos de imagen
   - Logos de plataformas (Clear Logo, Banner)
   - Imágenes de categorías
   - Imágenes de playlists/sagas

7. **Reproducción de video:**
   - Video del juego seleccionado (delay 500ms)
   - Video de plataforma
   - Video de playlist

---

## 4.5 UI Desktop — 🟡 Parcial

### MainViewModel.cs (784 líneas) — Funcionalidades:

| Funcionalidad | Estado |
|---------------|--------|
| Carga de plataformas | ✅ |
| Árbol de categorías en sidebar | ✅ |
| Lista de playlists en sidebar | ✅ |
| Carga de juegos por plataforma | ✅ |
| Caché en memoria + disco | ✅ |
| Búsqueda básica (título, dev, género) | ✅ |
| Ver favoritos | ✅ |
| Ver recientes | ✅ |
| Ver completados | ✅ |
| Lanzar juego seleccionado | ✅ |
| Toggle favorito | ✅ |
| Estadísticas por plataforma | ✅ |
| Video del juego seleccionado | ✅ |
| Cargar juegos de playlist | ✅ |

---

## 4.6 Sistema de Estadísticas — ✅ Funcional

### StatisticsTracker.cs

- Registra sesiones de juego (`RecordPlaySessionAsync`)
- Actualiza `PlayCount` y `PlayTime` en el XML
- Actualiza `DateModified` (que actúa como `LastPlayedDate`)
- Calcula estadísticas por plataforma:
  - Total de juegos
  - Juegos completados
  - Total de tiempo de juego
  - Total de veces jugado
  - Juego más jugado

---

## 4.7 GameCacheManager — ✅ Funcional

### GameCacheManager.cs

- Caché en memoria por plataforma (Dictionary<string, List<Game>>)
- Pre-carga de todas las plataformas en background
- Caché de plataformas (para Platform list)
- Búsqueda cruzada entre plataformas (para GetGameByIdAsync)
- Thread-safe con locks

---

## 4.8 GameManager — ✅ Funcional

### GameManager.cs (8,091 bytes)

| Operación | Estado |
|-----------|--------|
| Obtener juegos por plataforma | ✅ |
| Buscar juegos (texto libre) | ✅ |
| Obtener juego por ID | ✅ |
| Actualizar juego | ✅ |
| Obtener favoritos (cross-platform) | ✅ |
| Obtener recientes jugados | ✅ |
| Obtener completados | ✅ |
