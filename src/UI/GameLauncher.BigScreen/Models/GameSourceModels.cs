using System.Collections.Generic;

namespace GameLauncher.BigScreen.Models;

public class GameSourceItem
{
    public string Title { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string SourceName { get; set; } = "BlizzBoyGames";
    public string Category { get; set; } = string.Empty; // e.g. "Altos Requisitos"
    public string UploadDate { get; set; } = string.Empty;
}

public class GameRequirementInfo
{
    public string Label { get; set; } = string.Empty; // e.g. "Mínimos" or "Recomendados"
    public string Os { get; set; } = string.Empty;
    public string Cpu { get; set; } = string.Empty;
    public string Ram { get; set; } = string.Empty;
    public string Gpu { get; set; } = string.Empty;
    public string DirectX { get; set; } = string.Empty;
    public string Storage { get; set; } = string.Empty;
}

public class GameSourceDetail : GameSourceItem
{
    public string Description { get; set; } = string.Empty;
    public List<GameRequirementInfo> Requirements { get; set; } = new();
    public List<GameDownloadLink> DownloadLinks { get; set; } = new();
    public List<string> Screenshots { get; set; } = new();
}

public class GameDownloadLink
{
    public string Server { get; set; } = string.Empty; // e.g. "Mega", "Mediafire"
    public string Url { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty; // e.g. "Part 1"
}
