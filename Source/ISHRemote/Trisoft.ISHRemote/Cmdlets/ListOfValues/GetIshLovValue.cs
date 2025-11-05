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
    /// <para type="synopsis">The Get-IshLovValue cmdlet retrieves all values of the specified list of values</para>
    /// <para type="description">The Get-IshLovValue cmdlet retrieves all values of the specified list of values</para>
    /// </summary>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/InfoShareWS/" -IshUserName "username" -IshUserPassword  "userpassword"
    /// $lovValues = Get-IshLovValue -IshSession $ishSession -LovId DILLUSTRATIONTYPE
    /// </code>
    /// <para>Retrieve all values from List of Values</para>
    /// </example>
    /// <example>
    /// <code>
    /// New-IshSession -WsBaseUrl "https://example.com/InfoShareWS/"  -PSCredential username
    /// $lovValues = Get-IshLovValue -LovId DILLUSTRATIONTYPE -LovValueId ("VRESHIGH","VRESLOW")
    /// </code>
    /// <para>Retrieve the full LovValues including description and active flag</para>
    /// </example>
    [Cmdlet(VerbsCommon.Get, "IshLovValue", SupportsShouldProcess = false)]
    [OutputType(typeof(IshLovValue))]
    public sealed class GetIshLovValue : ListOfValuesCmdlet
    {
        /// <summary>
        /// <para type="description">The IshSession variable holds the authentication and contract information. This object can be initialized using the New-IshSession cmdlet.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false), ValidateNotNullOrEmpty]
        public IshSession IshSession { get; set; }

        /// <summary>
        /// <para type="description">The element name of the List of Values for which to retrieve values.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false), ValidateNotNullOrEmpty]
        public string[] LovId { get; set; }

        /// <summary>
        /// <para type="description">The element name of the value in the List of Values for which to retrieve complete initialized values.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false), ValidateNotNullOrEmpty]
        public string[] LovValueId { get; set; }

        /// <summary>
        /// <para type="description">The activity filter to limit the amount of objects returned. Default is no filtering.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false), ValidateNotNullOrEmpty]
        public Enumerations.ActivityFilter ActivityFilter
        {
            get { return _activityFilter; }
            set { _activityFilter = value; }
        }

        

        #region Private fields
        /// <summary>
        /// Private field to store the IshType and provide a default for non-mandatory parameters
        /// </summary>
        private Enumerations.ActivityFilter _activityFilter = Enumerations.ActivityFilter.None;
        #endregion

        protected override void BeginProcessing()
        {
            if (IshSession == null) { IshSession = (IshSession)SessionState.PSVariable.GetValue(ISHRemoteSessionStateIshSession); }
            if (IshSession == null) { throw new ArgumentException(ISHRemoteSessionStateIshSessionException); }
            WriteDebug($"Using IshSession[{IshSession.Name}] from SessionState.{ISHRemoteSessionStateIshSession}");
            base.BeginProcessing();
        }

        /// <summary>
        /// Process the Get-IshLovValue commandlet.
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

                ListOfValues25ServiceReference.ActivityFilter activityFilter = EnumConverter.ToActivityFilter<ListOfValues25ServiceReference.ActivityFilter>(ActivityFilter);

                // LovValues to write to the ouput
                List<IshLovValue> returnedLovValues = new List<IshLovValue>();

                // 2. Doing Retrieve
                WriteDebug("Retrieving");

                // 2a. Retrieve using provided LovIds array
                WriteDebug($"LovId[{LovId}]");
                string xmlIshLovValues = IshSession.ListOfValues25.RetrieveValues(LovId, activityFilter);
                IshLovValues retrievedLovValues = new IshLovValues(xmlIshLovValues);

                // 2b. Filter to provided LovValueIds, if any, otherwise return all
                if (LovValueId != null)
                {
                    // brute force, I know, but accurate
                    foreach (string lovId in LovId)
                    {
                        foreach (string lovValueId in LovValueId)
                        {
                            IshLovValue foundIshLovValue;
                            if (retrievedLovValues.TryGetIshLovValue(lovId, lovValueId, out foundIshLovValue))
                            {
                                returnedLovValues.Add(foundIshLovValue);
                            }
                        }
                    }
                    
                }
                else
                { 
                    returnedLovValues.AddRange(retrievedLovValues.LovValues);
                }

                // 3a. Write it
                WriteVerbose("returned value count[" + returnedLovValues.Count + "]");
                WriteObject(returnedLovValues, true);
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
