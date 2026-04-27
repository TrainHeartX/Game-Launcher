# 11 — BUGS CONOCIDOS Y PROBLEMAS TÉCNICOS

## 11.1 Bugs Confirmados

---

### 🔴 BUG-001: CommunityStarRatingTotalCount se pierde al guardar

**Severidad:** Alta (pérdida de datos)
**Archivo:** `src/Core/GameLauncher.Core/Models/Game.cs`
**Descripción:** El campo XML `<CommunityStarRatingTotalCount>` de LaunchBox no coincide con la propiedad C# `CommunityStarRatingTotalVotes`. Al deserializar se pierde el valor; al serializar escribe el nombre incorrecto.

**Fix:**
```csharp
[XmlElement("CommunityStarRatingTotalCount")]
public int CommunityStarRatingTotalVotes { get; set; }
```

---

### 🔴 BUG-002: LastPlayedDate no se persiste

**Severidad:** Alta (pérdida de datos)
**Archivo:** `src/Core/GameLauncher.Core/Models/Game.cs`
**Descripción:** LaunchBox guarda `<LastPlayedDate>` como campo propio. GameLauncher lo calcula como propiedad computada. Al guardar, `LastPlayedDate` se pierde del XML.

**Fix:**
```csharp
// Agregar en Game.cs:
public DateTime? LastPlayedDate { get; set; }
```

---

### 🔴 BUG-003: LaunchBoxDbId no existe en el modelo

**Severidad:** Media (pérdida de datos)
**Archivo:** `src/Core/GameLauncher.Core/Models/Game.cs`
**Descripción:** `<LaunchBoxDbId>` (entero) presente en los XMLs de LaunchBox no tiene propiedad en el modelo. Se ignora en lectura y se pierde en escritura.

**Fix:**
```csharp
public int? LaunchBoxDbId { get; set; }
```

---

### 🟠 BUG-004: Campos desconocidos del XML se pierden al guardar

**Severidad:** Media (riesgo de corrupción parcial)
**Archivo:** `src/Core/GameLauncher.Data/Xml/XmlDataContext.cs`
**Descripción:** El deserializador está configurado para ignorar elementos desconocidos (`UnknownElement += (s,e) => {}`). Cualquier campo de LaunchBox no mapeado en los modelos se silenciosamente descarta cuando GameLauncher guarda el archivo.

**Fix parcial (XmlAnyElement):**
```csharp
// Añadir a Game.cs, Platform.cs, Emulator.cs, etc.:
[XmlAnyElement]
public XmlElement[] UnknownElements { get; set; } = Array.Empty<XmlElement>();
```

**Fix completo:** Implementar un parser que haga pass-through del XML sin modificar nodos desconocidos.

---

### 🟠 BUG-005: VideoPath relativo vs absoluto

**Severidad:** Media
**Archivo:** `src/Core/GameLauncher.Core/Helpers/VideoPathResolver.cs`
**Descripción:** Al resolver la ruta de video de un juego, el sistema asume que la ruta es relativa a `LaunchBoxPath`. Pero si `Game.VideoPath` es una ruta absoluta, el `Path.Combine` producirá una ruta incorrecta.

**Fix:**
```csharp
string videoPath = game.VideoPath;
if (!Path.IsPathRooted(videoPath))
    videoPath = Path.Combine(launchBoxPath, videoPath);
```

---

### 🟠 BUG-006: SanitizeFileName duplicado en dos clases

**Severidad:** Baja (mantenimiento)
**Archivos:**
- `GamesWheelViewModel.cs` línea ~141
- `PlatformFiltersViewModel.cs` línea ~648

**Descripción:** El método `SanitizeFileName` está copiado en dos ViewModels. Si hay un bug en uno, hay que corregirlo en ambos.

**Fix:** Moverlo a un helper estático en `GameLauncher.Core.Helpers`:
```csharp
// FileNameHelper.cs
public static class FileNameHelper {
    public static string SanitizeForLaunchBox(string fileName) { ... }
}
```

---

