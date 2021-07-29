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
using Trisoft.ISHRemote.HelperClasses;

namespace Trisoft.ISHRemote.Cmdlets.EDT
{
    /// <summary>
    /// <para type="synopsis">The Remove-IshEDT cmdlet removes the EDTs that are passed through the pipeline or determined via provided parameters</para>
    /// <para type="description">The Remove-IshEDT cmdlet removes the EDTs that are passed through the pipeline or determined via provided parameters</para>
    /// </summary>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/InfoShareWS/" -IshUserName "username" -IshUserPassword  "userpassword"
    /// $metadata = Set-IshMetadataField -IshSession $ishSession -Name "EDT-CANDIDATE" -Level "none" -Value "XML" |
    ///             Set-IshMetadataField -IshSession $ishSession -Name "EDT-FILE-EXTENSION" -Level "none" -Value "XML" |
    ///             Set-IshMetadataField -IshSession $ishSession -Name "EDT-MIME-TYPE" -Level "none" -Value "text/xml"
    /// $edt = Add-IshEDT -IshSession $ishSession `
    ///  -Name "MYEDT" `
    ///  -Metadata $metadata
    /// #Remove EDT using pipeline
    /// $edt | Remove-IshEDT
    /// </code>
    /// <para>New-IshSession will submit into SessionState, so it can be reused by this cmdlet. Remove added EDT</para>
    /// </example>
    [Cmdlet(VerbsCommon.Remove, "IshEDT", SupportsShouldProcess = true)]
    public sealed class RemoveIshEDT : EDTCmdlet
    {

        /// <summary>
        /// <para type="description">The IshSession variable holds the authentication and contract information. This object can be initialized using the New-IshSession cmdlet.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshObjectsGroup")]
        [ValidateNotNullOrEmpty]
        public IshSession IshSession { get; set; }

        /// <summary>
        ///  <para type="description">The Id - element name of the EDT to be removed</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup"), ValidateNotNullOrEmpty]
        public string Id { get; set; }

        /// <summary>
        ///  <para type="description">The IshObject array containing EDTs that needs to be deleted. Pipeline</para>
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

        /// <summary>
        /// Process the Remove-IshEDT commandlet.
        /// </summary>
        /// <exception cref="TrisoftAutomationException"></exception>
        /// <exception cref="Exception"></exception>
        protected override void ProcessRecord()
        {
            try
            {
                // 1. Validating the input
                WriteDebug("Validating");

                // 2. Doing Remove
                WriteDebug("Removing");

                if (IshObject != null)
                {
                    // 2b. Remove using IshObject[] pipeline       
                    int current = 0;
                    foreach (IshObject ishObject in IshObject)
                    {
                        // read "ishRef" from the ishObject object
                        string id = ishObject.IshRef;
                        WriteDebug($"EDT Id[{id}] {++current}/{IshObject.Length}");
                        if (ShouldProcess(id))
                        {
                            IshSession.EDT25.Delete(id);
                        }
                    }
                }
                else
                {
                    // 2a. Remove using provided parameters (not piped IshObject)
                    WriteDebug($"EDT Id[{Id}]");
                    if (ShouldProcess(Id))
                    {
                        IshSession.EDT25.Delete(Id);
                    }
                }
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
