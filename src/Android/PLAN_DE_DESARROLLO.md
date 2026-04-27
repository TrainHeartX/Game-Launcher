# Proyecto: GameLauncher.Android & Sincronización PC-Android

## Objetivo
Crear una aplicación complementaria para Android capaz de importar juegos, metadata, imágenes, videos y **partidas guardadas (saves/states)** desde la aplicación de PC (`GameLauncher.Desktop`). El objetivo principal es permitir al usuario jugar en RetroArch en su PC, exportar el progreso al celular, y continuar jugando allí de forma fluida. Los juegos que no sean de RetroArch (ej. juegos nativos de Windows) deberán mostrarse en la interfaz del celular como solo lectura, indicando que solo se pueden jugar en PC.

## Preguntas Abiertas (Por favor, respóndeme esto en el chat)
1. **Método de Transferencia:** ¿Prefieres que la PC genere un archivo `.zip` que tú pasas al celular por cable USB/Google Drive, o preferirías que el programa de PC tenga un "Servidor Local" y el celular descargue los juegos por Wi-Fi presionando un botón?
2. **Cores de RetroArch:** RetroArch en PC usa núcleos terminados en `.dll` y en Android terminan en `.so`. ¿Tienes ya descargados los mismos *cores* en el RetroArch de tu celular, o usamos los que vienen por defecto en la versión de Android?
3. **Mapeo de Rutas de RetroArch:** Para que el PC extraiga tus partidas guardadas, ¿tu RetroArch está dentro de la carpeta de LaunchBox (`H:\LaunchBox\LaunchBox\Emulators\RetroArch`) o lo tienes instalado aparte?

## Cambios Propuestos

### 1. GameLauncher.Desktop (PC Export System)
Se añadirá la lógica para agrupar todo lo necesario de un juego y prepararlo para Android.

#### [NEW] AndroidExportService.cs
- Detectará si el emulador del juego es RetroArch.
- Leerá la configuración de RetroArch (`retroarch.cfg`) para encontrar las carpetas de `saves/` y `states/`.
- Agrupará la ROM, las imágenes de LaunchBox, el video, y los archivos `.srm` (partida) o `.state` (estados de guardado).
- Creará un paquete exportable con un archivo JSON minificado (`AndroidLibrary.json`) para que el celular lo lea rápidamente.

#### [MODIFY] MainViewModel.cs
- Se añadirá un comando `ExportToAndroidCommand` para los juegos seleccionados y listas de reproducción.

### 2. GameLauncher.Android (App Móvil)
Se creará un nuevo proyecto en `h:\GameLauncher\src\Android\GameLauncher.Android.csproj` usando **.NET MAUI**.

#### [NEW] Proyecto MAUI
- Referenciará a `GameLauncher.Core` para usar los mismos modelos (`Game`, `Playlist`).
- Interfaz gráfica basada en XAML pero optimizada para táctil (carruseles y cuadrículas).

#### [NEW] AndroidEmulatorLauncher.cs
- Lógica inteligente del botón "Jugar":
  - Si el emulador original era RetroArch, creará un `Android.Content.Intent` dirigido a `com.retroarch` pasándole la ROM importada y el Core correspondiente.
  - Si el emulador NO era RetroArch (ej. juegos de PC o emuladores exclusivos de PC), el botón cambiará de texto a "Sólo jugable en PC" y se deshabilitará.

#### [NEW] AndroidImportService.cs
- Descomprimirá el paquete enviado por la PC.
- Moverá los `saves` y `states` directamente a la carpeta pública de RetroArch en el almacenamiento de Android (`/storage/emulated/0/RetroArch/saves/`) para que el RetroArch del celular los detecte automáticamente al abrir.
