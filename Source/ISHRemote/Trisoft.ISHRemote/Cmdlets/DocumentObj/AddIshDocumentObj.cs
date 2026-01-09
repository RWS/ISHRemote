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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Management.Automation;
using System.ServiceModel;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using Trisoft.ISHRemote.Exceptions;
using Trisoft.ISHRemote.HelperClasses;
using Trisoft.ISHRemote.Objects;
using Trisoft.ISHRemote.Objects.Public;

namespace Trisoft.ISHRemote.Cmdlets.DocumentObj
{
    /// <summary>
    /// <para type="synopsis">The Add-IshDocumentObj cmdlet adds the new document object(s) (which include illustrations) that are passed through the pipeline or determined via provided parameters This commandlet allows to create all types of objects (Illustrations, Maps, etc. ), except for publication (outputs). 
    /// For publication (outputs) you need to use Add-IshPublicationOutput.</para>
    /// <para type="description">The Add-IshDocumentObj cmdlet adds the new document object(s) (which include illustrations) that are passed through the pipeline or determined via provided parameters This commandlet allows to create all types of objects (Illustrations, Maps, etc. ), except for publication (outputs). 
    /// For publication (outputs) you need to use Add-IshPublicationOutput.</para>
    /// </summary>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/InfoShareWS/" -PSCredential Admin
    /// Write-Host "`r`nCreate a logical, version, language level"
    /// $ditaFileContent = @"
    /// &lt;? xml version="1.0" ?>
    /// &lt;!DOCTYPE topic PUBLIC "-//OASIS//DTD DITA Topic//EN" "topic.dtd">
    /// &lt;topic>&lt;title>Enter the title of your topic here.&lt;? ish-replace-title?>&lt;/title>&lt;shortdesc>Enter a short description of your topic here(optional).&lt;/shortdesc>&lt;body>&lt;p>This is the start of your topic.&lt;/p>&lt;/body>&lt;/topic>
    /// "@
    /// $ishMetadataFields = Set-IshMetadataField -Name "FTITLE" -Level "Logical" -Value "Example ISHModule" |
    ///                      Set-IshMetadataField -Name "FCHANGES" -Level "Version" -Value "Changes text field" |
    ///                      Set-IshMetadataField -Name "FSTATUS" -Level "Lng" -Value "Draft" |
    ///                      Set-IshMetadataField -Name "FAUTHOR" -Level "Lng" -Value "admin"
    /// # add object
    /// $ishObject = Add-IshDocumentObj `
    ///              -FolderId "0" ` #use valid FolderId
    ///              -IshType "ISHModule" `
    ///              -Lng "en" `
    ///              -Metadata $ishMetadataFields `
    ///              -Edt "EDTXML" `
    ///              -FileContent $ditaFileContent
    /// $ishObject.ObjectRef| Format-Table
    /// </code>
    /// <para>New-IshSession will submit into SessionState, so it can be reused by this cmdlet. Add Module without providing LogicalId, Version and using FileContent parameter</para>
    /// </example>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/InfoShareWS/" -IshUserName "" -IshUserPassword  ""
    /// Write-Host "`r`nCreate a logical, version, language level"
    /// $timestamp = get-date -Format "yyyyMMddHHmmss"
    /// $logicalId = "MYGUID-$timestamp"
    /// $ditaFilePath = "c:\temp\Task.dita" # Template which is used to create a language level
    /// $ishMetadataFields = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level "Logical" -Value "Example ISHModule" `
    ///      | Set-IshMetadataField -IshSession $ishSession -Name "FCHANGES" -Level "Version" -Value "Changes text field" `
    ///      | Set-IshMetadataField -IshSession $ishSession -Name "FSTATUS" -Level "Lng" -Value "Draft" `
    ///      | Set-IshMetadataField -IshSession $ishSession -Name "FAUTHOR" -Level "Lng" -Value "admin"
    /// # add object
    /// $ishObject = Add-IshDocumentObj -IshSession $ishSession `
    ///         -FolderId "0" ` #use valid FolderId
    ///         -LogicalId $logicalId `
    ///         -IshType "ISHModule" `
    ///         -Version "1" `
    ///         -Lng "en" `
    ///         -Metadata $ishMetadataFields `
    ///         -Edt "EDTXML" `
    ///         -FilePath $ditaFilePath
    /// $ishObject.ObjectRef| Format-Table
    /// </code>
    /// <para>Add Module with providing LogicalId and Version</para>
    /// </example>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/InfoShareWS/" -IshUserName "" -IshUserPassword  ""
    /// Write-Host "`r`nCreate a logical, version, language level"
    /// $ditaFilePath = "c:\temp\Task.dita" # Template which is used to create a language level
    /// $ishMetadataFields = Set-IshMetadataField -IshSession $ishSession -Name "FTITLE" -Level "Logical" -Value "Example ISHModule" `
    ///      | Set-IshMetadataField -IshSession $ishSession -Name "FCHANGES" -Level "Version" -Value "Changes text field" `
    ///      | Set-IshMetadataField -IshSession $ishSession -Name "FSTATUS" -Level "Lng" -Value "Draft" `
    ///      | Set-IshMetadataField -IshSession $ishSession -Name "FAUTHOR" -Level "Lng" -Value "admin"
    /// # add object
    /// $ishObject = Add-IshDocumentObj -IshSession $ishSession `
    ///         -FolderId "0" ` #use valid FolderId
    ///         -IshType "ISHModule" `
    ///         -Lng "en" `
    ///         -Metadata $ishMetadataFields `
    ///         -Edt "EDTXML" `
    ///         -FilePath $ditaFilePath
    /// $ishObject.ObjectRef| Format-Table
    /// </code>
    /// <para>Add Module without providing LogicalId and Version</para>
    /// </example>
    [Cmdlet(VerbsCommon.Add, "IshDocumentObj", SupportsShouldProcess = true)]
    [OutputType(typeof(IshDocumentObj))]
    public sealed class AddIshDocumentObj : DocumentObjCmdlet
    {

