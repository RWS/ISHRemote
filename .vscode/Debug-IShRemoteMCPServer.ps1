# Debugging MCP is painful, this script allows a debug mcp.json entry
# Copy the ongoing script PS1 file development to the debug folder, as the pwsh session will launch from the debug folder... and we do not checkin files from the debug folder :)
Copy-Item -Path "$PSScriptRoot\..\Source\ISHRemote\Trisoft.ISHRemote\Scripts\" -Destination "$PSScriptRoot\..\Source\ISHRemote\Trisoft.ISHRemote\bin\Debug\ISHRemote\" -Force -Recurse
# Load the binary ISHRemote module from the module path as just-in-time loading is not reliable
# Import-Module ISHRemote  # the one nicely installed, typically from PSGallery
# Force load the scripts PS1 files to overwrite the standard ones
Import-Module "$PSScriptRoot\..\Source\ISHRemote\Trisoft.ISHRemote\bin\Debug\ISHRemote\ISHRemote.psm1" -Force
#Import-Module "$PSScriptRoot\..\Source\ISHRemote\Trisoft.ISHRemote\bin\Debug\ISHRemote\net6.0\Trisoft.ISHRemote.dll" -Force

<#
Start-IshRemoteMcpServer -CmdletsToRegister Get-Help, New-IshSession, Get-IshUser, Get-IshTimeZone, Get-IshTypeFieldDefinition `
                         -ActivateWhileLoop $true `
                         -LogFilePath "$PSScriptRoot\..\IshRemoteMcpServer.log"
#>
Start-IshRemoteMcpServer -CmdletsToRegister (((Get-Command -Module ISHRemote -ListImported -CommandType Cmdlet).Name) + "Get-Help") `
                         -ActivateWhileLoop $true `
                         -LogFilePath "$PSScriptRoot\..\IshRemoteMcpServer.log"
#>

