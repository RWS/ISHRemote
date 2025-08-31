function Register-IshRemoteMcpTool {
    param(
        [Parameter(Mandatory)][AllowEmptyString()]
        [object[]]$FunctionName,
        [int]$ParameterSet = 0,
        [Switch]$DoNotCompress
    )

    Write-IshRemoteLog -LogEntry @{ Level = 'Info'; Message = "Starting Register-IshRemoteMcpTool for functions: $($FunctionName -join ', ')" }
    $results = [ordered]@{}
    foreach ($fn in $FunctionName) {
        Write-IshRemoteLog -LogEntry @{ Level = 'Verbose'; Message = "Processing function: $fn"; TargetFunction = $fn }
        $CommandInfo = try { Get-Command -Name $fn -ErrorAction Stop } catch { $null }
        if ($null -eq $CommandInfo) {
            Write-IshRemoteLog -LogEntry @{ Level = 'Warn'; Message = "Function '$fn' not found."; TargetFunction = $fn }
            Write-Warning "$fn not found!"
            continue
        }
        if ($CommandInfo -is [System.Management.Automation.AliasInfo]) {
            Write-IshRemoteLog -LogEntry @{ Level = 'Verbose'; Message = "Resolving alias '$fn' to '$($CommandInfo.ResolvedCommand.Name)'"; Alias = $fn; ResolvedName = $CommandInfo.ResolvedCommand.Name }
            $CommandInfo = $CommandInfo.ResolvedCommand
        }
        if ($CommandInfo.ParameterSets.Count -lt $ParameterSet + 1) {
            Write-IshRemoteLog -LogEntry @{ Level = 'Error'; Message = "ParameterSet $ParameterSet does not exist for function '$fn'."; TargetFunction = $fn; ParameterSetIndex = $ParameterSet }
            Write-Error "ParameterSet $ParameterSet does not exist for $fn."
            continue
        }
        
        <# REPLACED
        Write-IshRemoteLog -LogEntry @{ Level = 'Verbose'; Message = "Getting help for function '$($CommandInfo.Name)'"; TargetFunction = $CommandInfo.Name }
        $help = Get-Help $CommandInfo.Name
        $description = $help.Synopsis | Out-String
        if (-not $description) {
            $description = $help.Description.Text | Out-String
        }
        $description = $description.Trim()
        if (-not $description) {
            Write-IshRemoteLog -LogEntry @{ Level = 'Error'; Message = "Function '$($CommandInfo.Name)' does not have a description (Synopsis or Description in comment-based help)."; TargetFunction = $CommandInfo.Name }
            Write-Error "Function '$($CommandInfo.Name)' does not have a description (Synopsis or Description in comment-based help). Aborting." -ErrorAction Stop
            continue
        } #>
        Write-IshRemoteLog -LogEntry @{ Level = 'Verbose'; Message = "Getting EXTENDED help for function '$($CommandInfo.Name)'"; TargetFunction = $CommandInfo.Name }

        $help = Get-Help $CommandInfo.Name -Detailed
        # Prefer Description over Synopsis
        $description = $help.Description | Out-String
        if (-not $description) {
            $description = $help.Synopsis | Out-String
        }
        $description = $description.Trim()
        if (-not $description) {
            Write-IshRemoteLog -LogEntry @{ Level = 'Error'; Message = "Function '$($CommandInfo.Name)' does not have a description (Synopsis or Description in comment-based help)."; TargetFunction = $CommandInfo.Name }
            Write-Error "Function '$($CommandInfo.Name)' does not have a description (Synopsis or Description in comment-based help). Aborting." -ErrorAction Stop
            continue
        }
        # Adding all syntax of parameter sets
        $description = $description + "`n`nThis PowerShell cmdlet has the following parameter sets to choose form:`n" + ($help.syntax | Out-String).Trim()
        # Adding all parameters
        $description = $description + "`n`nThe PowerShell cmdlet uses the following parameters in the parameter sets:`n" + ($help.parameters | Out-String).Trim()
        # Adding all examples
        $description = $description + "`n`nThe PowerShell cmdlet has the following examples as inspiration:`n" + ($help.examples | Out-String).Trim()

        Write-IshRemoteLog -LogEntry @{ Level = 'Verbose'; Message = "Processing parameters for function '$($CommandInfo.Name)'"; TargetFunction = $CommandInfo.Name }
        $Parameters = $CommandInfo.ParameterSets[$ParameterSet].Parameters |
        Where-Object { $_.Name -notmatch 'Verbose|Debug|ErrorAction|WarningAction|InformationAction|ErrorVariable|WarningVariable|InformationVariable|OutVariable|OutBuffer|PipelineVariable|WhatIf|Confirm|NoHyperLinkConversion|ProgressAction' }

        $inputSchema = [ordered]@{
            type       = 'object'
            properties = [ordered]@{}
            required   = @()
        }
        foreach ($Parameter in $Parameters) {
            $typeName = $Parameter.ParameterType.Name.ToLower()
            switch -Regex ($typeName) {
                'string' { $type = 'string' }
                'int|int32|int64|double' { $type = 'number' }
                'boolean' { $type = 'boolean' }
                'switchparameter' { $type = 'boolean' }
                default { $type = 'string' }
            }
            try {
                $paramHelp = (Get-Help $CommandInfo.Name -Parameter $Parameter.Name -ErrorAction Stop).Description | Out-String
            }
            catch {
                Write-IshRemoteLog -LogEntry @{ Level = 'Warn'; Message = "Could not get help for parameter '$($Parameter.Name)' on function '$($CommandInfo.Name)'."; TargetFunction = $CommandInfo.Name; ParameterName = $Parameter.Name }
                $paramHelp = $null
            }
            $paramHelp = if ($paramHelp) { $paramHelp.Trim() } else { "No description available for this parameter." }
            $inputSchema.properties[$Parameter.Name] = [ordered]@{ type = $type; description = $paramHelp }
            if ($Parameter.IsMandatory) {
                $inputSchema.required += $Parameter.Name
            }
        }

        # Set returns based on spec - use original function name for description
        # Inferring return type is complex in PowerShell, using example's 'number' for Invoke-Addition
        # Defaulting to 'string' otherwise, but this might need refinement based on actual function output types
        $returnType = 'string' # Default

        $returns = [ordered]@{ type = $returnType; description = $CommandInfo.Name }

        <# REPLACED
        $results[$CommandInfo.Name] = [ordered]@{ # Keep using CommandInfo.Name as key for internal logic
            name        = $CommandInfo.Name # Use original name for output
            description = $CommandInfo.Name # Use original name for output description
            inputSchema = $inputSchema
            returns     = $returns
        } #>
            $results[$CommandInfo.Name] = [ordered]@{ # Keep using CommandInfo.Name as key for internal logic
            name        = $CommandInfo.Name # Use original name for output
            description = $description
            inputSchema = $inputSchema
            returns     = $returns
        }
        Write-IshRemoteLog -LogEntry @{ Level = 'Info'; Message = "Successfully processed function '$($CommandInfo.Name)'."; TargetFunction = $CommandInfo.Name }
    }

    # Output an array of tool objects, ensuring it's an array even for a single function
    [array]$outputArray = @($results.Values)

    Write-IshRemoteLog -LogEntry @{ Level = 'Info'; Message = "Generating JSON output."; FunctionCount = $outputArray.Count }
    if ($DoNotCompress) {
        $outputArray | ConvertTo-Json -Depth 8
    }
    else {
        $outputArray | ConvertTo-Json -Depth 8 -Compress:$true
    }
    Write-IshRemoteLog -LogEntry @{ Level = 'Info'; Message = "Finished Register-IshRemoteMcpTool." }
}