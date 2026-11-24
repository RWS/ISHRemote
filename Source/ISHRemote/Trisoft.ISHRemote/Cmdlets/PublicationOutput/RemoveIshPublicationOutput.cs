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
using System.Collections.Specialized;
using System.Linq;
using System.Management.Automation;
using System.ServiceModel;
using Trisoft.ISHRemote.Exceptions;
using Trisoft.ISHRemote.Objects;
using Trisoft.ISHRemote.Objects.Public;
using Trisoft.ISHRemote.PublicationOutput25ServiceReference;

namespace Trisoft.ISHRemote.Cmdlets.PublicationOutput
{
    /// <summary>
    /// <para type="synopsis">The Remove-IshPublicationOutput cmdlet removes the publication outputs that are passed through the pipeline or determined via provided parameters</para>
    /// <para type="description">The Remove-IshPublicationOutput cmdlet removes the publication outputs that are passed through the pipeline or determined via provided parameters</para>
    /// </summary>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/InfoShareWS/" -IshUserName "username" -IshUserPassword  "userpassword"
    /// # Remove publication output
    /// Remove-IshPublicationOutput `
    /// -LogicalId "GUID-F66C1BB5-076D-455C-B055-DAC5D61AB3D9" `
    /// -Version "1" `
    /// -OutputFormat "PDF (A4 Manual)" `
    /// -LanguageCombination "en"
    /// </code>
    /// <para>New-IshSession will submit into SessionState, so it can be reused by this cmdlet. Removing a publication output</para>
    /// </example>
    /// <example>
    /// <code>
    /// $metadataFilter = Set-IshMetadataFilterField -Name "FISHLASTMODIFIEDON" -Level Lng -ValueType Value -FilterOperator LessThan -Value "01/01/2022 00:00:00"
    /// Get-IshFolder -FolderPath "General\MyPublications" -FolderTypeFilter @("ISHPublication") -Recurse |
    /// ForEach-Object -Process {
    ///     Get-IshFolderContent -IshFolder $_ -VersionFilter "" -MetadataFilter $metadataFilter |
    ///     Remove-IshPublicationOutput -Unrelease -WhatIf
    /// }
    /// </code>
    /// <para>Archiving pattern that walks the publication folders and, for the publication outputs not modified since 2022, describes (-WhatIf) the unrelease of released outputs followed by their removal. Remove -WhatIf to commit. The publication version and logical cards survive because -Force is not used.</para>
    /// </example>
    [Cmdlet(VerbsCommon.Remove, "IshPublicationOutput", SupportsShouldProcess = true)]
    public sealed class RemoveIshPublicationOutput : PublicationOutputCmdlet
    {

        /// <summary>
        /// <para type="description">The IshSession variable holds the authentication and contract information. This object can be initialized using the New-IshSession cmdlet.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshObjectGroup")]
        [ValidateNotNullOrEmpty]
        public IshSession IshSession { get; set; }

        /// <summary>
        /// <para type="description">Array with the publication outputs to remove. This array can be passed through the pipeline or explicitly passed via the parameter.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "IshObjectGroup")]
        [AllowEmptyCollection]
        public IshObject[] IshObject { get; set; }

        /// <summary>
        /// <para type="description">The LogicalId of the publication output</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup"), ValidateNotNullOrEmpty]
        public string LogicalId { get; set; }

        /// <summary>
        /// <para type="description">The version of the publication output</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup"), ValidateNotNullOrEmpty]
        public string Version { get; set; }

        /// <summary>
        /// <para type="description">The output format of the publication output.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup"), ValidateNotNullOrEmpty]
        public string OutputFormat { get; set; }

        /// <summary>
        /// <para type="description">The language combination of the publication output.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup"), ValidateNotNullOrEmpty]
        public string LanguageCombination { get; set; }

        /// <summary>
        /// <para type="description">The required current metadata of the publication output. This parameter can be used to avoid that users override changes done by other users. The cmdlet will check whether the fields provided in this parameter still have the same values in the repository:</para>
        /// <para type="description">If the metadata is still the same, the publication output is removed.</para>
        /// <para type="description">If the metadata is not the same anymore, an error is given and the publication output is not removed.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshObjectGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]
        [ValidateNotNullOrEmpty]
        public IshField[] RequiredCurrentMetadata { get; set; }

        /// <summary>
        /// <para type="description">When the Force switch is set, after deleting the publication output object, the publication version object and publication logical object will be deleted if they don't have any publication outputs anymore.
        /// Be carefull using this option, as it will delete the publication (version) and baseline if there are no outputformats left!!</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshObjectGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]        
        [ValidateNotNullOrEmpty]
        public SwitchParameter Force { get; set; }

