function Start-IshRemoteMcpServer {
    param(
        [string]$LogFilePath = "C:\TEMP\IshRemoteMcpServer.log",
        [bool]$ActivateWhileLoop = $true
    )
    $script:logFilePath = $LogFilePath
    #TODO MUST Needs extra parameter to even enable this logging downstream in Write-IshRemoteLog


    # load order of the write ps1??
    #Write-IshRemoteLog -LogEntry @{ Level = 'Info'; Message = "Loading ISHRemote MCP Server script" }


    $listCmdlets = @()
    foreach ($cmdlet in (Get-Command -Module ISHRemote -CommandType Cmdlet | Select-Object -Property Name)) { $listCmdlets += $cmdlet.Name }
    #$listCmdlets = @('New-IshSession', 'Get-IshUser', 'Get-IshTimeZone', 'Get-IshTypeFieldDefinition')

    # Convert the tools list to JSON format
    $toolsListJson = Register-IshRemoteMcpTool -FunctionName $listCmdlets

    #Write-IshRemoteLog -LogEntry @{ Level = 'Info'; Message = "Starting MCP Server" }
    while ($ActivateWhileLoop) {
        $inputLine = [Console]::In.ReadLine()
        if ([string]::IsNullOrEmpty($inputLine)) { continue }
        try {
            $request = $inputLine | ConvertFrom-Json -ErrorAction Stop
            if ($request.id) {
                # Handle the request and get the response
                #Write-IshRemoteLog -LogEntry @{ Level = 'Info'; Message = "Processing request"; RequestId = $request.id; Request = $inputLine }
                $jsonResponse = Invoke-IshRemoteMcpHandleRequest -request $request -toolsListJson $toolsListJson
                [Console]::WriteLine($jsonResponse)
                [Console]::Out.Flush()
            }
        }
        catch {
            # ignore parsing or handler errors
        }
    }
}