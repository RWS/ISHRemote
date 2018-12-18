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
    /// <para type="synopsis">The Remove-IshBaselineItem cmdlet removes the baseline entries of baselines that are passed through the pipeline or determined via provided parameters</para>
    /// <para type="description">The Remove-IshBaselineItem cmdlet removes the baseline entries of baselines that are passed through the pipeline or determined via provided parameters</para>
    /// </summary>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/ISHWS/" -PSCredential "Admin"
    /// $baselineIds = @("GUID-17443161-9CAD-4A9A-A3D3-F2942EDB0534","GUID-F1361489-66F3-4E27-A5D1-71C97025815A")
    /// $ishObjects = Get-IshBaseline -IshSession $ishSession -Id $baselineIds
    /// Remove-IshBaselineItem -IshSession $ishSession -IshObject $ishObjects -LogicalId "GUID-12345678-ABCD-EFGH-IJKL-1234567890AB"
    /// </code>
    /// <para>Removes the LogicalId as baseline entry for the identified baselines</para>
    /// </example>
    [Cmdlet(VerbsCommon.Remove, "IshBaselineItem", SupportsShouldProcess = false)]
    [OutputType(typeof(IshBaseline))]
    public sealed class RemoveIshBaselineItem : BaselineCmdlet
    {
        /// <summary>
        /// <para type="description">The IshSession variable holds the authentication and contract information. This object can be initialized using the New-IshSession cmdlet.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshBaselineItemsGroup")]
        [ValidateNotNullOrEmpty]
        public IshSession IshSession { get; set; }

        /// <summary>
        /// <para type="description">Array with the baselines for which to remove the baseline entries. This array can be passed through the pipeline or explicitly passed via the parameter.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshBaselineItemsGroup")]
        public IshObject[] IshObject { get; set; }

        /// <summary>
        /// <para type="description">The logical identifier of the content object</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup"), ValidateNotNullOrEmpty]
        public string LogicalId { get; set; }

        // TODO [Could] IshBaselineItem pipeline. Prefer to create IshBaselineItem as pipeline entries to update baselines (over IshObject parameter), then update one baseline entry in multiple baselines (so pipeline IshObject parameter)
        /// <summary>
        /// <para type="description">Array with the baselines for which to retrieve the baseline entries. This array can be passed through the pipeline or explicitly passed via the parameter.</para>
        /// </summary>
        //[Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "IshBaselineItemsGroup")]
        //[AllowEmptyCollection]
        //public IshBaselineItem[] IshBaselineItem { get; set; }

        private Dictionary<string, List<IshBaselineItem>> _baselineItemsToProcess = new Dictionary<string, List<Objects.Public.IshBaselineItem>>();

        protected override void BeginProcessing()
        {
            if (IshSession == null) { IshSession = (IshSession)SessionState.PSVariable.GetValue(ISHRemoteSessionStateIshSession); }
            if (IshSession == null) { throw new ArgumentNullException(ISHRemoteSessionStateIshSessionException); }
            WriteDebug($"Using IshSession[{IshSession.Name}] from SessionState.{ISHRemoteSessionStateIshSession}");
            base.BeginProcessing();
        }

        protected override void ProcessRecord()
        {
            // Aggregate incoming IshBaselineItem, to allow group Baseline25.Update call
            // ... or add single new IshBaselineItem for every incoming baseline
            if (IshObject != null && IshObject.Length == 0)
            {
                // Do nothing
                WriteVerbose("IshObject is empty, so nothing to do");
            }
            else
            {
                foreach (var ishObject in IshObject)
                {
                    if (!_baselineItemsToProcess.ContainsKey(ishObject.IshRef))
                    {
                        _baselineItemsToProcess.Add(ishObject.IshRef, new List<Objects.Public.IshBaselineItem>());
                    }
                    _baselineItemsToProcess[ishObject.IshRef].Add(new IshBaselineItem(ishObject.IshRef, LogicalId, "999999"));
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
                        StringWriter stringWriter;
                        using (stringWriter = new StringWriter())
                        {
                            using (XmlWriter xmlWriter = XmlWriter.Create(stringWriter))
                            {
                                xmlWriter.WriteStartElement("modifications");
                                foreach (var ishBaselineItem in _baselineItemsToProcess[baselineId])
                                {
                                    xmlWriter.WriteStartElement("object");
                                    xmlWriter.WriteAttributeString("ref", ishBaselineItem.LogicalId);
                                    xmlWriter.WriteAttributeString("action", "delete");
                                    xmlWriter.WriteAttributeString("versionnumber", ishBaselineItem.Version);
                                    xmlWriter.WriteAttributeString("source", "Manual");
                                    xmlWriter.WriteEndElement();
                                }
                                xmlWriter.WriteEndElement();
                            }
                        }
                        IshSession.Baseline25.Update(baselineId, stringWriter.ToString());
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
