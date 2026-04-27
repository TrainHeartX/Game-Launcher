using System;
using System.Threading.Tasks;
using GameLauncher.Core.Models;

#if ANDROID
using Android.Content;
using Android.App;
#endif

namespace GameLauncher.Android.Services
{
    public class AndroidEmulatorLauncher
    {
        public async Task<(bool Success, string Message)> LaunchGameAsync(AndroidExportedGame game, string libraryDir)
        {
            if (!game.IsRetroArch)
            {
                return (false, "Este juego fue exportado como 'Sólo jugable en PC' porque no utiliza RetroArch.");
            }

            if (string.IsNullOrEmpty(game.RomPath))
            {
                return (false, "Ruta de ROM no válida.");
            }

            var fullRomPath = Path.Combine(libraryDir, game.RomPath);
            if (!File.Exists(fullRomPath))
            {
                return (false, $"Archivo ROM no encontrado en el dispositivo:\n{fullRomPath}");
            }

            // Determine core based on platform
            string coreSoName = GetRetroArchCoreForPlatform(game.Platform);
            if (string.IsNullOrEmpty(coreSoName))
            {
                return (false, $"No se pudo determinar qué Core de RetroArch usar para la plataforma '{game.Platform}'.");
            }

#if ANDROID
            try
            {
                var context = Application.Context;
                Intent intent = new Intent(Intent.ActionMain);
                intent.SetComponent(new global::Android.Content.ComponentName("com.retroarch.aarch64", "com.retroarch.browser.retroactivity.RetroActivityFuture"));
                
                // Extra parameters for RetroArch to load the core and the ROM automatically
                intent.PutExtra("ROM", fullRomPath);
                intent.PutExtra("LIBRETRO", $"/data/data/com.retroarch.aarch64/cores/{coreSoName}");
                intent.PutExtra("CONFIGFILE", "/storage/emulated/0/RetroArch/retroarch.cfg");
                intent.PutExtra("IME", "com.android.inputmethod.latin/.LatinIME");
                intent.PutExtra("DATADIR", "/data/data/com.retroarch.aarch64");
                intent.PutExtra("APK", "/data/app/com.retroarch.aarch64-1/base.apk");
                intent.PutExtra("SDCARD", "/storage/emulated/0");
                intent.PutExtra("DOWNLOADS", "/storage/emulated/0/Download");
                intent.PutExtra("SCREENSHOTS", "/storage/emulated/0/Pictures");
                intent.PutExtra("EXTERNAL", "/storage/emulated/0/Android/data/com.retroarch.aarch64/files");

                // Start Activity
                intent.AddFlags(ActivityFlags.NewTask | ActivityFlags.ClearTop);
                context.StartActivity(intent);

                return await Task.FromResult((true, "Lanzando RetroArch..."));
            }
            catch (Exception ex)
            {
                // Fallback to normal com.retroarch if aarch64 is not installed
                try
                {
                    var context = Application.Context;
                    Intent intent = new Intent(Intent.ActionMain);
                    intent.SetComponent(new global::Android.Content.ComponentName("com.retroarch", "com.retroarch.browser.retroactivity.RetroActivityFuture"));
                    
                    intent.PutExtra("ROM", fullRomPath);
                    intent.PutExtra("LIBRETRO", $"/data/data/com.retroarch/cores/{coreSoName}");
                    intent.PutExtra("CONFIGFILE", "/storage/emulated/0/RetroArch/retroarch.cfg");

                    intent.AddFlags(ActivityFlags.NewTask | ActivityFlags.ClearTop);
                    context.StartActivity(intent);

                    return await Task.FromResult((true, "Lanzando RetroArch (32-bit)..."));
                }
                catch (Exception ex2)
                {
                    return (false, $"Error al intentar lanzar RetroArch. Asegúrate de tenerlo instalado.\n{ex2.Message}");
                }
            }
#else
            return await Task.FromResult((false, "Lanzamiento solo soportado en dispositivos Android reales."));
#endif
        }

        private string GetRetroArchCoreForPlatform(string? platform)
        {
            if (string.IsNullOrEmpty(platform)) return "";

            // Simple mapping for common platforms (Can be extracted to a config file later)
            if (platform.Contains("Super Nintendo", StringComparison.OrdinalIgnoreCase) || platform.Contains("SNES", StringComparison.OrdinalIgnoreCase))
                return "snes9x_libretro_android.so";
                
            if (platform.Contains("Nintendo 64", StringComparison.OrdinalIgnoreCase) || platform.Contains("N64", StringComparison.OrdinalIgnoreCase))
                return "mupen64plus_next_libretro_android.so";
                
            if (platform.Contains("Game Boy Advance", StringComparison.OrdinalIgnoreCase) || platform.Contains("GBA", StringComparison.OrdinalIgnoreCase))
                return "mgba_libretro_android.so";
                
            if (platform.Contains("Sega Genesis", StringComparison.OrdinalIgnoreCase) || platform.Contains("Megadrive", StringComparison.OrdinalIgnoreCase))
                return "genesis_plus_gx_libretro_android.so";
                
            if (platform.Contains("PlayStation", StringComparison.OrdinalIgnoreCase) && !platform.Contains("2"))
                return "pcsx_rearmed_libretro_android.so";

            if (platform.Contains("Nintendo Entertainment System", StringComparison.OrdinalIgnoreCase) || platform.Contains("NES", StringComparison.OrdinalIgnoreCase))
                return "nestopia_libretro_android.so";

            // Default fallback or prompt user? For now just try a common one or return empty to throw an error
            return "snes9x_libretro_android.so"; 
        }
    }
}