        /// <summary>
        /// <para type="description">The IshSession variable holds the authentication and contract information. This object can be initialized using the New-IshSession cmdlet.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroupFilePath")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroupFileContent")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshObjectsGroup")]
        [ValidateNotNullOrEmpty]
        public IshSession IshSession { get; set; }

        /// <summary>
        /// <para type="description">The FolderId of the DocumentObj.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroupFilePath")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroupFileContent")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshObjectsGroup")]
        [ValidateNotNullOrEmpty]
        public long FolderId
        {
            get { return _folderId; }
            set { _folderId = value; }
        }

        /// <summary>
        /// <para type="description">The FolderId of the DocumentObj by IshFolder object</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroupFilePath")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroupFileContent")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshObjectsGroup")]
        [ValidateNotNull]
        public IshFolder IshFolder
        {
            private get { return null; }  // required otherwise XmlDoc2CmdletDoc crashes with 'System.ArgumentException: Property Get method was not found.'
            set { _folderId = value.IshFolderRef; }
        }

        /// <summary>
        /// <para type="description">The <see cref="IshType"/>.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroupFilePath")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroupFileContent")]
        [ValidateNotNullOrEmpty]
        public Enumerations.ISHType IshType
        {
            //TODO: [Should] Derive IshType from incoming IshFolder
            get { return _ishType; }
            set { _ishType = value; }
        }

        /// <summary>
        /// <para type="description">The LogicalId of the DocumentObj.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroupFilePath")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroupFileContent")]
        [ValidateNotNullOrEmpty]
        public string LogicalId
        {
            get { return _logicalId; }
            set { _logicalId = value; }
        }

        /// <summary>
        /// <para type="description">The Version of the DocumentObj.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroupFilePath")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroupFileContent")]
        [ValidateNotNullOrEmpty]
        public string Version
        {
            get { return _version; }
            set { _version = value; }
        }

        /// <summary>
        /// <para type="description">The Language of the DocumentObj.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroupFilePath")]
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroupFileContent")]
        [ValidateNotNull]
        public string Lng { get; set; }

        /// <summary>
        /// <para type="description">The Resolution of the DocumentObj.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroupFilePath")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroupFileContent")]
        [ValidateNotNull]
        public string Resolution { get; set; }

        /// <summary>
        /// <para type="description">The metadata of the DocumentObj.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroupFilePath")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroupFileContent")]
        [ValidateNotNull]
        public IshField[] Metadata { get; set; }

        /// <summary>
        /// <para type="description">The unique identifier of the Electronic Document Type for the content (e.g. EDTPDF, EDTXML, EDTHTML,...) of the new object.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroupFilePath")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroupFileContent")]
        [ValidateNotNullOrEmpty]
        public string Edt
        {
            get { return _edt; }
            set { _edt = value; }
        }

        /// <summary>
        /// <para type="description">The path to the file containing data for the DocumentObj</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroupFilePath")]
        [ValidateNotNullOrEmpty]
        public string FilePath { get; set; }

        /// <summary>
        /// <para type="description">String with XML content of the DocumentObj</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroupFileContent")]
        [ValidateNotNullOrEmpty]
        public string FileContent { get; set; }

