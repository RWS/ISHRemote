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
using Trisoft.ISHRemote.BackgroundTask25ServiceReference;
using Trisoft.ISHRemote.EventMonitor25ServiceReference;

namespace Trisoft.ISHRemote.Cmdlets.BackgroundTask
{
    /// <summary>
    /// <para type="synopsis">Add BackgroundTask.</para>
    /// <para type="description">Adds BackgroundTask in use cases: </para>
    /// <para type="description">ParameterGroup uses BackgroundTask25 API to create BackgroundTask.</para>
    /// <para type="description">IshObjectsGroup uses content object(s) passed via pipeline or as a parameter to trigger BackgroundTask event using DocumentObj25.RaiseEventByIshLngRefs.</para>
    /// </summary>
    /// <example>
    /// <code>
    /// New-IshSession -WsBaseUrl "https://example.com/ISHWS/" -PSCredential "Admin"
    /// $ishBackgroundTask = Get-IshFolderContent -FolderPath "General\MyFolder" -VersionFilter Latest -Recurse | Add-IshBackgroundTask -EventType "SMARTTAG"
    /// </code>
    /// <para>Add BackgroundTask with event type"SMARTTAG" for the objects located under the "General\MyFolder" path</para> 
    /// </example>
    /// <example>
    /// <code>
    /// New-IshSession -WsBaseUrl "https://example.com/ISHWS/" -PSCredential "Admin"
    /// $ishBackgroundTask  = Add-IshBackgroundTask -EventType "PUSHTRANSLATIONS" -EventDescription "Custom event description" -RawInputData $rawData
    /// </code>
    /// <para>Add background task with the event type "PUSHTRANSLATIONS" and provided event description and input data</para> 
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
        /// <para type="description">Type of the event (e.g. SMARTTAG)</para>
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
        public byte[] RawInputData { get; set; }

        /// <summary>
        /// <para type="description">Description of the event</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]
        [ValidateNotNullOrEmpty]
        public string EventDescription { get; set; }

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
                        throw new PlatformNotSupportedException($"Add-IshBackgroundTask with the current parameter set requires server-side BackgroundTask API which only available starting from 13SP2/13.0.2 and up. ServerIshVersion[{IshSession.ServerVersion}]");
                    }
                    break;
                case "IshObjectsGroup":
                    if ((IshSession.ServerIshVersion.MajorVersion < 14) || ((IshSession.ServerIshVersion.MajorVersion == 14) && (IshSession.ServerIshVersion.RevisionVersion < 4)))
                    {
                        throw new PlatformNotSupportedException($"Add-IshBackgroundTask with the current parameter set requires server-side DocumentObj API which only available starting from 14SP4/14.0.4 and up. ServerIshVersion[{IshSession.ServerVersion}]");
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
                long progressId = 0;
                if (ParameterSetName == "IshObjectsGroup")
                {
                    var lngCardIds = _retrievedIshObjects.Select(ishObject => Convert.ToInt64(ishObject.ObjectRef[Enumerations.ReferenceType.Lng])).ToList();
                    progressId = IshSession.DocumentObj25.RaiseEventByIshLngRefs(lngCardIds.ToArray(), EventType);
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
                    var startEventResponse = IshSession.EventMonitor25.StartEvent(startEventRequest);
                    
                    // Create BackgroundTask 
                    var newBackgroundTaskRequest = new CreateBackgroundTaskRequest
                    {
                        eventType = EventType,
                        hashId = "",
                        inputData = RawInputData,
                        progressId = startEventResponse.progressId
                    };
                    var createBackgroundTaskResponse = IshSession.BackgroundTask25.CreateBackgroundTask(newBackgroundTaskRequest);
                    progressId = createBackgroundTaskResponse.progressId;
                }

                // Find and return IshBackgroundTask object
                metadataFilter.AddField(new IshMetadataFilterField(FieldElements.ProgressId, Enumerations.Level.Task, Enumerations.FilterOperator.Equal, progressId.ToString(), Enumerations.ValueType.Element));
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

