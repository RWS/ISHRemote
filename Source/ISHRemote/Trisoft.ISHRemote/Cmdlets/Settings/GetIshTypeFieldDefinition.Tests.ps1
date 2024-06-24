BeforeAll {
	$cmdletName = "Get-IshTypeFieldDefinition"
	Write-Host ("`r`nLoading ISHRemote.PesterSetup.ps1 on PSVersion[" + $psversionTable.PSVersion + "] over BeforeAll-block for MyCommand[" + $cmdletName + "]...")
	. (Join-Path (Split-Path -Parent $PSCommandPath) "\..\..\ISHRemote.PesterSetup.ps1")
	
	Write-Host ("Running "+$cmdletName+" Test Data and Variables initialization")
	$tempFilePath = (New-TemporaryFile).FullName
$tridkXmlSetupUserContent = @"
<?xml version="1.0"?><?xml-stylesheet href="full.export.xsl" type="text/xsl"?><!-- InfoShare Author 3.5.0 --><tridk:setup xml:lang="EN" xmlns:tridk="urn:trisoft.be:Tridk:Setup:1.0" tridk:version="120.11.0.3215"><tridk:cardtypes><!-- General cardtypes --><tridk:cardtype tridk:exportmode="cascade" tridk:element="USER" tridk:metatype="usercard"><tridk:displaydefinition><tridk:label>User</tridk:label><tridk:description/></tridk:displaydefinition><tridk:fielddefinition><tridk:cardtypefield tridk:element="CREATED-ON" tridk:userdefined="no"><tridk:sequence tridk:value="5"/></tridk:cardtypefield><tridk:cardtypefield tridk:element="DELETE-ACCESS" tridk:userdefined="no"><tridk:sequence tridk:value="11"/></tridk:cardtypefield><tridk:cardtypefield tridk:element="FUSERGROUP" tridk:userdefined="yes"><tridk:sequence tridk:value="1"/><tridk:memberdefinition><tridk:member tridk:element="CTUSERGROUP"/></tridk:memberdefinition></tridk:cardtypefield><tridk:cardtypefield tridk:element="NAME" tridk:userdefined="no"><tridk:sequence tridk:value="2"/></tridk:cardtypefield><tridk:cardtypefield tridk:element="OSUSER" tridk:userdefined="no"><tridk:sequence tridk:value="17"/></tridk:cardtypefield><tridk:cardtypefield tridk:element="PASSWORD" tridk:userdefined="no"><tridk:sequence tridk:value="52"/></tridk:cardtypefield><tridk:cardtypefield tridk:element="USERNAME" tridk:userdefined="no"><tridk:sequence tridk:value="3"/></tridk:cardtypefield></tridk:fielddefinition></tridk:cardtype></tridk:cardtypes><tridk:fields><tridk:field tridk:exportmode="cascade" tridk:element="CREATED-ON"><tridk:displaydefinition>	<tridk:label>Creation Date</tridk:label><tridk:description>Used on all cards to indicate the date that the object was created</tridk:description></tridk:displaydefinition><tridk:typedefinition><tridk:typedate><tridk:minnoofvalues tridk:value="1"/><tridk:maxnoofvalues tridk:value="1"/></tridk:typedate></tridk:typedefinition><tridk:public tridk:value="yes"/><tridk:system tridk:value="yes"/><tridk:classdefinition><tridk:class tridk:element="GENERAL"/><tridk:class tridk:element="INDEX"/><tridk:class tridk:element="VCLASSDISPLAYHISTORY"/></tridk:classdefinition></tridk:field><tridk:field tridk:exportmode="cascade" tridk:element="DELETE-ACCESS"><tridk:displaydefinition><tridk:label>Delete Access</tridk:label><tridk:description>User roles required in order to delete the object</tridk:description></tridk:displaydefinition><tridk:typedefinition><tridk:typelov tridk:element="DUSERGROUP"><tridk:minnoofvalues tridk:value="0"/><tridk:maxnoofvalues tridk:value="999999"/></tridk:typelov></tridk:typedefinition><tridk:public tridk:value="yes"/><tridk:system tridk:value="yes"/><tridk:classdefinition><tridk:class tridk:element="SECURITY"/></tridk:classdefinition></tridk:field><tridk:field tridk:exportmode="cascade" tridk:element="FUSERGROUP"><tridk:displaydefinition><tridk:label>Usergroup</tridk:label><tridk:description>Used on all card types. On the USER card type the field indicates that the user has write/modify access to documents of this usergroup. On all other objects the field contains the usergroup that owns the object and can modify the object.</tridk:description></tridk:displaydefinition><tridk:typedefinition><tridk:typecardreference><tridk:memberdefinedoncard tridk:value="no"/><tridk:minnoofvalues tridk:value="0"/><tridk:maxnoofvalues tridk:value="999999"/></tridk:typecardreference></tridk:typedefinition><tridk:public tridk:value="yes"/><tridk:system tridk:value="no"/><tridk:classdefinition><tridk:class tridk:element="NEW"/><tridk:class tridk:element="MODIFY"/><tridk:class tridk:element="INDEX"/><tridk:class tridk:element="DOCUMENT"/></tridk:classdefinition></tridk:field><tridk:field tridk:exportmode="cascade" tridk:element="NAME"><tridk:displaydefinition><tridk:label>Label</tridk:label><tridk:description>Internal system field to hold the unique label of the object within its own object type.</tridk:description></tridk:displaydefinition><tridk:typedefinition><tridk:typelanguagedependentstring><tridk:minnoofvalues tridk:value="1"/><tridk:maxnoofvalues tridk:value="1"/></tridk:typelanguagedependentstring></tridk:typedefinition><tridk:public tridk:value="yes"/><tridk:system tridk:value="yes"/><tridk:classdefinition><tridk:class tridk:element="GENERAL"/><tridk:class tridk:element="NEW"/><tridk:class tridk:element="MODIFY"/><tridk:class tridk:element="DOCUMENT"/></tridk:classdefinition></tridk:field><tridk:field tridk:exportmode="cascade" tridk:element="OSUSER"><tridk:displaydefinition><tridk:label>OS-User</tridk:label><tridk:description>The username for windows NT authentication</tridk:description></tridk:displaydefinition><tridk:typedefinition><tridk:typestring><tridk:minnoofvalues tridk:value="0"/><tridk:maxnoofvalues tridk:value="1"/></tridk:typestring></tridk:typedefinition><tridk:public tridk:value="yes"/><tridk:system tridk:value="yes"/><tridk:classdefinition><tridk:class tridk:element="NEW"/><tridk:class tridk:element="MODIFY"/></tridk:classdefinition></tridk:field>	<tridk:field tridk:exportmode="cascade" tridk:element="PASSWORD"><tridk:displaydefinition><tridk:label>Password</tridk:label><tridk:description>Encrypted password of the user.</tridk:description></tridk:displaydefinition><tridk:typedefinition><tridk:typelongtext><tridk:minnoofvalues tridk:value="0"/></tridk:typelongtext></tridk:typedefinition><tridk:public tridk:value="no"/><tridk:system tridk:value="yes"/><tridk:classdefinition/></tridk:field><tridk:field tridk:exportmode="cascade" tridk:element="USERNAME"><tridk:displaydefinition><tridk:label>User Name</tridk:label><tridk:description>The username of the user</tridk:description></tridk:displaydefinition><tridk:typedefinition><tridk:typelov tridk:element="USERNAME"><tridk:minnoofvalues tridk:value="1"/><tridk:maxnoofvalues tridk:value="1"/></tridk:typelov></tridk:typedefinition><tridk:public tridk:value="yes"/><tridk:system tridk:value="yes"/><tridk:classdefinition><tridk:class tridk:element="GENERAL"/><tridk:class tridk:element="NEW"/><tridk:class tridk:element="MODIFY"/></tridk:classdefinition></tridk:field></tridk:fields></tridk:setup>
"@

}

