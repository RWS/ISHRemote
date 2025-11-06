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
using System.ServiceModel;
using Trisoft.ISHRemote.DocumentObj25ServiceReference;
using Trisoft.ISHRemote.Exceptions;
using Trisoft.ISHRemote.Objects;
using Trisoft.ISHRemote.Objects.Public;

namespace Trisoft.ISHRemote.Cmdlets.Annotation
{
    /// <summary>
    /// <para type="synopsis">The Add-IshAnnotation cmdlet adds a new annotation to the specified content object</para>
    /// <para type="description">The Add-IshAnnotation cmdlet adds a new annotation to the specified content object</para>
    /// </summary>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/InfoShareWS/" -IshUserName "username" -IshUserPassword  "userpassword"
    /// $metadata = Set-IshMetadataField -Name "FISHREVISIONID" -Level Annotation -Value "GUID-MYREVISIONID" |
    ///             Set-IshMetadataField -Name "FISHPUBLOGICALID" -Level Annotation -Value "GUID-MYPUBLICATIONLOGICALID" |
    ///             Set-IshMetadataField -Name "FISHPUBVERSION" -Level Annotation -Value "1" |
    ///             Set-IshMetadataField -Name "FISHPUBLANGUAGE" -Level Annotation -Value "en" |
    ///             Set-IshMetadataField -Name "FISHANNOTATIONSTATUS" -Level Annotation -Value "VANNOTATIONSTATUSUNSHARED" -ValueType Element |
    ///             Set-IshMetadataField -Name "FISHANNOTATIONADDRESS" -Level Annotation -Value "My annotation address" |
    ///             Set-IshMetadataField -Name "FISHANNOTATIONTEXT" -Level Annotation -Value "My annotation text" |
    ///             Set-IshMetadataField -Name "FISHANNOTATIONCATEGORY" -Level Annotation -Value "Comment" |
    ///             Set-IshMetadataField -Name "FISHANNOTATIONTYPE" -Level Annotation -Value "General"
    /// $ishAnnotation = Add-IshAnnotation -IshSession $ishsession -Metadata $metadata
    /// </code>
    /// <para>Add annotation providing Metadata object. Full manual mode - validation of the incoming metadata is on the API side</para>
    /// </example>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/InfoShareWS/" -IshUserName "username" -IshUserPassword  "userpassword"
    /// $metadata = Set-IshMetadataField -Name "FISHANNOTPROPOSEDCHNGTXT" -Level Annotation -Value "My proposed change text"
    /// $ishAnnotation = Add-IshAnnotation -IshSession $ishsession `
    ///                     -PubLogicalId "GUID-MYPUBLICATIONLOGICALID" `
    ///                     -PubVersion "1" `
    ///                     -PubLng "en" `
    ///                     -LogicalId "MYCONTENTOBJECTLOGICALID" `
    ///                     -Version "1" `
    ///                     -Lng "en" `
    ///                     -Type "General" `
    ///                     -Text "My annotation text" `
    ///                     -Status "Unshared" `
    ///                     -Category "Comment" `
    ///                     -Address "My annotation address" `
    ///                     -Metadata $metadata
    /// </code>
    /// <para>Add annotation providing required parameters and Metadata object. Cmdlet will use the latest RevisionId for the provided content object and parameter values take precedence over metadata field values</para>
    /// </example>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/InfoShareWS/" -IshUserName "username" -IshUserPassword  "userpassword"
    /// $metadataFilter = Set-IshMetadataFilterField -Name "VERSION" -Level Version -ValueType Value -Value "1" |
    ///                   Set-IshMetadataFilterField -Name "DOC-LANGUAGE" -Level Lng -ValueType Value -Value "en"
    /// $ishObject = Get-IshDocumentObj -IshSession $ishsession -LogicalId "MYCONTENTOBJECTLOGICALID" -MetadataFilter $metadataFilter
    ///	$metadata =	Set-IshMetadataField -Name "FISHANNOTPROPOSEDCHNGTXT" -Level Annotation -Value "My proposed change text"
    ///	$ishAnnotation = Add-IshAnnotation -IshSession $ishsession `
    ///                     -PubLogicalId "GUID-MYPUBLICATIONLOGICALID" `
    ///                     -PubVersion "1" `
    ///                     -PubLng "en" `
    ///						-IshObject $ishObject `
    ///                     -Type $annotationType `
    ///                     -Text "My annotation text" `
    ///                     -Status "Unshared" `
    ///                     -Category "Comment" `
    ///                     -Address "My annotation address" `
    ///						-Metadata $metadata
    /// </code>
    /// <para>Add annotation providing IshObject of type IshDocumentObj. Cmdlet will use the latest RevisionId for the provided content object and parameter values take precedence over metadata field values</para>
    /// </example>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/InfoShareWS/" -IshUserName "username" -IshUserPassword  "userpassword"
    /// $ishAnnotation = Add-IshAnnotation -IshSession $ishsession `
    ///                     -PubLogicalId "GUID-MYPUBLICATIONLOGICALID" `
    ///                     -PubVersion "1" `
    ///                     -PubLng "en" `
    ///                     -LogicalId "MYCONTENTOBJECTLOGICALID" `
    ///                     -Version "1" `
    ///                     -Lng "en" `
    ///                     -Type "General" `
    ///                     -Text "My annotation text" `
    ///                     -Status "Unshared" `
    ///                     -Category "Comment" `
    ///                     -Address "My annotation address" `
    ///	$ishAnnotation = $ishAnnotation | Set-IshMetadataField -Name "FISHANNOTATIONTEXT" -Level Annotation -Value "My annotation text updated"
    /// $ishAnnotationAdded = $ishAnnotation | Add-IshAnnotation -IshSession $ishsession
    /// </code>
    /// <para>Add annotation providing IshAnnotation object.</para>
    /// </example>
    [Cmdlet(VerbsCommon.Add, "IshAnnotation", SupportsShouldProcess = true)]
    [OutputType(typeof(IshAnnotation))]
    public sealed class AddIshAnnotation :AnnotationCmdlet
    {
        /// <summary>
        /// <para type="description">The IshSession variable holds the authentication and contract information. This object can be initialized using the New-IshSession cmdlet.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParametersGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshObjectGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshAnnotationGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "MetadataGroup")]
        [ValidateNotNullOrEmpty]
        public IshSession IshSession { get; set; }

