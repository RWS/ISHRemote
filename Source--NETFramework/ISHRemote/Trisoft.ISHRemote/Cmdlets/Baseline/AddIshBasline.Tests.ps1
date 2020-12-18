Write-Host ("`r`nLoading ISHRemote.PesterSetup.ps1 for MyCommand[" + $MyInvocation.MyCommand + "]...")
. (Join-Path (Split-Path -Parent $MyInvocation.MyCommand.Path) "..\..\ISHRemote.PesterSetup.ps1")
$cmdletName = "Add-IshBaseline"
try {

Describe “Add-IshBaseline" -Tags "Create" {
	Write-Host "Initializing Test Data and Variables"
	
	Context “Add-IshBaseline ParameterGroup" {
		It "Parameter IshSession invalid" {
			{ Add-IshBaseline -IShSession "INVALIDISHSESSION" -Name "INVALIDBASELINENAME" } | Should Throw
		}
	}

	Context “Add-IshBaseline ParameterGroup" {
		It "GetType().Name" {
			$baselineName = ($cmdletName + " " + (Get-Date -Format "yyyyMMddHHmmssfff") + " Name")
			$ishObject = Add-IshBaseline -Name $baselineName
			(Get-IshMetadataField -IshObject $ishObject -Level None -Name "FISHDOCUMENTRELEASE").Length -gt 0 | Should Be $true
			$ishObject.GetType().Name | Should BeExactly "IshBaseline"
			$ishObject.Count | Should Be 1
		}
		It "GetType().Name with optional IshSession" {
			$baselineName = ($cmdletName + " " + (Get-Date -Format "yyyyMMddHHmmssfff") + " Name")
			$ishObject = Add-IshBaseline -IshSession $ishSession -Name $baselineName
			(Get-IshMetadataField -IshSession $ishSession -IshObject $ishObject -Level None -Name "FISHDOCUMENTRELEASE").Length -gt 0 | Should Be $true
			$ishObject.GetType().Name | Should BeExactly "IshBaseline"
			$ishObject.Count | Should Be 1
			$ishSession.DefaultRequestedMetadata | Should Be "Basic"
			$ishObject.fishdocumentrelease.Length -ge 1 | Should Be $true 
			$ishObject.fishdocumentrelease_none_element.StartsWith('GUID') | Should Be $true 
		}
	}

	Context “Add-IshBaseline IshObjectsGroup" {
		$baselineName = ($cmdletName + " " + (Get-Date -Format "yyyyMMddHHmmssfff") + " A")
		$ishObjectA = Add-IshBaseline -IshSession $ishSession -Name $baselineName | 
		              Get-IshBaseline -IshSession $ishSession -RequestedMetadata (Set-IshRequestedMetadataField -IshSession $ishSession -Name "NAME")
		$baselineName = ($cmdletName + " " + (Get-Date -Format "yyyyMMddHHmmssfff") + " B")
		$ishObjectB = Add-IshBaseline -IshSession $ishSession -Name $baselineName | 
		              Get-IshBaseline -IshSession $ishSession -RequestedMetadata (Set-IshRequestedMetadataField -IshSession $ishSession -Name "NAME")
		$baselineName = ($cmdletName + " " + (Get-Date -Format "yyyyMMddHHmmssfff") + " C")
		$ishObjectC = Add-IshBaseline -IshSession $ishSession -Name $baselineName | 
		              Get-IshBaseline -IshSession $ishSession -RequestedMetadata (Set-IshRequestedMetadataField -IshSession $ishSession -Name "NAME")
		$baselineName = ($cmdletName + " " + (Get-Date -Format "yyyyMMddHHmmssfff") + " D")
		$ishObjectD = Add-IshBaseline -IshSession $ishSession -Name $baselineName | 
		              Get-IshBaseline -IshSession $ishSession -RequestedMetadata (Set-IshRequestedMetadataField -IshSession $ishSession -Name "NAME")
		$baselineName = ($cmdletName + " " + (Get-Date -Format "yyyyMMddHHmmssfff") + " E")
		$ishObjectE = Add-IshBaseline -IshSession $ishSession -Name $baselineName | 
		              Get-IshBaseline -IshSession $ishSession -RequestedMetadata (Set-IshRequestedMetadataField -IshSession $ishSession -Name "NAME")
		$baselineName = ($cmdletName + " " + (Get-Date -Format "yyyyMMddHHmmssfff") + " F")
		$ishObjectF = Add-IshBaseline -IshSession $ishSession -Name $baselineName | 
		              Get-IshBaseline -IshSession $ishSession -RequestedMetadata (Set-IshRequestedMetadataField -IshSession $ishSession -Name "NAME")
		Remove-IshBaseline -IshSession $ishSession -IshObject @($ishObjectA,$ishObjectB,$ishObjectC,$ishObjectD,$ishObjectE,$ishObjectF)
		Start-Sleep -Milliseconds 1000  # Avoids uniquesness error which only up to the second " Cannot insert duplicate key row in object 'dbo.CARD' with unique index 'CARD_NAME_I1'. The duplicate key value is (...A12/10/2016 16:47:16)."
		It "Parameter IshObject invalid" {
			{ Add-IshBaseline -IShSession $ishSession -IshObject "INVALIDBASELINE" } | Should Throw
		}
		It "Parameter IshObject Single" {
			$ishObjects = Add-IshBaseline -IshSession $ishSession -IshObject $ishObjectA
			$ishObjects | Remove-IshBaseline -IshSession $ishSession
			$ishObjects.Count | Should Be 1
		}
		It "Parameter IshObject Multiple" {
			$ishObjects = Add-IshBaseline -IshSession $ishSession -IshObject @($ishObjectB,$ishObjectC)
			$ishObjects | Remove-IshBaseline -IshSession $ishSession
			$ishObjects.Count | Should Be 2
		}
		It "Pipeline IshObject Single" {
			$ishObjects = $ishObjectD | Add-IshBaseline -IshSession $ishSession
			$ishObjects | Remove-IshBaseline -IshSession $ishSession
			$ishObjects.Count | Should Be 1
		}
		It "Pipeline IshObject Multiple" {
			$ishObjects = @($ishObjectE,$ishObjectF) | Add-IshBaseline -IshSession $ishSession
			$ishObjects | Remove-IshBaseline -IshSession $ishSession
			$ishObjects.Count | Should Be 2
		}
	}
}


} finally {
	Write-Host "Cleaning Test Data and Variables"
	$baselines = Find-IshBaseline -IshSession $ishSession -MetadataFilter (Set-IshMetadataFilterField -IshSession $ishSession -Name "NAME" -FilterOperator like -Value "$cmdletName%")
	try { Remove-IshBaseline -IshSession $ishSession -IshObject $baselines } catch { }
}
