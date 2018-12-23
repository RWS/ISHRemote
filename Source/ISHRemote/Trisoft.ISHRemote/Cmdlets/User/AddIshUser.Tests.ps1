Write-Host ("`r`nLoading ISHRemote.PesterSetup.ps1 for MyCommand[" + $MyInvocation.MyCommand + "]...")
. (Join-Path (Split-Path -Parent $MyInvocation.MyCommand.Path) "..\..\ISHRemote.PesterSetup.ps1")
$cmdletName = "Add-IshUser"
try {

Describe "Add-IshUser" -Tags "Create" {
	Write-Host "Initializing Test Data and Variables"
	
	Context "Add-IshUser ParameterGroup" {
		It "Parameter IshSession invalid" {
			{ Add-IshUser -IShSession "INVALIDISHSESSION" -Name "INVALIDUSERNAME" } | Should Throw
		}
	}

	Context "Add-IshUser ParameterGroup" {
		It "GetType().Name" {
			$userName = ($cmdletName + " " + (Get-Date -Format "yyyyMMddHHmmssfff") + " Name")
			 $metadata = Set-IshMetadataField -IshSession $ishSession -Name FISHUSERLANGUAGE -Level None -ValueType Element -Value "VLANGUAGEEN" |
                         Set-IshMetadataField -IshSession $ishSession -Name FUSERGROUP -Level None -ValueType Element -Value "VUSERGROUPDEFAULTDEPARTMENT" |
                         Set-IshMetadataField -IshSession $ishSession -Name PASSWORD -Level None -Value "SomethingSecret"
			$ishObject = Add-IshUser -IshSession $ishSession -Name $userName -Metadata $metadata
			$ishObject.GetType().Name | Should BeExactly "IshUser"
			$ishObject.Count | Should Be 1
		}
		It "Parameter Metadata" {
			$userName = ($cmdletName + " " + (Get-Date -Format "yyyyMMddHHmmssfff") + " Metadata")
			$metadata = Set-IshMetadataField -IshSession $ishSession -Name FISHUSERLANGUAGE -Level None -ValueType Element -Value "VLANGUAGEEN" |
                        Set-IshMetadataField -IshSession $ishSession -Name FUSERGROUP -Level None -ValueType Element -Value "VUSERGROUPDEFAULTDEPARTMENT" |
                        Set-IshMetadataField -IshSession $ishSession -Name PASSWORD -Level None -Value "SomethingSecret"
			$ishObject = Add-IshUser -IshSession $ishSession -Name $userName -Metadata $metadata
			$ishObject.Count | Should Be 1
			$ishObject.IshRef -Like "VUSER*" | Should Be $true
		}
		It "Parameter Metadata return descriptive metadata" {
			$userName = ($cmdletName + " " + (Get-Date -Format "yyyyMMddHHmmssfff") + " Metadata")
			$metadata = Set-IshMetadataField -IshSession $ishSession -Name FISHUSERLANGUAGE -Level None -ValueType Element -Value "VLANGUAGEEN" |
                        Set-IshMetadataField -IshSession $ishSession -Name FUSERGROUP -Level None -ValueType Element -Value "VUSERGROUPDEFAULTDEPARTMENT" |
                        Set-IshMetadataField -IshSession $ishSession -Name PASSWORD -Level None -Value "SomethingSecret"
			$ishObject = Add-IshUser -IshSession $ishSession -Name $userName -Metadata $metadata
			(Get-IshMetadataField -IshSession $ishSession -IshObject $ishObject -Name FISHUSERLANGUAGE -Level None -ValueType Element).Length -gt 1 | Should Be $true # added user field by element name
			(Get-IshMetadataField -IshSession $ishSession -IshObject $ishObject -Name FISHUSERLANGUAGE -Level None -ValueType Value).Length -gt 1 | Should Be $true # added user field by element name, value added by AddDescriptiveFields
			(Get-IshMetadataField -IshSession $ishSession -IshObject $ishObject -Name FUSERGROUP -Level None -ValueType Element).Length -gt 1 | Should Be $true # added user field by element name
			(Get-IshMetadataField -IshSession $ishSession -IshObject $ishObject -Name FUSERGROUP -Level None -ValueType Value).Length -gt 1 | Should Be $true # added user field by element name, value added by AddDescriptiveFields
			(Get-IshMetadataField -IshSession $ishSession -IshObject $ishObject -Name USERNAME -Level None).Length -gt 1 | Should Be $true
			$ishSession.DefaultRequestedMetadata | Should Be "Basic"
			$ishObject.username.Length -ge 1 | Should Be $true 
			$ishObject.fishusertype.Length -ge 1 | Should Be $true 
			$ishObject.fishusertype_none_element.StartsWith('VUSERTYPE') | Should Be $true 
		}
		It "Parameter Metadata StrictMetadataPreference=Off with INVALIDFIELDNAME" {
			$strictMetadataPreference = $ishSession.StrictMetadataPreference
			$ishSession.StrictMetadataPreference = "Off"
			$userName = ($cmdletName + " " + (Get-Date -Format "yyyyMMddHHmmssfff") + " Metadata")
			$metadata = Set-IshMetadataField -IshSession $ishSession -Name "FISHUSERLANGUAGE" -Level None -ValueType Element -Value "VLANGUAGEEN" |
                        Set-IshMetadataField -IshSession $ishSession -Name "FUSERGROUP" -Level None -ValueType Element -Value "VUSERGROUPDEFAULTDEPARTMENT" |
                        Set-IshMetadataField -IshSession $ishSession -Name "PASSWORD" -Level None -Value "SomethingSecret" |
                        Set-IshMetadataField -IshSession $ishSession -Name "CREATED-ON" -Level None -Value "12/03/2017" | 
                        Set-IshMetadataField -IshSession $ishSession -Name "MODIFIED-ON" -Level None -Value "12/03/2017" |
                        Set-IshMetadataField -IshSession $ishSession -Name "READ-ACCESS" -Level None -Value "SomethingReadAccess"  |
                        Set-IshMetadataField -IshSession $ishSession -Name "OWNER" -Level None -Value "SomethingOwner" |
                        Set-IshMetadataField -IshSession $ishSession -Name "INVALIDFIELDNAME" -Level None -Value "SomethingInvalidFieldName"
			{ Add-IshUser -IshSession $ishSession -Name $userName -Metadata $metadata } | Should Throw
			$ishSession.StrictMetadataPreference = $strictMetadataPreference
		}
		It "Parameter Metadata StrictMetadataPreference=Continue with many system fields" {
			$strictMetadataPreference = $ishSession.StrictMetadataPreference
			$ishSession.StrictMetadataPreference = "Continue"
			$userName = ($cmdletName + " " + (Get-Date -Format "yyyyMMddHHmmssfff") + " Metadata")
			$metadata = Set-IshMetadataField -IshSession $ishSession -Name "FISHUSERLANGUAGE" -Level None -ValueType Element -Value "VLANGUAGEEN" |
                        Set-IshMetadataField -IshSession $ishSession -Name "FUSERGROUP" -Level None -ValueType Element -Value "VUSERGROUPDEFAULTDEPARTMENT" |
                        Set-IshMetadataField -IshSession $ishSession -Name "PASSWORD" -Level None -Value "SomethingSecret" |
                        Set-IshMetadataField -IshSession $ishSession -Name "READ-ACCESS" -Level None -Value "SomethingReadAccess"  |
						Set-IshMetadataField -IshSession $ishSession -Name "MODIFY-ACCESS" -Level None -Value "SomethingReadAccess"  |
						Set-IshMetadataField -IshSession $ishSession -Name "DELETE-ACCESS" -Level None -Value "SomethingReadAccess"  |
                        Set-IshMetadataField -IshSession $ishSession -Name "OWNER" -Level None -Value "SomethingOwner" |
						Set-IshMetadataField -IshSession $ishSession -Name "USERNAME" -Level None -Value "SomethingInvalidFieldName" | # RemoveSystemFields always removed upon Create
						Set-IshMetadataField -IshSession $ishSession -Name "NAME" -Level None -Value "SomethingInvalidFieldName" | # RemoveSystemFields always removed upon Create
						Set-IshMetadataField -IshSession $ishSession -Name "FISHOBJECTACTIVE" -Level None -Value "SomethingInvalidFieldName" | # RemoveSystemFields always removed upon Create
						Set-IshMetadataField -IshSession $ishSession -Name "FISHUSERDISABLED" -Level None -Value "SomethingInvalidFieldName" | # RemoveSystemFields always removed upon Create
						Set-IshMetadataField -IshSession $ishSession -Name "RIGHTS" -Level None -Value "SomethingInvalidFieldName" | # RemoveSystemFields always removed upon Create
						Set-IshMetadataField -IshSession $ishSession -Name "CREATED-ON" -Level None -Value "12/03/2017" | # RemoveSystemFields always removed upon Create
                        Set-IshMetadataField -IshSession $ishSession -Name "MODIFIED-ON" -Level None -Value "12/03/2017" | # RemoveSystemFields always removed upon Create
						Set-IshMetadataField -IshSession $ishSession -Name "FISHPASSWORDMODIFIEDON" -Level None -Value "12/03/2017" | # RemoveSystemFields always removed upon Create since Kojak/13.0.0
						Set-IshMetadataField -IshSession $ishSession -Name "FISHLOCKEDSINCE" -Level None -Value "12/03/2017" | # RemoveSystemFields always removed upon Create since Kojak/13.0.0
						Set-IshMetadataField -IshSession $ishSession -Name "FISHPASSWORDHISTORY" -Level None -Value "NoHistory" | # RemoveSystemFields always removed upon Create since Kojak/13.0.0
						Set-IshMetadataField -IshSession $ishSession -Name "FISHFAILEDATTEMPTS" -Level None -Value "10" | # RemoveSystemFields always removed upon Create since Kojak/13.0.0
						Set-IshMetadataField -IshSession $ishSession -Name "FISHFAVORITES" -Level None -Value "23" # RemoveSystemFields always removed upon Create
			{ Add-IshUser -IshSession $ishSession -Name $userName -Metadata $metadata } | Should Not Throw
			$ishSession.StrictMetadataPreference = $strictMetadataPreference
		}
		It "Parameter Metadata StrictMetadataPreference=Off with many system fields" {
			$strictMetadataPreference = $ishSession.StrictMetadataPreference
			$ishSession.StrictMetadataPreference = "Off"
			$userName = ($cmdletName + " " + (Get-Date -Format "yyyyMMddHHmmssfff") + " Metadata")
			$metadata = Set-IshMetadataField -IshSession $ishSession -Name "FISHUSERLANGUAGE" -Level None -ValueType Element -Value "VLANGUAGEEN" |
                        Set-IshMetadataField -IshSession $ishSession -Name "FUSERGROUP" -Level None -ValueType Element -Value "VUSERGROUPDEFAULTDEPARTMENT" |
                        Set-IshMetadataField -IshSession $ishSession -Name "PASSWORD" -Level None -Value "SomethingSecret" |
                        Set-IshMetadataField -IshSession $ishSession -Name "READ-ACCESS" -Level None -Value "SomethingReadAccess"  |
						Set-IshMetadataField -IshSession $ishSession -Name "MODIFY-ACCESS" -Level None -Value "SomethingReadAccess"  |
						Set-IshMetadataField -IshSession $ishSession -Name "DELETE-ACCESS" -Level None -Value "SomethingReadAccess"  |
                        Set-IshMetadataField -IshSession $ishSession -Name "OWNER" -Level None -Value "SomethingOwner" |
						Set-IshMetadataField -IshSession $ishSession -Name "USERNAME" -Level None -Value "SomethingInvalidFieldName" | # RemoveSystemFields always removed upon Create
						Set-IshMetadataField -IshSession $ishSession -Name "NAME" -Level None -Value "SomethingInvalidFieldName" | # RemoveSystemFields always removed upon Create
						Set-IshMetadataField -IshSession $ishSession -Name "FISHOBJECTACTIVE" -Level None -Value "SomethingInvalidFieldName" | # RemoveSystemFields always removed upon Create
						Set-IshMetadataField -IshSession $ishSession -Name "FISHUSERDISABLED" -Level None -Value "SomethingInvalidFieldName" | # RemoveSystemFields always removed upon Create
						Set-IshMetadataField -IshSession $ishSession -Name "RIGHTS" -Level None -Value "SomethingInvalidFieldName" | # RemoveSystemFields always removed upon Create
						Set-IshMetadataField -IshSession $ishSession -Name "CREATED-ON" -Level None -Value "12/03/2017" | # RemoveSystemFields always removed upon Create
                        Set-IshMetadataField -IshSession $ishSession -Name "MODIFIED-ON" -Level None -Value "12/03/2017" | # RemoveSystemFields always removed upon Create
						Set-IshMetadataField -IshSession $ishSession -Name "FISHFAVORITES" -Level None -Value "23" # RemoveSystemFields always removed upon Create
			if (([Version]$ishSession.ServerVersion).Major -ge 13)
			{
			  $metadata = $metadata |
			              Set-IshMetadataField -IshSession $ishSession -Name "FISHPASSWORDMODIFIEDON" -Level None -Value "12/03/2017" | # RemoveSystemFields always removed upon Create since Kojak/13.0.0
						  Set-IshMetadataField -IshSession $ishSession -Name "FISHLOCKEDSINCE" -Level None -Value "12/03/2017" | # RemoveSystemFields always removed upon Create since Kojak/13.0.0
						  Set-IshMetadataField -IshSession $ishSession -Name "FISHPASSWORDHISTORY" -Level None -Value "NoHistory" | # RemoveSystemFields always removed upon Create since Kojak/13.0.0
						  Set-IshMetadataField -IshSession $ishSession -Name "FISHFAILEDATTEMPTS" -Level None -Value "10"  # RemoveSystemFields always removed upon Create since Kojak/13.0.0
			}
			{ Add-IshUser -IshSession $ishSession -Name $userName -Metadata $metadata } | Should Throw
			$ishSession.StrictMetadataPreference = $strictMetadataPreference
		}
	}

	Context "Add-IshUser IshObjectsGroup" {
	    $metadata = Set-IshMetadataField -IshSession $ishSession -Name FISHUSERLANGUAGE -Level None -ValueType Element -Value "VLANGUAGEEN" |
                    Set-IshMetadataField -IshSession $ishSession -Name FUSERGROUP -Level None -ValueType Element -Value "VUSERGROUPDEFAULTDEPARTMENT" |
                    Set-IshMetadataField -IshSession $ishSession -Name PASSWORD -Level None -Value "SomethingSecret"
		$userName = ($cmdletName + " " + (Get-Date -Format "yyyyMMddHHmmssfff") + " A")
		$ishObjectA = Add-IshUser -IshSession $ishSession -Name $userName -Metadata $metadata
		$userName = ($cmdletName + " " + (Get-Date -Format "yyyyMMddHHmmssfff") + " B")
		$ishObjectB = Add-IshUser -IshSession $ishSession -Name $userName -Metadata $metadata
		$userName = ($cmdletName + " " + (Get-Date -Format "yyyyMMddHHmmssfff") + " C")
		$ishObjectC = Add-IshUser -IshSession $ishSession -Name $userName -Metadata $metadata
		$userName = ($cmdletName + " " + (Get-Date -Format "yyyyMMddHHmmssfff") + " D")
		$ishObjectD = Add-IshUser -IshSession $ishSession -Name $userName -Metadata $metadata
		$userName = ($cmdletName + " " + (Get-Date -Format "yyyyMMddHHmmssfff") + " E")
		$ishObjectE = Add-IshUser -IshSession $ishSession -Name $userName -Metadata $metadata
		$userName = ($cmdletName + " " + (Get-Date -Format "yyyyMMddHHmmssfff") + " F")
		$ishObjectF = Add-IshUser -IshSession $ishSession -Name $userName -Metadata $metadata
		Remove-IshUser -IshSession $ishSession -IshObject @($ishObjectA,$ishObjectB,$ishObjectC,$ishObjectD,$ishObjectE,$ishObjectF)
		Start-Sleep -Milliseconds 1000  # Avoids uniquesness error which only up to the second " Cannot insert duplicate key row in object 'dbo.CARD' with unique index 'CARD_NAME_I1'. The duplicate key value is (VUSERADD-ISHUSER20161012164716068A12/10/2016 16:47:16)."
		It "Parameter IshObject invalid" {
			{ Add-IshUser -IShSession $ishSession -IshObject "INVALIDUSER" } | Should Throw
		}
		It "Parameter IshObject Single with implicit IshSession" {
			$ishObjectA = $ishObjectA | Set-IshMetadataField -Name PASSWORD -Level None -Value "PasswordNotPutOnThePipeline"
		    $ishObjects = Add-IshUser -IshObject $ishObjectA
			$ishObjects | Remove-IshUser
			$ishObjects.Count | Should Be 1
		}
		It "Parameter IshObject Multiple with implicit IshSession" {
		    $ishObjectB = $ishObjectB | Set-IshMetadataField -Name PASSWORD -Level None -Value "PasswordNotPutOnThePipeline"
			$ishObjectC = $ishObjectC | Set-IshMetadataField -Name PASSWORD -Level None -Value "PasswordNotPutOnThePipeline"
			$ishObjects = Add-IshUser -IshObject @($ishObjectB,$ishObjectC)
			$ishObjects | Remove-IshUser
			$ishObjects.Count | Should Be 2
		}
		It "Pipeline IshObject Single" {
		    $ishObjectD = $ishObjectD | Set-IshMetadataField -IshSession $ishSession -Name PASSWORD -Level None -Value "PasswordNotPutOnThePipeline"
			$ishObjects = $ishObjectD | Add-IshUser -IshSession $ishSession
			$ishObjects | Remove-IshUser -IshSession $ishSession
			$ishObjects.Count | Should Be 1
		}
		It "Pipeline IshObject Multiple" {
		    $ishObjectE = $ishObjectE | Set-IshMetadataField -IshSession $ishSession -Name PASSWORD -Level None -Value "PasswordNotPutOnThePipeline"
			$ishObjectF = $ishObjectF | Set-IshMetadataField -IshSession $ishSession -Name PASSWORD -Level None -Value "PasswordNotPutOnThePipeline"
			$ishObjects = @($ishObjectE,$ishObjectF) | Add-IshUser -IshSession $ishSession
			$ishObjects | Remove-IshUser -IshSession $ishSession
			$ishObjects.Count | Should Be 2
		}
	}
}


} finally {
	Write-Host "Cleaning Test Data and Variables"
	$users = Find-IshUser -IshSession $ishSession -MetadataFilter (Set-IshMetadataFilterField -IshSession $ishSession -Name "USERNAME" -FilterOperator like -Value "$cmdletName%")
	try { Remove-IshUser -IshSession $ishSession -IshObject $users } catch { }
}
