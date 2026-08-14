<#
.DESCRIPTION
Starts an MCP server that exposes ISHRemote cmdlets as tools that can be called by MCP clients.
This client-side MCP server is a hidden PowerShell (pwsh) session that holds below while loop to answer
the LLM questions. These calls - after New-IshSession invoke in this hidden PowerShell process - could be 
direct queries to your chosen system or it can assist in writing scripts as every Mcp Tool is in essence 
an ISHRemote cmdlet. The server listens for JSON-RPC requests on standard input and writes responses 
to standard output.

.PARAMETER CmdletsToRegister
Array of cmdlet names to register for partial load so with Get-Help without syntax and examples.

.PARAMETER CmdletsToRegisterFullLoad
Array of cmdlet names to register for full load so with Get-Help -Detailed holding syntax and examples.
When set explicitly, best to add @('Get-Help','New-IshSession').

.PARAMETER LogFilePath
Path to the log file where server activity will be recorded. Empty string or $null means logging is off. 
Example value could be "C:\TEMP\IshRemoteMcpServer.log".

.PARAMETER ActivateWhileLoop
Controls whether the server runs in a continuous loop. Set to $false for testing purposes. Defaults to $true.

.EXAMPLE
Start-IshRemoteMcpServer -CmdletsToRegister ((Get-Command -Module ISHRemote -ListImported -CommandType Cmdlet).Name)
Starts the MCP server and registers the specified ISHRemote cmdlets as available tools. No log file.

.EXAMPLE
Start-IshRemoteMcpServer -CmdletsToRegisterFullLoad ((Get-Command -Module ISHRemote -ListImported -CommandType Cmdlet).Name) -LogFilePath "$env:TEMP\IshRemoteMcpServer.log"
Starts the MCP server and registers the specified ISHRemote cmdlets as available tools using all information in Get-Help -Detailed including syntax and examples. Plus a log file is started.

#>
function Start-IshRemoteMcpServer {
    param(
        [Parameter(Mandatory)]
        [object[]]$CmdletsToRegister,
        [object[]]$CmdletsToRegisterFullLoad = $null,
        [string]$LogFilePath = $null,
        [bool]$ActivateWhileLoop = $true
    )
    $script:logFilePath = $LogFilePath

    if ($CmdletsToRegisterFullLoad) {
        # using incoming values explicitly
    } else {
        # else making sure these cmdlets get a full load
        $CmdletsToRegisterFullLoad = @('Get-Help','New-IshSession')
    }
    $toolsListJson = Register-IshRemoteMcpTool -FunctionNameFullLoad $CmdletsToRegisterFullLoad -FunctionNamePartialLoad $CmdletsToRegister

    $resourcesListJson = "" # Register-IshRemoteMcpResource

    # TODO [SHOULD] Convert the instructions to JSON format, from the /Docs/ folder as overall these instructions have values for humans as well :)
    $instructionsJson = Register-IshRemoteMcpInstructions

    # Ensure UTF-8 encoding on stdin/stdout and enable AutoFlush so responses are
    # immediately visible to the MCP client. On Windows the default is SBCSCodePageEncoding
    # which causes the client's UTF-8 JSON to be misread, blocking ReadLine() indefinitely.
    # Console.Out is wrapped in SyncTextWriter so AutoFlush cannot be set directly;
    # replace it with a StreamWriter over the underlying stdout stream with AutoFlush=true.
    $utf8NoBom = [System.Text.UTF8Encoding]::new($false)
    [Console]::InputEncoding  = $utf8NoBom
    [Console]::OutputEncoding = $utf8NoBom
    $stdoutWriter = [System.IO.StreamWriter]::new([Console]::OpenStandardOutput(), $utf8NoBom)
    $stdoutWriter.AutoFlush  = $true
    [Console]::SetOut($stdoutWriter)
    Write-IshRemoteLog -LogEntry @{ Level = 'Info'; Message = "Starting MCP Server"; InputEncoding = [Console]::InputEncoding.EncodingName; OutputEncoding = [Console]::OutputEncoding.EncodingName }
    while ($ActivateWhileLoop) {
        $inputLine = [Console]::In.ReadLine()
        if ($null -eq $inputLine) {
            # EOF: client closed the input stream — exit cleanly per MCP stdio transport spec
            Write-IshRemoteLog -LogEntry @{ Level = 'Info'; Message = "stdin closed (EOF), exiting MCP Server" }
            break
        }
        if ([string]::IsNullOrEmpty($inputLine)) { continue }
        try {
            $request = $inputLine | ConvertFrom-Json -ErrorAction Stop
            if ($null -ne $request.id) {
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