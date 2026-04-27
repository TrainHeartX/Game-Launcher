# Preguntas Frecuentes (FAQ)

## General

### ¿Qué es GameLauncher?

GameLauncher es una aplicación moderna de gestión de juegos y emuladores para Windows que es 100% compatible con los archivos de datos de LaunchBox. Permite usar ambas aplicaciones en paralelo sin conflictos.

### ¿Es GameLauncher gratis?

Sí, GameLauncher es completamente gratuito y de código abierto. A diferencia de BigBox (la interfaz TV de LaunchBox), no requiere licencia de pago.

### ¿Reemplaza a LaunchBox?

No necesariamente. GameLauncher es compatible con LaunchBox, por lo que puedes usar ambos programas intercambiablemente. Comparten los mismos archivos XML, así que los cambios en uno se reflejan en el otro.

### ¿Necesito LaunchBox instalado?

Sí, necesitas tener LaunchBox instalado porque GameLauncher lee y escribe sus archivos de configuración XML. GameLauncher no viene con su propia base de datos de juegos.

---

## Instalación y Configuración

### ¿Cómo instalo GameLauncher?

1. Descarga la última versión desde Releases
2. Extrae el archivo ZIP a una carpeta de tu elección
3. Ejecuta `GameLauncher.Desktop.exe` o `GameLauncher.BigScreen.exe`
4. En la primera ejecución, selecciona la carpeta raíz de LaunchBox

### ¿Dónde se guardan las configuraciones de GameLauncher?

GameLauncher guarda su configuración local (como la ruta de LaunchBox) en:
```
C:\Users\TuUsuario\AppData\Local\GameLauncher\
```

Los datos de juegos, plataformas y estadísticas se guardan en los archivos XML de LaunchBox.

### ¿Puedo cambiar la ubicación de LaunchBox después de la configuración inicial?

Sí, puedes modificar la ruta en el archivo de configuración local o reconfigurarlo desde la aplicación (próximamente en la interfaz de Settings).

### ¿GameLauncher funciona con LaunchBox Portable?

Sí, funciona perfectamente con instalaciones portables de LaunchBox. Solo necesitas apuntar a la carpeta correcta durante la configuración inicial.

---

## Compatibilidad

### ¿Qué versiones de LaunchBox son compatibles?

GameLauncher es compatible con todas las versiones recientes de LaunchBox que usan el formato XML estándar (v10.x, v11.x, v12.x, v13.x).

### ¿Funciona con Big Box?

GameLauncher.BigScreen es una alternativa a Big Box. No necesitas Big Box instalado. Sin embargo, sí lee la configuración de BigBoxSettings.xml si existe.

### ¿Puedo usar temas de LaunchBox/BigBox?

**Desktop**: Los temas de LaunchBox Desktop no son compatibles (usan tecnología diferente).

**BigScreen**: Compatibilidad básica con temas de BigBox. Algunos temas pueden requerir adaptaciones debido a diferencias en namespaces.

### ¿Mis estadísticas se sincronizan entre LaunchBox y GameLauncher?

Sí, completamente. Ambas aplicaciones leen y escriben los mismos campos XML:
- `PlayCount` - Número de veces jugado
- `PlayTime` - Tiempo total de juego (en segundos)
- `LastPlayed` - Última vez jugado
- `Favorite` - Marcado como favorito
- `Completed` - Marcado como completado

### ¿GameLauncher modifica mis archivos de LaunchBox?

Solo modifica los archivos XML cuando haces cambios (como lanzar un juego, marcar favorito, etc.). Los cambios son compatibles 100% con LaunchBox y preservan la estructura XML exacta.

**Recomendación**: Siempre haz backup de tu carpeta `Data/` antes de usar cualquier software nuevo.

---

## Desktop Mode

### ¿Cómo navego por mis juegos?

