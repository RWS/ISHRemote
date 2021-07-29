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

namespace Trisoft.ISHRemote.Cmdlets.UserRole
{
    /// <summary>
    /// <para type="synopsis">The Remove-IshUserRole cmdlet removes the user roles that are passed through the pipeline or determined via provided parameters</para>
    /// <para type="description">The Remove-IshUserRole cmdlet removes the user roles that are passed through the pipeline or determined via provided parameters</para>
    /// </summary>
    /// <example>
    /// <code>
    /// New-IshSession -WsBaseUrl "https://example.com/InfoShareWS/" -PSCredential Admin
    /// Get-IshUserRole -Id VUSERROLEADMINISTRATOR | Remove-IshUserRole
    /// </code>
    /// <para>New-IshSession will submit into SessionState, so it can be reused by this cmdlet. Gets the user role and tries to remove it, which will fail if the user role is linked to objects.</para>
    /// </example>
    [Cmdlet(VerbsCommon.Remove, "IshUserRole", SupportsShouldProcess = true)]
    public sealed class RemoveIshUserRole : UserRoleCmdlet
    {
        /// <summary>
        /// <para type="description">The IshSession variable holds the authentication and contract information. This object can be initialized using the New-IshSession cmdlet.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshObjectsGroup")]
        [ValidateNotNullOrEmpty]
        public IshSession IshSession { get; set; }

        /// <summary>
        /// <para type="description">The identifier of the user role to remove</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup"), ValidateNotNullOrEmpty]
        public string Id { get; set; }

        /// <summary>
        /// <para type="description">Array with the user roles to remove. This array can be passed through the pipeline or explicitly passed via the parameter.</para>
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
                WriteDebug("Deleting");
               
                if (IshObject!=null)
                {
                    int current = 0;
                    // 1b. Using IshObject[] pipeline or specificly set
                    IshObject[] ishObjects = IshObject;
                    foreach (IshObject ishObject in ishObjects)
                    {
                        WriteDebug($"Id[{ishObject.IshRef}] {++current}/{ishObjects.Length}");
                        if (ShouldProcess(ishObject.IshRef))
                        {
                            IshSession.UserRole25.Delete(ishObject.IshRef);
                        }
                    }
                }
                else
                {
                    // 1a. Using Ids
                    WriteVerbose("Id[" + Id + "]");
                    if (ShouldProcess(Id))
                    {
                        IshSession.UserRole25.Delete(Id);
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
