using System;
using System.Collections.Generic;

namespace GameLauncher.Core.Models
{
    public class AndroidLibraryManifest
    {
        public List<AndroidExportedGame> Games { get; set; } = new List<AndroidExportedGame>();
        public DateTime ExportDate { get; set; } = DateTime.UtcNow;
    }

    public class AndroidExportedGame
    {
        public string ID { get => Id; set => Id = value; }
        public string Id { get; set; } = "";
        public string? Title { get; set; }
        public string? Platform { get; set; }
        public string? Developer { get; set; }
        public string? Publisher { get; set; }
        public int? ReleaseYear { get; set; }
        public string? Genre { get; set; }
        public bool Completed { get; set; }
        public bool Favorite { get; set; }

        public bool IsRetroArch { get; set; }
        public string? RomPath { get; set; }
        public string? BoxFrontPath { get; set; }
        public string? ClearLogoPath { get; set; }
        public bool HasSaves { get; set; }
    }
}
