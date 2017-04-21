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

namespace Trisoft.ISHRemote.Cmdlets.Baseline
{
    /// <summary>
    /// <para type="synopsis">The Remove-IshBaseline cmdlet removes the baselines that are passed through the pipeline or determined via provided parameters</para>
    /// <para type="description">The Remove-IshBaseline cmdlet removes the baselines that are passed through the pipeline or determined via provided parameters</para>
    /// </summary>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/ISHWS/" -PSCredential "Admin"
    /// $baselineId = "GUID-17443161-9CAD-4A9A-A3D3-F2942EDB0534"
    /// Remove-IshBaseline -IshSession $ishSession -Id $baselineId
    /// </code>
    /// <para>Retrieve metadata from the identified baseline</para>
    /// </example>
    [Cmdlet(VerbsCommon.Remove, "IshBaseline", SupportsShouldProcess = true)]
    public sealed class RemoveIshBaseline : BaselineCmdlet
    {
        /// <summary>
        /// <para type="description">The IshSession variable holds the authentication and contract information. This object can be initialized using the New-IshSession cmdlet.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshObjectsGroup")]
        [ValidateNotNullOrEmpty]
        public IshSession IshSession { get; set; }

        /// <summary>
        /// <para type="description">The identifier of the baseline to remove</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup"), ValidateNotNullOrEmpty]
        public string Id { get; set; }

        /// <summary>
        /// <para type="description">Array with the baselines to remove. This array can be passed through the pipeline or explicitly passed via the parameter.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "IshObjectsGroup")]
        [AllowEmptyCollection]
        public IshObject[] IshObject { get; set; }

        protected override void ProcessRecord()
        {

            try
            {
                WriteDebug("Deleting");

                if (IshObject != null)
                {
                    int current = 0;
                    // 1b. Using IshObject[] pipeline or specificly set
                    IshObject[] ishObjects = IshObject;
                    foreach (IshObject ishObject in ishObjects)
                    {
                        WriteDebug($"Id[{ishObject.IshRef}] {++current}/{ishObjects.Length}");
                        if (ShouldProcess(ishObject.IshRef))
                        {
                            IshSession.Baseline25.Delete(ishObject.IshRef);
                        }
                    }
                }
                else
                {
                    // 1a. Using Ids
                    WriteVerbose("Id[" + Id + "]");
                    if (ShouldProcess(Id))
                    {
                        IshSession.Baseline25.Delete(Id);
                    }
                }
                // Nothing to retrieve because we just Deleted it, so no output
                WriteVerbose("returned object count[0]");
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
