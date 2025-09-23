function Start-IshRemoteMcpServer {
    param(
        [Parameter(Mandatory)]
        [object[]]$CmdletsToRegister,
        [string]$LogFilePath = "C:\TEMP\IshRemoteMcpServer.log",
        [bool]$ActivateWhileLoop = $true
    )
    $script:logFilePath = $LogFilePath

    # TODO [SHOULD] Convert the tools list to JSON format, takes a while, so could be generated in ISHRemote at compile time
    $toolsListJson = Register-IshRemoteMcpTool $CmdletsToRegister

    $resourcesListJson = "" # Register-IshRemoteMcpResource

    # TODO [SHOULD] Convert the instructions to JSON format, from the /Docs/ folder as overall these instructions have values for humans as well :)
    $instructionsJson = Register-IshRemoteMcpInstructions

    Write-IshRemoteLog -LogEntry @{ Level = 'Info'; Message = "Starting MCP Server" }
    while ($ActivateWhileLoop) {
        $inputLine = [Console]::In.ReadLine()
        if ([string]::IsNullOrEmpty($inputLine)) { continue }
        try {
            $request = $inputLine | ConvertFrom-Json -ErrorAction Stop
            if ($request.id) {
                # Handle the request and get the response
                Write-IshRemoteLog -LogEntry @{ Level = 'Info'; Message = "Processing request"; RequestId = $request.id; Request = $inputLine }
                $jsonResponse = Invoke-IshRemoteMcpHandleRequest -Request $request -ToolsListJson $toolsListJson -ResourcesListJson $resourcesListJson -InstructionsJson $instructionsJson
                [Console]::WriteLine($jsonResponse)
                [Console]::Out.Flush()
            }
        }
        catch {
            # ignore parsing or handler errors
        }
    }
}