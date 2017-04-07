#
# Hat tip to
# * https://github.com/thinkbeforecoding/PSCompletion
# * http://www.powershellmagazine.com/2012/11/29/using-custom-argument-completers-in-powershell-3-0/
#

function Register-IshAuxParameterCompleter {
    <#
       .SYNOPSIS
       Registers a custom argument completer.
       .DESCRIPTION
       Registers a custom argument completer.
       Powershell console will use it to provide tab completion of specified command parameter.
       Powershell ISE will also used provided information to populate a dropdown list and tooltips
       when ctrl+space is pressed.
       .PARAMETER CommandName
       Specifies the name of the function to complete
       .PARAMETER ParameterName
       Specifies the name of the argument to complete
       .PARAMETER ScriptBlock
       Specifies the script to use for completion.
       The script block should take 5 arguments: 
            $commandName : the name of the completed command 
            $parameterName : the name of the completed parameter
            $wordToComplete : the start of the word to complete
            $commandAst : the abstract syntax tree when completion is done using Ast
            $fakeBoundParameter : A hashtable container names/values of other specified parameters
       .EXAMPLE
       C:\PS> Register-IshAuxParameterCompleter 'Get-Info' 'Text' {
       param($commandName, $parameterName, $wordToComplete, $commandAst, $fakeBoundParameter)
       New-IshAuxCompletionResult -CompletionText "'$wordToComplete Completed'" -ListItemText 'Text In Completion List' -ToolTip 'Completion List Tooltip'
       }
    #>
    param(
        [Parameter(Position=0, Mandatory)]
        [string]$CommandName,
        [Parameter(Position=1, Mandatory)]
        [string]$ParameterName,
        [Parameter(Position=2, Mandatory)]
        [scriptblock]$ScriptBlock
        )

    End {
        $global:options['CustomArgumentCompleters']["$($CommandName):$ParameterName"] = $ScriptBlock
    }
}