function Get-IshAuxSessionState
{
<#
.SYNOPSIS
	Returns SessionState module variables
.DESCRIPTION
	Returns SessionState module variables
.EXAMPLE
	Get-IshAuxSessionState -Name "ISHRemoteSessionStateIshSession"
#>
[CmdletBinding()] 
param(
	$Name
)
Process
{
	Write-Output ($PsCmdlet.SessionState.PSVariable.Get($Name)).Value
}
}