Add-Type -AssemblyName System.Drawing
$destination = Join-Path $PSScriptRoot '../AudioSwitch/Skins/Default'

function RoundedRect($graphics, $brush, [float]$x, [float]$y, [float]$width, [float]$height, [float]$radius) {
    $path = [System.Drawing.Drawing2D.GraphicsPath]::new()
    $d = $radius * 2
    $path.AddArc($x, $y, $d, $d, 180, 90)
    $path.AddArc($x + $width - $d, $y, $d, $d, 270, 90)
    $path.AddArc($x + $width - $d, $y + $height - $d, $d, $d, 0, 90)
    $path.AddArc($x, $y + $height - $d, $d, $d, 90, 90)
    $path.CloseFigure()
    $graphics.FillPath($brush, $path)
    $path.Dispose()
}

function Brush($hex) {
    return [System.Drawing.SolidBrush]::new([System.Drawing.ColorTranslator]::FromHtml($hex))
}

foreach ($dark in @($false, $true)) {
    $suffix = if ($dark) { '-dark' } else { '' }
    $surface = Brush $(if ($dark) { '#292929' } else { '#F7F7F7' })
    $border = Brush $(if ($dark) { '#494949' } else { '#D8D8D8' })
    $track = Brush $(if ($dark) { '#505050' } else { '#DCDCDC' })
    $accent = Brush $(if ($dark) { '#76C9FF' } else { '#0078D4' })
    $foreground = Brush $(if ($dark) { '#F5F5F5' } else { '#202020' })
    foreach ($state in @('back', 'mute', 'device', 'meter')) {
        $width = if ($state -eq 'meter') { 168 } else { 200 }
        $height = if ($state -eq 'meter') { 6 } else { 40 }
        # Supersample the small rounded surfaces for smooth transparent edges.
        $large = [System.Drawing.Bitmap]::new($width * 4, $height * 4)
        $g = [System.Drawing.Graphics]::FromImage($large)
        $g.SmoothingMode = 'AntiAlias'
        $g.ScaleTransform(4, 4)
        if ($state -eq 'meter') {
            $g.FillRectangle($accent, 0, 0, $width, $height)
        } else {
            $shadow = [System.Drawing.SolidBrush]::new([System.Drawing.Color]::FromArgb(24, 0, 0, 0))
            RoundedRect $g $shadow 2 3 196 36 8
            RoundedRect $g $border 1 1 198 36 8
            RoundedRect $g $surface 2 2 196 34 7
            $shadow.Dispose()
            if ($state -eq 'back') { RoundedRect $g $track 16 17 168 6 3 }
            if ($state -eq 'mute') {
                $font = [System.Drawing.Font]::new('Segoe UI', 11, [System.Drawing.FontStyle]::Regular, [System.Drawing.GraphicsUnit]::Pixel)
                $format = [System.Drawing.StringFormat]::new()
                $format.Alignment = 'Center'
                $format.LineAlignment = 'Center'
                $g.DrawString('Muted', $font, $foreground, [System.Drawing.RectangleF]::new(0, 0, 200, 37), $format)
                $format.Dispose()
                $font.Dispose()
            }
        }
        $g.Dispose()
        $bitmap = [System.Drawing.Bitmap]::new($width, $height)
        $g = [System.Drawing.Graphics]::FromImage($bitmap)
        $g.InterpolationMode = 'HighQualityBicubic'
        $g.DrawImage($large, 0, 0, $width, $height)
        $bitmap.Save((Join-Path $destination "$state$suffix.png"), [System.Drawing.Imaging.ImageFormat]::Png)
        $g.Dispose()
        $bitmap.Dispose()
        $large.Dispose()
    }
    foreach ($brush in @($surface, $border, $track, $accent, $foreground)) { $brush.Dispose() }
}
