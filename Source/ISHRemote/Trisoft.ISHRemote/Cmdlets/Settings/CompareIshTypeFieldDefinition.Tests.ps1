BeforeAll {
	$cmdletName = "Compare-IshTypeFieldDefinition"
	Write-Host ("`r`nLoading ISHRemote.PesterSetup.ps1 on PSVersion[" + $psversionTable.PSVersion + "] over BeforeAll-block for MyCommand[" + $cmdletName + "]...")
	. (Join-Path (Split-Path -Parent $PSCommandPath) "\..\..\ISHRemote.PesterSetup.ps1")
	
	Write-Host ("Running "+$cmdletName+" Test Data and Variables initialization")
	$tempFilePath1 = (New-TemporaryFile).FullName
	$tempFilePath2 = (New-TemporaryFile).FullName
$tridkXmlSetupUser1Content = @"
<?xml version="1.0"?><?xml-stylesheet href="full.export.xsl" type="text/xsl"?><!-- InfoShare Author 3.5.0 --><tridk:setup xml:lang="EN" xmlns:tridk="urn:trisoft.be:Tridk:Setup:1.0" tridk:version="120.11.0.3215"><tridk:cardtypes><!-- General cardtypes --><tridk:cardtype tridk:exportmode="cascade" tridk:element="USER" tridk:metatype="usercard"><tridk:displaydefinition><tridk:label>User</tridk:label><tridk:description/></tridk:displaydefinition><tridk:fielddefinition><tridk:cardtypefield tridk:element="CREATED-ON" tridk:userdefined="no"><tridk:sequence tridk:value="5"/></tridk:cardtypefield><tridk:cardtypefield tridk:element="DELETE-ACCESS" tridk:userdefined="no"><tridk:sequence tridk:value="11"/></tridk:cardtypefield><tridk:cardtypefield tridk:element="FUSERGROUP" tridk:userdefined="yes"><tridk:sequence tridk:value="1"/><tridk:memberdefinition><tridk:member tridk:element="CTUSERGROUP"/></tridk:memberdefinition></tridk:cardtypefield><tridk:cardtypefield tridk:element="NAME" tridk:userdefined="no"><tridk:sequence tridk:value="2"/></tridk:cardtypefield><tridk:cardtypefield tridk:element="OSUSERLEFT" tridk:userdefined="no"><tridk:sequence tridk:value="17"/></tridk:cardtypefield><tridk:cardtypefield tridk:element="PASSWORD" tridk:userdefined="no"><tridk:sequence tridk:value="52"/></tridk:cardtypefield><tridk:cardtypefield tridk:element="USERNAME" tridk:userdefined="no"><tridk:sequence tridk:value="3"/></tridk:cardtypefield></tridk:fielddefinition></tridk:cardtype></tridk:cardtypes><tridk:fields><tridk:field tridk:exportmode="cascade" tridk:element="CREATED-ON"><tridk:displaydefinition>	<tridk:label>Creation Date</tridk:label><tridk:description>Used on all cards to indicate the date that the object was created</tridk:description></tridk:displaydefinition><tridk:typedefinition><tridk:typedate><tridk:minnoofvalues tridk:value="1"/><tridk:maxnoofvalues tridk:value="1"/></tridk:typedate></tridk:typedefinition><tridk:public tridk:value="yes"/><tridk:system tridk:value="yes"/><tridk:classdefinition><tridk:class tridk:element="GENERAL"/><tridk:class tridk:element="INDEX"/><tridk:class tridk:element="VCLASSDISPLAYHISTORY"/></tridk:classdefinition></tridk:field><tridk:field tridk:exportmode="cascade" tridk:element="DELETE-ACCESS"><tridk:displaydefinition><tridk:label>Delete Access</tridk:label><tridk:description>User roles required in order to delete the object</tridk:description></tridk:displaydefinition><tridk:typedefinition><tridk:typelov tridk:element="DUSERGROUP"><tridk:minnoofvalues tridk:value="0"/><tridk:maxnoofvalues tridk:value="999999"/></tridk:typelov></tridk:typedefinition><tridk:public tridk:value="yes"/><tridk:system tridk:value="yes"/><tridk:classdefinition><tridk:class tridk:element="SECURITY"/></tridk:classdefinition></tridk:field><tridk:field tridk:exportmode="cascade" tridk:element="FUSERGROUP"><tridk:displaydefinition><tridk:label>Usergroup</tridk:label><tridk:description>Used on all card types. On the USER card type the field indicates that the user has write/modify access to documents of this usergroup. On all other objects the field contains the usergroup that owns the object and can modify the object.</tridk:description></tridk:displaydefinition><tridk:typedefinition><tridk:typecardreference><tridk:memberdefinedoncard tridk:value="no"/><tridk:minnoofvalues tridk:value="0"/><tridk:maxnoofvalues tridk:value="999999"/></tridk:typecardreference></tridk:typedefinition><tridk:public tridk:value="yes"/><tridk:system tridk:value="no"/><tridk:classdefinition><tridk:class tridk:element="NEW"/><tridk:class tridk:element="MODIFY"/><tridk:class tridk:element="INDEX"/><tridk:class tridk:element="DOCUMENT"/></tridk:classdefinition></tridk:field><tridk:field tridk:exportmode="cascade" tridk:element="NAME"><tridk:displaydefinition><tridk:label>Label</tridk:label><tridk:description>Internal system field to hold the unique label of the object within its own object type.</tridk:description></tridk:displaydefinition><tridk:typedefinition><tridk:typelanguagedependentstring><tridk:minnoofvalues tridk:value="1"/><tridk:maxnoofvalues tridk:value="1"/></tridk:typelanguagedependentstring></tridk:typedefinition><tridk:public tridk:value="yes"/><tridk:system tridk:value="yes"/><tridk:classdefinition><tridk:class tridk:element="GENERAL"/><tridk:class tridk:element="NEW"/><tridk:class tridk:element="MODIFY"/><tridk:class tridk:element="DOCUMENT"/></tridk:classdefinition></tridk:field><tridk:field tridk:exportmode="cascade" tridk:element="OSUSERLEFT"><tridk:displaydefinition><tridk:label>OS-User</tridk:label><tridk:description>The username for windows NT authentication</tridk:description></tridk:displaydefinition><tridk:typedefinition><tridk:typestring><tridk:minnoofvalues tridk:value="0"/><tridk:maxnoofvalues tridk:value="1"/></tridk:typestring></tridk:typedefinition><tridk:public tridk:value="yes"/><tridk:system tridk:value="yes"/><tridk:classdefinition><tridk:class tridk:element="NEW"/><tridk:class tridk:element="MODIFY"/></tridk:classdefinition></tridk:field>	<tridk:field tridk:exportmode="cascade" tridk:element="PASSWORD"><tridk:displaydefinition><tridk:label>Password</tridk:label><tridk:description>Encrypted password of the user.</tridk:description></tridk:displaydefinition><tridk:typedefinition><tridk:typelongtext><tridk:minnoofvalues tridk:value="0"/></tridk:typelongtext></tridk:typedefinition><tridk:public tridk:value="no"/><tridk:system tridk:value="yes"/><tridk:classdefinition/></tridk:field><tridk:field tridk:exportmode="cascade" tridk:element="USERNAME"><tridk:displaydefinition><tridk:label>User Name</tridk:label><tridk:description>The username of the user</tridk:description></tridk:displaydefinition><tridk:typedefinition><tridk:typelov tridk:element="USERNAME"><tridk:minnoofvalues tridk:value="1"/><tridk:maxnoofvalues tridk:value="1"/></tridk:typelov></tridk:typedefinition><tridk:public tridk:value="yes"/><tridk:system tridk:value="yes"/><tridk:classdefinition><tridk:class tridk:element="GENERAL"/><tridk:class tridk:element="NEW"/><tridk:class tridk:element="MODIFY"/></tridk:classdefinition></tridk:field></tridk:fields></tridk:setup>
"@
$tridkXmlSetupUser2Content = @"
<?xml version="1.0"?><?xml-stylesheet href="full.export.xsl" type="text/xsl"?><!-- InfoShare Author 3.5.0 --><tridk:setup xml:lang="EN" xmlns:tridk="urn:trisoft.be:Tridk:Setup:1.0" tridk:version="120.11.0.3215"><tridk:cardtypes><!-- General cardtypes --><tridk:cardtype tridk:exportmode="cascade" tridk:element="USER" tridk:metatype="usercard"><tridk:displaydefinition><tridk:label>User</tridk:label><tridk:description/></tridk:displaydefinition><tridk:fielddefinition><tridk:cardtypefield tridk:element="DELETE-ACCESS" tridk:userdefined="no"><tridk:sequence tridk:value="11"/></tridk:cardtypefield><tridk:cardtypefield tridk:element="FUSERGROUP" tridk:userdefined="yes"><tridk:sequence tridk:value="1"/><tridk:memberdefinition><tridk:member tridk:element="CTCONFIGURATION"/></tridk:memberdefinition></tridk:cardtypefield><tridk:cardtypefield tridk:element="NAME" tridk:userdefined="no"><tridk:sequence tridk:value="2"/></tridk:cardtypefield><tridk:cardtypefield tridk:element="OSUSERRIGHT" tridk:userdefined="no"><tridk:sequence tridk:value="17"/></tridk:cardtypefield><tridk:cardtypefield tridk:element="PASSWORD" tridk:userdefined="no"><tridk:sequence tridk:value="52"/></tridk:cardtypefield><tridk:cardtypefield tridk:element="USERNAME" tridk:userdefined="no"><tridk:sequence tridk:value="3"/></tridk:cardtypefield></tridk:fielddefinition></tridk:cardtype></tridk:cardtypes><tridk:fields><tridk:field tridk:exportmode="cascade" tridk:element="CREATED-ON"><tridk:displaydefinition>	<tridk:label>Creation Date</tridk:label><tridk:description>Used on all cards to indicate the date that the object was created</tridk:description></tridk:displaydefinition><tridk:typedefinition><tridk:typedate><tridk:minnoofvalues tridk:value="1"/><tridk:maxnoofvalues tridk:value="1"/></tridk:typedate></tridk:typedefinition><tridk:public tridk:value="yes"/><tridk:system tridk:value="yes"/><tridk:classdefinition><tridk:class tridk:element="GENERAL"/><tridk:class tridk:element="INDEX"/><tridk:class tridk:element="VCLASSDISPLAYHISTORY"/></tridk:classdefinition></tridk:field><tridk:field tridk:exportmode="cascade" tridk:element="DELETE-ACCESS"><tridk:displaydefinition><tridk:label>Delete Access</tridk:label><tridk:description>User roles required in order to delete the object</tridk:description></tridk:displaydefinition><tridk:typedefinition><tridk:typelov tridk:element="DUSERGROUP"><tridk:minnoofvalues tridk:value="0"/><tridk:maxnoofvalues tridk:value="999999"/></tridk:typelov></tridk:typedefinition><tridk:public tridk:value="yes"/><tridk:system tridk:value="yes"/><tridk:classdefinition><tridk:class tridk:element="SECURITY"/></tridk:classdefinition></tridk:field><tridk:field tridk:exportmode="cascade" tridk:element="FUSERGROUP"><tridk:displaydefinition><tridk:label>Usergroup</tridk:label><tridk:description>Used on all card types. On the USER card type the field indicates that the user has write/modify access to documents of this usergroup. On all other objects the field contains the usergroup that owns the object and can modify the object.</tridk:description></tridk:displaydefinition><tridk:typedefinition><tridk:typecardreference><tridk:memberdefinedoncard tridk:value="no"/><tridk:minnoofvalues tridk:value="0"/><tridk:maxnoofvalues tridk:value="999999"/></tridk:typecardreference></tridk:typedefinition><tridk:public tridk:value="yes"/><tridk:system tridk:value="no"/><tridk:classdefinition><tridk:class tridk:element="NEW"/><tridk:class tridk:element="MODIFY"/><tridk:class tridk:element="DOCUMENT"/></tridk:classdefinition></tridk:field><tridk:field tridk:exportmode="cascade" tridk:element="NAME"><tridk:displaydefinition><tridk:label>Label</tridk:label><tridk:description>Internal system field to hold the unique label of the object within its own object type.</tridk:description></tridk:displaydefinition><tridk:typedefinition><tridk:typelanguagedependentstring><tridk:minnoofvalues tridk:value="1"/><tridk:maxnoofvalues tridk:value="1000"/></tridk:typelanguagedependentstring></tridk:typedefinition><tridk:public tridk:value="no"/><tridk:system tridk:value="yes"/><tridk:classdefinition><tridk:class tridk:element="GENERAL"/><tridk:class tridk:element="NEW"/><tridk:class tridk:element="MODIFY"/><tridk:class tridk:element="DOCUMENT"/></tridk:classdefinition></tridk:field><tridk:field tridk:exportmode="cascade" tridk:element="OSUSERRIGHT"><tridk:displaydefinition><tridk:label>OS-User</tridk:label><tridk:description>The username for windows NT authentication</tridk:description></tridk:displaydefinition><tridk:typedefinition><tridk:typestring><tridk:minnoofvalues tridk:value="0"/><tridk:maxnoofvalues tridk:value="1"/></tridk:typestring></tridk:typedefinition><tridk:public tridk:value="yes"/><tridk:system tridk:value="yes"/><tridk:classdefinition><tridk:class tridk:element="NEW"/><tridk:class tridk:element="MODIFY"/></tridk:classdefinition></tridk:field>	<tridk:field tridk:exportmode="cascade" tridk:element="PASSWORD"><tridk:displaydefinition><tridk:label>Password</tridk:label><tridk:description>Encrypted password of the user.</tridk:description></tridk:displaydefinition><tridk:typedefinition><tridk:typelongtext><tridk:minnoofvalues tridk:value="0"/></tridk:typelongtext></tridk:typedefinition><tridk:public tridk:value="no"/><tridk:system tridk:value="yes"/><tridk:classdefinition/></tridk:field><tridk:field tridk:exportmode="cascade" tridk:element="USERNAME"><tridk:displaydefinition><tridk:label>User Name</tridk:label><tridk:description>The username of the user</tridk:description></tridk:displaydefinition><tridk:typedefinition><tridk:typelov tridk:element="USERNAME"><tridk:minnoofvalues tridk:value="1"/><tridk:maxnoofvalues tridk:value="1"/></tridk:typelov></tridk:typedefinition><tridk:public tridk:value="yes"/><tridk:system tridk:value="yes"/><tridk:classdefinition><tridk:class tridk:element="GENERAL"/><tridk:class tridk:element="NEW"/><tridk:class tridk:element="MODIFY"/></tridk:classdefinition></tridk:field></tridk:fields></tridk:setup>
"@

}

