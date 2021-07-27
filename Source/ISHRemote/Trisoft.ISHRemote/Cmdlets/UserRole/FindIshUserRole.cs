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
using System.Linq;
using System.Management.Automation;
using Trisoft.ISHRemote.Objects;
using Trisoft.ISHRemote.Objects.Public;
using Trisoft.ISHRemote.Exceptions;
using Trisoft.ISHRemote.HelperClasses;

namespace Trisoft.ISHRemote.Cmdlets.UserRole
{
    /// <summary>
    /// <para type="synopsis">The Find-IshUserRole cmdlet finds user roles using ActivityFilter and MetadataFilter that are provided.</para>
    /// <para type="description">The Find-IshUserRole cmdlet finds user roles using ActivityFilter and MetadataFilter that are provided.</para>
    /// </summary>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/InfoShareWS/" -IshUserName "username" -IshUserPassword  "userpassword"
    /// Find-IshUserRole
    /// </code>
    /// <para>New-IshSession will submit into SessionState, so it can be reused by this cmdlet. Retrieve all user roles with their Basic metadata.</para>
    /// </example>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/InfoShareWS/" -IshUserName "username" -IshUserPassword  "userpassword"
    /// Find-IshUserRole -IshSession $ishSession -RequestedMetadata (Set-IshRequestedMetadataField -IshSession $ishSession -Name "FISHUSERROLENAME")
    /// </code>
    /// <para>New-IshSession will submit into SessionState, so it can be reused by this cmdlet. Retrieve one specific field for all user roles.</para>
    /// </example>
    [Cmdlet(VerbsCommon.Find, "IshUserRole", SupportsShouldProcess = false)]
    [OutputType(typeof(IshUserRole))]
    public sealed class FindIshUserRole : UserRoleCmdlet
    {
        /// <summary>
        /// <para type="description">The IshSession variable holds the authentication and contract information. This object can be initialized using the New-IshSession cmdlet.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false), ValidateNotNullOrEmpty]
        public IshSession IshSession { get; set; }

        /// <summary>
        /// <para type="description">The activity filter to limit the amount of objects returned. Default is no filtering.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false), ValidateNotNullOrEmpty]
        public Enumerations.ActivityFilter ActivityFilter
        {
            get { return _activityFilter; }
            set { _activityFilter = value; }
        }

        /// <summary>
        /// <para type="description">The metadata filter with the filter fields to limit the amount of objects returned. Default is no filtering.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false), ValidateNotNull]
        public IshField[] MetadataFilter { get; set; }

        /// <summary>
        /// <para type="description">The metadata fields to retrieve</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false), ValidateNotNull]
        public IshField[] RequestedMetadata { get; set; }


        #region Private fields
        /// <summary>
        /// Private field to store the IshType and provide a default for non-mandatory parameters
        /// </summary>
        private Enumerations.ActivityFilter _activityFilter = Enumerations.ActivityFilter.None;
        #endregion

        protected override void BeginProcessing()
        {
            if (IshSession == null) { IshSession = (IshSession)SessionState.PSVariable.GetValue(ISHRemoteSessionStateIshSession); }
            if (IshSession == null) { throw new ArgumentException(ISHRemoteSessionStateIshSessionException); }
            WriteDebug($"Using IshSession[{IshSession.Name}] from SessionState.{ISHRemoteSessionStateIshSession}");
            base.BeginProcessing();
        }


        protected override void ProcessRecord()
        {
            try
            {
                IshFields metadataFilter = new IshFields(MetadataFilter);
                IshFields requestedMetadata = IshSession.IshTypeFieldSetup.ToIshRequestedMetadataFields(IshSession.DefaultRequestedMetadata, ISHType, new IshFields(RequestedMetadata), Enumerations.ActionMode.Find);
                var activityFilter = EnumConverter.ToActivityFilter<UserRole25ServiceReference.ActivityFilter>(ActivityFilter);

                WriteDebug($"Finding ActivityFilter[{activityFilter}] MetadataFilter.length[{metadataFilter.ToXml().Length}] RequestedMetadata.length[{requestedMetadata.ToXml().Length}]");
                string xmlIshObjects = IshSession.UserRole25.Find(
                    activityFilter,
                    metadataFilter.ToXml(),
                    requestedMetadata.ToXml());

                var returnedObjects = new IshObjects(ISHType, xmlIshObjects).ObjectList;
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
