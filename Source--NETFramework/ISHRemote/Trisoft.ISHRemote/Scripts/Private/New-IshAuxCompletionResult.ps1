#
# Hat tip to
# * https://github.com/thinkbeforecoding/PSCompletion
# * http://www.powershellmagazine.com/2012/11/29/using-custom-argument-completers-in-powershell-3-0/
#

function New-IshAuxCompletionResult {
    <#
       .SYNOPSIS
        Creates a new System.Management.Automation.CompletionResult object to return from
        Register-IshAuxParameterCompletion script block.
       .DESCRIPTION
        Creates a new System.Management.Automation.CompletionResult object to return from
        Register-IshAuxParameterCompletion script block.
       .PARAMETER CompletionText
       Specified the text used to set completed parameter value.
       .PARAMETER ListItemText
       Specifies the text to display in the completion list in ISE.
       .PARAMETER Tooltip
        Specifies the text to display in the tooltips of the completion list in ISE.
        The text can be multiline.
       .EXAMPLE
       C:\PS> Register-IshAuxParameterCompleter 'Get-Info' 'Text' {
         param($commandName, $parameterName, $wordToComplete, $commandAst, $fakeBoundParameter)
         New-IshAuxCompletionResult -CompletionText "'$wordToComplete Completed'" -ListItemText 'Text In Completion List' -ToolTip 'Completion List Tooltip'
       }
    #>
    param(
        [Parameter(Mandatory)]
        [string]$CompletionText,
        [string]$ListItemText = $CompletionText,
        [string]$ToolTip = $CompletionText
    )
    
    End {
        if ($CompletionText -match '^[^\''].*\s.*[^\'']$') {
            $CompletionText = "'" + ($CompletionText -replace "'", "''") + "'"
        }
            
        New-Object System.Management.Automation.CompletionResult $CompletionText, $ListItemText, 'ParameterValue', $ToolTip
    }
}