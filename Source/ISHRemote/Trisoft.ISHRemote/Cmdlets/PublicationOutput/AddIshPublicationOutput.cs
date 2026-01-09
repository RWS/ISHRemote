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
using System.Text;
using Trisoft.ISHRemote.Exceptions;
using Trisoft.ISHRemote.HelperClasses;
using Trisoft.ISHRemote.Objects;
using Trisoft.ISHRemote.Objects.Public;

namespace Trisoft.ISHRemote.Cmdlets.PublicationOutput
{
    /// <summary>
    /// <para type="synopsis">The Add-IshPublicationOutput cmdlet add the new publication outputs that are passed through the pipeline or determined via provided parameters</para>
    /// <para type="description">The Add-IshPublicationOutput cmdlet add the new publication outputs that are passed through the pipeline or determined via provided parameters</para>
    /// </summary>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/InfoShareWS/" -IshUserName "username" -IshUserPassword  "userpassword"
    /// $metaDataCreate = Set-IshMetadataField -IshSession $ishSession -Name 'FISHFALLBACKLNGDEFAULT' -Level 'lng' -Value 'en' |
    ///                   Set-IshMetadataField -IshSession $ishSession -Name 'FISHFALLBACKLNGIMAGES' -Level 'lng' -Value 'en' |
    ///                   Set-IshMetadataField -IshSession $ishSession -Name 'FISHFALLBACKLNGRESOURCES' -Level 'lng' -Value 'en'
    /// Add-IshPublicationOutput -IshSession $ishSession `
    /// -LogicalId "GUID-7EB6F836-A801-4DB3-A54A-22C207BAF671" `
    /// -Version "1" `
    /// -OutputFormat "GUID-2A69335D-F025-4963-A142-5E49988C7C0C" `
    /// -LanguageCombination "en" `
    /// -Metadata $metaDataCreate
    /// </code>
    /// <para>Creating a new publication output. New-IshSession will submit into SessionState, so it can be reused by this cmdlet.</para>
    /// </example>
    [Cmdlet(VerbsCommon.Add, "IshPublicationOutput", SupportsShouldProcess = true)]
    [OutputType(typeof(IshPublicationOutput))]
    public sealed class AddIshPublicationOutput : PublicationOutputCmdlet
    {

        /// <summary>
        /// <para type="description">The IshSession variable holds the authentication and contract information. This object can be initialized using the New-IshSession cmdlet.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshObjectsGroup")]
        [ValidateNotNullOrEmpty]
        public IshSession IshSession { get; set; }

        /// <summary>
        /// <para type="description">The <see cref="Objects.Public.IshObject"/>s that need to be added.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "IshObjectsGroup")]
        [AllowEmptyCollection]
        public IshObject[] IshObject { get; set; }

        /// <summary>
        /// <para type="description">The FolderId for the PublicationOutput object.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshObjectsGroup")]
        [ValidateNotNullOrEmpty]
        public long FolderId
        {
            get { return _folderId; }
            set { _folderId = value; }
        }

        /// <summary>
        /// <para type="description">The FolderId of the PublicationOutput by IshFolder object</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshObjectsGroup")]
        [ValidateNotNull]
        public IshFolder IshFolder
        {
            private get { return null; }  // required otherwise XmlDoc2CmdletDoc crashes with 'System.ArgumentException: Property Get method was not found.'
            set { _folderId = value.IshFolderRef; }
        }

        /// <summary>
        /// <para type="description">The LogicalId of the Publication.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup"), ValidateNotNullOrEmpty]
        public string LogicalId
        {
            get { return _logicalId; }
            set { _logicalId = value; }
        }

        /// <summary>
        /// <para type="description">The Version of the Publication.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup"), ValidateNotNull]
        public string Version
        {
            get { return _version; }
            set { _version = value; }
        }

        /// <summary>
        /// <para type="description">The requested OutputFormat for the new PublicationOutput.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup"), ValidateNotNull]
        public string OutputFormat { get; set; }

        /// <summary>
        /// <para type="description">The requested language combination for the new PublicationOutput.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup"), ValidateNotNull]
        public string LanguageCombination { get; set; }

        /// <summary>
        /// <para type="description">The metadata for the new PublicationOutput.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup"), ValidateNotNull]
        public IshField[] Metadata { get; set; }

