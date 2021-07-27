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

namespace Trisoft.ISHRemote.Cmdlets.ListOfValues
{
    /// <summary>
    /// <para type="synopsis">The Remove-IshLovValue cmd-let removes the values that are passed through the pipeline or determined via provided parameters from the specified List of Values</para>
    /// <para type="description">The Remove-IshLovValue cmd-let removes the values that are passed through the pipeline or determined via provided parameters from the specified List of Values</para>
    /// </summary>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/InfoShareWS/" -IshUserName "username" -IshUserPassword  "userpassword"
    /// $lovValue = Add-IshLovValue -IshSession $ishSession -LovId "DILLUSTRATIONTYPE" -Label "New image type" -Description "New image type description"
    /// Remove-IshLovValue -IshSession $ishSession -LovId $lovId -LovValueId $lovValue.IshRef
    /// </code>
    /// <para>Add and remove a value</para>
    /// </example>
    [Cmdlet(VerbsCommon.Remove, "IshLovValue", SupportsShouldProcess = true)]
    public sealed class RemoveIshLovValue : ListOfValuesCmdlet
    {

        /// <summary>
        /// <para type="description">The IshSession variable holds the authentication and contract information. This object can be initialized using the New-IshSession cmdlet.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshLovValueGroup")]
        [ValidateNotNullOrEmpty]
        public IshSession IshSession { get; set; }

        /// <summary>
        /// <para type="description">The element name of the List of Values to delete Value from</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]
        [ValidateNotNullOrEmpty]
        public string LovId{ get; set; }

        /// <summary>
        /// <para type="description">The element name of the value to remove</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup"), ValidateNotNullOrEmpty]
        public string LovValueId{ get; set; }

        /// <summary>
        /// <para type="description">LovValue object to remove</para>
        /// </summary>
        // TODO [Could] Promote parameter IshLovValue to IshLovValue[] processing
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "IshLovValueGroup")]
        public IshLovValue IshLovValue { get; set; }

        protected override void BeginProcessing()
        {
            if (IshSession == null) { IshSession = (IshSession)SessionState.PSVariable.GetValue(ISHRemoteSessionStateIshSession); }
            if (IshSession == null) { throw new ArgumentException(ISHRemoteSessionStateIshSessionException); }
            WriteDebug($"Using IshSession[{IshSession.Name}] from SessionState.{ISHRemoteSessionStateIshSession}");
            base.BeginProcessing();
        }

        /// <summary>
        /// Process the Remove-IshLovValue commandlet.
        /// </summary>
        /// <exception cref="TrisoftAutomationException"></exception>
        /// <exception cref="Exception"></exception>
        protected override void ProcessRecord()
        {
            try
            {
                WriteDebug("Removing");
                
                if( IshLovValue != null )
                {
                    WriteDebug($"LovId[{IshLovValue.LovId}] LovValueId[{IshLovValue.IshRef}]");
                    if (ShouldProcess(IshLovValue.IshRef))
                    {
                        IshSession.ListOfValues25.DeleteValue(IshLovValue.LovId, IshLovValue.IshRef);
                    }
                }
                else
                {
                    WriteDebug($"LovId[{LovId}] LovValueId[{LovValueId}]");
                    if (ShouldProcess(LovValueId))
                    {
                        IshSession.ListOfValues25.DeleteValue(LovId, LovValueId);
                    }
                }
                WriteVerbose("returned value count[0]");
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
