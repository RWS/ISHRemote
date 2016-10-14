/*
* Copyright (c) 2014 All Rights Reserved by the SDL Group.
* 
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
* 
*     http://www.apache.org/licenses/LICENSE-2.0
* 
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Xsl;

using System.Collections.Generic;
using System.Management.Automation;
using Trisoft.ISHRemote.Objects;
using Trisoft.ISHRemote.Objects.Public;
using Trisoft.ISHRemote.Exceptions;
using Trisoft.ISHRemote.HelperClasses;

namespace Trisoft.ISHRemote.Cmdlets.FileProcessor
{
    /// <summary>
    /// <para type="synopsis">The New-IshDitaGeneralizedXml cmdlet generalizes all files that are passed through the pipeline or via the -FilePath parameter. Both the specialized input xml file and resulting generalized xml output file are validated. Note that a DOCTYPE needs to be present in the input xml file and resulting generalized xml output file are validated.</para>
    /// <para type="description">The New-IshDitaGeneralizedXml cmdlet generalizes all files that are passed through the pipeline or via the -FilePath parameter. Both the specialized input xml file and resulting generalized xml output file are validated. Note that a DOCTYPE needs to be present in the input xml file and resulting generalized xml output file are validated.</para>
    /// </summary>
    /// <example>
    /// <code>
    /// # This sample script will process all DITA xml files in the input folder and will generalize all DITA files either to Topic or Map.
    /// # Note that ditabase and ditaval are not supported.
    /// # Note that the xmls in the input folder needs to have a DOCTYPE.
    /// 
    /// Write-Host "Setting current directory..."
    /// [string]$currentScriptDirectory = split-path -parent $MyInvocation.MyCommand.Definition
    /// Set-Location $currentScriptDirectory
    /// [Environment]::CurrentDirectory=$currentScriptDirectory
    /// # Input folder with the specialized xmls to generalize.
    /// $inputfolder = "Data-GeneralizeDitaXml\InputFiles"	
    /// # Output folder with the generalize xmls. The successfully generalized files will get a .gen extension, the failed ones will get .err with a .log file next to them why they failed.
    /// $outputfolder = "Data-GeneralizeDitaXml\OutputFiles"    # !This folder will be deleted if it already exists!
    /// 
    /// # Location of the catalog xml that contains the specialized dtds
    /// $specializedCatalogLocation = "Data-GeneralizeDitaXml\SpecializedDTDs\catalog-alldita12dtds.xml"
    /// # Location of the catalog xml that contains the "base" dtds
    /// $generalizedCatalogLocation = "Data-GeneralizeDitaXml\GeneralizedDTDs\catalog-dita12topic&amp;maponly.xml";
    /// # File that contains a mapping between the specialized dtd and the according generalized dtd.
    /// $generalizationCatalogMappingLocation = "Data-GeneralizeDitaXml\generalization-catalog-mapping.xml"
    /// # If you would have specialized attributes from the DITA 1.2 "props" attribute, specify those attributes here to generalize them to the "props" attribute again.  Here just using modelyear, market, vehicle as an example
    /// $attributesToGeneralizeToProps = @("modelA", "modelB", "modelC")
    /// # If you would have specialized attributes from the DITA 1.2 "base" attribute, specify those attributes here to generalize them to the "base" attribute again. Here just using basea, baseb, basec as an example
    /// $attributesToGeneralizeToBase = @("basea", "baseb", "basec")
    /// if (Test-Path $outputfolder)
    /// {
    /// 	Remove-Item $outputfolder -Recurse
    /// }
    /// 
    /// # First we will copy all the files of the inputfolder to the outputfolder recursively
    /// Copy-Item -Path $inputfolder -Destination $outputfolder -recurse -force
    /// 
    /// # Read all the xml files to process from the outputfolder
    /// $filesToProcess = get-childItem $outputfolder -include *.xml -recurse
    /// 
    /// # Generalize all the files in the outputfolder
    /// # The successfully generalized files will get a .gen extension, the failed ones will get .err with a .log file next to them why they failed.	
    /// $filesToProcess | New-IshDitaGeneralizedXml `
    /// 						-SpecializedCatalogLocation $SpecializedCatalogLocation `
    /// 					   -GeneralizedCatalogLocation $GeneralizedCatalogLocation `
    /// 					   -GeneralizationCatalogMappingLocation $GeneralizationCatalogMappingLocation `
    /// 					   -AttributesToGeneralizeToProps $attributesToGeneralizeToProps `
    /// 					   -AttributesToGeneralizeToBase $attributesToGeneralizeToBase
    /// </code>
    /// <para>Generalize all DITA xml files in a certain directory</para>
    /// </example>
    /// <example>
    /// <code>
    /// $generalizesamplesrootfolder = "C:\\temp\\";
    /// $specializedCatalogLocation = $generalizesamplesrootfolder + "SpecializedDTDs\\catalog-dita-1.2.xml";
    /// $generalizedCatalogLocation = $generalizesamplesrootfolder + "GeneralizedDTDs\\catalog-dita-1.1.xml";
    /// $generalizationCatalogMappingLocation = $generalizesamplesrootfolder + "generalization-catalog-mapping.xml"; 
    /// $attributesToGeneralizeToProps = @("complexity", "visibility")          # array containing 2 elements 
    /// $attributesToGeneralizeToBase = @();          # @() = empty array
    ///     
    /// New-IshDitaGeneralizedXml -SpecializedCatalogLocation $SpecializedCatalogLocation `
    /// -GeneralizedCatalogLocation $GeneralizedCatalogLocation `
    /// -GeneralizationCatalogMappingLocation $GeneralizationCatalogMappingLocation `
    /// -FilePath $FilePath `
    /// -AttributesToGeneralizeToProps $attributesToGeneralizeToProps `
    /// -AttributesToGeneralizeToBase $attributesToGeneralizeToBase
    /// </code>
    /// <para>Generalize one DITA xml file</para>
    /// </example>
    [Cmdlet(VerbsCommon.New, "IshDitaGeneralizedXml", SupportsShouldProcess = false)]
    [OutputType(typeof(FileInfo))]
    public sealed class NewIshDitaGeneralizedXml : FileProcessorCmdlet
    {
        /// <summary>
        /// <para type="description">The filepath of the catalog with the specialized DTDs. Best is to make a separate folders with all the specialized DTD files together + a catalog with relative locations to the specialized DTD files.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "FilesGroup")]
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "FileLocationGroup")]
        [ValidateNotNullOrEmpty]
        public string SpecializedCatalogLocation { get; set; }

        /// <summary>
        /// <para type="description">The filepath of the catalog with the generalized/standard DTDs. Best is to make a separate folders with all the generalized DTD files together + a catalog with relative locations to the generalized DTD files.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "FilesGroup")]
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "FileLocationGroup")]
        [ValidateNotNullOrEmpty]
        public string GeneralizedCatalogLocation { get; set; }

        /// <summary>
        /// <para type="description">The location of a generalization mapping file. This mapping file defines the relation between the specialized dtd rootelements/public ids and the corresponding generalized dtd rootelements/public ids. It is of the following form:
        /// &lt;generalizationcataloglookup>
        ///     &lt;match rootelement="learningSummary" dtdpublicid="-//OASIS//DTD DITA Learning Summary//EN" generalizedrootelement="topic" generalizeddtdpublicid="-//OASIS//DTD DITA Topic//EN" generalizeddtdsystemid="dita-oasis/1.1/topic.dtd" />
        ///     &lt;match rootelement="learningContent" generalizedrootelement="topic" generalizeddtdpublicid="-//OASIS//DTD DITA Topic//EN" generalizeddtdsystemid="dita-oasis/1.1/topic.dtd" />
        ///     ...
        /// &lt;/generalizationcataloglookup> The rootelement, dtdpublicid or dtdsystemid attributes are used to match the specialized xml rootelement or dtd, while the generalizedrootelement, generalizeddtdpublicid or generalizeddtdsystemid are used to specify the corresponding generalized rootelement/dtd. Note that:
        /// 1) At least one of the specialized attributes is required
        /// 2) The generalizedrootelement is required
        /// 3) Either the generalizeddtdpublicid or generalizeddtdsystemid is required</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "FilesGroup")]
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "FileLocationGroup")]
        [ValidateNotNullOrEmpty]
        public string GeneralizationCatalogMappingLocation { get; set; }

        /// <summary>
        /// <para type="description">Array of attributes that are specialized from the DITA "props" attribute (and need to be generalized to it).</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "FilesGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "FileLocationGroup")]
        [ValidateNotNull]
        public string[] AttributesToGeneralizeToProps { get; set; }

        /// <summary>
        /// <para type="description">Array of attributes that are specialized from the DITA "base" attribute (and need to be generalized to it).</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "FilesGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "FileLocationGroup")]
        [ValidateNotNull]
        public string[] AttributesToGeneralizeToBase { get; set; }

        /// <summary>
        /// <para type="description">FilePath can be used to specify one specialized input xml file location.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "FileLocationGroup"), ValidateNotNullOrEmpty]
        [Alias("FileLocation")]
        public string FilePath { get; set; }

        /// <summary>
        /// <para type="description">Array of FileInfo objects that is passed through the pipeline. This parameter holds the specialized xml input files to generalize.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "FilesGroup"), ValidateNotNull]
        [Alias("Files")]
        public FileInfo[] File { get; set; }

        protected override void ProcessRecord()
        {

            try
            {             
                int current = 0;

                WriteDebug("Generalizing");

                if (!String.IsNullOrEmpty(FilePath))
                {
                    File = new FileInfo[] { new FileInfo(FilePath) };
                }

                XmlResolverUsingCatalog specializedXmlResolver = new XmlResolverUsingCatalog(SpecializedCatalogLocation);
                XmlResolverUsingCatalog generalizedXmlResolver = new XmlResolverUsingCatalog(GeneralizedCatalogLocation);
                DitaXmlGeneralizationCatalogMapping generalizationCatalogMapping = new DitaXmlGeneralizationCatalogMapping(GeneralizationCatalogMappingLocation);

                DitaXmlGeneralization ditaXMLGeneralization = new HelperClasses.DitaXmlGeneralization(specializedXmlResolver,
                    generalizedXmlResolver, generalizationCatalogMapping);

                ditaXMLGeneralization.AttributesToGeneralizeToProps = AttributesToGeneralizeToProps;
                ditaXMLGeneralization.AttributesToGeneralizeToBase = AttributesToGeneralizeToBase;

                
                List<FileInfo> outFiles = new List<FileInfo>();
                foreach (FileInfo inputFile in File)
                {
                    FileInfo outFile = null;
                    try
                    {
                        WriteParentProgress("Generalizing file[" + inputFile.FullName + "]", ++current, File.Length);

                        outFile = new FileInfo(Path.ChangeExtension(inputFile.FullName, ".gen"));
                        // file.MoveTo(file.FullName + ".bak");             
                        ditaXMLGeneralization.Generalize(inputFile, outFile);                        
                        outFiles.Add(outFile);
                        System.IO.File.Delete(inputFile.FullName);
                    }
                    catch (Exception ex)
                    {
                        WriteWarning("Generalizing file " + inputFile.FullName + " failed: " + ex.Message);
                        if (System.IO.File.Exists(Path.ChangeExtension(inputFile.FullName, ".gen")))
                        {
                            System.IO.File.Move(Path.ChangeExtension(inputFile.FullName, ".gen"), Path.ChangeExtension(inputFile.FullName, ".errxml"));
                        }
                        System.IO.File.Move(inputFile.FullName, Path.ChangeExtension(inputFile.FullName, ".err"));
                        StreamWriter errFile = System.IO.File.CreateText(Path.ChangeExtension(inputFile.FullName, ".log"));
                        errFile.Write("Generalizing file " + outFile.FullName + " failed: " + ex.Message);
                        errFile.Close();
                    }
                }
                WriteDebug("outFiles.length[" + outFiles.Count + "]");
                WriteObject(outFiles, true);
            }
            catch (TrisoftAutomationException trisoftAutomationException)
            {
                ThrowTerminatingError(new ErrorRecord(trisoftAutomationException, base.GetType().Name, ErrorCategory.InvalidOperation, null));
            }
            catch (Exception exception)
            {
                ThrowTerminatingError(new ErrorRecord(exception, base.GetType().Name, ErrorCategory.NotSpecified, null));
            }
        }
    }
}
