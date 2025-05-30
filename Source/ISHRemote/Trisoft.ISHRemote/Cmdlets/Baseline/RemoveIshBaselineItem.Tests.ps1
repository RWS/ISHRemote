BeforeAll {
	$cmdletName = "Remove-IshBaselineItem"
	Write-Host ("`r`nLoading ISHRemote.PesterSetup.ps1 on PSVersion[" + $psversionTable.PSVersion + "] over BeforeAll-block for MyCommand[" + $cmdletName + "]...")
	. (Join-Path (Split-Path -Parent $PSCommandPath) "\..\..\ISHRemote.PesterSetup.ps1")

	Write-Host ("Running "+$cmdletName+" Test Data and Variables initialization")
}

Describe "Remove-IshBaselineItem" {
	Context "Remove-IshBaselineItem remove item from ISHObject" {
		BeforeAll {
			$baselineName = ($cmdletName + " " + (Get-Date -Format "yyyyMMddHHmmssfff") + " BASELINE")
			$ishObject = Add-IshBaseline -IshSession $ishSession -Name $baselineName
			$ishObject = Set-IshBaselineItem -IshObject $ishObject -LogicalId "$cmdletName--A" -Version "1"
			$ishObject = Set-IshBaselineItem -IshObject $ishObject -LogicalId "$cmdletName--AA" -Version "2"
			$ishObject = Set-IshBaselineItem -IshObject $ishObject -LogicalId "$cmdletName--AAA" -Version "3"
			
			$ishObjectRemove = Remove-IshBaselineItem -IshSession $ishSession -IshObject $ishObject -LogicalId "$cmdletName--AAA" #remove
		}
		
		It "Parameter IshSession invalid" {
			{Remove-IshBaselineItem -IShSession "INVALIDISHSESSION" -IshObject $ishObject -LogicalId "$cmdletName--A"} | Should -Throw
		}
		
		It "GetType()" {
			$ishObjectRemove.GetType().Name | Should -BeExactly "IshBaseline"
		}
		
		It "$ishObject.IshRef" {
			$ishObjectRemove.IshRef | Should -Not -BeNullOrEmpty
		}
		
		It "Remove 1 item" {
			$ishObjectRemove = Remove-IshBaselineItem -IshSession $ishSession -IshObject $ishObject -LogicalId "$cmdletName--AA"
			(Get-IshBaselineItem -IshSession $ishSession -IshObject $ishObject).Count | Should -Be 1
		}

		It "Remove the last item" {
			$ishObjectRemove = Remove-IshBaselineItem -IshSession $ishSession -IshObject $ishObject -LogicalId "$cmdletName--A"
			(Get-IshBaselineItem -IshSession $ishSession -IshObject $ishObject).Count | Should -Be 0
		}
	}
	
	Context "Remove-IshBaselineItem remove item from multiple ISHObject(s)" {
		BeforeAll {
			$baselineName1 = ($cmdletName + " " + (Get-Date -Format "yyyyMMddHHmmssfff") + " BASELINE")
			$ishObject1 = Add-IshBaseline -IshSession $ishSession -Name $baselineName1
			$ishObject1 = Set-IshBaselineItem -IshObject $ishObject1 -LogicalId "$cmdletName--A1" -Version "1"
			$ishObject1 = Set-IshBaselineItem -IshObject $ishObject1 -LogicalId "$cmdletName--AA" -Version "2"
			
			$baselineName2 = ($cmdletName + " " + (Get-Date -Format "yyyyMMddHHmmssfff") + " BASELINE")
			$ishObject2 = Add-IshBaseline -IshSession $ishSession -Name $baselineName2
			$ishObject2 = Set-IshBaselineItem -IshObject $ishObject2 -LogicalId "$cmdletName--A2" -Version "1"
			$ishObject2 = Set-IshBaselineItem -IshObject $ishObject2 -LogicalId "$cmdletName--AA" -Version "2"
		}
		
		It "Remove the same item from two baselines" {
			$ishObjects = Remove-IshBaselineItem -IshSession $ishSession -IshObject @($ishObject1, $ishObject2) -LogicalId "$cmdletName--AA"
			(Get-IshBaselineItem -IshSession $ishSession -IshObject $ishObject1).Count | Should -Be 1
			(Get-IshBaselineItem -IshSession $ishSession -IshObject $ishObject2).Count | Should -Be 1
			$ishObjects.Count | Should -Be 2
		}
		
		It "Remove item existing only in one of baselines" {
			$ishObjects = Remove-IshBaselineItem -IshSession $ishSession -IshObject @($ishObject1, $ishObject2) -LogicalId "$cmdletName--A1"
			(Get-IshBaselineItem -IshSession $ishSession -IshObject $ishObject1).Count | Should -Be 0
			(Get-IshBaselineItem -IshSession $ishSession -IshObject $ishObject2).Count | Should -Be 1
			$ishObjects.Count | Should -Be 2
		}
	}	
}

AfterAll {
	Write-Host ("Running "+$cmdletName+" Test Data and Variables cleanup")
	$baselines = Find-IshBaseline -IshSession $ishSession -MetadataFilter (Set-IshMetadataFilterField -IshSession $ishSession -Name "NAME" -FilterOperator like -Value "$cmdletName%")
	try { Remove-IshBaseline -IshSession $ishSession -IshObject $baselines } catch { }
}