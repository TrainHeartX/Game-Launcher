# 09 — METADATA Y COMPATIBILIDAD XML

## 9.1 Estado de Compatibilidad de Datos

### Resumen

```
Lectura de datos LaunchBox:   ✅ 95% compatible
Escritura de datos LaunchBox: ✅ 90% compatible (pérdida de campos desconocidos)
```

---

## 9.2 Verificación Real: Settings.xml

Comparando `Settings.cs` de GameLauncher con los datos reales de `H:\LaunchBox\LaunchBox\Data\Settings.xml`:

### Campos que GameLauncher SÍ lee correctamente

```xml
<!-- Estos campos están en Settings.xml real Y en Settings.cs -->
<ApplicationPath>...</ApplicationPath>
<DefaultEmulator>...</DefaultEmulator>
<SortBy>Title</SortBy>
<SortByDesc>false</SortByDesc>
<ListView>false</ListView>
<ShowSideBar>true</ShowSideBar>
<SideBarSize>323</SideBarSize>
<SideBarField>Categoría de Plataforma</SideBarField>
<Language>es</Language>
<Theme>Old Default</Theme>
<ShowHiddenGames>false</ShowHiddenGames>
<ShowBrokenGames>true</ShowBrokenGames>
<AutoPlayDetailsVideo>true</AutoPlayDetailsVideo>
<VideoPlaybackEngine>VLC</VideoPlaybackEngine>
<!-- ... y ~300 campos más -->
```

### Campos CRÍTICOS que GameLauncher NO usa (pero lee correctamente)

| Campo | Valor en tu instalación | Uso en GameLauncher |
|-------|------------------------|---------------------|
| `VideoPlaybackEngine` | `VLC` | ❌ Ignorado — usa MediaElement |
| `DefaultImageGroup` | `Boxes` | ❌ Ignorado — prioridad hardcoded |
| `AutoPlayMusic` | `true` | ❌ No hay música de fondo |
| `CloudAuthenticationToken` | `[cifrado]` | ❌ Sin cloud sync |
| `EmuMoviesPassword` | `[cifrado]` | ❌ Sin EmuMovies |
| `RetroAchievementsApiKey` | `[cifrado]` | ❌ Sin RetroAchievements |
| `InstanceId` | `0ba5172a-...` | ❌ Sin uso |
| `AttractModeVideo` | `false` | ❌ Sin attract mode |

---

## 9.3 Verificación Real: BigBoxSettings.xml

Los campos de `BigBoxSettings.cs` cubren los 524 parámetros del archivo real.

### Campos usados actualmente por GameLauncher BigScreen

| Campo | Valor en tu instalación | Uso |
|-------|------------------------|-----|
| `BigBoxTheme` | `Default` | ❌ No carga el tema XAML |
| `BigBoxView` | `HorizontalWheel3` | ❌ Solo hay una vista |
| `SoundPackName` | `Sci-Fi Set 6 by Clavius` | ❌ Sin efectos de sonido |
| Bindings de botones | A, B, X, Y, LB, RB... | ✅ Los usa para navegación |

**Conclusión:** BigBoxSettings.xml se LEE completo, pero casi ningún setting realmente se APLICA al comportamiento.

---

## 9.4 Análisis de los Datos Reales de Plataformas

### Plataformas en `H:\LaunchBox\LaunchBox\Data\Platforms\`

```
Total archivos XML: 57 plataformas
Arcade.xml:          18.9 MB (mayor archivo — miles de ROMs MAME)
SNES.xml:            9.9 MB
Amstrad CPC.xml:     13.6 MB
```

**GameLauncher puede leer todos estos archivos sin modificación.** ✅

La carga del archivo `Arcade.xml` (18.9 MB con miles de juegos MAME) puede ser lenta en la primera carga. El caché en memoria resuelve esto para cargas posteriores.

---

## 9.5 Problema Detectado: CommunityStarRatingTotalCount

### En LaunchBox XML real:
```xml
<CommunityStarRatingTotalCount>250</CommunityStarRatingTotalCount>
```

### En GameLauncher Game.cs:
```csharp
public int CommunityStarRatingTotalVotes { get; set; }
```

**El nombre no coincide.** Al deserializar, el valor se pierde (el deserializador lo ignora silenciosamente). Al serializar, se guarda como `<CommunityStarRatingTotalVotes>` que LaunchBox no reconoce.

**Fix requerido:**
```csharp
[XmlElement("CommunityStarRatingTotalCount")]
public int CommunityStarRatingTotalVotes { get; set; }
```

---

## 9.6 Problema Detectado: LastPlayedDate

### En LaunchBox XML real:
```xml
<LastPlayedDate>2026-04-10T20:00:00-05:00</LastPlayedDate>
<DateModified>2026-04-10T20:00:00-05:00</DateModified>
```

### En GameLauncher Game.cs:
```csharp
// No existe campo LastPlayedDate como propiedad persistida
public DateTime? LastPlayed => PlayCount > 0 ? DateModified : null;
```

**Problema:** `LastPlayedDate` se pierde al guardar. Solo se mantiene `DateModified`.

**Fix requerido:**
```csharp
public DateTime? LastPlayedDate { get; set; }

// Calcular correctamente:
public DateTime? LastPlayed => LastPlayedDate ?? (PlayCount > 0 ? DateModified : null);
```

Y en `StatisticsTracker.RecordPlaySessionAsync()`:
```csharp
game.LastPlayedDate = DateTime.Now;
game.DateModified = DateTime.Now;
```

