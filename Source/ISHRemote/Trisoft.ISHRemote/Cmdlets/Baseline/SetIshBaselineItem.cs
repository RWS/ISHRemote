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
using System.Management.Automation;
using Trisoft.ISHRemote.Objects;
using Trisoft.ISHRemote.Objects.Public;
using Trisoft.ISHRemote.Exceptions;
using Trisoft.ISHRemote.HelperClasses;
using System.Xml;
using System.IO;

namespace Trisoft.ISHRemote.Cmdlets.Baseline
{
    /// <summary>
    /// <para type="synopsis">The Set-IshBaselineItem cmdlet creates or updates the baseline entries of baselines that are passed through the pipeline or determined via provided parameters</para>
    /// <para type="description">The Set-IshBaselineItem cmdlet creates or updates the baseline entries of baselines that are passed through the pipeline or determined via provided parameters</para>
    /// </summary>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/ISHWS/" -PSCredential "Admin"
    /// $baselineIds = @("GUID-17443161-9CAD-4A9A-A3D3-F2942EDB0534","GUID-F1361489-66F3-4E27-A5D1-71C97025815A")
    /// $ishObjects = Get-IshBaseline -IshSession $ishSession -Id $baselineIds
    /// Set-IshBaselineItem -IshSession $ishSession -IshObject $ishObjects -LogicalId "GUID-12345678-ABCD-EFGH-IJKL-1234567890AB" -Version "999"
    /// </code>
    /// <para>Add or update the version of the baseline entry identified by the given LogicalId for the identified baselines</para>
    /// </example>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/ISHWS/" -PSCredential "Admin"
    /// $ishObjectSource = Get-IshBaseline -Id "GUID-17443161-9CAD-4A9A-A3D3-F2942EDB0534"
    /// $baselineItems = Get-IshBaselineItem -IshObject $ishObjectSource
    /// $ishObjectTarget = Add-IshBaseline -Name "New Baseline"
    /// $ishObjectTarget = Set-IshBaselineItem -IshObject ishObjectTarget -IshBaselineItem $baselineItems
    /// </code>
    /// <para>New-IshSession will submit into SessionState, so it can be reused by this cmdlet. Reading the information from the source baseline and creating a new target baseline which we fill with the selected versions.</para>
    /// </example>
    [Cmdlet(VerbsCommon.Set, "IshBaselineItem", SupportsShouldProcess = false)]
    [OutputType(typeof(IshBaseline))]
    public sealed class SetIshBaselineItem : BaselineCmdlet
    {
        /// <summary>
        /// <para type="description">The IshSession variable holds the authentication and contract information. This object can be initialized using the New-IshSession cmdlet.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshBaselineItemsGroup")]
        [ValidateNotNullOrEmpty]
        public IshSession IshSession { get; set; }

        /// <summary>
        /// <para type="description">Array with the baselines for which to create or update the baseline entries. This array can be passed through the pipeline or explicitly passed via the parameter.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshBaselineItemsGroup")]
        public IshObject[] IshObject { get; set; }

        /// <summary>
        /// <para type="description">The logical identifier of the content object</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup"), ValidateNotNullOrEmpty]
        public string LogicalId { get; set; }

        /// <summary>
        /// <para type="description">The version of the content object</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup"), ValidateNotNullOrEmpty]
        public string Version { get; set; }

        /// <summary>
        /// <para type="description">Array with the baseline entries which will be applied to the IshObject in a minimal number of API calls. This array can be passed through the pipeline or explicitly passed via the parameter.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "IshBaselineItemsGroup")]
        public IshBaselineItem[] IshBaselineItem { get; set; }

        private readonly Dictionary<string, List<IshBaselineItem>> _baselineItemsToProcess = new Dictionary<string, List<Objects.Public.IshBaselineItem>>();

        protected override void BeginProcessing()
        {
            if (IshSession == null) { IshSession = (IshSession)SessionState.PSVariable.GetValue(ISHRemoteSessionStateIshSession); }
            if (IshSession == null) { throw new ArgumentException(ISHRemoteSessionStateIshSessionException); }
            WriteDebug($"Using IshSession[{IshSession.Name}] from SessionState.{ISHRemoteSessionStateIshSession}");
            base.BeginProcessing();
        }

        protected override void ProcessRecord()
        {
            // Aggregate incoming IshBaselineItem, to allow group Baseline25.Update call
            if (IshObject != null && IshObject.Length == 0)
            {
                // Do nothing
                WriteVerbose("IshObject is empty, so nothing to do");
            }
            
            // add baseline item using provided LogicalId and Version parameters
            if (IshObject != null && IshObject.Length != 0 && IshBaselineItem == null)
            {
                foreach (var ishObject in IshObject)
                {
                    if (!_baselineItemsToProcess.ContainsKey(ishObject.IshRef))
                    {
                        _baselineItemsToProcess.Add(ishObject.IshRef, new List<Objects.Public.IshBaselineItem>());
                    }
                    WriteDebug($"Id[{ishObject.IshRef}] Add {LogicalId}={Version}");
                    _baselineItemsToProcess[ishObject.IshRef].Add(new IshBaselineItem(ishObject.IshRef, LogicalId, Version));
                }
            }

            // add baseline item(s) using provided IshBaselineItem[] array to all parameter baseline ishobjects
            if (IshObject != null && IshObject.Length != 0 && IshBaselineItem != null)
            {
                foreach (var ishObject in IshObject)
                {
                    if (!_baselineItemsToProcess.ContainsKey(ishObject.IshRef))
                    {
                        _baselineItemsToProcess.Add(ishObject.IshRef, new List<Objects.Public.IshBaselineItem>());
                    }
                    foreach (var ishBaselineItem in IshBaselineItem)
                    {
                        WriteDebug($"Id[{ishObject.IshRef}] Add {ishBaselineItem.LogicalId}={ishBaselineItem.Version}");
                        _baselineItemsToProcess[ishObject.IshRef].Add(new IshBaselineItem(ishObject.IshRef, ishBaselineItem.LogicalId, ishBaselineItem.Version));
                    }
                }
            }

        }

        protected override void EndProcessing()
        {
            try
            {
                int current = 0;
                foreach (var baselineId in _baselineItemsToProcess.Keys)
                {
                    WriteDebug($"Id[{baselineId}] {++current}/{_baselineItemsToProcess.Keys.Count}");
                    if (ShouldProcess(baselineId))
                    {
                        using (StringWriter stringWriter = new StringWriter())
                        {
                            using (XmlWriter xmlWriter = XmlWriter.Create(stringWriter))
                            {
                                xmlWriter.WriteStartElement("modifications");
                                foreach (var ishBaselineItem in _baselineItemsToProcess[baselineId])
                                {
                                    xmlWriter.WriteStartElement("object");
                                    xmlWriter.WriteAttributeString("ref", ishBaselineItem.LogicalId);
                                    xmlWriter.WriteAttributeString("action", "update");
                                    xmlWriter.WriteAttributeString("versionnumber", ishBaselineItem.Version);
                                    xmlWriter.WriteAttributeString("source", "Manual");
                                    xmlWriter.WriteEndElement();
                                }
                                xmlWriter.WriteEndElement();
                            }
                            IshSession.Baseline25.Update(baselineId, stringWriter.ToString());
                        }
                    }
                }
                WriteObject(IshObject, true);  // Incoming IshObject is not altered, already contains optional PSNoteProperty, so continuing the pipeline
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