### 🟠 BUG-007: ResolveCoverImagePath duplicado en dos clases

**Severidad:** Baja (mantenimiento)
**Archivos:**
- `GamesWheelViewModel.cs` línea ~62
- `PlatformFiltersViewModel.cs` línea ~589

**Descripción:** Mismo patrón de búsqueda de imágenes duplicado. Debería moverse a `ImagePathResolver.cs` en el Core.

---

### 🟡 BUG-008: Caché de imágenes no se invalida

**Severidad:** Baja (UX)
**Archivo:** `src/UI/GameLauncher.Desktop/ViewModels/MainViewModel.cs`
**Descripción:** El caché JSON de rutas de imágenes (`cache/images/{platform}.json`) nunca se invalida. Si el usuario agrega/mueve imágenes después de la primera carga, GameLauncher seguirá usando las rutas viejas (o fallará silenciosamente si el archivo ya no existe).

**Fix:** Agregar timestamp al caché y expirar después de N días, o detectar si las rutas en el caché siguen siendo válidas.

---

### 🟡 BUG-009: MediaElement WPF no soporta todos los formatos de video

**Severidad:** Media (funcionalidad)
**Descripción:** Los videos `.mp4` con codecs modernos (H.265/HEVC), `.mkv`, `.avi` con codecs no-Windows pueden fallar silenciosamente en `MediaElement` de WPF.

**Solución:** Integrar **LibVLCSharp** (binding .NET de VLC):
```
NuGet: VideoLAN.LibVLC.Windows + LibVLCSharp.WPF
```

---

### 🟡 BUG-010: Carga paralela de juegos puede causar condiciones de carrera

**Severidad:** Baja (estabilidad)
**Archivo:** `src/UI/GameLauncher.BigScreen/ViewModels/PlatformFiltersViewModel.cs`
**Descripción:** `LoadGameCountsAsync()` usa `Parallel.ForEach` con `lock(result)`. Si la colección de plataformas es modificada durante la carga paralela, puede ocurrir una excepción.

**Mitigación actual:** El lock parcial protege el Dictionary de resultado pero no la colección de entrada.

---

## 11.2 Limitaciones Conocidas (No son Bugs)

### L-001: Sin soporte de disco multi-parte
GameLauncher no puede crear archivos `.m3u` para juegos de múltiples discos. LaunchBox hace esto automáticamente.

### L-002: Sin virtualización en BigScreen
La `GamesWheelView` carga todas las portadas en memoria. Con 18,900 juegos de Arcade (en batches de 30) esto puede consumir varios GB de RAM.

**Mitigación:** La carga por batches mitiga el problema, pero no lo elimina completamente. Para Arcade, usar una vista de texto (TextList) sería más apropiado.

### L-003: Un solo proceso por sesión
GameLauncher solo puede lanzar un juego a la vez y espera a que termine antes de devolver el control. No soporta lanzar múltiples instancias simultáneas.

### L-004: Sin soporte de TeknoParrot
TeknoParrot (emulador de arcades modernos como Sega, Namco) requiere integración específica que no existe.

### L-005: Sin soporte de retroarch con netplay
Aunque RetroArch se lanza correctamente, no hay integración con el sistema de Netplay de RetroArch.

---

## 11.3 Orden de Corrección Recomendado

```
Prioridad 1 (Esta semana — evitan pérdida de datos):
  ✅ BUG-001: CommunityStarRatingTotalCount
  ✅ BUG-002: LastPlayedDate
  ✅ BUG-003: LaunchBoxDbId

Prioridad 2 (Próximas 2 semanas — mejoran estabilidad):
  🔧 BUG-004: XmlAnyElement para preservar campos desconocidos
  🔧 BUG-005: VideoPath absoluto vs relativo
  🔧 BUG-009: LibVLCSharp para video

Prioridad 3 (Refactoring limpio):
  🔧 BUG-006: SanitizeFileName → helper estático
  🔧 BUG-007: ResolveCoverImagePath → ImagePathResolver
  🔧 BUG-008: Invalidación de caché de imágenes
```
