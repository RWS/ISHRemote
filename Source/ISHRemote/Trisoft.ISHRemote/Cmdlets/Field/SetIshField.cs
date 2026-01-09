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
using Trisoft.ISHRemote.Objects;
using Trisoft.ISHRemote.Objects.Public;

namespace Trisoft.ISHRemote.Cmdlets.Field
{
    /// <summary>
    /// <para type="synopsis">The Set-IshField cmdlet sets/merges the IshFields 
    /// * If an IshObject[] array parameter is provided (either explicit or via the pipeline), the MergeFields are merged according to the ValueAction parameter to all the IshFields of all the objects, and the objects are returned
    /// * If an IshFields object is provided (either explicit or via the pipeline), the MergeFields are merged according to the ValueAction parameter to the IshFields object and the resulting fields are returned
    /// * If none of the above applies, the fields in the MergeFields are returned</para>
    /// <para type="description">The Set-IshField cmdlet sets/merges the IshFields 
    /// * If an IshObject[] array parameter is provided (either explicit or via the pipeline), the MergeFields are merged according to the ValueAction parameter to all the IshFields of all the objects, and the objects are returned
    /// * If an IshFields object is provided (either explicit or via the pipeline), the MergeFields are merged according to the ValueAction parameter to the IshFields object and the resulting fields are returned
    /// * If none of the above applies, the fields in the MergeFields are returned
    /// Best practice is to supply IshSession for future functionality.</para>
    /// </summary>
    /// <example>
    /// <code>
    /// New-IshSession -WsBaseUrl "https://example.com/ISHWS/" -PSCredential "Admin"
    /// $ishMetadataFieldsAction = Set-IshMetadataField -Name "FISHUSERDISABLED" -Level "none" -ValueType "Element" -Value "TRUE" |
    ///                            Set-IshMetadataField -Name "FISHOBJECTACTIVE" -Level "none" -ValueType "Element" -Value "FALSE"
	/// $ishobject = $ishobject |
    ///              Set-IshField -MergeFields $ishMetadataFieldsAction -ValueAction "Overwrite" |
    ///              Set-IshUser -IshSession $ishSession
    /// </code>
    /// <para>New-IshSession will submit into SessionState, so it can be reused by this cmdlet. Preferably use the specialized IshField cmdlets, more for testing purposes this one.</para>
    /// </example>
    [Cmdlet(VerbsCommon.Set, "IshField", SupportsShouldProcess = false)]
    [OutputType(typeof(IshField),typeof(IshObject))]
    public sealed class SetIshField : FieldCmdlet
    {
        /// <summary>
        /// <para type="description">The IshSession variable holds the authentication and contract information. This object can be initialized using the New-IshSession cmdlet.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshObjectsGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshFieldGroup")]
        [ValidateNotNullOrEmpty]
        public IshSession IshSession { get; set; }

        /// <summary>
        /// <para type="description">The IshField[] to merge</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshObjectsGroup")]
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshFieldGroup")]
        public IshField[] MergeField { get; set; }

        /// <summary>
        /// <para type="description">Whether the fields in the mergefields parameter will be prepended, appended or overwrite the values in the provided IshFields or IshObject objects</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshObjectsGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshFieldGroup")]
        public Enumerations.ValueAction ValueAction
        {
            get { return _valueAction; }
            set { _valueAction = value; }
        }

        /// <summary>
        /// <para type="description">Objects to merge the fields in</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "IshObjectsGroup")]
        [AllowEmptyCollection]
        public IshObject[] IshObject { get; set; }

        /// <summary>
        /// <para type="description">Fields to merge the mergefields in</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipeline = true, ParameterSetName = "IshFieldGroup")]
        public IshField[] IshField { get; set; }


        #region Private fields

        /// <summary>
        /// Private field to store the IshType and provide a default for non-mandatory parameters
        /// </summary>
        private Enumerations.ValueAction _valueAction = Enumerations.ValueAction.Append;

        private List<IshField> _incomingIshField = new List<IshField>();
        #endregion

        protected override void BeginProcessing()
        {
            if (IshSession == null) { IshSession = (IshSession)SessionState.PSVariable.GetValue(ISHRemoteSessionStateIshSession); }
            if (IshSession == null) { IshSession = (IshSession)SessionState.PSVariable.GetValue(ISHRemoteSessionStateGlobalIshSession); }
            if (IshSession == null) { throw new ArgumentException(ISHRemoteSessionStateIshSessionException); }
            WriteDebug($"Using IshSession[{IshSession.Name}] from SessionState.{ISHRemoteSessionStateIshSession} or in turn SessionState.{ISHRemoteSessionStateGlobalIshSession}");
            base.BeginProcessing();
        }

        protected override void ProcessRecord()
        {
            try
            {
                // We use IshObject if available, otherwise the provided or a new IshFields container
                if (IshObject != null)
                {
                    IshObject[] ishObjects = IshObject;
                    WriteVerbose("IshObject.length[" + ishObjects.Length + "] ValueAction[" + ValueAction.ToString() + "]");
                    IshFields ishMergeFields = new IshFields(MergeField);
                    foreach (IshObject ishObject in ishObjects)
                    {
                        ishObject.IshFields.JoinFields(ishMergeFields, ValueAction);
                    }
                    WriteObject(ishObjects, true);
                }
                else if (IshField != null)
                {
                    foreach (IshField ishField in IshField)
                    {
                        _incomingIshField.Add(ishField);
                    }
                }
                else
                {
                    WriteVerbose($"Set-IshField thinks you are passing something strange. Something is null?");
                }
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

        protected override void EndProcessing()
        {
            try
            {
                IshFields ishMergeFields = new IshFields(MergeField);
                IshFields ishFields = new IshFields(IshField);
                WriteVerbose("IshFields ValueAction[" + ValueAction.ToString() + "]");
                ishFields.JoinFields(ishMergeFields, ValueAction);
                WriteObject(ishFields.Fields(), true);
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
            finally
            {
                base.EndProcessing();
            }
        }
    }
}
