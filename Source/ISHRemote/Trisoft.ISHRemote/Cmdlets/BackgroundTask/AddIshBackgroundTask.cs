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
using System.Security.Cryptography;
using Trisoft.ISHRemote.BackgroundTask25ServiceReference;
using Trisoft.ISHRemote.EventMonitor25ServiceReference;
using System.Text;
using System.Xml.Linq;
using Microsoft.IdentityModel.Tokens;
using static Trisoft.ISHRemote.Objects.Enumerations;

namespace Trisoft.ISHRemote.Cmdlets.BackgroundTask
{
    /// <summary>
    /// <para type="synopsis">The Add-IshBackgroundTask cmdlet adds fire-and-forget asynchronous processing events to the CMS generic queuing system.</para>
    /// <para type="description">Add-IshBackgroundTask ParameterGroup variation uses BackgroundTask25.CreateBackgroundTask(WithStartAfter) that allows you to submit generic messages. Note that this requires a generic BackgroundTask service message handler.</para>
    /// <para type="description">Add-IshBackgroundTask IshObjectsGroup requires content object(s) which are transformed as message inputdata and passed to the Background Task.</para>
    /// </summary>
    /// <example>
    /// <code>
    /// New-IshSession -WsBaseUrl "https://example.com/ISHWS/"
    /// $ishBackgroundTask = Get-IshFolderContent -FolderPath "General\MyFolder\Topics" -VersionFilter Latest -LanguagesFilter en |
    ///                      Add-IshBackgroundTask -EventType "SMARTTAG"
    /// </code>
    /// <para>Add BackgroundTask with event type "SMARTTAG" for the objects located under the "General\MyFolder\Topics" path.  Trigger a legacy correction event of SMARTTAG across many folders. Note that the default value for the -InputDataTemplate is IshObjectsWithLngRef.</para> 
    /// </example>
    /// <example>
    /// <code>
    /// New-IshSession -WsBaseUrl "https://example.com/ISHWS/"
    /// $ishBackgroundTask = Get-IshFolder -FolderPath "General\Myfolder" -FolderTypeFilter @("ISHModule", "ISHMasterDoc", "ISHLibrary") -Recurse |
    ///                      ForEach-Object -Process {
    ///                        Get-IshFolderContent -IshFolder $_ -VersionFilter Latest -LanguagesFilter en |
    ///                        Add-IshBackgroundTask -EventType "SMARTTAG"
    ///                      }
    /// </code>
    /// <para>Add BackgroundTask with event type "SMARTTAG" for the latest-version en(glish) content objects of type topic, map and topic library; located under the "General\MyFolder" path. Trigger a legacy correction event of SMARTTAG per folder. Note that the default value for the -InputDataTemplate is IshObjectsWithLngRef. Note that Get-IshFolder gives you a progress bar for follow-up. Note that it is possible to configure the BackgroundTask-handler with a variation of the SMARTTAG event to do more-or-less fields for automatic concept suggestions.</para> 
    /// </example>
    /// <example>
    /// <code>
    /// New-IshSession -WsBaseUrl "https://example.com/ISHWS/"
    /// $ishBackgroundTask = Get-IshFolder -BaseFolder Data -FolderTypeFilter @("ISHIllustration") -Recurse |
    ///                      ForEach-Object -Process {
    ///                        Get-IshFolderContent -VersionFilter Latest |
    ///                        Add-IshBackgroundTask -EventType "SYNCHRONIZEMETRICS" -InputDataTemplate IshObjectsWithIshRef
    ///                      }
    /// </code>
    /// <para>Add BackgroundTask with event type "SYNCHRONIZEMETRICS" for the latest-version content objects of type image; located under the "General" path. Trigger an event of synchronizing images per folder to the Metrics subsystem. Note that Get-IshFolder gives you a progress bar for follow-up.</para>
    /// </example>
    /// <example>
    /// <code>
    /// New-IshSession -WsBaseUrl "https://example.com/ISHWS/"
    /// $ishBackgroundTask = Get-IshFolder -FolderPath "General\MyFolder\Images" -Recurse |
    ///                      ForEach-Object -Process {
    ///                        Get-IshFolderContent -IshFolder $_ -VersionFilter Latest -LanguagesFilter en |
    ///                        Add-IshBackgroundTask -EventType "FOLDEREXPORT" -InputDataTemplate EventDataWithIshLngRefs
    ///                      }
    /// </code>
    /// <para>Add BackgroundTask with event type `FOLDEREXPORT` for the objects located under the `General\MyFolder\Images` path. Note that the BackgroundTask handler behind all `...EXPORT` events like `SEARCHEXPORT` or `INBOXEXPORT` on Tridion Docs 15.1 and earlier is identical. One BackgroundTask message will appear per folder containing a list of all latest version English (`en`) content objects in the `InputData` of the message. Note that without the `ForEach-Object` construction all recursively found content objects would all be passed in one BackgroundTask message.</para>
    /// </example>
    /// <example>
    /// <code>
    /// New-IshSession -WsBaseUrl "https://example.com/ISHWS/"
    /// $rawData = "&lt;data&gt;&lt;export-document-type&gt;ISHPublication&lt;/export-document-type&gt;&lt;export-document-level&gt;lng&lt;/export-document-level&gt;&lt;export-ishlngref&gt;549482&lt;/export-ishlngref&gt;&lt;creationdate&gt;20210303070257182&lt;/creationdate&gt;&lt;/data&gt;"
    /// $ishBackgroundTask = Add-IshBackgroundTask -EventType "PUBLISH" -EventDescription "Custom publish event description" -RawInputData $rawData
    /// </code>
    /// <para>Add background task with the event type "PUBLISH" and provided event description and publish input raw data. Note: example code only, for publish operations usage of Publish-IshPublicationOutput cmdlet is preferred.</para> 
    /// </example>
    /// <example>
    /// <code>
    /// New-IshSession -WsBaseUrl "https://example.com/ISHWS/"
    /// $rawData = "&lt;data&gt;&lt;export-document-type&gt;ISHPublication&lt;/export-document-type&gt;&lt;export-document-level&gt;lng&lt;/export-document-level&gt;&lt;export-ishlngref&gt;549482&lt;/export-ishlngref&gt;&lt;creationdate&gt;20210303070257182&lt;/creationdate&gt;&lt;/data&gt;"
    /// $date = (Get-Date).AddDays(1)
    /// $ishBackgroundTask = Add-IshBackgroundTask -EventType "PUBLISH" -EventDescription "Custom publish event description" -RawInputData $rawData -StartAfter $date
    /// </code>
    /// <para>Add background task with the event type "PUBLISH" and provided event description and publish input raw data.
    /// Provided StartAfter parameter with tomorrow's date indicates that background task should not be executed before this date. Note: example code only, for publish operations usage of Publish-IshPublicationOutput cmdlet is preferred.</para> 
    /// </example>
    [Cmdlet(VerbsCommon.Add, "IshBackgroundTask", SupportsShouldProcess = true)]
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
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshObjectsGroup")]
        [ValidateNotNullOrEmpty]
        public string EventDescription { get; set; }

