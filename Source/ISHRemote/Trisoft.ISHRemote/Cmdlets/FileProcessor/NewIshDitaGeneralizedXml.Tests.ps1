Write-Host ("`r`nLoading ISHRemote.PesterSetup.ps1 for MyCommand[" + $MyInvocation.MyCommand + "]...")
. (Join-Path (Split-Path -Parent $MyInvocation.MyCommand.Path) "..\..\ISHRemote.PesterSetup.ps1")
$scriptFolderPath = Split-Path -Parent $MyInvocation.MyCommand.Path  # Needs to be outside Describe script block
$cmdletName = "New-IshDitaGeneralizedXml"

$ditaTaskFileContent = @"
<?xml version="1.0" ?>
<!DOCTYPE task PUBLIC "-//OASIS//DTD DITA Task//EN" "task.dtd">
<task id="GUID-TASK"><title>Enter the title of your task here.</title><shortdesc>Enter a short description of your task here (optional).</shortdesc><taskbody><prereq>Enter the prerequisites here (optional).</prereq><context>Enter the context of your task here (optional).</context><steps><step><cmd>Enter your first step here.</cmd><stepresult>Enter the result of your step here (optional).</stepresult></step></steps><example>Enter an example that illustrates the current task (optional).</example><postreq>Enter the tasks the user should do after finishing this task (optional).</postreq></taskbody></task>
"@
$ditaGeneralizedTaskFileContent = @"
<?xml version="1.0"?>
<!DOCTYPE topic PUBLIC "-//OASIS//DTD DITA Topic//EN" "dita-oasis/1.2/topic.dtd"[]>
<topic id="GUID-TASK"><title>Enter the title of your task here.</title><shortdesc>Enter a short description of your task here (optional).</shortdesc><body><section>Enter the prerequisites here (optional).</section><section>Enter the context of your task here (optional).</section><ol><li><ph>Enter your first step here.</ph><itemgroup>Enter the result of your step here (optional).</itemgroup></li></ol><example>Enter an example that illustrates the current task (optional).</example><section>Enter the tasks the user should do after finishing this task (optional).</section></body></topic>

"@ # empty line space is required for comparison

$ditaBookMapFileContent = @"
<!DOCTYPE bookmap PUBLIC "-//OASIS//DTD DITA Learning BookMap//EN" "../../dtd/bookmap.dtd"[]>
<bookmap id="GUID-BOOKMAP"><booktitle><mainbooktitle>Product tasks</mainbooktitle><booktitlealt>Tasks and what they do</booktitlealt></booktitle><bookmeta><author>John Doe</author><bookrights><copyrfirst><year>2006</year></copyrfirst><bookowner><person href="janedoe.dita">Jane Doe</person></bookowner></bookrights></bookmeta><frontmatter><preface/></frontmatter><chapter format="ditamap" href="installing.ditamap"/><chapter href="configuring.dita"/><chapter href="maintaining.dita"><topicref href="maintainstorage.dita"/><topicref href="maintainserver.dita"/><topicref href="maintaindatabase.dita"/></chapter><appendix href="task_appendix.dita"/></bookmap>
"@
$ditaGeneralizedBookMapFileContent = @"
<?xml version="1.0" encoding="utf-16"?><!DOCTYPE map PUBLIC "-//OASIS//DTD DITA Map//EN" "dita-oasis/1.2/technicalContent/dtd/map.dtd"[]>
<map id="GUID-BOOKMAP"><title><ph>Product tasks</ph><ph>Tasks and what they do</ph></title><topicmeta><author>John Doe</author><data><data><ph>2006</ph></data><data><data href="janedoe.dita">Jane Doe</data></data></data></topicmeta><topicref><topicref /></topicref><topicref format="ditamap" href="installing.ditamap" /><topicref href="configuring.dita" /><topicref href="maintaining.dita"><topicref href="maintainstorage.dita" /><topicref href="maintainserver.dita" /><topicref href="maintaindatabase.dita" /></topicref><topicref href="task_appendix.dita" /></map>

"@ # empty line space is required for comparison

