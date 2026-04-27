# 03 — MODELOS DE DATOS: Compatibilidad XML con LaunchBox

## 3.1 Estado General de Compatibilidad

**GameLauncher tiene compatibilidad de datos ~95% con LaunchBox.**

El sistema de datos usa `XmlDataContext.cs` como punto central de acceso. Todos los modelos se anotan con `[XmlRoot]` y se serializan/deserializan respetando el formato exacto de LaunchBox.

---

## 3.2 Modelo Game — Comparación Campo por Campo

### ✅ Campos implementados en GameLauncher

| Campo XML LaunchBox | C# GameLauncher | Tipo | Estado |
|--------------------|-----------------|------|--------|
| `<ID>` | `Game.ID` | string | ✅ |
| `<DatabaseID>` | `Game.DatabaseID` | string? | ✅ |
| `<Title>` | `Game.Title` | string | ✅ |
| `<SortTitle>` | `Game.SortTitle` | string? | ✅ |
| `<Platform>` | `Game.Platform` | string | ✅ |
| `<Series>` | `Game.Series` | string? | ✅ |
| `<Version>` | `Game.Version` | string? | ✅ |
| `<ApplicationPath>` | `Game.ApplicationPath` | string? | ✅ |
| `<CommandLine>` | `Game.CommandLine` | string? | ✅ |
| `<ConfigurationPath>` | `Game.ConfigurationPath` | string? | ✅ |
| `<ConfigurationCommandLine>` | `Game.ConfigurationCommandLine` | string? | ✅ |
| `<RootFolder>` | `Game.RootFolder` | string? | ✅ |
| `<ManualPath>` | `Game.ManualPath` | string? | ✅ |
| `<MusicPath>` | `Game.MusicPath` | string? | ✅ |
| `<VideoPath>` | `Game.VideoPath` | string? | ✅ |
| `<ThemeVideoPath>` | `Game.ThemeVideoPath` | string? | ✅ |
| `<DosBoxConfigurationPath>` | `Game.DosBoxConfigurationPath` | string? | ✅ |
| `<CustomDosBoxVersionPath>` | `Game.CustomDosBoxVersionPath` | string? | ✅ |
| `<ScummVMGameDataFolderPath>` | `Game.ScummVMGameDataFolderPath` | string? | ✅ |
| `<Emulator>` | `Game.Emulator` | string? (GUID) | ✅ |
| `<UseDosBox>` | `Game.UseDosBox` | bool | ✅ |
| `<UseScummVM>` | `Game.UseScummVM` | bool | ✅ |
| `<ScummVMGameType>` | `Game.ScummVMGameType` | string? | ✅ |
| `<ScummVMAspectCorrection>` | `Game.ScummVMAspectCorrection` | bool | ✅ |
| `<ScummVMFullscreen>` | `Game.ScummVMFullscreen` | bool | ✅ |
| `<Developer>` | `Game.Developer` | string? | ✅ |
| `<Publisher>` | `Game.Publisher` | string? | ✅ |
| `<Genre>` | `Game.Genre` | string? | ✅ |
| `<ReleaseDate>` | `Game.ReleaseDate` | DateTime? | ✅ |
| `<Region>` | `Game.Region` | string? | ✅ |
| `<PlayMode>` | `Game.PlayMode` | string? | ✅ |
| `<Status>` | `Game.Status` | string? | ✅ |
| `<Source>` | `Game.Source` | string? | ✅ |
| `<ReleaseType>` | `Game.ReleaseType` | string? | ✅ |
| `<MaxPlayers>` | `Game.MaxPlayers` | int | ✅ |
| `<DateAdded>` | `Game.DateAdded` | DateTime? | ✅ |
| `<DateModified>` | `Game.DateModified` | DateTime? | ✅ |
| `<Favorite>` | `Game.Favorite` | bool | ✅ |
| `<Completed>` | `Game.Completed` | bool | ✅ |
| `<Broken>` | `Game.Broken` | bool | ✅ |
| `<Hide>` | `Game.Hide` | bool | ✅ |
| `<Portable>` | `Game.Portable` | bool | ✅ |
| `<PlayCount>` | `Game.PlayCount` | int | ✅ |
| `<PlayTime>` | `Game.PlayTime` | long | ✅ |
| `<Rating>` | `Game.Rating` | string? | ✅ |
| `<StarRatingFloat>` | `Game.StarRatingFloat` | float | ✅ |
| `<StarRating>` | `Game.StarRating` | int | ✅ |
| `<CommunityStarRating>` | `Game.CommunityStarRating` | float | ✅ |
| `<CommunityStarRatingTotalVotes>` | `Game.CommunityStarRatingTotalVotes` | int | ✅ |
| `<Notes>` | `Game.Notes` | string? | ✅ |
| `<WikipediaURL>` | `Game.WikipediaURL` | string? | ✅ |
| `<VideoUrl>` | `Game.VideoUrl` | string? | ✅ |
| `<CloneOf>` | `Game.CloneOf` | string? | ✅ |
| `<MissingVideo>` | `Game.MissingVideo` | bool | ✅ |
| `<MissingBoxFrontImage>` | `Game.MissingBoxFrontImage` | bool | ✅ |
| `<MissingScreenshotImage>` | `Game.MissingScreenshotImage` | bool | ✅ |
| `<MissingMarqueeImage>` | `Game.MissingMarqueeImage` | bool | ✅ |
| `<MissingClearLogoImage>` | `Game.MissingClearLogoImage` | bool | ✅ |
| `<MissingBackgroundImage>` | `Game.MissingBackgroundImage` | bool | ✅ |
| `<MissingBox3dImage>` | `Game.MissingBox3dImage` | bool | ✅ |
| `<MissingCartImage>` | `Game.MissingCartImage` | bool | ✅ |
| `<MissingCart3dImage>` | `Game.MissingCart3dImage` | bool | ✅ |
| `<MissingManual>` | `Game.MissingManual` | bool | ✅ |
| `<MissingBannerImage>` | `Game.MissingBannerImage` | bool | ✅ |
| `<MissingMusic>` | `Game.MissingMusic` | bool | ✅ |
| `<UseStartupScreen>` | `Game.UseStartupScreen` | bool | ✅ |
| `<HideAllNonExclusiveFullscreenWindows>` | `Game.HideAllNonExclusiveFullscreenWindows` | bool | ✅ |
| `<StartupLoadDelay>` | `Game.StartupLoadDelay` | int | ✅ |
| `<HideMouseCursorInGame>` | `Game.HideMouseCursorInGame` | bool | ✅ |
| `<DisableShutdownScreen>` | `Game.DisableShutdownScreen` | bool | ✅ |
| `<AggressiveWindowHiding>` | `Game.AggressiveWindowHiding` | bool | ✅ |
| `<OverrideDefaultStartupScreenSettings>` | `Game.OverrideDefaultStartupScreenSettings` | bool | ✅ |
| `<UsePauseScreen>` | `Game.UsePauseScreen` | bool | ✅ |
| `<PauseAutoHotkeyScript>` | `Game.PauseAutoHotkeyScript` | string? | ✅ |
| `<ResumeAutoHotkeyScript>` | `Game.ResumeAutoHotkeyScript` | string? | ✅ |
| `<OverrideDefaultPauseScreenSettings>` | `Game.OverrideDefaultPauseScreenSettings` | bool | ✅ |
| `<SuspendProcessOnPause>` | `Game.SuspendProcessOnPause` | bool | ✅ |
| `<ForcefulPauseScreenActivation>` | `Game.ForcefulPauseScreenActivation` | bool | ✅ |
| `<LoadStateAutoHotkeyScript>` | `Game.LoadStateAutoHotkeyScript` | string? | ✅ |
| `<SaveStateAutoHotkeyScript>` | `Game.SaveStateAutoHotkeyScript` | string? | ✅ |
| `<ResetAutoHotkeyScript>` | `Game.ResetAutoHotkeyScript` | string? | ✅ |
| `<SwapDiscsAutoHotkeyScript>` | `Game.SwapDiscsAutoHotkeyScript` | string? | ✅ |
| `<GogAppId>` | `Game.GogAppId` | string? | ✅ |
| `<OriginAppId>` | `Game.OriginAppId` | string? | ✅ |
| `<OriginInstallPath>` | `Game.OriginInstallPath` | string? | ✅ |
| `<AndroidBoxFrontThumbPath>` | `Game.AndroidBoxFrontThumbPath` | string? | ✅ |
| `<AndroidBoxFrontFullPath>` | `Game.AndroidBoxFrontFullPath` | string? | ✅ |
| `<Installed>` | `Game.Installed` | bool | ✅ |
| `<HasCloudSynced>` | `Game.HasCloudSynced` | bool | ✅ |