        /// <summary>
        /// <para type="description">The <see cref="IshObjects"/>s that need to be added.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "IshObjectsGroup")]
        [AllowEmptyCollection]
        public IshObject[] IshObject { get; set; }


        #region Private fields 
        /// <summary>
        /// Private fields to store the provide a default for non-mandatory parameters
        /// </summary>
        private Enumerations.ISHType _ishType = Enumerations.ISHType.ISHNone;
        /// <summary>
        /// Logical Id will be defaulted to typical GUID-XYZ in uppercase
        /// </summary>
        private string _logicalId = ("GUID-" + Guid.NewGuid()).ToUpper();
        /// <summary>
        /// Version will be defaulted to NEW, meaning that a first or next version will be created for existing objects
        /// </summary>
        private string _version = "NEW";
        /// <summary>
        /// EDT will be defaulted to EDTXML. Needs to match up with FileContent or FilePath content.
        /// </summary>
        private string _edt = "EDTXML";
        /// <summary>
        /// Holds the folder card id, specified by incoming parameter (long,IShObject)
        /// </summary>
        private long _folderId = -1;
        /// <summary>
        /// Holds IshData object initialized either by EDT and FilePath or EDT and FileContent
        /// </summary>
        private IshData _ishData;
        #endregion

        protected override void BeginProcessing()
        {
            if (IshSession == null) { IshSession = (IshSession)SessionState.PSVariable.GetValue(ISHRemoteSessionStateIshSession); }
            if (IshSession == null) { IshSession = (IshSession)SessionState.PSVariable.GetValue(ISHRemoteSessionStateGlobalIshSession); }
            if (IshSession == null) { throw new ArgumentException(ISHRemoteSessionStateIshSessionException); }
            WriteDebug($"Using IshSession[{IshSession.Name}] from SessionState.{ISHRemoteSessionStateIshSession} or in turn SessionState.{ISHRemoteSessionStateGlobalIshSession}");
            base.BeginProcessing();
        }

