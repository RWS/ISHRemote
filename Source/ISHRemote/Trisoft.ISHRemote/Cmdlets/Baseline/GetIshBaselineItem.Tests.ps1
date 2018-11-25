Write-Host ("`r`nLoading ISHRemote.PesterSetup.ps1 for MyCommand[" + $MyInvocation.MyCommand + "]...")
. (Join-Path (Split-Path -Parent $MyInvocation.MyCommand.Path) "..\..\ISHRemote.PesterSetup.ps1")
$cmdletName = "Get-IshBaselineItem"
try {

Describe "Get-IshBaselineItem" -Tags "Read" {
	Write-Host "Initializing Test Data and Variables"

	Context "Get-IshBaselineItem ParameterGroup" {
		It "Parameter IshSession invalid" {
			{ Get-IshBaselineItem -IShSession "INVALIDISHSESSION" } | Should Throw
		}
	}

	Context "Get-IshBaselineItem returns IshBaselineItem object" {
		$baselineName = ($cmdletName + " " + (Get-Date -Format "yyyyMMddHHmmssfff") + " A")
		$ishObject = Add-IshBaseline -IshSession $ishSession -Name $baselineName
		$ishObject = Set-IshBaselineItem -IshSession $ishSession -IshObject $ishObject -LogicalId "$cmdletName--AAA" -Version "1"
		
		It "Parameter IshSession explicit" {
			$ishBaselineItem = (Get-IshBaselineItem -IShSession $ishSession -IshObject $ishObject)[0]
			$ishBaselineItem.GetType().Name | Should BeExactly "IshBaselineItem"
			$ishBaselineItem.IshRef | Should Not BeNullOrEmpty
			$ishBaselineItem.LogicalId | Should Not BeNullOrEmpty
			$ishBaselineItem.Version | Should Not BeNullOrEmpty
			$ishBaselineItem.Author | Should Not BeNullOrEmpty
			$ishBaselineItem.CreatedOn | Should Not BeNullOrEmpty
			$ishBaselineItem.ModifiedOn | Should Not BeNullOrEmpty
		}
		It "Parameter IshSession implicit" {
			$ishBaselineItem = (Get-IshBaselineItem -IshObject $ishObject)[0]
			$ishBaselineItem.GetType().Name | Should BeExactly "IshBaselineItem"
			$ishBaselineItem.IshRef | Should Not BeNullOrEmpty
			$ishBaselineItem.LogicalId | Should Not BeNullOrEmpty
			$ishBaselineItem.Version | Should Not BeNullOrEmpty
			$ishBaselineItem.Author | Should Not BeNullOrEmpty
			$ishBaselineItem.CreatedOn | Should Not BeNullOrEmpty
			$ishBaselineItem.ModifiedOn | Should Not BeNullOrEmpty
		}
	}

	Context "Get-IshBaselineItem IshObjectsGroup" {
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
		It "Parameter IshObject invalid" {
			{ Get-IshBaselineItem -IShSession $ishSession -IshObject "INVALIDBASELINE" } | Should Throw
		}
		It "Parameter IshObject Single" {
			(Get-IshBaselineItem -IshSession $ishSession -IshObject $ishObjectA).Count | Should Be 2
		}
		It "Parameter IshObject Multiple" {
			(Get-IshBaselineItem -IshSession $ishSession -IshObject @($ishObjectB,$ishObjectC)).Count | Should Be 4
		}
		It "Pipeline IshObject Single" {
			$ishBaselineItems = $ishObjectD | Get-IshBaselineItem -IshSession $ishSession
			$ishBaselineItems.Count | Should Be 2
		}
		It "Pipeline IshObject Multiple" {
			$ishBaselineItems = @($ishObjectA,$ishObjectB,$ishObjectC,$ishObjectD) | Get-IshBaselineItem -IshSession $ishSession
			$ishBaselineItems.Count | Should Be 8
		}
	}
}


} finally {
	Write-Host "Cleaning Test Data and Variables"
	$baselines = Find-IshBaseline -IshSession $ishSession -MetadataFilter (Set-IshMetadataFilterField -IshSession $ishSession -Name "NAME" -FilterOperator like -Value "$cmdletName%")
	try { Remove-IshBaseline -IshSession $ishSession -IshObject $baselines } catch { }
}
