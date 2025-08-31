
Copy-Item -Path "$PSScriptRoot\..\Source\ISHRemote\Trisoft.ISHRemote\Scripts\" -Destination "$PSScriptRoot\..\Source\ISHRemote\Trisoft.ISHRemote\bin\Debug\ISHRemote\" -Force -Recurse
Import-Module ISHRemote  # the one nicely installed, typically from PSGallery
Import-Module "$PSScriptRoot\..\Source\ISHRemote\Trisoft.ISHRemote\bin\Debug\ISHRemote\ISHRemote.psm1" -Force
#Import-Module "$PSScriptRoot\..\Source\ISHRemote\Trisoft.ISHRemote\bin\Debug\ISHRemote\net6.0\Trisoft.ISHRemote.dll" -Force
#Start-McpServer -CmdletsToRegister New-IshSession, Get-IshUser, Get-IshTimeZone, Get-IshTypeFieldDefinition
Start-IshRemoteMcpServer -CmdletsToRegister (Get-Command -Module ISHRemote -ListImported -CommandType Cmdlet).Name `
                         -ActivateWhileLoop $true `
                         -LogFilePath "$PSScriptRoot\..\IshRemoteMcpServer.log"