        /// <summary>
        /// <para type="description">When the Unrelease switch is set, publication outputs with status Released are first unreleased (status set to Publishing Cancelled) so the server accepts their removal. Publication outputs that are currently publishing are skipped with a warning and neither unreleased nor removed. Requires 14SP4/14.0.4 with hotfix CRQ-31475, or 15/15.0.0 and up.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshObjectGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]
        public SwitchParameter Unrelease { get; set; }

        protected override void BeginProcessing()
        {
            if (IshSession == null) { IshSession = (IshSession)SessionState.PSVariable.GetValue(ISHRemoteSessionStateIshSession); }
            if (IshSession == null) { IshSession = (IshSession)SessionState.PSVariable.GetValue(ISHRemoteSessionStateGlobalIshSession); }
            if (IshSession == null) { throw new ArgumentException(ISHRemoteSessionStateIshSessionException); }
            WriteDebug($"Using IshSession[{IshSession.Name}] from SessionState.{ISHRemoteSessionStateIshSession} or in turn SessionState.{ISHRemoteSessionStateGlobalIshSession}");
            if (Unrelease.IsPresent &&
                ((IshSession.ServerIshVersion.MajorVersion < 14) || ((IshSession.ServerIshVersion.MajorVersion == 14) && (IshSession.ServerIshVersion.RevisionVersion < 4))))
            {
                throw new PlatformNotSupportedException($"Remove-IshPublicationOutput with parameter Unrelease requires server-side unrelease of publication outputs which is only available starting from 14SP4/14.0.4 (with hotfix CRQ-31475) and up. ServerIshVersion[{IshSession.ServerVersion}]");
            }
            base.BeginProcessing();
        }

        private const string PublishingStatus = "VPUBSTATUSPUBLISHING";
        private const string ReleasedStatus = "VPUBSTATUSRELEASED";

        private IshFields PublicationStatusRequestedMetadata()
        {
            var requestedMetadata = new IshFields();
            requestedMetadata.AddField(new IshRequestedMetadataField("FISHPUBSTATUS", Enumerations.Level.Lng, Enumerations.ValueType.Element));
            return requestedMetadata;
        }

        private IshFields UnreleaseMetadata()
        {
            var metadata = new IshFields();
            metadata.AddField(new IshMetadataField("FISHPUBSTATUS", Enumerations.Level.Lng, Enumerations.ValueType.Element, "VPUBSTATUSPUBLISHINGCANCELLED"));
            return IshSession.IshTypeFieldSetup.ToIshMetadataFields(ISHType, metadata, Enumerations.ActionMode.Update);
        }

        /// <summary>
        /// Handles the optional unrelease of a publication output right before its removal.
        /// </summary>
        /// <returns>False when the publication output is currently publishing and must be skipped</returns>
        private bool UnreleaseBeforeRemove(string target, string currentStatus, Action unrelease)
        {
            WriteDebug($"target[{target}] FISHPUBSTATUS[{currentStatus}]");
            if (currentStatus == PublishingStatus)
            {
                WriteWarning($"Skipping target[{target}] because it is currently publishing. Re-run after publishing completes.");
                return false;
            }
            if (currentStatus == ReleasedStatus && ShouldProcess(target + " Unrelease"))
            {
                unrelease();
            }
            return true;
        }


