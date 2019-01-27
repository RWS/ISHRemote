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
using System.Management.Automation;
using Trisoft.ISHRemote.Objects;
using Trisoft.ISHRemote.Objects.Public;
using Trisoft.ISHRemote.Exceptions;
using Trisoft.ISHRemote.HelperClasses;

namespace Trisoft.ISHRemote.Cmdlets.Feature
{
    /// <summary>
    /// <para type="synopsis">The Set-IshFeature cmdlet creates a new IshFeature based on the parameters provided. When IshFeature[] object is passed through the pipeline then the new feature is added based on the parameters provided</para>
    /// <para type="description">The Set-IshFeature cmdlet creates a new IshFeature based on the parameters provided. When IshFeature[] object is passed through the pipeline then the new feature is added based on the parameters provided</para>
    /// </summary>
    /// <example>
    /// <code>
    /// New-IshSession -WsBaseUrl "https://example.com/ISHWS/" -PSCredential "Admin"
    /// $ishFeatures = Set-IshFeature -Name "ISHRemoteStringCond" -Value "StringOne" |
    ///                Set-IshFeature -Name "ISHRemoteVersionCond" -Value "12.0.1"
    /// </code>
    /// <para>New-IshSession will submit into SessionState, so it can be reused by this cmdlet. Build a Condition Context for passing to Get-DocumentObjData.</para>
    /// </example>
    [Cmdlet(VerbsCommon.Set, "IshFeature", SupportsShouldProcess = false)]
    [OutputType(typeof(IshFeature))]
    public sealed class SetIshFeature : TrisoftCmdlet
    {
        /// <summary>
        /// <para type="description">The IshSession variable holds the authentication and contract information. This object can be initialized using the New-IshSession cmdlet.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false)]
        [ValidateNotNullOrEmpty]
        public IshSession IshSession { get; set; }

        /// <summary>
        /// <para type="description">The condition name</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false)]
        public string Name { get; set; }

        /// <summary>
        /// <para type="description">The condition value</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false)]
        public string Value { get; set; }

        /// <summary>
        /// <para type="description">The set of condition names and values</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipeline = true)]
        public IshFeature[] IshFeature { get; set; }

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
                // Work with piped IshFeatures object or create a new one.
                IshFeatures ishFeatures = new IshFeatures(IshFeature);
                string name = Name ?? "";
                string value = Value ?? "";

                WriteVerbose("name[" + name + "] value[" + value + "]");

                if (Name != "")
                {
                    IshFeature ishFeature = new IshFeature(name, value);
                    ishFeatures.AddFeature(ishFeature);
                }

                WriteObject(ishFeatures.Features, true);
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
