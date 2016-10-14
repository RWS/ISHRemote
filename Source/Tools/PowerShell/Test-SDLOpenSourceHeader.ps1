param(
    [Parameter(Mandatory=$true)]
    [ValidateNotNullOrEmpty()]
    [string]$Path,
    [Parameter(Mandatory=$false)]
    [switch]$CSharp=$false,
    [Parameter(Mandatory=$false)]
    [switch]$PowerShell=$false
)
. $PSScriptRoot\Modules\SDLDevTools\Test-SDLOpenSourceHeader.ps1

$targetFiles=@{
    CSharp=@()
    PowerShell=@()
}

if($CSharp)
{
    $targetExtensions=@(
        "*.cs"
    )
    $targetExtensions|ForEach-Object {
        $targetFiles.CSharp+=Get-ChildItem $Path -Filter $_ -Recurse |Select-Object -ExpandProperty FullName
    }
}
if($PowerShell)
{
    $targetExtensions=@(
        "*.ps1"
        "*.psm1"
        "*.psd1"
    )
    
    $targetExtensions|ForEach-Object {
        $targetFiles.PowerShell+=Get-ChildItem $Path -Filter $_ -Recurse |Select-Object -ExpandProperty FullName
    }
}

$report=$targetFiles.CSharp | ForEach-Object {
    Test-SDLOpenSourceHeader -FilePath $_ -Format CSharp -PassThru
}
$report+=$targetFiles.PowerShell | ForEach-Object {
    Test-SDLOpenSourceHeader -FilePath $_ -Format PowerShell -PassThru
}

$filesWithError=$report|Where-Object -Property IsValid -EQ $false
$filesWithError | ForEach-Object {
    Write-Warning "$($_.FilePath) validation failed. $($_.Error)"
}

if($filesWithError.Count -gt 0)
{
    return -1
}
else
{
    return 0
}