Describe "Get-IshTypeFieldDefinition" -Tags "Read" {
	Context "Get-IshTypeFieldDefinition without IshSession/TriDKXmlSetupFilePath loads latest resource entry" {
		BeforeAll {
			$ishTypeFieldDefinitions = Get-IshTypeFieldDefinition
		}
		It "GetType().Name" {
			$ishTypeFieldDefinitions[0].GetType().Name | Should -BeExactly "IshTypeFieldDefinition"
		}
		It "ishTypeFieldDefinitions[0].ISHType" {
			$ishTypeFieldDefinitions[0].ISHType | Should -Not -BeNullOrEmpty
		}
		It "ishTypeFieldDefinitions[0].Level" {
			$ishTypeFieldDefinitions[0].Level | Should -Not -BeNullOrEmpty
		}
		It "ishTypeFieldDefinitions[0].Name" {
			$ishTypeFieldDefinitions[0].Name | Should -Not -BeNullOrEmpty
		}
		It "ishTypeFieldDefinitions[0].DataType" {
			$ishTypeFieldDefinitions[0].DataType | Should -Not -BeNullOrEmpty
		}
		It "FXYEDITOR is not a standard field for <13.0.x" {
			# Making sure the implicit IshSession stored in SessionState is temporarily removed
			$restoreIshSession=$executioncontext.SessionState.PSVariable.GetValue("ISHRemoteSessionStateIshSession")
			$executioncontext.SessionState.PSVariable.Set("ISHRemoteSessionStateIshSession", $null)
			(Get-IshTypeFieldDefinition | Where-Object -Property Name -EQ -Value "FXYEDITOR").Count | Should -Be 0
			$executioncontext.SessionState.PSVariable.Set("ISHRemoteSessionStateIshSession", $restoreIshSession)
		}
		# More tests required for the IshTypeFieldDefinition properties
		# Also test that there CardFields and TableFields present
	}
	Context "Get-IshTypeFieldDefinition without IshSession only TriDKXmlSetupFilePath" {
		It "Parameter TriDKXmlSetupFilePath invalid" {
			{ Get-IshTypeFieldDefinition -TriDKXmlSetupFilePath "INVALIDFILEPATH" } | Should -Throw
		}
		It "Parameter TriDKXmlSetup without IshSession" {
			$WarningPreference="SilentlyContinue"
			$tridkXmlSetupUserContent | Out-File $tempFilePath
			$ishTypeFieldDefinitions = Get-IshTypeFieldDefinition -TriDKXmlSetupFilePath $tempFilePath
			$ishTypeFieldDefinitions.Count | Should -Be 6
		}
	}
	Context "Get-IshTypeFieldDefinition with IshSession loads matching resource entry or Settings25" {
		It "Parameter IshSession invalid" {
			{ Get-IshTypeFieldDefinition -IShSession "INVALIDISHSESSION" } | Should -Throw
		}
		It "IshSession.IshTypeFieldDefinition[0].GetType().Name" {
			Get-IshTypeFieldDefinition -IshSession $ishSession
			$ishSession.IshTypeFieldDefinition[0].GetType().Name | Should -BeExactly "IshTypeFieldDefinition"
		}
		It "Table ISHBackgroundTask" {
			(Get-IshTypeFieldDefinition -IshSession $ishSession | Where-Object -Property ISHType -EQ 'ISHBackgroundTask' | Where-Object -Property Level -EQ 'Task').Count | Should -Be 15
			(Get-IshTypeFieldDefinition -IshSession $ishSession | Where-Object -Property ISHType -EQ 'ISHBackgroundTask' | Where-Object -Property Level -EQ 'History').Count | Should -Be 9
			(Get-IshTypeFieldDefinition -IshSession $ishSession | Where-Object -Property ISHType -EQ 'ISHBackgroundTask' | Where-Object -Property AllowOnRead -EQ $true).Count | Should -Be 24 # all columns are allowed to be read
			(Get-IshTypeFieldDefinition -IshSession $ishSession | Where-Object -Property ISHType -EQ 'ISHBackgroundTask' | Where-Object -Property AllowOnCreate -EQ $false).Count | Should -Be 20 # all columns are explicit api parameters and cannot be set over metadata
			(Get-IshTypeFieldDefinition -IshSession $ishSession | Where-Object -Property ISHType -EQ 'ISHBackgroundTask' | Where-Object -Property IsMultiValue -EQ $false).Count | Should -Be 24 # all columns are single value
			(Get-IshTypeFieldDefinition -IshSession $ishSession | Where-Object -Property ISHType -EQ 'ISHBackgroundTask' | Where-Object -Property IsSystem -EQ $true).Count | Should -Be 24 # all columns are system columns
			(Get-IshTypeFieldDefinition -IshSession $ishSession | Where-Object -Property ISHType -EQ 'ISHBackgroundTask' | Where-Object -Property Name -EQ 'STATUS').DataSource | Should -Be 'DBACKGROUNDTASKSTATUS'
			(Get-IshTypeFieldDefinition -IshSession $ishSession | Where-Object -Property ISHType -EQ 'ISHBackgroundTask' | Where-Object -Property Name -EQ 'USERID').DataSource | Should -Be 'ISHUser'
		}
		It "Table ISHEvent" {
            if((([Version]$ishSession.ServerVersion).Major -eq 15 -and ([Version]$ishSession.ServerVersion).Minor -ge 1) -or ([Version]$ishSession.ServerVersion).Major -ge 16)
            {
		        (Get-IshTypeFieldDefinition -IshSession $ishSession | Where-Object -Property ISHType -EQ 'ISHEvent' | Where-Object -Property Level -EQ 'Progress').Count | Should -Be 11
		        (Get-IshTypeFieldDefinition -IshSession $ishSession | Where-Object -Property ISHType -EQ 'ISHEvent' | Where-Object -Property Level -EQ 'Detail').Count | Should -Be 12
		        (Get-IshTypeFieldDefinition -IshSession $ishSession | Where-Object -Property ISHType -EQ 'ISHEvent' | Where-Object -Property AllowOnRead -EQ $true).Count | Should -Be 23 # all columns are allowed to be read
		        (Get-IshTypeFieldDefinition -IshSession $ishSession | Where-Object -Property ISHType -EQ 'ISHEvent' | Where-Object -Property AllowOnCreate -EQ $false).Count | Should -Be 11 # all columns are explicit api parameters and cannot be set over metadata
		        (Get-IshTypeFieldDefinition -IshSession $ishSession | Where-Object -Property ISHType -EQ 'ISHEvent' | Where-Object -Property IsMultiValue -EQ $false).Count | Should -Be 23 # all columns are single value
		        (Get-IshTypeFieldDefinition -IshSession $ishSession | Where-Object -Property ISHType -EQ 'ISHEvent' | Where-Object -Property IsSystem -EQ $true).Count | Should -Be 23 # all columns are system columns
		        (Get-IshTypeFieldDefinition -IshSession $ishSession | Where-Object -Property ISHType -EQ 'ISHEvent' | Where-Object -Property Name -EQ 'USERID').DataSource | Should -Be 'ISHUser'
            }
            else
            {
		    	(Get-IshTypeFieldDefinition -IshSession $ishSession | Where-Object -Property ISHType -EQ 'ISHEvent' | Where-Object -Property Level -EQ 'Progress').Count | Should -Be 11
		    	(Get-IshTypeFieldDefinition -IshSession $ishSession | Where-Object -Property ISHType -EQ 'ISHEvent' | Where-Object -Property Level -EQ 'Detail').Count | Should -Be 12
		    	(Get-IshTypeFieldDefinition -IshSession $ishSession | Where-Object -Property ISHType -EQ 'ISHEvent' | Where-Object -Property AllowOnRead -EQ $true).Count | Should -Be 23 # all columns are allowed to be read
		    	(Get-IshTypeFieldDefinition -IshSession $ishSession | Where-Object -Property ISHType -EQ 'ISHEvent' | Where-Object -Property AllowOnCreate -EQ $false).Count | Should -Be 11 # all columns are explicit api parameters and cannot be set over metadata
		    	(Get-IshTypeFieldDefinition -IshSession $ishSession | Where-Object -Property ISHType -EQ 'ISHEvent' | Where-Object -Property IsMultiValue -EQ $false).Count | Should -Be 23 # all columns are single value
		    	(Get-IshTypeFieldDefinition -IshSession $ishSession | Where-Object -Property ISHType -EQ 'ISHEvent' | Where-Object -Property IsSystem -EQ $true).Count | Should -Be 23 # all columns are system columns
		    	(Get-IshTypeFieldDefinition -IshSession $ishSession | Where-Object -Property ISHType -EQ 'ISHEvent' | Where-Object -Property Name -EQ 'USERID').DataSource | Should -Be 'ISHUser'
            }
		}
	}
	Context "Get-IshTypeFieldDefinition with IshSession/TriDKXmlSetupFilePath loads if ServerVersion<13.0.0" {
		It "Parameter IshSession invalid" {
			{ Get-IshTypeFieldDefinition -IShSession "INVALIDISHSESSION" -TriDKXmlSetupFilePath "INVALIDFILEPATH" } | Should -Throw
		}
	}
    Context "Get-IshTypeFieldDefinition and Metadata bound fields" {
		BeforeAll {
        	$typeDefinitions = Get-IshTypeFieldDefinition -IshSession $ishSession
		}
		It "Check AllowOnSmartTagging not null, empty and boolean"{
			# Initially test was 14s long doing a 'foreach($typeDefinition in $typeDefinitions)', now testing the first array entry
			if ($typeDefinitions.Length -gt 0)
			{
				$typeDefinitions[0].AllowOnSmartTagging | Should -Not -BeNullOrEmpty
				$typeDefinitions[0].AllowOnSmartTagging | Should -BeOfType System.Boolean
			}
        }
		It "Check Metadata bound field - if configured in Extension XML settings"{
			$typeDefinitionsMetadataBinding = $typeDefinitions | Where-Object -Property DataType -EQ "ISHMetadataBinding"
			if($typeDefinitionsMetadataBinding.Count -gt 0)
			{
				foreach($typeDefinitionMetadataBinding in $typeDefinitionsMetadataBinding)
				{
					$typeDefinitionMetadataBinding.DataSource | Should -Not -BeNullOrEmpty
					$typeDefinitionMetadataBinding.ReferenceMetadataBinding | Should -Not -BeNullOrEmpty
					($typeDefinitionMetadataBinding.DataSource -eq $typeDefinitionMetadataBinding.ReferenceMetadataBinding) | Should -Be $true
				}
			}
		}
    }
    Context "IshTypeFieldDefinition - check properties" {
        BeforeAll {
			$typeDefinitions = Get-IshTypeFieldDefinition -IshSession $ishSession
		}
        It "Check CRUST, MM and SDB length"{
            if ($typeDefinitions.Length -gt 0)
			{
				$typeDefinitions[0].CRUST.Length | Should -Be 5
				$typeDefinitions[0].MM.Length | Should -Be 2
				$typeDefinitions[0].SDB.Length | Should -Be 3
            }
        }
    }
}

AfterAll {
	Write-Host ("Running "+$cmdletName+" Test Data and Variables cleanup")
	try { Remove-Item $tempFilePath -Force } catch { }
}

