# 05 — FUNCIONALIDADES FALTANTES: Lo que No Está Implementado

## 5.1 Funcionalidades Críticas (Bloquean Uso Normal)

### ❌ 1. Importador de ROMs
**Qué es:** La capacidad de agregar juegos nuevos a la biblioteca escaneando carpetas o añadiéndolos manualmente.

**En LaunchBox:** "Tools > Import ROMs" con wizard paso a paso:
- Selección de carpeta de ROMs
- Selección de plataforma
- Búsqueda automática de metadata en LaunchBox DB/IGDB
- Descarga de imágenes automática (si EmuMovies/LaunchBox DB configurado)
- Detección de duplicados
- Vista previa antes de importar

**En GameLauncher:** ❌ No existe. Solo puede leer la biblioteca ya creada por LaunchBox.

**Impacto:** Sin esto, GameLauncher solo funciona como visor de una biblioteca LaunchBox existente.

---

### ❌ 2. Descarga/Scraping de Metadata
**Qué es:** Obtener información de juegos desde bases de datos en línea.

**En LaunchBox:** Integración con:
- LaunchBox Games DB (propio)
- EmuMovies (videos, imágenes)
- IGDB (Twitch)
- TheGamesDB
- ScreenScraper

**En GameLauncher:** ❌ No existe ninguna integración con APIs externas.

**Impacto:** No se pueden enriquecer datos de juegos. Las imágenes/videos existentes en disco se usan, pero no se pueden descargar nuevos.

---

### ❌ 3. Scraper de Imágenes y Videos
**Qué es:** Descargar automáticamente carátulas, screenshots, videos de gameplay, marquesinas, etc.

**En LaunchBox:** Menú contextual > "Download Media" o bulk scraper desde "Tools > Download"

**En GameLauncher:** ❌ No existe.

---

## 5.2 Funcionalidades UI Faltantes (BigScreen)

### ❌ 4. Sistema de Temas XAML
**Qué es:** En LaunchBox BigBox, el sistema de temas permite cargar diferentes layouts, paletas de color, fuentes y animaciones desde carpetas XAML externas.

**En LaunchBox:** 
- Carpeta `Themes/` con temas XAML
- Activo: "Default" (HorizontalWheel3, Sci-Fi sound pack)
- Permite cambio de tema en tiempo de ejecución

**En GameLauncher:** ❌ Solo hay un tema hardcodeado en XAML. No existe loader de temas externos.

**Para implementar:** Crear sistema de templates XAML dinámicos (ResourceDictionary + MergedDictionaries).

---

### ❌ 5. Múltiples Tipos de Vista de Juegos
**Qué es:** BigBox permite cambiar entre varios layouts para mostrar la lista de juegos.

**En LaunchBox BigBox:**
- `HorizontalWheel3` ← (activo en tu instalación)
- `HorizontalWheel`, `HorizontalWheel2`
- `VerticalWheel`, `VerticalWheel2`
- `CoverFlow`, `CoverFlowFlow`
- `WallGames` (cuadrícula)
- `TextList` (lista de texto sin imágenes)
- `TextDetails` (lista con panel de detalles)

**En GameLauncher:** Solo hay `GamesWheelView` (rueda horizontal). ❌ No hay alternativas.

---

### ❌ 6. Transiciones Animadas
**Qué es:** LaunchBox BigBox tiene transiciones suaves y configurables entre pantallas.

**En LaunchBox:** Transiciones en XAML del tema (fade, slide, zoom)

**En GameLauncher:** ❌ Sin transiciones. Los cambios de vista son instantáneos.

---

### ❌ 7. Attract Mode (Modo Demostración)
**Qué es:** Cuando BigBox está inactivo, reproduce videos de juegos aleatoriamente como screensaver.

**En LaunchBox:** Configurable en `BigBoxSettings.xml`:
```xml
<AttractModeVideo>true</AttractModeVideo>
<AttractModeVideoDelay>120</AttractModeVideoDelay>
<AttractModeScreensaver>false</AttractModeScreensaver>
```

**En GameLauncher:** ❌ No implementado.

---

### ❌ 8. Sistema de Sonidos de BigBox
**Qué es:** Efectos de sonido al navegar (confirmación, selección, error).

**En LaunchBox:** Sound pack configurable. Activo en tu instalación: "Sci-Fi Set 6"
- `Sounds/[Pack]/NavigationConfirm.wav`
- `Sounds/[Pack]/NavigationLeft.wav`
- etc.

**En GameLauncher:** ❌ Sin efectos de sonido.

---

### ❌ 9. Pantalla de Inicio (Startup Screen)
**Qué es:** Overlay que muestra información del juego mientras carga el emulador.

**En LaunchBox:** Configurable por juego y globalmente. Temas en `StartupThemes/`.

**En GameLauncher:** ❌ No implementado. El emulador se lanza directamente.

---

### ❌ 10. Pantalla de Pausa
**Qué es:** Overlay que aparece cuando presionas Pause en BigBox, con opciones como guardar estado, cambiar controles, volver al menú.

**En LaunchBox:** Temas en `PauseThemes/`. Configurable por juego.

**En GameLauncher:** ❌ No implementado.

---

### ❌ 11. Música de Fondo (Background Music)
**Qué es:** LaunchBox reproduce música de la carpeta `Music/[Plataforma]/` mientras navegas.

