Function Test-SDLOpenSourceHeader {
    param(
        [Parameter(Mandatory=$true,ParameterSetName="Folder")]
        [string]$FolderPath,
        [Parameter(Mandatory=$true,ParameterSetName="File")]
        [string]$FilePath,
        [Parameter(Mandatory=$true,ParameterSetName="File")]
        [ValidateSet("CSharp","PowerShell")]
        [string]$Format,
        [Parameter(Mandatory=$false,ParameterSetName="File")]
        [switch]$PassThru=$false
    )
    begin {
        . $PSScriptRoot\Get-SDLOpenSourceHeader.ps1
    }
    process {
        switch($PSCmdlet.ParameterSetName)
        {
            'Folder' {
                Get-ChildItem $FolderPath -Filter "*.*" -Recurse -File | ForEach-Object {
                    if(-not ($_.FullName))
                    {
                        continue
                    }
                    Write-Debug "Calculating format for $($_.FullName)"
                    switch($_.Extension)
                    {
                        '.cs' {
                            $fileFormat="CSharp"
                        }
                        {$_ -in @(".ps1",".psm1",".psd1")} {
                            $fileFormat="PowerShell"
                        }
                        Default {
                            $fileFormat=$null
                        }
                    }
                    if($fileFormat)
                    {
                        Write-Debug "Format is $fileFormat for $($_.FullName)"
                        if(-not (Test-SDLOpenSourceHeader -FilePath ($_.FullName) -Format $fileFormat -WriteError))
                        {
                            $false
                        }
                    }
                    else
                    {
                        Write-Verbose "Skipped $($_.FullName)"
                    }
                    $true
                }
            }
            'File' {
                Write-Debug "Testing header for $FilePath"
                $header = Get-SDLOpenSourceHeader -Format $Format
                $content=Get-Content $FilePath

                $hash=@{
                    FilePath=$FilePath
                    Format=$Format
                    Error=$null
                }
                if($content.Count -lt $header.Count)
                {
                    $hash.Error="Line count less than header"
                }
                else
                {
                    for($i=0;$i -lt $header.Count;$i++)
                    {
                        if($header[$i] -ne $content[$i])
                        {
                            $hash.Error="Mismatch on line $($i+1)"
                            break
                        }
                    }
                }
                $hash.IsValid=$hash.Error -eq $null
                Write-Verbose "Tested header for $FilePath"
                if($PassThru)
                {
                    New-Object -TypeName PSObject -Property $hash
                }
                else
                {
                    if(-not $hash.IsValid)
                    {
                        Write-Error "Failed for $FilePath because of `"$($hash.Error)`"."
                    }
                    $hash.IsValid
                }
            }
        }
    }

    end {

    }
}