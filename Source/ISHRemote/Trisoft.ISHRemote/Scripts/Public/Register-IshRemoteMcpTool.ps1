function Register-IshRemoteMcpTool {
    param(
        [Parameter(Mandatory)][AllowEmptyString()]
        [object[]]$FunctionName,
        [Switch]$DoNotCompress
    )

    Write-IshRemoteLog -LogEntry @{ Level = 'Info'; Message = "Register-IshRemoteMcpTool for functions[$($FunctionName -join ', ')]" }
    $results = [ordered]@{}
    foreach ($fn in $FunctionName) {
        
        
        Write-IshRemoteLog -LogEntry @{ Level = 'Verbose'; Message = "Register-IshRemoteMcpTool function[$fn]"; TargetFunction = $fn }
        $CommandInfo = try { Get-Command -Name $fn -ErrorAction Stop } catch { $null }
        if ($null -eq $CommandInfo) {
            Write-IshRemoteLog -LogEntry @{ Level = 'Warn'; Message = "Register-IshRemoteMcpTool function[$fn] not found."; TargetFunction = $fn }
            Write-Warning "Register-IshRemoteMcpTool function[$fn] not found."
            continue
        }
        if ($CommandInfo -is [System.Management.Automation.AliasInfo]) {
            Write-IshRemoteLog -LogEntry @{ Level = 'Verbose'; Message = "Register-IshRemoteMcpTool alias[$fn] to [$($CommandInfo.ResolvedCommand.Name)]"; Alias = $fn; ResolvedName = $CommandInfo.ResolvedCommand.Name }
            $CommandInfo = $CommandInfo.ResolvedCommand
        }

        
        Write-IshRemoteLog -LogEntry @{ Level = 'Verbose'; Message = "Register-IshRemoteMcpTool function[$($CommandInfo.Name)] extended help"; TargetFunction = $CommandInfo.Name }
        $help = Get-Help $CommandInfo.Name -Detailed
        # Prefer Description over Synopsis
        $description = $help.Description | Out-String
        if (-not $description) {
            $description = $help.Synopsis | Out-String
        }
        $description = $description.Trim()
        if (-not $description) {
            Write-IshRemoteLog -LogEntry @{ Level = 'Error'; Message = "Register-IshRemoteMcpTool function[$($CommandInfo.Name)] does not have a description (Synopsis or Description in comment-based help)."; TargetFunction = $CommandInfo.Name }
            Write-Error "Register-IshRemoteMcpTool function[$($CommandInfo.Name)] does not have a description (Synopsis or Description in comment-based help). Aborting." -ErrorAction Stop
            continue
        }
        # Adding all syntax of parameter sets
        $description = $description + "`n`nThis PowerShell cmdlet has the following parameter sets to choose form where square brackets indicate optional parameters while the other parameters are mandatory:`n" + ($help.syntax | Out-String).Trim()

        # Adding all examples
        $description = $description + "`n`nThe PowerShell cmdlet has the following examples as inspiration:`n" + ($help.examples | Out-String).Trim()

        # Adding all parameters
        Write-IshRemoteLog -LogEntry @{ Level = 'Verbose'; Message = "Register-IshRemoteMcpTool function[$($CommandInfo.Name)] all parameters across parameter sets"; TargetFunction = $CommandInfo.Name }
        $Parameters = $CommandInfo.ParameterSets.Parameters |
        Where-Object { $_.Name -notmatch 'Verbose|Debug|ErrorAction|WarningAction|InformationAction|ErrorVariable|WarningVariable|InformationVariable|OutVariable|OutBuffer|PipelineVariable|WhatIf|Confirm|NoHyperLinkConversion|ProgressAction' } | Sort-Object -Property Name -Unique
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
                Write-IshRemoteLog -LogEntry @{ Level = 'Warn'; Message = "Could not get help description for parameter '$($Parameter.Name)' on function '$($CommandInfo.Name)'. Often happens with forced import-module loading of PowerShell module while developing."; TargetFunction = $CommandInfo.Name; ParameterName = $Parameter.Name }
                $paramHelp = ""
            }
            $paramHelp = if ($paramHelp) { $paramHelp.Trim() } else { "No description available." }
            $inputSchema.properties[$Parameter.Name] = [ordered]@{ type = $type; description = $paramHelp }
            # Only $help.syntax indicates if parameters are mandatory within parameter sets. E.g. setting Get-IshFolder -FolderId as required blocks other parameter sets where FolderId is not mandatory.
            #if ($Parameter.IsMandatory) {
            #    $inputSchema.required += $Parameter.Name
            #}
        }


        # Set returns based on spec - use original function name for description
        # Inferring return type is complex in PowerShell, using example's 'number' for Invoke-Addition
        # Defaulting to 'string' otherwise, but this might need refinement based on actual function output types
        $returnType = 'string' # Default
        $returns = [ordered]@{ type = $returnType; description = $CommandInfo.Name }


        Write-IshRemoteLog -LogEntry @{ Level = 'Verbose'; Message = "Register-IshRemoteMcpTool function[$($CommandInfo.Name)] tool annotations"; TargetFunction = $CommandInfo.Name }
        $annotations = [ordered]@{
            type       = 'object'
            destructiveHint = 'true'
            idempotentHint = 'false'
            readOnlyHint = 'false'
        }
        switch -Regex ($CommandInfo.Name) {
            { $_.StartsWith("Find-IshDocumentObj") -or $_.StartsWith("Find-IshPublicationOutput") } { $annotations.destructiveHint = 'true' ; $annotations.idempotentHint = 'true'; $annotations.readOnlyHint = 'true' }  # mostly because these Find can return the full repository
            { $_.StartsWith("Get") -or $_.StartsWith("Test") -or $_.StartsWith("Search") -or $_.StartsWith("Find") -or $_.StartsWith("Compare") } { $annotations.destructiveHint = 'false' ; $annotations.idempotentHint = 'true'; $annotations.readOnlyHint = 'true' }
            { $_.StartsWith("Set") -or $_.StartsWith("New") -or $_.StartsWith("Add") -or $_.StartsWith("Remove") -or $_.StartsWith("Move") -or $_.StartsWith("Stop") -or $_.StartsWith("Publish")} { <#defaults are good#> }
            default { Write-IshRemoteLog -LogEntry @{ Level = 'Warn'; Message = "Unknown verb or cmdlet '$($CommandInfo.Name)', defaulting destructiveHint and idempotentHint to false" } }
        }
        # If verb is Delete, then destructiveHint=$true
        # If verb is Get, then idempotentHint=$true
        # Omit title and openWorldHint to hold defaults
        # if verb is Get, then readOnlyHint=$true



        $results[$CommandInfo.Name] = [ordered]@{ # Keep using CommandInfo.Name as key for internal logic
            name        = $CommandInfo.Name # Use original name for output
            description = $description
            annotations = $annotations
            inputSchema = $inputSchema
            returns     = $returns
        }
        Write-IshRemoteLog -LogEntry @{ Level = 'Info'; Message = "Register-IshRemoteMcpTool function[$fn] done"; TargetFunction = $CommandInfo.Name }
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