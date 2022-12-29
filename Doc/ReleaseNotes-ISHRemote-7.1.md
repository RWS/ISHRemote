# Release Notes of ISHRemote v7.1

High level release notes are on [Github](https://github.com/rws/ISHRemote/releases/tag/v7.1), below the most detailed release notes we have :)

Remember
* All C# source code of the ISHRemote library is online at [master](https://github.com/rws/ISHRemote/tree/master/Source/ISHRemote/Trisoft.ISHRemote), including handling of WS-Trust protocol ([InfoShareWcfConnection.cs](https://github.com/rws/ISHRemote/tree/master/Source/ISHRemote/Trisoft.ISHRemote/InfoShareWcfConnection.cs)) in a NET 4.8 and NET 6.0+ style.
* All PowerShell-based Pester integration tests are located per cmdlet complying with the `*.tests.ps1` file naming convention. See for example [AddIshDocumentObj.Tests.ps1](https://github.com/rws/ISHRemote/tree/master/Source/ISHRemote/Trisoft.ISHRemote/Cmdlets/DocumentObj/AddIshDocumentObj.Tests.ps1) or [TestIshValidXml.Tests.ps1](https://github.com/rws/ISHRemote/tree/master/Source/ISHRemote/Trisoft.ISHRemote/Cmdlets/FileProcessor/TestIshValidXml.Tests.ps1)

The below text describes the delta compared to fielded release ISHRemote v7.0.

## General

This release inherits the v0.1 to v0.14 up to v7.0 development branch and features. By enabling PowerShell 7+ powered by NET 6+ next to existing Windows PowerShell 5.1 powered by NET Framework 4.8; we had to do some breaking changes forced by platform support. Most cmdlets and business logic are fully compatible except around authentication (`New-IshSession`, `Test-IshSession` and `New-IshObfuscatedFile`).

* work-in-progress

## Implementation Details

* Help of cmdlet `New-IshSession` was still suggesting obsolete parameter `-WsTrustIssuerUrl` in examples

## Breaking Changes - Cmdlets

Again, most cmdlets and business logic are fully compatible, except the below:

1. work-in-progress

## Breaking Changes - Requirements

* work-in-progress


## Quality Assurance

Added more Invoke-Pester 5.3.0 Tests, see Github actions for the Windows PowerShell 5.1 and PowerShell 7+ hosts where
* the skipped are about SslPolicyErrors testing
* the failed are about IMetadata bound fields (issue #58)

Below is not an official performance compare, but a recurring thing noticed along the way. Using the same client machine, same ISHRemote build and same backend but different PowerShell hosts we noticed a considerable speed up of the Pester tests.

| Name                     | Client Platform                     | Server Platform       | Test Results         |
|--------------------------|-------------------------------------|----------------------|----------------|
| ISHRemote 6.0.9523.0     | Windows PowerShell 5.1 on .NET 4.8  | SOAP-WCF and WS-Trust | Tests completed in 353.57s AND                                                                                Tests Passed: 917, Failed: 0, Skipped: 8 NotRun: 0 |
| ISHRemote 6.0.9523.0     | PowerShell 7.3.0 on .NET 7.0.0      | SOAP-WCF and WS-Trust | Tests completed in 305.46s AND Tests Passed: 921, Failed: 0, Skipped: 8 NotRun: 0 |
