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
		$ishObject = Set-IshBaselineItem -IshSession $ishSession -IshObject $ishObject -LogicalId "$cmdletName--A" -Version "1"   # new
		$ishObject = Set-IshBaselineItem -IshSession $ishSession -IshObject $ishObject -LogicalId "$cmdletName--AA" -Version "2"  # new
		$ishObject = Set-IshBaselineItem -IshSession $ishSession -IshObject $ishObject -LogicalId "$cmdletName--AAA" -Version "3" # new
		It "GetType()" {
			$ishObject.GetType().Name | Should BeExactly "IshObject"
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
}


} finally {
	Write-Host "Cleaning Test Data and Variables"
	$baselines = Find-IshBaseline -IshSession $ishSession -MetadataFilter (Set-IshMetadataFilterField -IshSession $ishSession -Name "NAME" -FilterOperator like -Value "$cmdletName%")
	try { Remove-IshBaseline -IshSession $ishSession -IshObject $baselines } catch { }
}
