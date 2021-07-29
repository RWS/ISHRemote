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
    /// <para type="synopsis">The New-IshDitaGeneralizedXml cmdlet generalizes all incoming files. Both the specialized input xml file and resulting generalized xml output file are validated.</para>
    /// <para type="description">The New-IshDitaGeneralizedXml cmdlet generalizes all incoming files. Both the specialized input xml file and resulting generalized xml output file are validated. Note that a DOCTYPE needs to be present in the input xml file and resulting generalized xml output file are validated.</para>
    /// </summary>
    /// <example>
    /// <code>
    /// # This sample script will process all DITA xml files in the input folder and will generalize all DITA files either to Topic or Map.
    /// # Note that ditabase and ditaval are not supported.
    /// # Note that the xmls in the input folder needs to have a DOCTYPE.
    /// 
    /// # Input folder with the specialized xmls to generalize.
    /// $inputfolder = "Data-GeneralizeDitaXml\InputFiles"	
    /// # Output folder with the generalize xmls.
    /// $outputfolder = "Data-GeneralizeDitaXml\OutputFiles"
    /// 
    /// # Location of the catalog xml that contains the specialized dtds
    /// $specializedCatalogLocation = "Samples\Data-GeneralizeDitaXml\SpecializedDTDs\catalog-alldita12dtds.xml"
    /// # Location of the catalog xml that contains the "base" dtds
    /// $generalizedCatalogLocation = "Samples\Data-GeneralizeDitaXml\GeneralizedDTDs\catalog-dita12topic&amp;maponly.xml";
    /// # File that contains a mapping between the specialized dtd and the according generalized dtd.
    /// $generalizationCatalogMappingLocation = "Samples\Data-GeneralizeDitaXml\generalization-catalog-mapping.xml"
    /// # If you would have specialized attributes from the DITA 1.2 "props" attribute, specify those attributes here to generalize them to the "props" attribute again.  Here just using modelyear, market, vehicle as an example
    /// $attributesToGeneralizeToProps = @("modelA", "modelB", "modelC")
    /// # If you would have specialized attributes from the DITA 1.2 "base" attribute, specify those attributes here to generalize them to the "base" attribute again. Here just using basea, baseb, basec as an example
    /// $attributesToGeneralizeToBase = @("basea", "baseb", "basec")
    /// 
    /// # Read all the xml files to process and generalize all the files in the outputfolder
    /// Get-ChildItem $inputfolder -Include *.xml -Recurse |
    /// New-IshDitaGeneralizedXml -SpecializedCatalogFilePath $SpecializedCatalogFilePath `
    /// 					      -GeneralizedCatalogFilePath $GeneralizedCatalogFilePath `
    /// 					      -GeneralizationCatalogMappingFilePath $GeneralizationCatalogMappingFilePath `
    /// 					      -AttributesToGeneralizeToProps $attributesToGeneralizeToProps `
    /// 					      -AttributesToGeneralizeToBase $attributesToGeneralizeToBase `
    /// 					      -FolderPath $outputfolder
    /// </code>
    /// <para>Generalize all DITA xml files in a certain directory</para>
    /// </example>
    /// <example>
    /// <code>
    /// $someLearningAssessmentFilePath = "Samples\InputFiles\Learning\LearningAssessment.xml"
    /// $outputfolder = "C:\temp\"
    /// $specializedCatalogLocation = "Samples\SpecializedDTDs\catalog-dita-1.2.xml"
    /// $generalizedCatalogLocation = "Samples\GeneralizedDTDs\catalog-dita-1.1.xml"
    /// $generalizationCatalogMappingLocation = "Samples\generalization-catalog-mapping.xml";
    /// $attributesToGeneralizeToProps = @("complexity", "visibility")          # array containing 2 elements 
    /// $attributesToGeneralizeToBase = @();          # @() = empty array
    ///     
    /// New-IshDitaGeneralizedXml -SpecializedCatalogFilePath $SpecializedCatalogFilePath `
    ///                           -GeneralizedCatalogFilePath $GeneralizedCatalogFilePath `
    ///                           -GeneralizationCatalogMappingFilePath $GeneralizationCatalogMappingFilePath `
    ///                           -AttributesToGeneralizeToProps $attributesToGeneralizeToProps `
    ///                           -AttributesToGeneralizeToBase $attributesToGeneralizeToBase
    ///                           -FilePath $someLearningAssessmentFilePath `
    /// 					      -FolderPath $outputfolder
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
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false)]
        [ValidateNotNullOrEmpty]
        public string SpecializedCatalogFilePath { get; set; }

        /// <summary>
        /// <para type="description">The filepath of the catalog with the generalized/standard DTDs. Best is to make a separate folders with all the generalized DTD files together + a catalog with relative locations to the generalized DTD files.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false)]
        [ValidateNotNullOrEmpty]
        public string GeneralizedCatalogFilePath { get; set; }

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
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false)]
        [ValidateNotNullOrEmpty]
        public string GeneralizationCatalogMappingFilePath { get; set; }

        /// <summary>
        /// <para type="description">Array of attributes that are specialized from the DITA "props" attribute (and need to be generalized to it).</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false)]
        public string[] AttributesToGeneralizeToProps { get; set; }

        /// <summary>
        /// <para type="description">Array of attributes that are specialized from the DITA "base" attribute (and need to be generalized to it).</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false)]
        public string[] AttributesToGeneralizeToBase { get; set; }

        /// <summary>
        /// <para type="description">FilePath can be used to specify one or more specialized input xml file locations.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true)]
        [ValidateNotNullOrEmpty]
        public string[] FilePath { get; set; }

        /// <summary>
        /// <para type="description">Folder on the Windows filesystem where to store the new data files</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = false)]
        [ValidateNotNullOrEmpty]
        public string FolderPath { get; set; }

        #region Private fields
        /// <summary>
        /// Single conversion object
        /// </summary>
        private DitaXmlGeneralization _ditaXMLGeneralization = null;
        /// <summary>
        /// Collection of the files to process
        /// </summary>
        private readonly List<FileInfo> _files = new List<FileInfo>();
        #endregion
        

        protected override void BeginProcessing()
        {
            //Validate all incoming catalogs once
            WriteDebug("Loading SpecializedCatalogFilePath[${SpecializedCatalogFilePath}]");
            XmlResolverUsingCatalog specializedXmlResolver = new XmlResolverUsingCatalog(SpecializedCatalogFilePath);
            WriteDebug("Loading GeneralizedCatalogFilePath[${GeneralizedCatalogFilePath}]");
            XmlResolverUsingCatalog generalizedXmlResolver = new XmlResolverUsingCatalog(GeneralizedCatalogFilePath);
            WriteDebug("Loading GeneralizationCatalogMappingFilePath[${GeneralizationCatalogMappingFilePath}]");
            DitaXmlGeneralizationCatalogMapping generalizationCatalogMapping = new DitaXmlGeneralizationCatalogMapping(GeneralizationCatalogMappingFilePath);
            _ditaXMLGeneralization = new HelperClasses.DitaXmlGeneralization(specializedXmlResolver, generalizedXmlResolver, generalizationCatalogMapping);
            if (AttributesToGeneralizeToProps != null)
            { 
                WriteDebug("Loading AttributesToGeneralizeToProps[" + String.Join(",", AttributesToGeneralizeToProps) + "]");
                _ditaXMLGeneralization.AttributesToGeneralizeToProps = AttributesToGeneralizeToProps;
            }
            if (AttributesToGeneralizeToBase != null)
            { 
                WriteDebug("Loading AttributesToGeneralizeToBase[" + String.Join(",", AttributesToGeneralizeToBase) + "]");
                _ditaXMLGeneralization.AttributesToGeneralizeToBase = AttributesToGeneralizeToBase;
            }

            if (!Directory.Exists(FolderPath))
            {
                WriteDebug("Creating FolderPath[${FolderPath}]");
                string tempLocation = Directory.CreateDirectory(FolderPath).FullName;
            }
            base.BeginProcessing();
        }

        /// <summary>
        /// Process the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            try
            {
                foreach (string filePath in FilePath)
                {
                    _files.Add(new FileInfo(filePath));
                }
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

        protected override void EndProcessing()
        {
            try
            {             
                int current = 0;
                WriteDebug("Generalizing _files.Count[" + _files.Count + "]");
                foreach (FileInfo inputFile in _files)
                {
                    // Could be nicer, but currently loosing inputFile relative files and dropping all in OutputFolder
                    FileInfo outputFile = new FileInfo(Path.Combine(FolderPath, inputFile.Name));
                    try
                    {
                        WriteDebug("Generalizing inputFile[" + inputFile.FullName + "] to outputFile[" + outputFile.FullName + "]");
                        WriteParentProgress("Generalizing inputFile["+inputFile.FullName+"] to outputFile["+ outputFile.FullName +"]", ++current, FilePath.Length);
                        _ditaXMLGeneralization.Generalize(inputFile, outputFile);
                        WriteObject(outputFile);
                    }
                    catch (Exception ex)
                    {
                        WriteWarning("Generalizing inputFile[" + inputFile.FullName + "] to outputFile[" + outputFile.FullName + "] failed: " + ex.Message);
                    }
                }
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
