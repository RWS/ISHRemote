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

namespace Trisoft.ISHRemote.Cmdlets.BackgroundTask
{
    /// <summary>
    /// <para type="synopsis">Gets BackgroundTask entries with filtering options.</para>
    /// <para type="description">Uses BackgroundTask25 API to retrieve ishevents showing progress of background task events from the centralized log system.</para>
    /// <para type="description">This table oriented API maps straight through to database column names regarding ishfield usage.</para>
    /// </summary>
    /// <example>
    /// <code>
    /// $requestedMetadata = Set-IshRequestedMetadataField -IshSession $ishSession -Name "EVENTID" -Level "Progress" |
    ///                      Set-IshRequestedMetadataField -IshSession $ishSession -Name "EVENTTYPE" -Level "Progress"
    /// Get-IshEvent -IshSession $ishSession -EventTypes @("EXPORTFORPUBLICATION","SYNCHRONIZETOLIVECONTENT") -RequestedMetadata $requestedMetadata
    /// </code>
    /// <para>Gets all top-level (progress) ishevents filtered to publish and synchronize events.</para>
    /// </example>
    /// <example>
    /// <code>
    /// $requestedMetadata = Set-IshRequestedMetadataField -IshSession $ishSession -Name "EVENTID" -Level "Progress" |
    ///                      Set-IshRequestedMetadataField -IshSession $ishSession -Name "EVENTTYPE" -Level "Progress" |
    ///                      Set-IshRequestedMetadataField -IshSession $ishSession -Name "EVENTDATATYPE" -Level "Detail"
    /// $metadataFilter = Set-IshMetadataFilterField -IshSession $ishSession -Name "EVENTDATATYPE" -Level "Detail" -FilterOperator "NotEqual" -Value "10"
    /// Get-IshEvent -IshSession $ishSession -EventTypes @("EXPORTFORPUBLICATION","SYNCHRONIZETOLIVECONTENT") -RequestedMetadata $requestedMetadata -MetadataFilter $metadataFilter
    /// </code>
    ///   <para>Gets up to detail ishevents filtered to publish and synchronize events and the eventdatatype should differ from 10.</para>
    /// </example>
    /// <example>
    /// <code>
    /// $requestedMetadata = Set-IshRequestedMetadataField -IshSession $ishSession -Name "EVENTID" -Level "Progress" |
    ///                      Set-IshRequestedMetadataField -IshSession $ishSession -Name "EVENTTYPE" -Level "Progress" |
    ///                      Set-IshRequestedMetadataField -IshSession $ishSession -Name "EVENTDATATYPE" -Level "Detail"
    /// $metadataFilter = Set-IshMetadataFilterField -IshSession $ishSession -Name "ACTION" -Level "Detail" -FilterOperator In -Value "Request started, Start execution, Execution completed"
    /// Get-IshEvent -IshSession $ishSession -EventTypes @("EXPORTFORPUBLICATION") -RequestedMetadata $requestedMetadata -MetadataFilter $metadataFilter
    /// </code>
    ///   <para>Gets up to detail ishevents filtered to publish events and filters to only have the queue event, processing started and processing ended. This allows calculation of lead times and through put.</para>
    /// </example>
    [Cmdlet(VerbsCommon.Get, "IshBackgroundTask", SupportsShouldProcess = false)]
    [OutputType(typeof(IshBackgroundTask))]
    public sealed class GetIshBackgroundTask : BackgroundTaskCmdlet
    {

        /// <summary>
        /// <para type="description">The IshSession variable holds the authentication and contract information. This object can be initialized using the New-IshSession cmdlet.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshBackgroundTasksGroup")]
        [ValidateNotNullOrEmpty]
        public IshSession IshSession { get; set; }

        /// <summary>
        /// <para type="description">Enumeration indicating if only events of the current user or all events must be retrieved</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]
        [ValidateNotNullOrEmpty]
        public Enumerations.UserFilter UserFilter
        {
            private get { return Enumerations.UserFilter.All; }  // required otherwise XmlDoc2CmdletDoc crashes with 'System.ArgumentException: Property Get method was not found.'
            set { _userFilter = EnumConverter.ToUserFilter<BackgroundTask25ServiceReference.eUserFilter>(value); }
        }

        /// <summary>
        /// <para type="description">A date limiting the events that will be retrieved based on the last modification date of the events</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]
        [ValidateNotNullOrEmpty]
        public DateTime ModifiedSince
        {
            get { return _modifiedSince; }
            set { _modifiedSince = value; }
        }

        /// <summary>
        /// <para type="description">Filter on metadata to limit the objects on which metadata has to be returned</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshBackgroundTasksGroup")]
        [ValidateNotNullOrEmpty]
        public IshField[] MetadataFilter { get; set; }

        /// <summary>
        /// <para type="description">XML structure indicating which metadata has to be retrieved.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshBackgroundTasksGroup")]
        [ValidateNotNullOrEmpty]
        public IshField[] RequestedMetadata { get; set; }

        /// <summary>
        /// <para type="description">The enumeration indicating which overall status the event must have (e.g. All, Success, Failed,...)</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]
        [ValidateNotNullOrEmpty]
        public Enumerations.BackgroundTaskStatusFilter BackgroundTaskStatusFilter
        {
            private get { return Enumerations.BackgroundTaskStatusFilter.All; }  // required otherwise XmlDoc2CmdletDoc crashes with 'System.ArgumentException: Property Get method was not found.'
            set { _progressStatusFilter = EnumConverter.ToBackgroundTaskStatusFilter<BackgroundTask25ServiceReference.e>(value); }
        }