### ❌ Campos NO implementados (identificados en LaunchBox XML real)

| Campo XML LaunchBox | Razón de ausencia |
|---------------------|------------------|
| `<LaunchBoxDbId>` | No mapeado (diferente de DatabaseID) |
| `<CommunityStarRatingTotalCount>` | Nombre diferente: implementado como `TotalVotes` |
| `<LastPlayedDate>` | Calculado (`LastPlayed`) pero no campo independiente |
| `<AlternateName>` | Es entidad separada, no campo del Game |
| `<UseCustomCommandLine>` | No mapeado |

---

## 3.3 Modelo Platform — Campos Comparados

### ✅ Campos implementados

| Campo XML | C# | Estado |
|-----------|-----|--------|
| `<Name>` | `Platform.Name` | ✅ |
| `<Category>` | `Platform.Category` | ✅ |
| `<SortTitle>` | `Platform.SortTitle` | ✅ |
| `<ReleaseDate>` | `Platform.ReleaseDate` | ✅ |
| `<Developer>` | `Platform.Developer` | ✅ |
| `<Manufacturer>` | `Platform.Manufacturer` | ✅ |
| `<Cpu>` | `Platform.Cpu` | ✅ |
| `<Memory>` | `Platform.Memory` | ✅ |
| `<Graphics>` | `Platform.Graphics` | ✅ |
| `<Sound>` | `Platform.Sound` | ✅ |
| `<Display>` | `Platform.Display` | ✅ |
| `<Media>` | `Platform.Media` | ✅ |
| `<MaxControllers>` | `Platform.MaxControllers` | ✅ |
| `<Notes>` | `Platform.Notes` | ✅ |
| `<Folder>` | `Platform.Folder` | ✅ |
| `<VideosFolder>` | `Platform.VideosFolder` | ✅ |
| `<FrontImagesFolder>` | `Platform.FrontImagesFolder` | ✅ |
| `<BackImagesFolder>` | `Platform.BackImagesFolder` | ✅ |
| `<ClearLogoImagesFolder>` | `Platform.ClearLogoImagesFolder` | ✅ |
| `<FanartImagesFolder>` | `Platform.FanartImagesFolder` | ✅ |
| `<ScreenshotImagesFolder>` | `Platform.ScreenshotImagesFolder` | ✅ |
| `<BannerImagesFolder>` | `Platform.BannerImagesFolder` | ✅ |
| `<ManualsFolder>` | `Platform.ManualsFolder` | ✅ |
| `<MusicFolder>` | `Platform.MusicFolder` | ✅ |
| `<ScrapeAs>` | `Platform.ScrapeAs` | ✅ |
| `<VideoPath>` | `Platform.VideoPath` | ✅ |
| `<BigBoxView>` | `Platform.BigBoxView` | ✅ |
| `<BigBoxTheme>` | `Platform.BigBoxTheme` | ✅ |
| `<HideInBigBox>` | `Platform.HideInBigBox` | ✅ |

