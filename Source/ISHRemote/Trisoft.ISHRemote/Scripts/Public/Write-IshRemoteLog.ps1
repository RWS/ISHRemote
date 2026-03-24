<#
.DESCRIPTION
Simple log function to the logFilePath when set in calling Start-ISHRemoteMcpServer.
#>
function Write-IshRemoteLog {
    param(
        [Parameter(Mandatory = $true)]        
        [object]$LogEntry
    )
    if ($script:logFilePath) {
        # Add a timestamp to the log entry
        $logObject = $LogEntry | Select-Object *, @{Name = 'Timestamp'; Expression = { (Get-Date -Format 'o') } }
        # Convert the object to a JSON string and append to the log file
        $logObject | ConvertTo-Json -Depth 10 -Compress | Add-Content -Path $script:logFilePath
    }
}
