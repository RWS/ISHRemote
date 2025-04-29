#
# Tests Header dot-sourced import PS1 file for *.Tests.ps1 files
#

# Quick check if you are using Pester 5, or default installed Pester 3.4.0 (#132)
$pesterVersion = [version](Get-Command Invoke-Pester).Version
if ($pesterVersion -lt [version]("5.3.0")) { Write-Warning ("ISHRemote.PesterSetup.ps1 Invoke-Pester version["+$pesterVersion+"] while 5.3+ is expected!") }

$DebugPreference   = "SilentlyContinue"   # Continue or SilentlyContinue
$VerbosePreference = "SilentlyContinue"   # Continue or SilentlyContinue
$WarningPreference = "Continue"   # Continue or SilentlyContinue or Stop
$ProgressPreference= "SilentlyContinue"   # Continue or SilentlyContinue

# Generating and Executing Import-Module Statement
$project = (Split-Path -Parent $MyInvocation.MyCommand.Path).ToLower()
$project = $project.Substring(0, $project.LastIndexOf("ishremote"))
$project = ($project + "ishremote")
if ($env:GITHUB_ACTIONS -eq "true") {
	$moduleFolder = (Join-Path $project "\bin\release\ISHRemote")
}
else {
	$moduleFolder = (Join-Path $project "\bin\debug\ISHRemote")
}
Write-Host ("Running ISHRemote.PesterSetup.ps1 Import Module folder["+$moduleFolder+"] ...")
Import-Module ($moduleFolder) -DisableNameChecking

Write-Host ("Running ISHRemote.PesterSetup.ps1 Global Test Data and Variables initialization on "+(Get-Date -UFormat "%Y-%m-%dT%H-%M-%S%Z"))
$timestamp = Get-Date -Format "yyyyMMddHHmmss"
Write-Verbose "Running ISHRemote.PesterSetup.ps1 OASIS DITA File Contents initialization"
$ditaTopicFileContent = @"
<?xml version="1.0" ?>
<!DOCTYPE topic PUBLIC "-//OASIS//DTD DITA Topic//EN" "topic.dtd">
<topic><title>Enter the title of your topic here.<?ish-replace-title?></title><shortdesc>Enter a short description of your topic here (optional).</shortdesc><body><p>This is the start of your topic.</p><ul><li>List item without condition</li><li ishcondition="ISHRemoteStringCond='StringOne'">ISHRemoteStringCond condition</li><li ishcondition="ISHRemoteVersionCond='12.0.1'">ISHRemoteVersionCond condition</li></ul></body></topic>
"@
$ditaMapFileContent = @"
<?xml version="1.0" ?>
<!DOCTYPE map PUBLIC "-//OASIS//DTD DITA Map//EN" "map.dtd">
<map><title>Enter the title of your map here.<?ish-replace-title?></title></map>
"@
$ditaMapWithTopicrefFileContent = @"
<?xml version="1.0" ?>
<!DOCTYPE map PUBLIC "-//OASIS//DTD DITA Map//EN" "map.dtd">
<map><title>Enter the title of your map here.<?ish-replace-title?></title>
<topicref href="<GUID-PLACEHOLDER>"><topicmeta></topicmeta></topicref>
</map>
"@

Write-Verbose "Running ISHRemote.PesterSetup.ps1 variables for UserName/Password/Client/Secret based tests...initialization"
$baseUrl = $env:ISH_BASE_URL
if ([string]::IsNullOrEmpty($baseUrl))
{
	$baseUrl = 'https://ish.example.com'
}
$ishUserName = $env:ISH_USER_NAME
if ([string]::IsNullOrEmpty($ishUserName))
{
	$ishUserName = 'myusername'
}
$ishPassword = $env:ISH_PASSWORD
if ([string]::IsNullOrEmpty($ishPassword))
{
	$ishPassword = 'mypassword'
}
$amClientId = $env:ISH_CLIENT_ID
if ([string]::IsNullOrEmpty($amClientId))
{
	$amClientId = 'myserviceaccountclientid'
}
$amClientSecret = $env:ISH_CLIENT_SECRET
if ([string]::IsNullOrEmpty($amClientSecret))
{
	$amClientSecret = 'myserviceaccountclientsecret'
}

$webServicesBaseUrl = "$baseUrl/ISHWS/"  # must have trailing slash for tests to succeed
#$wsTrustIssuerUrl = "$baseUrl/ISHSTS/issue/wstrust/mixed/username"  # Removed since v7.0
#$wsTrustIssuerMexUrl = "$baseUrl/ISHSTS/issue/wstrust/mex"  # Removed since v7.0

