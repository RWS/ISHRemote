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
# 2. Comment/Uncomment the $RepositoryAPIKey and $repositoryName so the
#    repository you want to publish to is the active variable
#    NOTE: Respect these API Keys!! The reason why it is seperated to 
#          ISHRemote-build source control in the first place.
# 3. Download and extract the Github Actions build result called 
#    "ISHRemote-ReleaseV1CI-Module" to folder modulePath[C:\TEMP\ishremote\v1\ISHRemote]
# 4. Once edited, run this script 
#

#region Placeholder to inject your variable overrides. 
Write-Host "Running ISHRemote.SignAndPublish.ps1 Global Test Data and Variables for initialization"
$moduleName="ISHRemote"
$modulePath="C:\TEMP\ishremote\v1\ISHRemote"

$debugFilePath = Join-Path (Split-Path -Parent $MyInvocation.MyCommand.Path) "ISHRemote.SignAndPublish.Debug.ps1"
if (Test-Path -Path $debugFilePath -PathType Leaf)
{
	. ($debugFilePath)
	# Where the .Debug.ps1 should contain at least
	# $RepositoryAPIKey="aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"
    # $repositoryName="MyRegisteredPSRepository"
    # Optionally it could hold FirstUseOnly registration like
    # $sourceName="MyRegisteredPSRepository"
    # $sourceLocation="https://example.com:8081/repository/branch-develop-PowerShell/"
    # $publishLocation="https://example.com:8081/repository/branch-develop-PowerShell/"
    # Unregister-PSRepository -Name $sourceName -ErrorAction SilentlyContinue | Out-Null
    # Register-PSRepository -Name $sourceName -SourceLocation $sourceLocation -PublishLocation  $publishLocation -InstallationPolicy Trusted | Out-Null
}
Write-Host "Using moduleName[$moduleName] modulePath[$modulePath] repositoryName[$repositoryName]"
#endregion





#region publish module
try
{
    Write-Host "Updating PSD1 file by removing 'PreRelease' tag"
    $psd1Path = Join-Path $modulePath "\ISHRemote.psd1"
	Write-Verbose "psd1Path[$psd1Path]"
    $manifest = Get-Content $psd1Path -Raw
    $manifest = $manifest.Replace('        Prerelease = ','        # Prerelease = ')
    $manifest | Out-File -FilePath $psd1Path -Force
    

    if ($repositoryName -eq "PSGallery")
    {
        Write-Host "Updating PSD1 file by adding 'ISH' tag for PSGallery"
	    $psd1Path = Join-Path $modulePath "\ISHRemote.psd1"
	    Write-Verbose "psd1Path[$psd1Path]"
	    # Update-ModuleManifest modifies more than it should in the file, so we don't use the Update-ModuleManifest
	    # Update-ModuleManifest -Path $psd1Path -Tags @("ISH")
	    $manifest = Get-Content $psd1Path -Raw
	    $manifest = $manifest.Replace('        # Tags = @()','        Tags = @("ISH")')
	    $manifest | Out-File -FilePath $psd1Path -Force
    }

    Write-Host "Publishing module[$moduleName] to repository[$repositoryName]"
    if($RepositoryAPIKey)
    {
        Publish-Module -Repository $repositoryName -NuGetApiKey $RepositoryAPIKey -Path "$modulePath"
        Write-Host "Publisheded module"
        Write-Host "Listing module[$moduleName] from repository[$repositoryName] (inc prerelease)"
        Find-Module -Name $moduleName -Repository $repositoryName -AllowPrerelease
        Write-Host "Potential next steps..."
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