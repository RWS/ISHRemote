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
using Trisoft.ISHRemote.HelperClasses;
using System.Linq;
using System.Runtime.Remoting.Messaging;
using Trisoft.ISHRemote.BackgroundTask25ServiceReference;
using Trisoft.ISHRemote.EventMonitor25ServiceReference;
using System.Text;

namespace Trisoft.ISHRemote.Cmdlets.BackgroundTask
{
    /// <summary>
    /// <para type="synopsis">The Add-IshBackgroundTask cmdlet add fire-and-forget asynchronous processing events to the CMS generic queuing system.</para>
    /// <para type="description">Add-IshBackgroundTask ParameterGroup variation uses BackgroundTask25.CreateBackgroundTask(WithStartAfter) that allows you to submit generic messages. Note that this requires a generic BackgroundTask service message handler.</para>
    /// <para type="description">Add-IshBackgroundTask IshObjectsGroup requires content object(s) which are transformed as message inputdata and passed to DocumentObj25.RaiseEventByIshLngRefs. This function will server-side validate the incoming objects and trigger an internal BackgroundTask25.CreateBackgroundTask.</para>
    /// </summary>
    /// <example>
    /// <code>
    /// New-IshSession -WsBaseUrl "https://example.com/ISHWS/" -PSCredential "Admin"
    /// $ishBackgroundTask = Get-IshFolderContent -FolderPath "General\MyFolder\Topics" -VersionFilter Latest -LanguagesFilter en |
    ///                      Add-IshBackgroundTask -EventType "SMARTTAG"
    /// </code>
    /// <para>Add BackgroundTask with event type "SMARTTAG" for the objects located under the "General\MyFolder\Topics" path</para> 
    /// </example>
    /// <example>
    /// <code>
    /// New-IshSession -WsBaseUrl "https://example.com/ISHWS/" -PSCredential "Admin"
    /// $ishBackgroundTask = Get-IshFolder -FolderPath "General\Myfolder" -FolderTypeFilter @("ISHModule", "ISHMasterDoc", "ISHLibrary") -Recurse |
    ///                      Get-IshFolderContent -VersionFilter Latest -LanguagesFilter en |
    ///                      Add-IshBackgroundTask -EventType "SMARTTAG"
    /// </code>
    /// <para>Add BackgroundTask with event type "SMARTTAG" for the latest-version en(glish) content objects of type topic, map and topic library; located under the "General\MyFolder" path. Trigger a legacy correction event of SMARTTAG across many folders. Note that Get-IshFolder gives you a progress bar for follow-up. Note that it is possible to configure the BackgroundTask-handler with a variation of the SMARTTAG event to do more-or-less fields for automatic concept suggestions.</para> 
    /// </example>
    /// <example>
    /// <code>
    /// New-IshSession -WsBaseUrl "https://example.com/ISHWS/" -PSCredential "Admin"
    /// $rawData = "&lt;data&gt;&lt;export-document-type&gt;ISHPublication&lt;/export-document-type&gt;&lt;export-document-level&gt;lng&lt;/export-document-level&gt;&lt;export-ishlngref&gt;549482&lt;/export-ishlngref&gt;&lt;creationdate&gt;20210303070257182&lt;/creationdate&gt;&lt;/data&gt;"
    /// $ishBackgroundTask = Add-IshBackgroundTask -EventType "PUBLISH" -EventDescription "Custom publish event description" -RawInputData $rawData
    /// </code>
    /// <para>Add background task with the event type "PUBLISH" and provided event description and publish input raw data. Note: example code only, for publish operations usage of Publish-IshPublicationOutput cmdlet is preferred.</para> 
    /// </example>
    /// <example>
    /// <code>
    /// New-IshSession -WsBaseUrl "https://example.com/ISHWS/" -PSCredential "Admin"
    /// $rawData = "&lt;data&gt;&lt;export-document-type&gt;ISHPublication&lt;/export-document-type&gt;&lt;export-document-level&gt;lng&lt;/export-document-level&gt;&lt;export-ishlngref&gt;549482&lt;/export-ishlngref&gt;&lt;creationdate&gt;20210303070257182&lt;/creationdate&gt;&lt;/data&gt;"
    /// $date = (Get-Date).AddDays(1)
    /// $ishBackgroundTask = Add-IshBackgroundTask -EventType "PUBLISH" -EventDescription "Custom publish event description" -RawInputData $rawData -StartAfter $date
    /// </code>
    /// <para>Add background task with the event type "PUBLISH" and provided event description and publish input raw data.
    /// Provided StartAfter parameter with tomorrow's date indicates that background task should not be executed before this date. Note: example code only, for publish operations usage of Publish-IshPublicationOutput cmdlet is preferred.</para> 
    /// </example>
    [Cmdlet(VerbsCommon.Add, "IshBackgroundTask", SupportsShouldProcess = false)]
    [OutputType(typeof(IshBackgroundTask))]
    public sealed class AddIshBackgroundTask : BackgroundTaskCmdlet
    {
        /// <summary>
        /// <para type="description">The IshSession variable holds the authentication and contract information. This object can be initialized using the New-IshSession cmdlet.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshObjectsGroup")]
        [ValidateNotNullOrEmpty]
        public IshSession IshSession { get; set; }

