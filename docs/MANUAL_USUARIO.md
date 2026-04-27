# Manual de Usuario - GameLauncher

**Versión**: 1.5.0-beta
**Fecha**: 2026-04-27

---

## Tabla de Contenidos

1. [Introducción](#introducción)
2. [Instalación](#instalación)
3. [Configuración Inicial](#configuración-inicial)
4. [Desktop Mode](#desktop-mode)
5. [BigScreen Mode](#bigscreen-mode)
6. [Estadísticas](#estadísticas)
7. [Troubleshooting](#troubleshooting)
8. [Apéndices](#apéndices)

---

## Introducción

### ¿Qué es GameLauncher?

GameLauncher es una aplicación moderna para Windows que te permite gestionar y lanzar tu colección de juegos y emuladores. Es 100% compatible con LaunchBox, lo que significa que ambas aplicaciones pueden coexistir y compartir los mismos datos.

### Características Principales

- **Desktop Mode**: Interfaz de escritorio para navegar tu biblioteca
- **BigScreen Mode**: Interfaz fullscreen para TV controlada por gamepad
- **Estadísticas Avanzadas**: Tracking detallado de tiempo de juego
- **100% Compatible con LaunchBox**: Usa los mismos archivos XML

### Requisitos del Sistema

**Mínimos**:
- Windows 10 (64-bit)
- 4 GB RAM
- 50 MB espacio en disco
- .NET 8.0 Runtime

**Recomendados**:
- Windows 11 (64-bit)
- 8 GB RAM
- SSD para mejor performance
- Gamepad Xbox (para BigScreen Mode)

---

## Instalación

### Opción 1: Instalación desde Release (Recomendado)

1. **Descargar**
   - Ve a la página de [Releases](https://github.com/tuusuario/GameLauncher/releases)
   - Descarga la última versión `GameLauncher-v1.0.0-beta.zip`

2. **Extraer**
   - Extrae el archivo ZIP a una carpeta de tu elección
   - Ejemplo: `C:\Program Files\GameLauncher\`

3. **Verificar .NET 8.0 Runtime**
   - Abre PowerShell y ejecuta: `dotnet --version`
   - Si no está instalado, descarga desde: https://dotnet.microsoft.com/download/dotnet/8.0

4. **Ejecutar**
   - Doble click en `GameLauncher.Desktop.exe` (Desktop Mode)
   - O doble click en `GameLauncher.BigScreen.exe` (BigScreen Mode)

### Opción 2: Compilar desde Código Fuente

Para desarrolladores o usuarios avanzados:

```bash
# 1. Clonar repositorio
git clone https://github.com/tuusuario/GameLauncher.git
cd GameLauncher

# 2. Restaurar dependencias
dotnet restore

# 3. Compilar
dotnet build --configuration Release

# 4. Ejecutar
dotnet run --project src/UI/GameLauncher.Desktop/GameLauncher.Desktop.csproj
```

---

## Configuración Inicial

### Primera Ejecución

La primera vez que ejecutes GameLauncher, verás el asistente de configuración inicial.

#### Paso 1: Seleccionar Carpeta de LaunchBox

1. Haz click en "Examinar"
2. Navega a la carpeta raíz de tu instalación de LaunchBox
   - Ejemplo: `H:\LaunchBox\LaunchBox\`
   - La carpeta debe contener una subcarpeta `Data\` con archivos XML

3. Verifica que la ruta sea correcta
4. Haz click en "Continuar"

#### Paso 2: Verificación de Datos

GameLauncher escaneará tu biblioteca de LaunchBox:

- **Plataformas detectadas**: Muestra el número de plataformas encontradas
- **Juegos totales**: Suma de juegos en todas las plataformas
- **Emuladores configurados**: Emuladores disponibles

Si los números parecen correctos, haz click en "Finalizar".

#### Paso 3: Carga Inicial

La primera carga puede tardar algunos minutos si tienes una colección grande:

- **Arcade**: ~10-30 segundos (si tienes 10,000+ juegos)
- **Otras plataformas**: ~1-5 segundos cada una

**Nota**: Las cargas subsecuentes serán instantáneas gracias al sistema de caché.

---

## Desktop Mode

Desktop Mode es la interfaz principal de GameLauncher, optimizada para uso con mouse y teclado.

### Interfaz Principal

La ventana principal está dividida en 3 paneles:

```
┌──────────────┬────────────────────────┬───────────────┐
│   FILTROS    │    JUEGOS (GRID)       │   DETALLES    │
│              │                        │               │
│ Plataformas  │  ┌─────┬─────┬─────┐  │  Título       │
│ - Arcade     │  │ Box │ Box │ Box │  │  Desarrollador│
│ - NES        │  │ Art │ Art │ Art │  │  Género       │
│ - SNES       │  └─────┴─────┴─────┘  │  Rating       │
│ - PS1        │                        │  Descripción  │
└──────────────┴────────────────────────┴───────────────┘
```

#### Panel Izquierdo: Filtros

**Plataformas**:
- Lista de todas tus plataformas
- Haz click para filtrar juegos
- Indica número de juegos por plataforma

**Filtros Adicionales** (próximamente):
- Favoritos
- Completados
- Jugados recientemente
- Por género
- Por desarrollador

#### Panel Central: Lista de Juegos

**Barra de Herramientas (Toolbar)**:
- **Orden**: Selecciona el campo por el que ordenar (Título, Fecha, Rating, etc.).
- **Dirección**: Alterna entre orden Ascendente y Descendente.
- **Filtro Rápido**: Filtra instantáneamente por Favoritos, Instalados o Completados.

**Vista Grid**:
- Muestra carátulas en cuadrícula de alta densidad.
- Carga asincrónica de imágenes para scroll fluido.
- Prioridad de imagen principal (sin sufijo) para evitar duplicados visuales.

**Vista Lista**:
- Lista detallada con metadatos en columnas.
- Ordenable haciendo click en los encabezados.
- Optimización de carga virtualizada para miles de juegos.

#### Panel Derecho: Detalles del Juego

Muestra información completa del juego seleccionado:

- **Box Art**: Imagen grande
- **Título**: Nombre del juego
- **Plataforma**: Sistema de juego
- **Desarrollador**: Estudio desarrollador
- **Publisher**: Editorial
- **Fecha de Lanzamiento**: Año de lanzamiento
- **Género**: Categoría del juego
- **Rating**: Clasificación ESRB/PEGI
- **Descripción**: Sinopsis del juego

**Estadísticas** (si has jugado el juego):
- Tiempo jugado total
- Número de veces lanzado
- Última vez jugado

### Navegación

#### Con Mouse

**Seleccionar Juego**:
- Click simple: Selecciona (muestra detalles)
- Doble click: Lanza el juego

**Filtrar**:
- Click en plataforma del panel izquierdo

**Redimensionar Paneles**:
- Arrastra los GridSplitters (líneas verticales entre paneles)
- La configuración se guarda automáticamente

#### Con Teclado

**Navegación**:
- `↑↓←→`: Navegar por la lista de juegos
- `Enter`: Lanzar juego seleccionado
- `Tab`: Cambiar foco entre paneles
- `Esc`: Cancelar/Cerrar diálogos

**Búsqueda**:
- `Ctrl+F`: Abrir búsqueda
- `Esc`: Cerrar búsqueda

**Acciones**:
- `F5`: Recargar biblioteca
- `Ctrl+R`: Recargar caché
- `S`: Cambiar campo de orden
- `D`: Cambiar dirección de orden
- `Q`: Ciclar filtros rápidos
- `Tab`: Alternar entre Grid y Lista

### Lanzar Juegos

1. **Seleccionar Juego**
   - Navega o busca el juego que quieres jugar

2. **Lanzar**
   - Doble click en el juego
   - O presiona `Enter`
   - O click derecho → "Lanzar"

3. **Proceso de Lanzamiento**
   - **Feedback Visual**: Aparece una pantalla de carga ("Lanzando...") con el título del juego y una animación pulsante.
   - **Minimización**: La ventana de GameLauncher se minimiza automáticamente para no interferir con el emulador.
   - **Compatibilidad**: GameLauncher usa `UseShellExecute` como fallback para soportar emuladores basados en asociaciones de archivos (.lnk, .sfc, etc.).

4. **Durante el Juego**
   - El tracker de estadísticas cuenta el tiempo real de ejecución.
   - La música de fondo se pausa automáticamente.

5. **Al Cerrar el Juego**
   - GameLauncher se restaura y recupera el foco automáticamente.
   - Se actualiza el XML de plataforma de forma segura (preservando apps adicionales).
   - La música de fondo se reanuda.
   - Se actualizan las estadísticas: `PlayCount`, `PlayTime` y `LastPlayedDate`.

### Búsqueda Avanzada

#### Abrir Búsqueda

Presiona `Ctrl+F` o haz click en el botón "Buscar".

#### Buscar por Campos

La búsqueda es multi-campo y busca en:

- **Título del juego**
- **Desarrollador**
- **Publisher**
- **Género**
- **Plataforma**
- **Año de lanzamiento**

#### Ejemplos de Búsqueda

- `"street fighter"` → Encuentra todos los Street Fighter
- `"capcom"` → Encuentra todos los juegos de Capcom
- `"1991"` → Encuentra juegos de 1991
- `"rpg"` → Encuentra juegos de género RPG

#### Filtros Combinados

Próximamente: Búsqueda avanzada con múltiples filtros.

### Menú Contextual (Click Derecho)

Click derecho en un juego para acceder a:

**Acciones Rápidas**:
- ✅ Lanzar
- ✅ Marcar como Favorito
- ✅ Marcar como Completado
- ⏳ Editar Metadata
- ⏳ Ver en LaunchBox
- ⏳ Abrir carpeta de ROM

**Estadísticas**:
- ⏳ Ver estadísticas del juego
- ⏳ Resetear estadísticas

### Redimensionar Paneles

Los paneles son redimensionables:

1. **Posiciona el cursor** sobre el GridSplitter (línea vertical)
2. **Arrastra** izquierda/derecha
3. **Suelta** cuando estés satisfecho

El tamaño se guarda automáticamente en `Settings.xml`.

---

## BigScreen Mode

BigScreen Mode es una interfaz fullscreen optimizada para TVs y control por gamepad. Es la alternativa gratuita a BigBox.

### Iniciar BigScreen Mode

**Desde Desktop**:
- Archivo → Cambiar a BigScreen Mode (próximamente)

**Directamente**:
- Ejecuta `GameLauncher.BigScreen.exe`
- O crea un acceso directo en tu escritorio/Steam

### Configurar Gamepad

#### Gamepads Compatibles

**XInput** (Nativos):
- Xbox 360 Controller
- Xbox One Controller
- Xbox Series X/S Controller
- Gamepads genéricos con soporte XInput

**DirectInput** (Requiere emulación):
- PlayStation 4/5 Controller (con [DS4Windows](https://ds4windows.com/))
- Nintendo Switch Pro Controller (con [BetterJoy](https://github.com/Davidobot/BetterJoy))

#### Verificar Detección

Al iniciar BigScreen:
- Si el gamepad está conectado, verás el indicador en la esquina
- Presiona cualquier botón para verificar respuesta

### Controles por Defecto

**Navegación**:
- **D-Pad / Left Stick**: Mover selección
- **A (Button 1)**: Seleccionar/Confirmar
- **B (Button 2)**: Volver/Cancelar

**Acciones de Juego**:
- **A (Button 1)**: Confirmar / Ver detalles
- **X (Button 3)**: Galería de imágenes (I en teclado)
- **Y (Button 4)**: Menú de gestión / Editar (A en teclado)
- **Right Trigger**: Lanzar juego rápido
- **Left Trigger**: Toggle favorito
- **LB / RB**: Navegación rápida por secciones

**Sistema**:
- **Start**: Menú de opciones
- **Back**: Volver
- **Tab (Teclado)**: Cambiar modo de vista (Wheel / Lista)
- **S / D / Q (Teclado)**: Sort / Direction / Filter
- **Esc (Teclado)**: Salir de BigScreen

### Navegación por Vistas

BigScreen usa navegación stack-based:

```
Platform Filters → Games Wheel → Game Details
      ↑                ↑              ↑
   (B Back)        (B Back)       (B Back)
```

#### Vista 1: Platform Filters

**Función**: Seleccionar plataforma

**Controles**:
- `↑↓`: Navegar por lista de plataformas
- `A`: Seleccionar plataforma → Va a Games Wheel
- `B`: Salir (próximamente)

**Información Mostrada**:
- Nombre de plataforma
- Número de juegos
- Logo de plataforma (si existe)

#### Vista 2: Games Wheel / List

**Función**: Navegar juegos de la plataforma seleccionada. Soporta dos modos de vista alternables con `Tab`.

**Controles**:
- `←→`: Navegar por wheel horizontal (en modo Wheel).
- `↑↓`: Navegar lista vertical (en modo Lista).
- `A`: Ver detalles del juego → Va a Game Details.
- `B`: Volver a Platform Filters.
- `Right Trigger`: Lanzar juego directamente.
- `X`: Abrir Galería de imágenes.

**Información Mostrada**:
- Carátulas 3D con reflejos y sombreados.
- Título y metadatos rápidos.
- Badges de estado (Favorito, Completado, Roto).
- Indicador de calificación por estrellas interactivo.

#### Vista 3: Game Details

**Función**: Ver detalles completos y lanzar juego

**Controles**:
- `A`: Lanzar juego
- `B`: Volver a Games Wheel
- `Left Trigger`: Toggle favorito
- `↑↓`: Scroll en descripción (si es larga)

**Información Mostrada**:
- Box art grande
- Título
- Desarrollador, Publisher
- Género, Año
- Rating ESRB/PEGI
- Descripción completa
- Estadísticas de juego

### Transiciones

BigScreen incluye transiciones suaves entre vistas:

**Tipos de Transición**:
- **Fade**: Fade in/out (por defecto)
- **Slide Horizontal**: Deslizar izquierda/derecha
- **Slide Vertical**: Deslizar arriba/abajo
- **Rotate**: Rotación 3D (próximamente)

**Configuración**:
Edita `BigBoxSettings.xml` en la carpeta `Data/` de LaunchBox:

```xml
<ViewTransitionType>Fade</ViewTransitionType>
<TransitionDuration>300</TransitionDuration>
```

### Attract Mode (Modo Atracción)

GameLauncher incluye un protector de pantalla dinámico que se activa tras un periodo de inactividad.

- **Activación**: Se dispara automáticamente si no hay input (teclado/gamepad) durante el tiempo configurado en `IdleTimeout`.
- **Contenido**: Reproduce vídeos aleatorios de tus juegos a pantalla completa con efectos de scanlines.
- **Interacción**: Presiona cualquier tecla o botón para detener el modo Attract y volver instantáneamente a la biblioteca.
- **Información**: Muestra un overlay con el título y metadatos del juego que se está previsualizando.

### Salir de BigScreen

**Con Gamepad**:
- Presiona `B` repetidamente hasta volver a la pantalla de salida o usa el menú de sistema (`Start`).

**Con Teclado**:
- Presiona `Esc` en cualquier momento.

---

## Estadísticas

GameLauncher incluye un panel de estadísticas avanzadas que va más allá de LaunchBox.

### Acceder a Estadísticas

**Desktop Mode**:
- Menú → Estadísticas (próximamente)
- O shortcut `Ctrl+S` (próximamente)

### Panel de Resumen

**Cards Principales**:

1. **Tiempo Total de Juego**
   - Suma de tiempo en todos los juegos
   - Formato: "142h 35m"
   - Incluye todas las plataformas

2. **Juegos Jugados**
   - Número de juegos únicos lanzados al menos una vez
   - Excluye juegos sin PlayCount

3. **Plataformas Activas**
   - Plataformas con al menos 1 juego jugado
   - Útil para ver tu diversidad de gaming

### Top 10 Juegos Más Jugados

Lista ordenada por tiempo de juego:

**Columnas**:
- **Título**: Nombre del juego
- **Tiempo**: Tiempo total jugado (ej: "23h 15m")
- **Veces**: Número de veces lanzado (PlayCount)

**Orden**:
Por defecto ordenado por tiempo descendente.

### Top Plataformas

Lista de plataformas ordenadas por actividad:

**Columnas**:
- **Plataforma**: Nombre (ej: "Arcade")
- **Juegos**: Número de juegos jugados en esa plataforma
- **Tiempo Total**: Suma de tiempo en todos los juegos

**Orden**:
Por defecto ordenado por tiempo descendente.

### Gráficas (Próximamente v1.1)

**Gráfica de Actividad**:
- Line chart de tiempo de juego por día
- Últimos 30 días

**Distribución por Plataforma**:
- Pie chart de tiempo por plataforma
- Porcentajes visuales

**Progreso de Completación**:
- Bar chart de % de juegos completados por plataforma

### Exportar Estadísticas (Próximamente v1.1)

**Formatos**:
- CSV (Excel compatible)
- Excel (.xlsx)
- JSON (para análisis programático)

**Contenido**:
- Todos los juegos con estadísticas
- Agregados por plataforma
- Histórico de sesiones (futuro)

---

## Troubleshooting

### Problemas Comunes

#### 1. GameLauncher no inicia

**Síntomas**:
- Doble click en el ejecutable no hace nada
- La ventana se cierra inmediatamente

**Soluciones**:
1. **Verificar .NET 8.0 Runtime**
   ```bash
   dotnet --version
   # Debería mostrar 8.0.x o superior
   ```

2. **Ejecutar como Administrador**
   - Click derecho en `GameLauncher.Desktop.exe`
   - "Ejecutar como administrador"

3. **Revisar Event Viewer**
   - Win+R → `eventvwr`
   - Windows Logs → Application
   - Buscar errores recientes de GameLauncher

4. **Reinstalar .NET 8.0**
   - Descarga desde: https://dotnet.microsoft.com/download/dotnet/8.0
   - Instala .NET 8.0 Desktop Runtime

#### 2. No encuentra carpeta de LaunchBox

**Síntomas**:
- Error: "No se encontró la carpeta Data/ en la ruta especificada"

**Soluciones**:
1. **Verificar Ruta**
   - La carpeta debe contener `Data\Platforms.xml`
   - Ejemplo correcto: `H:\LaunchBox\LaunchBox\`
   - Ejemplo incorrecto: `H:\LaunchBox\` (falta subfolder)

2. **LaunchBox Portable**
   - Si usas LaunchBox portable, asegúrate de apuntar a la carpeta con `LaunchBox.exe`

3. **Permisos**
   - Verifica que tengas permisos de lectura en la carpeta

#### 3. Juegos no se lanzan

**Síntomas**:
- Al lanzar un juego, nada sucede
- Error: "No se pudo lanzar el emulador"

**Soluciones**:
1. **Verificar en LaunchBox**
   - Intenta lanzar el mismo juego desde LaunchBox
   - Si falla también, el problema es de configuración

2. **Verificar Emulador**
   - Abre `Data/Emulators.xml`
   - Verifica que `<ApplicationPath>` sea correcta
   - Verifica que el emulador exista en esa ruta

3. **Verificar ROM**
   - Abre `Data/Platforms/[Plataforma].xml`
   - Busca el juego y verifica `<ApplicationPath>`
   - Verifica que la ROM exista en esa ruta

4. **Revisar Command Line**
   - Algunos emuladores requieren parámetros específicos
   - Verifica en LaunchBox que funciona primero

#### 4. Performance Lento

**Síntomas**:
- GameLauncher tarda mucho en cargar
- Lag al navegar juegos
- Alto uso de RAM

**Soluciones**:
1. **Primera Carga Normal**
   - La primera carga de plataformas grandes es lenta (esperado)
   - Cargas subsecuentes son rápidas (caché)

2. **Reducir Caché**
   - Cierra otras aplicaciones
   - Considera filtrar por plataforma

3. **SSD Recomendado**
   - Si tienes LaunchBox en HDD, considera moverlo a SSD

4. **Invalidar Caché**
   - Cierra y abre GameLauncher
   - El caché se reconstruirá

#### 5. BigScreen no detecta gamepad

**Síntomas**:
- Gamepad no responde en BigScreen Mode
- No hay indicador de gamepad conectado

**Soluciones**:
1. **Verificar XInput**
   - GameLauncher solo soporta XInput (Xbox controllers)
   - Para PS4/PS5, usa [DS4Windows](https://ds4windows.com/)

2. **Drivers**
   - Asegúrate de tener drivers Xbox instalados
   - Windows 10/11 los incluye por defecto

3. **Probar en otro Juego**
   - Verifica que el gamepad funcione en Steam Big Picture o juegos

4. **Reconectar**
   - Desconecta y reconecta el gamepad
   - Cierra y abre GameLauncher

#### 6. Estadísticas no actualizan

**Síntomas**:
- Jugaste un juego pero las estadísticas no cambian
- PlayTime no se incrementa

**Soluciones**:
1. **Cerrar LaunchBox**
   - Si LaunchBox está abierto, ciérralo
   - LaunchBox cachea los XMLs

2. **Verificar XML**
   - Abre `Data/Platforms/[Plataforma].xml`
   - Busca el juego y verifica `<PlayCount>` y `<PlayTime>`
   - Debería estar actualizado

3. **Recargar Caché**
   - Presiona `F5` en GameLauncher
   - O cierra y abre GameLauncher

---

## Apéndices

### Apéndice A: Estructura de Archivos XML

#### Platforms.xml

```xml
<?xml version="1.0" standalone="yes"?>
<LaunchBox>
  <Platform>
    <Name>Nintendo Entertainment System</Name>
    <Category>Consoles</Category>
    <ReleaseDate>1983-07-15T00:00:00-07:00</ReleaseDate>
    <Developer>Nintendo</Developer>
    <Manufacturer>Nintendo</Manufacturer>
    <!-- ... más campos ... -->
  </Platform>
</LaunchBox>
```

#### Platforms/[Platform].xml

```xml
<?xml version="1.0" standalone="yes"?>
<LaunchBox>
  <Game>
    <ID>12345-guid-here</ID>
    <Title>Super Mario Bros.</Title>
    <Platform>Nintendo Entertainment System</Platform>
    <ApplicationPath>ROMs\NES\Super Mario Bros.nes</ApplicationPath>
    <PlayCount>47</PlayCount>
    <PlayTime>85500</PlayTime> <!-- en segundos -->
    <LastPlayed>2026-02-01T15:30:00-08:00</LastPlayed>
    <Favorite>true</Favorite>
    <!-- ... más campos ... -->
  </Game>
</LaunchBox>
```

### Apéndice B: Shortcuts de Teclado

#### Desktop Mode

| Shortcut | Acción |
|----------|--------|
| `Ctrl+F` | Buscar |
| `F5` | Recargar biblioteca |
| `Ctrl+R` | Recargar caché |
| `Enter` | Lanzar juego seleccionado |
| `Esc` | Cancelar/Cerrar |
| `Tab` | Cambiar foco entre paneles |
| `↑↓←→` | Navegar |

#### BigScreen Mode

| Control | Acción |
|---------|--------|
| `Esc` | Salir de BigScreen |
| `↑↓←→` o D-Pad | Navegar |
| `Enter` o A | Seleccionar |
| `Backspace` o B | Volver |

### Apéndice C: Configuración Avanzada

#### Editar Settings.xml

Ubicación: `[LaunchBox]\Data\Settings.xml`

**Campos importantes**:
- `<SidebarWidth>` - Ancho del panel de filtros (Desktop)
- `<GameDetailsWidth>` - Ancho del panel de detalles (Desktop)
- `<ThemeColor>` - Color del tema (ARGB integer)

#### Editar BigBoxSettings.xml

Ubicación: `[LaunchBox]\Data\BigBoxSettings.xml`

**Campos importantes**:
- `<ViewTransitionType>` - Tipo de transición (Fade, Slide, etc.)
- `<TransitionDuration>` - Duración en ms (default: 300)
- `<BigBoxTheme>` - Nombre del tema activo

### Apéndice D: Formatos de Tiempo

GameLauncher usa dos formatos de tiempo:

**PlayTime en XML**:
- Formato: Entero (segundos)
- Ejemplo: `85500` = 23 horas, 45 minutos

**PlayTime en UI**:
- Formato: "XXh XXm"
- Ejemplo: "23h 45m"

**Conversión**:
```
Segundos → Horas y Minutos
85500 / 3600 = 23.75 horas = 23h 45m
```

### Apéndice E: Recursos Adicionales

**Documentación**:
- [README.md](README.md) - Documentación principal
- [FAQ.md](FAQ.md) - Preguntas frecuentes
- [CHANGELOG.md](CHANGELOG.md) - Registro de cambios

**Links Externos**:
- [LaunchBox Official](https://www.launchbox-app.com/)
- [LaunchBox Forums](https://forums.launchbox-app.com/)
- [.NET 8.0 Download](https://dotnet.microsoft.com/download/dotnet/8.0)

**Comunidad**:
- GitHub Issues: https://github.com/tuusuario/GameLauncher/issues
- GitHub Discussions: https://github.com/tuusuario/GameLauncher/discussions

---

**Fin del Manual de Usuario**

**Versión**: 1.5.0-beta
**Última actualización**: 2026-04-27

¿Necesitas ayuda adicional? [Abre un issue](https://github.com/tuusuario/GameLauncher/issues) en GitHub.
