# PowerShell Module file in the same folder as the AssemblyName.DLL with the name AssemblyName.PSM1
# This file will add aliasses, including backward compatible entries
# SRC
#   http://stackoverflow.com/questions/13583604/is-there-a-way-to-add-alias-to-powershell-cmdlet-programmatically
#   http://stackoverflow.com/questions/14206595/unable-to-create-a-powershell-alias-in-a-binary-module

Import-Module $PSScriptRoot\ISHRemote.dll

$privateCmdlet  = @(Get-ChildItem -Path $PSScriptRoot\Scripts\Private\*.ps1 -ErrorAction SilentlyContinue -Exclude *.Tests.ps1)
$publicCmdlet  = @(Get-ChildItem -Path $PSScriptRoot\Scripts\Public\*.ps1 -ErrorAction SilentlyContinue -Exclude *.Tests.ps1)
Foreach($import in @($privateCmdlet + $publicCmdlet))
{
    Try
    {
        Write-Debug ("[" + $MyInvocation.MyCommand + "] Loading [" + $import.fullname + "]")
        . $import.fullname
    }
    Catch
    {
        Write-Error -Message "Failed to import function $($import.fullname): $_"
    }
}

