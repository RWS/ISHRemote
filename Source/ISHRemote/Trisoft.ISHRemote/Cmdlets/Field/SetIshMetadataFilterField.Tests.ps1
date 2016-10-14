Write-Host ("`r`nLoading ISHRemote.PesterSetup.ps1 for MyCommand[" + $MyInvocation.MyCommand + "]...")
. (Join-Path (Split-Path -Parent $MyInvocation.MyCommand.Path) "..\..\ISHRemote.PesterSetup.ps1")
$cmdletName = "Set-IshMetadataFilterField"
try {

Describe “Set-IshMetadataFilterField" -Tags "Read" {
	Write-Host "Initializing Test Data and Variables"

	Context "Set-IshMetadataFilterField -IshSession $ishSession returns IshMetadataFilterField" {
		It "GetType().Name" {
			(Set-IshMetadataFilterField -IshSession $ishSession -Name "FTITLE").GetType().Name | Should BeExactly "IshMetadataFilterField"
		}
	}

	Context “Set-IshMetadataFilterField" {
		It "Parameter IshSession/Name/Level invalid" {
			{ Set-IshMetadataFilterField -IshSession "INVALIDISHSESSION" -Name "INVALIDFIELDNAME" -Level "INVALIDFIELDLEVEL" } | Should Throw
		}
		It "Parameter ValueType invalid" {
			{ Set-IshMetadataFilterField -IshSession $ishSession -Name "INVALIDFIELDNAME" -Level "INVALIDFIELDLEVEL" -ValueType "INVALIDFIELDVALUETYPE" } | Should Throw
		}
		It "Parameter Name" {
			Set-IshMetadataFilterField -IshSession $ishSession -Name "FTITLE" | Get-IshMetadataField -IshSession $ishSession -Name "FTITLE" | Should Be ""
		}
		It "Parameter Name Level" {
			Set-IshMetadataFilterField -IshSession $ishSession -Name "FTITLE" -Level Logical | Get-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical | Should Be ""
		}
		It "Parameter Name Level Operator" {
			Set-IshMetadataFilterField -IshSession $ishSession -Name "FTITLE" -Level Logical -FilterOperator Equal | Get-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical | Should Be ""
		}
		It "Parameter Name Level Operator Value" {
			$value = "MyString"
			(Set-IshMetadataFilterField -IshSession $ishSession -Name "FTITLE" -Level Logical -FilterOperator Equal -value $value | Get-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical) -eq $value | Should Be $true
		}
		It "Parameter Name Level Operator Value Twice" {
			(Set-IshMetadataFilterField -IshSession $ishSession -Name 'FISHLASTMODIFIEDON' -Level Lng -FilterOperator GreaterThanOrEqual -Value '01/01/2016' |
			Set-IshMetadataFilterField -IshSession $ishSession -Name 'FISHLASTMODIFIEDON' -Level Lng -FilterOperator LessThan -Value '02/01/2016').Length | Should Be 2
		}
		It "Parameter Name Level Operator ValueType Value" {
			$value = "VUSERADMIN"
			(Set-IshMetadataFilterField -IshSession $ishSession -Name "FAUTHOR" -Level Lng -FilterOperator Equal -ValueType Element -value $value).FilterOperator -match 'equal' | Should Be $true
		}
		It "Parameter Name Level Operator ValueType Value Twice" {
			$value = "VUSERADMIN"
			(Set-IshMetadataFilterField -IshSession $ishSession -Name 'FAUTHOR' -Level Lng -FilterOperator In -ValueType Element -Value $value |
			Set-IshMetadataFilterField -IshSession $ishSession -Name 'FAUTHOR' -Level Lng -FilterOperator Equal -ValueType Element -Value $value).Length | Should Be 2
		}
		It "Parameter IshFields Single Add" {
			$ishFields = $null
			$value = "SingleString"
			(Set-IshMetadataFilterField -IshSession $ishSession -IshField $ishFields -Name "FTITLE" -Level Logical -ValueType Element -value $value | Get-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -ValueType Element) -eq $value | Should Be $true
		}
		It "Parameter IshFields Single Add Twice" {
			$firstFiltervalue = "OrginalString"
			$ishFields = Set-IshMetadataFilterField -IshSession $ishSession -Name "FTITLE" -Level Logical -ValueType Element -value $firstFiltervalue
			$secondFiltervalue = "NewString"
			$result = Set-IshMetadataFilterField -IshSession $ishSession -IshField $ishFields -Name "FTITLE" -Level Logical -ValueType Element -value $secondFiltervalue | Get-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -ValueType Element
			(($result -eq $firstFiltervalue) -or ($result -eq $secondFiltervalue)) | Should Be $true
		}
		It "Parameter IshFields Multiple extra filter criteria are added" {
			$value = "OrginalString"
			$ishFieldA =Set-IshMetadataFilterField -IshSession $ishSession -Name "FTITLE" -Level Logical -ValueType Element -value $value
			$ishFieldB =Set-IshMetadataFilterField -IshSession $ishSession -Name "FTITLE" -Level Logical -ValueType Element -value $value
			$value = "SingleString"
			(Set-IshMetadataFilterField -IshSession $ishSession -IshField @($ishFieldA,$ishFieldB) -Name "FTITLE" -Level Logical -ValueType Element -value $value).Length | Should Be 3
		}
		It "Pipeline IshFields Single" {
			$ishFields = $null
			$value = "SingleString"
			($ishFields | Set-IshMetadataFilterField -IshSession $ishSession -Name "FTITLE" -Level Logical -ValueType Element -value $value | Get-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level Logical -ValueType Element) -eq $value | Should Be $true
		}
		It "Pipeline IshFields Multiple extra filter criteria are added" {
			$value = "OrginalString"
			$ishFieldA =Set-IshMetadataFilterField -IshSession $ishSession -Name "FTITLE" -Level Logical -ValueType Element -value $value
			$ishFieldB =Set-IshMetadataFilterField -IshSession $ishSession -Name "FTITLE" -Level Logical -ValueType Element -value $value
			$value = "SingleString"
			(@($ishFieldA,$ishFieldB) | Set-IshMetadataFilterField -IshSession $ishSession -Name "FTITLE" -Level Logical -ValueType Element -value $value).Length | Should Be 3
		}
	}
}


} finally {
	Write-Host "Cleaning Test Data and Variables"
}
