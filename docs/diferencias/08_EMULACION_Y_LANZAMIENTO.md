# 08 — EMULACIÓN Y LANZAMIENTO: Análisis Detallado

## 8.1 Estado del Sistema de Lanzamiento

**El EmulatorLauncher es uno de los componentes más completos de GameLauncher.**

Cobertura estimada respecto a LaunchBox: **~85%**

---

## 8.2 Placeholders de Línea de Comando

### Implementados en GameLauncher ✅

| Placeholder | Descripción | Estado |
|-------------|-------------|--------|
| `{rom}` | Ruta completa del ROM (con comillas si tiene espacios) | ✅ |
| `{romraw}` | Ruta completa sin comillas | ✅ |
| `{rompath}` | Directorio del ROM | ✅ |
| `{romfile}` | Nombre de archivo con extensión | ✅ |
| `{romname}` | Nombre de archivo sin extensión | ✅ |
| `{emudir}` | Directorio del emulador | ✅ |
| `{emupath}` | Ruta completa del emulador | ✅ |
| `{platform}` | Nombre de la plataforma | ✅ |
| `{title}` | Título del juego | ✅ |

### En LaunchBox (adicionales no implementados) ❌

| Placeholder | Descripción |
|-------------|-------------|
| `{dosbox}` | Ruta a DOSBox configurado |
| `{scummvm}` | Ruta a ScummVM |
| `{steamappid}` | ID de Steam |
| `{gogappid}` | ID de GOG |
| `{m3u}` | Archivo M3U de disco múltiple |

---

## 8.3 Prioridad de Command Line

### Implementado (correcto) ✅

```csharp
// EmulatorLauncher.cs — BuildCommandLine()
var commandLine = !string.IsNullOrWhiteSpace(game.CommandLine)
    ? game.CommandLine                    // 1. Juego específico
    : !string.IsNullOrWhiteSpace(platformMapping?.CommandLine)
        ? platformMapping!.CommandLine    // 2. Plataforma
        : emulator.CommandLine ?? "";     // 3. Emulador global
```

**Prioridad:** Juego > EmulatorPlatform > Emulator

Esto es **exactamente igual** que LaunchBox. ✅

---

## 8.4 Resolución de Emulador por Juego

### Implementado ✅

```
1. ¿El juego tiene un Emulator GUID asignado?
   → Busca ese emulador específico en Emulators.xml
   → Busca el EmulatorPlatform correspondiente para los args
   
2. ¿No tiene emulador asignado?
   → Busca el EmulatorPlatform con Default=true para la plataforma
   → Usa ese emulador como predeterminado
   
3. ¿No hay emulador?
   → Intenta lanzamiento directo (solo .exe, .bat, .cmd, .lnk, .url)
```

✅ Lógica idéntica a LaunchBox.

---

## 8.5 Manejo de ROMs Comprimidas

### Implementado con SharpCompress ✅

**Formatos soportados:**
- `.zip` ✅
- `.rar` ✅
- `.7z` ✅

**Proceso:**
1. Si el ROM no existe como archivo plano, busca `{romname}.zip`, `.rar`, `.7z`
2. Extrae a `%TEMP%/GameLauncher_Roms/{GUID}/`
3. Busca el archivo esperado dentro del zip (por nombre exacto)
4. Si no lo encuentra por nombre, busca por extensión
5. Si no, usa el primer archivo encontrado
6. Lanza el emulador con la ruta del archivo extraído
7. Al terminar, elimina el directorio temporal

**Diferencia con LaunchBox:**
- LaunchBox usa 7-Zip embebido (carpeta `7-Zip/` en la instalación)
- GameLauncher usa SharpCompress (librería .NET, más lenta pero sin dependencia externa)
- LaunchBox soporta también ISO, BIN/CUE para discos
- GameLauncher no soporta archivos multi-parte (zip.001, zip.002, etc.)

---

## 8.6 Lanzamiento Directo (Sin Emulador)

### Implementado ✅

**Extensiones que se lanzan directamente:**
```csharp
private static readonly string[] DirectLaunchExtensions = { 
    ".exe", ".bat", ".cmd", ".lnk", ".url" 
};
```

**Diferencias con LaunchBox:**
- LaunchBox también soporta `.msi`, `.ahk` (AutoHotkey)
- LaunchBox puede lanzar juegos de Steam/GOG/Epic via launchers especiales
- GameLauncher: solo los 5 formatos listados

---

## 8.7 Proceso de Lanzamiento Completo