        /// <summary>
        /// <para type="description">Type of the event (e.g. SMARTTAG). Needs a match CMS BackgroundTask service handler entry.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshObjectsGroup")]
        [ValidateNotNullOrEmpty]
        public string EventType { get; set; }

        /// <summary>
        /// <para type="description">The input data for the background task.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]
        [ValidateNotNullOrEmpty]
        public string RawInputData { get; set; }

        /// <summary>
        /// <para type="description">Description of the event</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]
        [ValidateNotNullOrEmpty]
        public string EventDescription { get; set; }

        /// <summary>
        /// <para type="description">Date time indicating that the background task should not be picked up and executed before it.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]
        public DateTime? StartAfter { get; set; }

        /// <summary>
        /// <para type="description">The <see cref="IshObjects"/>s that will be used for background task creation.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "IshObjectsGroup")]
        [AllowEmptyCollection]
        public IshObject[] IshObject { get; set; }

        #region Private fields
        private readonly List<IshObject> _retrievedIshObjects = new List<IshObject>();
        private readonly DateTime _modifiedSince = DateTime.Today.AddDays(-1);
        private readonly BackgroundTask25ServiceReference.eUserFilter _userFilter = EnumConverter.ToUserFilter<BackgroundTask25ServiceReference.eUserFilter>(Enumerations.UserFilter.Current);
        private readonly int _startEventMaxProgress = 100;
        #endregion

