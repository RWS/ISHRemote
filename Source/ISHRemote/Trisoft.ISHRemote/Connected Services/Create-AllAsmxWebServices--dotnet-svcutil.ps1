
# Prequisite:
# dotnet tool install --global dotnet-svcutil

$webServicesBaseUrl = "https://lvndevdemeyer01.global.sdl.corp/ISHWSDita"
$connectedServicesFolderPath = "C:\GITHUB\ISHRemote\Source\ISHRemote\Trisoft.ISHRemote\Connected Services"
Set-Location $connectedServicesFolderPath

Get-ChildItem -Recurse -Filter Reference.cs | Remove-Item

$env:DOTNET_SVCUTIL_TELEMETRY_OPTOUT=1
& dotnet-svcutil "$webServicesBaseUrl/annotation25.asmx" --outputDir "$connectedServicesFolderPath\Trisoft.ISHRemote.Annotation25ServiceReference" --outputFile "$connectedServicesFolderPath\Trisoft.ISHRemote.Annotation25ServiceReference\Reference.cs" --namespace *,Trisoft.ISHRemote.Annotation25ServiceReference --serializer XmlSerializer --sync --targetFramework netstandard2.0
& dotnet-svcutil "$webServicesBaseUrl/application25.asmx" --outputDir "$connectedServicesFolderPath\Trisoft.ISHRemote.Application25ServiceReference" --outputFile "$connectedServicesFolderPath\Trisoft.ISHRemote.Application25ServiceReference\Reference.cs" --namespace *,Trisoft.ISHRemote.Application25ServiceReference --serializer XmlSerializer --sync --targetFramework netstandard2.0
& dotnet-svcutil "$webServicesBaseUrl/backgroundtask25.asmx" --outputDir "$connectedServicesFolderPath\Trisoft.ISHRemote.BackgroundTask25ServiceReference" --outputFile "$connectedServicesFolderPath\Trisoft.ISHRemote.BackgroundTask25ServiceReference\Reference.cs" --namespace *,Trisoft.ISHRemote.BackgroundTask25ServiceReference --serializer XmlSerializer --sync --targetFramework netstandard2.0
& dotnet-svcutil "$webServicesBaseUrl/baseline25.asmx" --outputDir "$connectedServicesFolderPath\Trisoft.ISHRemote.Baseline25ServiceReference" --outputFile "$connectedServicesFolderPath\Trisoft.ISHRemote.Baseline25ServiceReference\Reference.cs" --namespace *,Trisoft.ISHRemote.Baseline25ServiceReference --serializer XmlSerializer --sync --targetFramework netstandard2.0
& dotnet-svcutil "$webServicesBaseUrl/documentobj25.asmx" --outputDir "$connectedServicesFolderPath\Trisoft.ISHRemote.DocumentObj25ServiceReference" --outputFile "$connectedServicesFolderPath\Trisoft.ISHRemote.DocumentObj25ServiceReference\Reference.cs" --namespace *,Trisoft.ISHRemote.DocumentObj25ServiceReference --serializer XmlSerializer --sync --targetFramework netstandard2.0
& dotnet-svcutil "$webServicesBaseUrl/edt25.asmx" --outputDir "$connectedServicesFolderPath\Trisoft.ISHRemote.EDT25ServiceReference" --outputFile "$connectedServicesFolderPath\Trisoft.ISHRemote.EDT25ServiceReference\Reference.cs" --namespace *,Trisoft.ISHRemote.EDT25ServiceReference --serializer XmlSerializer --sync --targetFramework netstandard2.0
& dotnet-svcutil "$webServicesBaseUrl/eventmonitor25.asmx" --outputDir "$connectedServicesFolderPath\Trisoft.ISHRemote.EventMonitor25ServiceReference" --outputFile "$connectedServicesFolderPath\Trisoft.ISHRemote.EventMonitor25ServiceReference\Reference.cs" --namespace *,Trisoft.ISHRemote.EventMonitor25ServiceReference --serializer XmlSerializer --sync --targetFramework netstandard2.0
& dotnet-svcutil "$webServicesBaseUrl/folder25.asmx" --outputDir "$connectedServicesFolderPath\Trisoft.ISHRemote.Folder25ServiceReference" --outputFile "$connectedServicesFolderPath\Trisoft.ISHRemote.Folder25ServiceReference\Reference.cs" --namespace *,Trisoft.ISHRemote.Folder25ServiceReference --serializer XmlSerializer --sync --targetFramework netstandard2.0
& dotnet-svcutil "$webServicesBaseUrl/listofvalues25.asmx" --outputDir "$connectedServicesFolderPath\Trisoft.ISHRemote.ListOfValues25ServiceReference" --outputFile "$connectedServicesFolderPath\Trisoft.ISHRemote.ListOfValues25ServiceReference\Reference.cs" --namespace *,Trisoft.ISHRemote.ListOfValues25ServiceReference --serializer XmlSerializer --sync --targetFramework netstandard2.0
& dotnet-svcutil "$webServicesBaseUrl/metadatabinding25.asmx" --outputDir "$connectedServicesFolderPath\Trisoft.ISHRemote.MetadataBinding25ServiceReference" --outputFile "$connectedServicesFolderPath\Trisoft.ISHRemote.MetadataBinding25ServiceReference\Reference.cs" --namespace *,Trisoft.ISHRemote.MetadataBinding25ServiceReference --serializer XmlSerializer --sync --targetFramework netstandard2.0
& dotnet-svcutil "$webServicesBaseUrl/outputformat25.asmx" --outputDir "$connectedServicesFolderPath\Trisoft.ISHRemote.OutputFormat25ServiceReference" --outputFile "$connectedServicesFolderPath\Trisoft.ISHRemote.OutputFormat25ServiceReference\Reference.cs" --namespace *,Trisoft.ISHRemote.OutputFormat25ServiceReference --serializer XmlSerializer --sync --targetFramework netstandard2.0
& dotnet-svcutil "$webServicesBaseUrl/publicationoutput25.asmx" --outputDir "$connectedServicesFolderPath\Trisoft.ISHRemote.PublicationOutput25ServiceReference" --outputFile "$connectedServicesFolderPath\Trisoft.ISHRemote.PublicationOutput25ServiceReference\Reference.cs" --namespace *,Trisoft.ISHRemote.PublicationOutput25ServiceReference --serializer XmlSerializer --sync --targetFramework netstandard2.0
& dotnet-svcutil "$webServicesBaseUrl/search25.asmx" --outputDir "$connectedServicesFolderPath\Trisoft.ISHRemote.Search25ServiceReference" --outputFile "$connectedServicesFolderPath\Trisoft.ISHRemote.Search25ServiceReference\Reference.cs" --namespace *,Trisoft.ISHRemote.Search25ServiceReference --serializer XmlSerializer --sync --targetFramework netstandard2.0
& dotnet-svcutil "$webServicesBaseUrl/settings25.asmx" --outputDir "$connectedServicesFolderPath\Trisoft.ISHRemote.Settings25ServiceReference" --outputFile "$connectedServicesFolderPath\Trisoft.ISHRemote.Settings25ServiceReference\Reference.cs" --namespace *,Trisoft.ISHRemote.Settings25ServiceReference --serializer XmlSerializer --sync --targetFramework netstandard2.0
& dotnet-svcutil "$webServicesBaseUrl/translationjob25.asmx" --outputDir "$connectedServicesFolderPath\Trisoft.ISHRemote.TranslationJob25ServiceReference" --outputFile "$connectedServicesFolderPath\Trisoft.ISHRemote.TranslationJob25ServiceReference\Reference.cs" --namespace *,Trisoft.ISHRemote.TranslationJob25ServiceReference --serializer XmlSerializer --sync --targetFramework netstandard2.0
& dotnet-svcutil "$webServicesBaseUrl/translationtemplate25.asmx" --outputDir "$connectedServicesFolderPath\Trisoft.ISHRemote.TranslationTemplate25ServiceReference" --outputFile "$connectedServicesFolderPath\Trisoft.ISHRemote.TranslationTemplate25ServiceReference\Reference.cs" --namespace *,Trisoft.ISHRemote.TranslationTemplate25ServiceReference --serializer XmlSerializer --sync --targetFramework netstandard2.0
& dotnet-svcutil "$webServicesBaseUrl/user25.asmx" --outputDir "$connectedServicesFolderPath\Trisoft.ISHRemote.User25ServiceReference" --outputFile "$connectedServicesFolderPath\Trisoft.ISHRemote.User25ServiceReference\Reference.cs" --namespace *,Trisoft.ISHRemote.User25ServiceReference --serializer XmlSerializer --sync --targetFramework netstandard2.0
& dotnet-svcutil "$webServicesBaseUrl/usergroup25.asmx" --outputDir "$connectedServicesFolderPath\Trisoft.ISHRemote.UserGroup25ServiceReference" --outputFile "$connectedServicesFolderPath\Trisoft.ISHRemote.UserGroup25ServiceReference\Reference.cs" --namespace *,Trisoft.ISHRemote.UserGroup25ServiceReference --serializer XmlSerializer --sync --targetFramework netstandard2.0
& dotnet-svcutil "$webServicesBaseUrl/userrole25.asmx" --outputDir "$connectedServicesFolderPath\Trisoft.ISHRemote.UserRole25ServiceReference" --outputFile "$connectedServicesFolderPath\Trisoft.ISHRemote.UserRole25ServiceReference\Reference.cs" --namespace *,Trisoft.ISHRemote.UserRole25ServiceReference --serializer XmlSerializer --sync --targetFramework netstandard2.0

foreach ($file in (Get-ChildItem -Recurse -Filter Reference.cs))
{
    Write-Warning ("Processing $file")
    $content = $file | Get-Content -Raw
    $content = $content.Replace("$webServicesBaseUrl", 'https://ish.example.com/ISHWS')
    $content | Set-Content $file
}

foreach ($file in (Get-ChildItem -Recurse -Filter dotnet-svcutil.params.json))
{
    Write-Warning ("Processing $file")
    $content = $file | Get-Content -Raw
    $content = $content.Replace("$webServicesBaseUrl", 'https://ish.example.com/ISHWS')
    $content | Set-Content $file
}

