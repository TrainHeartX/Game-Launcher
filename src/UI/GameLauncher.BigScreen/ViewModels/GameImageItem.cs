namespace GameLauncher.BigScreen.ViewModels
{
    /// <summary>
    /// Representa una imagen de un juego en la galería.
    /// </summary>
    public class GameImageItem
    {
        /// <summary>Ruta completa al archivo de imagen.</summary>
        public string FullPath { get; set; } = string.Empty;

        /// <summary>Tipo de imagen (Box - Front, Screenshot, etc.).</summary>
        public string TypeName { get; set; } = string.Empty;
    }
}