        protected override void BeginProcessing()
        {
            if (IshSession == null) { IshSession = (IshSession)SessionState.PSVariable.GetValue(ISHRemoteSessionStateIshSession); }
            if (IshSession == null) { throw new ArgumentException(ISHRemoteSessionStateIshSessionException); }
            WriteDebug($"Using IshSession[{IshSession.Name}] from SessionState.{ISHRemoteSessionStateIshSession}");
            
            switch (ParameterSetName)
            {
                case "ParameterGroup":
                    if ((IshSession.ServerIshVersion.MajorVersion < 13) || ((IshSession.ServerIshVersion.MajorVersion == 13) && (IshSession.ServerIshVersion.RevisionVersion < 2)))
                    {
                        throw new PlatformNotSupportedException($"Add-IshBackgroundTask with the current parameter set requires server-side BackgroundTask API which is only available starting from 13SP2/13.0.2 and up. ServerIshVersion[{IshSession.ServerVersion}]");
                    }
                    break;
                case "IshObjectsGroup":
                    if ((IshSession.ServerIshVersion.MajorVersion < 14) || ((IshSession.ServerIshVersion.MajorVersion == 14) && (IshSession.ServerIshVersion.RevisionVersion < 4)))
                    {
                        throw new PlatformNotSupportedException($"Add-IshBackgroundTask with the current parameter set requires server-side DocumentObj API which is only available starting from 14SP4/14.0.4 and up. ServerIshVersion[{IshSession.ServerVersion}]");
                    }
                    break;
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
        /// Process the Add-IshBackgroundTask command-let.
        /// </summary>
        /// <exception cref="TrisoftAutomationException"></exception>
        /// <exception cref="Exception"></exception>
        /// <remarks>Writes <see cref="IshBackgroundTask"/> to the pipeline.</remarks>
        protected override void EndProcessing()
        {
            try
            {
                IshFields metadataFilter = new IshFields();
                IshFields requestedMetadata = IshSession.IshTypeFieldSetup.ToIshRequestedMetadataFields(IshSession.DefaultRequestedMetadata, ISHType, new IshFields(), Enumerations.ActionMode.Find);
                var startEventResponse = new StartEventResponse();
                var progressIds = new List<long>();
                if (ParameterSetName == "IshObjectsGroup")
                {
                   var ishObjectsDividedInBatches = DevideListInBatchesByLogicalId(_retrievedIshObjects, IshSession.MetadataBatchSize);

                    int currentIshObjectsCount = 0;
                    foreach (var ishObjectsGroup in ishObjectsDividedInBatches)
                    {
                        // Create BackgroundTask
                        var lngCardIds = ishObjectsGroup.Select(ishObject => Convert.ToInt64(ishObject.ObjectRef[Enumerations.ReferenceType.Lng])).ToList();
                        var progressId = IshSession.DocumentObj25.RaiseEventByIshLngRefs(lngCardIds.ToArray(), EventType);
                        progressIds.Add(progressId);
                        currentIshObjectsCount += ishObjectsGroup.Count;
                        WriteDebug($"RaiseEventByIshLngRefs.length[{lngCardIds.Count}] {currentIshObjectsCount}/{_retrievedIshObjects.Count}");
                    }
                }
                
                if (ParameterSetName == "ParameterGroup")
                {
                    // Start event 
                    var startEventRequest = new StartEventRequest
                    {
                        description = EventDescription,
                        eventType = EventType,
                        maximumProgress = _startEventMaxProgress
                    };
                    startEventResponse = IshSession.EventMonitor25.StartEvent(startEventRequest);
                }

                if (ParameterSetName == "ParameterGroup" && StartAfter.HasValue)
                { 
                    // Create BackgroundTask 
                    var newBackgroundTaskWithStartAfterRequest = new CreateBackgroundTaskWithStartAfterRequest
                    {
                        eventType = EventType,
                        hashId = "",
                        inputData = Encoding.Unicode.GetBytes(RawInputData),
                        startAfter = StartAfter.Value,
                        progressId = startEventResponse.progressId
                    };
                    var createBackgroundTaskStartAfterResponse = IshSession.BackgroundTask25.CreateBackgroundTaskWithStartAfter(newBackgroundTaskWithStartAfterRequest);
                    var progressId = createBackgroundTaskStartAfterResponse.progressId;
                    progressIds.Add(progressId);
                }

                if (ParameterSetName == "ParameterGroup" && !StartAfter.HasValue)
                {
                    // Create BackgroundTask 
                    var newBackgroundTaskRequest = new CreateBackgroundTaskRequest
                    {
                        eventType = EventType,
                        hashId = "",
                        inputData = Encoding.Unicode.GetBytes(RawInputData),
                        progressId = startEventResponse.progressId
                    };
                    var createBackgroundTaskResponse = IshSession.BackgroundTask25.CreateBackgroundTask(newBackgroundTaskRequest);
                    var progressId = createBackgroundTaskResponse.progressId;
                    progressIds.Add(progressId);
                }

                // Find and return IshBackgroundTask object
                if (progressIds.Count > 1)
                {
                    var progressIdsAsString = string.Join(IshSession.Separator, progressIds.ToArray());
                    metadataFilter.AddField(new IshMetadataFilterField(FieldElements.BackgroundTaskProgressId,
                        Enumerations.Level.Task, Enumerations.FilterOperator.In, progressIdsAsString,
                        Enumerations.ValueType.Element));
                }
                else
                {
                    metadataFilter.AddField(new IshMetadataFilterField(FieldElements.BackgroundTaskProgressId,
                        Enumerations.Level.Task, Enumerations.FilterOperator.In, progressIds.First().ToString(),
                        Enumerations.ValueType.Element));
                }

                WriteDebug($"Finding BackgroundTask UserFilter[{_userFilter}] MetadataFilter.length[{metadataFilter.ToXml().Length}] RequestedMetadata.length[{requestedMetadata.ToXml().Length}]");
                var xmlIshBackgroundTasks = IshSession.BackgroundTask25.Find(
                    _modifiedSince,
                    _userFilter,
                    metadataFilter.ToXml(),
                    requestedMetadata.ToXml());
                List<IshBackgroundTask> returnIshBackgroundTasks = new IshBackgroundTasks(xmlIshBackgroundTasks).BackgroundTasks;

                WriteVerbose("returned object count[" + returnIshBackgroundTasks.Count + "]");
                WriteObject(IshSession, ISHType, returnIshBackgroundTasks.ConvertAll(x => (IshBaseObject)x), true);
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

