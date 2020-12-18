Write-Host "Setting preferences..."
$DebugPreference   = "SilentlyContinue"   # Continue or SilentlyContinue
$VerbosePreference = "SilentlyContinue"   # Continue or SilentlyContinue
$WarningPreference = "Continue"   # Continue or SilentlyContinue or Stop
$ProgressPreference= "Continue"   # Continue or SilentlyContinue

Write-Host "Defining function Generate-PublishConfigXml"

# Generates a publish configuration xml that can be used by Publish-IshPublicationOutputs
# If you don't provide a MetadataFilter, it will put all non-released publication outputs in the XML
function Generate-PublishConfigXml 
{
	param
	(
		[parameter(Mandatory=$true)][string]$wsBaseUrl,
		[parameter(Mandatory=$true)][string]$userName,
		[parameter(Mandatory=$true)][string]$password,
		[parameter(Mandatory=$true)][string]$publishConfigFileLocation,
		[parameter(Mandatory=$false)][Trisoft.ISHRemote.Objects.IshField]$publicationOutputsMetadataFilter
	)
	
	try 
	{

		$session = New-IshSession $wsBaseUrl $userName $password 	
		
		# get an XMLTextWriter to create the XML
		$XmlWriter = New-Object System.XMl.XmlTextWriter($publishConfigFileLocation,$Null)
		 
		# choose a pretty formatting:
		$xmlWriter.Formatting = 'Indented'
		$xmlWriter.Indentation = 1
		$XmlWriter.IndentChar = "`t"
		 
		# write the header
		$xmlWriter.WriteStartDocument()

		$xmlWriter.WriteStartElement("publish")
		
		$xmlWriter.WriteElementString("wsBaseUrl", $wsBaseUrl)
		$xmlWriter.WriteElementString("timeout", "7200")
		$xmlWriter.WriteElementString("userName", $userName)
		$xmlWriter.WriteElementString("password", $password)
		$xmlWriter.WriteElementString("outputFolder", "C:\PublishedDocs")
		
		$xmlWriter.WriteStartElement("publications")
		
		$outputFormats = Find-IshOutputFormat -IshSession $session -RequestedMetadata $(Set-IshRequestedMetadataField -IshSession $ishSession -Name "FISHOUTPUTEDT" -Level "None" -ValueType "value")
		 
		$requestedMetadata = Set-IshRequestedMetadataField -IshSession $ishSession -Name "FISHOUTPUTFORMATREF" -Level "Lng" -ValueType "Element" `
						     | Set-IshRequestedMetadataField -IshSession $ishSession -Name "FISHPUBSTATUS" -Level "Lng" -ValueType "Element"		 
		if ($publicationOutputsMetadataFilter -ne $null)
		{
			$puboutputs = Find-IshPublicationOutput -IshSession $session -MetadataFilter $publicationOutputsMetadataFilter  -RequestedMetadata $requestedMetadata `
				| ForEach-Object  {$_}  `
				| Where-Object {$_.IshField.GetFieldValue('FISHPUBSTATUS','Lng',"Element") -ne "VPUBSTATUSRELEASED" }
		}
		else
		{
			$puboutputs = Find-IshPublicationOutput -IshSession $session -RequestedMetadata $requestedMetadata `
				| ForEach-Object  {$_}  `
				| Where-Object {$_.IshField.GetFieldValue('FISHPUBSTATUS','Lng',"Element") -ne "VPUBSTATUSRELEASED" }
		}
		
		foreach($puboutput in $puboutputs)
		{
			$version = $puboutput.IshField.GetFieldValue('VERSION','Version',"value")
			$outputformat = $puboutput.IshField.GetFieldValue('FISHOUTPUTFORMATREF','Lng',"value")
			$outputformatId = $puboutput.IshField.GetFieldValue('FISHOUTPUTFORMATREF','Lng',"Element")					
			$matchingOutputFormats = $outputFormats | Where-Object {$_.IshRef -eq $outputformatId}
			if ($matchingOutputFormats.Length -gt 0)
			{
				$extension = $matchingOutputFormats[0].IshField.GetFieldValue('FISHOUTPUTEDT','None',"value").ToLower()
			}			
			else
			{
				$extension = "unknown"
			}
			$lngCombination = $puboutput.IshField.GetFieldValue('FISHPUBLNGCOMBINATION','Lng',"value")
			$title = $puboutput.IshField.GetFieldValue('FTITLE','Logical',"value")
			
			$xmlWriter.WriteStartElement("publication")
			$xmlWriter.WriteElementString("publicationId", $puboutput.IshRef)
			$xmlWriter.WriteElementString("publicationVersion", $version)
			$xmlWriter.WriteElementString("outputFormat", $outputformatId)
			$xmlWriter.WriteElementString("lngCombination", $lngCombination)
			$filename = $title + "=" + $version + "=" + $lngCombination + "." + $extension
			$xmlWriter.WriteElementString("outputFileName", [RegEx]::Replace($filename, "[{0}]" -f ([RegEx]::Escape(-join [System.IO.Path]::GetInvalidFileNameChars())), ''))
			$xmlWriter.WriteEndElement()
		}

		$xmlWriter.WriteEndElement()
		$xmlWriter.WriteEndElement()
		
		# finalize the document:
		$xmlWriter.WriteEndDocument()
		$xmlWriter.Flush()
		$xmlWriter.Close()
	} 
	catch 
	{
		Write-Error "ERROR in Generate-PublishConfigXml:`n'$_.Exception'." 
	}
}

