Write-Host ("`r`nLoading ISHRemote.PesterSetup.ps1 for MyCommand[" + $MyInvocation.MyCommand + "]...")
. (Join-Path (Split-Path -Parent $MyInvocation.MyCommand.Path) "..\..\ISHRemote.PesterSetup.ps1")
$scriptFolderPath = Split-Path -Parent $MyInvocation.MyCommand.Path  # Needs to be outside Describe script block
$cmdletName = "New-IshObfuscatedFile"

$ditaTaskFileContent = @"
<?xml version="1.0" ?>
<!DOCTYPE task PUBLIC "-//OASIS//DTD DITA Task//EN" "task.dtd">
<task id="GUID-TASK"><title>Enter the title of your task here.</title><shortdesc>Enter a short description of your task here (optional).</shortdesc><taskbody><prereq>Enter the prerequisites here (optional).</prereq><context>Enter the context of your task here (optional).</context><steps><step><cmd>Enter your first step here.</cmd><stepresult>Enter the result of your step here (optional).</stepresult></step></steps><example>Enter an example that illustrates the current task (optional).</example><postreq>Enter the tasks the user should do after finishing this task (optional).</postreq></taskbody></task>
"@
$ditaObfuscatedTaskFileContent = @"
<?xml version="1.0"?>
<!DOCTYPE task PUBLIC "-//OASIS//DTD DITA Task//EN" "task.dtd"[]>
<task id="GUID-TASK"><title>Would the would be easy easy easy.</title><shortdesc>Would a would alternative be easy easy easy (zucchini).</shortdesc><taskbody><prereq>Would the extraordinary easy (zucchini).</prereq><context>Would the healthy be easy easy easy (zucchini).</context><steps><step><cmd>Would easy would easy easy.</cmd><stepresult>Would the summer be easy easy easy (zucchini).</stepresult></step></steps><example>Would be healthy easy alternative the healthy easy (zucchini).</example><postreq>Would the would the easy summer be would breakfast easy easy (zucchini).</postreq></taskbody></task>

"@ # empty line space is required for comparison

$ditaBookMapFileContent = @"
<!DOCTYPE bookmap PUBLIC "-//OASIS//DTD DITA Learning BookMap//EN" "../../dtd/bookmap.dtd"[]>
<bookmap id="GUID-BOOKMAP"><booktitle><mainbooktitle>Product tasks</mainbooktitle><booktitlealt>Tasks and what they do</booktitlealt></booktitle><bookmeta><author>John Doe</author><bookrights><copyrfirst><year>2006</year></copyrfirst><bookowner><person href="janedoe.dita">Jane Doe</person></bookowner></bookrights></bookmeta><frontmatter><preface/></frontmatter><chapter format="ditamap" href="installing.ditamap"/><chapter href="configuring.dita"/><chapter navtitle="maintaining as navtitle" href="maintaining.dita"><topicref href="maintainstorage.dita"/><topicref href="maintainserver.dita"/><topicref href="maintaindatabase.dita"/></chapter><appendix href="task_appendix.dita"/></bookmap>
"@
$ditaObfuscatedBookMapWithoutNavtitleFileContent = @"
<!DOCTYPE bookmap PUBLIC "-//OASIS//DTD DITA Learning BookMap//EN" "../../dtd/bookmap.dtd"[]>
<bookmap id="GUID-BOOKMAP"><booktitle><mainbooktitle>Healthy would</mainbooktitle><booktitlealt>Would the easy easy be</booktitlealt></booktitle><bookmeta><author>Easy The</author><bookrights><copyrfirst><year>2006</year></copyrfirst><bookowner><person href="janedoe.dita">Easy The</person></bookowner></bookrights></bookmeta><frontmatter><preface /></frontmatter><chapter format="ditamap" href="installing.ditamap" /><chapter href="configuring.dita" /><chapter navtitle="maintaining as navtitle" href="maintaining.dita"><topicref href="maintainstorage.dita" /><topicref href="maintainserver.dita" /><topicref href="maintaindatabase.dita" /></chapter><appendix href="task_appendix.dita" /></bookmap>

