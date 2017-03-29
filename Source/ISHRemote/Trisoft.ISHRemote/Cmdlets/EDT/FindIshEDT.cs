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
using System.Linq;
using System.Management.Automation;
using Trisoft.ISHRemote.Objects;
using Trisoft.ISHRemote.Objects.Public;
using Trisoft.ISHRemote.Exceptions;
using Trisoft.ISHRemote.HelperClasses;

namespace Trisoft.ISHRemote.Cmdlets.EDT
{
    /// <summary>
    /// <para type="synopsis">The Find-IshEDT cmdlet finds EDT objects using ActivityFilter and MetadataFilter that are provided</para>
    /// <para type="description">The Find-IshEDT cmdlet finds EDT objects using ActivityFilter and MetadataFilter that are provided</para>
    /// </summary>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/InfoShareWS/" -IshUserName "username" -IshUserPassword  "userpassword"
    /// $ishMetadataFilterFields = Set-IshMetadataFilterField -IshSession $ishSession -Name "EDT-FILE-EXTENSION" -Level "None" -FilterOperator "Like" -Value "%xml%"
    /// $ishRequestedFields = Set-IshRequestedMetadataField -IshSession $ishSession -Name "NAME" -Level "None"
    /// $edtList = Find-IshEDT -IshSession $ishSession `
    ///       -ActivityFilter "None" `
    ///       -MetadataFilter $ishMetadataFilterFields `
    ///       -RequestedMetadata $ishRequestedFields
    /// </code>
    /// <para>Find EDTs with names containing "edt" text</para>
    /// </example>
    [Cmdlet(VerbsCommon.Find, "IshEDT", SupportsShouldProcess = false)]
    [OutputType(typeof(IshObject))]
    public sealed class FindIshEDT : EDTCmdlet
    {
        /// <summary>
        /// <para type="description">The IshSession variable holds the authentication and contract information. This object can be initialized using the New-IshSession cmdlet.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false), ValidateNotNullOrEmpty]
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
        /// <para type="description">The metadata filter with the filter fields to limit the amount of objects returned</para>
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
        
        protected override void ProcessRecord()
        {
            try
            {
                // 1. Validating the input
                WriteDebug("Validating");

                var activityFilter = EnumConverter.ToActivityFilter<EDT25ServiceReference.ActivityFilter>(ActivityFilter);
                IshFields metadataFilter = new IshFields(MetadataFilter);
                // add more fields required for pipe operations
                IshFields requestedMetadata = IshSession.IshTypeFieldSetup.ToIshRequestedMetadataFields(ISHType, new IshFields(RequestedMetadata), Enumerations.ActionMode.Find);

                // 2. Finding 
                WriteDebug($"Finding ActivityFilter[{activityFilter}] MetadataFilter.length[{metadataFilter.ToXml().Length}] RequestedMetadata.length[{requestedMetadata.ToXml().Length}]");
                string xmlIshObjects = IshSession.EDT25.Find(
                    activityFilter,
                    metadataFilter.ToXml(),
                    requestedMetadata.ToRequestedFields().ToXml());
                WriteVerbose("xmlIshObjects.length[" + xmlIshObjects.Length + "]");

                // 3. Write it
                var returnedObjects = new IshObjects(xmlIshObjects).Objects;
                WriteVerbose("returned object count[" + returnedObjects.Length + "]");
                WriteObject(returnedObjects, true);
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
