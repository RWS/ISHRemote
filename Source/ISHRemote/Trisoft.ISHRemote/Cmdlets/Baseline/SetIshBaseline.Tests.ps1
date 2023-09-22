BeforeAll {
	$cmdletName = "Set-IshBaseline"
	Write-Host ("`r`nLoading ISHRemote.PesterSetup.ps1 on PSVersion[" + $psversionTable.PSVersion + "] over BeforeAll-block for MyCommand[" + $cmdletName + "]...")
	. (Join-Path (Split-Path -Parent $PSCommandPath) "\..\..\ISHRemote.PesterSetup.ps1")

	Write-Host ("Running "+$cmdletName+" Test Data and Variables initialization")
}

Describe "Set-IshBaseline" -Tags "Create" {
	Context "Set-IshBaseline ParameterGroup" {
		It "Parameter IshSession invalid" {
			{ Set-IshBaseline -IShSession "INVALIDISHSESSION" -Name "INVALIDBASELINENAME" } | Should -Throw
		}
	}
	Context "Set-IshBaseline ParameterGroup" {
		It "Parameter IshSession explicit" {
			$baselineName = ($cmdletName + " " + (Get-Date -Format "yyyyMMddHHmmssfff") + " Name")
			$ishObject = Add-IshBaseline -IshSession $ishSession -Name $baselineName
			$metadata = Set-IshMetadataField -IshSession $ishSession -Name "FISHDOCUMENTRELEASE" -Level None -Value "$baselineName RENAMED"
			$ishObject = Set-IshBaseline -IshSession $ishSession -Id $ishObject.IshRef -Metadata $metadata
			$ishObject.GetType().Name | Should -BeExactly "IshBaseline"
			$ishObject.Count | Should -Be 1
		}
		It "Parameter IshSession implicit" {
			$baselineName = ($cmdletName + " " + (Get-Date -Format "yyyyMMddHHmmssfff") + " Name")
			$ishObject = Add-IshBaseline -Name $baselineName
			$metadata = Set-IshMetadataField -Name "FISHDOCUMENTRELEASE" -Level None -Value "$baselineName RENAMED"
			$ishObject = Set-IshBaseline -Id $ishObject.IshRef -Metadata $metadata
			$ishObject.GetType().Name | Should -BeExactly "IshBaseline"
			$ishObject.Count | Should -Be 1
			$ishSession.DefaultRequestedMetadata | Should -Be "Basic"
			$ishObject.fishdocumentrelease.Length -ge 1 | Should -Be $true 
			$ishObject.fishdocumentrelease_none_element.StartsWith('GUID') | Should -Be $true 
		}
	}

	Context "Set-IshBaseline IshObjectsGroup" {
		BeforeAll {
			$baselineName = ($cmdletName + " " + (Get-Date -Format "yyyyMMddHHmmssfff") + " A")
			$ishObjectA = Add-IshBaseline -IshSession $ishSession -Name $baselineName
			$baselineName = ($cmdletName + " " + (Get-Date -Format "yyyyMMddHHmmssfff") + " B")
			$ishObjectB = Add-IshBaseline -IshSession $ishSession -Name $baselineName
			$baselineName = ($cmdletName + " " + (Get-Date -Format "yyyyMMddHHmmssfff") + " C")
			$ishObjectC = Add-IshBaseline -IshSession $ishSession -Name $baselineName
			$baselineName = ($cmdletName + " " + (Get-Date -Format "yyyyMMddHHmmssfff") + " D")
			$ishObjectD = Add-IshBaseline -IshSession $ishSession -Name $baselineName
			$baselineName = ($cmdletName + " " + (Get-Date -Format "yyyyMMddHHmmssfff") + " E")
			$ishObjectE = Add-IshBaseline -IshSession $ishSession -Name $baselineName
			$baselineName = ($cmdletName + " " + (Get-Date -Format "yyyyMMddHHmmssfff") + " F")
			$ishObjectF = Add-IshBaseline -IshSession $ishSession -Name $baselineName
		}
		It "Parameter IshObject invalid" {
			{ Set-IshBaseline -IShSession $ishSession -IshObject "INVALIDBASELINE" } | Should -Throw
		}
		It "Parameter IshObject Single" {
			$ishObjectA = $ishObjectA | Set-IshMetadataField -IshSession $ishSession -Name "FISHDOCUMENTRELEASE" -Level None -Value ($cmdletName + " " + (Get-Date -Format "yyyyMMddHHmmssfff") + " A Parameter IshObject Single RENAMED")
			$ishObjects = Set-IshBaseline -IshSession $ishSession -IshObject $ishObjectA
			$ishObjects.Count | Should -Be 1
		}
		It "Parameter IshObject Multiple" {
			$ishObjectB = $ishObjectB | Set-IshMetadataField -IshSession $ishSession -Name "FISHDOCUMENTRELEASE" -Level None -Value ($cmdletName + " " + (Get-Date -Format "yyyyMMddHHmmssfff") + " B Parameter IshObject Multiple RENAMED")
			$ishObjectC = $ishObjectC | Set-IshMetadataField -IshSession $ishSession -Name "FISHDOCUMENTRELEASE" -Level None -Value ($cmdletName + " " + (Get-Date -Format "yyyyMMddHHmmssfff") + " C Parameter IshObject Multiple RENAMED")
			$ishObjects = Set-IshBaseline -IshSession $ishSession -IshObject @($ishObjectB,$ishObjectC)
			$ishObjects.Count | Should -Be 2
		}
		It "Pipeline IshObject Single" {
			$ishObjectD = $ishObjectD | Set-IshMetadataField -IshSession $ishSession -Name "FISHDOCUMENTRELEASE" -Level None -Value ($cmdletName + " " + (Get-Date -Format "yyyyMMddHHmmssfff") + " D Pipeline IshObject Single RENAMED")
			$ishObjects = $ishObjectD | Set-IshBaseline -IshSession $ishSession
			$ishObjects.Count | Should -Be 1
		}
		It "Pipeline IshObject Multiple" {
			$ishObjectE = $ishObjectE | Set-IshMetadataField -IshSession $ishSession -Name "FISHDOCUMENTRELEASE" -Level None -Value ($cmdletName + " " + (Get-Date -Format "yyyyMMddHHmmssfff") + " E Pipeline IshObject Multiple RENAMED")
			$ishObjectF = $ishObjectF | Set-IshMetadataField -IshSession $ishSession -Name "FISHDOCUMENTRELEASE" -Level None -Value ($cmdletName + " " + (Get-Date -Format "yyyyMMddHHmmssfff") + " F Pipeline IshObject Multiple RENAMED")
			$ishObjects = @($ishObjectE,$ishObjectF) | Set-IshBaseline -IshSession $ishSession
			$ishObjects.Count | Should -Be 2
		}
	}
}

AfterAll {
	Write-Host ("Running "+$cmdletName+" Test Data and Variables cleanup")
	$baselines = Find-IshBaseline -IshSession $ishSession -MetadataFilter (Set-IshMetadataFilterField -IshSession $ishSession -Name "NAME" -FilterOperator like -Value "$cmdletName%")
	try { Remove-IshBaseline -IshSession $ishSession -IshObject $baselines } catch { }
}