"@ # empty line space is required for comparison
$ditaObfuscatedBookMapWithNavtitleFileContent = @"
<!DOCTYPE bookmap PUBLIC "-//OASIS//DTD DITA Learning BookMap//EN" "../../dtd/bookmap.dtd"[]>
<bookmap id="GUID-BOOKMAP"><booktitle><mainbooktitle>Healthy would</mainbooktitle><booktitlealt>Would the easy easy be</booktitlealt></booktitle><bookmeta><author>Easy The</author><bookrights><copyrfirst><year>2006</year></copyrfirst><bookowner><person href="janedoe.dita">Easy The</person></bookowner></bookrights></bookmeta><frontmatter><preface /></frontmatter><chapter format="ditamap" href="installing.ditamap" /><chapter href="configuring.dita" /><chapter navtitle="alternative be zucchini" href="maintaining.dita"><topicref href="maintainstorage.dita" /><topicref href="maintainserver.dita" /><topicref href="maintaindatabase.dita" /></chapter><appendix href="task_appendix.dita" /></bookmap>

"@ # empty line space is required for comparison

try {

Describe “New-IshObfuscatedFile" -Tags "Read" {
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

	$imageFilePath = Join-Path -Path $inputFolder -ChildPath "image==1=en.jpg"
	Add-Type -AssemblyName "System.Drawing"
	$bmp = New-Object -TypeName System.Drawing.Bitmap(100,100)
	for ($i = 0; $i -lt 100; $i++) {    for ($j = 0; $j -lt 100; $j++) { $bmp.SetPixel($i, $j, 'Red') }    }
	$bmp.Save($imageFilePath, [System.Drawing.Imaging.ImageFormat]::Jpeg)
	
	  
	Context "New-IshObfuscatedFile" {
		$taskFileInfo = New-IshObfuscatedFile -FolderPath $outputFolder `
											  -FilePath $taskFilePath
		It "GetType().Name" {
			$taskFileInfo.GetType().Name | Should BeExactly "FileInfo"
		}
		It "OutputFilePath" {
			($taskFileInfo.FullName -eq (Join-Path -Path $outputFolder -ChildPath "task==1=en.xml")) | Should Be $True
		}
		It "Task Result String Comparison" {
			(Get-Content -Path $taskFileInfo -Raw) -eq $ditaObfuscatedTaskFileContent | Should Be $True
		}
		It "BookMap Result String Comparison without XmlAttributesToObfuscate @navtitle" {
			$bookMapFileInfo = New-IshObfuscatedFile -FolderPath $outputFolder -FilePath $bookMapFilePath
			(Get-Content -Path $bookMapFileInfo -Raw) -eq $ditaObfuscatedBookMapWithoutNavtitleFileContent | Should Be $True
		}
		It "BookMap Result String Comparison with XmlAttributesToObfuscate @navtitle" {
			$bookMapFileInfo = New-IshObfuscatedFile -FolderPath $outputFolder -FilePath $bookMapFilePath -XmlAttributesToObfuscate @("navtitle")
			(Get-Content -Path $bookMapFileInfo -Raw) -eq $ditaObfuscatedBookMapWithNavtitleFileContent | Should Be $True
		}
		It "Image Result Comparison" {
			$imageFilePath = New-IshObfuscatedFile -FolderPath $outputFolder `
											       -FilePath $imageFilePath
			$imageFilePath.Count -gt 0 | Should Be $True
		}
		It "Parameter XmlFileExtensions invalid" {
			{
				 New-IshObfuscatedFile -FolderPath $outputFolder -FilePath $bookMapFilePath -XmlFileExtensions "INVALID"
			} | Should Not Throw
		}
		It "Parameter ImageFileExtensions invalid" {
			{
				 New-IshObfuscatedFile -FolderPath $outputFolder -FilePath $imageFilePath -ImageFileExtensions "INVALID"
			} | Should Not Throw
		}
		It "Parameter FilePath Single" {
			$fileInfoArray =New-IshObfuscatedFile -FolderPath $outputFolder -FilePath $taskFilePath
			$fileInfoArray.Count | Should Be 1
		}
		It "Parameter FilePath Multiple" {
			$fileInfoArray = New-IshObfuscatedFile -FolderPath $outputFolder -FilePath @($taskFilePath,$bookMapFilePath)
			$fileInfoArray.Count | Should Be 2
		}
		It "Pipeline FilePath Single" {
			$fileInfos = Get-Item -Path $taskFilePath
			($fileInfos | New-IshObfuscatedFile -FolderPath $outputFolder).Count | Should Be 1
		}
		It "Pipeline FilePath Multiple" {
			$fileInfos = @((Get-Item -Path $taskFilePath), (Get-Item -Path $bookMapFilePath))
			($fileInfos | New-IshObfuscatedFile -FolderPath $outputFolder).Count | Should Be 2
		}
	}
}


} finally {
	Write-Host "Cleaning Test Data and Variables"
	try { Remove-Item (Join-Path $env:TEMP $cmdletName) -Recurse -Force } catch { }
}