Write-Host "Defining function Publish-IshPublicationOutputs"

# Publishes the publication outputs in the given configuration file 
# This means it will:
# (1) Start publishing of all publications present in the given configuration file 
# (2) Poll every 50 seconds until they are all finished or a certain timeout is reached
# (3) Cancel all publication outputs which did not finish
# (4) Download all publication outputs to the folder given in the given configuration file 
function Publish-IshPublicationOutputs 
{
	param([parameter(Mandatory=$true)][string]$publishconfigfilelocation)
	
	[xml]$publishConfig = Get-Content $publishconfigfilelocation
	[XML.xmlelement]$publish = $publishConfig.DocumentElement
	[int]$returnValue = 0

	#Сonstants
	[string]$FieldStatus = 'FISHPUBSTATUS'
	[string]$FieldVersion = 'VERSION'
	[string]$FieldOutputFormat = 'FISHOUTPUTFORMATREF'
	[string]$FieldLanguageCombination = 'FISHPUBLNGCOMBINATION'
	[string]$FieldTitle = 'FTITLE'
	
	[string]$LevelLogical = 'Logical'
	[string]$LevelVersion = 'Version'
	[string]$LevelLanguage = 'Lng'
	
	[string]$ValueTypeValue = 'Value'
	[string]$ValueTypeElement = 'Element'
	
	[string]$OperatorEqual = 'Equal'
	
	[int]$InvalidStatusTransitionError = -134
	
	[int]$SleepTime = 50
	
	try 
	{
		if ($publish -ne $null) 
		{
			#Connection info
			[string]$wsBaseUrl = $publish.wsBaseUrl
			if ([String]::IsNullOrEmpty($wsBaseUrl)) { throw "wsBaseUrl is not specified." }
			[int]$timeout = $publish.timeout
			# Credentials
			[string]$trisoftUserName = $publish.userName
			[string]$trisoftPassword = $publish.password	
			[string]$outputFolder = $publish.outputFolder
			if ([String]::IsNullOrEmpty($outputFolder)) { throw "outputFolder is required." }

			Write-Host "Using parameters..."
			Write-Host "WSBaseUrl: $wsBaseUrl"
			Write-Host "OutputFolder: $outputFolder"

			# Create Trisoft session
		    $ishSession = New-IshSession `
				-WsBaseUrl $wsBaseUrl `
				-TrisoftUserName $trisoftUserName `
				-TrisoftPassword $trisoftPassword
			Write-Host "Trisoft session created."

			if ($publish.publications.HasChildNodes) 
			{
				# Publish all publications
				$publishingPublicationInfos = @()
				$publishingPublications = @()
				foreach($publication in $publish.publications.publication)
				{
					try 
					{
						[string]$publicationId = $publication.publicationId
						[string]$publicationVersion = $publication.publicationVersion
						[string]$outputFormat = $publication.outputFormat
						[string]$lngCombination = $publication.lngCombination
						[string]$outputFileName = $publication.outputFileName 
																				
						$metadataFilter = `
							Set-IshMetadataFilterField -IshSession $ishSession -Name $FieldVersion -Level $LevelVersion -ValueType $ValueTypeValue `
								-FilterOperator $OperatorEqual -Value $publicationVersion | `
							Set-IshMetadataFilterField -IshSession $ishSession -Name $FieldOutputFormat -Level $LevelLanguage -ValueType $ValueTypeElement `
								-FilterOperator $OperatorEqual -Value $outputformat | `
							Set-IshMetadataFilterField -IshSession $ishSession -Name $FieldLanguageCombination -Level $LevelLanguage -ValueType $ValueTypeValue `
								-FilterOperator $OperatorEqual -Value $lngCombination

						$publishObject = Get-IshPublicationOutput `
							-IshSession $ishSession `
							-LogicalIds $publicationId `
							-MetadataFilter $metadataFilter
						
						if ($publishObject -eq $null -or $publishObject.Length -eq 0) 
						{
							throw "Publication with Id '{0}', Version '{1}', Output format '{2}', Language '{3}' not found." `
								-f $publicationId, $publicationVersion, $outputformat, $lngCombination 
						}
						if ($publishObject.Length -gt 1) 
						{
							throw "More than 1 publication found with Id '{0}', Version '{1}', Output format '{2}', Language '{3}'" `
								-f $publicationId, $publicationVersion, $outputformat, $lngCombination 
						}
						
						try 
						{
							Write-Host ("{0} Publishing triggered for '{1}'." -f (Get-Date), $outputFileName)
							$publishObject = $publishObject | `
							Publish-IshPublicationOutput -IshSession $ishSession
						} 
						catch 
						{
							if ($_.Exception.Detail.errnumber -ne $InvalidStatusTransitionError) 
							{
								throw
							}
							Write-Warning ("{0} '{1}' was already publishing." -f (Get-Date), $outputFileName)
						}
						$publishingPublications += $publishObject;
						$publishingPublicationInfos += $publishObject | Select-Object `
									@{l='Publication';e={$_}}, `
									@{l='OutputFileName';e={$outputFileName}},
									@{l='PublishingFinished';e={$false}}
					} 
					catch 
					{
						$returnValue = 1
						Write-Host ("{0} Start publishing failed with '{1}'." -f (Get-Date), $_)
					}
				}

				# Poll for completion
				$requestedMetaData = Set-IshRequestedMetadataField -IshSession $ishSession -Name $FieldStatus -Level $LevelLanguage -ValueType $ValueTypeElement `
					| Set-IshRequestedMetadataField -IshSession $ishSession -Name $FieldTitle -Level $LevelLogical -ValueType $ValueTypeValue
				[bool]$publishCompleted = $false
				[int]$timeElapsed = 0

				# Poll until status is not 'published' or similar and timeout is not expired. 
				# If timeout is 0, it never expires.
				while (-not $publishCompleted -and ($timeElapsed -lt $timeout -or $timeout -eq 0)) 
				{		
					Start-Sleep -Seconds $SleepTime
					$timeElapsed = $timeElapsed + $SleepTime					
					$publishCompleted = $true
					try 
					{
						Write-Host ("Retrieving publication status (waited {0}s)." -f $timeElapsed)
						$publicationOutputArray = $publishingPublications | `
							Get-IshPublicationOutput -IshSession $ishSession -RequestedMetaData $requestedMetaData					
							
						foreach ($publishingPublicationInfo in $publishingPublicationInfos) 
						{
							if ($publishingPublicationInfo.PublishingFinished -ne $true)
							{
								$matchingPublicationOutputs = $publicationOutputArray | Where-Object {$_.ObjectRef.Item("Lng") -eq $publishingPublicationInfo.Publication.ObjectRef.Item("Lng")}
								if ($matchingPublicationOutputs.Length -gt 0)
								{
									$output = $matchingPublicationOutputs[0]
									$status = $output.IshField.RetrieveFirst($FieldStatus, $LevelLanguage, $ValueTypeElement)
									# Failed publications can retry so they are considered as running
									if ($status.Value -eq "VPUBSTATUSPUBLISHPENDING" -or $status.Value -eq "VPUBSTATUSPUBLISHING" -or $status.Value -eq "VPUBSTATUSPUBLISHINGFAILED") 
									{
										# Still busy
										$publishCompleted = $false
										$publishingPublicationInfo.Publication = $output
									}
									else
									{
										# Done
										$publishingPublicationInfo.Publication = $output
										$publishingPublicationInfo.PublishingFinished = $true												
										$title = $output.IshField.GetFieldValue($FieldTitle, $LevelLogical, $ValueTypeValue)
										$outputFormat = $output.IshField.GetFieldValue($FieldOutputFormat, $LevelLanguage, $ValueTypeValue)
										Write-Host ("{0} Publishing completed '{1}' '{2}' with status '{3}'." -f (Get-Date), $title, $outputFormat, $status.Value)	
									}
								}
							}
						}
						
						# Uncomment below to debug the current status of the publishes
						# $publishingPublicationInfos
					} 
					catch 
					{
						$returnValue = 1
						# Something failed
						# eg "The underlying connection was closed: A connection that was expected to be kept alive was closed by the server..Exception"
						Write-Error ("{0} Polling failed with {1}." -f (Get-Date), $_)
					}
				}			
				
				# Get the publication output data
				foreach ($publishingPublicationInfo in $publishingPublicationInfos) 
				{
					# Check if the publication failed
					$status = $publishingPublicationInfo.Publication.IshField.RetrieveFirst($FieldStatus, $LevelLanguage, $ValueTypeElement)
					if ($status.Value -eq "VPUBSTATUSPUBLISHINGFAILED") 
					{
						# Publish failed
						$returnValue = 1
						Write-Warning ("{0} Publishing failed for '{1}'." -f (Get-Date), $publishingPublicationInfo.OutputFileName)
					}
					# Check if publication is finished
					ElseIf ($publishingPublicationInfo.PublishingFinished -ne $true) 
					{
						# It is not finished, so cancel the publication
						$returnValue = 1						
						Write-Warning ("{0} Cancelling publish of '{1}' because it takes longer than the specified timeout value." -f (Get-Date), $publishingPublicationInfo.OutputFileName)
						try 
						{
							$publishingPublicationInfo | Select-Object -ExpandProperty Publication | Cancel-IshPublicationOutput -IshSession $ishSession | Out-Null
							Write-Warning ("{0} Cancelled publish of '{1}'." -f (Get-Date), $publishingPublicationInfo.OutputFileName)
						} 
						catch 
						{
							$returnValue = 1
							Write-Warning ("{0} Cancelling publish failed with '{1}'." -f (Get-Date), $_)
						}
					}
					# Download it, even if the current publish timed out, we can still get a previous version
					try 
					{ 
						Write-Host ("{0} Downloading '{1}'." -f (Get-Date), $publishingPublicationInfo.OutputFileName)
						$fileInfoList = $publishingPublicationInfo | 
							Select-Object -ExpandProperty Publication | 
							Get-IshPublicationOutputData -IshSession $ishSession -FolderPath $outputFolder						
						if ($fileInfoList -ne $null) 
						{
							foreach ($fileInfo in $fileInfoList) 
							{
								$oldFileLocation = $fileInfo.FullName															
								$oldFileName = $fileInfo.Name															
								if (-not [String]::IsNullOrEmpty($publishingPublicationInfo.OutputFileName)) 
								{
									$newFileName = $publishingPublicationInfo.OutputFileName
									$newFileLocation = join-path -path $outputFolder -childpath $newFileName
									if ($oldFileName.CompareTo($newFileName) -ne 0)
									{
										If (Test-Path $newFileLocation)
										{
											Remove-Item $newFileLocation
										}
										Rename-Item -Path $oldFileLocation -NewName $newFileName -force
									}
								} 
							}
						}
						Write-Host ("{0} Downloading '{1}' completed." -f (Get-Date), $publishingPublicationInfo.OutputFileName)
					} 
					catch 
					{
						$returnValue = 1
						Write-Warning ("{0} Downloading failed with '{1}'." -f (Get-Date), $_)
					}
				}
			}

		}
	} 
	catch 
	{
		$returnValue = 1
		Write-Error "ERROR in Publish-IshPublicationOutputs:`n'$_.Exception'." 
	}

	return $returnValue
}


Write-Host "Setting current directory..."

[string]$currentScriptDirectory = split-path -parent $MyInvocation.MyCommand.Definition

Set-Location $currentScriptDirectory
[Environment]::CurrentDirectory=$currentScriptDirectory

# This statement can be used to create a publish config file with all non-released publications. 
# You don't need to do this every time, and you can edit the file afterwards before providing it to the Publish-IshPublicationOutputs
# Generate-PublishConfigXml "https://hostname/InfoShareWS/" "username" "password" "publishconfig.sample.xml"

# Publish, wait, and download all configured publications
Publish-IshPublicationOutputs "publishconfig.sample.xml"
