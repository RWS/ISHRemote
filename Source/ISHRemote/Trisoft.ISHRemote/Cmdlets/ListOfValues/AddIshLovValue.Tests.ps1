Write-Host ("`r`nLoading ISHRemote.PesterSetup.ps1 for MyCommand[" + $MyInvocation.MyCommand + "]...")
. (Join-Path (Split-Path -Parent $MyInvocation.MyCommand.Path) "..\..\ISHRemote.PesterSetup.ps1")
$cmdletName = "Add-IshLovValue"
try {
	
Describe “Add-IshLovValue" -Tags "Create" {
	Write-Host "Initializing Test Data and Variables"

	Context “Add-IshLovValue ParameterGroup" {
		It "Parameter IshSession invalid" {
			{ Add-IshLovValue -IShSession "INVALIDISHSESSION" -LovId $ishLovId -Label "ISHRemote $ishLovId Entry" -Description "ISHRemote $ishLovId Entry Description" -IshLovValue "ISHREMOTE$ishLovId" } | Should Throw
		}
	}

	Context "Add-IshLovValue returns IshLovValue object" {
		$timestamp = Get-Date -Format "yyyyMMddHHmmssfff"
		$label = "ISHRemote $ishLovId Entry $timestamp"
		$description = "ISHRemote::$ishLovId Entry $timestamp Description"
		$lovValueId = ("ISHREMOTE"+$ishLovId+$timestamp)
		$ishLovValue = Add-IshLovValue -IShSession $ishSession -LovId $ishLovId -Label $label -Description $description -LovValueId $lovValueId
		It "GetType().Name" {
			$ishLovValue.GetType().Name | Should BeExactly "IshLovValue"
		}
		It "ishLovValue.IshLovValueRef" {
			$ishLovValue.IshLovValueRef -ge 0 | Should Be $true
		}
		It "ishLovValue.LovId" {
			$ishLovValue.LovId | Should Be "$ishLovId"
		}
		It "ishLovValue.IshRef" {
			$ishLovValue.IshRef | Should be $lovValueId
		}
		It "ishLovValue.Label" {
			$ishLovValue.Label | Should Be $label
		}
		It "ishLovValue.Description" {
			$ishLovValue.Description | Should Be $description
		}
		It "ishLovValue.Active" {
			$ishLovValue.Active | Should Be $True
		}
	}

	Context “Add-IshLovValue IshLovValueGroup" {
		$timestamp = Get-Date -Format "yyyyMMddHHmmssfff"
		$label = "ISHRemote $ishLovId Entry $timestamp"
		$description = "ISHRemote::$ishLovId Entry $timestamp Description"
		$lovValueId = ("ISHREMOTE"+$ishLovId+$timestamp)
		$ishLovValue = Add-IshLovValue -IShSession $ishSession -LovId $ishLovId -Label $label -Description $description -LovValueId $lovValueId
		It "Parameter IshLovValue invalid" {
			{ Add-IshLovValue -IShSession $ishSession -IshLovValue "INVALIDLOVVALUE" } | Should Throw
		}
		It "Parameter IshLovValue Single with optional IshSession" {
			Remove-IshLovValue -LovId $ishLovId -LovValueId $lovValueId
			$ishLovValues = Add-IshLovValue -IshLovValue $ishLovValue
			$ishLovValues.Count | Should Be 1
		}
		<# NotImplemented
		It "Parameter IshLovValue Multiple" {
			$ishFolderEditorTemplate.IshField = $ishFolderEditorTemplate.IshField | Set-IshMetadataField -IshSession $ishSession -Name "FNAME" -Level None -Value "EditorTemplate IshFoldersGroup Parameter IshFolder Multiple"
			$ishFolderFavorites.IshField = $ishFolderFavorites.IshField | Set-IshMetadataField -IshSession $ishSession -Name "FNAME" -Level None -Value "Favorites IshFoldersGroup Parameter IshFolder Multiple"
			$ishFolders = Add-IshFolder -IshSession $ishSession -IshFolder @($ishFolderEditorTemplate,$ishFolderFavorites) -ParentFolderId $ishFolderCmdlet.IshFolderRef
			$ishFolders.Count | Should Be 2
		} #>
		It "Pipeline IshLovValue Single" {
			Remove-IshLovValue -IshSession $ishSession -LovId $ishLovId -LovValueId $lovValueId
			$ishLovValues = $ishLovValue | Add-IshLovValue -IShSession $ishSession
			$ishLovValues.Count | Should Be 1
		}
		<# NotImplemented
		It "Pipeline IshLovValue Multiple" {
			$ishFolderData.IshField = $ishFolderData.IshField | Set-IshMetadataField -IshSession $ishSession -Name "FNAME" -Level None -Value "EditorTemplate IshFoldersGroup Pipeline IshFolder Multiple"
			$ishFolderSystem.IshField = $ishFolderSystem.IshField | Set-IshMetadataField -IshSession $ishSession -Name "FNAME" -Level None -Value "System IshFoldersGroup Pipeline IshFolder Multiple"
			$ishFolders = @($ishFolderData,$ishFolderSystem) | Add-IshFolder -IshSession $ishSession -ParentFolderId $ishFolderCmdlet.IshFolderRef
			$ishFolders.Count | Should Be 2
		} #>
	}
}


} finally {
	Write-Host "Cleaning Test Data and Variables"
	$ishLovValues = Get-IshLovValue -IshSession $ishSession -LovId $ishLovId
	foreach ($ishLovValue in $ishLovValues)
	{
		if ($ishLovValue.Description -like "ISHRemote::$ishLovId*")
		{
			Remove-IshLovValue -IshSession $ishSession -IshLovValue $ishLovValue
		}
	}
}