        /// <summary>
        /// <para type="description">Date time indicating that the background task should not be picked up and executed before it.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshObjectsGroup")]
        public DateTime? StartAfter { get; set; }

        /// <summary>
        /// <para type="description">The <see cref="IshObjects"/>s that will be used for background task creation.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "IshObjectsGroup")]
        [AllowEmptyCollection]
        public IshObject[] IshObject { get; set; }

        /// <summary>
        /// <para type="description">The InputDataTemplate (e.g. IshObjectWithLngRef) indicates whether a list of ishObjects or one ishObject is submitted as input data to the background task.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshObjectsGroup")]
        public InputDataTemplate InputDataTemplate { get; set; } = InputDataTemplate.IshObjectsWithLngRef;

        /// <summary>
        /// <para type="description">The hash id of the background task.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshObjectsGroup")]
        [ValidateLength(0, 80)]
        public string HashId { get; set; }

        #region Private fields
        private readonly List<IshObject> _retrievedIshObjects = new List<IshObject>();
        private readonly DateTime _modifiedSince = DateTime.Today.AddDays(-1);
        private readonly eUserFilter _userFilter = EnumConverter.ToUserFilter<eUserFilter>(Enumerations.UserFilter.Current);
        private readonly int _startEventMaxProgress = 100;
        #endregion

