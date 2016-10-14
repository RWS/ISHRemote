# PowerShell Module file in the same folder as the AssemblyName.DLL with the name AssemblyName.PSM1
# This file will add aliasses, including backward compatible entries
# SRC
#   http://stackoverflow.com/questions/13583604/is-there-a-way-to-add-alias-to-powershell-cmdlet-programmatically
#   http://stackoverflow.com/questions/14206595/unable-to-create-a-powershell-alias-in-a-binary-module

Import-Module $PSScriptRoot\ISHRemote.dll