- **Panel Izquierdo**: Haz click en una plataforma para filtrar juegos
- **Panel Central**: Visualiza la lista/grid de juegos
- **Panel Derecho**: Muestra detalles del juego seleccionado
- **Búsqueda**: Presiona `Ctrl+F` para buscar

### ¿Cómo lanzo un juego?

- Doble click en el juego
- O selecciona el juego y presiona Enter
- O click derecho → "Lanzar"

### ¿Puedo personalizar el diseño?

Los GridSplitters entre paneles son redimensionables arrastrándolos con el mouse. La configuración se guarda automáticamente.

### ¿Cómo busco juegos?

Presiona `Ctrl+F` y escribe:
- Título del juego
- Desarrollador
- Género
- Plataforma
- Cualquier campo de metadata

La búsqueda es en tiempo real y busca en todas las plataformas.

### ¿Puedo marcar juegos como favoritos?

Sí, click derecho en el juego → "Marcar como Favorito". El cambio se guarda en el XML y es visible en LaunchBox.

---

## BigScreen Mode

### ¿Para qué sirve BigScreen Mode?

BigScreen Mode es una interfaz fullscreen optimizada para TVs y monitores grandes, controlada completamente por gamepad. Es la alternativa gratuita a BigBox.

### ¿Qué gamepads son compatibles?

Actualmente soportamos gamepads compatibles con XInput:
- Xbox 360 Controller
- Xbox One Controller
- Xbox Series X/S Controller
- Gamepads de PC compatibles con XInput

**Nota**: DirectInput gamepads (como PS4/PS5 sin DS4Windows) no están soportados todavía.

### ¿Cómo uso mi gamepad PS4/PS5?

