$ishSession = 
# Fixed data set
$metadataFilter = Set-IshMetadataFilterField -Level Lng -Name MODIFIED-ON -FilterOperator GreaterThanOrEqual -Value "01/01/2016" |
                  Set-IshMetadataFilterField -Level Lng -Name MODIFIED-ON -FilterOperator LessThan -Value "01/01/2017" 

$ishSession.DefaultRequestedMetadata = "Descriptive"
$ishSession.PipelineObjectPreference = "PSObjectNoteProperty"
$totalTime = (Measure-Command { $o = Find-IshDocumentObj -MetadataFilter $metadataFilter }).TotalMilliseconds
Write-Host ($ishSession.PipelineObjectPreference.ToString() + " " + $ishSession.DefaultRequestedMetadata.ToString() + ": " + $totalTime)
$ishSession.PipelineObjectPreference = "Off"
$totalTime = (Measure-Command { $o = Find-IshDocumentObj -MetadataFilter $metadataFilter }).TotalMilliseconds
Write-Host ($ishSession.PipelineObjectPreference.ToString() + " " + $ishSession.DefaultRequestedMetadata.ToString() + ": " + $totalTime)


$ishSession.DefaultRequestedMetadata = "Basic"
$ishSession.PipelineObjectPreference = "PSObjectNoteProperty"
$totalTime = (Measure-Command { $o = Find-IshDocumentObj -MetadataFilter $metadataFilter }).TotalMilliseconds
Write-Host ($ishSession.PipelineObjectPreference.ToString() + " " + $ishSession.DefaultRequestedMetadata.ToString() + ": " + $totalTime)
$ishSession.PipelineObjectPreference = "Off"
$totalTime = (Measure-Command { $o = Find-IshDocumentObj -MetadataFilter $metadataFilter }).TotalMilliseconds
Write-Host ($ishSession.PipelineObjectPreference.ToString() + " " + $ishSession.DefaultRequestedMetadata.ToString() + ": " + $totalTime)

$ishSession.DefaultRequestedMetadata = "All"
$ishSession.PipelineObjectPreference = "PSObjectNoteProperty"
$totalTime = (Measure-Command { $o = Find-IshDocumentObj -MetadataFilter $metadataFilter }).TotalMilliseconds
Write-Host ($ishSession.PipelineObjectPreference.ToString() + " " + $ishSession.DefaultRequestedMetadata.ToString() + ": " + $totalTime)
$ishSession.PipelineObjectPreference = "Off"
$totalTime = (Measure-Command { $o = Find-IshDocumentObj -MetadataFilter $metadataFilter }).TotalMilliseconds
Write-Host ($ishSession.PipelineObjectPreference.ToString() + " " + $ishSession.DefaultRequestedMetadata.ToString() + ": " + $totalTime)


<#
PSObjectNoteProperty Descriptive: 1934.9672
Off Descriptive: 1684.9407
PSObjectNoteProperty Basic: 3095.9967
Off Basic: 2670.181
PSObjectNoteProperty All: 5125.0702
Off All: 4502.4662
#>