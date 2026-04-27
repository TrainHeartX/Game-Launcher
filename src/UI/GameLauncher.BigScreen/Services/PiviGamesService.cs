using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using GameLauncher.BigScreen.Models;

namespace GameLauncher.BigScreen.Services;

public class PiviGamesService : IGameSourceService
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "https://pivigames.blog/";

    public string SourceName => "PiviGames";

    public PiviGamesService()
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/91.0.4472.124 Safari/537.36");
    }

    public async Task<List<GameSourceItem>> GetLatestGamesAsync()
    {
        var allGames = new List<GameSourceItem>();
        try
        {
            // Scrape first 3 pages
            for (int i = 1; i <= 3; i++)
            {
                string url = i == 1 ? BaseUrl : $"{BaseUrl}page/{i}/";
                var html = await _httpClient.GetStringAsync(url);
                var games = ParseHomeGames(html);
                allGames.AddRange(games);
            }
            return allGames.DistinctBy(g => g.Url).ToList();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error fetching PiviGames: {ex.Message}");
            return allGames;
        }
    }

    private string StripHtml(string input)
    {
        if (string.IsNullOrEmpty(input)) return string.Empty;
        // Replace common block tags with newlines to preserve structure
        input = Regex.Replace(input, @"<(br|p|div|li)[^>]*>", "\n", RegexOptions.IgnoreCase);
        // Remove all other tags
        input = Regex.Replace(input, @"<[^>]+>", "", RegexOptions.IgnoreCase);
        // Decode HTML entities
        return System.Net.WebUtility.HtmlDecode(input).Trim();
    }

    public async Task<GameSourceDetail> GetGameDetailsAsync(string url)
    {
        var detail = new GameSourceDetail { Url = url };
        try
        {
            var html = await _httpClient.GetStringAsync(url);
            
            // Strat 1: Meta Description (Good for brief, but misses Ficha Técnica/Includes)
            // User wants FULL info including Ficha Técnica and "Esta versión incluye".
            // So we will prioritize the entry-content extraction over the meta description if entry-content is found.
            
            string fullDescription = "";
            var entryContentIndex = html.IndexOf("class=\"entry-content\"", StringComparison.OrdinalIgnoreCase);
            
            if (entryContentIndex != -1)
            {
                var startIndex = entryContentIndex + "class=\"entry-content\"".Length;
                
                // Find end markers - We want to Keep FICHA TÉCNICA and INCLUDES
                // We only strictly want to cut off at "REQUISITOS" or Related Posts because Requisitos has its own section
                var endIndex = html.IndexOf("REQUISITOS DEL SISTEMA", startIndex, StringComparison.OrdinalIgnoreCase);
                if (endIndex == -1) endIndex = html.IndexOf("<div id=\"jp-relatedposts\"", startIndex, StringComparison.OrdinalIgnoreCase);
                // Also cut off at "Ver Enlaces" or "Instrucciones" if they appear as main headers
                if (endIndex == -1) endIndex = html.IndexOf("<h3>ENLACES", startIndex, StringComparison.OrdinalIgnoreCase);

                // If no markers found, just take a safe chunk
                if (endIndex == -1) endIndex = Math.Min(startIndex + 4000, html.Length);

                var rawContent = html.Substring(startIndex, endIndex - startIndex);
                
                // Clean up HTML but preserve some structure
                fullDescription = StripHtml(rawContent);
            }

            // Fallback to meta if content failed
            if (string.IsNullOrEmpty(fullDescription) || fullDescription.Length < 50)
            {
                 var metaMatch = Regex.Match(html, @"<meta\s+property=""og:description""\s+content=""([^""]+)""", RegexOptions.IgnoreCase);
                 if (metaMatch.Success) fullDescription = System.Net.WebUtility.HtmlDecode(metaMatch.Groups[1].Value);
            }

            detail.Description = !string.IsNullOrEmpty(fullDescription) ? fullDescription.Trim() : "Descripción disponible en la web.";
            
            // Extract Requirements
            detail.Requirements = ParseRequirements(html);

            // Extract Screenshots
            detail.Screenshots = ParseScreenshots(html);

            // Extract Download Links
            detail.DownloadLinks = ParseDownloadLinks(html);

            return detail;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error fetching details: {ex.Message}");
            detail.Description = "Error al cargar los detalles.";
            return detail;
        }
    }

    private List<string> ParseScreenshots(string html)
    {
        var list = new List<string>();
        // Find entry content
        var contentMatch = Regex.Match(html, @"class=""entry-content""[^>]*>(.*?)<div id=""jp-relatedposts""", RegexOptions.Singleline | RegexOptions.IgnoreCase);
        if (!contentMatch.Success) 
             contentMatch = Regex.Match(html, @"class=""entry-content""[^>]*>(.*?)REQUISITOS", RegexOptions.Singleline | RegexOptions.IgnoreCase);

        if (contentMatch.Success)
        {
            var content = contentMatch.Groups[1].Value;
            // Find images
            var imgMatches = Regex.Matches(content, @"<img\s+[^>]*src=""([^""]+)""", RegexOptions.IgnoreCase);
            foreach (Match match in imgMatches)
            {
                var src = match.Groups[1].Value;
                // Filter out small icons/emojis or specific UI elements
                if (src.Contains("icon") || src.Contains("emoji") || src.Contains("logo")) continue;
                
                // Get high res if available
                // Re-match the full tag to check for data-large-file
                 var fullTagMatch = Regex.Match(content, @"<img\s+[^>]*src=""" + Regex.Escape(src) + @"""[^>]*>", RegexOptions.IgnoreCase);
                 if (fullTagMatch.Success)
                 {
                     var largeMatch = Regex.Match(fullTagMatch.Value, @"data-large-file=""([^""]+)""", RegexOptions.IgnoreCase);
                     if (largeMatch.Success) src = largeMatch.Groups[1].Value;
                 }

                if (!list.Contains(src)) list.Add(src);
            }
        }
        return list.Take(10).ToList();
    }

    private List<GameDownloadLink> ParseDownloadLinks(string html)
    {
        var list = new List<GameDownloadLink>();
        
        // PiviGames often puts links in "Ver Enlaces" drop-downs or direct buttons.
        // It's tricky because they use "paste" services or "share" links.
        // We look for <a> tags with specific keywords in text or href.

        // Pattern: <a href="(url)"> (Icon) (ServerName) </a>
        // Or text "DESCARGAR"
        
        // Let's grab all A tags and filter.
        var linkMatches = Regex.Matches(html, @"<a\s+[^>]*href=""([^""]+)""[^>]*>(.*?)</a>", RegexOptions.Singleline | RegexOptions.IgnoreCase);

        foreach (Match match in linkMatches)
        {
            var url = match.Groups[1].Value;
            var text = StripHtml(match.Groups[2].Value);
            
            // Filter invalid
            if (string.IsNullOrEmpty(url) || url.StartsWith("#") || url.Contains("javascript")) continue;
            
            // Check for keywords
            string server = "";
            
            // Normalize for checking
            var checkUrl = url.ToLower();
            var checkText = text.ToLower();

            if (checkUrl.Contains("mega.nz") || checkText.Contains("mega")) server = "MEGA";
            else if (checkUrl.Contains("mediafire.com") || checkText.Contains("mediafire")) server = "Mediafire";
            else if (checkUrl.Contains("drive.google.com") || checkText.Contains("google drive") || checkText.Contains("drive")) server = "Google Drive";
            else if (checkUrl.Contains("u.pcloud.link") || checkText.Contains("pcloud")) server = "pCloud";
            else if (checkUrl.Contains("pixeldrain") || checkText.Contains("pixeldrain")) server = "Pixeldrain";
            else if (checkUrl.Contains("1fichier") || checkText.Contains("1fichier")) server = "1Fichier";
            else if (checkUrl.Contains("gofile") || checkText.Contains("gofile")) server = "Gofile";
            else if (checkUrl.Contains("qiwi") || checkText.Contains("qiwi")) server = "Qiwi";
            else if (checkUrl.Contains("torrent") || checkText.Contains("torrent")) server = "Torrent";
            else if (checkUrl.Contains("elamigos") || checkText.Contains("elamigos")) server = "ElAmigos";
            else if (checkUrl.Contains("lolaup") || checkText.Contains("lolaup")) server = "LolaUp";
            else if (checkUrl.Contains("solred") || checkText.Contains("solred")) server = "SolRed";
            else if (checkUrl.Contains("uptocash") || checkText.Contains("uptocash")) server = "UptoCash";
            else if (checkUrl.Contains("game-2u") || checkText.Contains("game-2u")) server = "Game-2u"; 
            else if (checkText.Contains("crack") || checkText.Contains("online") || checkText.Contains("steamworks")) server = "Crack/Online Fix";
            else if (checkText.Contains("ver enlaces") || checkText.Contains("descargar"))
            {
                // Generic link, might be a pastebin or folder
                server = "Descarga / Ver Enlaces";
            }

            if (!string.IsNullOrEmpty(server))
            {
                // Fix Relative URLs
                if (!url.StartsWith("http")) continue;

                if (!list.Any(l => l.Url == url))
                {
                    list.Add(new GameDownloadLink
                    {
                        Server = server,
                        Url = url,
                        Note = text.Length < 50 ? text : "Link"
                    });
                }
            }
        }

        return list;
    }

    private List<GameSourceItem> ParseHomeGames(string html)
    {
        var games = new List<GameSourceItem>();
        
        // PiviGames uses specific <section class="gp-post-item ..."> blocks for each entry.
        // Identify sections that are standard posts and filter out ads/offers.
        
        var sectionMatches = Regex.Matches(html, @"<section\s+[^>]*class=""([^""]*gp-post-item[^""]*)""[^>]*>(.*?)</section>", RegexOptions.Singleline | RegexOptions.IgnoreCase);

        foreach (Match match in sectionMatches)
        {
            var classes = match.Groups[1].Value.ToLower();
            var content = match.Groups[2].Value;

            // Filter out offers, external links, and "link" formats (usually ads like Netflix/Disney or Instant Gaming)
            if (classes.Contains("category-oferta") || 
                classes.Contains("format-link") || 
                classes.Contains("post-format-link"))
            {
                continue;
            }

            // Extract URL and Title
            // Try to find the title in <div class="gp-loop-title"> first as it is cleaner
            // Pattern: <div class="gp-loop-title"><a href="..." title="...">TITLE</a></div>
            var titleMatch = Regex.Match(content, @"<div class=""gp-loop-title"">\s*<a\s+href=""([^""]+)""[^>]*>(.*?)</a>", RegexOptions.Singleline | RegexOptions.IgnoreCase);
            
            string url = "";
            string title = "";

            if (titleMatch.Success)
            {
                url = titleMatch.Groups[1].Value;
                title = titleMatch.Groups[2].Value;
            }
            else
            {
                // Fallback to thumbnail anchor if title div is missing
                var thumbMatch = Regex.Match(content, @"<div class=""gp-post-thumbnail[^""]*"">\s*<a\s+href=""([^""]+)""\s+title=""([^""]+)""", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                if (thumbMatch.Success)
                {
                    url = thumbMatch.Groups[1].Value;
                    title = thumbMatch.Groups[2].Value;
                }
            }

            if (string.IsNullOrEmpty(url) || string.IsNullOrEmpty(title)) continue;

            // Strict URL filtering to ensure we stay on the blog to avoid external redirects/ads
            if (!url.Contains("pivigames.blog")) continue;
            
            // Content Filtering (Titles)
            if (title.Contains("Solución", StringComparison.OrdinalIgnoreCase) ||
                title.Contains("Error", StringComparison.OrdinalIgnoreCase) ||
                title.Contains("Problema", StringComparison.OrdinalIgnoreCase) ||
                title.Contains("Descargar e Instalar", StringComparison.OrdinalIgnoreCase) || // Often tutorials
                title.Contains("Regalan", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            // Image Extraction
            // Try to get data-large-file first for best quality, then fallback to src
            string imgUrl = "";
            var imgLargeMatch = Regex.Match(content, @"data-large-file=""([^""]+)""", RegexOptions.IgnoreCase);
            if (imgLargeMatch.Success)
            {
                imgUrl = imgLargeMatch.Groups[1].Value;
            }
            else
            {
                // Fallback to src
                var imgSrcMatch = Regex.Match(content, @"<img\s+[^>]*src=""([^""]+)""", RegexOptions.IgnoreCase);
                if (imgSrcMatch.Success)
                {
                    imgUrl = imgSrcMatch.Groups[1].Value;
                }
            }

            title = System.Net.WebUtility.HtmlDecode(title).Trim();

            // Deduplicate
            if (!games.Any(g => g.Url == url))
            {
                games.Add(new GameSourceItem
                {
                    Title = title,
                    ImageUrl = imgUrl,
                    Url = url,
                    SourceName = SourceName
                });
            }
        }
        
        return games;
    }

    private List<GameRequirementInfo> ParseRequirements(string html)
    {
        var list = new List<GameRequirementInfo>();
        
        // Extract the Requirements Section specifically
        var reqBlockMatch = Regex.Match(html, @"REQUISITOS DEL SISTEMA(.*?)(INSTRUCCIONES|ENLACES|You May Also Like|VER COMENTARIOS|$)", RegexOptions.Singleline | RegexOptions.IgnoreCase);
        
        if (reqBlockMatch.Success)
        {
            var rawBlock = reqBlockMatch.Groups[1].Value;
            
            // Pre-clean the block to handle list items which are common in Pivi
            // <li><strong>SO:</strong> Windows 10</li>  ->  SO: Windows 10
            var cleanBlock = rawBlock
                .Replace("<li>", "\n")
                .Replace("</li>", "")
                .Replace("<br>", "\n")
                .Replace("<br />", "\n");
                
            cleanBlock = StripHtml(cleanBlock);

            // Now we have plain text with newlines.
            
            // Try to split into Min and Rec
            var minIdx = cleanBlock.IndexOf("Mínimos", StringComparison.OrdinalIgnoreCase);
            var recIdx = cleanBlock.IndexOf("Recomendados", StringComparison.OrdinalIgnoreCase);

            if (minIdx != -1)
            {
                string minText = (recIdx != -1) ? cleanBlock.Substring(minIdx, recIdx - minIdx) : cleanBlock.Substring(minIdx);
                list.Add(ParseSingleRequirementBlock(minText, "Mínimos"));
            }

            if (recIdx != -1)
            {
                string recText = cleanBlock.Substring(recIdx);
                list.Add(ParseSingleRequirementBlock(recText, "Recomendados"));
            }
        }

        return list;
    }

    private GameRequirementInfo ParseSingleRequirementBlock(string text, string label)
    {
        var info = new GameRequirementInfo { Label = label };

        // Regex updated to be strictly looking for COLONS (:) to avoid false positives like "Requiere un procesador..."
        // Pattern: "(Label)\s*:\s*(Value)"
        
        info.Os = ExtractField(text, @"(SO|SISTEMA OPERATIVO|OS)\s*[:]\s*(.*?)(Procesador|CPU|Memoria|\n|$)");
        info.Cpu = ExtractField(text, @"(Procesador|CPU)\s*[:]\s*(.*?)(Memoria|RAM|Gráficos|\n|$)");
        info.Ram = ExtractField(text, @"(Memoria|RAM)\s*[:]\s*(.*?)(Gráficos|Video|DirectX|\n|$)");
        info.Gpu = ExtractField(text, @"(Gráficos|Video|GPU|Tarjeta Gráfica)\s*[:]\s*(.*?)(DirectX|Almacenamiento|Espacio|\n|$)");
        info.Storage = ExtractField(text, @"(Almacenamiento|Espacio|Disco)\s*[:]\s*(.*?)(Tarjeta|Notas|\n|$)");

        return info;
    }

    private string ExtractField(string text, string pattern)
    {
        var match = Regex.Match(text, pattern, RegexOptions.IgnoreCase | RegexOptions.Singleline);
        if (match.Success)
        {
            var val = match.Groups[2].Value.Trim();
            
            // Critical: Remove the label if it was captured inside the value group
            // Example: "Procesador: Intel Core i5..." captured "Procesador: Intel Core i5..." if regex was loose.
            // Our regex is strict on capturing group 2 after the colon, but let's be safe.
            // Also remove common suffixes or next-line artifacts
            
            // Remove "Procesador:" or "Gráficos:" if they appear at start (shouldn't with current regex but extra safety)
            val = Regex.Replace(val, @"^(Procesador|CPU|Memoria|RAM|Gráficos|Video|DirectX|Almacenamiento)\s*[:]\s*", "", RegexOptions.IgnoreCase);
            
            return val.TrimEnd(',', '.', ' ', '\t', '\n', '\r');
        }
        return "No especificado";
    }
}
