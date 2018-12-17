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

namespace Trisoft.ISHRemote.Cmdlets.DocumentObj
{
    /// <summary>
    /// <para type="synopsis">The Find-IshDocumentObj cmdlet finds document objects (which include illustrations) using MetadataFilter, TypeFilter and StatusFilter that are provided This commandlet allows to find all types of objects (Illustrations, Maps, etc. ), except for publication (outputs). 
    /// For publication (outputs) you need to use Find-IshPublicationOutput.</para>
    /// <para type="description">The Find-IshDocumentObj cmdlet finds document objects (which include illustrations) using MetadataFilter, TypeFilter and StatusFilter that are provided This commandlet allows to find all types of objects (Illustrations, Maps, etc. ), except for publication (outputs). 
    /// For publication (outputs) you need to use Find-IshPublicationOutput.</para>
    /// </summary>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/InfoShareWS/" -PSCredential Admin
    /// $yesterday = (Get-Date).AddDays(-1).ToString("dd/MM/yyyy HH:mm:ss")
    /// Find-IshDocumentObj -MetadataFilter(Set-IshMetadataFilterField -Level Lng -Name MODIFIED-ON -FilterOperator GreaterThan -Value $yesterday)
    /// </code>
    /// <para>New-IshSession will submit into SessionState, so it can be reused by this cmdlet. Returns the documents that are touched since yesterday.</para>
    /// </example>
    /// <example>
    /// <code>
    /// $yesterday = (Get-Date).AddDays(-2000).ToString("dd/MM/yyyy HH:mm:ss")
    /// $ishObjects = Find-IshDocumentObj -MetadataFilter(Set-IshMetadataFilterField -Level Lng -Name MODIFIED-ON -FilterOperator GreaterThan -Value $yesterday) `
    ///                                   -RequestedMetadata(Set-IshRequestedMetadataField -Level Lng -Name FISHLASTMODIFIEDON)
    /// $ishObjects | Select-Object -Property IshType, IshRef, version_version_value, doc-language, fresolution, fishlastmodifiedon, fishlastmodifiedby, fstatus, checked-out-by, ftitle_logical_value | Format-Table
    /// </code>
    /// <para></para>
    /// </example>
    [Cmdlet(VerbsCommon.Find, "IshDocumentObj", SupportsShouldProcess = false)]
    [OutputType(typeof(IshDocumentObj))]
    public sealed class FindIshDocumentObj : DocumentObjCmdlet
    {

        /// <summary>
        /// <para type="description">The IshSession variable holds the authentication and contract information. This object can be initialized using the New-IshSession cmdlet.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false), ValidateNotNullOrEmpty]
        public IshSession IshSession { get; set; }

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

        /// <summary>
        /// <para type="description">The folder type filter to limit the amount of objects returned. Default is no filtering.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false), ValidateNotNullOrEmpty]
        public Enumerations.ISHType[] IshTypeFilter { get; set; }

        /// <summary>
        /// <para type="description">The status type filter to limit the amount of objects returned. Default is no filtering.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false), ValidateNotNullOrEmpty]
        public Enumerations.StatusFilter StatusFilter
        {
            get { return _statusFilter; }
            set { _statusFilter = value; }
        }


        #region Private fields

        /// <summary>
        /// Private field to store the IshType and provide a default for non-mandatory parameters
        /// </summary>
        private Enumerations.StatusFilter _statusFilter = Enumerations.StatusFilter.ISHNoStatusFilter;
        #endregion

        protected override void BeginProcessing()
        {
            if (IshSession == null) { IshSession = (IshSession)SessionState.PSVariable.GetValue(ISHRemoteSessionStateIshSession); }
            if (IshSession == null) { throw new ArgumentNullException(ISHRemoteSessionStateIshSessionException); }
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
        /// Process the Find-IshDocumentObj commandlet.
        /// </summary>
        /// <exception cref="TrisoftAutomationException"></exception>
        /// <exception cref="Exception"></exception>
        /// <remarks>Writes an <see cref="IshObject"/> array to the pipeline.</remarks>
        protected override void ProcessRecord()
        {

            try
            {
                IshFields metadataFilter = new IshFields(MetadataFilter);
                IshFields requestedMetadata = IshSession.IshTypeFieldSetup.ToIshRequestedMetadataFields(IshSession.DefaultRequestedMetadata, ISHType, new IshFields(RequestedMetadata), Enumerations.ActionMode.Find);
                string ishTypeFilter = (IshTypeFilter != null) ? String.Join(IshSession.Seperator, IshTypeFilter) : "";
                var statusFilter = EnumConverter.ToStatusFilter<DocumentObj25ServiceReference.StatusFilter>(StatusFilter);

                // Finding any hits with extra requested metadata if specified.
                // Note that is better to do a Find and pipe it to a Get which will retrieve additional metadata in batches
                WriteDebug($"Finding StatusFilter[{statusFilter}] MetadataFilter.length[{metadataFilter.ToXml().Length}] RequestedMetadata.length[{requestedMetadata.ToXml().Length}]");
                string xmlIshObjects = IshSession.DocumentObj25.Find(
                    ishTypeFilter,
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
