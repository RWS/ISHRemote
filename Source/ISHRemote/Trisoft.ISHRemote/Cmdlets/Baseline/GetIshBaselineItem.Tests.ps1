BeforeAll {
	$cmdletName = "Get-IshBaselineItem"
	Write-Host ("`r`nLoading ISHRemote.PesterSetup.ps1 over BeforeAll-block for MyCommand[" + $cmdletName + "]...")
	. (Join-Path (Split-Path -Parent $PSCommandPath) "\..\..\ISHRemote.PesterSetup.ps1")

	Write-Host ("Running "+$cmdletName+" Test Data and Variables initialization")
}

Describe "Get-IshBaselineItem" -Tags "Read" {
	Context "Get-IshBaselineItem ParameterGroup" {
		It "Parameter IshSession invalid" {
			{ Get-IshBaselineItem -IShSession "INVALIDISHSESSION" } | Should -Throw
		}
	}
	Context "Get-IshBaselineItem returns IshBaselineItem object" {
		BeforeAll {
			$baselineName = ($cmdletName + " " + (Get-Date -Format "yyyyMMddHHmmssfff") + " A")
			$ishObject = Add-IshBaseline -IshSession $ishSession -Name $baselineName
			$ishObject = Set-IshBaselineItem -IshSession $ishSession -IshObject $ishObject -LogicalId "$cmdletName--AAA" -Version "1"
		}
		It "Parameter IshSession explicit" {
			$ishBaselineItem = (Get-IshBaselineItem -IShSession $ishSession -IshObject $ishObject)[0]
			$ishBaselineItem.GetType().Name | Should -BeExactly "IshBaselineItem"
			$ishBaselineItem.IshRef | Should -Not -BeNullOrEmpty
			$ishBaselineItem.LogicalId | Should -Not -BeNullOrEmpty
			$ishBaselineItem.Version | Should -Not -BeNullOrEmpty
			$ishBaselineItem.Author | Should -Not -BeNullOrEmpty
			$ishBaselineItem.CreatedOn | Should -Not -BeNullOrEmpty
			$ishBaselineItem.CreatedOnAsSortableDateTime | Should -Not -BeNullOrEmpty
			$ishBaselineItem.ModifiedOn | Should -Not -BeNullOrEmpty
			$ishBaselineItem.ModifiedOnAsSortableDateTime | Should -Not -BeNullOrEmpty
		}
		It "Parameter IshSession implicit" {
			$ishBaselineItem = (Get-IshBaselineItem -IshObject $ishObject)[0]
			$ishBaselineItem.GetType().Name | Should -BeExactly "IshBaselineItem"
			$ishBaselineItem.IshRef | Should -Not -BeNullOrEmpty
			$ishBaselineItem.LogicalId | Should -Not -BeNullOrEmpty
			$ishBaselineItem.Version | Should -Not -BeNullOrEmpty
			$ishBaselineItem.Author | Should -Not -BeNullOrEmpty
			$ishBaselineItem.CreatedOn | Should -Not -BeNullOrEmpty
			$ishBaselineItem.CreatedOnAsSortableDateTime | Should -Not -BeNullOrEmpty
			$ishBaselineItem.ModifiedOn | Should -Not -BeNullOrEmpty
			$ishBaselineItem.ModifiedOnAsSortableDateTime | Should -Not -BeNullOrEmpty		
		}
	}
	Context "Get-IshBaselineItem IshObjectsGroup" {
		BeforeAll {
			$baselineName = ($cmdletName + " " + (Get-Date -Format "yyyyMMddHHmmssfff") + " A")
			$ishObjectA = Add-IshBaseline -IshSession $ishSession -Name $baselineName
			Set-IshBaselineItem -IshSession $ishSession -IshObject $ishObjectA -LogicalId "$cmdletName--A" -Version "1"
			Set-IshBaselineItem -IshSession $ishSession -IshObject $ishObjectA -LogicalId "$cmdletName--AA" -Version "2"
			$baselineName = ($cmdletName + " " + (Get-Date -Format "yyyyMMddHHmmssfff") + " B")
			$ishObjectB = Add-IshBaseline -IshSession $ishSession -Name $baselineName
			Set-IshBaselineItem -IshSession $ishSession -IshObject $ishObjectB -LogicalId "$cmdletName--B" -Version "1"
			Set-IshBaselineItem -IshSession $ishSession -IshObject $ishObjectB -LogicalId "$cmdletName--BB" -Version "2"
			$baselineName = ($cmdletName + " " + (Get-Date -Format "yyyyMMddHHmmssfff") + " C")
			$ishObjectC = Add-IshBaseline -IshSession $ishSession -Name $baselineName
			Set-IshBaselineItem -IshSession $ishSession -IshObject $ishObjectC -LogicalId "$cmdletName--C" -Version "1"
			Set-IshBaselineItem -IshSession $ishSession -IshObject $ishObjectC -LogicalId "$cmdletName--CC" -Version "2"
			$baselineName = ($cmdletName + " " + (Get-Date -Format "yyyyMMddHHmmssfff") + " D")
			$ishObjectD = Add-IshBaseline -IshSession $ishSession -Name $baselineName
			Set-IshBaselineItem -IshSession $ishSession -IshObject $ishObjectD -LogicalId "$cmdletName--D" -Version "1"
			Set-IshBaselineItem -IshSession $ishSession -IshObject $ishObjectD -LogicalId "$cmdletName--DD" -Version "2"
		}
		It "Parameter IshObject invalid" {
			{ Get-IshBaselineItem -IShSession $ishSession -IshObject "INVALIDBASELINE" } | Should -Throw
		}
		It "Parameter IshObject Single" {
			(Get-IshBaselineItem -IshSession $ishSession -IshObject $ishObjectA).Count | Should -Be 2
		}
		It "Parameter IshObject Multiple" {
			(Get-IshBaselineItem -IshSession $ishSession -IshObject @($ishObjectB,$ishObjectC)).Count | Should -Be 4
		}
		It "Pipeline IshObject Single" {
			$ishBaselineItems = $ishObjectD | Get-IshBaselineItem -IshSession $ishSession
			$ishBaselineItems.Count | Should -Be 2
		}
		It "Pipeline IshObject Multiple" {
			$ishBaselineItems = @($ishObjectA,$ishObjectB,$ishObjectC,$ishObjectD) | Get-IshBaselineItem -IshSession $ishSession
			$ishBaselineItems.Count | Should -Be 8
		}
	}
}

AfterAll {
	Write-Host ("Running "+$cmdletName+" Test Data and Variables cleanup")
	$baselines = Find-IshBaseline -IshSession $ishSession -MetadataFilter (Set-IshMetadataFilterField -IshSession $ishSession -Name "NAME" -FilterOperator like -Value "$cmdletName%")
	try { Remove-IshBaseline -IshSession $ishSession -IshObject $baselines } catch { }
}