        /// <summary>
        /// <para type="description">Publication LogicalId</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParametersGroup")]
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshObjectGroup")]
        [ValidateNotNullOrEmpty]
        public string PubLogicalId { get; set; }

        /// <summary>
        /// <para type="description">Publication version</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParametersGroup")]
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshObjectGroup")]
        [ValidateNotNullOrEmpty]
        public string PubVersion { get; set; }
       
        /// <summary>
        /// <para type="description">Publication language</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParametersGroup")]
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshObjectGroup")]
        [ValidateNotNullOrEmpty]
        public string PubLng { get; set; }

        /// <summary>
        /// <para type="description">LogicalId of the content object</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParametersGroup")]
        [ValidateNotNullOrEmpty]
        public string LogicalId { get; set; }

        /// <summary>
        /// <para type="description">Version of the content object</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParametersGroup")]
        [ValidateNotNullOrEmpty]
        public string Version { get; set; }

        /// <summary>
        /// <para type="description">Language of the content object</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParametersGroup")]
        [ValidateNotNullOrEmpty]
        public string Lng { get; set; }

        /// <summary>
        /// <para type="description">Type of the annotation</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParametersGroup")]
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshObjectGroup")]
        [ValidateNotNullOrEmpty]
        public string Type { get; set; }

        /// <summary>
        /// <para type="description">Text of the annotation</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParametersGroup")]
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshObjectGroup")]
        [ValidateNotNullOrEmpty]
        public string Text { get; set; }
       
        /// <summary>
        /// <para type="description">Status of the annotation</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParametersGroup")]
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshObjectGroup")]
        [ValidateNotNullOrEmpty]
        public string Status { get; set; }

        /// <summary>
        /// <para type="description">Address of the annotation</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParametersGroup")]
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshObjectGroup")]
        [ValidateNotNullOrEmpty]
        public string Address { get; set; }

        /// <summary>
        /// <para type="description">Category of the annotation</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParametersGroup")]
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshObjectGroup")]
        [ValidateNotNullOrEmpty]
        public string Category { get; set; }

