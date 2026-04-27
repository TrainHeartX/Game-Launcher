# 06 — UI BIGSCREEN: Análisis Detallado

## 6.1 Vistas Actuales vs LaunchBox BigBox

### PlatformFiltersView (Navegación Principal)

**Lo que hace:**
- Muestra el árbol de categorías/plataformas/playlists en drill-down
- Al seleccionar una plataforma: muestra su logo + video
- Al seleccionar una playlist/saga: muestra logo, lista de juegos con portadas, estadísticas
- Navega con D-pad, entra con A, retrocede con B

**Diferencias con LaunchBox BigBox:**

| Aspecto | LaunchBox BigBox | GameLauncher BigScreen |
|---------|-----------------|----------------------|
| Layout visual | Wheel horizontal con imágenes grandes | Lista vertical con logos |
| Animaciones | Transiciones suaves configurables | Sin transiciones |
| Fondo dinámico | Fanart/video del item seleccionado | Solo imagen pequeña |
| Ruido/efecto | Efectos de sonido al navegar | Sin sonido |
| Contador de juegos | Visible en el tile | ✅ Visible (se carga async) |
| Categorías | Íconos/imágenes de categoría | Solo ícono emoji |
| Vista alternativa | Wall / CoverFlow / TextList | Solo lista |

---

### GamesWheelView (Vista de Juegos)

**Lo que hace:**
- Rueda horizontal de portadas de juegos
- Panel de detalles con: título, developer, género, año, tiempo, veces jugado, notas, publisher, serie, región
- Video del juego (con delay 500ms para no lagear al pasar rápido)
- Menú de gestión (A button): toggle instalado/favorito/completado/roto, rating
- Editor de metadatos (desde menú gestión)
- Galería de imágenes (X button): grid navegable con zoom

**Diferencias con LaunchBox BigBox:**

| Aspecto | LaunchBox BigBox | GameLauncher BigScreen |
|---------|-----------------|----------------------|
| Layout principal | Configurable (wheel/wall/list) | Solo HorizontalWheel |
| Tamaño de portadas | Configurable | Fijo |
| Efecto de rueda | 3D con perspectiva (HorizontalWheel3) | 2D plano |
| Ordenamiento | Múltiples opciones | Hardcoded: fecha + título |
| Búsqueda | Búsqueda con teclado virtual | ❌ Sin búsqueda |
| Filtros | Por género, rating, año, etc. | ❌ Sin filtros |
| Badges | Multiple Versions, RA, Favorito, etc. | ❌ Sin badges |
| Marquesina | Imagen marquesina en arcade | ❌ No mostrada |
| 3D models | Modelos 3D de cartuchos/consolas | ❌ No implementado |
| Sonido nav | ✅ Efectos de sonido | ❌ Sin sonido |

---

### GameDetailsView (Detalles del Juego)

**Lo que hace:**
- Vista de detalles expandida del juego seleccionado
- Galería de imágenes completa
- Información técnica

**Diferencias:**

| Aspecto | LaunchBox BigBox | GameLauncher BigScreen |
|---------|-----------------|----------------------|
| Manual PDF | ✅ Lector PDF integrado | ❌ No implementado |
| Video trailer | ✅ Multiple tipos de video | 🟡 Solo Video Snap/Theme |
| Retroachievements | ✅ Badge de logros | ❌ No implementado |
| Cheat codes | ✅ Integrado | ❌ No implementado |
| Wikipedia | ✅ Browser integrado | ❌ No implementado |
| Additional apps | ✅ Aplicaciones adicionales | ❌ No mostrado |

---

## 6.2 Sistema de Input BigScreen

### Lo que está implementado ✅

```csharp
// MainWindow.xaml.cs — manejo de gamepad
// Usando SharpDX.XInput o similar
private void HandleGamepadInput() {
    // D-Pad: navegación
    // A: confirmar/abrir
    // B: atrás
    // X: galería imágenes  
    // Y: favorito
    // LB/RB: sección en editor
    // Select+Start: kill proceso
}
```

### Lo que falta ❌

| Función | LaunchBox | GameLauncher |
|---------|-----------|-------------|
| Teclado virtual (búsqueda) | ✅ | ❌ |
| Hotkeys configurables | ✅ | ❌ |
| Soporte multi-gamepad | ✅ | 🟡 Solo XInput |
| DInput support | ✅ | ❌ |
| Keyboard navigation | ✅ | ❌ |
| Mouse navigation | ✅ | ❌ |

---

## 6.3 Resolución de Imágenes en BigScreen

### Portadas de Juegos (GameItem.ResolveCoverImagePath)

**Prioridad implementada:**
```
1. Box - Front
2. Box - Front - Reconstructed
3. Fanart - Box - Front
4. Steam Poster
5. Epic Games Poster
6. GOG Poster
7. Origin Poster
8. Box - 3D
9. Banner
10. Clear Logo
```

