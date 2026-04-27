
$html = Get-Content -Path "H:\GameLauncher\nioh_page.html" -Raw

# Normalize as in New C# logic
$clean = $html.Replace("<br />", "`n").Replace("<br>", "`n").Replace("&nbsp;", " ")
$clean = [regex]::Replace($clean, '<(strong|b|span|p|/p|font|u|i|em).*?>', "", "IgnoreCase")

# Extract Block
function ExtractBlock($html, $startMarker, $endMarkerPattern) {
    $startIdx = $html.IndexOf($startMarker)
    if ($startIdx -eq -1) { return "" }
    $remainder = $html.Substring($startIdx + $startMarker.Length)
    if ($remainder -match $endMarkerPattern) {
        return $remainder.Substring(0, $Matches.Index).Trim()
    }
    return $remainder.Trim()
}

$minBlock = ExtractBlock $clean "Requisitos Mínimos" "Requisitos Recomendados"
Write-Host "--- MIN BLOCK START ---"
Write-Host $minBlock
Write-Host "--- MIN BLOCK END ---"

# Extract Field helper
function ExtractField($text, $pattern) {
    if ($text -match "(?s)$pattern") {
        return $Matches[1].Trim()
    }
    return "No especificado"
}

Write-Host "OS: $(ExtractField $minBlock 'Sistema Operativo:\s*(.*?)(\n|Procesador|$)')"
Write-Host "CPU: $(ExtractField $minBlock 'Procesador:\s*(.*?)(\n|RAM|Memoria|$)')"
Write-Host "RAM: $(ExtractField $minBlock '(RAM|Memoria):\s*(.*?)(\n|Gráficos|Video|Tarjeta|$)')"
Write-Host "GPU: $(ExtractField $minBlock '(Gráficos|Video|Tarjeta de video|Tarjeta Gráfica):\s*(.*?)(\n|DirectX|$)')"
Write-Host "DirectX: $(ExtractField $minBlock 'DirectX:\s*(.*?)(\n|Almacenamiento|$)')"
Write-Host "Storage: $(ExtractField $minBlock 'Almacenamiento:\s*(.*?)(\n|Notas|sonido|$)')"

Write-Host "`n--- SCREENSHOTS ---"
$startIdx = $html.IndexOf("Capturas.png")
if ($startIdx -eq -1) { $startIdx = $html.IndexOf("<h1") }
if ($startIdx -eq -1) { $startIdx = 0 }

$endIdx = $html.IndexOf("Requisitos.png", $startIdx)
if ($endIdx -eq -1) { $endIdx = $html.IndexOf("gp-related-wrapper", $startIdx) }
if ($endIdx -eq -1) { $endIdx = $html.Length }

$content = $html.Substring($startIdx, $endIdx - $startIdx)
$imgMatches = [regex]::Matches($content, '<img\s+[^>]*src="([^"]+)"')
foreach ($m in $imgMatches) {
    $src = $m.Groups[1].Value
    $lowerSrc = $src.ToLower()
    if ($lowerSrc -like "*icon*" -or $lowerSrc -like "*logo*" -or $lowerSrc -like "*b-gratis*" -or 
        $lowerSrc -like "*banner*" -or $lowerSrc -like "*separador*" -or $lowerSrc -like "*requisitos*" -or
        $lowerSrc -like "*enlaces*" -or $lowerSrc -like "*instalar*" -or $lowerSrc -like "*descargar*" -or
        $lowerSrc -like "*necesarios*" -or $lowerSrc -like "*boton*" -or $lowerSrc -like "*cdgif*" -or
        $lowerSrc -like "*capturas*" -or $lowerSrc -like "*descripción*" -or $lowerSrc -like "*ficha-técnica*" -or
        $lowerSrc -like "*728x90*" -or $lowerSrc -like "*300x250*") {
        continue
    }
    Write-Host "IMG: $src"
}
