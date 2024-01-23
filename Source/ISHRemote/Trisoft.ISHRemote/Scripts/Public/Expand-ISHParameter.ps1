#
# Hat tip to
# * https://github.com/thinkbeforecoding/PSCompletion
# * http://www.powershellmagazine.com/2012/11/29/using-custom-argument-completers-in-powershell-3-0/
# * http://www.powertheshell.com/dynamicargumentcompletion/
# * https://learn.microsoft.com/en-us/powershell/module/microsoft.powershell.core/register-argumentcompleter?view=powershell-5.1
#
# Relies on
#   Get-IshAuxSessionState.ps1
#   New-IshAuxCompletionResult.ps1
#   Register-IshAuxParameterCompleter.ps1
#


<#
    Create a global options to pass to [System.Management.Automation.CommandCompletion]::CompleteInput
    containing registered argument completers
#>
if (-not $global:options) {
    $global:options = @{CustomArgumentCompleters = @{};NativeArgumentCompleters = @{}}
}

<#
    Change the orignal TabExpansion2 function used for PS 3.0 completion
    to merge passed $options with $global:options
    The change will happen only once event if executed several times because
    on second pass, the function text doesn't match anymore
#>
$function:tabexpansion2 = $function:tabexpansion2 -replace 'End\r\n{','End { if ($null -ne $options) { $options += $global:options} else {$options = $global:options}'

#
# Thanks to the above boost, finally the added value for this module
#

Write-Debug ("[" + $MyInvocation.MyCommand + "] Parameter binding for *-IshLovValue:LovId")
$starIshLovValueLovId = {
	param($commandName, $parameterName, $wordToComplete, $commandAst, $fakeBoundParameter)
	$ishSession = Get-IshAuxSessionState -Name "ISHRemoteSessionStateIshSession"
	if (($null -ne $fakeBoundParameter['IshSession']) -And ($fakeBoundParameter['IshSession'].GetType().Name -eq 'IshSession') -And ($fakeBoundParameter['IshSession'].IshTypeFieldDefinition.Count -gt 0))
	{
		# Explicit -IshSession takes precedence over SessionState one
		$ishSession = $fakeBoundParameter['IshSession']
	}
	if ( $null -ne $ishSession)
	{
		$ishSession.IshTypeFieldDefinition |
		Where-Object { $_.ReferenceLov -like "$wordToComplete*" } |
		Sort-Object -Unique ReferenceLov |
		ForEach-Object { New-IshAuxCompletionResult -CompletionText $_.ReferenceLov }
	}
}
Register-IshAuxParameterCompleter -CommandName 'Add-IshLovValue' -ParameterName 'LovId' -ScriptBlock $starIshLovValueLovId
Register-IshAuxParameterCompleter -CommandName 'Get-IshLovValue' -ParameterName 'LovId' -ScriptBlock $starIshLovValueLovId
Register-IshAuxParameterCompleter -CommandName 'Remove-IshLovValue' -ParameterName 'LovId' -ScriptBlock $starIshLovValueLovId
Register-IshAuxParameterCompleter -CommandName 'Set-IshLovValue' -ParameterName 'LovId' -ScriptBlock $starIshLovValueLovId


Write-Debug ("[" + $MyInvocation.MyCommand + "] Parameter binding for *-*Field:Name (field names)")
$fieldName = {
	param($commandName, $parameterName, $wordToComplete, $commandAst, $fakeBoundParameter)
	$ishSession = Get-IshAuxSessionState -Name "ISHRemoteSessionStateIshSession"
	if (($null -ne $fakeBoundParameter['IshSession']) -And ($fakeBoundParameter['IshSession'].GetType().Name -eq 'IshSession') -And ($fakeBoundParameter['IshSession'].IshTypeFieldDefinition.Count -gt 0))
	{
		# Explicit -IshSession takes precedence over SessionState one
		$ishSession = $fakeBoundParameter['IshSession']
	}
	if ($ishSession -ne $null)
	{
		# TODO [Could] Expand parameter could take -Level into account to filter even more
		$ishSession.IshTypeFieldDefinition |
		Where-Object { $_.Name -like "$wordToComplete*" } |
		Sort-Object -Unique Name |
		
		# Creation with ToolTips is often too slow, causing Ctrl-Space actions to fall back to file system suggestion
		# ForEach-Object { New-IshAuxCompletionResult -CompletionText $_.Name -ToolTip ($_.Name + " (" + $_.Type + ") - " + $_.Description ) }
		ForEach-Object { New-IshAuxCompletionResult -CompletionText $_.Name }
	}
}
Register-IshAuxParameterCompleter -CommandName 'Get-IshMetadataField' -ParameterName 'Name' -ScriptBlock $fieldName
Register-IshAuxParameterCompleter -CommandName 'Set-IshMetadataField' -ParameterName 'Name' -ScriptBlock $fieldName
Register-IshAuxParameterCompleter -CommandName 'Set-IshMetadataFilterField' -ParameterName 'Name' -ScriptBlock $fieldName
Register-IshAuxParameterCompleter -CommandName 'Set-IshRequestedMetadataField' -ParameterName 'Name' -ScriptBlock $fieldName
Register-IshAuxParameterCompleter -CommandName 'Set-IshRequiredCurrentMetadataField' -ParameterName 'Name' -ScriptBlock $fieldName


Write-Debug ("[" + $MyInvocation.MyCommand + "] Parameter binding for *-*FieldName (field names)")
$fieldName = {
	param($commandName, $parameterName, $wordToComplete, $commandAst, $fakeBoundParameter)
	$ishSession = Get-IshAuxSessionState -Name "ISHRemoteSessionStateIshSession"
	if (($null -ne $fakeBoundParameter['IshSession']) -And ($fakeBoundParameter['IshSession'].GetType().Name -eq 'IshSession') -And ($fakeBoundParameter['IshSession'].IshTypeFieldDefinition.Count -gt 0))
	{
		# Explicit -IshSession takes precedence over SessionState one
		$ishSession = $fakeBoundParameter['IshSession']
	}
	if ($ishSession -ne $null)
	{
		# TODO [Could] Expand parameter could take -Level into account to filter even more
		$ishSession.IshTypeFieldDefinition |
		Where-Object { ($_.ISHType -eq 'ISHConfiguration') -and ($_.Name -like "$wordToComplete*") } |
		Sort-Object -Unique Name |
		
		# Creation with ToolTips is often too slow, causing Ctrl-Space actions to fall back to file system suggestion
		# ForEach-Object { New-IshAuxCompletionResult -CompletionText $_.Name -ToolTip ($_.Name + " (" + $_.Type + ") - " + $_.Description ) }
		ForEach-Object { New-IshAuxCompletionResult -CompletionText $_.Name }
	}
}
Register-IshAuxParameterCompleter -CommandName 'Get-IshSetting' -ParameterName 'FieldName' -ScriptBlock $fieldName
Register-IshAuxParameterCompleter -CommandName 'Set-IshSetting' -ParameterName 'FieldName' -ScriptBlock $fieldName