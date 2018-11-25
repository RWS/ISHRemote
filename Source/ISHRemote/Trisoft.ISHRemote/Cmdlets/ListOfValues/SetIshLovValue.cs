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

namespace Trisoft.ISHRemote.Cmdlets.ListOfValues
{
    /// <summary>
    /// <para type="synopsis">The Set-IshLovValue cmdlet updates the value that is passed through the pipeline or determined via provided parameters in the specified List of Values.</para>
    /// <para type="description">The Set-IshLovValue cmdlet updates the value that is passed through the pipeline or determined via provided parameters in the specified List of Values.</para>
    /// </summary>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/InfoShareWS/" -IshUserName "username" -IshUserPassword  "userpassword"
    /// $lovValue1 = Add-IshLovValue -IshSession $ishSession -LovId "DILLUSTRATIONTYPE" -Label "New image type" -Description "New image type description"
    /// $lovValue2 = Set-IshLovValue -IshSession $ishSession -LovId "DILLUSTRATIONTYPE" -LovValueId $lovValue1.IshRef -Label "Updated new image type" -Active $true -Description "Updated description"
    /// </code>
    /// <para>Add and update Value</para>
    /// </example>
    [Cmdlet(VerbsCommon.Set, "IshLovValue", SupportsShouldProcess = true)]
    [OutputType(typeof(IshLovValue))]
    public sealed class SetIshLovValue : ListOfValuesCmdlet
    {

        /// <summary>
        /// <para type="description">The IshSession variable holds the authentication and contract information. This object can be initialized using the New-IshSession cmdlet.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshLovValueGroup")]
        [ValidateNotNullOrEmpty]
        public IshSession IshSession { get; set; }

        /// <summary>
        /// <para type="description">The element name of the List of Values</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]
        [ValidateNotNullOrEmpty]
        public string LovId { get; set; }

        /// <summary>
        /// <para type="description">The element name of the value to update</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup"), ValidateNotNullOrEmpty]
        public string LovValueId { get; set; }

        /// <summary>
        /// <para type="description">The Label of the Value</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]
        [ValidateNotNullOrEmpty]
        public string Label { get; set; }

        /// <summary>
        /// <para type="description">The Description of the Value</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]
        [ValidateNotNullOrEmpty]
        public string Description { get; set; }

        /// <summary>
        /// <para type="description">The Active parameter indicates if the value is still active. Making it mandatory since uninitialized boolean is 'false' which will be used in update.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]
        [ValidateNotNullOrEmpty]
        public bool Active { get; set; }

        /// <summary>
        /// <para type="description">Lov value to update. This lov value can be passed through the pipeline or explicitly passed via the parameter.</para>
        /// </summary>
        // TODO [Could] Promote parameter IshLovValue to IshLovValue[] processing
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "IshLovValueGroup")]
        public IshLovValue IshLovValue {get ; set ;}

        protected override void BeginProcessing()
        {
            if (IshSession == null) { IshSession = (IshSession)SessionState.PSVariable.GetValue(ISHRemoteSessionStateIshSession); }
            if (IshSession == null) { throw new ArgumentNullException(ISHRemoteSessionStateIshSessionException); }
            WriteDebug($"Using IshSession[{IshSession.Name}] from SessionState.{ISHRemoteSessionStateIshSession}");
            base.BeginProcessing();
        }

        /// <summary>
        /// Process the Set-IshLovValue commandlet.
        /// </summary>
        /// <exception cref="TrisoftAutomationException"></exception>
        /// <exception cref="Exception"></exception>
        /// <remarks>Writes an <see cref="IshLovValue"/> array to the pipeline</remarks>
        protected override void ProcessRecord()
        {
            try
            {
                // 1. Validating the input
                WriteDebug("Validating");
                string lovIdToRetrieve;
                string lovValueIdToRetrieve;

                // 2. Doing Update
                WriteDebug("Updating");

                // 2a. Update using provided parameters 
                if (IshLovValue != null)
                {
                    // 2b. Update using IshLovValue pipeline
                    WriteDebug($"LovId[{IshLovValue.LovId}] LovValueId[{IshLovValue.IshRef}]");
                    if (ShouldProcess(IshLovValue.IshRef))
                    {
                        IshSession.ListOfValues25.UpdateValue(
                            IshLovValue.LovId,
                            IshLovValue.IshRef,
                            IshLovValue.Label,
                            IshLovValue.Description,
                            IshLovValue.Active);
                    }
                    lovIdToRetrieve = IshLovValue.LovId;
                    lovValueIdToRetrieve = IshLovValue.IshRef;
                }
                else
                {
                    WriteDebug($"LovId[{LovId}] LovValueId[{LovValueId}]");
                    if (ShouldProcess(LovValueId))
                    {
                        IshSession.ListOfValues25.UpdateValue(
                            LovId,
                            LovValueId,
                            Label,
                            Description,
                            Active);
                    }
                    lovIdToRetrieve = LovId;
                    lovValueIdToRetrieve = LovValueId;
                }

                // 3a. Retrieve updated value and write to the output
                WriteDebug("Retrieving");

                string xmlIshLovValues = IshSession.ListOfValues25.RetrieveValues(
                    new string[] { lovIdToRetrieve },
                    ListOfValues25ServiceReference.ActivityFilter.None);
                IshLovValues retrievedLovValues = new IshLovValues(xmlIshLovValues);
                
                // find the value we updated in a list of all LovValues within the given LovId
                foreach(IshLovValue ishLovValue in retrievedLovValues.LovValues)
                {
                    if (ishLovValue.IshRef == lovValueIdToRetrieve)
                    {
                        WriteObject(ishLovValue);
                        break;
                    }
                }
                WriteVerbose("returned value count[1]");
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
