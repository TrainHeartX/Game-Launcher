# 01 — RESUMEN EJECUTIVO: Estado del Proyecto GameLauncher

## 1.1 Descripción del Proyecto

**GameLauncher** es una implementación alternativa open-source de los conceptos clave de LaunchBox Premium 13.6. Está desarrollado como una aplicación WPF .NET con arquitectura en capas.

- **Repositorio:** `H:\GameLauncher\`
- **Solución:** `GameLauncher.sln` (6 proyectos)
- **Lenguaje:** C# 12, .NET 8, WPF

---

## 1.2 Estado Global de Implementación

```
╔══════════════════════════════════════════════╗
║  COMPLETITUD GLOBAL: ~45%                    ║
╚══════════════════════════════════════════════╝
```

### Por Componente

| Componente | Estado | % |
|-----------|--------|---|
| **Capa de Datos (XML)** | ✅ Completo | 95% |
| **Modelos de Dominio** | ✅ Completo | 95% |
| **Lanzamiento de Juegos** | ✅ Sólido | 85% |
| **Árbol de Navegación** | ✅ Funcional | 80% |
| **UI Desktop** | 🟡 Parcial | 50% |
| **UI BigScreen** | 🟡 Parcial | 40% |
| **Sistema de Imágenes** | 🟡 Parcial | 55% |
| **Sistema de Video** | 🟡 Parcial | 60% |
| **Playlists** | 🟡 Parcial | 65% |
| **Editor de Metadatos** | 🟡 Parcial | 55% |
| **Sistema de Temas** | ❌ No implementado | 0% |
| **Importador de ROMs** | ❌ No implementado | 0% |
| **Descarga de Metadata** | ❌ No implementado | 0% |
| **RetroAchievements** | ❌ No implementado | 0% |
| **Cloud Sync** | ❌ No implementado | 0% |
| **EmuMovies** | ❌ No implementado | 0% |
| **Scraper de imágenes** | ❌ No implementado | 0% |
| **Plugins** | ❌ No implementado | 0% |
| **LEDBlinky / OBS** | ❌ No implementado | 0% |
| **TeknoParrot** | ❌ No implementado | 0% |

---

## 1.3 Proyectos en la Solución

```
GameLauncher.sln
├── src/Core/
│   ├── GameLauncher.Core              → Modelos de dominio
│   ├── GameLauncher.Data              → Acceso a datos XML
│   └── GameLauncher.Infrastructure    → Servicios de negocio
├── src/UI/
│   ├── GameLauncher.BigScreen         → Modo pantalla completa
│   ├── GameLauncher.Desktop           → Modo escritorio
│   └── GameLauncher.UI.Shared        → Componentes compartidos
├── src/Plugins/                       → (vacío)
└── tests/                             → Tests
```

---

## 1.4 Fortalezas del Proyecto

### ✅ Lo que funciona MUY BIEN:

1. **Compatibilidad de Datos XML** — Lee y escribe los mismos XMLs que LaunchBox sin perder datos. Estructura 100% compatible.

2. **Modelo de Datos Completo** — `Game.cs` tiene todos los ~70 campos que usa LaunchBox. `Platform.cs`, `Emulator.cs`, `Playlist.cs` también completos.

3. **Lanzador de Emuladores** — `EmulatorLauncher.cs` implementa:
   - Resolución de emulador por plataforma/juego
   - Construcción de línea de comando con todos los placeholders (`{rom}`, `{emudir}`, `{romraw}`, etc.)
   - Extracción automática de ROMs comprimidas (zip, rar, 7z via SharpCompress)
   - Lanzamiento directo de ejecutables Windows (.exe, .lnk, .url, .bat)
   - Medición de tiempo de juego
   - Kill del proceso con Select+Start

4. **Árbol de Navegación** — `PlatformManager.GetNavigationTreeAsync()` construye exactamente la misma jerarquía de categorías > subcategorías > plataformas > playlists que LaunchBox usando `Parents.xml`

5. **Sistema de Caché** — Caché en memoria + caché en disco (JSON) para imágenes, muy eficiente

---

## 1.5 Debilidades Críticas

### ❌ Lo que FALTA y es crítico:

1. **Sin Importador de ROMs** — No hay forma de añadir nuevos juegos. Solo se puede leer la biblioteca existente de LaunchBox.

2. **Sin Scraper/Downloader de Metadata** — No se puede descargar carátulas, screenshots, videos, ni metadata desde EmuMovies, LaunchBox DB, IGDB, etc.

3. **Sin Sistema de Temas** — Los temas XAML de BigBox no están implementados. Solo hay un tema hardcodeado.

4. **UI BigScreen Incompleta** — Faltan vistas de lista de texto, Wall, CoverFlow, múltiples tipos de wheel. Solo hay HorizontalWheel.

5. **Sin RetroAchievements** — No hay integración de logros/achievements.

6. **Sin Editor de Emuladores** — No hay UI para agregar/configurar emuladores. Se hace todo a mano en el XML.

7. **Sin Configuración de Plataformas** — No se pueden crear, editar o eliminar plataformas desde la UI.

---

## 1.6 Diferencias Conceptuales Clave

| Aspecto | LaunchBox | GameLauncher |
|---------|-----------|-------------|
| Licencia | Commercial (Premium) | Código propio |
| Runtime | .NET 6.0 | .NET 8.0 |
| MVVM | Caliburn.Micro | CommunityToolkit.Mvvm |
| Datos | XML propietario | Lee mismo XML de LaunchBox |
| Multimedia | VLC embebido | MediaElement nativo WPF |
| Scraping | EmuMovies + IGDB + propio | ❌ No implementado |
| Cloud | Servidor propio LaunchBox | ❌ No implementado |
| Plugins | API .NET pública | ❌ No implementado |
| Temas | XAML dinámicos | Solo hardcoded |
| 3D Models | Integrado | ❌ No implementado |

---

## 1.7 Compatibilidad con los Datos de LaunchBox Instalado

**GameLauncher puede leer directamente los datos de la instalación de LaunchBox en `H:\LaunchBox\LaunchBox\Data\`**

| Archivo de datos | Compatible |
|-----------------|-----------|
| `Settings.xml` | ✅ Lee y escribe |
| `BigBoxSettings.xml` | ✅ Lee y escribe |
| `Emulators.xml` | ✅ Lee y escribe |
| `Platforms.xml` | ✅ Lee y escribe |
| `Data/Platforms/*.xml` (juegos) | ✅ Lee y escribe |
| `Parents.xml` | ✅ Lee |
| `Playlists/*.xml` | ✅ Lee y escribe |
| `GameControllers.xml` | ✅ Lee |
| `InputBindings.xml` | ✅ Lee |
| `ListCache.xml` | ✅ Lee y escribe |
| `ImportBlacklist.xml` | ✅ Lee y escribe |

> **Conclusión:** GameLauncher puede usarse directamente sobre los datos de la instalación de LaunchBox sin migración ni conversión. Los datos son 100% compartibles.
