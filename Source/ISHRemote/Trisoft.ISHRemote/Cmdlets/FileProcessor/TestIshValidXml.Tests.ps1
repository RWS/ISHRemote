BeforeAll {
	$cmdletName = "Test-IshValidXml"
	Write-Host ("`r`nLoading ISHRemote.PesterSetup.ps1 over BeforeAll-block for MyCommand[" + $cmdletName + "]...")
	. (Join-Path (Split-Path -Parent $PSCommandPath) "\..\..\ISHRemote.PesterSetup.ps1")
	
	Write-Host ("Running "+$cmdletName+" Test Data and Variables initialization")
	$tempFolder = [System.IO.Path]::GetTempPath()
	$scriptFolderPath = Split-Path -Parent $PSCommandPath

$ditaTaskFileContent = @"
<?xml version="1.0" ?>
<!DOCTYPE task PUBLIC "-//OASIS//DTD DITA Task//EN" "task.dtd">
<task id="GUID-TASK"><title>Enter the title of your task here.</title><shortdesc>Enter a short description of your task here (optional).</shortdesc><taskbody><prereq>Enter the prerequisites here (optional).</prereq><context>Enter the context of your task here (optional).</context><steps><step><cmd>Enter your first step here.</cmd><stepresult>Enter the result of your step here (optional).</stepresult></step></steps><example>Enter an example that illustrates the current task (optional).</example><postreq>Enter the tasks the user should do after finishing this task (optional).</postreq></taskbody></task>
"@
$ditaObfuscatedTaskFileContent = @"
<?xml version="1.0"?>
<!DOCTYPE task PUBLIC "-//OASIS//DTD DITA Task//EN" "task.dtd"[]>
<task id="GUID-TASK"><title>Would the would be easy easy easy.</titleINVALID><shortdesc>Would a would alternative be easy easy easy (zucchini).</shortdesc><taskbody><prereq>Would the extraordinary easy (zucchini).</prereq><context>Would the healthy be easy easy easy (zucchini).</context><steps><step><cmd>Would easy would easy easy.</cmd><stepresult>Would the summer be easy easy easy (zucchini).</stepresult></step></steps><example>Would be healthy easy alternative the healthy easy (zucchini).</example><postreq>Would the would the easy summer be would breakfast easy easy (zucchini).</postreq></taskbody></task>

"@ # empty line space is required for comparison

$ditaBookMapFileContent = @"
<!DOCTYPE bookmap PUBLIC "-//OASIS//DTD DITA Learning BookMap//EN" "../../dtd/bookmap.dtd"[]>
<bookmap id="GUID-BOOKMAP"><booktitle><mainbooktitle>Product tasks</mainbooktitle><booktitlealt>Tasks and what they do</booktitlealt></booktitle><bookmeta><author>John Doe</author><bookrights><copyrfirst><year>2006</year></copyrfirst><bookowner><person href="janedoe.dita">Jane Doe</person></bookowner></bookrights></bookmeta><frontmatter><preface/></frontmatter><chapter format="ditamap" href="installing.ditamap"/><chapter href="configuring.dita"/><chapter navtitle="maintaining as navtitle" href="maintaining.dita"><topicref href="maintainstorage.dita"/><topicref href="maintainserver.dita"/><topicref href="maintaindatabase.dita"/></chapter><appendix href="task_appendix.dita"/></bookmap>
"@
$ditaObfuscatedBookMapWithoutNavtitleFileContent = @"
<!DOCTYPE bookmap PUBLIC "-//OASIS//DTD DITA Learning BookMap//EN" "../../dtd/bookmap.dtd"[]>
<bookmap id="GUID-BOOKMAP"><booktitle INVALIDINVALID='INVALIDINVALID'><mainbooktitle>Healthy would</mainbooktitle><booktitlealt>Would the easy easy be</booktitlealt></booktitle><bookmeta><author>Easy The</author><bookrights><copyrfirst><year>2006</year></copyrfirst><bookowner><person href="janedoe.dita">Easy The</person></bookowner></bookrights></bookmeta><frontmatter><preface /></frontmatter><chapter format="ditamap" href="installing.ditamap" /><chapter href="configuring.dita" /><chapter navtitle="maintaining as navtitle" href="maintaining.dita"><topicref href="maintainstorage.dita" /><topicref href="maintainserver.dita" /><topicref href="maintaindatabase.dita" /></chapter><appendix href="task_appendix.dita" /></bookmap>

"@ # empty line space is required for comparison
$ditaObfuscatedBookMapWithNavtitleFileContent = @"
<!DOCTYPE bookmap PUBLIC "-//OASIS//DTD DITA Learning BookMap//EN" "../../dtd/bookmap.dtd"[]>
<INVALIDINVALIDINVALIDbookmap id="GUID-BOOKMAP"><booktitle><mainbooktitle>Healthy would</mainbooktitle><booktitlealt>Would the easy easy be</booktitlealt></booktitle><bookmeta><author>Easy The</author><bookrights><copyrfirst><year>2006</year></copyrfirst><bookowner><person href="janedoe.dita">Easy The</person></bookowner></bookrights></bookmeta><frontmatter><preface /></frontmatter><chapter format="ditamap" href="installing.ditamap" /><chapter href="configuring.dita" /><chapter navtitle="alternative be zucchini" href="maintaining.dita"><topicref href="maintainstorage.dita" /><topicref href="maintainserver.dita" /><topicref href="maintaindatabase.dita" /></chapter><appendix href="task_appendix.dita" /></bookmap>

"@ # empty line space is required for comparison

}

