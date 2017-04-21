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

namespace Trisoft.ISHRemote.Cmdlets.Baseline
{
    /// <summary>
    /// <para type="synopsis">The Get-IshBaselineItem cmdlet retrieves the baseline entries of baselines that are passed through the pipeline or determined via provided parameters</para>
    /// <para type="description">The Get-IshBaselineItem cmdlet retrieves the baseline entries of baselines that are passed through the pipeline or determined via provided parameters</para>
    /// </summary>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/ISHWS/" -PSCredential "Admin"
    /// $baselineIds = @("GUID-17443161-9CAD-4A9A-A3D3-F2942EDB0534","GUID-F1361489-66F3-4E27-A5D1-71C97025815A")
    /// Get-IshBaselineItem -IshSession $ishSession -Id $baselineIds
    /// </code>
    /// <para>Returns the baseline entries for the identified baselines including the baseline identifier</para>
    /// </example>
    [Cmdlet(VerbsCommon.Get, "IshBaselineItem", SupportsShouldProcess = false)]
    [OutputType(typeof(IshBaselineItem))]
    public sealed class GetIshBaselineItem : BaselineCmdlet
    {
        /// <summary>
        /// <para type="description">The IshSession variable holds the authentication and contract information. This object can be initialized using the New-IshSession cmdlet.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshObjectsGroup")]
        [ValidateNotNullOrEmpty]
        public IshSession IshSession { get; set; }

        /// <summary>
        /// <para type="description">The baseline identifiers for which to retrieve the baseline entries</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup"), ValidateNotNullOrEmpty]
        public string[] Id { get; set; }

        /// <summary>
        /// <para type="description">Array with the baselines for which to retrieve the baseline entries. This array can be passed through the pipeline or explicitly passed via the parameter.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "IshObjectsGroup")]
        [AllowEmptyCollection]
        public IshObject[] IshObject { get; set; }


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
                    var ids = (IshObject != null) ? new IshObjects(IshObject).Ids : Id;
                    int current = 0;
                    foreach (var id in ids)
                    {
                        WriteDebug($"Id[{id}] {++current}/{ids.Length}");
                        if (ShouldProcess(id))
                        {
                            var response = IshSession.Baseline25.GetBaseline(new Baseline25ServiceReference.GetBaselineRequest(id, string.Empty));
                            string xmlIshBaselineItems = response.xmlBaseline;
                            WriteObject(new IshBaselineItems(id, xmlIshBaselineItems).Items, true);
                        }
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
    }
}
