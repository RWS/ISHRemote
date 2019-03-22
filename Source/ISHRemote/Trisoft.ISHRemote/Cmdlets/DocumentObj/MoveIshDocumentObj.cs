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
using System.Linq;
using System.Management.Automation;
using Trisoft.ISHRemote.Objects;
using Trisoft.ISHRemote.Objects.Public;
using Trisoft.ISHRemote.Exceptions;
using Trisoft.ISHRemote.HelperClasses;
using System.Xml.Linq;
using System.Xml.XPath;

namespace Trisoft.ISHRemote.Cmdlets.DocumentObj
{
    /// <summary>
    /// <para type="synopsis">The Move-IshDocumentObj cmdlet moves document objects that are passed through the pipeline or determined via provided parameters from one repository folder to another folder. This commandlet allows to move all types of objects (Illustrations, Maps, etc. ), except for publication (outputs). 
    /// For publication (outputs) you need to use Move-IshPublicationOutput.</para>
    /// <para type="description">The Move-IshDocumentObj cmdlet moves document objects that are passed through the pipeline or determined via provided parameters from one repository folder to another folder. This commandlet allows to move all types of objects (Illustrations, Maps, etc. ), except for publication (outputs). For publication (outputs) you need to use Move-IshPublicationOutput.</para>
    /// </summary>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/InfoShareWS/" -PSCredential Admin
    /// Move-IshDocumentObj -LogicalId ISHPUBLILLUSTRATIONMISSING -FromIshFolder (Get-IshFolder -BaseFolder System) -ToIshFolder (Get-IshDocumentObjFolderLocation -LogicalId ISHPUBLILLUSTRATIONMISSING)
    /// </code>
    /// <para>New-IshSession will submit into SessionState, so it can be reused by this cmdlet. Moves DocumentObj (and PublicationOutput) to another folder.</para>
    /// </example>
    [Cmdlet(VerbsCommon.Move, "IshDocumentObj", SupportsShouldProcess = true)]
    [OutputType(typeof(IshDocumentObj))]
    public sealed class MoveIshDocumentObj : DocumentObjCmdlet
    {
        /// <summary>
        /// <para type="description">The IshSession variable holds the authentication and contract information. This object can be initialized using the New-IshSession cmdlet.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshObjectsGroup")]
        [ValidateNotNullOrEmpty]
        public IshSession IshSession { get; set; }

        /// <summary>
        /// <para type="description">The logical identifiers of the document objects to move</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup"), ValidateNotNullOrEmpty]
        public string[] LogicalId { get; set; }

        /// <summary>
        /// <para type="description">The folder identifier where the objects are currently located</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshObjectsGroup")]
        [ValidateNotNullOrEmpty]
        public long FromFolderId
        {
            get { return _fromFolderId; }
            set { _fromFolderId = value; }
        }

        /// <summary>
        /// <para type="description">The IshFolder object where the objects are currently located</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshObjectsGroup")]
        [ValidateNotNull]
        public IshFolder FromIshFolder
        {
            private get { return null; }  // required otherwise XmlDoc2CmdletDoc crashes with 'System.ArgumentException: Property Get method was not found.'
            set { _fromFolderId = value.IshFolderRef; }
        }

        /// <summary>
        /// <para type="description">The folder identifier where to move the document objects to</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshObjectsGroup")]
        [ValidateNotNullOrEmpty]
        public long ToFolderId
        {
            get { return _toFolderId; }
            set { _toFolderId = value; }
        }

        /// <summary>
        /// <para type="description">The IshFolder object where to move the document objects to</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshObjectsGroup")]
        [ValidateNotNull]
        public IshFolder ToIshFolder
        {
            private get { return null; }  // required otherwise XmlDoc2CmdletDoc crashes with 'System.ArgumentException: Property Get method was not found.'
            set { _toFolderId = value.IshFolderRef; }
        }

