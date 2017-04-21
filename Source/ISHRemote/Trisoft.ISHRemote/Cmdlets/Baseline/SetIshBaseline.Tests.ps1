Write-Host ("`r`nLoading ISHRemote.PesterSetup.ps1 for MyCommand[" + $MyInvocation.MyCommand + "]...")
. (Join-Path (Split-Path -Parent $MyInvocation.MyCommand.Path) "..\..\ISHRemote.PesterSetup.ps1")
$cmdletName = "Set-IshBaseline"
try {

Describe “Set-IshBaseline" -Tags "Create" {
	Write-Host "Initializing Test Data and Variables"
	
	Context “Set-IshBaseline ParameterGroup" {
		It "Parameter IshSession invalid" {
			{ Set-IshBaseline -IShSession "INVALIDISHSESSION" -Name "INVALIDBASELINENAME" } | Should Throw
		}
	}

	Context “Set-IshBaseline ParameterGroup" {
		It "GetType().Name" {
			$baselineName = ($cmdletName + " " + (Get-Date -Format "yyyyMMddHHmmssfff") + " Name")
			$ishObject = Add-IshBaseline -IshSession $ishSession -Name $baselineName
			$metadata = Set-IshMetadataField -IshSession $ishSession -Name "FISHDOCUMENTRELEASE" -Level None -Value "$baselineName RENAMED"
			$ishObject = Set-IshBaseline -IshSession $ishSession -Id $ishObject.IshRef -Metadata $metadata
			$ishObject.GetType().Name | Should BeExactly "IshObject"
			$ishObject.Count | Should Be 1
		}
	}

	Context “Set-IshBaseline IshObjectsGroup" {
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
		It "Parameter IshObject invalid" {
			{ Set-IshBaseline -IShSession $ishSession -IshObject "INVALIDBASELINE" } | Should Throw
		}
		It "Parameter IshObject Single" {
			$ishObjectA = $ishObjectA | Set-IshMetadataField -IshSession $ishSession -Name "FISHDOCUMENTRELEASE" -Level None -Value ($cmdletName + " " + (Get-Date -Format "yyyyMMddHHmmssfff") + " A Parameter IshObject Single RENAMED")
			$ishObjects = Set-IshBaseline -IshSession $ishSession -IshObject $ishObjectA
			$ishObjects.Count | Should Be 1
		}
		It "Parameter IshObject Multiple" {
			$ishObjectB = $ishObjectB | Set-IshMetadataField -IshSession $ishSession -Name "FISHDOCUMENTRELEASE" -Level None -Value ($cmdletName + " " + (Get-Date -Format "yyyyMMddHHmmssfff") + " B Parameter IshObject Multiple RENAMED")
			$ishObjectC = $ishObjectC | Set-IshMetadataField -IshSession $ishSession -Name "FISHDOCUMENTRELEASE" -Level None -Value ($cmdletName + " " + (Get-Date -Format "yyyyMMddHHmmssfff") + " C Parameter IshObject Multiple RENAMED")
			$ishObjects = Set-IshBaseline -IshSession $ishSession -IshObject @($ishObjectB,$ishObjectC)
			$ishObjects.Count | Should Be 2
		}
		It "Pipeline IshObject Single" {
			$ishObjectD = $ishObjectD | Set-IshMetadataField -IshSession $ishSession -Name "FISHDOCUMENTRELEASE" -Level None -Value ($cmdletName + " " + (Get-Date -Format "yyyyMMddHHmmssfff") + " D Pipeline IshObject Single RENAMED")
			$ishObjects = $ishObjectD | Set-IshBaseline -IshSession $ishSession
			$ishObjects.Count | Should Be 1
		}
		It "Pipeline IshObject Multiple" {
			$ishObjectE = $ishObjectE | Set-IshMetadataField -IshSession $ishSession -Name "FISHDOCUMENTRELEASE" -Level None -Value ($cmdletName + " " + (Get-Date -Format "yyyyMMddHHmmssfff") + " E Pipeline IshObject Multiple RENAMED")
			$ishObjectF = $ishObjectF | Set-IshMetadataField -IshSession $ishSession -Name "FISHDOCUMENTRELEASE" -Level None -Value ($cmdletName + " " + (Get-Date -Format "yyyyMMddHHmmssfff") + " F Pipeline IshObject Multiple RENAMED")
			$ishObjects = @($ishObjectE,$ishObjectF) | Set-IshBaseline -IshSession $ishSession
			$ishObjects.Count | Should Be 2
		}
	}
}


} finally {
	Write-Host "Cleaning Test Data and Variables"
	$baselines = Find-IshBaseline -IshSession $ishSession -MetadataFilter (Set-IshMetadataFilterField -IshSession $ishSession -Name "NAME" -FilterOperator like -Value "$cmdletName%")
	try { Remove-IshBaseline -IshSession $ishSession -IshObject $baselines } catch { }
}
