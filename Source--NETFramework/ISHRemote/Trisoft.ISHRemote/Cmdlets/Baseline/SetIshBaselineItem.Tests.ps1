Write-Host ("`r`nLoading ISHRemote.PesterSetup.ps1 for MyCommand[" + $MyInvocation.MyCommand + "]...")
. (Join-Path (Split-Path -Parent $MyInvocation.MyCommand.Path) "..\..\ISHRemote.PesterSetup.ps1")
$cmdletName = "Set-IshBaselineItem"
try {

Describe "Set-IshBaselineItem" {
	Write-Host "Initializing Test Data and Variables"

	Context "Set-IshBaselineItem ParameterGroup" {
		It "Parameter IshSession invalid" {
			{ Set-IshBaselineItem -IShSession "INVALIDISHSESSION" } | Should Throw
		}
	}

	Context "Set-IshBaselineItem returns IshObject" {
		$baselineName = ($cmdletName + " " + (Get-Date -Format "yyyyMMddHHmmssfff") + " A")
		$ishObject = Add-IshBaseline -IshSession $ishSession -Name $baselineName
		$ishObject = Set-IshBaselineItem -IshObject $ishObject -LogicalId "$cmdletName--A" -Version "1"   # new
		$ishObject = Set-IshBaselineItem -IshObject $ishObject -LogicalId "$cmdletName--AA" -Version "2"  # new
		$ishObject = Set-IshBaselineItem -IshObject $ishObject -LogicalId "$cmdletName--AAA" -Version "3" # new
		It "GetType()" {
			$ishObject.GetType().Name | Should BeExactly "IshBaseline"
		}
		It "$ishObject.IshRef" {
			$ishObject.IshRef | Should Not BeNullOrEmpty
		}
		It "3 News" {
			(Get-IshBaselineItem -IshSession $ishSession -IshObject $ishObject).Count | Should Be 3
		}
		It "3 News and 1 Update" {
			Set-IshBaselineItem -IshSession $ishSession -IshObject $ishObject -LogicalId "$cmdletName--A" -Version "4" # update
			(Get-IshBaselineItem -IshSession $ishSession -IshObject $ishObject).Count | Should Be 3
		}
		It "3 News and 1 Update and 2 Removes" {
			Remove-IshBaselineItem -IshSession $ishSession -IshObject $ishObject -LogicalId "$cmdletName--A" # remove
			Remove-IshBaselineItem -IshSession $ishSession -IshObject $ishObject -LogicalId "$cmdletName--AA" # remove
			(Get-IshBaselineItem -IshSession $ishSession -IshObject $ishObject).Count | Should Be 1
		}
	}

	Context "Set-IshBaseline IshBaselineItemsGroup" {
		$baselineName = ($cmdletName + " " + (Get-Date -Format "yyyyMMddHHmmssfff") + " Single")
		$ishObjectSingle = Add-IshBaseline -IshSession $ishSession -Name $baselineName
		$ishObjectSingle = Set-IshBaselineItem -IshSession $ishSession -IshObject $ishObjectSingle -LogicalId "$cmdletName--Single" -Version "1"
		$ishObjectItemSingle = Get-IshBaselineItem -IshSession $ishSession -IshObject $ishObjectSingle
		$baselineName = ($cmdletName + " " + (Get-Date -Format "yyyyMMddHHmmssfff") + " Multiple")
		$ishObjectMultiple = Add-IshBaseline -IshSession $ishSession -Name $baselineName
		$ishObjectMultiple = Set-IshBaselineItem -IshSession $ishSession -IshObject $ishObjectMultiple -LogicalId "$cmdletName--Multiple" -Version "1"
		$ishObjectMultiple = Set-IshBaselineItem -IshSession $ishSession -IshObject $ishObjectMultiple -LogicalId "$cmdletName--MultipleMultiple" -Version "2"
		$ishObjectItemMultiple = Get-IshBaselineItem -IshSession $ishSession -IshObject $ishObjectMultiple

		It "Setup IshBaselineItem Test Single GetType()" {
			($ishObjectItemSingle).GetType().Name | Should BeExactly "IshBaselineItem"
		}
		It "Setup IshBaselineItem Test Multiple GetType()" {
			($ishObjectItemMultiple).GetType().Name | Should BeExactly "Object[]"
		}
		It "Parameter IshBaselineItem invalid" {
			{ Set-IshBaselineItem -IShSession $ishSession -IshBaselineItem "INVALIDBASELINE" } | Should Throw
		}
		It "Parameter IshBaselineItem Single" {
			$baselineName = ($cmdletName + " " + (Get-Date -Format "yyyyMMddHHmmssfff") + " A")
			$ishObjectA = Add-IshBaseline -IshSession $ishSession -Name $baselineName
			$ishObjectA = Set-IshBaselineItem -IshSession $ishSession -IshObject $ishObjectA -IshBaselineItem $ishObjectItemSingle
			(Get-IshBaselineItem -IshSession $ishSession -IshObject $ishObjectA).Count | Should Be 1
		}
		It "Parameter IshBaselineItem Multiple" {
			$baselineName = ($cmdletName + " " + (Get-Date -Format "yyyyMMddHHmmssfff") + " B")
			$ishObjectB = Add-IshBaseline -IshSession $ishSession -Name $baselineName
			$ishObjectB = Set-IshBaselineItem -IshSession $ishSession -IshObject $ishObjectB -IshBaselineItem $ishObjectItemMultiple
			(Get-IshBaselineItem -IshSession $ishSession -IshObject $ishObjectB).Count | Should Be 2
		}
		It "Pipeline IshBaselineItem Single" {
			$baselineName = ($cmdletName + " " + (Get-Date -Format "yyyyMMddHHmmssfff") + " C")
			$ishObjectC = Add-IshBaseline -IshSession $ishSession -Name $baselineName
			$ishObjectC = $ishObjectItemSingle | Set-IshBaselineItem -IshSession $ishSession -IshObject $ishObjectC
			(Get-IshBaselineItem -IshSession $ishSession -IshObject $ishObjectC).Count | Should Be 1
		}
		It "Pipeline IshBaselineItem Multiple" {
			$baselineName = ($cmdletName + " " + (Get-Date -Format "yyyyMMddHHmmssfff") + " D")
			$ishObjectD = Add-IshBaseline -IshSession $ishSession -Name $baselineName
			$ishObjectD = $ishObjectItemMultiple | Set-IshBaselineItem -IshSession $ishSession -IshObject $ishObjectD
			(Get-IshBaselineItem -IshSession $ishSession -IshObject $ishObjectD).Count | Should Be 2
		}
	}
}


} finally {
	Write-Host "Cleaning Test Data and Variables"
	$baselines = Find-IshBaseline -IshSession $ishSession -MetadataFilter (Set-IshMetadataFilterField -IshSession $ishSession -Name "FISHDOCUMENTRELEASE" -FilterOperator like -Value "$cmdletName%")
	try { Remove-IshBaseline -IshSession $ishSession -IshObject $baselines } catch { }
}
