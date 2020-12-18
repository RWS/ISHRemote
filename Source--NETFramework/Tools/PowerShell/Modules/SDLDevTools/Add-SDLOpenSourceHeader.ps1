Function Add-SDLOpenSourceHeader {
    param(
        [Parameter(Mandatory=$true,ParameterSetName="Folder")]
        [string]$FolderPath,
        [Parameter(Mandatory=$true,ParameterSetName="File")]
        [string]$FilePath,
        [Parameter(Mandatory=$true,ParameterSetName="File")]
        [ValidateSet("CSharp","PowerShell")]
        [string]$Format,
        [Parameter(Mandatory=$false)]
        [switch]$WhatIf=$false
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
                        Set-SDLOpenSourceHeader -FilePath ($_.FullName) -Format $fileFormat -WhatIf:$WhatIf
                    }
                    else
                    {
                        Write-Verbose "Skipped $($_.FullName)"
                    }
                }
            }
            'File' {
                Write-Debug "Setting header for $FilePath"
                $header = Get-SDLOpenSourceHeader -Format $Format
                $newContent=($header + (Get-Content $FilePath)) 
                if($WhatIf)
                {
                    Write-Host "What if: Performing the operation "Set Content" on target `"Path: $FilePath`"."
                }
                else
                {
                    [System.IO.File]::WriteAllLines($FilePath,$newContent)
            
                    # http://stackoverflow.com/questions/10480673/find-and-replace-in-files-fails
                    #$newContent| Set-Content -FilePath $FilePath -WhatIf:$WhatIf
                }
                Write-Verbose "Set header for $FilePath"            
            }
        }
        

    }

    end {

    }
}