try {

Describe “New-IshDitaGeneralizedXml" -Tags "Read" {
	Write-Host "Initializing Test Data and Variables"
	$rootFolder = Join-Path -Path $env:TEMP -ChildPath $cmdletName 
	New-Item -ItemType Directory -Path $rootFolder
	$inputFolder = Join-Path -Path $rootFolder -ChildPath "input"
	New-Item -ItemType Directory -Path $inputFolder
	$outputFolder = Join-Path -Path $rootFolder -ChildPath "output"
	New-Item -ItemType Directory -Path $outputFolder
	$taskFilePath = Join-Path -Path $inputFolder -ChildPath "task==1=en.xml"
	Set-Content -Path $taskFilePath -Value $ditaTaskFileContent -Encoding UTF8
	$bookMapFilePath = Join-Path -Path $inputFolder -ChildPath "bookmap==1=en.xml"
	Set-Content -Path $bookMapFilePath -Value $ditaBookMapFileContent -Encoding UTF8
	Write-Host "Testing Catalog Existance"
	# Location of the catalog xml that contains the specialized dtds
	$specializedCatalogFilePath = Join-Path -Path $scriptFolderPath -ChildPath "..\..\Samples\Data-GeneralizeDitaXml\SpecializedDTDs\catalog-alldita12dtds.xml"
	# Location of the catalog xml that contains the "base" dtds
    $generalizedCatalogFilePath = Join-Path -Path $scriptFolderPath -ChildPath "..\..\Samples\Data-GeneralizeDitaXml\GeneralizedDTDs\catalog-dita12topic&maponly.xml"
	# File that contains a mapping between the specialized dtd and the according generalized dtd.
    $generalizationCatalogMappingFilePath = Join-Path -Path $scriptFolderPath -ChildPath "..\..\Samples\Data-GeneralizeDitaXml\generalization-catalog-mapping.xml"
	if (-Not (Test-Path -Path $specializedCatalogFilePath -PathType Leaf)) { Write-Warning ("Catalog specializedCatalogFilePath[$specializedCatalogFilePath] missing!") }
	if (-Not (Test-Path -Path $generalizedCatalogFilePath -PathType Leaf)) { Write-Warning ("Catalog generalizedCatalogFilePath[$generalizedCatalogFilePath] missing!") }
	if (-Not (Test-Path -Path $generalizationCatalogMappingFilePath -PathType Leaf)) { Write-Warning ("Catalog generalizationCatalogMappingFilePath[$generalizationCatalogMappingFilePath] missing!") }
	# If you would have specialized attributes from the DITA 1.2 "props" attribute, specify those attributes here to generalize them to the "props" attribute again.  Here just using modelyear, market, vehicle as an example
	$attributesToGeneralizeToProps = @("modelA", "modelB", "modelC")
	# If you would have specialized attributes from the DITA 1.2 "base" attribute, specify those attributes here to generalize them to the "base" attribute again. Here just using basea, baseb, basec as an example
	$attributesToGeneralizeToBase = @("basea", "baseb", "basec")

	  
	Context "New-IshDitaGeneralizedXml returns FileInfo" {
		$taskFileInfo = New-IshDitaGeneralizedXml -SpecializedCatalogLocation $specializedCatalogFilePath `
											  -GeneralizedCatalogLocation $generalizedCatalogFilePath `
											  -GeneralizationCatalogMappingLocation $generalizationCatalogMappingFilePath `
											  -AttributesToGeneralizeToProps $attributesToGeneralizeToProps `
											  -AttributesToGeneralizeToBase $attributesToGeneralizeToBase `
											  -FolderPath $outputFolder `
											  -FilePath $taskFilePath
		It "GetType().Name" {
			$taskFileInfo.GetType().Name | Should BeExactly "FileInfo"
		}
		It "Task Result String Comparison" {
			(Get-Content -Path $taskFileInfo -Raw) -eq $ditaGeneralizedTaskFileContent | Should Be $True
		}
		It "BookMap without Attributes options Result String Comparison" {
			$bookMapFileInfo = Get-Item $bookMapFilePath | 
			                   New-IshDitaGeneralizedXml -SpecializedCatalogLocation $specializedCatalogFilePath `
							                             -GeneralizedCatalogLocation $generalizedCatalogFilePath `
														 -GeneralizationCatalogMappingLocation $generalizationCatalogMappingFilePath `
														 -FolderPath $outputFolder
			(Get-Content -Path $bookMapFileInfo -Raw) -eq $ditaGeneralizedBookMapFileContent | Should Be $True
		}
		It "Parameter SpecializedCatalogLocation invalid" {
			{
				$fileInfo = New-IshDitaGeneralizedXml -SpecializedCatalogLocation "INVALIDILEPATH" `
											  -GeneralizedCatalogLocation $generalizedCatalogFilePath `
											  -GeneralizationCatalogMappingLocation $generalizationCatalogMappingFilePath `
											  -AttributesToGeneralizeToProps $attributesToGeneralizeToProps `
											  -AttributesToGeneralizeToBase $attributesToGeneralizeToBase `
											  -FolderPath $outputFolder `
											  -FilePath $taskFilePath
			} | Should Throw
		}
		It "Parameter GeneralizedCatalogLocation invalid" {
			{
				$fileInfo = New-IshDitaGeneralizedXml -SpecializedCatalogLocation $specializedCatalogFilePath `
											  -GeneralizedCatalogLocation "INVALIDILEPATH" `
											  -GeneralizationCatalogMappingLocation $generalizationCatalogMappingFilePath `
											  -AttributesToGeneralizeToProps $attributesToGeneralizeToProps `
											  -AttributesToGeneralizeToBase $attributesToGeneralizeToBase `
											  -FolderPath $outputFolder `
											  -FilePath $taskFilePath
			} | Should Throw
		}
		It "Parameter GeneralizationCatalogMappingLocation invalid" {
			{
				$fileInfo = New-IshDitaGeneralizedXml -SpecializedCatalogLocation $specializedCatalogFilePath `
											  -GeneralizedCatalogLocation $generalizationCatalogMappingFilePath `
											  -GeneralizationCatalogMappingLocation "INVALIDILEPATH" `
											  -AttributesToGeneralizeToProps $attributesToGeneralizeToProps `
											  -AttributesToGeneralizeToBase $attributesToGeneralizeToBase `
											  -FolderPath $outputFolder `
											  -FilePath $taskFilePath
			} | Should Throw
		}
		It "Parameter FilePath invalid will result in warning" {
			{
				$fileInfo = New-IshDitaGeneralizedXml -SpecializedCatalogLocation $specializedCatalogFilePath `
											  -GeneralizedCatalogLocation $generalizationCatalogMappingFilePath `
											  -GeneralizationCatalogMappingLocation  $generalizationCatalogMappingFilePath `
											  -AttributesToGeneralizeToProps $attributesToGeneralizeToProps `
											  -AttributesToGeneralizeToBase $attributesToGeneralizeToBase `
											  -FolderPath $outputFolder `
											  -FilePath "INVALIDILEPATH"
			} | Should Not Throw
		}
		It "Parameter FilePath Single" {
			$fileInfoArray = New-IshDitaGeneralizedXml -SpecializedCatalogLocation $specializedCatalogFilePath `
											  -GeneralizedCatalogLocation $generalizedCatalogFilePath `
											  -GeneralizationCatalogMappingLocation $generalizationCatalogMappingFilePath `
											  -AttributesToGeneralizeToProps $attributesToGeneralizeToProps `
											  -AttributesToGeneralizeToBase $attributesToGeneralizeToBase `
											  -FolderPath $outputFolder `
											  -FilePath $taskFilePath
			$fileInfoArray.Count | Should Be 1
		}
		It "Parameter FilePath Multiple" {
			$fileInfoArray = New-IshDitaGeneralizedXml -SpecializedCatalogLocation $specializedCatalogFilePath `
											  -GeneralizedCatalogLocation $generalizedCatalogFilePath `
											  -GeneralizationCatalogMappingLocation $generalizationCatalogMappingFilePath `
											  -AttributesToGeneralizeToProps $attributesToGeneralizeToProps `
											  -AttributesToGeneralizeToBase $attributesToGeneralizeToBase `
											  -FolderPath $outputFolder `
											  -FilePath @($taskFilePath,$bookMapFilePath)
			$fileInfoArray.Count | Should Be 2
		}
		It "Pipeline FilePath Single" {
			$fileInfos = Get-Item -Path $taskFilePath
			($fileInfos | New-IshDitaGeneralizedXml -SpecializedCatalogLocation $specializedCatalogFilePath `
											  -GeneralizedCatalogLocation $generalizedCatalogFilePath `
											  -GeneralizationCatalogMappingLocation $generalizationCatalogMappingFilePath `
											  -AttributesToGeneralizeToProps $attributesToGeneralizeToProps `
											  -AttributesToGeneralizeToBase $attributesToGeneralizeToBase `
											  -FolderPath $outputFolder).Count | Should Be 1
		}
		It "Pipeline FilePath Multiple" {
			$fileInfos = @((Get-Item -Path $taskFilePath), (Get-Item -Path $bookMapFilePath))
			($fileInfos | New-IshDitaGeneralizedXml -SpecializedCatalogLocation $specializedCatalogFilePath `
											  -GeneralizedCatalogLocation $generalizedCatalogFilePath `
											  -GeneralizationCatalogMappingLocation $generalizationCatalogMappingFilePath `
											  -AttributesToGeneralizeToProps $attributesToGeneralizeToProps `
											  -AttributesToGeneralizeToBase $attributesToGeneralizeToBase `
											  -FolderPath $outputFolder).Count | Should Be 2
		}
	}
}


} finally {
	Write-Host "Cleaning Test Data and Variables"
	try { Remove-Item (Join-Path $env:TEMP $cmdletName) -Recurse -Force } catch { }
}
