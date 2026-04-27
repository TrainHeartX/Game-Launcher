using System;
using System.IO;
using System.Windows;

namespace GameLauncher.UI.Shared.Themes
{
    /// <summary>
    /// Carga temas personalizados desde archivos XAML.
    /// </summary>
    public class ThemeLoader
    {
        public static void LoadTheme(string themePath)
        {
            try
            {
                if (!Directory.Exists(themePath))
                    return;

                var themeFile = Path.Combine(themePath, "Theme.xaml");
                if (!File.Exists(themeFile))
                    return;

                var themeUri = new Uri(themeFile, UriKind.Absolute);
                var themeDict = new ResourceDictionary { Source = themeUri };

                Application.Current.Resources.MergedDictionaries.Add(themeDict);
            }
            catch
            {
                // Ignorar errores de carga de tema
            }
        }

        public static void LoadDefaultTheme()
        {
            // Tema por defecto ya está en App.xaml
        }
    }
}
