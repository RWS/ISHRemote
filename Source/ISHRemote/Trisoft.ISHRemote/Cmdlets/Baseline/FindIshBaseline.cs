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

namespace Trisoft.ISHRemote.Cmdlets.Baseline
{
    /// <summary>
    /// <para type="synopsis">The Find-IshBaseline cmdlet finds baselines using MetadataFilter that are provided.</para>
    /// <para type="description">The Find-IshBaseline cmdlet finds baselines using MetadataFilter that are provided.</para>
    /// </summary>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/ISHWS/" -PSCredential "Admin"
    /// Find-IshBaseline -IshSession $ishSession -RequestedMetadata (Set-IshRequestedMetadataField -IshSession $ishSession -Name "NAME")
    /// </code>
    /// <para>Retrieve all baselines</para>
    /// </example>
    /// <example>
    /// <code>
    /// New-IshSession -WsBaseUrl "https://example.com/ISHWS/" -PSCredential "Admin"
    /// Find-IshBaseline -MetadataFilter (Set-IshMetadataFilterField -IshSession $ishSession -Name "FISHBASELINEACTIVE" -ValueType Element -Value "FALSE")
    /// </code>
    /// <para>New-IshSession will submit into SessionState, so it can be reused by this cmdlet. Retrieve all active baselines with Basic metadata.</para>
    /// </example>
    [Cmdlet(VerbsCommon.Find, "IshBaseline", SupportsShouldProcess = false)]
    [OutputType(typeof(IshBaseline))]
    public sealed class FindIshBaseline : BaselineCmdlet
    {
        /// <summary>
        /// <para type="description">The IshSession variable holds the authentication and contract information. This object can be initialized using the New-IshSession cmdlet.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false)]
        [ValidateNotNullOrEmpty]
        public IshSession IshSession { get; set; }

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

        protected override void BeginProcessing()
        {
            if (IshSession == null) { IshSession = (IshSession)SessionState.PSVariable.GetValue(ISHRemoteSessionStateIshSession); }
            if (IshSession == null) { throw new ArgumentNullException(ISHRemoteSessionStateIshSessionException); }
            WriteDebug($"Using IshSession[{IshSession.Name}] from SessionState.{ISHRemoteSessionStateIshSession}");
            base.BeginProcessing();
            // Working code, but not required for Baseline object type
            //if (MetadataFilter == null)
            //{
            //    var fieldName = "FISHMODIFIEDON";
            //    var dateTime = DateTime.Today.AddDays(-1).ToString("dd/MM/yyyy HH:mm:ss");
            //    var metadataFilter = new IshMetadataFilterField(fieldName, Enumerations.Level.None, Enumerations.FilterOperator.GreaterThanOrEqual, dateTime, Enumerations.ValueType.Value);
            //    WriteVerbose($"Filtering to 1 day using -MetadataFilter {metadataFilter}");
            //    MetadataFilter = new IshFields().AddField(metadataFilter).Fields();
            //}
        }

        protected override void ProcessRecord()
        {
            try
            {
                IshFields metadataFilter = new IshFields(MetadataFilter);
                IshFields requestedMetadata = IshSession.IshTypeFieldSetup.ToIshRequestedMetadataFields(IshSession.DefaultRequestedMetadata, ISHType, new IshFields(RequestedMetadata), Enumerations.ActionMode.Find);

                WriteDebug($"Finding MetadataFilter.length[{metadataFilter.ToXml().Length}] RequestedMetadata.length[{requestedMetadata.ToXml().Length}]");
                string xmlIshObjects = IshSession.Baseline25.GetList(
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