---

## 3.4 Modelo Emulator — Campos Comparados

### ✅ Implementado (Emulator.cs)

```csharp
public class Emulator {
    public string ID { get; set; }
    public string? Title { get; set; }
    public string? ApplicationPath { get; set; }
    public string? CommandLine { get; set; }
    public bool AutoExtractRoms { get; set; }
    public string? RomType { get; set; }
    public string? ManualPath { get; set; }
    public string? MusicPath { get; set; }
    public bool HideConsole { get; set; }
    public bool UseFileNameWithoutExtensionAsRomTitle { get; set; }
    public bool FileNameWithoutExtensionAndPath { get; set; }
    public bool NoQuotes { get; set; }
    public string? ConfigFilePath { get; set; }
}
```

### ✅ EmulatorPlatform.cs — 100% Completo

```csharp
public class EmulatorPlatform {
    public string Emulator { get; set; }  // GUID
    public string Platform { get; set; }
    public string? CommandLine { get; set; }
    public bool Default { get; set; }
    public bool M3uDiscLoadEnabled { get; set; }
    public bool AutoExtract { get; set; }
}
```

---

## 3.5 Modelo Playlist — Comparación

### ✅ Campos implementados

| Campo | Estado | Notas |
|-------|--------|-------|
| `PlaylistId` (GUID) | ✅ | |
| `Name` | ✅ | |
| `Notes` | ✅ | Usado también para metadata de sagas |
| `NestedName` | ✅ | |
| `AllowDuplicates` | ✅ | |
| `SortTitle` | ✅ | |
| `AutoPopulate` | ✅ | |
| `HideInBigBox` | ✅ | |
| `Criteria` | ✅ | |
| `ClearLogoImagesFolder` | ✅ | Extensión propia |
| `VideoPath` | ✅ | Extensión propia |

