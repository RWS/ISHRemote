<#
.SYNOPSIS
Returns the ISHRemote MCP server instructions as a compressed JSON string.
.DESCRIPTION
Reads Doc/McpInstructions-ISHRemote.md, which is copied next to this script during the
build, and returns the full Markdown content as a compressed JSON string for consumption
by the MCP server protocol. The source file in Doc/ is the single source of truth for
Tridion Docs domain knowledge shared between humans, agents, and the MCP runtime.
#>
function Register-IshRemoteMcpInstructions {
    $instructionsFilePath = Join-Path $PSScriptRoot 'McpInstructions-ISHRemote.md'
    $instructions = Get-Content -Path $instructionsFilePath -Raw
    Write-Output ($instructions | ConvertTo-Json -Compress)
}