        #region Private fields 
        /// <summary>
        /// Logical Id will be defaulted to typical GUID-XYZ in uppercase
        /// </summary>
        private string _logicalId = ("GUID-" + Guid.NewGuid()).ToUpper();
        /// <summary>
        /// Version will be defaulted to NEW, meaning that a first or next version will be created for existing objects
        /// </summary>
        private string _version = "NEW";
        /// <summary>
        /// Holds the folder card id, specified by incoming parameter (long,IShObject)
        /// </summary>
        private long _folderId = -1;
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
        /// Process the Add-IshPublicationOutput commandlet.
        /// </summary>
        /// <exception cref="TrisoftAutomationException"></exception>
        /// <exception cref="Exception"></exception>
        /// <remarks>Writes an <see cref="Objects.Public.IshObject"/> array to the pipeline.</remarks>
        protected override void ProcessRecord()
        {
            try
            {
                // validating the input 
                List<IshObject> returnedObjects = new List<IshObject>();

                WriteDebug("Adding");

                if (IshObject != null)
                {
                    // Using the pipeline
                    int current = 0;
                    IshObjects ishObjects = new IshObjects(IshObject);
                    foreach (IshObject ishObject in ishObjects.Objects)
                    {
                        // Get values
                        WriteDebug($"Id[{ishObject.IshRef}] {++current}/{IshObject.Length}");
                        string logicalId = ishObject.IshRef;
                        //Remember that RetrieveFirst prefers id over element over value ishfields
                        var versionMetadataField = ishObject.IshFields.RetrieveFirst("VERSION", Enumerations.Level.Version).ToMetadataField() as IshMetadataField;
                        string version = versionMetadataField.Value;
                        var outputFormatMetadataField = ishObject.IshFields.RetrieveFirst("FISHOUTPUTFORMATREF", Enumerations.Level.Lng).ToMetadataField() as IshMetadataField;
                        string outputFormat = outputFormatMetadataField.Value;
                        var languageCombinationMetadataField = ishObject.IshFields.RetrieveFirst("FISHPUBLNGCOMBINATION", Enumerations.Level.Lng).ToMetadataField() as IshMetadataField;
                        string languageCombination = languageCombinationMetadataField.Value;
                        var metadata = IshSession.IshTypeFieldSetup.ToIshMetadataFields(ISHType, ishObject.IshFields, Enumerations.ActionMode.Create);
                        
                        PublicationOutput25ServiceReference.CreateResponse response = null;
                        if (ShouldProcess(logicalId + "=" + version + "=" + languageCombination + "=" + outputFormat))
                        {
                            response =
                                IshSession.PublicationOutput25.Create(new PublicationOutput25ServiceReference.
                                    CreateRequest(
                                    _folderId,
                                    logicalId,
                                    version,
                                    outputFormat,
                                    languageCombination,
                                    metadata.ToXml()));
                        }

                        IshFields requestedMetadata = IshSession.IshTypeFieldSetup.ToIshRequestedMetadataFields(IshSession.DefaultRequestedMetadata, ISHType, metadata, Enumerations.ActionMode.Read);
                        var response2 =
                            IshSession.PublicationOutput25.GetMetadata(new PublicationOutput25ServiceReference.
                                GetMetadataRequest(
                                response.logicalId, response.version, outputFormat, languageCombination,
                                requestedMetadata.ToXml()));
                        string xmlIshObjects = response2.xmlObjectList;
                        IshObjects retrievedObjects = new IshObjects(ISHType, xmlIshObjects);
                        returnedObjects.AddRange(retrievedObjects.Objects);

                    }
                }
                else
                {
                    var metadata = IshSession.IshTypeFieldSetup.ToIshMetadataFields(ISHType, new IshFields(Metadata), Enumerations.ActionMode.Create);

                    PublicationOutput25ServiceReference.CreateResponse response = null;
                    if (ShouldProcess(LogicalId + "=" + Version + "=" + LanguageCombination + "=" + OutputFormat))
                    {
                        response =
                            IshSession.PublicationOutput25.Create(new PublicationOutput25ServiceReference.
                            CreateRequest(
                            _folderId,
                            LogicalId,
                            Version,
                            OutputFormat,
                            LanguageCombination,
                            metadata.ToXml()));
                    }

                    IshFields requestedMetadata = IshSession.IshTypeFieldSetup.ToIshRequestedMetadataFields(IshSession.DefaultRequestedMetadata, ISHType, metadata, Enumerations.ActionMode.Read);
                    var response2 =
                        IshSession.PublicationOutput25.GetMetadata(new PublicationOutput25ServiceReference.
                            GetMetadataRequest(
                            response.logicalId, response.version, OutputFormat, LanguageCombination,
                            requestedMetadata.ToXml()));
                    string xmlIshObjects = response2.xmlObjectList;
                    IshObjects retrievedObjects = new IshObjects(ISHType, xmlIshObjects);
                    returnedObjects.AddRange(retrievedObjects.Objects);
                }

                // Write objects to the pipeline               
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