        /// <summary>
        /// <para type="description">Array with the object to move. This array can be passed through the pipeline or explicitly passed via the parameter.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "IshObjectsGroup")]
        public IshObject IshObject { get; set; }

        #region Private fields 
        /// <summary>
        /// Holds the folder card id, specified by incoming parameter (long,IShObject)
        /// </summary>
        private long _fromFolderId = -1;
        /// <summary>
        /// Holds the folder card id, specified by incoming parameter (long,IShObject)
        /// </summary>
        private long _toFolderId = -2;
        /// <summary>
        /// Initially holds incoming IshObject entries from the pipeline to correct the incorrect array-objects from Trisoft.Automation
        /// </summary>
        private readonly List<IshObject> _retrievedIshObjects = new List<IshObject>();
        #endregion

        protected override void BeginProcessing()
        {
            if (IshSession == null) { IshSession = (IshSession)SessionState.PSVariable.GetValue(ISHRemoteSessionStateIshSession); }
            if (IshSession == null) { throw new ArgumentException(ISHRemoteSessionStateIshSessionException); }
            WriteDebug($"Using IshSession[{IshSession.Name}] from SessionState.{ISHRemoteSessionStateIshSession}");
            base.BeginProcessing();
        }

        /// <summary>
        /// Process the Move-IshDocumentObj commandlet.
        /// </summary>
        /// <exception cref="TrisoftAutomationException"></exception>
        /// <exception cref="Exception"></exception>
        protected override void ProcessRecord()
        {
            try
            {
                if (IshObject != null)
                {
                    _retrievedIshObjects.Add(IshObject);
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

        /// <summary>
        /// EndProcess the Move-IshDocumentObj commandlet.
        /// </summary>
        /// <exception cref="TrisoftAutomationException"></exception>
        /// <exception cref="Exception"></exception>
        protected override void EndProcessing()
        {
          try
          {
                // Validating the input where piped objects overrules explicit LogicalId parameter
                WriteDebug("Validating");
                string[] logicalIds;
                if (_retrievedIshObjects.Count > 0)
                {
                    logicalIds = _retrievedIshObjects.Select(ishObject => ishObject.IshRef).ToArray<string>();
                }
                else
                {
                    logicalIds = LogicalId;
                }

                WriteDebug("Moving");
                if (ShouldProcess(String.Join(", ",logicalIds)))
                {
                    WriteDebug($"Moving logicalIds.length[{logicalIds.Length}] FromFolderId[{_fromFolderId}] ToFolderId[{_toFolderId}] 0/{logicalIds.Length}");
                    // Devides the list of language card ids in different lists that all have maximally MetadataBatchSize elements
                    List<List<string>> devidedlogicalIdsList = DevideListInBatches<string>(logicalIds.ToList(), IshSession.MetadataBatchSize);
                    int currentLogicalIdCount = 0;
                    foreach (List<string> logicalIdBatch in devidedlogicalIdsList)
                    {
                        // Process language card ids in batches
                        string errorReport = IshSession.Folder25.MoveObjects(_toFolderId, logicalIdBatch.ToArray(), _fromFolderId);
                        ReportErrors(errorReport);
                        currentLogicalIdCount += logicalIdBatch.Count;
                        WriteDebug($"Moving logicalIds.length[{logicalIds.Length}] FromFolderId[{_fromFolderId}] ToFolderId[{ _toFolderId}] {currentLogicalIdCount}/{logicalIds.Length}");
                    }
                }

                // Retrieve moved object(s) to write to the pipeline for a potential security/usergroup change
                WriteDebug("Retrieving");
                var returnIshObjects = new List<IshObject>();
                // Add the required fields (needed for pipe operations)
                IshFields requestedMetadata = IshSession.IshTypeFieldSetup.ToIshRequestedMetadataFields(IshSession.DefaultRequestedMetadata, ISHType, new IshFields(), Enumerations.ActionMode.Read);
                if (_retrievedIshObjects.Count > 0)
                {
                    var lngCardIds =_retrievedIshObjects.Select(ishObject => Convert.ToInt64(ishObject.ObjectRef[Enumerations.ReferenceType.Lng])).ToList();
                    WriteDebug($"Retrieving CardIds.length[{lngCardIds.Count}] RequestedMetadata.length[{requestedMetadata.ToXml().Length}] 0/{lngCardIds.Count}");
                    // Devides the list of language card ids in different lists that all have maximally MetadataBatchSize elements
                    List<List<long>> devidedlngCardIdsList = DevideListInBatches<long>(lngCardIds, IshSession.MetadataBatchSize);
                    int currentLngCardIdCount = 0;
                    foreach (List<long> lngCardIdBatch in devidedlngCardIdsList)
                    {
                        // Process language card ids in batches
                        string xmlIshObjects = IshSession.DocumentObj25.RetrieveMetadataByIshLngRefs(
                            lngCardIdBatch.ToArray(),
                            requestedMetadata.ToXml());
                        IshObjects retrievedObjects = new IshObjects(ISHType, xmlIshObjects);
                        returnIshObjects.AddRange(retrievedObjects.Objects);
                        currentLngCardIdCount += lngCardIdBatch.Count;
                        WriteDebug($"Retrieving CardIds.length[{lngCardIdBatch.Count}] RequestedMetadata.length[{requestedMetadata.ToXml().Length}] including data {currentLngCardIdCount}/{lngCardIds.Count}");
                    }
                }
                else
                {
                    // IshObject from pipeline is null. Retrieve using only provided LogicalId array
                    WriteDebug($"Retrieving LogicalId.length[{LogicalId.Length}] RequestedMetadata.length[{requestedMetadata.ToXml().Length}] 0/{LogicalId.Length}");
                    // Devides the list of language card ids in different lists that all have maximally MetadataBatchSize elements
                    List<List<string>> devidedlogicalIdsList = DevideListInBatches<string>(LogicalId.ToList(), IshSession.MetadataBatchSize);
                    int currentLogicalIdCount = 0;
                    foreach (List<string> logicalIdBatch in devidedlogicalIdsList)
                    {
                        // Process language card ids in batches
                        string xmlIshObjects = IshSession.DocumentObj25.RetrieveMetadata(logicalIdBatch.ToArray(), DocumentObj25ServiceReference.StatusFilter.ISHNoStatusFilter, "", requestedMetadata.ToXml());
                        IshObjects retrievedObjects = new IshObjects(ISHType, xmlIshObjects);
                        returnIshObjects.AddRange(retrievedObjects.Objects);
                        currentLogicalIdCount += logicalIdBatch.Count;
                        WriteDebug($"Retrieving LogicalId.length[{logicalIdBatch.Count}] RequestedMetadata.length[{requestedMetadata.ToXml().Length}] {currentLogicalIdCount}/{LogicalId.Length}");
                    }
                }

                // Write retrieved objects to pipeline
                WriteVerbose("returned object count[" + returnIshObjects.Count + "]");
                WriteObject(IshSession, ISHType, returnIshObjects.ConvertAll(x => (IshBaseObject)x), true);
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

        /// <summary>
        /// Reports back the errors, warnings and messages retrieved by API
        /// </summary>
        /// <param name="errorReport">logger/logs Xml structure</param>
        private void ReportErrors(string errorReport)
        {
            var errorReportXml = XDocument.Parse(errorReport);
            foreach (var error in errorReportXml.XPathSelectElements(@"/logger/logs/error"))
            {
                var exception = new Exception(error.Value);
                WriteError(new ErrorRecord(exception, error.Attribute("number").Value, ErrorCategory.NotSpecified, null));
            }
            foreach (var warning in errorReportXml.XPathSelectElements(@"/logger/logs/warning"))
            {
                WriteWarning(warning.Value);
            }
            foreach (var message in errorReportXml.XPathSelectElements(@"/logger/logs/message"))
            {
                WriteVerbose(message.Value);
            }
        }
    }
}