        /// <summary>
        /// Process the Add-IshDocumentObj commandlet.
        /// </summary>
        /// <exception cref="TrisoftAutomationException"></exception>
        /// <exception cref="Exception"></exception>
        /// <remarks>Writes an <see cref="IshObject"/> array to the pipeline.</remarks>
        protected override void ProcessRecord()
        {
            try
            {
                // 1. Validating the input
                WriteDebug("Validating");
                
                // Parameters to retrieve added document
                List<IshObject> returnIshObjects = new List<IshObject>();               

                WriteDebug("Adding");

                if (IshObject != null)
                {
                    int current = 0;
                    foreach (IshObject ishObject in IshObject)
                    {
                        WriteDebug($"lngRef[{ishObject.ObjectRef[Enumerations.ReferenceType.Lng]}] {++current}/{IshObject.Length}");

                        // 2b. Add using IshObject pipeline
                        // Initialize from the object
                        string logicalId = ishObject.IshRef;
                        string ishObjectVersion = ishObject.IshFields.GetFieldValue("VERSION", Enumerations.Level.Version, Enumerations.ValueType.Value);
                        //TODO: [Should] Respect LanguageApplicability, which means take the first entry from the ishfield DOC-LANGUAGE value
                        string ishObjectLanguage = ishObject.IshFields.GetFieldValue("DOC-LANGUAGE", Enumerations.Level.Lng, Enumerations.ValueType.Value);
                        string ishObjectResolution = ishObject.IshFields.GetFieldValue("FRESOLUTION", Enumerations.Level.Lng, Enumerations.ValueType.Value);
                        var metadata = IshSession.IshTypeFieldSetup.ToIshMetadataFields(ISHType, ishObject.IshFields, Enumerations.ActionMode.Create);
                        
                        // Add
                        WriteDebug($"LogicalId[{logicalId}] Version[{ishObjectVersion}] Lng[{ishObjectLanguage}] Resolution[{ishObjectResolution}] Metadata.length[{metadata.ToXml().Length}] dataSize[{ishObject.IshData.Size()}]");
                        DocumentObj25ServiceReference.CreateResponse response = null;
                        if (ShouldProcess(logicalId + "=" + ishObjectVersion + "=" + ishObjectLanguage + "=" + ishObjectResolution))
                        {
                            response = IshSession.DocumentObj25.Create(new DocumentObj25ServiceReference.CreateRequest(
                                    _folderId,
                                    ishObject.IshType.ToString(),
                                    logicalId,
                                    ishObjectVersion,
                                    ishObjectLanguage,
                                    ishObjectResolution,
                                    metadata.ToXml(),
                                    ishObject.IshData.Edt,
                                    ishObject.IshData.ByteArray));
                        }

                        IshFields requestedMetadata = IshSession.IshTypeFieldSetup.ToIshRequestedMetadataFields(IshSession.DefaultRequestedMetadata, ISHType, metadata, Enumerations.ActionMode.Read);
                        var response2 = IshSession.DocumentObj25.GetMetadata(new DocumentObj25ServiceReference.GetMetadataRequest(
                            response.logicalId, response.version, ishObjectLanguage, ishObjectResolution,
                            requestedMetadata.ToXml()));
                        string xmlIshObjects = response2.xmlObjectList;
                        IshObjects retrievedObjects = new IshObjects(ISHType, xmlIshObjects);
                        returnIshObjects.AddRange(retrievedObjects.Objects);
                    }
                }
                // 2a. LogicalId is provided
                else
                {
                    string resolution = Resolution ?? "";
                    if ((FileContent != null) && (FilePath == null))
                    {
                        if (!_edt.Equals("EDTXML", StringComparison.Ordinal))
                        {
                            throw new NotImplementedException("FileContent parameter is only supported with Edt='EDTXML'");
                        }

                        var doc = XDocument.Parse(FileContent, LoadOptions.PreserveWhitespace);
                        var ms = new MemoryStream();
                        doc.Save(ms, SaveOptions.DisableFormatting);
                        _ishData = new IshData(_edt, ms.ToArray());
                    }
                    if ((FileContent == null) && (FilePath != null))
                    {
                        _ishData = new IshData(_edt, FilePath);
                    }

                    var metadata = IshSession.IshTypeFieldSetup.ToIshMetadataFields(ISHType, new IshFields(Metadata), Enumerations.ActionMode.Create);
                    WriteDebug($"LogicalId[{LogicalId}] Version[{Version}] Lng[{Lng}] Resolution[{resolution}] Metadata.length[{metadata.ToXml().Length}] byteArray[{_ishData.Size()}]");
                    DocumentObj25ServiceReference.CreateResponse response = null;
                    if (ShouldProcess(LogicalId + "=" + Version + "=" + Lng + "=" + resolution))
                    {
                        response = IshSession.DocumentObj25.Create(new DocumentObj25ServiceReference.CreateRequest(
                            _folderId,
                            IshType.ToString(),
                            LogicalId,
                            Version,
                            Lng,
                            resolution,
                            metadata.ToXml(),
                            _ishData.Edt,
                            _ishData.ByteArray));
                        IshFields requestedMetadata = IshSession.IshTypeFieldSetup.ToIshRequestedMetadataFields(IshSession.DefaultRequestedMetadata, ISHType, metadata, Enumerations.ActionMode.Read);
                        var response2 = IshSession.DocumentObj25.GetMetadata(new DocumentObj25ServiceReference.GetMetadataRequest(
                            response.logicalId, response.version, Lng, resolution,
                            requestedMetadata.ToXml()));
                        string xmlIshObjects = response2.xmlObjectList;
                        IshObjects retrievedObjects = new IshObjects(ISHType, xmlIshObjects);
                        returnIshObjects.AddRange(retrievedObjects.Objects);
                    }
                }

                WriteVerbose("returned object count[" + returnIshObjects.Count + "]");
                WriteObject(IshSession, ISHType, returnIshObjects.ConvertAll(x => (IshBaseObject)x), true);
            }
            catch (TrisoftAutomationException trisoftAutomationException)
            {
                ThrowTerminatingError(new ErrorRecord(trisoftAutomationException, base.GetType().Name, ErrorCategory.InvalidOperation, null));
            }
            catch (AggregateException aggregateException)
            {
                var flattenedAggregateException = aggregateException.Flatten();
                WriteWarning(flattenedAggregateException.ToString());
                ThrowTerminatingError(new ErrorRecord(flattenedAggregateException, base.GetType().Name, ErrorCategory.NotSpecified, null));
            }
            catch (TimeoutException timeoutException)
            {
                WriteVerbose("TimeoutException Message[" + timeoutException.Message + "] StackTrace[" + timeoutException.StackTrace + "]");
                ThrowTerminatingError(new ErrorRecord(timeoutException, base.GetType().Name, ErrorCategory.OperationTimeout, null));
            }
            catch (CommunicationException communicationException)
            {
                WriteVerbose("CommunicationException Message[" + communicationException.Message + "] StackTrace[" + communicationException.StackTrace + "]");
                ThrowTerminatingError(new ErrorRecord(communicationException, base.GetType().Name, ErrorCategory.OperationStopped, null));
            }
            catch (Exception exception)
            {
                ThrowTerminatingError(new ErrorRecord(exception, base.GetType().Name, ErrorCategory.NotSpecified, null));
            }
        }
    }
}
