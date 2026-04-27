using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GameLauncher.BigScreen.Models;

namespace GameLauncher.BigScreen.Services;

public class BlizzBoyGamesService : IGameSourceService
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "https://www.blizzboygames.net/";

    public string SourceName => "BlizzBoyGames";

    public BlizzBoyGamesService()
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
    }

    public async Task<List<GameSourceItem>> GetLatestGamesAsync()
    {
        try
        {
            var html = await _httpClient.GetStringAsync(BaseUrl);
            return ParseHomeGames(html);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error fetching games: {ex.Message}");
            return new List<GameSourceItem>();
        }
    }

    public async Task<GameSourceDetail> GetGameDetailsAsync(string url)
    {
        var detail = new GameSourceDetail { Url = url };
        try
        {
            var html = await _httpClient.GetStringAsync(url);
            
            // 1. Description (Main text area)
            var descMatch = Regex.Match(html, @"Descripci[oó]n\.png", RegexOptions.IgnoreCase);
            if (descMatch.Success)
            {
                var descStart = descMatch.Index;
                var fichaMatch = Regex.Match(html, @"Ficha-T[eé]cnica\.png", RegexOptions.IgnoreCase);
                var capturasMatch = Regex.Match(html, @"Capturas\.png", RegexOptions.IgnoreCase);
                
                int descEnd = html.Length;
                if (fichaMatch.Success && fichaMatch.Index > descStart) descEnd = fichaMatch.Index;
                else if (capturasMatch.Success && capturasMatch.Index > descStart) descEnd = capturasMatch.Index;
                
                var descChunk = html.Substring(descStart, descEnd - descStart);
                detail.Description = StripHtml(descChunk).Replace("Descripción", "", StringComparison.OrdinalIgnoreCase).Replace("Descripcion", "", StringComparison.OrdinalIgnoreCase).Trim();
            }

            // 2. Extra Info (Version, Password, Included)
            var passMatch = Regex.Match(html, @"Contraseña:\s*(.*?)(<br|<\/p)", RegexOptions.IgnoreCase);
            var verMatch = Regex.Match(html, @"Versión:\s*(.*?)(<br|<\/p)", RegexOptions.IgnoreCase);
            
            string extraInfo = "";
            if (passMatch.Success) extraInfo += $"🔐 Contraseña: {StripHtml(passMatch.Groups[1].Value)}\n";
            if (verMatch.Success) extraInfo += $"ℹ️ Versión: {StripHtml(verMatch.Groups[1].Value)}\n";

            var incIndex = html.IndexOf("Esta Versión Incluye:", StringComparison.OrdinalIgnoreCase);
            if (incIndex != -1)
            {
                var endIndex = html.IndexOf("Para Instalar", incIndex, StringComparison.OrdinalIgnoreCase);
                if (endIndex == -1) endIndex = html.Length;
                var incBlock = html.Substring(incIndex, Math.Min(endIndex - incIndex, 1000));
                extraInfo += $"\n{StripHtml(incBlock)}";
            }

            if (!string.IsNullOrEmpty(extraInfo))
            {
                detail.Description += $"\n\n--- INFORMACIÓN ADICIONAL ---\n{extraInfo}";
            }

            // 3. Screenshots
            detail.Screenshots = ParseScreenshots(html);

            // 4. Download Links
            detail.DownloadLinks = ParseDownloadLinks(html);

            // 5. Requirements
            detail.Requirements = ParseRequirements(html);
        
            return detail;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error fetching details: {ex.Message}");
            detail.Description = "Error al cargar los detalles.";
            return detail;
        }
    }

    private List<GameRequirementInfo> ParseRequirements(string html)
    {
        var list = new List<GameRequirementInfo>();

        // Normalize HTML by stripping most tags but keeping/converting breaks
        var cleanRequirementsHtml = html.Replace("<br />", "\n").Replace("<br>", "\n").Replace("&nbsp;", " ");
        // Strip other tags like <strong>, <span>, <font>, etc. even with attributes
        cleanRequirementsHtml = Regex.Replace(cleanRequirementsHtml, @"<(strong|b|span|p|/p|font|u|i|em).*?>", "", RegexOptions.IgnoreCase);

        // Try to find blocks using markers
        var minBlock = ExtractBlock(cleanRequirementsHtml, "Requisitos Mínimos", "Requisitos Recomendados");
        var recBlock = ExtractBlock(cleanRequirementsHtml, "Requisitos Recomendados", "SERVIDORES|Intercambiables|Instrucciones|TRAILER|ESTA VERSIÓN INCLUYE|VARIOS SERVIDORES|DESCARGAR JUEGO");

        if (!string.IsNullOrEmpty(minBlock)) list.Add(ParseSingleRequirementBlock(minBlock, "Mínimos"));
        if (!string.IsNullOrEmpty(recBlock)) list.Add(ParseSingleRequirementBlock(recBlock, "Recomendados"));

        return list;
    }

    private string ExtractBlock(string html, string startMarker, string endMarkerPattern)
    {
        var startIdx = html.IndexOf(startMarker, StringComparison.OrdinalIgnoreCase);
        if (startIdx == -1) return string.Empty;

        var remainder = html.Substring(startIdx + startMarker.Length);
        
        // Find end
        var endMatch = Regex.Match(remainder, endMarkerPattern, RegexOptions.IgnoreCase);
        if (endMatch.Success)
        {
            return remainder.Substring(0, endMatch.Index).Trim();
        }

        return remainder.Trim(); // Fallback take rest
    }

    private GameRequirementInfo ParseSingleRequirementBlock(string text, string label)
    {
        var info = new GameRequirementInfo { Label = label };

        info.Os = ExtractField(text, @"Sistema Operativo:\s*(.*?)(\n|Procesador|$)");
        info.Cpu = ExtractField(text, @"Procesador:\s*(.*?)(\n|RAM|Memoria|$)");
        info.Ram = ExtractField(text, @"(RAM|Memoria):\s*(.*?)(\n|Gráficos|Video|Tarjeta|$)");
        info.Gpu = ExtractField(text, @"(Gráficos|Video|Tarjeta de video|Tarjeta Gráfica):\s*(.*?)(\n|DirectX|$)");
        info.DirectX = ExtractField(text, @"DirectX:\s*(.*?)(\n|Almacenamiento|$)");
        info.Storage = ExtractField(text, @"Almacenamiento:\s*(.*?)(\n|Notas|sonido|$)");

        return info;
    }

    private string ExtractField(string text, string pattern)
    {
        var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
        if (match.Success)
        {
            var value = match.Groups[match.Groups.Count - 1].Value.Trim();
            // Cleanup leading/trailing separators if any
            return CleanupValue(value);
        }
        return "No especificado";
    }

    private string CleanupValue(string val)
    {
        val = StripHtml(val).Trim();
        if (val.EndsWith(",")) val = val.TrimEnd(',');
        return val;
    }

    private List<string> ParseScreenshots(string html)
    {
        var list = new List<string>();
        
        // Find "Capturas" section using regex for flexibility
        var capturasMatch = Regex.Match(html, @"Capturas\.png", RegexOptions.IgnoreCase);
        var startIdx = capturasMatch.Success ? capturasMatch.Index : html.IndexOf("<h1", StringComparison.OrdinalIgnoreCase);
        if (startIdx == -1) startIdx = 0;
        
        // Find "Requisitos" header as end of screenshots
        var requisitosMatch = Regex.Match(html, @"Requisitos\.png", RegexOptions.IgnoreCase);
        var endIdx = requisitosMatch.Success && requisitosMatch.Index > startIdx ? requisitosMatch.Index : -1;
        
        if (endIdx == -1) endIdx = html.IndexOf("gp-related-wrapper", startIdx, StringComparison.OrdinalIgnoreCase);
        if (endIdx == -1) endIdx = html.IndexOf("<footer", startIdx, StringComparison.OrdinalIgnoreCase);
        if (endIdx == -1) endIdx = html.Length;
        
        var content = html.Substring(startIdx, endIdx - startIdx);

        var matches = Regex.Matches(content, @"<img\s+[^>]*src=""([^""]+)""", RegexOptions.IgnoreCase);
        foreach (Match match in matches)
        {
            var src = match.Groups[1].Value;
            var lowerSrc = src.ToLower();

            // Filter out system/UI images and common banners
            if (lowerSrc.Contains("icon") || lowerSrc.Contains("logo") || lowerSrc.Contains("b-gratis") || 
                lowerSrc.Contains("banner") || lowerSrc.Contains("separador") || lowerSrc.Contains("requisitos") ||
                lowerSrc.Contains("enlaces") || lowerSrc.Contains("instalar") || lowerSrc.Contains("descargar") ||
                lowerSrc.Contains("necesarios") || lowerSrc.Contains("boton") || lowerSrc.Contains("cdgif") ||
                lowerSrc.Contains("capturas") || lowerSrc.Contains("descripción") || lowerSrc.Contains("ficha-técnica")) continue;
            
            // Filter out Instant Gaming or other external ad-banners (usually they have fixed dimensions in the URL or name)
            if (lowerSrc.Contains("728x90") || lowerSrc.Contains("300x250")) continue;
             
            if (!list.Contains(src)) list.Add(src);
        }
        
        return list.Take(12).ToList();
    }

    private List<GameDownloadLink> ParseDownloadLinks(string html)
    {
        var list = new List<GameDownloadLink>();
        
        // Similar to Pivi, look for specific keywords in A tags.
        // Also handling 'blizzgame.php' which is their redirector.
        
        var matches = Regex.Matches(html, @"<a\s+[^>]*href=""([^""]+)""[^>]*>(.*?)</a>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
        foreach (Match match in matches)
        {
            var url = match.Groups[1].Value;
            var text = StripHtml(match.Groups[2].Value);
            
             // Filter invalid
            if (string.IsNullOrEmpty(url) || url.StartsWith("#") || url.Contains("javascript")) continue;

            string server = "";
            var lowerUrl = url.ToLower();
            var lowerText = text.ToLower();

            if (lowerUrl.IndexOf("mega.nz", StringComparison.OrdinalIgnoreCase) >= 0 || lowerText.IndexOf("mega", StringComparison.OrdinalIgnoreCase) >= 0) server = "MEGA";
            else if (lowerUrl.IndexOf("mediafire.com", StringComparison.OrdinalIgnoreCase) >= 0 || lowerText.IndexOf("mediafire", StringComparison.OrdinalIgnoreCase) >= 0) server = "Mediafire";
            else if (lowerUrl.IndexOf("drive.google.com", StringComparison.OrdinalIgnoreCase) >= 0 || lowerText.IndexOf("drive", StringComparison.OrdinalIgnoreCase) >= 0) server = "Google Drive";
            else if (lowerUrl.IndexOf("torrent", StringComparison.OrdinalIgnoreCase) >= 0 || lowerText.IndexOf("torrent", StringComparison.OrdinalIgnoreCase) >= 0) server = "Torrent";
            else if (lowerUrl.IndexOf("blizzgame.php", StringComparison.OrdinalIgnoreCase) >= 0) server = "Ver Enlaces / Descargar"; 
            else if (lowerText.IndexOf("descargar juego", StringComparison.OrdinalIgnoreCase) >= 0 || lowerText.IndexOf("ver enlaces", StringComparison.OrdinalIgnoreCase) >= 0) server = "Descargar";

            if (!string.IsNullOrEmpty(server))
            {
                 // Fix Relative URLs
                 if (!url.StartsWith("http") && !url.StartsWith("www")) url = BaseUrl.TrimEnd('/') + "/" + url.TrimStart('/');
                 
                 if (!list.Any(l => l.Url == url))
                 {
                     list.Add(new GameDownloadLink
                     {
                         Server = server,
                         Url = url,
                         Note = text.Length < 30 ? text : "Link"
                     });
                 }
            }
        }
        return list;
    }

    private string StripHtml(string input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;
        var noHtml = Regex.Replace(input, "<.*?>", String.Empty);
        return System.Net.WebUtility.HtmlDecode(noHtml).Trim();
    }

    private List<GameSourceItem> ParseHomeGames(string html)
    {
        var games = new List<GameSourceItem>();
        
        // Define section headers as they likely appear in the HTML
        var sections = new Dictionary<string, string>
        {
            { "Altos Requisitos", @"(JUEGOS DE\s+)?ALTOS REQUISITOS" },
            { "Medios Requisitos", @"(JUEGOS DE\s+)?MEDIOS REQUISITOS" },
            { "Bajos Requisitos", @"(JUEGOS DE\s+)?BAJOS REQUISITOS" }
        };

        var normalizedHtml = html;

        // 1. Process specific requirement blocks
        foreach (var section in sections)
        {
            var categoryName = section.Key;
            var pattern = section.Value;
            
            // Search for the header text in the HTML
            var headerMatch = Regex.Match(normalizedHtml, $">{pattern}", RegexOptions.IgnoreCase);
            if (!headerMatch.Success) continue;

            var startIdx = headerMatch.Index;

            // Find next section or end of specific area
            // Heuristic: next h2/h3 or category link or footer
            var endIdx = normalizedHtml.IndexOf("<h2", startIdx + 10, StringComparison.OrdinalIgnoreCase);
            if (endIdx == -1) endIdx = normalizedHtml.IndexOf("<footer", startIdx, StringComparison.OrdinalIgnoreCase);
            if (endIdx == -1) endIdx = normalizedHtml.Length;

            var sectionHtml = normalizedHtml.Substring(startIdx, endIdx - startIdx);
            var sectionGames = ExtractGamesFromHtml(sectionHtml, categoryName);
            foreach (var g in sectionGames)
            {
                if (!games.Any(existing => existing.Url == g.Url))
                {
                    games.Add(g);
                }
            }
        }

        // 2. Process everything else as "Novedades" or "Últimos Juegos" if not already caught
        var latestGames = ExtractGamesFromHtml(normalizedHtml, "Novedades");
        foreach (var g in latestGames)
        {
            if (!games.Any(existing => existing.Url == g.Url))
            {
                games.Add(g);
            }
        }
        
        return games.DistinctBy(g => g.Url).Take(50).ToList();
    }

    private List<GameSourceItem> ExtractGamesFromHtml(string html, string category)
    {
        var games = new List<GameSourceItem>();
        
        // Regex to capture (URL) and (Title/Image)
        var matches = Regex.Matches(html, @"<a\s+href=""([^""]+)""[^>]*>\s*<img\s+[^>]*src=""([^""]+)""[^>]*alt=""([^""]+)""", RegexOptions.Singleline | RegexOptions.IgnoreCase);
        
        if (matches.Count == 0)
        {
             matches = Regex.Matches(html, @"<a\s+href=""([^""]+)""[^>]*>\s*<img\s+[^>]*src=""([^""]+)""", RegexOptions.Singleline | RegexOptions.IgnoreCase);
        }

        foreach (Match match in matches)
        {
            var url = match.Groups[1].Value;
            var img = match.Groups[2].Value;
            var title = match.Groups.Count > 3 ? match.Groups[3].Value : "";

            if (string.IsNullOrEmpty(title))
            {
                try {
                    var uri = new Uri(url);
                    title = uri.Segments.Last().Replace("-", " ").Replace("/", "").Trim();
                } catch { continue; }
            }

            title = System.Net.WebUtility.HtmlDecode(title).Trim();
            
            if (string.IsNullOrWhiteSpace(title)) continue;
            
            var lowerTitle = title.ToLower();
            var lowerUrl = url.ToLower();

            if (lowerTitle.IndexOf("descargar netflix", StringComparison.OrdinalIgnoreCase) >= 0 || 
                lowerTitle.IndexOf("descargar disney", StringComparison.OrdinalIgnoreCase) >= 0 || 
                lowerTitle.IndexOf("descargar hbo", StringComparison.OrdinalIgnoreCase) >= 0 || 
                lowerTitle.IndexOf("descargar crunchyroll", StringComparison.OrdinalIgnoreCase) >= 0 ||
                lowerTitle.IndexOf("oferta -", StringComparison.OrdinalIgnoreCase) >= 0 || 
                lowerTitle.IndexOf("juegos gratis de la semana", StringComparison.OrdinalIgnoreCase) >= 0 ||
                lowerTitle.IndexOf("ver mas", StringComparison.OrdinalIgnoreCase) >= 0 ||
                lowerTitle.IndexOf("contacto", StringComparison.OrdinalIgnoreCase) >= 0 ||
                lowerTitle.IndexOf("dmca", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                continue;
            }
            
            if (lowerUrl.IndexOf("/categoria/", StringComparison.OrdinalIgnoreCase) >= 0) continue;
            if (lowerTitle == "altos requisitos" || lowerTitle == "medios requisitos" || lowerTitle == "bajos requisitos") continue;

            games.Add(new GameSourceItem
            {
                Title = title,
                ImageUrl = img,
                Url = url,
                SourceName = SourceName,
                Category = category
            });
        }
        return games;
    }
}