        protected override void BeginProcessing()
        {
            if (IshSession == null) { IshSession = (IshSession)SessionState.PSVariable.GetValue(ISHRemoteSessionStateIshSession); }
            if (IshSession == null) { throw new ArgumentException(ISHRemoteSessionStateIshSessionException); }
            WriteDebug($"Using IshSession[{IshSession.Name}] from SessionState.{ISHRemoteSessionStateIshSession}");

            if ((IshSession.ServerIshVersion.MajorVersion < 13) || ((IshSession.ServerIshVersion.MajorVersion == 13) && (IshSession.ServerIshVersion.RevisionVersion < 2)))
            {
                throw new PlatformNotSupportedException($"Add-IshBackgroundTask with the current parameter set requires server-side BackgroundTask API which is only available starting from 13SP2/13.0.2 and up. ServerIshVersion[{IshSession.ServerVersion}]");
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
        /// Process the Add-IshBackgroundTask cmdlet.
        /// </summary>
        /// <exception cref="TrisoftAutomationException"></exception>
        /// <exception cref="Exception"></exception>
        /// <remarks>Writes <see cref="IshBackgroundTask"/> to the pipeline.</remarks>
        protected override void EndProcessing()
        {
            try
            {
                var metadataFilter = new IshFields();
                var requestedMetadata = IshSession.IshTypeFieldSetup.ToIshRequestedMetadataFields(IshSession.DefaultRequestedMetadata, ISHType, new IshFields(), ActionMode.Find);
                long progressId = 0;
                var ishObjectsCount = 0;
                var inputData = "";

                switch (InputDataTemplate)
                {
                    case InputDataTemplate.IshObjectWithLngRef:
                        {
                            // inputData looks like <ishobject ishtype='ISHMasterDoc' ishref='GUID-X' ishlogicalref='45677' ishversionref='45678' ishlngref='45679'> or <ishobject ishtype='ISHBaseline' ishref='GUID-X' ishbaselineref='45798'>
                            if (_retrievedIshObjects.Count > 1)
                            {
                                WriteWarning("More than one IshObject was provided. Only the first will be passed, the others will be ignored.");
                            }
                            var ishObject = _retrievedIshObjects.First();
                            if (ishObject.IshType == Enumerations.ISHType.ISHBaseline)
                            {
                                var ishObjectElement = new XElement("ishobject",
                                    new XAttribute("ishtype", ishObject.IshType),
                                    new XAttribute("ishref", ishObject.IshRef),
                                    new XAttribute("ishbaselineref", ishObject.ObjectRef[ReferenceType.Baseline])
                                );
                                inputData = ishObjectElement.ToString();
                            }
                            else
                            {
                                var ishObjectElement = new XElement("ishobject",
                                    new XAttribute("ishtype", ishObject.IshType),
                                    new XAttribute("ishref", ishObject.IshRef),
                                    new XAttribute("ishlogicalref", ishObject.ObjectRef[ReferenceType.Logical]),
                                    new XAttribute("ishversionref", ishObject.ObjectRef[ReferenceType.Version]),
                                    new XAttribute("ishlngref", ishObject.ObjectRef[ReferenceType.Lng])
                                );
                                inputData = ishObjectElement.ToString();
                            }

                            ishObjectsCount = 1;
                            break;
                        }
                    case InputDataTemplate.IshObjectsWithLngRef:
                        {
                            // inputData looks like <ishobjects><ishobject ishtype='ISHMasterDoc' ishref='GUID-X' ishlngref='45679'>...
                            var ishObjects = new XElement("ishobjects");
                            foreach (var ishObjectElement in _retrievedIshObjects.Select(retrievedIshObject => new XElement("ishobject",
                                         new XAttribute("ishtype", retrievedIshObject.IshType),
                                         new XAttribute("ishref", retrievedIshObject.IshRef),
                                         new XAttribute("ishlogicalref", retrievedIshObject.ObjectRef[ReferenceType.Logical]),
                                         new XAttribute("ishversionref", retrievedIshObject.ObjectRef[ReferenceType.Version]),
                                         new XAttribute("ishlngref", retrievedIshObject.ObjectRef[ReferenceType.Lng]))))
                            {
                                ishObjects.Add(ishObjectElement);

                                ishObjectsCount++;
                            }

                            inputData = ishObjects.ToString();

                            break;
                        }
                    case InputDataTemplate.IshObjectsWithIshRef:
                        {
                            // inputData looks like <ishobjects><ishobject ishtype='ISHMasterDoc' ishref='GUID-X'>...
                            var ishObjectsGroupedByIshRef = _retrievedIshObjects.GroupBy(ishObject => ishObject.IshRef);
                            var ishObjects = new XElement("ishobjects");
                            foreach (var ishObjectsIshRefGroup in ishObjectsGroupedByIshRef)
                            {
                                var ishObjectElement = new XElement("ishobject",
                                    new XAttribute("ishtype", ishObjectsIshRefGroup.First().IshType),
                                    new XAttribute("ishref", ishObjectsIshRefGroup.First().IshRef));
                                ishObjects.Add(ishObjectElement);

                                ishObjectsCount++;
                            }

                            inputData = ishObjects.ToString();

                            break;
                        }
                    case InputDataTemplate.EventDataWithIshLngRefs:
                        {
                            // inputData looks like <eventdata><lngcardids>13043819, 13058357, 14246721, 13058260</lngcardids></eventdata>
                            var lngCardIds = _retrievedIshObjects.Select(ishObject => Convert.ToInt64(ishObject.ObjectRef[Enumerations.ReferenceType.Lng])).ToList();
                            var eventdataElement = new XElement("eventdata");
                            var lngcardidsElement = new XElement("lngcardids")
                            {
                                Value = String.Join(", ", lngCardIds)
                            };
                            eventdataElement.Add(lngcardidsElement);

                            ishObjectsCount = lngCardIds.Count;
                            inputData = eventdataElement.ToString();
                            break;
                        }
                }

                if (EventDescription.IsNullOrEmpty())
                {
                    EventDescription = $"Executing {EventType} for {ishObjectsCount} IshObjects";
                }

                // Start event 
                WriteDebug($"Create StartEvent EventType[{EventType}] EventDescription[{EventDescription}]");
                var startEventRequest = new StartEventRequest
                {
                    description = EventDescription,
                    eventType = EventType,
                    maximumProgress = _startEventMaxProgress
                };

                var backgroundTaskInputData = Encoding.Unicode.GetBytes(ParameterSetName == "IshObjectsGroup" ? inputData : RawInputData);

                if (HashId == null)
                {
                    HashId = CalculateHashId(backgroundTaskInputData);
                }

                if (StartAfter.HasValue)
                {
                    // Create BackgroundTask
                    var message = ParameterSetName == "IshObjectsGroup" ?
                        $"Create BackgroundTask EventType[{EventType}] InputData.length[{inputData.Length}] StartAfter[{StartAfter}]" :
                        $"Create BackgroundTask EventType[{EventType}] RawInputData.length[{RawInputData.Length}] StartAfter[{StartAfter}]";
                    WriteDebug(message);
                    if (ShouldProcess(message))
                    {
                        var startEventResponse = IshSession.EventMonitor25.StartEvent(startEventRequest);
                        var newBackgroundTaskWithStartAfterRequest = new CreateBackgroundTaskWithStartAfterRequest
                        {
                            eventType = EventType,
                            hashId = HashId,
                            inputData = backgroundTaskInputData,
                            startAfter = StartAfter.Value,
                            progressId = startEventResponse.progressId
                        };
                    
                        var createBackgroundTaskStartAfterResponse = IshSession.BackgroundTask25.CreateBackgroundTaskWithStartAfter(newBackgroundTaskWithStartAfterRequest);
                        progressId = createBackgroundTaskStartAfterResponse.progressId;
                    }
                }

                if (!StartAfter.HasValue)
                {
                    // Create BackgroundTask
                    var message = ParameterSetName == "IshObjectsGroup" ?
                        $"Create BackgroundTask EventType[{EventType}] InputData.length[{inputData.Length}]" :
                        $"Create BackgroundTask EventType[{EventType}] RawInputData.length[{RawInputData.Length}]";
                    WriteDebug(message);
                    if (ShouldProcess(message))
                    {
                        var startEventResponse = IshSession.EventMonitor25.StartEvent(startEventRequest);
                        var newBackgroundTaskRequest = new CreateBackgroundTaskRequest
                        {
                            eventType = EventType,
                            hashId = HashId,
                            inputData = backgroundTaskInputData,
                            progressId = startEventResponse.progressId
                        };
                        var createBackgroundTaskStartAfterResponse = IshSession.BackgroundTask25.CreateBackgroundTask(newBackgroundTaskRequest);
                        progressId = createBackgroundTaskStartAfterResponse.progressId;
                    }
                }

                // Find and return IshBackgroundTask object
                metadataFilter.AddField(new IshMetadataFilterField(FieldElements.BackgroundTaskProgressId,
                    Level.Task, FilterOperator.In, progressId.ToString(),
                    Enumerations.ValueType.Value));

                WriteDebug($"Finding BackgroundTask UserFilter[{_userFilter}] MetadataFilter.length[{metadataFilter.ToXml().Length}] RequestedMetadata.length[{requestedMetadata.ToXml().Length}]");
                var xmlIshBackgroundTasks = IshSession.BackgroundTask25.Find(
                    _modifiedSince,
                    _userFilter,
                    metadataFilter.ToXml(),
                    requestedMetadata.ToXml());
                var returnIshBackgroundTasks = new IshBackgroundTasks(xmlIshBackgroundTasks).BackgroundTasks;

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

        private static string CalculateHashId(byte[] inputData)
        {
            using (var sha256Hash = SHA256.Create())
            {
                var bytes = sha256Hash.ComputeHash(inputData);
                var builder = new StringBuilder();

                foreach (var b in bytes)
                {
                    builder.Append(b.ToString("x2"));
                }

                return builder.ToString();
            }
        }
    }
}