Write-Verbose "Running ISHRemote.PesterSetup.ps1 variables for System Setup initialization"
$folderTestRootPath = "\General\__ISHRemote"  # requires leading FolderPathSeparator for tests to succeed
$ishLng = 'VLANGUAGEEN'
$ishLngLabel = 'en'
$ishLngTarget1 = 'VLANGUAGEES'
$ishLngTarget1Label = 'es'
$ishLngTarget2 = 'VLANGUAGEDE'
$ishLngTarget2Label = 'de'
$ishResolution = 'VRESLOW'
$ishStatusDraft = 'VSTATUSDRAFT'
$ishStatusReleased = 'VSTATUSRELEASED'  # Direct status transition from $ishStatusDraft (D) to $ishStatusReleased (R) is required by the executing user
$ishUserAuthor = 'VUSERADMIN'
$ishLngCombination = 'en'  # LanguageCombination like 'en+fr+nl' can only be expressed with labels
$ishOutputFormatDitaXml = 'GUID-079A324-FE52-45C4-82CD-A1A9663C2777'  # 'DITA XML' element name
$ishLovId = "DLANGUAGE"  # ListOfValues where the Lov tests will work on
$ishLovId2 = "DRESOLUTION"  # ListOfValues where the Lov tests will work on
$ishEventTypeToPurge = "PUSHTRANSLATIONS"

#region Placeholder to inject your variable overrides. 
Write-Host "Running ISHRemote.PesterSetup.ps1 Global Test Data and Variables for debug initialization"
$debugPesterSetupFilePath = Join-Path (Split-Path -Parent $MyInvocation.MyCommand.Path) "ISHRemote.PesterSetup.Debug.ps1"
if (Test-Path -Path $debugPesterSetupFilePath -PathType Leaf)
{
	. ($debugPesterSetupFilePath)
	# Where the .Debug.ps1 should contain at least
	# $baseUrl = 'https://ish.example.com'
	# $webServicesBaseUrl = "$baseUrl/ISHWS/"
	# $wsTrustIssuerUrl = "$baseUrl/ISHSTS/issue/wstrust/mixed/username"
	# $wsTrustIssuerMexUrl = "$baseUrl/ISHSTS/issue/wstrust/mex"
	# $ishUserName = 'myusername'
	# $ishPassword = 'mypassword'
	# $amClientId = 'myserviceaccountclientid'
	# $amClientSecret = 'myserviceaccountclientsecret'
}
#endregion

$webServicesBaseUrl -match "https://((?<hostname>.+))+/(.)+/" | Out-Null
$hostname=$Matches['hostname']

#
# Note
# * Only variables and generic PowerShell object initialization
# * Avoid ISHRemote execution, $ishSession creation will be overwritten in New-IshSession to avoid tests to fail
#
#if ($null -eq $global:ishSession)
#{
	$webServicesConnectionConfigurationUrl = $webServicesBaseUrl + "connectionconfiguration.xml"
	Write-Host "Running ISHRemote.PesterSetup.ps1 Detect version over webServicesConnectionConfigurationUrl[$webServicesConnectionConfigurationUrl] webServicesConnectionConfigurationUrl.Length[$($webServicesConnectionConfigurationUrl.Length)]"
	$connectionConfigurationRaw = Invoke-RestMethod -Uri $webServicesConnectionConfigurationUrl #Only PS7#-SkipCertificateCheck 
	$connectionConfigurationRaw -match "<infosharesoftwareversion>(?<myVersion>.*)</infosharesoftwareversion>"  # Straight string handling avoids UTF8-BOM cross-platform handling
	[version]$infosharesoftwareversion = $matches['myversion']
	if ($infosharesoftwareversion.Major -lt 15) # 14SP4 and earlier, initialize ONE session over -IshUserName/-IshPassword
	{
		$global:ishSession = New-IshSession -Protocol WcfSoapWithWsTrust -WsBaseUrl $webServicesBaseUrl -IshUserName $ishUserName -IshPassword $ishPassword -WarningAction SilentlyContinue
	}
	else # 15 and later, initialize ONE session over -ClientId/-ClientSecret
	{
		$global:ishSession = New-IshSession -Protocol WcfSoapWithOpenIdConnect -WsBaseUrl $webServicesBaseUrl -ClientId $amClientId -ClientSecret $amClientSecret -WarningAction SilentlyContinue
	}
#}
$ishSession = $global:ishSession
# TODO [Must] The StateStore is now required for all tests, but it is only done in New-IshSession. 50s performance boost to gain 
