param(
    [Parameter(Mandatory=$true)]
    [ValidateNotNullOrEmpty()]
    [string]$Path,
    [Parameter(Mandatory=$false)]
    [switch]$CSharp=$false,
    [Parameter(Mandatory=$false)]
    [switch]$PowerShell=$false,
    [Parameter(Mandatory=$false)]
    [switch]$WhatIf=$false
)
. $PSScriptRoot\Modules\SDLDevTools\Add-SDLOpenSourceHeader.ps1

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

$targetFiles.CSharp | ForEach-Object {
    Add-SDLOpenSourceHeader -FilePath $_ -Format CSharp -WhatIf:$WhatIf
}
$targetFiles.PowerShell | ForEach-Object {
    Add-SDLOpenSourceHeader -FilePath $_ -Format PowerShell -WhatIf:$WhatIf
}