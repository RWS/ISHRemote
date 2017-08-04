#
# Tests Header dot-sourced import PS1 file for *.Tests.ps1 files
#
$DebugPreference   = "SilentlyContinue"   # Continue or SilentlyContinue
$VerbosePreference = "SilentlyContinue"   # Continue or SilentlyContinue
$WarningPreference = "Continue"   # Continue or SilentlyContinue or Stop
$ProgressPreference= "SilentlyContinue"   # Continue or SilentlyContinue
Write-Host "Generating and Executing Import-Module Statement..."
$project = (Split-Path -Parent $MyInvocation.MyCommand.Path).ToLower()
$project = $project.Substring(0, $project.LastIndexOf("ishremote"))
$project = ($project + "ishremote")
Import-Module (Join-Path $project "\bin\debug\ishremote.psm1") -DisableNameChecking
Write-Host    "Initializing Global Test Data and Variables"
$timestamp = Get-Date -Format "yyyyMMddHHmmss"
Write-Verbose "Initializing OASIS DITA File Contents"
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
Write-Verbose "Initializing variables for UserName/Password based tests, so ISHSTS-like..."
$baseUrl = 'https://ish.example.com'
$webServicesBaseUrl = "$baseUrl/ISHWS/"  # must have trailing slash for tests to succeed
$wsTrustIssuerUrl = "$baseUrl/ISHSTS/issue/wstrust/mixed/username"
$wsTrustIssuerMexUrl = "$baseUrl/ISHSTS/issue/wstrust/mex"
$ishUserName = 'admin'
$ishPassword = 'admin'
Write-Verbose "Initializing variables for System Setup"
$folderTestRootPath = "\General\__ISHRemote"  # requires leading FolderPathSeparator for tests to succeed
$ishLng = 'VLANGUAGEEN'
$ishResolution = 'VRESLOW'
$ishStatusDraft = 'VSTATUSDRAFT'
$ishUserAuthor = 'VUSERADMIN'
$ishLngCombination = 'en'  # LanguageCombination like 'en+fr+nl' can only be expressed with labels
$ishOutputFormatDitaXml = 'GUID-079A324-FE52-45C4-82CD-A1A9663C2777'  # 'DITA XML' element name
$ishLovId = "DLANGUAGE"  # ListOfValues where the Lov tests will work on


#region Placeholder to inject your variable overrides. DONT FORGET TO DELETE

# e.g.
# $ishUserName = 'myusername'
# $ishPassword = 'mypassword'

#endregion

$webServicesBaseUrl -match "https://((?<hostname>.+))+/(.)+/"
$hostname=$Matches['hostname']
$localWebServicesBaseUrl = $webServicesBaseUrl.Replace($hostname,"localhost")
$localWsTrustIssuerUrl =  $wsTrustIssuerUrl.Replace($hostname,"localhost")
$localWsTrustIssuerMexUrl =  $wsTrustIssuerMexUrl.Replace($hostname,"localhost")

#
# Note
# * Only variables and generic PowerShell object initialization
# * Avoid ISHRemote execution, $ishSession creation will be overwritten in New-IshSession to avoid tests to fail
#
if ($null -eq $global:ishSession)
{
	$global:ishSession = New-IshSession -WsBaseUrl $webServicesBaseUrl -IshUserName $ishUserName -IshPassword $ishPassword
}
$ishSession = $global:ishSession