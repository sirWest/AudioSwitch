param([string]$Title = 'AudioSwitch Settings', [string]$Tab, [string]$Output = "$PSScriptRoot/../artifacts/settings.png", [int]$ProcessId)
$ErrorActionPreference = 'Stop'
Add-Type -AssemblyName UIAutomationClient, UIAutomationTypes, System.Drawing
Add-Type @'
using System;
using System.Runtime.InteropServices;
public static class WindowCapture {
    [DllImport("user32.dll")] public static extern bool PrintWindow(IntPtr window, IntPtr dc, uint flags);
}
'@
$condition = [System.Windows.Automation.PropertyCondition]::new([System.Windows.Automation.AutomationElement]::NameProperty, $Title)
if ($ProcessId) { $condition = [System.Windows.Automation.AndCondition]::new($condition, [System.Windows.Automation.PropertyCondition]::new([System.Windows.Automation.AutomationElement]::ProcessIdProperty, $ProcessId)) }
$window = [System.Windows.Automation.AutomationElement]::RootElement.FindFirst([System.Windows.Automation.TreeScope]::Children, $condition)
if ($null -eq $window) { throw "Window not found: $Title" }
if ($Tab) {
    $tabCondition = [System.Windows.Automation.AndCondition]::new(
        [System.Windows.Automation.PropertyCondition]::new([System.Windows.Automation.AutomationElement]::NameProperty, $Tab),
        [System.Windows.Automation.PropertyCondition]::new([System.Windows.Automation.AutomationElement]::ControlTypeProperty, [System.Windows.Automation.ControlType]::TabItem))
    $item = $window.FindFirst([System.Windows.Automation.TreeScope]::Descendants, $tabCondition)
    ([System.Windows.Automation.SelectionItemPattern]$item.GetCurrentPattern([System.Windows.Automation.SelectionItemPattern]::Pattern)).Select()
    Start-Sleep -Milliseconds 400
}
$rect = $window.Current.BoundingRectangle
$bitmap = [System.Drawing.Bitmap]::new([int]$rect.Width, [int]$rect.Height)
$graphics = [System.Drawing.Graphics]::FromImage($bitmap)
try {
    try { $graphics.CopyFromScreen([int]$rect.Left, [int]$rect.Top, 0, 0, $bitmap.Size) }
    catch {
        $dc = $graphics.GetHdc()
        try { if (-not [WindowCapture]::PrintWindow([IntPtr]$window.Current.NativeWindowHandle, $dc, 2)) { throw 'Window capture failed.' } }
        finally { $graphics.ReleaseHdc($dc) }
    }
    $outputPath = [System.IO.Path]::GetFullPath($Output)
    [System.IO.Directory]::CreateDirectory([System.IO.Path]::GetDirectoryName($outputPath)) | Out-Null
    $bitmap.Save($outputPath, [System.Drawing.Imaging.ImageFormat]::Png)
    Write-Output $outputPath
} finally { $graphics.Dispose(); $bitmap.Dispose() }
