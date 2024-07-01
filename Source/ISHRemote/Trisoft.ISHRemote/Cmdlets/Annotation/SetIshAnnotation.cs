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
using System.Management.Automation;
using Trisoft.ISHRemote.Objects;
using Trisoft.ISHRemote.Objects.Public;
using Trisoft.ISHRemote.Exceptions;

namespace Trisoft.ISHRemote.Cmdlets.Annotation
{
    /// <summary>
    /// <para type="synopsis">The Set-IshAnnotation cmdlet updates annotations that are passed through the pipeline or determined via provided parameters</para>
    /// <para type="description">The Set-IshAnnotation cmdlet updates annotations that are passed through the pipeline or determined via provided parameters</para>
    /// </summary>
    /// <example>
    /// <code>
    /// New-IshSession -WsBaseUrl "https://example.com/InfoShareWS/" -IshUserName "username" -IshUserPassword "userpassword"
    /// $metadataUpdate = Set-IshMetadataField -Name "FISHANNOTATIONSTATUS" -Level Annotation -Value "Shared" -ValueType Value
    /// $requiredCurrentMetadata = Set-IshRequiredCurrentMetadataField -Name "FISHANNOTATIONSTATUS" -Level Annotation -Value "Unshared" -ValueType Value
    /// $ishAnnotation = Set-IshAnnotation -AnnotationId "MYANNOTATIONID" -Metadata $metadataUpdate -RequiredCurrentMetadata $requiredCurrentMetadata
    /// </code>
    /// <para>Update annotation providing AnnotationId, Metadata and RequiredCurrentMetadata</para>
    /// </example>
    /// <example>
    /// <code>
    /// New-IshSession -WsBaseUrl "https://example.com/InfoShareWS/" -IshUserName "username" -IshUserPassword  "userpassword"
    /// $metadata = Set-IshMetadataField -Name "FISHANNOTPROPOSEDCHNGTXT" -Level Annotation -Value "My proposed change text"
    /// $ishAnnotation = Add-IshAnnotation -PubLogicalId "GUID-MYPUBLICATIONLOGICALID" `
    ///                     -PubVersion "1" `
    ///                     -PubLng "en" `
    ///                     -LogicalId "MYCONTENTOBJECTLOGICALID" `
    ///                     -Version "1" `
    ///                     -Lng "en" `
    ///                     -Type "General" `
    ///                     -Text "My annotation text" `
    ///                     -Status "Unshared" `
    ///                     -Category "Comment" `
    ///                     -Address "My annotation address"  
    /// $metadataUpdate = Set-IshMetadataField -Name "FISHANNOTATIONSTATUS" -Level Annotation -Value "Shared" -ValueType Value
    /// $requiredCurrentMetadata = Set-IshRequiredCurrentMetadataField -Name "FISHANNOTATIONSTATUS" -Level Annotation -Value "Unshared" -ValueType Value
    /// $ishAnnotationUpdated = $ishAnnotation | Set-IshAnnotation -Metadata $metadataUpdate -RequiredCurrentMetadata $requiredCurrentMetadata
    /// </code>
    /// <para>Update annotation providing ISHAnnotation object via pipeline, Metadata and RequiredCurrentMetadata</para>
    /// </example>
    [Cmdlet(VerbsCommon.Set, "IshAnnotation", SupportsShouldProcess = true)]
    [OutputType(typeof(IshAnnotation))]
    public sealed class SetIshAnnotation :AnnotationCmdlet
    {
        /// <summary>
        /// <para type="description">The IshSession variable holds the authentication and contract information. This object can be initialized using the New-IshSession cmdlet.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParametersGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshAnnotationGroup")]
        [ValidateNotNullOrEmpty]
        public IshSession IshSession { get; set; }

        /// <summary>
        /// <para type="description">Id of the Annotation</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParametersGroup")]
        [ValidateNotNullOrEmpty]
        public string AnnotationId { get; set; }

        /// <summary>
        /// <para type="description">The metadata of the Annotation</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParametersGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshAnnotationGroup")]
        [ValidateNotNullOrEmpty]
        public IshField[] Metadata { get; set; }

        /// <summary>
        /// <para type="description">The required current metadata of the Annotation</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParametersGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshAnnotationGroup")]
        [ValidateNotNullOrEmpty]
        public IshField[] RequiredCurrentMetadata { get; set; }

        /// <summary>
        /// <para type="description">The IshAnnotation array that needs to be set. Pipeline</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "IshAnnotationGroup")]
        [ValidateNotNullOrEmpty]
        public IshAnnotation[] IshAnnotation { get; set; }
        
        #region Private fields
        private readonly List<IshAnnotation> _ishAnnotationsToSet = new List<IshAnnotation>();
        #endregion