Describe "Compare-IshTypeFieldDefinition" -Tags "Read" {
	Context "Compare-IshTypeFieldDefinition with 2 IshTypeFieldDefinitions" {
		BeforeAll {
			$WarningPreference="SilentlyContinue"
			$tridkXmlSetupUser1Content | Out-File $tempFilePath1
			$referenceIshTypeFieldDefinitions = Get-IshTypeFieldDefinition -TriDKXmlSetupFilePath $tempFilePath1
			$tridkXmlSetupUser2Content | Out-File $tempFilePath2
			$differenceIshTypeFieldDefinitions = Get-IshTypeFieldDefinition -TriDKXmlSetupFilePath $tempFilePath2
			$ishTypeFieldDefinitionCompares = Compare-IshTypeFieldDefinition -LeftIshTypeFieldDefinition $referenceIshTypeFieldDefinitions -RightIshTypeFieldDefinition $differenceIshTypeFieldDefinitions
		}
		It "GetType().Name" {
			$ishTypeFieldDefinitionCompares[0].GetType().Name | Should -BeExactly "IshTypeFieldDefinitionCompare"
		}
		It "Parameter Left invalid" {
			{ Compare-IshTypeFieldDefinition -LeftIshTypeFieldDefinition "INVALIDREFERENCELIST" -RightIshTypeFieldDefinition "INVALIDDIFFERENCELIST" } | Should -Throw
		}
		It "Parameter Right invalid" {
			{ Compare-IshTypeFieldDefinition -LeftIshTypeFieldDefinition $referenceIshTypeFieldDefinitions -RightIshTypeFieldDefinition "INVALIDDIFFERENCELIST" } | Should -Throw
		}
		It "Parameter Left/Right using IncludeIdentical" {
			$ishTypeFieldDefinitionCompares = Compare-IshTypeFieldDefinition -LeftIshTypeFieldDefinition $referenceIshTypeFieldDefinitions -RightIshTypeFieldDefinition $differenceIshTypeFieldDefinitions -IncludeIdentical
			$ishTypeFieldDefinitionCompares.Count | Should -Be 9
		}
		It "Parameter Left/Right" {
			$ishTypeFieldDefinitionCompares = Compare-IshTypeFieldDefinition -LeftIshTypeFieldDefinition $referenceIshTypeFieldDefinitions -RightIshTypeFieldDefinition $differenceIshTypeFieldDefinitions
			$ishTypeFieldDefinitionCompares.Count | Should -Be 7  # =9entries-2equals
		}
		It "Parameter Left/Right using ExcludeDifferent" {
			$ishTypeFieldDefinitionCompares = Compare-IshTypeFieldDefinition -LeftIshTypeFieldDefinition $referenceIshTypeFieldDefinitions -RightIshTypeFieldDefinition $differenceIshTypeFieldDefinitions -ExcludeDifferent
			$ishTypeFieldDefinitionCompares.Count | Should -Be 3  # =9entries-2equals-2x2diff
		}
		It "Parameter Left/Right using ExcludeDifferent/ExcludeLeftUnique" {
			$ishTypeFieldDefinitionCompares = Compare-IshTypeFieldDefinition -LeftIshTypeFieldDefinition $referenceIshTypeFieldDefinitions -RightIshTypeFieldDefinition $differenceIshTypeFieldDefinitions -ExcludeDifferent -ExcludeLeftUnique
			$ishTypeFieldDefinitionCompares.Count | Should -Be 1  # =9entries-2equals-2x2diff-2left
		}
		It "Parameter Left/Right using ExcludeDifferent/ExcludeRightUnique" {
			$ishTypeFieldDefinitionCompares = Compare-IshTypeFieldDefinition -LeftIshTypeFieldDefinition $referenceIshTypeFieldDefinitions -RightIshTypeFieldDefinition $differenceIshTypeFieldDefinitions -ExcludeDifferent -ExcludeRightUnique
			$ishTypeFieldDefinitionCompares.Count | Should -Be 2  # =9entries-2equals-2x2diff-1right
		}
		It "Parameter Left/Right using IncludeIdentical/ExcludeDifferent/ExcludeLeftUnique/ExcludeRightUnique" {
			$ishTypeFieldDefinitionCompares = Compare-IshTypeFieldDefinition -LeftIshTypeFieldDefinition $referenceIshTypeFieldDefinitions -RightIshTypeFieldDefinition $differenceIshTypeFieldDefinitions -IncludeIdentical -ExcludeDifferent -ExcludeLeftUnique -ExcludeRightUnique
			$ishTypeFieldDefinitionCompares.Count | Should -Be 2  # =9entries-2x2diff-2left-1right
		}
		# More tests possible for the IshTypeFieldDefinition properties
		# Perhaps also test that there CardFields and TableFields present
	}
	Context "Compare-IshTypeFieldDefinition using IshSession" {
		It "Parameter Left invalid" {
			{ Compare-IshTypeFieldDefinition -LeftIshSession "INVALIDREFERENCEISHSESSION" -RightIshSession "INVALIDDIFFERENCEISHSESSION" } | Should -Throw
		}
		It "Parameter Right invalid" {
			{ Compare-IshTypeFieldDefinition -LeftIshSession $ishSession -RightIshSession "INVALIDDIFFERENCEISHSESSION" } | Should -Throw
		}
		It "Parameter Left/Right same IshSession" {
			{ Compare-IshTypeFieldDefinition -LeftIshSession $ishSession -RightIshSession $ishSession } | Should -Not -Throw
		}
		It "Parameter Left/Right same IshSession using IncludeIdentical" {
			$ishTypeFieldDefinitionCompares = Compare-IshTypeFieldDefinition -LeftIshSession $ishSession -RightIshSession $ishSession -IncludeIdentical
			$ishTypeFieldDefinitionCompares.Count -eq $ishSession.IshTypeFieldDefinition.Count | Should -Be $true
		}
		It "Parameter Left/Right same IshSession using ExcludeDifferent" {
			$ishTypeFieldDefinitionCompares = Compare-IshTypeFieldDefinition -LeftIshSession $ishSession -RightIshSession $ishSession -ExcludeDifferent
			$ishTypeFieldDefinitionCompares.Count | Should -Be 0
		}
		It "Parameter Left/Right same IshSession using ExcludeLeftUnique" {
			$ishTypeFieldDefinitionCompares = Compare-IshTypeFieldDefinition -LeftIshSession $ishSession -RightIshSession $ishSession -ExcludeLeftUnique
			$ishTypeFieldDefinitionCompares.Count | Should -Be 0
		}
		It "Parameter Left/Right same IshSession using IncludeIdentical/ExcludeRightUnique" {
			$ishTypeFieldDefinitionCompares = Compare-IshTypeFieldDefinition -LeftIshSession $ishSession -RightIshSession $ishSession -IncludeIdentical -ExcludeRightUnique
			$ishTypeFieldDefinitionCompares.Count -eq $ishSession.IshTypeFieldDefinition.Count | Should -Be $true
		}
		It "Parameter Left/Right same IshSession using IncludeIdentical/ExcludeDifferent/ExcludeLeftUnique/ExcludeRightUnique" {
			$ishTypeFieldDefinitionCompares = Compare-IshTypeFieldDefinition -LeftIshSession $ishSession -RightIshSession $ishSession -IncludeIdentical -ExcludeDifferent -ExcludeLeftUnique -ExcludeRightUnique
			$ishTypeFieldDefinitionCompares.Count -eq $ishSession.IshTypeFieldDefinition.Count | Should -Be $true
		}
	}
	Context "Compare-IshTypeFieldDefinition mixing IshSession and TriDKXmlSetupFilePath" {
		BeforeAll {
			$WarningPreference="SilentlyContinue"
			$tridkXmlSetupUser1Content | Out-File $tempFilePath1
			$referenceIshTypeFieldDefinitions = Get-IshTypeFieldDefinition -TriDKXmlSetupFilePath $tempFilePath1
		}
		It "Parameter Left is IshSession and Right is TriDKXmlSetupFilePath" {
			(Compare-IshTypeFieldDefinition -LeftIshSession $ishSession -RightIshTypeFieldDefinition $referenceIshTypeFieldDefinitions).Count -ge 460 | Should -Be $true
		}
		It "Parameter Left is IshSession and Right is TriDKXmlSetupFilePath using ExcludeDifferent" {
			(Compare-IshTypeFieldDefinition -LeftIshSession $ishSession -RightIshTypeFieldDefinition $referenceIshTypeFieldDefinitions -ExcludeDifferent).Count -ge 460 | Should -Be $true
		}
		It "Parameter Left is TriDKXmlSetupFilePath and Right is IshSession using IncludeIdentical" {
			(Compare-IshTypeFieldDefinition -LeftIshTypeFieldDefinition $referenceIshTypeFieldDefinitions -RightIshSession $ishSession -IncludeIdentical).Count -ge 460 | Should -Be $true
		}
		It "Parameter Left is TriDKXmlSetupFilePath and Right is IshSession using ExcludeDifferent" {
			(Compare-IshTypeFieldDefinition -LeftIshTypeFieldDefinition $referenceIshTypeFieldDefinitions -RightIshSession $ishSession -ExcludeDifferent).Count -ge 460 | Should -Be $true
		}
		It "Parameter Left is TriDKXmlSetupFilePath and Right is IshSession using ExcludeLeftUnique" {
			(Compare-IshTypeFieldDefinition -LeftIshTypeFieldDefinition $referenceIshTypeFieldDefinitions -RightIshSession $ishSession -ExcludeLeftUnique).Count -ge 460 | Should -Be $true
		}
		It "Parameter Left is TriDKXmlSetupFilePath and Right is IshSession using ExcludeRightUnique" {
			(Compare-IshTypeFieldDefinition -LeftIshTypeFieldDefinition $referenceIshTypeFieldDefinitions -RightIshSession $ishSession -ExcludeRightUnique).Count | Should -Be 9
		}
		It "Parameter Left is TriDKXmlSetupFilePath and Right is IshSession using ExcludeLeftUnique/ExcludeRightUnique" {
			(Compare-IshTypeFieldDefinition -LeftIshTypeFieldDefinition $referenceIshTypeFieldDefinitions -RightIshSession $ishSession -ExcludeLeftUnique -ExcludeRightUnique).Count | Should -Be 8
		}
		It "Parameter Left is TriDKXmlSetupFilePath and Right is IshSession using ExcludeDifferent/ExcludeLeftUnique/ExcludeRightUnique" {
			(Compare-IshTypeFieldDefinition -LeftIshTypeFieldDefinition $referenceIshTypeFieldDefinitions -RightIshSession $ishSession -ExcludeDifferent -ExcludeLeftUnique -ExcludeRightUnique).Count | Should -Be 0
		}
		It "Parameter Left is TriDKXmlSetupFilePath and Right is IshSession using ExcludeDifferent/ExcludeLeftUnique/ExcludeRightUnique" {
			(Compare-IshTypeFieldDefinition -LeftIshTypeFieldDefinition $referenceIshTypeFieldDefinitions -RightIshSession $ishSession -IncludeIdentical -ExcludeDifferent -ExcludeLeftUnique -ExcludeRightUnique).Count | Should -Be 1
		}
	}
}

AfterAll {
	Write-Host ("Running "+$cmdletName+" Test Data and Variables cleanup")
	try { Remove-Item $tempFilePath1 -Force } catch { }
	try { Remove-Item $tempFilePath2 -Force } catch { }
}