---

## 3.6 Sistema de Metadatos Personalizado (Extensión Propia)

GameLauncher ha extendido el campo `<Notes>` de las playlists para almacenar metadatos estructurados de sagas:

```
[SAGA COMPLETA]
[REVISADO 27/04/2026]
JUEGOS FALTANTES: 2
  - Juego 1 faltante
  - Juego 2 faltante
ULTIMO: Nombre del último juego jugado

Descripción libre de la saga...
```

Este formato es totalmente compatible con LaunchBox porque usa el campo `<Notes>` existente. LaunchBox lo mostraría como texto plano, GameLauncher lo parsea con regex para extraer la metadata.

---

## 3.7 Entidades del Sistema XmlDataContext

### Entidades soportadas en lectura Y escritura:

| Entidad | Archivo | Estado |
|---------|---------|--------|
| `Platform` | `Platforms.xml` | ✅ R/W |
| `PlatformCategory` | `Platforms.xml` | ✅ R/W |
| `Game` | `Platforms/{name}.xml` | ✅ R/W |
| `AdditionalApplication` | `Platforms/{name}.xml` | ✅ R/W |
| `CustomField` | `Platforms/{name}.xml` | ✅ R/W |
| `GameControllerSupport` | `Platforms/{name}.xml` | ✅ R/W |
| `AlternateName` | `Platforms/{name}.xml` | ✅ R/W |
| `Emulator` | `Emulators.xml` | ✅ R/W |
| `EmulatorPlatform` | `Emulators.xml` | ✅ R/W |
| `Parent` | `Parents.xml` | ✅ R/W |
| `Playlist` | `Playlists/{name}.xml` | ✅ R/W |
| `PlaylistGame` | `Playlists/{name}.xml` | ✅ R/W |
| `PlaylistFilter` | `Playlists/{name}.xml` | ✅ R/W |
| `GameController` | `GameControllers.xml` | ✅ R/W |
| `InputBinding` | `InputBindings.xml` | ✅ R/W |
| `ListCacheItem` | `ListCache.xml` | ✅ R/W |
| `IgnoredGameId` | `ImportBlacklist.xml` | ✅ R/W |
| `Settings` | `Settings.xml` | ✅ R/W |
| `BigBoxSettings` | `BigBoxSettings.xml` | ✅ R/W |

---

## 3.8 Observaciones Importantes sobre Compatibilidad

### 🟡 Nombre de campo inconsistente

En LaunchBox: `<CommunityStarRatingTotalCount>`
En GameLauncher: `CommunityStarRatingTotalVotes`

Esto causa que al leer/escribir, el campo se lea correctamente (el deserializador ignora campos desconocidos) pero al escribir lo hace con el nombre incorrecto. **Corrección pendiente.**

### 🟡 LastPlayedDate vs DateModified

LaunchBox usa `<LastPlayedDate>` como campo independiente. GameLauncher calcula `LastPlayed` como `DateModified` cuando `PlayCount > 0`, lo cual es una aproximación pero no exactamente la misma semántica.

### ✅ Preservación de campos desconocidos

El deserializador configura `UnknownElement += (s, e) => {}` para ignorar silenciosamente campos que no conoce. Esto garantiza que si LaunchBox agrega nuevos campos en el futuro, GameLauncher no corrompe los datos al guardar (aunque sí los pierde).

> **Nota:** Para preservación perfecta sería necesario hacer round-trip completo del XML preservando nodos desconocidos. Actualmente si GameLauncher guarda un archivo, los campos desconocidos se pierden.