**Búsqueda en subdirectorios de región:** ✅ (busca en carpetas dentro de cada tipo)

**Sanitización de nombre de archivo:** ✅
```
: → _
/ → _
\ → _
? → _
* → _
" → _
< → _
> → _
| → _
' → _
```

**Diferencias con LaunchBox:**
- LaunchBox lee la prioridad desde `Settings.xml > DefaultImageGroup`
- GameLauncher tiene la prioridad hardcodeada
- LaunchBox busca también por nombre alternativo del juego
- GameLauncher solo busca por título principal

### Imágenes de Plataforma

**Prioridad implementada:** Clear Logo → Fanart → Banner

**Ubicación buscada:** `Images/Platforms/{platformName}/{subfolder}/`

✅ Correcto (mismo que LaunchBox)

### Imágenes de Categoría

**Ubicación buscada:** `Images/Platform Categories/{categoryName}/{subfolder}/`

✅ Correcto

### Imágenes de Playlist/Saga

**Prioridad:** ClearLogoImagesFolder (si configurado en Playlist model) → Clear Logo → Fanart → Banner

**Ubicación:** `Images/Playlists/{playlistName}/{subfolder}/`

✅ Correcto y extendido con propiedad custom

---

## 6.4 Reproducción de Video en BigScreen

### PlatformFiltersView — Video de Plataforma

```
Busca en: Videos/Platforms/{platformName}.mp4
```

✅ Correcto (coincide con LaunchBox)

### PlatformFiltersView — Video de Playlist

```
Prioridad:
1. Playlist.VideoPath (absoluto o relativo)
2. Videos/Playlists/{playlistName}.mp4/.wmv/.avi/.mkv
```

✅ Extendido vs LaunchBox base

### GamesWheelView — Video de Juego

```
Busca en: Videos/{Platform}/Video Snap/{Title}-01.mp4
          Videos/{Platform}/Theme Video/{Title}-01.mp4
```

**Delay de 500ms** para evitar carga innecesaria al pasar rápido ✅

**Problema detectado:** GameLauncher usa `MediaElement` de WPF que solo soporta formatos Windows Media (WMV, AVI, MP4 con codec H.264). Los MKV y algunos codecs no funcionarán.

**LaunchBox:** Usa VLC que soporta prácticamente todos los formatos.

---

## 6.5 Editor de Metadatos (BigScreen)

### GameEditorViewModel.cs (30,945 bytes — muy completo)

**Secciones del editor:**

| Sección | Campos editables |
|---------|-----------------|
| Información | Title, Platform, Developer, Publisher |
| Descripción | Genre, Year, Region, Rating, PlayMode, Series, Status, ReleaseType, MaxPlayers, Notes |
| Estado | Favorite (bool), Completed (bool), Broken (bool), Installed (bool) |

**Navegación:** LB/RB para cambiar sección, D-pad para campo, A para editar, B para cancelar

**Tipos de campo:**
- Text: input directo con teclado
- YesNo: toggle true/false
- List: lista desplegable con opciones predefinidas
- Number: entrada numérica

**Guarda en:** XML directamente via `IGameManager.UpdateGameAsync()`

**Diferencias con LaunchBox:**
- LaunchBox tiene editor visual más rico con imágenes
- LaunchBox permite editar la ruta de la ROM
- LaunchBox permite gestionar imágenes desde el editor
- LaunchBox tiene scraping desde el editor (buscar en DB)

---

## 6.6 Vista SystemInfoView

```csharp
// SystemInfoViewModel.cs — Muestra información del sistema
// CPU, RAM, GPU, almacenamiento
// Versión de la app
// Estadísticas de la biblioteca
```

**En LaunchBox BigBox:** No existe una vista de "System Info" como tal.

Esta vista es una **extensión propia** de GameLauncher, no presente en LaunchBox.

---

## 6.7 Tareas Pendientes BigScreen (Ordenadas por Impacto)

```
🔴 CRÍTICO:
  [ ] Búsqueda de juegos con teclado virtual en BigScreen
  [ ] Ordenamiento configurable de juegos (fecha, título, jugados)
  [ ] Filtros básicos (favoritos, completados, instalados)

🟠 IMPORTANTE:
  [ ] Efectos de sonido al navegar (cargar Sci-Fi Set 6)
  [ ] Música de fondo por plataforma
  [ ] Transiciones animadas entre vistas
  [ ] Attract Mode (reproduce videos cuando está inactivo)
  [ ] Vista tipo TextList (sin imágenes, solo texto) para plataformas grandes

🟡 DESEADO:
  [ ] Pantalla de inicio al lanzar juego (StartupScreen)
  [ ] Pantalla de pausa durante el juego
  [ ] Multiple tipos de wheel/wall
  [ ] Badges de estado (favorito, completado, RA, etc.)
  [ ] Soporte de temas XAML externos
```
