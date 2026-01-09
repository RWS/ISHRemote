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
using System.ServiceModel;
using Trisoft.ISHRemote.Exceptions;
using Trisoft.ISHRemote.HelperClasses;
using Trisoft.ISHRemote.Objects;
using Trisoft.ISHRemote.Objects.Public;

namespace Trisoft.ISHRemote.Cmdlets.DocumentObj
{
    /// <summary>
    /// <para type="synopsis">Retrieves the metadata and optionally blob of document objects</para>
    /// <para type="description">Gets IshObject entries through logical id filtering or language card ids, optionally providing the blob.</para>
    /// <para type="description">Uses DocumentObj25 API to retrieve ishobjects.</para>
    /// <para type="description">The Get-IshDocumentObj cmdlet retrieves metadata of the document objects that are passed through the pipeline or determined via provided parameters. This commandlet allows to retrieve all types of objects(Illustrations, Maps, etc. ), except for publication(outputs). For publication(outputs) you need to use Get-IshPublicationOutput.</para>
    /// </summary>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/InfoShareWS/" -PSCredential Admin
    /// Get-IshDocumentObj -LogicalId ISHPUBLILLUSTRATIONMISSING
    /// </code>
    /// <para>New-IshSession will submit into SessionState, so it can be reused by this cmdlet. Returns all versions/language of object identified through LogicalId Get-IshDocumentObj -LogicalId ISHPUBLILLUSTRATIONMISSING (typically also GUIDs).</para>
    /// </example>
    [Cmdlet(VerbsCommon.Get, "IshDocumentObj", SupportsShouldProcess = false)]
    [OutputType(typeof(IshDocumentObj))]
    public sealed class GetIshDocumentObj : DocumentObjCmdlet
    {
        /// <summary>
        /// <para type="description">The IshSession variable holds the authentication and contract information. This object can be initialized using the New-IshSession cmdlet.</para>
        /// </summary>
        [Parameter(Mandatory =false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]
        [Parameter(Mandatory =false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshObjectsGroup")]
        [ValidateNotNullOrEmpty]
        public IshSession IshSession { get; set; }

        /// <summary>
        /// <para type="description">The logical identifiers of the document objects for which to retrieve the metadata</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup"), ValidateNotNullOrEmpty]
        public string[] LogicalId { get; set; }

        /// <summary>
        /// <para type="description">The metadata filter with the filter fields to limit the amount of objects returned. Default is no filtering.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshObjectsGroup")]
        [ValidateNotNullOrEmpty]
        public IshField[] MetadataFilter { get; set; }

        /// <summary>
        /// <para type="description">The metadata fields to retrieve</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshObjectsGroup")]
        [ValidateNotNullOrEmpty]
        public IshField[] RequestedMetadata { get; set; }

        /// <summary>
        /// <para type="description">The status filter to limit the amount of objects returned. Default is no filtering.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshObjectsGroup")]
        [ValidateNotNullOrEmpty]
        public Enumerations.StatusFilter StatusFilter 
        {
            get { return _statusFilter; }
            set { _statusFilter = value; }
        }

        /// <summary>
        /// <para type="description">Switch patameter that specifies if the return objects should hold the blob (ishdata) sections, potentially conditionally published if IshFeature was passed</para>
        /// </summary>
        [Parameter(Mandatory = false, ParameterSetName = "ParameterGroup")]
        [Parameter(Mandatory = false, ParameterSetName = "IshObjectsGroup")]
        public SwitchParameter IncludeData
        {
            get { return _includeData; }
            set { _includeData = value; }
        }

        /// <summary>
        /// <para type="description">The condition context to use for conditional filtering. If no context is provided, the elements containing ishcondition attributes will always remain in the data content. You can use the Set-IshFeature cmdlet to create a condition context.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshObjectsGroup")]
        public IshFeature[] IshFeature { get; set; }

        /// <summary>
        /// <para type="description">Array with the objects for which to retrieve the metadata. This array can be passed through the pipeline or explicitly passed via the parameter.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "IshObjectsGroup")]
        [AllowEmptyCollection]
        public IshObject[] IshObject { get; set; }



        

        #region Private fields

        /// <summary>
        /// Private field to store the IshType and provide a default for non-mandatory parameters
        /// </summary>
        private Enumerations.StatusFilter _statusFilter = Enumerations.StatusFilter.ISHNoStatusFilter;

        /// <summary>
        /// Switch patameter that specifies if the return objects should hold the blob (ishdata) sections, potentially conditionally published if IshFeature was passed
        /// </summary>
        private bool _includeData = false;
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
        /// Process the Get-IshDocumentObj commandlet.
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
                
                List<IshObject> returnIshObjects = new List<IshObject>();

                if (IshObject != null && IshObject.Length == 0)
                {
                    // Do nothing
                    WriteVerbose("IshObject is empty, so nothing to retrieve");
                }
                else
                {
                    WriteDebug("Retrieving");
                    IshFields requestedMetadata = IshSession.IshTypeFieldSetup.ToIshRequestedMetadataFields(IshSession.DefaultRequestedMetadata, ISHType, new IshFields(RequestedMetadata), Enumerations.ActionMode.Read);

                    if (IshObject != null)
                    {
                        // 2a. Retrieve using LngCardIds
                        IshObjects ishObjects = new IshObjects(IshObject);
                        var lngCardIds =
                            ishObjects.Objects.Select(
                                ishObject => Convert.ToInt64(ishObject.ObjectRef[Enumerations.ReferenceType.Lng])).ToList();
                        if (!_includeData)
                        {
                            //RetrieveMetadata
                            WriteDebug("Retrieving CardIds.length[{lngCardIds.Count}] RequestedMetadata.length[{requestedMetadata.ToXml().Length}] 0/{lngCardIds.Count}");
                            // Divides the list of language card ids in different lists that all have maximally MetadataBatchSize elements
                            List<List<long>> dividedLngCardIdsList = DivideListInBatches<long>(lngCardIds, IshSession.MetadataBatchSize);
                            int currentLngCardIdCount = 0;
                            foreach (List<long> lngCardIdBatch in dividedLngCardIdsList)
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
                            //RetrieveObjects
                            WriteDebug($"Retrieving CardIds.length[{lngCardIds.Count}] RequestedMetadata.length[{requestedMetadata.ToXml().Length}] including data 0/{lngCardIds.Count}");
                            IshFeatures productDefinitionFeatures = new IshFeatures(IshFeature);
                            // Devides the list of language card ids in different lists that all have maximally BlobBatchSize elements
                            List<List<long>> dividedLngCardIdsList = DivideListInBatches<long>(lngCardIds, IshSession.BlobBatchSize);
                            int currentLngCardIdCount = 0;
                            foreach (List<long> lngCardIdBatch in dividedLngCardIdsList)
                            {
                                // Process language card ids in batches
                                string xmlIshObjects = IshSession.DocumentObj25.RetrieveObjectsByIshLngRefs(
                                    lngCardIdBatch.ToArray(),
                                    productDefinitionFeatures.ToXml(),
                                    requestedMetadata.ToXml());
                                IshObjects retrievedObjects = new IshObjects(ISHType, xmlIshObjects);
                                returnIshObjects.AddRange(retrievedObjects.Objects);
                                currentLngCardIdCount += lngCardIdBatch.Count;
                                WriteDebug($"Retrieving CardIds.length[{lngCardIdBatch.Count}] RequestedMetadata.length[{requestedMetadata.ToXml().Length}] including data {currentLngCardIdCount}/{lngCardIds.Count}");
                            }
                        }
                    }
                    else
                    {
                        // 2b. Retrieve using LogicalId
                        IshFields metadataFilter = new IshFields(MetadataFilter);
                        var statusFilter = EnumConverter.ToStatusFilter<DocumentObj25ServiceReference.StatusFilter>(StatusFilter);
                        if (!_includeData)
                        {
                            //RetrieveMetadata
                            WriteDebug($"Retrieving LogicalId.length[{LogicalId.Length}] StatusFilter[{statusFilter}] MetadataFilter.length[{metadataFilter.ToXml().Length}] RequestedMetadata.length[{requestedMetadata.ToXml().Length}] 0/{LogicalId.Length}");
                            // Divides the list of language card ids in different lists that all have maximally MetadataBatchSize elements
                            List<List<string>> dividedLogicalIdsList = DivideListInBatches<string>(LogicalId.ToList(), IshSession.MetadataBatchSize);
                            int currentLogicalIdCount = 0;
                            foreach (List<string> logicalIdBatch in dividedLogicalIdsList)
                            {
                                // Process language card ids in batches
                                string xmlIshObjects = IshSession.DocumentObj25.RetrieveMetadata(
                                    logicalIdBatch.ToArray(),
                                    statusFilter,
                                    metadataFilter.ToXml(),
                                    requestedMetadata.ToXml());
                                IshObjects retrievedObjects = new IshObjects(ISHType, xmlIshObjects);
                                returnIshObjects.AddRange(retrievedObjects.Objects);
                                currentLogicalIdCount += logicalIdBatch.Count;
                                WriteDebug($"Retrieving LogicalId.length[{logicalIdBatch.Count}] StatusFilter[{statusFilter}] MetadataFilter.length[{metadataFilter.ToXml().Length}] RequestedMetadata.length[{requestedMetadata.ToXml().Length}] {currentLogicalIdCount}/{LogicalId.Length}");
                            }
                        }
                        else
                        {
                            //RetrieveObjects
                            WriteDebug($"Retrieving LogicalId.length[{LogicalId.Length}] StatusFilter[{statusFilter}] MetadataFilter.length[{metadataFilter.ToXml().Length}] RequestedMetadata.length[{requestedMetadata.ToXml().Length}] 0/{LogicalId.Length}");
                            IshFeatures productDefinitionFeatures = new IshFeatures(IshFeature);
                            // Divides the list of language card ids in different lists that all have maximally BlobBatchSize elements
                            List<List<string>> dividedLogicalIdsList = DivideListInBatches<string>(LogicalId.ToList(), IshSession.BlobBatchSize);
                            int currentLogicalIdCount = 0;
                            foreach (List<string> logicalIdBatch in dividedLogicalIdsList)
                            {
                                // Process language card ids in batches
                                string xmlIshObjects = IshSession.DocumentObj25.RetrieveObjects(
                                    logicalIdBatch.ToArray(),
                                    statusFilter,
                                    metadataFilter.ToXml(),
                                    productDefinitionFeatures.ToXml(),
                                    requestedMetadata.ToXml());
                                IshObjects retrievedObjects = new IshObjects(ISHType, xmlIshObjects);
                                returnIshObjects.AddRange(retrievedObjects.Objects);
                                currentLogicalIdCount += logicalIdBatch.Count;
                                WriteDebug($"Retrieving LogicalId.length[{logicalIdBatch.Count}] StatusFilter[{statusFilter}] MetadataFilter.length[{metadataFilter.ToXml().Length}] RequestedMetadata.length[{requestedMetadata.ToXml().Length}] {currentLogicalIdCount}/{LogicalId.Length}");
                            }
                        }
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
