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
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using Trisoft.ISHRemote.Objects;
using Trisoft.ISHRemote.Objects.Public;
using Trisoft.ISHRemote.Exceptions;
using Trisoft.ISHRemote.HelperClasses;

namespace Trisoft.ISHRemote.Cmdlets.Baseline
{
    /// <summary>
    /// <para type="synopsis">The Get-IshBaseline cmdlet retrieves the metadat of baselines that are passed through the pipeline or determined via provided parameters</para>
    /// <para type="description">The Get-IshBaseline cmdlet retrieves the metadat of baselines that are passed through the pipeline or determined via provided parameters</para>
    /// </summary>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/ISHWS/" -PSCredential "Admin"
    /// $baselineIds = @("GUID-17443161-9CAD-4A9A-A3D3-F2942EDB0534","GUID-F1361489-66F3-4E27-A5D1-71C97025815A")
    /// Get-IshBaseline -Id $baselineIds -RequestedMetadata (Set-IshRequestedMetadataField -IshSession $ishSession -Name "FISHDOCUMENTRELEASE")
    /// </code>
    /// <para>New-IshSession will submit into SessionState, so it can be reused by this cmdlet. Retrieve metadata from the identified baselines</para>
    /// </example>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/ISHWS/" -PSCredential "Admin"
    /// $baselineIds = @("GUID-17443161-9CAD-4A9A-A3D3-F2942EDB0534","GUID-F1361489-66F3-4E27-A5D1-71C97025815A")
    /// Get-IshBaseline -IshSesssion $ishSession -Id $baselineIds -RequestedMetadata (Set-IshRequestedMetadataField -IshSession $ishSession -Name "FISHDOCUMENTRELEASE")
    /// </code>
    /// <para>Retrieve metadata from the identified baselines</para>
    /// </example>
    [Cmdlet(VerbsCommon.Get, "IshBaseline", SupportsShouldProcess = false)]
    [OutputType(typeof(IshBaseline))]
    public sealed class GetIshBaseline : BaselineCmdlet
    {
        /// <summary>
        /// <para type="description">The IshSession variable holds the authentication and contract information. This object can be initialized using the New-IshSession cmdlet.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshObjectsGroup")]
        [ValidateNotNullOrEmpty]
        public IshSession IshSession { get; set; }

        /// <summary>
        /// <para type="description">The baseline identifiers for which to retrieve the metadata</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup"), ValidateNotNullOrEmpty]
        public string[] Id { get; set; }

        /// <summary>
        /// <para type="description">The metadata filter with the filter fields to limit the amount of objects returned. Default is no filtering.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshObjectsGroup")]
        [ValidateNotNull]
        public IshField[] MetadataFilter { get; set; }

        /// <summary>
        /// <para type="description">The metadata fields to retrieve</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshObjectsGroup")]
        [ValidateNotNull]
        public IshField[] RequestedMetadata { get; set; }

        /// <summary>
        /// <para type="description">Array with the baselines for which to retrieve the metadata. This array can be passed through the pipeline or explicitly passed via the parameter.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "IshObjectsGroup")]
        [AllowEmptyCollection]
        public IshObject[] IshObject { get; set; }

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
                List<IshObject> returnedObjects = new List<IshObject>();

                if (IshObject != null && IshObject.Length == 0)
                {
                    // Do nothing
                    WriteVerbose("IshObject is empty, so nothing to retrieve");
                }
                else
                {
                    IshFields metadataFilter = new IshFields(MetadataFilter);
                    IshFields requestedMetadata = IshSession.IshTypeFieldSetup.ToIshRequestedMetadataFields(IshSession.DefaultRequestedMetadata, ISHType, new IshFields(RequestedMetadata), Enumerations.ActionMode.Read);
                    var ids = (IshObject != null) ? new IshObjects(IshObject).Ids : Id;
                    WriteDebug($"Retrieving Id.length[{ids.Length}] MetadataFilter.length[{metadataFilter.ToXml().Length}] RequestedMetadata.length[{requestedMetadata.ToXml().Length}]");
                    string xmlIshObjects = IshSession.Baseline25.RetrieveMetadata(
                        ids,
                        metadataFilter.ToXml(),
                        requestedMetadata.ToXml());

                    returnedObjects.AddRange(new IshObjects(ISHType, xmlIshObjects).Objects);
                }

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
