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
using System.IO;
using System.Collections.Generic;
using System.Management.Automation;
using System.Xml.Schema;
using Trisoft.ISHRemote.Exceptions;
using Trisoft.ISHRemote.HelperClasses;
using System.Xml;

namespace Trisoft.ISHRemote.Cmdlets.FileProcessor
{
    /// <summary>
    /// <para type="synopsis">The Test-IshValidFile checks wellformedness and validness using the given xml catalog.</para>
    /// <para type="description">Quick test to validate the given xml using the DocTypes/DTDs specified through catalog.xml.</para>
    /// </summary>
    /// <example>
    /// <code>Get-ChildItem -Path C:\temp\*.xml | Test-IshValidXml -XmlCatalogFilePath C:\InfoShare\WebDITA\Author\ASP\DocTypes\catalog.xml
    /// </code>
    /// <para>All xml files will get a Boolean True/False back from the Test-cmdlet using a local file system catalog and DTDs.</para>
    /// </example>
    /// <example>
    /// <code>Test-IshValidXml -XmlCatalogFilePath https://example.com/ISHCM/DocTypes/catalog.xml -FilePath C:\temp\somefile.xml
    /// </code>
    /// <para>Single xml file will get a single Boolean True/False back from the Test-cmdlet using a remote https accessible catalog and DTDs. Note that this option is SLOW as every catalog, dtd, ent, mod file required will be downloaded over-and-over again - but it does work.</para>
    /// </example>
    [Cmdlet(VerbsDiagnostic.Test, "IshValidXml", SupportsShouldProcess = false)]
    [OutputType(typeof(FileInfo))]
    public sealed class TestIshValidXml : FileProcessorCmdlet
    {
        /// <summary>
        /// <para type="description">XmlCatalogFilePath should point to the leading catalog.xml file. For example C:\InfoShare\Web\Author\ASP\DocTypes\catalog.xml</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false)]
        [ValidateNotNullOrEmpty]
        public string XmlCatalogFilePath { get; set; }

        /// <summary>
        /// <para type="description">FilePath can be used to specify one or more input xml file location.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true)]
        [ValidateNotNullOrEmpty]
        public string[] FilePath { get; set; }

        #region Private fields
        private XmlResolverUsingCatalog _xmlResolverUsingCatalog;
        /// <summary>
        /// XmlReaderSettings that hold the processing types and xml catalog references.
        /// </summary>
        private readonly XmlReaderSettings _xmlReaderSettings = new XmlReaderSettings();
        /// <summary>
        /// Collection of the files to process
        /// </summary>
        private readonly List<FileInfo> _files = new List<FileInfo>();
        #endregion

        /// <summary>
        /// Handler method for validation errors. Throws an exception when an error is encountered
        /// </summary>
        /// <param name="sender">Sender of the validation event</param>
        /// <param name="args">Validation error arguments</param>
        private static void ValidationHandler(object sender, System.Xml.Schema.ValidationEventArgs args)
        {
            switch (args.Severity)
            {
                case System.Xml.Schema.XmlSeverityType.Error:
                    XmlSchemaException xmlSchemaException = args.Exception as XmlSchemaException;
                    if (xmlSchemaException != null)
                    {
                        throw new InvalidOperationException("'" + xmlSchemaException.Message + "' at line " + xmlSchemaException.LineNumber + " position " + xmlSchemaException.LinePosition);
                    }
                    else
                    {
                        throw new InvalidOperationException("'" + args.Message + "'");
                    }
                case System.Xml.Schema.XmlSeverityType.Warning:
                    if (args.Message == "No DTD found.")       // Unfortunately there does not seem to be a typed exception for not having a DTD, so need to test the message :-(
                    {
                        throw new InvalidOperationException("'" + args.Message + "'");
                    }
                    // Else: Do nothing
                    break;
            }
        }

        protected override void BeginProcessing()
        {
            WriteDebug("Loading XmlResolverUsingCatalog[${XmlCatalogFilePath}]");
            _xmlResolverUsingCatalog = new XmlResolverUsingCatalog(XmlCatalogFilePath);
            // When ProhibitDTD = true, the following error occurs:
            // "For security reasons DTD is prohibited in this XML document. To enable DTD processing set the ProhibitDtd property on XmlReaderSettings to false and pass the settings into XmlReader.Create method."
            WriteDebug("Loading XmlReaderSettings");
            _xmlReaderSettings.ConformanceLevel = ConformanceLevel.Auto;
            _xmlReaderSettings.DtdProcessing = DtdProcessing.Parse;
            _xmlReaderSettings.ValidationType = ValidationType.DTD;
            _xmlReaderSettings.ValidationEventHandler += new System.Xml.Schema.ValidationEventHandler(ValidationHandler);
            _xmlReaderSettings.XmlResolver = _xmlResolverUsingCatalog;
            _xmlReaderSettings.IgnoreComments = false;
            _xmlReaderSettings.IgnoreProcessingInstructions = false;
            _xmlReaderSettings.IgnoreWhitespace = false;
            _xmlReaderSettings.CloseInput = true;
            base.BeginProcessing();
        }

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
                WriteDebug("Validating _files.Count[" + _files.Count + "]");
                foreach (FileInfo inputFile in _files)
                {
                    WriteDebug("Validating inputFile[" + inputFile.FullName + "]");
                    WriteParentProgress("Validating inputFile[" + inputFile.FullName + "]", ++current, _files.Count);
                    try
                    {
                        using (StreamReader streamReader = new StreamReader(inputFile.FullName))
                        {
                            using (XmlReader xmlReader = XmlReader.Create(streamReader, _xmlReaderSettings))
                            {
                                while (xmlReader.Read()) { }
                            }
                        }
                        WriteObject(true);
                    }
                    catch (Exception ex)
                    {
                        WriteWarning("Validating inputFile[" + inputFile.FullName + "] failed: " + ex.Message);
                        WriteObject(false);
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
            finally
            {
                base.EndProcessing();
            }
        }
    }
}
