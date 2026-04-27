
$html = Get-Content -Path "H:\GameLauncher\game_page.html" -Raw

# Normalize as in C#
$clean = $html.Replace("<strong>", "").Replace("</strong>", "").Replace("<b>", "").Replace("</b>", "").Replace("<br />", "`n").Replace("<br>", "`n").Replace("<p>", "").Replace("</p>", "`n").Replace("&nbsp;", " ")

# Extract Block
$startMarker = "Requisitos Mínimos"
$endMarkerPattern = "Requisitos Recomendados"

$startIdx = $clean.IndexOf($startMarker)
if ($startIdx -eq -1) {
    Write-Host "Marker '$startMarker' not found!"
    exit
}

$remainder = $clean.Substring($startIdx + $startMarker.Length)
if ($remainder -match $endMarkerPattern) {
    $block = $remainder.Substring(0, $Matches.Index).Trim()
}
else {
    $block = $remainder.Trim()
}

Write-Host "--- MIN BLOCK START ---"
Write-Host $block
Write-Host "--- MIN BLOCK END ---"

# Extract Field helper
function ExtractField($text, $pattern) {
    if ($text -match "(?s)$pattern") {
        return $Matches[1].Trim()
    }
    return "No especificado"
}

Write-Host "OS: $(ExtractField $block 'Sistema Operativo:\s*(.*?)(\n|Procesador|$)')"
Write-Host "CPU: $(ExtractField $block 'Procesador:\s*(.*?)(\n|RAM|Memoria|$)')"
Write-Host "RAM: $(ExtractField $block '(RAM|Memoria):\s*(.*?)(\n|Gráficos|Video|Tarjeta|$)')"
Write-Host "GPU: $(ExtractField $block '(Gráficos|Video|Tarjeta de video|Tarjeta Gráfica):\s*(.*?)(\n|DirectX|$)')"
Write-Host "DirectX: $(ExtractField $block 'DirectX:\s*(.*?)(\n|Almacenamiento|$)')"
Write-Host "Storage: $(ExtractField $block 'Almacenamiento:\s*(.*?)(\n|Notas|sonido|$)')"

Write-Host "`n--- SCREENSHOTS ---"
# ParseScreenshots simulation
$h1Index = $html.IndexOf("<h1")
if ($h1Index -eq -1) { $h1Index = 0 }
$limitIndex = $html.IndexOf("gp-related-wrapper")
if ($limitIndex -eq -1) { $limitIndex = $html.IndexOf("jp-relatedposts") }
if ($limitIndex -eq -1) { $limitIndex = $html.Length }

$content = $html.Substring($h1Index, [Math]::Min($limitIndex - $h1Index, $html.Length - $h1Index))
$imgMatches = [regex]::Matches($content, '<img\s+[^>]*src="([^"]+)"')
foreach ($m in $imgMatches) {
    $src = $m.Groups[1].Value
    $lowerSrc = $src.ToLower()
    if ($lowerSrc -like "*icon*" -or $lowerSrc -like "*logo*" -or $lowerSrc -like "*b-gratis*" -or 
        $lowerSrc -like "*banner*" -or $lowerSrc -like "*separador*" -or $lowerSrc -like "*requisitos*" -or
        $lowerSrc -like "*enlaces*" -or $lowerSrc -like "*instalar*" -or $lowerSrc -like "*descargar*" -or
        $lowerSrc -like "*necesarios*" -or $lowerSrc -like "*boton*" -or $lowerSrc -like "*cdgif*") {
        continue
    }
    Write-Host "IMG: $src"
}