        protected override void BeginProcessing()
        {
            if (IshSession == null) { IshSession = (IshSession)SessionState.PSVariable.GetValue(ISHRemoteSessionStateIshSession); }
            if (IshSession == null) { throw new ArgumentException(ISHRemoteSessionStateIshSessionException); }
            WriteDebug($"Using IshSession[{IshSession.Name}] from SessionState.{ISHRemoteSessionStateIshSession}");

            if ((IshSession.ServerIshVersion.MajorVersion < 14) || ((IshSession.ServerIshVersion.MajorVersion == 14) && (IshSession.ServerIshVersion.RevisionVersion < 2)))
            {
                throw new PlatformNotSupportedException($"Set-IshAnnotation requires server-side Annotation API which is only available starting from 14.0.2 and up. ServerIshVersion[{IshSession.ServerVersion}]");
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
                if (IshAnnotation != null)
                {
                    foreach (IshAnnotation ishAnnotation in IshAnnotation)
                    {
                        _ishAnnotationsToSet.Add(ishAnnotation);
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

        /// <summary>
        /// Process the Set-IshAnnotation commandlet.
        /// </summary>
        /// <exception cref="TrisoftAutomationException"></exception>
        /// <exception cref="Exception"></exception>
        /// <remarks>Writes <see cref="IshAnnotation"/> array to the pipeline</remarks>
        protected override void EndProcessing()
        {
            try
            {
                List<IshObject> returnedObjects = new List<IshObject>();
                List<string> annotationIdsToRetrieve = new List<string>();
                IshFields returnFields = new IshFields();

                var metadata = (Metadata == null) ? new IshFields() : new IshFields(Metadata);
                var requiredCurrentMetadata = new IshFields(RequiredCurrentMetadata);

                //1. Set annotations depending on the chosen ParameterSet
                switch (ParameterSetName)
                {
                    case "IshAnnotationGroup":
                        int current = 0;
                        foreach (IshObject ishObject in _ishAnnotationsToSet)
                        {
                            WriteDebug($"AnnotationId[{ishObject.IshRef}] Metadata.length[{metadata.ToXml().Length}] RequiredCurrentMetadata.length[{requiredCurrentMetadata.ToXml().Length}] {++current}/{_ishAnnotationsToSet.Count}");
                            if (ShouldProcess(Convert.ToString(ishObject.IshRef)))
                            {
                                IshSession.Annotation25.Update(ishObject.IshRef, metadata.ToXml(), requiredCurrentMetadata.ToXml());
                            }
                            annotationIdsToRetrieve.Add(ishObject.IshRef);
                        }
                        returnFields = (_ishAnnotationsToSet[0] == null) ? new IshFields() : _ishAnnotationsToSet[0].IshFields;
                        break;

                    case "ParametersGroup":
                        WriteDebug($"AnnotationId[{AnnotationId}] Metadata.length[{metadata.ToXml().Length}]");
                        if (ShouldProcess(AnnotationId))
                        {
                            IshSession.Annotation25.Update(AnnotationId, metadata.ToXml(), requiredCurrentMetadata.ToXml());
                        }
                        annotationIdsToRetrieve.Add(AnnotationId);
                        returnFields = metadata;
                        break;
                }
                
                //2. Retrieve set annotations
                WriteDebug($"Retrieving AnnotationId.length[{annotationIdsToRetrieve.Count}]");
                // Divides the list of Annotation Ids in different lists that all have maximally MetadataBatchSize elements
                List<List<string>> dividedAnnotationIdsList = DivideListInBatches<string>(annotationIdsToRetrieve, IshSession.MetadataBatchSize);
                IshFields requestedMetadata = IshSession.IshTypeFieldSetup.ToIshRequestedMetadataFields(IshSession.DefaultRequestedMetadata, ISHType, returnFields, Enumerations.ActionMode.Read);
                int currentAnnotationIdCount = 0;
                foreach (List<string> annotationIdBatch in dividedAnnotationIdsList)
                {
                    string xmlIshObjectsRetrieved = IshSession.Annotation25.RetrieveMetadata(annotationIdBatch.ToArray(), "", requestedMetadata.ToXml());
                    IshObjects ishObjectsRetrieved = new IshObjects(ISHType, xmlIshObjectsRetrieved);
                    returnedObjects.AddRange(ishObjectsRetrieved.Objects);
                    currentAnnotationIdCount += annotationIdBatch.Count;
                    WriteDebug($"Retrieving AnnotationId.length[{annotationIdBatch.Count}] RequestedMetadata.length[{requestedMetadata.ToXml().Length}] {currentAnnotationIdCount}/{annotationIdsToRetrieve.Count}");
                }

                //3. Write it
                WriteVerbose("returned object count[" + returnedObjects.Count + "]");
                WriteObject(IshSession, ISHType, returnedObjects.ConvertAll(x => (IshBaseObject)x), true);
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