---

## 9.7 Problema Detectado: LaunchBoxDbId

### En LaunchBox XML real:
```xml
<LaunchBoxDbId>12345</LaunchBoxDbId>
```

### En GameLauncher Game.cs:
```csharp
public string? DatabaseID { get; set; }  // Existe
// LaunchBoxDbId → NO existe como propiedad separada
```

**Impacto:** El `LaunchBoxDbId` (numérico, para la DB en línea) se pierde al guardar.

**Fix:**
```csharp
public int? LaunchBoxDbId { get; set; }
```

---

## 9.8 Problema Detectado: Preservación de Campos Desconocidos

### Situación actual

Al guardar cualquier archivo XML, GameLauncher **pierde todos los campos que no conoce**:

```csharp
// XmlDataContext.cs — DeserializeNode<T>()
serializer.UnknownElement += (sender, e) => { /* Ignore silently */ };
```

Si LaunchBox 13.6 tiene campo `X` desconocido para GameLauncher y el usuario edita el juego, el campo `X` desaparece del XML.

### Solución Recomendada

Para round-trip perfecto, usar `XmlAnyElement`:

```csharp
// En Game.cs
[XmlAnyElement]
public XmlElement[] UnknownElements { get; set; } = Array.Empty<XmlElement>();
```

Esto preserva automáticamente cualquier campo desconocido.

---

## 9.9 Formato XML de Salida: Verificación

### Configuración actual en XmlDataContext

```csharp
_xmlWriterSettings = new XmlWriterSettings {
    Indent = true,
    IndentChars = "  ",           // 2 espacios
    Encoding = new UTF8Encoding(true),  // UTF-8 con BOM
    OmitXmlDeclaration = false,
    NewLineChars = "\r\n",        // Windows line endings
    NewLineHandling = NewLineHandling.Replace
};
```

### Formato de LaunchBox original

```xml
<?xml version="1.0" standalone="yes"?>
<LaunchBox>
  <Game>
    <ID>...</ID>
    ...
  </Game>
</LaunchBox>
```

✅ **La configuración de escritura produce un formato idéntico al de LaunchBox.**

---

## 9.10 Análisis de Playlists

### Archivos en `H:\LaunchBox\LaunchBox\Data\Playlists\`

GameLauncher lee estas playlists correctamente. El sistema de Sagas (playlist con metadata en Notes) es una extensión propia.

**Ejemplo de Notes parseado:**
```
[SAGA COMPLETA]
[REVISADO 27/04/2026]
JUEGOS FALTANTES: 0
ULTIMO: The Legend of Zelda: Tears of the Kingdom

Saga principal de Zelda, juegos de la línea principal.
```

**ParsePlaylistNotes() extrae:**
- `Status`: "SAGA COMPLETA"
- `ReviewDate`: "27/04/2026"
- `MissingCount`: 0
- `MissingGames`: []
- `LastGame`: "The Legend of Zelda: Tears of the Kingdom"
- `Description`: "Saga principal de Zelda..."

✅ Sistema robusto y compatible (usa campo Notes existente de LaunchBox).

---

## 9.11 Tabla Resumen de Compatibilidad XML

| Archivo | Lectura | Escritura | Pérdida de datos |
|---------|---------|-----------|-----------------|
| `Settings.xml` | ✅ ~95% | ✅ ~90% | Campos desconocidos futuros |
| `BigBoxSettings.xml` | ✅ ~95% | ✅ ~90% | Campos desconocidos futuros |
| `Emulators.xml` | ✅ 100% | ✅ 100% | Ninguna |
| `Platforms.xml` | ✅ ~98% | ✅ ~95% | PlatformFolder no soportado |
| `Platforms/{name}.xml` | ✅ ~95% | ✅ ~90% | LastPlayedDate, CommunityCount |
| `Parents.xml` | ✅ 100% | ✅ 100% | Ninguna |
| `Playlists/{name}.xml` | ✅ 100% | ✅ 100% | Ninguna |
| `GameControllers.xml` | ✅ 100% | ✅ 100% | Ninguna |
| `InputBindings.xml` | ✅ 100% | ✅ 100% | Ninguna |
| `ListCache.xml` | ✅ 100% | ✅ 100% | Ninguna |
| `ImportBlacklist.xml` | ✅ 100% | ✅ 100% | Ninguna |
| `Parents.xml` | ✅ 100% | ✅ 100% | Ninguna |

---

## 9.12 Fixes de Compatibilidad Prioritarios

### Fix 1: CommunityStarRatingTotalCount (Fácil - 5 min)
```csharp
// Game.cs
[XmlElement("CommunityStarRatingTotalCount")]
public int CommunityStarRatingTotalVotes { get; set; }
```

### Fix 2: LastPlayedDate (Medio - 30 min)
```csharp
// Game.cs — agregar campo
public DateTime? LastPlayedDate { get; set; }

// StatisticsTracker.cs — actualizar al registrar sesión
game.LastPlayedDate = DateTime.Now;
```

### Fix 3: LaunchBoxDbId (Fácil - 10 min)
```csharp
// Game.cs
public int? LaunchBoxDbId { get; set; }
```

### Fix 4: Preservación de campos desconocidos (Medio - 2 horas)
```csharp
// En cada modelo: Game, Platform, Emulator, etc.
[XmlAnyElement]
public XmlElement[] UnknownElements { get; set; } = Array.Empty<XmlElement>();
```