        /// <summary>
        /// <para type="description">The metadata of the Annotation</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParametersGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshObjectGroup")]
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "MetadataGroup")]
        [ValidateNotNullOrEmpty]
        public IshField[] Metadata { get; set; }

        /// <summary>
        /// <para type="description">The IShObject - content object containing full LogicalId/Version/Language combination</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipeline = true, ParameterSetName = "IshObjectGroup")]
        [ValidateNotNullOrEmpty]
        public IshDocumentObj IshObject { get; set; }

        /// <summary>
        /// <para type="description">The IshAnnotation array that needs to be created. Pipeline</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "IshAnnotationGroup")]
        [ValidateNotNullOrEmpty]
        public IshAnnotation[] IshAnnotation { get; set; }
        
        protected override void BeginProcessing()
        {
            if (IshSession == null) { IshSession = (IshSession)SessionState.PSVariable.GetValue(ISHRemoteSessionStateIshSession); }
            if (IshSession == null) { throw new ArgumentException(ISHRemoteSessionStateIshSessionException); }
            WriteDebug($"Using IshSession[{IshSession.Name}] from SessionState.{ISHRemoteSessionStateIshSession}");

            if ((IshSession.ServerIshVersion.MajorVersion < 14) || ((IshSession.ServerIshVersion.MajorVersion == 14) && (IshSession.ServerIshVersion.RevisionVersion < 2)))
            {
                throw new PlatformNotSupportedException($"Add-IshAnnotation requires server-side Annotation API which is only available starting from 14.0.2 and up. ServerIshVersion[{IshSession.ServerVersion}]");
            }

            base.BeginProcessing();
        }

        /// <summary>
        /// Process the Add-IshAnnotation commandlet.
        /// </summary>
        /// <exception cref="TrisoftAutomationException"></exception>
        /// <exception cref="Exception"></exception>
        /// <remarks>Writes <see cref="IshAnnotation"/> array to the pipeline</remarks>
        protected override void ProcessRecord()
        {
            try
            {
                List<IshObject> returnedObjects = new List<IshObject>();

                //1. Add annotations depending on the chosen ParameterSet
                List<string> returnAnnotations = new List<string>();
                IshFields returnFields = new IshFields();

                if (ParameterSetName == "MetadataGroup")
                {
                    var metadata = IshSession.IshTypeFieldSetup.ToIshMetadataFields(ISHType, new IshFields(Metadata), Enumerations.ActionMode.Create);
                    if (ShouldProcess("AnnotationAddress: '" + 
                        metadata.GetFieldValue(FieldElements.AnnotationAddress, Enumerations.Level.Annotation, Enumerations.ValueType.Value) +
                        "' AnnotationText: '" + 
                        metadata.GetFieldValue(FieldElements.AnnotationText, Enumerations.Level.Annotation, Enumerations.ValueType.Value) + "'"))
                    {
                        string annotationId = IshSession.Annotation25.Create(metadata.ToXml());
                        returnAnnotations.Add(annotationId);
                    }
                    returnFields = metadata;
                }

                if (ParameterSetName == "ParametersGroup" || ParameterSetName == "IshObjectGroup")
                {
                    // 1.1. Get the latest RevisionId for the given LogicalId/Version/Language of the content object
                    var metadata = (Metadata == null) ? new IshFields() : new IshFields(Metadata);
                    IshFields requestedMetadataContentObject = new IshFields();
                    requestedMetadataContentObject.AddField(new IshRequestedMetadataField(FieldElements.ED, Enumerations.Level.Lng, Enumerations.ValueType.Element));
                    string xmlIshContentObjects;
                    string logicalId;
                    string version;
                    string lng;

                    if (IshObject != null)
                    {
                        // IshObjectGroup 
                        xmlIshContentObjects = IshSession.DocumentObj25.GetMetadataByIshLngRef(Convert.ToInt64(IshObject.LngRef), requestedMetadataContentObject.ToXml());
                        logicalId = IshObject.IshRef;
                        version = IshObject.IshFields.GetFieldValue(FieldElements.Version, Enumerations.Level.Version, Enumerations.ValueType.Value);
                        lng = IshObject.IshFields.GetFieldValue(FieldElements.DocumentLanguage, Enumerations.Level.Lng, Enumerations.ValueType.Value);
                    }
                    else
                    {
                        // ParametersGroup
                        var response = IshSession.DocumentObj25.GetMetadata(new GetMetadataRequest(LogicalId,
                            Version,
                            Lng,
                            "",
                            requestedMetadataContentObject.ToXml()));
                        // GetMetadata call should throw in case of non-existing LogicalId/Version/Lng combination. So not checking count of returned objects
                        xmlIshContentObjects = response.xmlObjectList;
                        logicalId = LogicalId;
                        version = Version;
                        lng = Lng;
                    }

                    IshObjects retrievedContentObjects = new IshObjects(ISHType, xmlIshContentObjects);
                    string retrievedContentObjectED = retrievedContentObjects.Objects[0].IshFields.GetFieldValue(FieldElements.ED, Enumerations.Level.Lng, Enumerations.ValueType.Element);

                    // 1.2. Owerwrite incoming metadata field values with parameters provided in the parameters
                    metadata.AddOrUpdateField(new IshMetadataField(FieldElements.AnnotationPublicationLogicalId, Enumerations.Level.Annotation, PubLogicalId), Enumerations.ActionMode.Update);
                    metadata.AddOrUpdateField(new IshMetadataField(FieldElements.AnnotationPublicationVersion, Enumerations.Level.Annotation, PubVersion), Enumerations.ActionMode.Update);
                    metadata.AddOrUpdateField(new IshMetadataField(FieldElements.AnnotationContentObjectLogicalId, Enumerations.Level.Annotation, logicalId), Enumerations.ActionMode.Update);
                    metadata.AddOrUpdateField(new IshMetadataField(FieldElements.AnnotationContentObjectVersion, Enumerations.Level.Annotation, version), Enumerations.ActionMode.Update);
                    metadata.AddOrUpdateField(new IshMetadataField(FieldElements.AnnotationContentObjectLanguage, Enumerations.Level.Annotation, lng), Enumerations.ActionMode.Update);
                    metadata.AddOrUpdateField(new IshMetadataField(FieldElements.AnnotationRevisionId, Enumerations.Level.Annotation, Enumerations.ValueType.Element, retrievedContentObjectED), Enumerations.ActionMode.Update);
                    metadata.AddOrUpdateField(new IshMetadataField(FieldElements.AnnotationType, Enumerations.Level.Annotation, Type), Enumerations.ActionMode.Update);
                    metadata.AddOrUpdateField(new IshMetadataField(FieldElements.AnnotationText, Enumerations.Level.Annotation, Text), Enumerations.ActionMode.Update);
                    metadata.AddOrUpdateField(new IshMetadataField(FieldElements.AnnotationStatus, Enumerations.Level.Annotation, Status), Enumerations.ActionMode.Update);
                    metadata.AddOrUpdateField(new IshMetadataField(FieldElements.AnnotationAddress, Enumerations.Level.Annotation, Address), Enumerations.ActionMode.Update);
                    metadata.AddOrUpdateField(new IshMetadataField(FieldElements.AnnotationCategory, Enumerations.Level.Annotation, Category), Enumerations.ActionMode.Update);
                    metadata.AddOrUpdateField(new IshMetadataField(FieldElements.AnnotationPublicationLanguage, Enumerations.Level.Annotation, PubLng), Enumerations.ActionMode.Update);
                    metadata = IshSession.IshTypeFieldSetup.ToIshMetadataFields(ISHType, metadata, Enumerations.ActionMode.Create);

                    if (ShouldProcess("AnnotationAddress: '" +
                        metadata.GetFieldValue(FieldElements.AnnotationAddress, Enumerations.Level.Annotation, Enumerations.ValueType.Value) +
                        "' AnnotationText: '" +
                        metadata.GetFieldValue(FieldElements.AnnotationText, Enumerations.Level.Annotation, Enumerations.ValueType.Value) + "'"))
                    {
                        string annotationId = IshSession.Annotation25.Create(metadata.ToXml());
                        returnAnnotations.Add(annotationId);
                    }
                    returnFields = metadata;
                }

                if(ParameterSetName == "IshAnnotationGroup")
                {
                    foreach(IshAnnotation ishAnnotation in IshAnnotation)
                    {
                       IshFields metadata = IshSession.IshTypeFieldSetup.ToIshMetadataFields(ISHType, ishAnnotation.IshFields, Enumerations.ActionMode.Create);
                        if (ShouldProcess("AnnotationAddress: '" +
                            metadata.GetFieldValue(FieldElements.AnnotationAddress, Enumerations.Level.Annotation, Enumerations.ValueType.Value) +
                            "' AnnotationText: '" +
                            metadata.GetFieldValue(FieldElements.AnnotationText, Enumerations.Level.Annotation, Enumerations.ValueType.Value) + "'"))
                        {
                            string annotationId = IshSession.Annotation25.Create(metadata.ToXml());
                            returnAnnotations.Add(annotationId);
                        }
                    }
                    returnFields = (IshAnnotation[0] == null) ? new IshFields() : IshAnnotation[0].IshFields;
                }

                //2. Retrieve added annotations
                WriteDebug("Retrieving");

                // Add the required fields
                IshFields requestedMetadata = IshSession.IshTypeFieldSetup.ToIshRequestedMetadataFields(IshSession.DefaultRequestedMetadata, ISHType, returnFields, Enumerations.ActionMode.Read);
                string xmlIshObjects = IshSession.Annotation25.RetrieveMetadata(returnAnnotations.ToArray(), "", requestedMetadata.ToXml());
                IshObjects retrievedObjects = new IshObjects(ISHType, xmlIshObjects);
                returnedObjects.AddRange(retrievedObjects.Objects);
                
                //3. Write it
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
