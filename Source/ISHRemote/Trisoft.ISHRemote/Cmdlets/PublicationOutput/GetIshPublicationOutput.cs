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

namespace Trisoft.ISHRemote.Cmdlets.PublicationOutput
{
    /// <summary>
    /// <para type="synopsis">The Get-IshPublicationOutput cmdlet retrieves the metadata of the publication outputs that are passed through the pipeline or determined via provided parameters</para>
    /// <para type="description">The Get-IshPublicationOutput cmdlet retrieves the metadata of the publication outputs that are passed through the pipeline or determined via provided parameters</para>
    /// </summary>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/InfoShareWS/" -IshUserName "username" -IshUserPassword  "userpassword"
    /// $requestedMetadataRetrieve = Set-IshRequestedMetadataField -IshSession $ishSession -Name 'FISHOUTPUTFORMATREF' -Level "Lng" |
    ///                              Set-IshRequestedMetadataField -IshSession $ishSession -Name 'FISHPUBLNGCOMBINATION' -Level "Lng" |
    ///                              Set-IshRequestedMetadataField -IshSession $ishSession -Name 'FISHPUBSTATUS' -Level "Lng" |
    ///                              Set-IshRequestedMetadataField -IshSession $ishSession -Name 'FISHPUBLISHER' -Level "Lng" |
    ///                              Set-IshRequestedMetadataField -IshSession $ishSession -Name 'FISHPUBSTARTDATE' -Level "Lng" |
    ///                              Set-IshRequestedMetadataField -IshSession $ishSession -Name 'FISHPUBENDDATE' -Level "Lng" |
    ///                              Set-IshRequestedMetadataField -IshSession $ishSession -Name 'VERSION' -Level "Version"
    /// $metadataFilterRetrieve = Set-IshMetadataFilterField -IshSession $ishSession -Name 'FISHPUBSTATUS' -Level 'Lng' -ValueType "Value" -FilterOperator "Equal" -Value "Draft"
    /// $publicationOutput = Get-IshPublicationOutput `
    /// -LogicalId @("GUID-412E3A98-9AA8-484E-A1AA-3DE3B58947BD", "GUID-F66C1BB5-076D-455C-B055-DAC5D61AB3D9") `
    /// -StatusFilter "ishreleasedordraftstates" `
    /// -RequestedMetadata $requestedMetadataRetrieve `
    /// -MetadataFilter $metadataFilterRetrieve
    /// </code>
    /// <para>New-IshSession will submit into SessionState, so it can be reused by this cmdlet. Retrieving publication outputs</para>
    /// </example>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/InfoShareWS/" -IshUserName "username" -IshUserPassword  "userpassword"
    /// $metadataFilter = Set-IshMetadataFilterField -Level Version -Name VERSION -FilterOperator Equal -Value 14 |
    ///                   Set-IshMetadataFilterField -Level Lng -Name FISHPUBLNGCOMBINATION -FilterOperator Equal -Value "en" |
    ///                   Set-IshMetadataFilterField -Level Lng -Name FISHOUTPUTFORMATREF -FilterOperator Equal -ValueType Element -Value VOUTPUTFORMATDITADELIVERY # see Find-IshOutputFormat Id column
    /// $publicationOutput = Get-IshPublicationOutput -LogicalId @("GUID-03081B9A-11E4-4862-845B-27339E0C400D", "GUID-F66C1BB5-076D-455C-B055-DAC5D61AB3D9") -MetadataFilter $metadataFilter
    /// </code>
    /// <para>Retrieve specific ParameterGroup identified IshObjects, similar to what Publish-IshPublicationOutput would push to the pipeline for usage downstream on Get-IshPublicationOutputData.</para>
    /// </example>
    [Cmdlet(VerbsCommon.Get, "IshPublicationOutput", SupportsShouldProcess = false)]
    [OutputType(typeof(IshPublicationOutput))]
    public sealed class GetIshPublicationOutput : PublicationOutputCmdlet
    {
        /// <summary>
        /// <para type="description">The IshSession variable holds the authentication and contract information. This object can be initialized using the New-IshSession cmdlet.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshObjectsGroup")]
        [ValidateNotNullOrEmpty]
        public IshSession IshSession { get; set; }

        /// <summary>
        /// <para type="description">The LogicalId of the publication output objects.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup"), ValidateNotNullOrEmpty]
        public string[] LogicalId { get; set; }

        /// <summary>
        /// <para type="description">Array with the publication outputs for which to retrieve the metadata. This array can be passed through the pipeline or explicitly passed via the parameter.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "IshObjectsGroup")]
        [AllowEmptyCollection]
        public IshObject[] IshObject { get; set; }

        /// <summary>
        /// <para type="description">The status filter to limit the amount of objects returned. Default is no filtering.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]
        [ValidateNotNullOrEmpty]
        public Enumerations.StatusFilter StatusFilter
        {
            get { return _statusFilter; }
            set { _statusFilter = value; }
        }

