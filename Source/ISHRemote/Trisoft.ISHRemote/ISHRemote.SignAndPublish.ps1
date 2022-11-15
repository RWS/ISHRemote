#
# SDL Belgium/ddemeyer
# 20160818
# SUMMARY OF STEPS TO SIGN AND PUBLISH THE MODULE TO A GALLERY
#
# DISCLAIMER
# The content of this document is provided "AS IS" without warranty of any 
# kind, either express, statutory or implied, including, but not limited to 
# warranties regarding non-infringement, merchantability,  fitness for a 
# particular purpose or accuracy of data.  Without limiting the generality of 
# the foregoing, no technical assistance and/or support is provided by SDL. 
#
# STEPS
# 1. [FirstUseOnly] Execute the code of the registerScriptBlock (like 
#    registerNexusScriptBlock) to register Nuget/PSGallery repository (like 
#    internal nexus.sdl.com) from ISHRemote.SignAndPublish.Debug.ps1
# 2. [Remember] ISHRemote is no longer holding a signed assembly since 
#    v1.0 (.SNK no longer required)
# 2. Comment/Uncomment the $PowerShellGalleryAPIKey and $repositoryName so the
#    repository you want to publish to is the active variable
#    NOTE: Respect these API Keys!! The reason why it is seperated to 
#          ISHRemote-build source control in the first place.
# 3. Once edited, run this script from the \ISHRemote-build\ folder in PowerShell
#    PS C:\STASH\ISHRemote-build\__PrivateTools> .\SignAndPublish.ps1
#

#region Placeholder to inject your variable overrides. 
Write-Host "Running ISHRemote.SignAndPublish.ps1 Global Test Data and Variables for initialization"
$debugFilePath = Join-Path (Split-Path -Parent $MyInvocation.MyCommand.Path) "ISHRemote.SignAndPublish.Debug.ps1"
if (Test-Path -Path $debugFilePath -PathType Leaf)
{
	. ($debugFilePath)
	# Where the .Debug.ps1 should contain at least
	# $PowerShellGalleryAPIKey="aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"
    # $repositoryName="MyRegisteredPSRepository"
    # Optionally it could hold FirstUseOnly registration like
    # $sourceName="MyRegisteredPSRepository"
    # $sourceLocation="https://example.com:8081/repository/branch-develop-PowerShell/"
    # $publishLocation="https://example.com:8081/repository/branch-develop-PowerShell/"
    # Unregister-PSRepository -Name $sourceName -ErrorAction SilentlyContinue | Out-Null
    # Register-PSRepository -Name $sourceName -SourceLocation $sourceLocation -PublishLocation  $publishLocation -InstallationPolicy Trusted | Out-Null
}
#endregion



$moduleName="ISHRemote"
$modulePath="C:\TEMP\ishremote\v1\ISHRemote"


#region publish module
try
{
    <#
    Write-Debug "Preparing module folder"
    $modulePath="$releasePath\..\$moduleName"
    if (-not (Test-Path $modulePath))
    {
        New-Item $modulePath -ItemType Directory
    }
    Remove-Item "$modulePath\*" -Recurse -Force
    Copy-Item -Path "$releasePath\*" -Destination $modulePath -Recurse
    #>

    if ($repositoryName -eq "PSGallery")
    {
        try
        {
        Write-Verbose "Add tags to manifest"
	    $psd1Path=Join-Path $modulePath "\ISHRemote.psd1"
	    Write-Debug "psd1Path[$psd1Path]"
	    # Update-ModuleManifest modifies more than it should in the file, so we don't use the Update-ModuleManifest
	    # Update-ModuleManifest -Path $psd1Path -Tags @("ISH")
	    $manifest=Get-Content $psd1Path -Raw
	    $manifest=$manifest.Replace('# Tags = @()','Tags = @("ISH")')
	    $manifest |Out-File -FilePath $psd1Path -Force
	    Write-Verbose "Added tags to $psd1Path"
        }
        finally
        {
        }
    }

    Write-Debug "Publish module"
    if($PowerShellGalleryAPIKey)
    {
        Publish-Module -Repository $repositoryName -NuGetApiKey $PowerShellGalleryAPIKey -Path "$modulePath"
        Write-Verbose "Published module"
        Find-Module -Name $moduleName -Repository $repositoryName -AllowPrerelease
        Write-Host "NEXT: Install-Module -Name $moduleName -Repository $repositoryName -AllowPrerelease"
        Write-Host "NEXT: Uninstall-Module -Name $moduleName -Force"
        Write-Host "NEXT: Get-InstalledModule"
    }
    else
    {
        Publish-Module -Repository $repositoryName -NuGetApiKey "NuGetApiKey" -WhatIf -Path "$modulePath"
        Write-Warning "Skipped publishing the module"
    }
}
finally
{
}
#endregion