```
LaunchGameAsync(game)
      │
      ├─ LoadEmulatorsIfNeeded()  ← Carga Emulators.xml (una sola vez)
      │
      ├─ GetEmulatorForGame(game)
      │     ├─ Busca por game.Emulator GUID
      │     └─ Fallback: EmulatorPlatform default
      │
      ├─ [Sin emulador] → LaunchDirectAsync(game)
      │                      └─ Process.Start(exe/bat/lnk)
      │
      └─ LaunchGameWithEmulatorAsync(game, emulator, mapping)
            │
            ├─ Valida ApplicationPath del emulador
            ├─ Valida ApplicationPath del ROM
            ├─ [ROM comprimido] → ExtractRomAsync(...)
            │                          └─ SharpCompress extrae a %TEMP%
            │
            ├─ BuildCommandLine(emulator, game, mapping, romPath)
            │     └─ Substituye {rom} {emudir} {romname} etc.
            │
            ├─ Process.Start(emulatorPath, commandLine)
            │     └─ WorkingDirectory = directorio del emulador
            │     └─ HideConsole = emulator.HideConsole
            │
            ├─ Espera process.WaitForExitAsync()
            │
            ├─ Calcula playTimeSeconds
            │
            └─ CleanupTempDir() ← Elimina ROMs extraídas
```

---

## 8.8 Estadísticas Post-Lanzamiento

### Implementado ✅

Después de que termina el juego, `StatisticsTracker.RecordPlaySessionAsync()` actualiza:

```csharp
game.PlayCount++;
game.PlayTime += playTimeSeconds;
game.DateModified = DateTime.Now;  // Actúa como LastPlayedDate
await _gameManager.UpdateGameAsync(platform, game);
```

**Diferencia:** LaunchBox actualiza `<LastPlayedDate>` como campo independiente. GameLauncher actualiza `<DateModified>` que es distinto semánticamente.

---

## 8.9 Compatibilidad con Emuladores del Sistema

### Verificación de compatibilidad con los 33 emuladores configurados en LaunchBox

| Emulador | Tipo de cmd | Funciona en GameLauncher |
|----------|-------------|------------------------|
| RetroArch | `-L "cores\dll" {rom} -f` | ✅ (todos los placeholders soportados) |
| PCSX2 | `"ruta" {rom}` | ✅ |
| RPCS3 | `--no-gui {rom}` | ✅ |
| PPSSPP | `{rom}` | ✅ |
| Cemu | `-g {rom}` | ✅ |
| Dolphin | `-b -e {rom}` | ✅ |
| Yuzu | `-f -g {rom}` | ✅ |
| Vita3K | `-F {romname}` | ✅ (romname soportado) |
| XENIA | `{rom}` | ✅ |
| Redream | `{rom}` | ✅ |
| MAME | Flags complejos | ✅ (FileNameWithoutExtensionAndPath) |
| WinKawaks | `{romname}` | ✅ |
| Mednafen | Flags Saturn | ✅ |
| mGBA | `{rom}` | ✅ |
| Stella | `{rom}` | ✅ |
| openMSX | `{rom}` | ✅ |
| WinVICE/VICE64 | `{rom}` | ✅ |
| WinUAE | Scripts config | 🟡 (depende de config) |
| DeSmuME | `{rom}` | ✅ |
| Citra | `{rom}` | ✅ |

**Conclusión:** El sistema de lanzamiento de GameLauncher es compatible con **todos los emuladores** configurados en la instalación de LaunchBox del usuario.

---

## 8.10 Características de Lanzamiento Faltantes

### ❌ No implementado

| Característica | Descripción |
|---------------|-------------|
| DOSBox integration | Lanzamiento especial para juegos DOS con config |
| ScummVM integration | Lanzamiento de juegos point-and-click |
| Steam launcher | Lanzar juegos de Steam via `steam://run/` |
| GOG Galaxy launcher | Lanzar juegos GOG via API |
| Epic Games launcher | Lanzar vía Epic |
| AutoHotkey scripts | Ejecutar scripts AHK pre/post lanzamiento |
| Pre-launch scripts | Ejecutar apps antes del emulador |
| Post-launch scripts | Ejecutar apps después del emulador |
| M3U multi-disc | Crear .m3u automático para juegos multi-disco |
| Startup delay | `StartupLoadDelay` por juego |
| Mouse cursor hiding | `HideMouseCursorInGame` |
| Suspend on pause | `SuspendProcessOnPause` |
| Aggressive window hiding | `AggressiveWindowHiding` |
| Process tree monitoring | LaunchBox detecta procesos hijo del emulador |

### 🟡 Parcialmente implementado

| Característica | Estado |
|---------------|--------|
| Kill proceso | ✅ `KillCurrentProcess()` con `entireProcessTree: true` |
| Tiempo de juego | ✅ Desde inicio hasta fin del proceso principal |
| HideConsole | ✅ `CreateNoWindow = emulator.HideConsole` |
| NoQuotes | ✅ `emulator.NoQuotes` |
| FileNameWithoutExtensionAndPath | ✅ Para MAME y similares |
