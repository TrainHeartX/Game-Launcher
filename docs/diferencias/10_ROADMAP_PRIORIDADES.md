# 10 — ROADMAP Y PRIORIDADES: Plan de Acción

## 10.1 Visión

Convertir GameLauncher de un "visor de biblioteca LaunchBox" a un **frontend independiente completo** capaz de:
1. Crear y gestionar su propia biblioteca de juegos
2. Ser usado como alternativa completa a LaunchBox Desktop + BigBox
3. Mantener compatibilidad total con los datos XML de LaunchBox

---

## 10.2 Fase 1: Correcciones de Compatibilidad (1-2 semanas)

**Objetivo:** Garantizar que los datos no se corrompan al guardar.

### 1.1 Fix: CommunityStarRatingTotalCount

**Archivo:** `src/Core/GameLauncher.Core/Models/Game.cs`

```csharp
// ANTES:
public int CommunityStarRatingTotalVotes { get; set; }

// DESPUÉS:
[XmlElement("CommunityStarRatingTotalCount")]
public int CommunityStarRatingTotalVotes { get; set; }
```
**Tiempo:** 5 minutos

---

### 1.2 Fix: LastPlayedDate

**Archivo:** `src/Core/GameLauncher.Core/Models/Game.cs`

```csharp
// AGREGAR campo:
public DateTime? LastPlayedDate { get; set; }

// MODIFICAR LastPlayed calculado:
public DateTime? LastPlayed => LastPlayedDate ?? (PlayCount > 0 ? DateModified : null);
```

**Archivo:** `src/Core/GameLauncher.Infrastructure/Services/StatisticsTracker.cs`

```csharp
// En RecordPlaySessionAsync():
game.LastPlayedDate = DateTime.Now;
game.DateModified = DateTime.Now;
```
**Tiempo:** 30 minutos

---

### 1.3 Fix: LaunchBoxDbId

**Archivo:** `src/Core/GameLauncher.Core/Models/Game.cs`

```csharp
public int? LaunchBoxDbId { get; set; }
```
**Tiempo:** 5 minutos

---

### 1.4 Fix: Prioridad de imágenes desde Settings.xml

**Problema:** La prioridad de tipos de imagen está hardcodeada.
**Solución:** Leer `DefaultImageGroup` y `ImageTypeSettings` de `Settings.xml`.

**Archivo a modificar:** `GamesWheelViewModel.cs` + `GameViewModel.cs`

```csharp
// En App.xaml.cs o SettingsManager
var settings = _dataContext.LoadSettings();
var imageTypePriority = settings.GetImageTypePriority();
// Pasar como parámetro a los ViewModels
```
**Tiempo:** 2-3 horas

---

### 1.5 Fix: UseCustomCommandLine del juego

**Archivo:** `src/Core/GameLauncher.Core/Models/Game.cs`

```csharp
public bool UseCustomCommandLine { get; set; }
```

**Archivo:** `EmulatorLauncher.cs` — usar el flag:
```csharp
var commandLine = game.UseCustomCommandLine && !string.IsNullOrWhiteSpace(game.CommandLine)
    ? game.CommandLine
    : ...
```
**Tiempo:** 30 minutos

---

## 10.3 Fase 2: Funcionalidades Core Faltantes (2-4 semanas)

**Objetivo:** Hacer GameLauncher usable de forma independiente.

### 2.1 Búsqueda Cross-Platform (ALTA PRIORIDAD)

**Qué hacer:** Aprovechar `GameCacheManager` que ya carga todas las plataformas en background para implementar búsqueda global.

```csharp
// GameManager.cs — nuevo método
public async Task<List<Game>> SearchAllPlatformsAsync(string searchText) {
    await _cacheManager.WaitForFullLoadAsync();
    return _cacheManager.GetAllGames()
        .Where(g => g.Title.Contains(searchText, StringComparison.OrdinalIgnoreCase)
                 || (g.Developer?.Contains(...) ?? false)
                 || (g.Genre?.Contains(...) ?? false))
        .Take(500)  // Limitar resultados
        .ToList();
}
```
**Tiempo:** 4-8 horas

---

