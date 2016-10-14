#Variables
$directory = $args[0];
$modulepath = $args[1];

if ((Test-Path -path "$modulepath") -eq $True)
{
	#Create directory if necessary
	if ((Test-Path -path "$modulepath\ISHRemote") -ne $True)
	{
		New-Item "$modulepath\ISHRemote" -type directory
	}

	#Copy files
	Copy-Item ($directory + "ISHRemote.dll") "$modulepath/ISHRemote" -Force
	#Copy-Item ($directory + "ISHRemote.pdb") "$modulepath/ISHRemote" -Force
	Copy-Item ($directory + "ISHRemote.psm1") "$modulepath/ISHRemote/ISHRemote.psm1" -Force
	Copy-Item ($directory + "ISHRemote.dll-help.xml") "$modulepath/ISHRemote" -Force
	Copy-Item ($directory + "ISHRemote.psd1") "$modulepath/ISHRemote/ISHRemote.psd1" -Force
	Copy-Item ($directory + "ISHRemote.Format.ps1xml") "$modulepath/ISHRemote" -Force
}