        /// <summary>
        /// <para type="description">The metadata filter with the filter fields to limit the amount of objects returned. Default is no filtering.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup"), ValidateNotNull]
        public IshField[] MetadataFilter { get; set; }

        /// <summary>
        /// <para type="description">The metadata fields to retrieve.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshObjectsGroup")]
        [ValidateNotNull]
        public IshField[] RequestedMetadata { get; set; }


        #region Private fields

        /// <summary>
        /// Private field to store the IshType and provide a default for non-mandatory parameters
        /// </summary>
        private Enumerations.StatusFilter _statusFilter = Enumerations.StatusFilter.ISHNoStatusFilter;
        #endregion

        protected override void BeginProcessing()
        {
            if (IshSession == null) { IshSession = (IshSession)SessionState.PSVariable.GetValue(ISHRemoteSessionStateIshSession); }
            if (IshSession == null) { throw new ArgumentException(ISHRemoteSessionStateIshSessionException); }
            WriteDebug($"Using IshSession[{IshSession.Name}] from SessionState.{ISHRemoteSessionStateIshSession}");
            base.BeginProcessing();
        }

        /// <summary>
        /// Process the Get-IshPublicationOutput commandlet.
        /// </summary>
        /// <exception cref="TrisoftAutomationException"></exception>
        /// <exception cref="Exception"></exception>
        /// <remarks>Writes an <see cref="Objects.Public.IshObject"/> array to the pipeline.</remarks>
        protected override void ProcessRecord()
        {
            try
            {
                // 1. Validating the input
                WriteDebug("Validating");

                List<IshObject> returnedObjects = new List<IshObject>();

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
                                ishObject => Convert.ToInt64(ishObject.ObjectRef[Enumerations.ReferenceType.Lng]))
                                .ToList();
                        WriteDebug($"Retrieving CardIds.length[{lngCardIds}] RequestedMetadata.length[{requestedMetadata.ToXml().Length}] 0/{lngCardIds.Count}");
                        // Divides the list of language card ids in different lists that all have maximally MetadataBatchSize elements
                        List<List<long>> dividedLngCardIdsList = DivideListInBatches<long>(lngCardIds, IshSession.MetadataBatchSize);
                        int currentLngCardIdCount = 0;
                        foreach (List<long> lngCardIdBatch in dividedLngCardIdsList)
                        {
                            // Process language card ids in batches
                            string xmlIshObjects = IshSession.PublicationOutput25.RetrieveMetadataByIshLngRefs(
                                lngCardIdBatch.ToArray(),
                                requestedMetadata.ToXml());
                            IshObjects retrievedObjects = new IshObjects(ISHType, xmlIshObjects);
                            returnedObjects.AddRange(retrievedObjects.Objects);
                            currentLngCardIdCount += lngCardIdBatch.Count;
                            WriteDebug($"Retrieving CardIds.length[{lngCardIdBatch.Count}] RequestedMetadata.length[{requestedMetadata.ToXml().Length}] including data {currentLngCardIdCount}/{lngCardIds.Count}");
                        }
                    }
                    else
                    {
                        // 2b. Retrieve using LogicalId
                        PublicationOutput25ServiceReference.StatusFilter statusFilter =
                            EnumConverter.ToStatusFilter<PublicationOutput25ServiceReference.StatusFilter>(StatusFilter);
                        IshFields metadataFilter = new IshFields(MetadataFilter);
                        WriteDebug($"Retrieving LogicalId.length[{LogicalId.Length}] StatusFilter[{statusFilter}] MetadataFilter.length[{metadataFilter.ToXml().Length}] RequestedMetadata.length[{requestedMetadata.ToXml().Length}] 0/{LogicalId.Length}");
                        // Divides the list of language card ids in different lists that all have maximally MetadataBatchSize elements
                        List<List<string>> dividedLogicalIdsList = DivideListInBatches<string>(LogicalId.ToList(), IshSession.MetadataBatchSize);
                        int currentLogicalIdCount = 0;
                        foreach (List<string> logicalIdBatch in dividedLogicalIdsList)
                        {
                            // Process language card ids in batches
                            string xmlIshObjects = IshSession.PublicationOutput25.RetrieveMetadata(
                                logicalIdBatch.ToArray(),
                                statusFilter,
                                metadataFilter.ToXml(),
                                requestedMetadata.ToXml());
                            IshObjects retrievedObjects = new IshObjects(ISHType, xmlIshObjects);
                            returnedObjects.AddRange(retrievedObjects.Objects);
                            currentLogicalIdCount += logicalIdBatch.Count;
                            WriteDebug($"Retrieving LogicalId.length[{logicalIdBatch.Count}] StatusFilter[{statusFilter}] MetadataFilter.length[{metadataFilter.ToXml().Length}] RequestedMetadata.length[{requestedMetadata.ToXml().Length}] {currentLogicalIdCount}/{LogicalId.Length}");
                        }
                    }
                }

                WriteVerbose("returned object count[" + returnedObjects.Count + "]");
                WriteObject(IshSession, ISHType, returnedObjects.ConvertAll(x => (IshBaseObject)x), true);
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
