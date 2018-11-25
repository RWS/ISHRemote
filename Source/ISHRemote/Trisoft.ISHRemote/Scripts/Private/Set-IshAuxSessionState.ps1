function Set-IshAuxSessionState
{
<#
.SYNOPSIS
	Sets SessionState module variables
.DESCRIPTION
	Sets SessionState module variables
.EXAMPLE
	Set-IshAuxSessionState -Name "ISHRemoteSessionStateIshSession" -Object $ishSession
#>
[CmdletBinding()] 
param(
	$Name,
	$Object
)
Process
{
	$PsCmdlet.SessionState.PSVariable.Set($Name, $Object)
}
}