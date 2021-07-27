/*
* Copyright © 2014 All Rights Reserved by the RWS Group for and on behalf of its affiliates and subsidiaries.
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
using System.Linq;

namespace Trisoft.ISHRemote.Cmdlets.Annotation
{
    /// <summary>
    /// <para type="synopsis">The Get-IshAnnotation cmdlet gets annotations for the specified AnnotationIds for IshAnnotation object or for the specified IshObject</para>
    /// <para type="description">The Get-IshAnnotation cmdlet gets annotations for the specified AnnotationIds for IshAnnotation object or for the specified IshObject</para>
    /// </summary>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/InfoShareWS/" -IshUserName "username" -IshUserPassword  "userpassword"
    /// $requestedMetadata = Set-IshRequestedMetadataField -IshSession $ishSession -Name "FISHANNOTATIONREPLIES" -Level Annotation
    /// $metadataFilter = Set-IshMetadataFilterField -IshSession $ishSession -Name FISHANNOTATIONTEXT -Level Annotation -FilterOperator Like -Value "Test%"
    /// $ishAnnotations = Get-IshAnnotation -IshSession $ishsession `
    ///                                    -AnnotationId @("GUID-ANNOTATION-ID1", "GUID-ANNOTATION-ID2") `
    ///                                    -RequestedMetadata $requestedMetadata `
    ///                                    -MetadataFilter $metadataFilter
    /// </code>
    /// <para>Get annotations providing AnnotationId array, RequestedMetadata and MetadataFilter</para>
    /// </example>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/InfoShareWS/" -IshUserName "username" -IshUserPassword  "userpassword"
    /// $requestedMetadata = Set-IshRequestedMetadataField -IshSession $ishSession -Name "FISHANNOTPROPOSEDCHNGTXT" -Level Annotation
    /// $ishAnnotations = @($ishObjectPublication1, $ishObjectPublication2) | Get-IshAnnotation -IshSession $ishsession -RequestedMetadata $requestedMetadata
    /// </code>
    /// <para>Get annotations by providing IshPublication objects through the pipeline</para>
    /// </example>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/InfoShareWS/" -IshUserName "username" -IshUserPassword  "userpassword"
    /// $requestedMetadata = Set-IshRequestedMetadataField -IshSession $ishSession -Name "FISHANNOTPROPOSEDCHNGTXT" -Level Annotation
    /// $ishAnnotations = @($ishDocumentObj1, $ishDocumentObj2) | Get-IshAnnotation -IshSession $ishsession -RequestedMetadata $requestedMetadata
    /// </code>
    /// <para>Get annotations by providing DocumentObj (like ISHModule, ISHMasterDoc,...) objects through the pipeline</para>
    /// </example>
    /// <example>
    /// <code>
    /// New-IshSession -WsBaseUrl "https://example.com/InfoShareWS/" -PSCredential username
    /// $requestedMetadata = Set-IshRequestedMetadataField -Name "FISHANNOTPROPOSEDCHNGTXT" -Level Annotation
    /// $ishAnnotations = Get-Ishfolder -FolderPath "General\Myfolder" -Recurse |
    ///                   Get-IshFolderContent |
    ///                   Get-IshAnnotation -RequestedMetadata $requestedMetadata
    /// </code>
    /// <para>Get annotations by piping Get-IshFolderContent output</para>
    /// </example>

    [Cmdlet(VerbsCommon.Get, "IshAnnotation", SupportsShouldProcess = false, DefaultParameterSetName = "IshAnnotationGroup")]
    [OutputType(typeof(IshAnnotation))]
    public sealed class GetIshAnnotation : AnnotationCmdlet
    {
        /// <summary>
        /// <para type="description">The IshSession variable holds the authentication and contract information. This object can be initialized using the New-IshSession cmdlet.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParametersGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshObjectGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshAnnotationGroup")]
        [ValidateNotNullOrEmpty]
        public IshSession IshSession { get; set; }

        /// <summary>
        /// <para type="description">Annotation Ids</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParametersGroup")]
        [ValidateNotNullOrEmpty]
        public string[] AnnotationId { get; set; }

        /// <summary>
        /// <para type="description">The metadata filter with the filter fields to limit the amount of annotations returned. Default is no filtering.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParametersGroup")]
        [ValidateNotNullOrEmpty]
        public IshField[] MetadataFilter { get; set; }

        /// <summary>
        /// <para type="description">The metadata fields to retrieve</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParametersGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshObjectGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshAnnotationGroup")]
        [ValidateNotNullOrEmpty]
        public IshField[] RequestedMetadata { get; set; }

        /// <summary>
        /// <para type="description">IshPublication or IshDocumentObj objects containing full LogicalId/Version/Language combination</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "IshObjectGroup")]
        [ValidateNotNullOrEmpty]
        public IshObject[] IshObject { get; set; }
        
        /// <summary>
        /// <para type="description">The IshAnnotation array that needs to be retrieved. Pipeline</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "IshAnnotationGroup")]
        [ValidateNotNullOrEmpty]
        public IshAnnotation[] IshAnnotation { get; set; }

        #region Private fields
        private readonly List<IshObject> _retrievedIshObjects = new List<IshObject>();
        private readonly List<IshAnnotation> _retrievedIshAnnotations = new List<IshAnnotation>();
        #endregion

        protected override void BeginProcessing()
        {
            if (IshSession == null) { IshSession = (IshSession)SessionState.PSVariable.GetValue(ISHRemoteSessionStateIshSession); }
            if (IshSession == null) { throw new ArgumentException(ISHRemoteSessionStateIshSessionException); }
            WriteDebug($"Using IshSession[{IshSession.Name}] from SessionState.{ISHRemoteSessionStateIshSession}");

            if ((IshSession.ServerIshVersion.MajorVersion < 14) || ((IshSession.ServerIshVersion.MajorVersion == 14) && (IshSession.ServerIshVersion.RevisionVersion < 2)))
            {
                throw new PlatformNotSupportedException($"Get-IshAnnotation requires server-side Annotation API which is only available starting from 14.0.2 and up. ServerIshVersion[{IshSession.ServerVersion}]");
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
                if (IshObject != null)
                {
                    foreach (IshObject ishObject in IshObject)
                    {
                        _retrievedIshObjects.Add(ishObject);
                    }
                }
                if (IshAnnotation != null)
                {
                    foreach (IshAnnotation ishAnnotation in IshAnnotation)
                    {
                        _retrievedIshAnnotations.Add(ishAnnotation);
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
        /// Process the Get-IshAnnotation commandlet.
        /// </summary>
        /// <exception cref="TrisoftAutomationException"></exception>
        /// <exception cref="Exception"></exception>
        /// <remarks>Writes <see cref="IshAnnotation"/> array to the pipeline</remarks>
        protected override void EndProcessing()
        {
            try
            {
                //1. Get annotations depending on the chosen ParameterSet
                List<IshObject> returnedObjects = new List<IshObject>();
                IshFields requestedMetadata = IshSession.IshTypeFieldSetup.ToIshRequestedMetadataFields(IshSession.DefaultRequestedMetadata, ISHType, new IshFields(RequestedMetadata), Enumerations.ActionMode.Read);
                IshFields metadataFilter = new IshFields(MetadataFilter);
                List<string> annotationIds = new List<string>();

                if (ParameterSetName == "IshObjectGroup" )
                {
                    IshFields ishMetadataFilterFind;
                    List<IshObject> foundObjects = new List<IshObject>();
                    foreach (IshObject ishObject in _retrievedIshObjects)
                    {
                        string xmlIshAnnotations;
                        switch (ishObject.IshType)
                        {
                            case Enumerations.ISHType.ISHPublication:
                                ishMetadataFilterFind = new IshFields();
                                ishMetadataFilterFind.AddField(new IshMetadataFilterField(FieldElements.AnnotationPublicationLogicalId, Enumerations.Level.Annotation, Enumerations.FilterOperator.Equal, ishObject.IshRef, Enumerations.ValueType.Element));
                                string pubVersion = ishObject.IshFields.GetFieldValue(FieldElements.Version, Enumerations.Level.Version, Enumerations.ValueType.Value);
                                ishMetadataFilterFind.AddField(new IshMetadataFilterField(FieldElements.AnnotationPublicationVersion, Enumerations.Level.Annotation, Enumerations.FilterOperator.Equal, pubVersion, Enumerations.ValueType.Value));
                                string pubLanguage = ishObject.IshFields.GetFieldValue(FieldElements.PublicationSourceLanguages, Enumerations.Level.Version, Enumerations.ValueType.Value);  // TODO [Could] PublicationSourceLanguage theoretically is multi-value but passed below with an equal filter operator
                                ishMetadataFilterFind.AddField(new IshMetadataFilterField(FieldElements.AnnotationPublicationLanguage, Enumerations.Level.Annotation, Enumerations.FilterOperator.Equal, pubLanguage, Enumerations.ValueType.Value));
                                xmlIshAnnotations = IshSession.Annotation25.Find(ishMetadataFilterFind.ToXml(), "");
                                foundObjects.AddRange(new IshObjects(ISHType, xmlIshAnnotations).Objects);
                                break;
                            case Enumerations.ISHType.ISHMasterDoc:
                            case Enumerations.ISHType.ISHModule:
                            case Enumerations.ISHType.ISHLibrary:
                            case Enumerations.ISHType.ISHIllustration:
                            case Enumerations.ISHType.ISHTemplate:
                                ishMetadataFilterFind = new IshFields();
                                ishMetadataFilterFind.AddField(new IshMetadataFilterField(FieldElements.AnnotationContentObjectLogicalId, Enumerations.Level.Annotation, Enumerations.FilterOperator.Equal, ishObject.IshRef, Enumerations.ValueType.Element));
                                string documentVersion = ishObject.IshFields.GetFieldValue(FieldElements.Version, Enumerations.Level.Version, Enumerations.ValueType.Value);
                                ishMetadataFilterFind.AddField(new IshMetadataFilterField(FieldElements.AnnotationContentObjectVersion, Enumerations.Level.Annotation, Enumerations.FilterOperator.Equal, documentVersion, Enumerations.ValueType.Value));
                                string documentLanguage = ishObject.IshFields.GetFieldValue(FieldElements.DocumentLanguage, Enumerations.Level.Lng, Enumerations.ValueType.Value);
                                ishMetadataFilterFind.AddField(new IshMetadataFilterField(FieldElements.AnnotationContentObjectLanguage, Enumerations.Level.Annotation, Enumerations.FilterOperator.Equal, documentLanguage, Enumerations.ValueType.Value));
                                xmlIshAnnotations = IshSession.Annotation25.Find(ishMetadataFilterFind.ToXml(), "");
                                foundObjects.AddRange(new IshObjects(ISHType, xmlIshAnnotations).Objects);
                                break;
                            default:
                                WriteDebug($"Object type ishObject.IshType[{ishObject.IshType}] with ishObject.IshRef[{ishObject.IshRef}] is not supported, skipping");
                                break;
                        }
                    }
                    annotationIds = foundObjects.Select(ishObject => Convert.ToString(ishObject.IshRef)).ToList();
                }

                if (ParameterSetName == "ParametersGroup")
                {
                    annotationIds = AnnotationId.ToList();
                }

                if( ParameterSetName == "IshAnnotationGroup")
                {
                    annotationIds = _retrievedIshAnnotations.Select(ishAnnotation => Convert.ToString(ishAnnotation.IshRef)).ToList();
                }

                //2. Retrieve in batches
                // remove duplicates from annotationIds list
                annotationIds = annotationIds.Distinct().ToList();
                WriteDebug($"Retrieving AnnotationId.length[{annotationIds.Count}]  MetadataFilter.length[{metadataFilter.ToXml().Length}] RequestedMetadata.length[{requestedMetadata.ToXml().Length}]");
                // Devides the list of Annotation Ids in different lists that all have maximally MetadataBatchSize elements
                List<List<string>> devidedAnnotationIdsList = DevideListInBatches<string>(annotationIds, IshSession.MetadataBatchSize);
                int currentAnnotationIdCount = 0;
                foreach (List<string> annotationIdBatch in devidedAnnotationIdsList)
                {
                    string xmlIshObjectsRetrieved = IshSession.Annotation25.RetrieveMetadata(annotationIdBatch.ToArray(), metadataFilter.ToXml(), requestedMetadata.ToXml());
                    IshObjects ishObjectsRetrieved = new IshObjects(ISHType, xmlIshObjectsRetrieved);
                    returnedObjects.AddRange(ishObjectsRetrieved.Objects);
                    currentAnnotationIdCount += annotationIdBatch.Count;
                    WriteDebug($"Retrieving AnnotationId.length[{annotationIdBatch.Count}] MetadataFilter.length[{metadataFilter.ToXml().Length}] RequestedMetadata.length[{requestedMetadata.ToXml().Length}] {currentAnnotationIdCount}/{annotationIds.Count}");
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
