BeforeAll {
	$cmdletName = "Get-IshMetadataField"
	Write-Host ("`r`nLoading ISHRemote.PesterSetup.ps1 on PSVersion[" + $psversionTable.PSVersion + "] over BeforeAll-block for MyCommand[" + $cmdletName + "]...")
	. (Join-Path (Split-Path -Parent $PSCommandPath) "\..\..\ISHRemote.PesterSetup.ps1")

	Write-Host ("Running "+$cmdletName+" Test Data and Variables initialization")
}

Describe "Get-IshMetadataField" -Tags "Read" {
	BeforeAll {
		# Helper function that iteratively tries to find a reduced set of BackgroundTask entries depending on availability 
		# Get one, start recent, expand from minutes to hour to day until you have one, similar to GetIshEvents
		function GetBackgroundTasks {
			param (
				$ishSession,
				$userFilter = "All",
				$requestedMetadata = $null
			)
			if ($null -eq $requestedMetadata) { $ishBackgroundTasks = Get-IshBackgroundTask -IshSession $ishSession -ModifiedSince ((Get-Date).AddSeconds(-10)) -UserFilter $userFilter }
			if ($null -ne $requestedMetadata) { $ishBackgroundTasks = Get-IshBackgroundTask -IshSession $ishSession -ModifiedSince ((Get-Date).AddSeconds(-10)) -UserFilter $userFilter -RequestedMetadata $requestedMetadata }
			if ($ishBackgroundTasks.Count -eq 0) {
				if ($null -eq $requestedMetadata) { $ishBackgroundTasks = Get-IshBackgroundTask -IshSession $ishSession -ModifiedSince ((Get-Date).AddSeconds(-30)) -UserFilter $userFilter }
				if ($null -ne $requestedMetadata) { $ishBackgroundTasks = Get-IshBackgroundTask -IshSession $ishSession -ModifiedSince ((Get-Date).AddSeconds(-30)) -UserFilter $userFilter -RequestedMetadata $requestedMetadata }
			}
			if ($ishBackgroundTasks.Count -eq 0) {
				if ($null -eq $requestedMetadata) { $ishBackgroundTasks = Get-IshBackgroundTask -IshSession $ishSession -ModifiedSince ((Get-Date).AddMinutes(-2)) -UserFilter $userFilter }
				if ($null -ne $requestedMetadata) { $ishBackgroundTasks = Get-IshBackgroundTask -IshSession $ishSession -ModifiedSince ((Get-Date).AddMinutes(-2)) -UserFilter $userFilter -RequestedMetadata $requestedMetadata }
			}
			if ($ishBackgroundTasks.Count -eq 0) {
				if ($null -eq $requestedMetadata) { $ishBackgroundTasks = Get-IshBackgroundTask -IshSession $ishSession -ModifiedSince ((Get-Date).AddMinutes(-10)) -UserFilter $userFilter }
				if ($null -ne $requestedMetadata) { $ishBackgroundTasks = Get-IshBackgroundTask -IshSession $ishSession -ModifiedSince ((Get-Date).AddMinutes(-10)) -UserFilter $userFilter -RequestedMetadata $requestedMetadata }
			}
			if ($ishBackgroundTasks.Count -eq 0) {
				if ($null -eq $requestedMetadata) { $ishBackgroundTasks = Get-IshBackgroundTask -IshSession $ishSession -ModifiedSince ((Get-Date).AddHours(-2)) -UserFilter $userFilter }
				if ($null -ne $requestedMetadata) { $ishBackgroundTasks = Get-IshBackgroundTask -IshSession $ishSession -ModifiedSince ((Get-Date).AddHours(-2)) -UserFilter $userFilter -RequestedMetadata $requestedMetadata }
			}
			if ($ishBackgroundTasks.Count -eq 0) {
				if ($null -eq $requestedMetadata) { $ishBackgroundTasks = Get-IshBackgroundTask -IshSession $ishSession -ModifiedSince ((Get-Date).AddHours(-24)) -UserFilter $userFilter }
				if ($null -ne $requestedMetadata) { $ishBackgroundTasks = Get-IshBackgroundTask -IshSession $ishSession -ModifiedSince ((Get-Date).AddHours(-24)) -UserFilter $userFilter -RequestedMetadata $requestedMetadata }
			}
			if ($ishBackgroundTasks.Count -eq 0) {
				if ($null -eq $requestedMetadata) { $ishBackgroundTasks = Get-IshBackgroundTask -IshSession $ishSession -ModifiedSince ((Get-Date).AddHours(-120)) -UserFilter $userFilter }
				if ($null -ne $requestedMetadata) { $ishBackgroundTasks = Get-IshBackgroundTask -IshSession $ishSession -ModifiedSince ((Get-Date).AddHours(-120)) -UserFilter $userFilter -RequestedMetadata $requestedMetadata }
			}
			return $ishBackgroundTasks
		}
		# Helper function that iteratively tries to find a reduced set of Event entries depending on availability 
		# Get one, start recent, expand from minutes to hour to day until you have one, similar to GetIshBackgroundTasks
		function GetIshEvents {
			param (
				$ishSession,
				$userFilter = "All",
				$requestedMetadata = $null
			)
			if ($null -eq $requestedMetadata) { $ishEvents = Get-IshEvent -IshSession $ishSession -ModifiedSince ((Get-Date).AddSeconds(-10)) -UserFilter $userFilter }
			if ($null -ne $requestedMetadata) { $ishEvents = Get-IshEvent -IshSession $ishSession -ModifiedSince ((Get-Date).AddSeconds(-10)) -UserFilter $userFilter -RequestedMetadata $requestedMetadata }
			if ($ishEvents.Count -eq 0) {
				if ($null -eq $requestedMetadata) { $ishEvents = Get-IshEvent -IshSession $ishSession -ModifiedSince ((Get-Date).AddSeconds(-30)) -UserFilter $userFilter }
				if ($null -ne $requestedMetadata) { $ishEvents = Get-IshEvent -IshSession $ishSession -ModifiedSince ((Get-Date).AddSeconds(-30)) -UserFilter $userFilter -RequestedMetadata $requestedMetadata }
			}
			if ($ishEvents.Count -eq 0) {
				if ($null -eq $requestedMetadata) { $ishEvents = Get-IshEvent -IshSession $ishSession -ModifiedSince ((Get-Date).AddMinutes(-2)) -UserFilter $userFilter }
				if ($null -ne $requestedMetadata) { $ishEvents = Get-IshEvent -IshSession $ishSession -ModifiedSince ((Get-Date).AddMinutes(-2)) -UserFilter $userFilter -RequestedMetadata $requestedMetadata }
			}
			if ($ishEvents.Count -eq 0) {
				if ($null -eq $requestedMetadata) { $ishEvents = Get-IshEvent -IshSession $ishSession -ModifiedSince ((Get-Date).AddMinutes(-10)) -UserFilter $userFilter }
				if ($null -ne $requestedMetadata) { $ishEvents = Get-IshEvent -IshSession $ishSession -ModifiedSince ((Get-Date).AddMinutes(-10)) -UserFilter $userFilter -RequestedMetadata $requestedMetadata }
			}
			if ($ishEvents.Count -eq 0) {
				if ($null -eq $requestedMetadata) { $ishEvents = Get-IshEvent -IshSession $ishSession -ModifiedSince ((Get-Date).AddHours(-2)) -UserFilter $userFilter }
				if ($null -ne $requestedMetadata) { $ishEvents = Get-IshEvent -IshSession $ishSession -ModifiedSince ((Get-Date).AddHours(-2)) -UserFilter $userFilter -RequestedMetadata $requestedMetadata }
			}
			if ($ishEvents.Count -eq 0) {
				if ($null -eq $requestedMetadata) { $ishEvents = Get-IshEvent -IshSession $ishSession -ModifiedSince ((Get-Date).AddHours(-24)) -UserFilter $userFilter }
				if ($null -ne $requestedMetadata) { $ishEvents = Get-IshEvent -IshSession $ishSession -ModifiedSince ((Get-Date).AddHours(-24)) -UserFilter $userFilter -RequestedMetadata $requestedMetadata }
			}
			if ($ishEvents.Count -eq 0) {
				if ($null -eq $requestedMetadata) { $ishEvents = Get-IshEvent -IshSession $ishSession -ModifiedSince ((Get-Date).AddHours(-120)) -UserFilter $userFilter }
				if ($null -ne $requestedMetadata) { $ishEvents = Get-IshEvent -IshSession $ishSession -ModifiedSince ((Get-Date).AddHours(-120)) -UserFilter $userFilter -RequestedMetadata $requestedMetadata }
			}
			return $ishEvents
		}

		$requestedMetadata = Set-IshRequestedMetadataField -IshSession $ishSession -Name "FNAME" |
							Set-IshRequestedMetadataField -IshSession $ishSession -Name "FDOCUMENTTYPE" |
							Set-IshRequestedMetadataField -IshSession $ishSession -Name "READ-ACCESS" -ValueType Element
		$ishFolderDataOriginal = Get-IshFolder -IShSession $ishSession -BaseFolder Data -RequestedMetadata $requestedMetadata
		$ishFolderSystemOriginal = Get-IshFolder -IShSession $ishSession -BaseFolder System -RequestedMetadata $requestedMetadata
		$ishFolderFavoritesOriginal = Get-IshFolder -IShSession $ishSession -BaseFolder Favorites -RequestedMetadata $requestedMetadata
		$ishFolderEditorTemplateOriginal = Get-IshFolder -IShSession $ishSession -BaseFolder EditorTemplate -RequestedMetadata $requestedMetadata
	}
	Context "Get-IshMetadataField -IshSession ishSession returns String object" {
		It "GetType().Name" {
			$ishFolderData = Get-IshMetadataField -IshSession $ishSession -Name "FNAME" -IshField $ishFolderDataOriginal.IshField
			$ishFolderData.GetType().Name | Should -BeExactly "String"
		}
	}
	Context "Get-IshMetadataField -IshSession ishSession IshFieldsGroup" {
		It "Parameter IshSession/Name invalid" {
			{ Get-IshMetadataField -IshSession "INVALIDISHSESSION" -Name "INVALIDFIELDNAME" } | Should -Throw
		}
		It "Parameter Name invalid" {
			{ Get-IshMetadataField -IshSession $ishSession -Name "INVALIDFIELDNAME" } | Should -Throw
		}
		It "Parameter Level invalid" {
			{ Get-IshMetadataField -IshSession $ishSession -Name "INVALIDFIELDNAME" -Level "INVALIDFIELDLEVEL" } | Should -Throw
		}
		It "Parameter ValueType invalid" {
			{ Get-IshMetadataField -IshSession $ishSession -Name "INVALIDFIELDNAME" -Level "INVALIDFIELDLEVEL" -ValueType "INVALIDFIELDVALUETYPE" } | Should -Throw
		}
		It "Parameter IshFolder FNAME" {
			Get-IshMetadataField -IshSession $ishSession -Name "FNAME" -IshField $ishFolderSystemOriginal.IshField | Should -Not -BeNullOrEmpty
		}
		It "Parameter IshFolder FDOCUMENTTYPE" {
			Get-IshMetadataField -IshSession $ishSession -Name "FDOCUMENTTYPE" -Level None -IshField $ishFolderSystemOriginal.IshField | Should -Be "None"
		}
		It "Parameter IshFolder READ-ACCESS with implicit IshSession" {
			Get-IshMetadataField -Name "READ-ACCESS" -Level None -IshField $ishFolderSystemOriginal.IshField -ValueType Element | Should -BeNullOrEmpty
		}
	}
	Context "Get-IshMetadataField -IshSession ishSession IshObjectGroup" {
		It "Parameter IshObject invalid" {
			{ Get-IshMetadataField -IshSession $ishSession -Name "FNAME" -IshObject "INVALIDOBJECT" } | Should -Throw
		}
		It "Parameter IshObject Single" {
			$ishUser = Get-IshUser -IshSession $ishSession -RequestedMetadata (Set-IshRequestedMetadataField -IshSession $ishSession -Name "USERNAME")
			(Get-IshMetadataField -IshSession $ishSession -Name "FNAME" -IshObject $ishUser).Count | Should -Be 1
		}
	}
	Context "Get-IshMetadataField -IshSession ishSession IshFolderGroup" {
		It "Parameter IshFolder invalid" {
			{ Get-IshMetadataField -IshSession $ishSession -Name "FNAME" -IshFolder "INVALIDFOLDER" } | Should -Throw
		}
		It "Parameter IshFolder Single" {
			(Get-IshMetadataField -IshSession $ishSession -Name "FNAME" -IshFolder $ishFolderDataOriginal).Count | Should -Be 1
		}
		It "Parameter IshFolder Multiple" {
			(Get-IshMetadataField -IshSession $ishSession -Name "FNAME" -IshFolder @($ishFolderDataOriginal,$ishFolderFavoritesOriginal)).Count | Should -Be 2
		}
		It "Pipeline IshFolder" {
			(@($ishFolderDataOriginal) | Get-IshMetadataField -IshSession $ishSession -Name "FNAME").Count | Should -Be 1
		}
		It "Pipeline IshFolder Multiple" {
			(@($ishFolderDataOriginal,$ishFolderFavoritesOriginal) | Get-IshMetadataField -IshSession $ishSession -Name "FNAME").Count | Should -Be 2
		}
	}
	Context "Get-IshMetadataField -IshSession ishSession IshEventGroup" {
		It "Parameter IshEvent invalid" {
			{ Get-IshMetadataField -IshSession $ishSession -Name "FNAME" -IshEvent "INVALIDEVENT" } | Should -Throw
		}
		It "Pipeline IshEvent Multiple" {
			#TODO test possibly is slow (or fails) when there are no EventMonitor entries
			(GetIshEvents -IshSession $ishSession | Get-IshMetadataField -IshSession $ishSession -Name "EVENTTYPE" -Level Progress).Count -ge 0 | Should -Be $true
		}
	}
	Context "Get-IshMetadataField -IshSession ishSession IshBackgroundTask" {
		It "Parameter IshBackgroundTask invalid" {
			{ Get-IshMetadataField -IshSession $ishSession -Name "FNAME" -IshBackgroundTask "INVALIDBACKGROUNDTASK" } | Should -Throw
		}
		It "Pipeline IshBackgroundTask Multiple" {
			((GetBackgroundTasks -ishSession $ishSession -userFilter All)[0] | Get-IshMetadataField -IshSession $ishSession -Name "EVENTTYPE" -Level Task).Count -ge 0 | Should -Be $true
		}
	}
	Context "Get-IshMetadataField IshFields.ToXml() for API Testing" {
		It "Trisoft.ISHRemote.Objects.IShFields is public" {
			{ New-Object -TypeName Trisoft.ISHRemote.Objects.IShFields } | Should -Not -Throw
		}
		It "Trisoft.ISHRemote.Objects.IShFields.AddField() is public" {
			{ (New-Object -TypeName Trisoft.ISHRemote.Objects.IShFields).AddField((Set-IshRequestedMetadataField -IshSession $ishSession -Name "USERNAME")).ToXml() } | Should -Not -Throw
		}
		It "Trisoft.ISHRemote.Objects.IShFields.ToXml() xml content single" {
			$ishFields = New-Object -TypeName Trisoft.ISHRemote.Objects.IShFields
			$ishFields.AddField((Set-IshRequestedMetadataField -IshSession $ishSession -Name "USERNAME"))
			$result = '<?xml version="1.0" encoding="utf-16"?><ishfields><ishfield name="USERNAME" level="none" ishvaluetype="value" /></ishfields>'
			$ishFields.ToXML() -eq $result | Should -Be $true
		}
		It "Trisoft.ISHRemote.Objects.IShFields.ToXml() xml content multiple" {
			$ishFields = New-Object -TypeName Trisoft.ISHRemote.Objects.IShFields
			$ishFields.AddField((Set-IshRequestedMetadataField -IshSession $ishSession -Name "USERNAME"))
			$ishFields.AddField((Set-IshRequestedMetadataField -IshSession $ishSession -Name "FUSERGROUP"))
			$result = '<?xml version="1.0" encoding="utf-16"?><ishfields><ishfield name="USERNAME" level="none" ishvaluetype="value" /><ishfield name="FUSERGROUP" level="none" ishvaluetype="value" /></ishfields>'
			$ishFields.ToXML() -eq $result | Should -Be $true
		}
	}
}

AfterAll {
	Write-Host ("Running "+$cmdletName+" Test Data and Variables cleanup")
}