        /// <summary>
        /// Process the Remove-IshPublicationOutput commandlet.
        /// </summary>
        /// <exception cref="TrisoftAutomationException"></exception>
        /// <exception cref="Exception"></exception>
        /// <remarks>Writes an <see cref="Objects.Public.IshObject"/> array to the pipeline.</remarks>
        protected override void ProcessRecord()
        {
            try
            {               
                var logicalIdsVersionsCollection = new NameValueCollection();

                if (IshObject != null && IshObject.Length == 0)
                {
                    // Do nothing
                    WriteVerbose("IshObject is empty, so nothing to remove");
                }
                else
                {
                    WriteDebug("Removing");

                    IshFields requiredCurrentMetadata = new IshFields(RequiredCurrentMetadata);

                    if (IshObject != null)
                    {
                        // Using the pipeline
                        int current = 0;
                        IshObjects ishObjects = new IshObjects(IshObject);
                        foreach (IshObject ishObject in ishObjects.Objects)
                        {
                            long lngRef = Convert.ToInt64(ishObject.ObjectRef[Enumerations.ReferenceType.Lng]);
                            WriteDebug($"lngRef[{lngRef}] {++current}/{ishObjects.Objects.Length}");
                            if (Unrelease.IsPresent)
                            {
                                string currentStatus = ishObject.IshFields.GetFieldValue("FISHPUBSTATUS", Enumerations.Level.Lng, Enumerations.ValueType.Element);
                                if (string.IsNullOrEmpty(currentStatus))
                                {
                                    string xmlStatus = IshSession.PublicationOutput25.RetrieveMetadataByIshLngRefs(new long[] { lngRef }, PublicationStatusRequestedMetadata().ToXml());
                                    var statusObject = new IshObjects(ISHType, xmlStatus).Objects.FirstOrDefault();
                                    currentStatus = (statusObject == null) ? "" : statusObject.IshFields.GetFieldValue("FISHPUBSTATUS", Enumerations.Level.Lng, Enumerations.ValueType.Element);
                                }
                                if (!UnreleaseBeforeRemove(Convert.ToString(lngRef), currentStatus,
                                    () => IshSession.PublicationOutput25.SetMetadataByIshLngRef(lngRef, UnreleaseMetadata().ToXml(), "")))
                                {
                                    continue;
                                }
                            }
                            if (ShouldProcess(Convert.ToString(lngRef)))
                            {
                                IshSession.PublicationOutput25.DeleteByIshLngRef(lngRef, requiredCurrentMetadata.ToXml());
                            }
                            logicalIdsVersionsCollection.Add(ishObject.IshRef,
                                ishObject.IshFields.GetFieldValue("VERSION", Enumerations.Level.Version,
                                    Enumerations.ValueType.Value));
                        }
                    }
                    else
                    {
                        if (Unrelease.IsPresent)
                        {
                            var statusResponse = IshSession.PublicationOutput25.GetMetadata(new GetMetadataRequest(
                                LogicalId, Version, OutputFormat, LanguageCombination, PublicationStatusRequestedMetadata().ToXml()));
                            var statusObject = new IshObjects(ISHType, statusResponse.xmlObjectList).Objects.FirstOrDefault();
                            string currentStatus = (statusObject == null) ? "" : statusObject.IshFields.GetFieldValue("FISHPUBSTATUS", Enumerations.Level.Lng, Enumerations.ValueType.Element);
                            if (!UnreleaseBeforeRemove(LogicalId + "=" + Version + "=" + LanguageCombination + "=" + OutputFormat, currentStatus,
                                () => IshSession.PublicationOutput25.SetMetadata(new SetMetadataRequest(
                                    LogicalId, Version, OutputFormat, LanguageCombination, UnreleaseMetadata().ToXml(), ""))))
                            {
                                return;
                            }
                        }
                        if (ShouldProcess(LogicalId + "=" + Version + "=" + LanguageCombination + "=" + OutputFormat))
                        {
                            IshSession.PublicationOutput25.Delete(
                                LogicalId,
                                Version,
                                OutputFormat,
                                LanguageCombination,
                                requiredCurrentMetadata.ToXml());
                        }
                        logicalIdsVersionsCollection.Add(LogicalId, Version);
                    }
                }

                if (Force.IsPresent && logicalIdsVersionsCollection.Count > 0)
                { 
                    var xmlIshObjects = IshSession.PublicationOutput25.RetrieveMetadata(logicalIdsVersionsCollection.AllKeys.ToArray(),
                        StatusFilter.ISHNoStatusFilter, "", "<ishfields><ishfield name='VERSION' level='version'/><ishfield name='FISHPUBLNGCOMBINATION' level='lng'/></ishfields>");
                    List<IshObject> retrievedObjects = new List<IshObject>();
                    retrievedObjects.AddRange(new IshObjects(xmlIshObjects).Objects);

                    // Delete versions which do not have any language card anymore
                    foreach (string logicalId in logicalIdsVersionsCollection.AllKeys)
                    {                            
                        var versions = logicalIdsVersionsCollection.GetValues(logicalId).Distinct();
                        foreach (var version in versions)
                        {
                            bool versionWithLanguagesFound = false;
                            foreach (var retrievedObject in retrievedObjects)
                            {
                                if (retrievedObject.IshRef == logicalId && retrievedObject.IshFields.GetFieldValue("VERSION", Enumerations.Level.Version, Enumerations.ValueType.Value) == version)
                                {
                                    versionWithLanguagesFound = true;
                                }
                            }
                            if (!versionWithLanguagesFound)
                            {
                                if (ShouldProcess(logicalId + "=" + version + "=" + "="))
                                {
                                    IshSession.PublicationOutput25.Delete(logicalId, version, "", "", "");
                                }
                            }
                        }
                    }

                    if ((IshSession.ServerIshVersion.MajorVersion < 15) || ((IshSession.ServerIshVersion.MajorVersion == 15) && (IshSession.ServerIshVersion.MinorVersion < 1)))
                    {
                        // Delete logical cards which do not have any languages anymore
                        foreach (string logicalId in logicalIdsVersionsCollection.AllKeys)
                        {
                            bool logicalIdFound = false;
                            foreach (var retrievedObject in retrievedObjects)
                            {
                                if (retrievedObject.IshRef == logicalId)
                                {
                                    logicalIdFound = true;
                                }
                            }
                            if (!logicalIdFound)
                            {
                                if (ShouldProcess(logicalId + "==="))
                                {
                                    IshSession.PublicationOutput25.Delete(logicalId, "", "", "", "");
                                }
                            }
                        }
                    }
                }
                WriteVerbose("returned object count[0]");
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
