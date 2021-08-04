# PowerShell Module file in the same folder as the AssemblyName.DLL with the name AssemblyName.PSM1
# This file will add aliasses, including backward compatible entries
# SRC
#   http://stackoverflow.com/questions/13583604/is-there-a-way-to-add-alias-to-powershell-cmdlet-programmatically
#   http://stackoverflow.com/questions/14206595/unable-to-create-a-powershell-alias-in-a-binary-module

# Import-Module $PSScriptRoot\Trisoft.ISHRemote.dll

# $privateCmdlet  = @(Get-ChildItem -Path $PSScriptRoot\Scripts\Private\*.ps1 -ErrorAction SilentlyContinue -Exclude *.Tests.ps1)
# $publicCmdlet  = @(Get-ChildItem -Path $PSScriptRoot\Scripts\Public\*.ps1 -ErrorAction SilentlyContinue -Exclude *.Tests.ps1)
# Foreach($import in @($privateCmdlet + $publicCmdlet))
# {
#     Try
#     {
#         Write-Debug ("[" + $MyInvocation.MyCommand + "] Loading [" + $import.fullname + "]")
#         . $import.fullname
#     }
#     Catch
#     {
#         Write-Error -Message "Failed to import function $($import.fullname): $_"
#     }
# }

#
# Script module for module 'ISHRemote'
#
Set-StrictMode -Version Latest

# Set up some helper variables to make it easier to work with the module
$PSModule = $ExecutionContext.SessionState.Module
$PSModuleRoot = $PSModule.ModuleBase

# Import the appropriate nested binary module based on the current PowerShell version
$binaryModuleRoot = $PSModuleRoot


if (($PSVersionTable.Keys -contains "PSEdition") -and ($PSVersionTable.PSEdition -eq 'Desktop')) {
    $binaryModuleRoot = Join-Path -Path $PSModuleRoot -ChildPath 'net472'
}
else
{
    if ($PSVersionTable.PSVersion -gt [Version]'7.1')
    {
        $binaryModuleRoot = Join-Path -Path $PSModuleRoot -ChildPath 'netcoreapp3.1'
    }
}

$binaryModulePath = Join-Path -Path $binaryModuleRoot -ChildPath 'Trisoft.ISHRemote.dll'
$binaryModule = Import-Module -Name $binaryModulePath -PassThru

# When the module is unloaded, remove the nested binary module that was loaded with it
$PSModule.OnRemove = {
    Remove-Module -ModuleInfo $binaryModule
}