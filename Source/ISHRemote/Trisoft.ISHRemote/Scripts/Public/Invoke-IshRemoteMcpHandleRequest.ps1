function Invoke-IshRemoteMcpHandleRequest {
    param(
        [object]$Request,
        [string]$ToolsListJson,
        [string]$ResourcesListJson,
        [string]$InstructionsJson
    )

    # Initialize Method
    if ($Request.method -eq "initialize") {
        # Static response for simplicity, adjust serverInfo as needed
        $response = '{"jsonrpc":"2.0","id":' + ($Request.id | ConvertTo-Json -Depth 10 -Compress) + ',"result":{"protocolVersion":"0.3.0","capabilities":{"tools":{"listChanged":false}},"instructions":' + $instructionsJson + ',"serverInfo":{"name":"PowerShell MCP Server (Template)","version":"0.1.0"}}}'
        return $response
    }

    # Ping Method
    if ($Request.method -eq "ping") {
        $pingResponse = @{
            jsonrpc = "2.0"
            id      = $Request.id
            result  = @{}
        }

        $response = $pingResponse | ConvertTo-Json -Depth 10 -Compress
        return $response
    }

    # Tools/List Method
    if ($Request.method -eq "tools/list") {

        $response = '{"jsonrpc":"2.0","id":' + ($Request.id | ConvertTo-Json -Depth 10 -Compress) + ',"result":{"tools":' + $ToolsListJson + '}}'

        # Use the Write-IshRemoteLog function correctly with a hashtable
        Write-IshRemoteLog -LogEntry @{
            RequestId   = $Request.id
            Method      = $Request.method
            FullRequest = ($Request | ConvertTo-Json -Depth 10 -Compress) # Keep as string if preferred, or parse if needed elsewhere
            ToolsList   = $ToolsListJson # Keep as string
        }

        return $response
    }

    # Tools/Call Method
    if ($Request.method -eq "tools/call") {
        $toolName = $Request.params.name
        $targetArgs = $Request.params.arguments | ConvertTo-Json -Depth 10 | ConvertFrom-Json -Depth 10 -AsHashtable
        $result = $null
        try {
            $result = & $toolName @targetArgs
        }
        catch {
            Write-IshRemoteLog -LogEntry @{
                Level      = 'Error'
                Message    = "Error invoking tool [$toolName]: $_"
                RequestId  = $Request.id
                Method     = $Request.method
                ToolName   = $toolName
                Arguments  = $targetArgs
            }
        }

        # Log structured data
        Write-IshRemoteLog -LogEntry @{
            RequestId = $Request.id
            Method    = $Request.method
            ToolName  = $toolName
            Arguments = $targetArgs
            Result    = $result
            # FullRequest = $Request # Optionally include the full Request if needed, can be large
        }

        # TODO if $result holds a cmdlet error like 'Find-IshUser: [-102001]...' or PowerShell errors like 'ParseError'
        # then add 'see docs://tools for more information' to instruct the LLM to download this McpResource for proper usage

        $response = [ordered]@{
            jsonrpc = "2.0"
            id      = $Request.id
            result  = @{
                content = @(
                    [ordered]@{
                        type = "text"
                        text = $result | Out-String
                    }
                )
                isError = $false
            }
        }

        return ($response | ConvertTo-Json -Depth 10 -Compress)
    }

    # Unknown Method Error
    $response = '{"jsonrpc":"2.0","id":' + ($Request.id | ConvertTo-Json -Depth 10 -Compress) + ',"error":{"code":-32601,"message":"Method not found"}}'

    return $response
}
