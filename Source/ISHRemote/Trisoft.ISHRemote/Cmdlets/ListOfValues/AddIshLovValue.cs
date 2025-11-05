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
using System.ServiceModel;
using Trisoft.ISHRemote.Exceptions;
using Trisoft.ISHRemote.HelperClasses;
using Trisoft.ISHRemote.Objects;
using Trisoft.ISHRemote.Objects.Public;

namespace Trisoft.ISHRemote.Cmdlets.ListOfValues
{
    /// <summary>
    /// <para type="synopsis">The Add-IshLovValue cmdlet adds the new values that are passed through the pipeline or determined via provided parameters to the specified List of Values</para>
    /// <para type="description">The Add-IshLovValue cmdlet adds the new values that are passed through the pipeline or determined via provided parameters to the specified List of Values</para>
    /// </summary>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/InfoShareWS/" -IshUserName "username" -IshUserPassword  "userpassword"
    /// $lovValue = Add-IshLovValue -LovId "DILLUSTRATIONTYPE" -Label "New image type" -Description "New image type description"
    /// </code>
    /// <para>New-IshSession will submit into SessionState, so it can be reused by this cmdlet. Adding a Value into the List of Values</para>
    /// </example>
    [Cmdlet(VerbsCommon.Add, "IshLovValue", SupportsShouldProcess = true)]
    [OutputType(typeof(IshLovValue))]
    public sealed class AddIshLovValue : ListOfValuesCmdlet
	{

        /// <summary>
        /// <para type="description">The IshSession variable holds the authentication and contract information. This object can be initialized using the New-IshSession cmdlet.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshLovValueGroup")]
        [ValidateNotNullOrEmpty]
		public IshSession IshSession { get; set; }

        /// <summary>
        /// <para type="description">The identifier (element name) of the list of values where the new lov value will be created</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]
        [ValidateNotNullOrEmpty]
		public string LovId { get; set; }

        /// <summary>
        /// <para type="description">The label of the new lov value</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup"), ValidateNotNullOrEmpty]
		public string Label { get; set; }

        /// <summary>
        /// <para type="description">The description of the new lov value</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup"), ValidateNotNullOrEmpty]
		public string Description { get; set; }

        /// <summary>
        /// <para type="description">The lov value to create. This lov value can be passed through the pipeline or explicitly passed via the parameter.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "IshLovValueGroup")]
        // TODO [Could] Promote parameter IshLovValue to IshLovValue[] processing
        public IshLovValue IshLovValue { get; set; }

        /// <summary>
        /// <para type="description">The identifier (element name) of the new lov value</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]
        [ValidateNotNullOrEmpty]
		public string LovValueId { get; set; }

        protected override void BeginProcessing()
        {
            if (IshSession == null) { IshSession = (IshSession)SessionState.PSVariable.GetValue(ISHRemoteSessionStateIshSession); }
            if (IshSession == null) { throw new ArgumentException(ISHRemoteSessionStateIshSessionException); }
            WriteDebug($"Using IshSession[{IshSession.Name}] from SessionState.{ISHRemoteSessionStateIshSession}");
            base.BeginProcessing();
        }

        /// <summary>
        /// Process the Add-IshLovValue commandlet.
        /// </summary>
        /// <exception cref="TrisoftAutomationException"></exception>
        /// <exception cref="Exception"></exception>
        /// <remarks>Writes an <see cref="IshLovValue"/> array to the pipeline</remarks>
        protected override void ProcessRecord()
		{
			try
			{
			    string lovId = null;
			    string lovValueId = null;

				// 2. Doing Add
				WriteDebug("Adding");

				// 2a. Add using provided parameters 
                if (IshLovValue != null)
                {
                    WriteDebug($"LovId[{IshLovValue.LovId}] LovValueId[{IshLovValue.IshRef}] Label[{IshLovValue.Label}] Description[{IshLovValue.Description}]");
                    if (ShouldProcess(IshLovValue.IshRef))
                    {
                        var response = IshSession.ListOfValues25.CreateValue2(new ListOfValues25ServiceReference.CreateValue2Request(
                             IshLovValue.LovId,
                             IshLovValue.IshRef,
                             IshLovValue.Label,
                             IshLovValue.Description));
                        lovId = IshLovValue.LovId;
                        lovValueId = response.lovValueId;
                    }
                }
                else
                {
                    var inlovValueId = LovValueId ?? "";
                    WriteDebug($"LovId[{LovId}] LovValueId[{LovValueId}] Label[{Label}] Description[{Description}]");
                    if (ShouldProcess(inlovValueId))
                    {
                        var response = IshSession.ListOfValues25.CreateValue2(new ListOfValues25ServiceReference.CreateValue2Request(
                            LovId,
                            inlovValueId,
                            Label,
                            Description));
                        lovId = LovId;
                        lovValueId = response.lovValueId;
                    }
                }

				// 3a. Retrieve added value and write it out
				WriteDebug("Retrieving");

				string xmlIshLovValues = IshSession.ListOfValues25.RetrieveValues(
					new string[] { lovId },
					ListOfValues25ServiceReference.ActivityFilter.None);
				IshLovValues retrievedLovValues = new IshLovValues(xmlIshLovValues);
				// find the value we added in a list of all LovValues within the given LovId
				foreach (IshLovValue ishLovValue in retrievedLovValues.LovValues)
				{
					if (ishLovValue.IshRef == lovValueId)
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
            catch (AggregateException aggregateException)
            {
                var flattenedAggregateException = aggregateException.Flatten();
                WriteWarning(flattenedAggregateException.ToString());
                ThrowTerminatingError(new ErrorRecord(flattenedAggregateException, base.GetType().Name, ErrorCategory.NotSpecified, null));
            }
            catch (TimeoutException timeoutException)
            {
                WriteVerbose("TimeoutException Message[" + timeoutException.Message + "] StackTrace[" + timeoutException.StackTrace + "]");
                ThrowTerminatingError(new ErrorRecord(timeoutException, base.GetType().Name, ErrorCategory.OperationTimeout, null));
            }
            catch (CommunicationException communicationException)
            {
                WriteVerbose("CommunicationException Message[" + communicationException.Message + "] StackTrace[" + communicationException.StackTrace + "]");
                ThrowTerminatingError(new ErrorRecord(communicationException, base.GetType().Name, ErrorCategory.OperationStopped, null));
            }
            catch (Exception exception)
			{
				ThrowTerminatingError(new ErrorRecord(exception, base.GetType().Name, ErrorCategory.NotSpecified, null));
			}
		}
	}
}