Usa [DS4Windows](https://ds4windows.com/) para emular un gamepad Xbox. GameLauncher lo detectará como XInput.

### ¿Cuáles son los controles por defecto?

- **D-Pad / Left Stick**: Navegar
- **A (Button 1)**: Seleccionar
- **B (Button 2)**: Volver
- **Right Trigger**: Lanzar juego
- **Left Trigger**: Marcar favorito
- **Start**: Menú de opciones (próximamente)

### ¿Puedo personalizar los controles?

Los controles son configurables desde `BigBoxSettings.xml`. Una interfaz gráfica para personalización está en el roadmap (v1.1).

### ¿Cómo salgo de BigScreen Mode?

Presiona `Esc` en el teclado o navega hacia atrás hasta la vista principal y selecciona "Salir".

### ¿GameLauncher tiene modo de protector de pantalla (Attract Mode)?

Sí, la versión 1.5.0-beta incluye un **Modo Attract** robusto. Se activa automáticamente tras un periodo de inactividad (configurable) y reproduce vídeos aleatorios de tus juegos con efectos de scanlines y metadatos en pantalla.

### ¿BigScreen soporta vídeos de juegos?

¡Sí! El soporte de vídeo ahora está integrado mediante **LibVLCSharp**. El modo Attract utiliza esta tecnología para reproducir previews de tus juegos. El soporte para vídeos en segundo plano durante la navegación normal está siendo optimizado para la v1.6.

---

## Estadísticas

### ¿Qué estadísticas trackea GameLauncher?

GameLauncher trackea automáticamente:
- Tiempo de juego por sesión
- Número de veces que lanzaste cada juego
- Última vez que jugaste
- Estadísticas agregadas por plataforma

### ¿Dónde veo mis estadísticas?

**Desktop Mode**: Panel de Estadísticas en el menú principal.

**BigScreen Mode**: Próximamente en v1.1.

### ¿Puedo exportar mis estadísticas?

La exportación a CSV/Excel está planeada para v1.1.

### ¿Las estadísticas son retroactivas?

GameLauncher usa los datos existentes en LaunchBox (PlayCount, PlayTime). Si ya tienes estadísticas en LaunchBox, GameLauncher las mostrará y seguirá actualizándolas.

---

## Performance

### GameLauncher tarda mucho en cargar

**Primera carga**: La primera vez que cargas una plataforma con muchos juegos (ej: Arcade con 10,000+ juegos), puede tardar algunos segundos mientras parsea el XML.

**Cargas posteriores**: El sistema de caché hace que cargas subsecuentes sean instantáneas.

**Solución**: Ten paciencia en la primera carga. El caché se invalida automáticamente cuando los archivos XML cambian.

### ¿Cuánta memoria RAM usa?

Depende del tamaño de tu colección:
- **Pequeña** (1,000 juegos): ~100-200 MB
- **Mediana** (10,000 juegos): ~300-500 MB
- **Grande** (50,000+ juegos): ~800 MB - 1 GB

El caché en memoria es necesario para buena performance.

### ¿Puedo reducir el uso de memoria?

El caché es esencial para performance. Si tienes limitaciones de RAM, considera:
- Cerrar otras aplicaciones
- Usar solo Desktop o solo BigScreen (no ambos a la vez)
- Filtrar por plataforma en lugar de cargar todas a la vez

---

## Problemas Comunes

### LaunchBox no detecta mis cambios

**Problema**: Lancé un juego en GameLauncher pero LaunchBox no muestra el tiempo de juego actualizado.

**Solución**: LaunchBox carga los XML al iniciar. Cierra LaunchBox completamente y vuelve a abrirlo para ver los cambios.

### BigScreen no detecta mi gamepad

**Problema**: GameLauncher.BigScreen no responde al gamepad.

**Solución**:
1. Verifica que sea un gamepad XInput (Xbox 360/One/Series)
2. Asegúrate de que los drivers estén instalados
3. Prueba el gamepad en otro juego primero
4. Cierra y abre GameLauncher

### Los juegos no se lanzan

**Problema**: Al lanzar un juego, nada sucede.

**Solución**:
1. Verifica que el emulador esté configurado en LaunchBox
2. Verifica que la ROM exista en la ruta especificada
3. Intenta lanzar el juego desde LaunchBox para descartar problemas de configuración
4. Revisa los logs (próximamente)

### GameLauncher se cierra inesperadamente

**Problema**: La aplicación se cierra sin mensaje de error.

**Solución**:
1. Verifica que tengas .NET 8.0 Runtime instalado
2. Intenta ejecutar como Administrador
3. Revisa el Event Viewer de Windows para errores
4. Reporta el issue en GitHub con detalles

### Error "Archivo XML corrupto"

**Problema**: GameLauncher dice que un archivo XML está corrupto.

**Solución**:
1. **IMPORTANTE**: Restaura desde tu backup de `Data/`
2. Verifica que LaunchBox pueda abrir el archivo
3. Si LaunchBox también tiene problemas, el archivo puede estar corrupto
4. Reporta el issue en GitHub con el archivo de ejemplo (sin datos personales)

### Las imágenes de juegos no se muestran

**Problema**: GameLauncher no muestra las carátulas/box art.

**Solución**:
1. Verifica que las imágenes existan en la carpeta `Images/` de LaunchBox
2. Verifica que el juego tenga el campo `ImagePath` o `BoxFrontPath` en el XML
3. Las imágenes se buscan en el orden: Box Front → Clear Logo → Screenshot

---

## Desarrollo y Contribución

### ¿Puedo contribuir al proyecto?

¡Sí! GameLauncher es de código abierto. Contribuciones son bienvenidas:
- Reportar bugs
- Solicitar features
- Enviar pull requests
- Mejorar documentación
- Crear temas personalizados

### ¿Cómo reporto un bug?

Abre un issue en GitHub: https://github.com/tuusuario/GameLauncher/issues

Incluye:
- Versión de GameLauncher
- Versión de Windows
- Pasos para reproducir el bug
- Screenshots si es posible

### ¿Cómo compilo desde el código fuente?

```bash
git clone https://github.com/tuusuario/GameLauncher.git
cd GameLauncher
dotnet restore
dotnet build
dotnet run --project src/UI/GameLauncher.Desktop/GameLauncher.Desktop.csproj
```

### ¿Qué tecnologías usa GameLauncher?

- .NET 8.0 / C# 12
- WPF/XAML
- CommunityToolkit.Mvvm
- XInputDotNetPure
- NUnit (tests)

---

## Seguridad y Privacidad

### ¿GameLauncher recopila datos?

No. GameLauncher no recopila ningún dato ni telemetría. Todo es local.

### ¿GameLauncher se conecta a internet?

No. GameLauncher funciona 100% offline. No hace llamadas a APIs externas ni envía datos.

### ¿Es seguro usar GameLauncher con mi colección de LaunchBox?

Sí. GameLauncher tiene tests de compatibilidad (round-trip tests) que garantizan que los archivos XML no se corrompen. Sin embargo, **siempre haz backup** de tu carpeta `Data/` antes de usar cualquier software nuevo.

### ¿GameLauncher incluye malware?

No. El código fuente es público y puedes compilarlo tú mismo. No incluye malware, adware ni spyware.

---

## Futuras Características

### ¿Cuándo estará disponible [característica X]?

Revisa el [Roadmap](README.md#-roadmap) en el README para ver características planeadas.

### ¿Puedo solicitar una característica?

¡Sí! Abre un issue en GitHub con la etiqueta "feature request".

### ¿Habrá soporte para Linux/macOS?

Está en el roadmap para v2.0 usando Avalonia UI. Por ahora, GameLauncher es solo para Windows.

### ¿Habrá una versión web o móvil?

Está en el roadmap para v2.0:
- Servidor web para acceso remoto
- App móvil companion (iOS/Android)

---

## Comparación con LaunchBox/BigBox

### ¿Cuáles son las ventajas de GameLauncher?

**vs LaunchBox Desktop**:
- Estadísticas avanzadas con gráficas
- Interface moderna con .NET 8
- Código abierto

**vs BigBox**:
- **Gratis** (BigBox requiere licencia de $75 USD)
- Más ligero y rápido
- Código abierto y personalizable

### ¿Cuáles son las desventajas?

**vs LaunchBox**:
- Menos features (editor de metadata, scraper, etc.)
- Menos maduro (en desarrollo activo)

**vs BigBox**:
- Menos temas disponibles
- Menos vistas/layouts
- Sin video playback todavía
- Menos opciones de personalización

### ¿Debería usar GameLauncher o LaunchBox?

Depende de tus necesidades:

**Usa GameLauncher si**:
- Quieres una alternativa gratuita a BigBox
- Te interesa el código abierto
- Quieres estadísticas avanzadas
- Tienes conocimientos técnicos para compilar/contribuir

**Usa LaunchBox si**:
- Necesitas todas las features (scraper, editor, etc.)
- Prefieres software maduro y estable
- No te importa pagar por BigBox
- Quieres soporte oficial

**Usa ambos**:
- ¡Puedes usar ambos en paralelo sin conflictos!
- Usa LaunchBox para gestión y GameLauncher para jugar

---

## Contacto y Soporte

### ¿Dónde puedo obtener ayuda?

- **GitHub Issues**: https://github.com/tuusuario/GameLauncher/issues
- **GitHub Discussions**: https://github.com/tuusuario/GameLauncher/discussions
- **Wiki**: https://github.com/tuusuario/GameLauncher/wiki

### ¿Hay un Discord o foro?

Próximamente. Por ahora usa GitHub Discussions.

### ¿Puedo donar al proyecto?

GameLauncher es gratuito y de código abierto. Si quieres apoyar el proyecto, considera:
- Contribuir con código o documentación
- Reportar bugs y mejorar la calidad
- Compartir el proyecto con otros usuarios de LaunchBox

---

**Última actualización**: 2026-04-27

**Versión**: 1.5.0-beta

¿No encuentras respuesta a tu pregunta? [Abre un issue](https://github.com/tuusuario/GameLauncher/issues) en GitHub.