        /// <summary>
        /// <para type="description">The <see cref="IshBackgroundTask"/>s that need to be handled.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "IshBackgroundTasksGroup")]
        public IshEvent[] IshBackgroundTask { get; set; }

        

        #region Private fields
        private DateTime _modifiedSince = DateTime.Today.AddDays(-1);
        //private BackgroundTask25ServiceReference.B _progressStatusFilter = BackgroundTask25ServiceReference.ProgressStatusFilter.All;
        private BackgroundTask25ServiceReference.eUserFilter _userFilter = BackgroundTask25ServiceReference.eUserFilter.All;
        private List<IshBackgroundTask> _retrievedIshBackgroundTask = new List<IshBackgroundTask>();
        #endregion


        /// <summary>
        /// Process the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            try
            {
                if (IshBackgroundTask != null)
                {
                    foreach(IshBackgroundTask ishBackgroundTask in IshBackgroundTask)
                    {
                        _retrievedIshBackgroundTask.Add(ishBackgroundTask);
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
        /// Process the cmdlet.
        /// </summary>
        /// <exception cref="TrisoftAutomationException"></exception>
        /// <exception cref="Exception"></exception>
        /// <remarks>Writes <see cref="IshEvent"/> to the pipeline.</remarks>
        protected override void EndProcessing()
        {
            try
            {
                IshFields metadataFilter = new IshFields(MetadataFilter);
                IshFields requestedMetadata = IshSession.IshTypeFieldSetup.ToIshRequestedMetadataFields(ISHType, new IshFields(RequestedMetadata), Enumerations.ActionMode.Find);
                string xmlIshBackgroundTasks;
                WriteDebug($"Finding UserFilter[{_userFilter}] MetadataFilter.length[{metadataFilter.ToXml().Length}] RequestedMetadata.length[{requestedMetadata.ToXml().Length}]");
                xmlIshBackgroundTasks = IshSession.BackgroundTask25.Find(
                    ModifiedSince,
                    _userFilter,
                    metadataFilter.ToXml(), 
                    requestedMetadata.ToXml());
                var returnIshBackgroundTasks = new IshBackgroundTasks(xmlIshBackgroundTasks).BackgroundTasks;

                WriteVerbose("returned object count[" + returnIshBackgroundTasks.Length + "]");
                WriteObject(returnIshBackgroundTasks, true);

                /*
                IshFields requestedMetadata = AddRequiredFields(new IshFields(RequestedMetadata).ToRequestedFields());  // ToRequestedFields() should not be required if TableType like CardTypes pass IshTypeFieldSetup
                string xmlIshEvents;
                if (_retrievedIshEvents.Count == 0)
                {
                    WriteVerbose("Retrieving overview");
                    var progressLevelRequestedMetadata = requestedMetadata.ToRequestedFields(Enumerations.Level.Progress);
                    WriteDebug($"Retrieving ProgressStatusFilter[{_progressStatusFilter}] UserFilter[{_userFilter}] RequestedMetadata.length[{progressLevelRequestedMetadata.ToXml().Length}]");
                    xmlIshEvents = IshSession.BackgroundTask25.RetrieveEventOverview(EventTypes, _progressStatusFilter, ModifiedSince, _userFilter, progressLevelRequestedMetadata.ToXml());
                    _retrievedIshEvents = new IshEvents(xmlIshEvents).Events;
                }

                // if there is a filter RetrieveEventsByProgressIds after RetrieveEventOverview or on incoming IShEvents
                var progressRefs = _retrievedIshEvents.Select(ishEvent => Convert.ToInt64(ishEvent.ObjectRef[Enumerations.ReferenceType.EventProgress])).ToList();
                if (progressRefs.Count != 0)
                {
                    WriteVerbose("Retrieving details");
                    IshFields metadataFilter = new IshFields(MetadataFilter);
                    //TODO: [Could]  could become the highest ishdetailref value to retrieve less, but then you need to 'append' to the incoming IshEvents so low priority
                    long lastDetailId = 0;
                    //TODO: [Could] requestedMetadata can be all levels here
                    xmlIshEvents = IshSession.BackgroundTask25.RetrieveEventsByProgressIds(progressRefs.ToArray(), _eventLevel, lastDetailId, metadataFilter.ToXml(), requestedMetadata.ToXml());
                    _retrievedIshEvents = new IshEvents(xmlIshEvents).Events;
                }
                WriteVerbose("returned event count[" + _retrievedIshEvents.Count + "]");

                //Every cmdlet should return as a promoted PSObject which allows up-to-date PSNoteProperty
                WriteObject(WrapAsPSObjectAndAddNoteProperties(_retrievedIshEvents), true);
                */
            }
            catch (TrisoftAutomationException trisoftAutomationException)
            {
                ThrowTerminatingError(new ErrorRecord(trisoftAutomationException, base.GetType().Name, ErrorCategory.InvalidOperation, null));
            }
            catch (Exception exception)
            {
                ThrowTerminatingError(new ErrorRecord(exception, base.GetType().Name, ErrorCategory.NotSpecified, null));
            }
            finally
            {
                base.EndProcessing();
            }
        }
    }
}