### 2.2 Editor de Juego en Desktop (ALTA PRIORIDAD)

**Qué hacer:** Click derecho en juego → modal de edición de metadatos.

El `GameEditorViewModel.cs` ya está completo en BigScreen. Solo falta:
1. Una ventana WPF Desktop para mostrarlo
2. Conectar con MainViewModel

**Tiempo:** 6-10 horas

---

### 2.3 Ordenamiento Configurable (MEDIA PRIORIDAD)

**Desktop:**
```csharp
// MainViewModel.cs
public SortField CurrentSortField { get; set; }
public bool SortDescending { get; set; }

private IEnumerable<GameViewModel> SortGames(IEnumerable<GameViewModel> games) {
    return CurrentSortField switch {
        SortField.Title => SortDescending 
            ? games.OrderByDescending(g => g.Title) 
            : games.OrderBy(g => g.Title),
        SortField.LastPlayed => games.OrderByDescending(g => g.LastPlayed),
        SortField.PlayCount => games.OrderByDescending(g => g.PlayCount),
        SortField.ReleaseDate => games.OrderBy(g => g.ReleaseDate),
        SortField.StarRating => games.OrderByDescending(g => g.StarRating),
        _ => games.OrderBy(g => g.Title)
    };
}
```
**Tiempo:** 4-6 horas

---

### 2.4 Filtros Básicos en Desktop (MEDIA PRIORIDAD)

**Filtros a implementar:**
- Solo favoritos
- Solo completados
- Solo instalados
- Por género (dropdown)
- Por developer (dropdown)
- Por rating (>=N estrellas)

**Tiempo:** 8-12 horas

---

### 2.5 Gestor de Playlists (MEDIA PRIORIDAD)

**Funcionalidades:**
- Ver todas las playlists
- Crear nueva playlist
- Agregar juego a playlist (click derecho)
- Quitar juego de playlist
- Renombrar/eliminar playlist

**Tiempo:** 12-20 horas

---

### 2.6 Música de Fondo (FÁCIL - ALTO IMPACTO) ⭐

**Qué hacer:** Reproducir música de `Music/{Platform}/` cuando se carga una plataforma.

```csharp
// En GamesWheelViewModel.LoadGamesAsync()
var musicFolder = Path.Combine(launchBoxPath, "Music", platformName);
if (Directory.Exists(musicFolder)) {
    var tracks = Directory.GetFiles(musicFolder, "*.mp3").Concat(...).ToList();
    _musicPlayer.PlayShuffle(tracks);
}
```
**Tiempo:** 4-6 horas — **Gran mejora de experiencia con poco esfuerzo**

---

### 2.7 Efectos de Sonido BigScreen (FÁCIL - ALTO IMPACTO) ⭐

**Qué hacer:** Reproducir efectos de sonido al navegar usando el pack activo.

```csharp
// SoundPlayer simple
var soundPack = settings.BigBoxSettings.SoundPackName; // "Sci-Fi Set 6 by Clavius"
var soundPath = Path.Combine(launchBoxPath, "Sounds", soundPack);

// Al navegar:
PlaySound(Path.Combine(soundPath, "NavigationLeft.wav"));
```
**Tiempo:** 3-4 horas — **Gran mejora de experiencia con poco esfuerzo**

---

## 10.4 Fase 3: Mejoras de UI (1-2 meses)

### 3.1 Vista de Lista en BigScreen (TextList)

Para plataformas con miles de juegos (Arcade con 18,900 ROMs), una lista de texto es mucho más eficiente:

```xaml
<!-- ListView simple en lugar de rueda horizontal -->
<ListView ItemsSource="{Binding Games}" VirtualizingPanel.IsVirtualizing="True">
    <ListView.ItemTemplate>
        <DataTemplate>
            <StackPanel Orientation="Horizontal">
                <TextBlock Text="{Binding Title}" />
                <TextBlock Text="{Binding Developer}" />
                <TextBlock Text="{Binding ReleaseYear}" />
            </StackPanel>
        </DataTemplate>
    </ListView.ItemTemplate>
</ListView>
```
**Tiempo:** 8-12 horas

---

### 3.2 Transiciones Animadas BigScreen

