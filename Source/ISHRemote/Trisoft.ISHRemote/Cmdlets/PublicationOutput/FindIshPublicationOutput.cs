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
using System.Text;
using System.Management.Automation;
using Trisoft.ISHRemote.Objects;
using Trisoft.ISHRemote.Objects.Public;
using Trisoft.ISHRemote.Exceptions;
using Trisoft.ISHRemote.HelperClasses;

namespace Trisoft.ISHRemote.Cmdlets.PublicationOutput
{
    /// <summary>
    /// <para type="synopsis">The Find-IshPublicationOutput cmdlet finds all publication outputs using the StatusFilter and MetadataFilter that are provided.</para>
    /// <para type="description">The Find-IshPublicationOutput cmdlet finds all publication outputs using the StatusFilter and MetadataFilter that are provided.</para>
    /// </summary>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/InfoShareWS/" -IshUserName "username" -IshUserPassword  "userpassword"
    /// $requestedMetadataRetrieve = Set-IshRequestedMetadataField -IshSession $ishSession -Name 'FISHOUTPUTFORMATREF' -Level "Lng" |
    ///                              Set-IshRequestedMetadataField -IshSession $ishSession -Name 'FISHPUBLNGCOMBINATION' -Level "Lng" |
    ///                              Set-IshRequestedMetadataField -IshSession $ishSession -Name 'FISHPUBSTATUS' -Level "Lng" |
    ///                              Set-IshRequestedMetadataField -IshSession $ishSession -Name 'FISHPUBLISHER' -Level "Lng" |
    ///                              Set-IshRequestedMetadataField -IshSession $ishSession -Name 'FISHPUBSTARTDATE' -Level "Lng" |
    ///                              Set-IshRequestedMetadataField -IshSession $ishSession -Name 'FISHPUBENDDATE' -Level "Lng" |
    ///                              Set-IshRequestedMetadataField -IshSession $ishSession -Name 'VERSION' -Level "Version"
    /// $metadataFilterRetrieve = Set-IshMetadataFilterField -IshSession $ishSession -Name 'FISHPUBSTATUS' -Level 'Lng' -ValueType "Value" -FilterOperator "Equal" -Value "To Be Published" 
    /// $publicationOutputs = Find-IshPublicationOutput -StatusFilter "ishnostatusfilter" -MetadataFilter $metadataFilterRetrieve -RequestedMetadata $requestedMetadataRetrieve
    /// </code>
    /// <para>New-IshSession will submit into SessionState, so it can be reused by this cmdlet. Finding publication outputs</para>
    /// </example>
    [Cmdlet(VerbsCommon.Find, "IshPublicationOutput", SupportsShouldProcess = false)]
    [OutputType(typeof(IshPublicationOutput))]
    public sealed class FindIshPublicationOutput : PublicationOutputCmdlet
    {

        /// <summary>
        /// <para type="description">The IshSession variable holds the authentication and contract information. This object can be initialized using the New-IshSession cmdlet.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false), ValidateNotNullOrEmpty]
        public IshSession IshSession { get; set; }

        /// <summary>
        /// <para type="description">The status filter to limit the amount of objects returned. Default is no filtering.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false), ValidateNotNullOrEmpty]
        public Enumerations.StatusFilter StatusFilter
        {
            get { return _statusFilter; }
            set { _statusFilter = value; }
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
        private Enumerations.StatusFilter _statusFilter = Enumerations.StatusFilter.ISHNoStatusFilter;
        #endregion

        protected override void BeginProcessing()
        {
            if (IshSession == null) { IshSession = (IshSession)SessionState.PSVariable.GetValue(ISHRemoteSessionStateIshSession); }
            if (IshSession == null) { throw new ArgumentException(ISHRemoteSessionStateIshSessionException); }
            WriteDebug($"Using IshSession[{IshSession.Name}] from SessionState.{ISHRemoteSessionStateIshSession}");
            base.BeginProcessing();
            // Working code, but breaks backward behavior compatibility in projected 0.7/1.0 release, see #49
            // if (MetadataFilter == null)
            // {
            //     var fieldName = "MODIFIED-ON";
            //     var dateTime = DateTime.Today.AddDays(-1).ToString("dd/MM/yyyy HH:mm:ss");
            //     var metadataFilter = new IshMetadataFilterField(fieldName, Enumerations.Level.Lng, Enumerations.FilterOperator.GreaterThanOrEqual, dateTime, Enumerations.ValueType.Value);
            //     WriteVerbose($"Filtering to 1 day using -MetadataFilter {metadataFilter}");
            //     MetadataFilter = new IshFields().AddField(metadataFilter).Fields();
            // }
        }


        /// <summary>
        /// Process the Find-IshPublicationOutput commandlet.
        /// </summary>
        /// <exception cref="TrisoftAutomationException"></exception>
        /// <exception cref="Exception"></exception>
        /// <remarks>Writes an <see cref="IshObject"/> array to the pipeline.</remarks>
        protected override void ProcessRecord()
        {
            try
            {             
                string xmlIshObjects = "";
                PublicationOutput25ServiceReference.StatusFilter statusFilter = EnumConverter.ToStatusFilter<PublicationOutput25ServiceReference.StatusFilter>(StatusFilter);
                IshFields metadataFilter = new IshFields(MetadataFilter);
                // Add the required fields (needed for pipe operations)
                IshFields requestedMetadata = IshSession.IshTypeFieldSetup.ToIshRequestedMetadataFields(IshSession.DefaultRequestedMetadata, ISHType, new IshFields(RequestedMetadata), Enumerations.ActionMode.Read);
                WriteDebug($"Finding StatusFilter[{statusFilter}] MetadataFilter.length[{metadataFilter.ToXml().Length}] RequestedMetadata.length[{requestedMetadata.ToXml().Length}]");
                xmlIshObjects = IshSession.PublicationOutput25.Find(
                    statusFilter, 
                    metadataFilter.ToXml(), 
                    requestedMetadata.ToXml());
                var returnIshObjects = new IshObjects(ISHType, xmlIshObjects);

                WriteVerbose("returned object count[" + returnIshObjects.ObjectList.Count + "]");
                WriteObject(IshSession, ISHType, returnIshObjects.ObjectList.ConvertAll(x => (IshBaseObject)x), true);
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
