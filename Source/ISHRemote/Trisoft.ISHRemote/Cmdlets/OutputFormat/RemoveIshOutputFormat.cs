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
using System.Management.Automation;
using Trisoft.ISHRemote.Objects;
using Trisoft.ISHRemote.Objects.Public;
using Trisoft.ISHRemote.Exceptions;
using Trisoft.ISHRemote.HelperClasses;

namespace Trisoft.ISHRemote.Cmdlets.OutputFormat
{
    /// <summary>
    /// <para type="synopsis">The Remove-IshOutputFormat cmdlet removes the output formats that are passed through the pipeline or determined via provided parameters</para>
    /// <para type="description">The Remove-IshOutputFormat cmdlet removes the output formats that are passed through the pipeline or determined via provided parameters</para>
    /// </summary>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/InfoShareWS/" -IshUserName "username" -IshUserPassword  "userpassword"
    /// $metadata = Set-IshMetadataField -IshSession $ishSession -Name "FISHRESOLUTIONS" -Level "none" -Value "Low"  |
    ///             Set-IshMetadataField -IshSession $ishSession -Name "FISHSINGLEFILE" -Level "none" -Value "TRUE" -ValueType Element |
    ///             Set-IshMetadataField -IshSession $ishSession -Name "FISHCLEANUP" -Level "none" -Value "TRUE" -ValueType Element |
    ///             Set-IshMetadataField -IshSession $ishSession -Name "FISHKEEPDTDSYSTEMID" -Level "none" -Value "TRUE" -ValueType Element |
    ///             Set-IshMetadataField -IshSession $ishSession -Name "FISHPUBRESOLVEVARIABLES" -Level "none" -Value "TRUE" -ValueType Element
    /// $outputFormatAdd = Add-IshOutputFormat -IshSession $ishSession `
    /// -Name "MyOutputFormat" `
    /// -EDT "EDTPDF" `
    /// -Metadata $metadata
    /// Remove-IshOutputFormat -Id $outputFormatAdd[0].IshRef
    /// </code>
    /// <para>New-IshSession will submit into SessionState, so it can be reused by this cmdlet. Remove added output format</para>
    /// </example>
    [Cmdlet(VerbsCommon.Remove, "IshOutputFormat", SupportsShouldProcess = true)]
    public sealed class RemoveIshOutputFormat : OutputFormatCmdlet
    {

        /// <summary>
        /// <para type="description">The IshSession variable holds the authentication and contract information. This object can be initialized using the New-IshSession cmdlet.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshObjectsGroup")]
        [ValidateNotNullOrEmpty]
        public IshSession IshSession { get; set; }

        /// <summary>
        /// <para type="description">The Id - element name of the OutputFormat to be removed</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup"), ValidateNotNullOrEmpty]
        public string Id { get; set; }

        /// <summary>
        /// <para type="description">The IshObject array containing OutputFormats that needs to be deleted. Pipeline</para>
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
        /// Process the Remove-IshOutputFormat commandlet.
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
                        WriteDebug($"OutputFormat Id[{id}] {++current}/{IshObject.Length}");
                        if (ShouldProcess(id))
                        {
                            IshSession.OutputFormat25.Delete(id);
                        }
                    }
                }
                else
                { 
                    // 2a. Remove using provided parameters (not piped IshObject)
                    WriteDebug($"OutputFormat Id[{Id}]");
                    if (ShouldProcess(Id))
                    {
                        IshSession.OutputFormat25.Delete(Id);
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