Describe "Test-IshValidXml" -Tags "Read" {
	BeforeAll {
		$rootFolder = Join-Path -Path $tempFolder -ChildPath $cmdletName 
		New-Item -ItemType Directory -Path $rootFolder
		$inputFolder = Join-Path -Path $rootFolder -ChildPath "input"
		New-Item -ItemType Directory -Path $inputFolder
		
		$task1FilePath = Join-Path -Path $inputFolder -ChildPath "task==1=en.xml"
		Set-Content -Path $task1FilePath -Value $ditaTaskFileContent -Encoding UTF8
		$task2FilePath = Join-Path -Path $inputFolder -ChildPath "task==2=en.xml"
		Set-Content -Path $task2FilePath -Value $ditaObfuscatedTaskFileContent -Encoding UTF8
		
		$bookMap1FilePath = Join-Path -Path $inputFolder -ChildPath "bookmap==1=en.xml"
		Set-Content -Path $bookMap1FilePath -Value $ditaBookMapFileContent -Encoding UTF8
		$bookMap2FilePath = Join-Path -Path $inputFolder -ChildPath "bookmap==2=en.xml"
		Set-Content -Path $bookMap2FilePath -Value $ditaObfuscatedBookMapWithoutNavtitleFileContent -Encoding UTF8
		$bookMap3FilePath = Join-Path -Path $inputFolder -ChildPath "bookmap==3=en.xml"
		Set-Content -Path $bookMap3FilePath -Value $ditaObfuscatedBookMapWithNavtitleFileContent -Encoding UTF8

		$catalogFilePath = Join-Path -Path $scriptFolderPath -ChildPath "..\..\Samples\Data-GeneralizeDitaXml\SpecializedDTDs\catalog-alldita12dtds.xml"
	}  
	Context "Test-IshValidXml" {
		It "GetType().Name" {
			$result = Test-IshValidXml -XmlCatalogFilePath $catalogFilePath -FilePath $task1FilePath
			$result.GetType().Name | Should -BeExactly "Boolean"
		}
		It "Parameter XmlCatalogFilePath invalid" {
			{
				Test-IshValidXml -XmlCatalogFilePath "INVALID" -FilePath $task1FilePath
			} | Should -Throw
		}
		It "Parameter FilePath invalid" {
			Test-IshValidXml -XmlCatalogFilePath $catalogFilePath -FilePath "INVALID" | Should -Be $False
		}
		It "Parameter FilePath Single" {
			$resultArray = Test-IshValidXml -XmlCatalogFilePath $catalogFilePath -FilePath $task1FilePath
			$resultArray.Count | Should -Be 1
			$resultArray[0] | Should -Be $True
		}
		It "Parameter FilePath Multiple" {
			$resultArray = Test-IshValidXml -XmlCatalogFilePath $catalogFilePath -FilePath @($task1FilePath,$task2FilePath)
			$resultArray.Count | Should -Be 2
			$resultArray[0] | Should -Be $True
			$resultArray[1] | Should -Be $False
		}
		It "Pipeline FilePath Single" {
			$fileInfoArray = Get-Item -Path $task2FilePath
			$resultArray = $fileInfoArray | Test-IshValidXml -XmlCatalogFilePath $catalogFilePath
			$resultArray.Count | Should -Be 1
			$resultArray[0] | Should -Be $False
		}
		It "Pipeline FilePath Multiple" {
			$fileInfoArray = @((Get-Item -Path $bookMap2FilePath), (Get-Item -Path $bookMap3FilePath))
			$resultArray = $fileInfoArray | Test-IshValidXml -XmlCatalogFilePath $catalogFilePath
			$resultArray.Count | Should -Be 2
			$resultArray[0] | Should -Be $False
			$resultArray[1] | Should -Be $False
		}
	}
}

AfterAll {
	Write-Host ("Running "+$cmdletName+" Test Data and Variables cleanup")
	try { Remove-Item (Join-Path $tempFolder $cmdletName) -Recurse -Force } catch { }
}