**En LaunchBox:** Shuffle, volumen configurable, por plataforma.

**En GameLauncher:** ❌ No implementado. No hay player de música de fondo.

---

### ❌ 12. Vista de Detalles de Plataforma
**Qué es:** En BigBox, al seleccionar una plataforma se puede ver información técnica de la consola antes de entrar a ver los juegos.

**En GameLauncher:** Solo se muestra el logo/imagen. No hay vista de specs técnicos de la plataforma.

---

## 5.3 Funcionalidades de Gestión Faltantes

### ❌ 13. Editor de Plataformas
**Qué es:** Agregar, editar o eliminar plataformas desde la UI.

**En LaunchBox:** `Tools > Manage Platforms`

**En GameLauncher:** ❌ Solo se pueden ver. Hay que editar `Platforms.xml` manualmente.

---

### ❌ 14. Editor de Emuladores
**Qué es:** Configurar qué emulador usar para cada plataforma, ajustar líneas de comando, crear nuevos mapeos.

**En LaunchBox:** `Tools > Manage Emulators`

**En GameLauncher:** ❌ No existe. Solo se leen los emuladores configurados en LaunchBox.

---

### ❌ 15. Editor/Gestor de Playlists
**Qué es:** Crear, editar, eliminar playlists. Añadir/quitar juegos.

**En LaunchBox:** Sidebar derecho, botón "Manage Playlists"

**En GameLauncher:** ❌ Las playlists son de solo lectura. No se pueden crear ni modificar desde la UI.

---

### ❌ 16. Búsqueda Avanzada
**Qué es:** Filtrar juegos por múltiples criterios simultáneamente.

**En LaunchBox:** Sidebar con filtros por: Género, Desarrollador, Publisher, Región, Rating, PlayMode, Fecha, Status, etc.

**En GameLauncher (Desktop):** Solo búsqueda básica por título/desarrollador/género (texto libre). ❌ Sin filtros combinados.

**En GameLauncher (BigScreen):** ❌ Sin búsqueda ni filtros.

---

### ❌ 17. Ordenamiento de Juegos
**Qué es:** Ordenar la lista de juegos por diferentes criterios.

**En LaunchBox:** Por título, fecha, rating, última vez jugado, más jugado, etc.

**En GameLauncher (Desktop):** Hardcodeado por título.
**En GameLauncher (BigScreen):** Hardcodeado por fecha + título.

---

## 5.4 Integraciones de Servicios Faltantes

### ❌ 18. RetroAchievements
**Qué es:** Sistema de logros para juegos retro. Al lanzar un juego compatible, LaunchBox puede mostrar los logros en progreso.

**En LaunchBox:** Configurado con `RetroAchievementsApiKey` en Settings.xml.

**En GameLauncher:** ❌ Sin integración.

---

### ❌ 19. EmuMovies
**Qué es:** Servicio para descargar videos y snapshots de videojuegos.

**En LaunchBox:** Autenticación con cuenta EmuMovies, descarga masiva de media.

**En GameLauncher:** ❌ Sin integración.

---

### ❌ 20. Cloud Sync (Sincronización en la Nube)
**Qué es:** Sincronizar la biblioteca y estadísticas entre múltiples instalaciones de LaunchBox.

**En LaunchBox:** `CloudAuthenticationToken` en Settings.xml, servidor LaunchBox.

**En GameLauncher:** ❌ Sin integración.

---

## 5.5 Funcionalidades de Hardware Faltantes

### ❌ 21. LEDBlinky
**Qué es:** Control de iluminación LED de gabinetes arcade según el juego activo.

**En LaunchBox:** Integración directa.
**En GameLauncher:** ❌ Sin integración.

---

### ❌ 22. Controladores Exclusivos
**Qué es:** Soporte para controladores específicos de arcade, spinners, volantes, etc.

**En LaunchBox:** `GameControllers.xml` con perfiles de controladores.

**En GameLauncher:** Lee el archivo pero no aplica perfiles de controladores especiales.

---

## 5.6 Resumen Priorizado de Funcionalidades Faltantes

| Prioridad | Funcionalidad | Esfuerzo | Impacto |
|-----------|---------------|----------|---------|
| 🔴 Alta | Importador de ROMs | Grande | Crítico para uso independiente |
| 🔴 Alta | Búsqueda avanzada con filtros | Medio | Alta usabilidad |
| 🔴 Alta | Editor de Playlists | Medio | Gestión de biblioteca |
| 🟠 Media | Múltiples vistas BigScreen | Grande | Experiencia de usuario |
| 🟠 Media | Sistema de sonidos | Pequeño | Experiencia de usuario |
| 🟠 Media | Música de fondo | Pequeño | Experiencia de usuario |
| 🟠 Media | Transiciones animadas | Medio | Experiencia de usuario |
| 🟠 Media | Ordenamiento configurable | Pequeño | Usabilidad |
| 🟡 Baja | RetroAchievements | Grande | Nice-to-have |
| 🟡 Baja | EmuMovies | Grande | Nice-to-have |
| 🟡 Baja | Attract Mode | Medio | Nice-to-have |
| 🟡 Baja | Pantalla de pausa | Medio | Nice-to-have |
| 🟡 Baja | Sistema de temas | Grande | Nice-to-have |
| 🟡 Baja | Cloud Sync | Grande | Nice-to-have |