```csharp
// En NavigationService.Navigate()
// Usar Storyboard/DoubleAnimation para fade/slide
var storyboard = new Storyboard();
// FadeOut → Navigate → FadeIn
```
**Tiempo:** 6-10 horas

---

### 3.3 Badges de Estado en GamesWheelView

Mostrar iconos pequeños sobre las portadas:
- ⭐ Favorito
- ✓ Completado
- 🏆 Tiene RetroAchievements
- ❌ Roto

```xaml
<Grid>
    <Image Source="{Binding CoverImage}" />
    <StackPanel Orientation="Horizontal" VerticalAlignment="Top">
        <TextBlock Text="⭐" Visibility="{Binding Favorite, Converter=...}" />
        <TextBlock Text="✓" Visibility="{Binding Completed, Converter=...}" />
    </StackPanel>
</Grid>
```
**Tiempo:** 4-6 horas

---

### 3.4 Attract Mode

```csharp
// En MainWindow.xaml.cs — después de N segundos sin input
private void StartAttractMode() {
    // Cargar lista de juegos con video
    // Reproducir videos aleatoriamente en pantalla completa
    // Detener al detectar input
}
```
**Tiempo:** 12-16 horas

---

## 10.5 Fase 4: Importador de ROMs (Gran Proyecto)

**Esta es la funcionalidad más compleja y más necesaria para independencia total.**

### Plan de Implementación

```
ImportRomsWizard
  ├── Paso 1: Seleccionar carpeta de ROMs
  ├── Paso 2: Seleccionar plataforma
  ├── Paso 3: Escanear archivos (con extensiones válidas para la plataforma)
  ├── Paso 4: Previsualizar lista de juegos encontrados
  ├── Paso 5: Opciones (detección de duplicados, formato de nombre)
  └── Paso 6: Importar (crear Game objects y guardar en XML)
```

**Sin scraping:** Versión mínima que solo crea entradas en el XML con título = nombre del archivo.

**Con scraping local:** Buscar en una base de datos offline (ej: No-Intro DAT files) para enriquecer metadata.

**Tiempo:** 40-80 horas (solo importador básico)

---

## 10.6 Prioridad Recomendada Inmediata

### Esta semana (quick wins de alto impacto):

```
✅ 1 hora:   Fix CommunityStarRatingTotalCount
✅ 2 horas:  Fix LastPlayedDate
✅ 4 horas:  Efectos de sonido BigScreen (cargar Sci-Fi Set 6)
✅ 4 horas:  Música de fondo por plataforma
✅ 6 horas:  Búsqueda en BigScreen (teclado virtual simple)
```

### Próximas 2 semanas:

```
▷ Ordenamiento configurable (title, fecha, jugados, rating)
▷ Filtros rápidos (favoritos, completados, instalados)
▷ Editor de juego en Desktop (reutilizar GameEditorViewModel)
▷ Transiciones animadas básicas
```

---

## 10.7 Métricas Objetivo

| Métrica | Ahora | Meta Fase 1 | Meta Fase 2 | Meta Final |
|---------|-------|------------|------------|-----------|
| Compatibilidad datos | 90% | 98% | 98% | 99% |
| Funcionalidades BigBox | 40% | 45% | 65% | 80% |
| Funcionalidades Desktop | 50% | 55% | 75% | 85% |
| Funcionalidades sistema | 45% | 50% | 65% | 75% |
| Completitud global | ~45% | ~52% | ~70% | ~85% |

---

## 10.8 Lo Que NUNCA Necesitará Replicar

Algunas características de LaunchBox son exclusivas del ecosistema comercial y no tienen sentido replicar:

- **LaunchBox Games DB** — Base de datos propietaria de LaunchBox LLC
- **EmuMovies** — Servicio de pago de terceros
- **Cloud Sync** — Servidor propietario de LaunchBox
- **Licencia Premium** — Sistema de activación

Alternativas open-source a considerar:
- **IGDB API** (Twitch/Activision) — Metadata gratuita
- **TheGamesDB** — Metadata open-source
- **ScreenScraper** — Imágenes/videos (requiere cuenta gratuita)
- **Archive.org** — Imágenes/videos históricos
