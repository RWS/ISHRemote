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
    /// <para type="description">Uses BackgroundTask25 API to retrieve backgroundtasks showing their status, lease, etc from the virtual queue.</para>
    /// <para type="description">This table oriented API maps straight through to database column names regarding ishfield usage.</para>
    /// </summary>
    /// <example>
    /// <code>
    /// $allMetadata = Set-IshRequestedMetadataField -IshSession $ishSession -Level Task -Name CREATIONDATE  | 
    ///                Set-IshRequestedMetadataField -IshSession $ishSession -Level Task -Name CURRENTATTEMPT |
    ///                Set-IshRequestedMetadataField -IshSession $ishSession -Level Task -Name EVENTTYPE |
    ///                Set-IshRequestedMetadataField -IshSession $ishSession -Level Task -Name EXECUTEAFTERDATE |
    ///                Set-IshRequestedMetadataField -IshSession $ishSession -Level Task -Name HASHID | 
    ///                Set-IshRequestedMetadataField -IshSession $ishSession -Level Task -Name INPUTDATAID |
    ///                Set-IshRequestedMetadataField -IshSession $ishSession -Level Task -Name LEASEDBY |
    ///                Set-IshRequestedMetadataField -IshSession $ishSession -Level Task -Name LEASEDON |
    ///                Set-IshRequestedMetadataField -IshSession $ishSession -Level Task -Name MODIFICATIONDATE | 
    ///                Set-IshRequestedMetadataField -IshSession $ishSession -Level Task -Name OUTPUTDATAID |
    ///                Set-IshRequestedMetadataField -IshSession $ishSession -Level Task -Name PROGRESSID |
    ///                Set-IshRequestedMetadataField -IshSession $ishSession -Level Task -Name STATUS -ValueType Value |
    ///                Set-IshRequestedMetadataField -IshSession $ishSession -Level Task -Name STATUS -ValueType Element |
    ///                Set-IshRequestedMetadataField -IshSession $ishSession -Level Task -Name TASKID |
    ///                Set-IshRequestedMetadataField -IshSession $ishSession -Level Task -Name TRACKINGID |
    ///                Set-IshRequestedMetadataField -IshSession $ishSession -Level Task -Name USERID -ValueType All |
    ///                Set-IshRequestedMetadataField -IshSession $ishSession -Level History -Name ENDDATE | 
    ///                Set-IshRequestedMetadataField -IshSession $ishSession -Level History -Name ERROR |
    ///                Set-IshRequestedMetadataField -IshSession $ishSession -Level History -Name ERRORNUMBER |
    ///                Set-IshRequestedMetadataField -IshSession $ishSession -Level History -Name EXITCODE |
    ///                Set-IshRequestedMetadataField -IshSession $ishSession -Level History -Name HISTORYID | 
    ///                Set-IshRequestedMetadataField -IshSession $ishSession -Level History -Name HOSTNAME |
    ///                Set-IshRequestedMetadataField -IshSession $ishSession -Level History -Name OUTPUT |
    ///                Set-IshRequestedMetadataField -IshSession $ishSession -Level History -Name STARTDATE
    /// Get-IshBackgroundTask -IshSession $ishSession -RequestedMetadata $allMetadata               
    /// </code>
    /// <para>Returns the full denormalized task/history entries limited to All users and the last 24 hours.</para>
    /// </example>
    /// <example>
    /// <code>
    /// Get-IshBackgroundTask -IshSession $ishSession -ModifiedSince ((Get-Date).AddSeconds(-10)) -UserFilter Current
    /// </code>
    /// <para>Returns the full denormalized task/history entries limited to only Basic fields, the current user and limited to 10 seconds ago of activity.</para>
    /// </example>
    /// <example>
    /// <code>
    /// $filterMetadata = Set-IshMetadataFilterField -IshSession $ishSession -Level Task -Name EVENTTYPE -FilterOperator In -Value "CREATETRANSLATIONS, CREATETRANSLATIONFROMLIST" |
    ///                   Set-IshMetadataFilterField -IshSession $ishSession -Level Task -Name TASKID -Value $taskId
    /// Get-IshBackgroundTask -IshSession $ishSession -MetadataFilter $filterMetadata
    /// </code>
    /// <para>Returns the full denormalized task/history entries limited to only Basic fields, for All users and the last 24 hours.</para>
    /// </example>
    /// <example>
    /// <code>
    /// $metadata = Set-IshRequestedMetadataField -IshSession $ishSession -Level Task -Name STATUS
    /// Get-IshBackgroundTask -IshSession $ishSession -RequestedMetadata $metadata | Group-Object -Property status
    /// </code>
    /// <para>Returns the group-by count by status, for All users and the last 24 hours.</para>
    /// </example>
    [Cmdlet(VerbsCommon.Get, "IshBackgroundTask", SupportsShouldProcess = false)]
    [OutputType(typeof(IshBackgroundTask))]
    public sealed class GetIshBackgroundTask : BackgroundTaskCmdlet
    {

        /// <summary>
        /// <para type="description">The IshSession variable holds the authentication and contract information. This object can be initialized using the New-IshSession cmdlet.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshBackgroundTasksGroup")]
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
        /// <para type="description">The <see cref="IshBackgroundTask"/>s that need to be handled.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "IshBackgroundTasksGroup")]
        public IshBackgroundTask[] IshBackgroundTask { get; set; }

        

        #region Private fields
        private DateTime _modifiedSince = DateTime.Today.AddDays(-1);
        //private BackgroundTask25ServiceReference.B _progressStatusFilter = BackgroundTask25ServiceReference.ProgressStatusFilter.All;
        private BackgroundTask25ServiceReference.eUserFilter _userFilter = BackgroundTask25ServiceReference.eUserFilter.All;
        private readonly List<IshBackgroundTask> _retrievedIshBackgroundTask = new List<IshBackgroundTask>();
        #endregion

        protected override void BeginProcessing()
        {
            if (IshSession == null) { IshSession = (IshSession)SessionState.PSVariable.GetValue(ISHRemoteSessionStateIshSession); }
            if (IshSession == null) { throw new ArgumentNullException(ISHRemoteSessionStateIshSessionException); }
            WriteDebug($"Using IshSession[{IshSession.Name}] from SessionState.{ISHRemoteSessionStateIshSession}");

            if ((IshSession.ServerIshVersion.MajorVersion < 13) || ((IshSession.ServerIshVersion.MajorVersion == 13) && (IshSession.ServerIshVersion.RevisionVersion < 2)))
            {
                throw new PlatformNotSupportedException($"Get-IshBackgroundTask requires server-side BackgroundTask API which only available starting from 13SP2/13.0.2 and up. ServerIshVersion[{IshSession.ServerVersion}]");
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
                IshFields requestedMetadata = IshSession.IshTypeFieldSetup.ToIshRequestedMetadataFields(IshSession.DefaultRequestedMetadata, ISHType, new IshFields(RequestedMetadata), Enumerations.ActionMode.Find);
                string xmlIshBackgroundTasks;
                if (_retrievedIshBackgroundTask.Count != 0)
                {
                    var backgroundTaskIds = _retrievedIshBackgroundTask.Select(ishEvent => Convert.ToInt64(ishEvent.ObjectRef[Enumerations.ReferenceType.BackgroundTask])).ToList();
                    if (backgroundTaskIds.Count != 0)
                    {
                        var backgroundTaskIdsAsString = string.Join(", ", backgroundTaskIds.ToArray());
                        metadataFilter.AddOrUpdateField(new IshMetadataFilterField("TASKID", Enumerations.Level.Task, Enumerations.FilterOperator.In, backgroundTaskIdsAsString, Enumerations.ValueType.Value), Enumerations.ActionMode.Find);
                    }
                }
                WriteDebug($"Finding UserFilter[{_userFilter}] MetadataFilter.length[{metadataFilter.ToXml().Length}] RequestedMetadata.length[{requestedMetadata.ToXml().Length}]");
                xmlIshBackgroundTasks = IshSession.BackgroundTask25.Find(
                    ModifiedSince,
                    _userFilter,
                    metadataFilter.ToXml(),
                    requestedMetadata.ToXml());
                List<IshBackgroundTask> returnIshBackgroundTasks = new IshBackgroundTasks(xmlIshBackgroundTasks).BackgroundTasks;
                WriteVerbose("returned object count[" + returnIshBackgroundTasks.Count + "]");
                
                switch (IshSession.PipelineObjectPreference)
                {
                    case Enumerations.PipelineObjectPreference.PSObjectNoteProperty:
                        WriteObject(WrapAsPSObjectAndAddNoteProperties(IshSession, returnIshBackgroundTasks), true);
                        break;
                    case Enumerations.PipelineObjectPreference.Off:
                        WriteObject(returnIshBackgroundTasks, true);
                        break;
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
            finally
            {
                base.EndProcessing();
            }
        }
    }
}

