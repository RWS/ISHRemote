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

namespace Trisoft.ISHRemote.Cmdlets.Field
{
    /// <summary>
    /// <para type="synopsis">The Set-IshRequestedMetadataField cmdlet creates a field with a certain name, level and value type When IshFields object is passed through the pipeline then the new field is added to that object.</para>
    /// <para type="description">The Set-IshRequestedMetadataField cmdlet creates a field with a certain name, level and value type When IshFields object is passed through the pipeline then the new field is added to that object.</para>
    /// </summary>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/InfoShareWS/" -PSCredential Admin
    /// Set-IshRequestedMetadataField -IshSession $ishSession -Name "USERNAME"
    /// </code>
    /// <para>Creates an IshFields structure holding one IshRequestedMetadataField with name USERNAME and defaults to level None.</para>
    /// </example>
    [Cmdlet(VerbsCommon.Set, "IshRequestedMetadataField", SupportsShouldProcess = false)]
    [OutputType(typeof(IshField))]
    public sealed class SetIshRequestedMetadataField : FieldCmdlet
    {
        /// <summary>
        /// <para type="description">The IshSession variable holds the authentication and contract information. This object can be initialized using the New-IshSession cmdlet.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false)]
        [ValidateNotNullOrEmpty]
        public IshSession IshSession { get; set; }

        /// <summary>
        /// <para type="description">The field name</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false)]
        public string Name { get; set; }

        /// <summary>
        /// <para type="description">The field level</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false)]
        public Enumerations.Level Level
        {
            get { return _level; }
            set { _level = value; }
        }

        /// <summary>
        /// <para type="description">The value type</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false)]
        public Enumerations.ValueType ValueType
        {
            get { return _valueType; }
            set { _valueType = value; }
        }

        /// <summary>
        /// <para type="description">The fields container object</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipeline = true)]
        public IshField[] IshField { get; set; }


        #region Private fields
        /// <summary>
        /// Private fields to store the parameters and provide a default for non-mandatory parameters
        /// </summary>
        private Enumerations.Level _level = Enumerations.Level.None;
        private Enumerations.ValueType _valueType = Enumerations.ValueType.Value;

        private List<IshField> _incomingIshField = new List<IshField>();
        #endregion

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
                if (IshField != null)
                {
                    foreach (IshField ishField in IshField)
                    {
                        _incomingIshField.Add(ishField);
                    }
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
                WriteVerbose("name[" + Name + "] level[" + Level + "] valueType[" + ValueType + "]");
                IshFields ishFields = new IshFields(_incomingIshField);
                // Check if enum values are set
                IshField ishField = new IshRequestedMetadataField(Name, Level, ValueType);
                ishFields.AddOrUpdateField(ishField, Enumerations.ActionMode.Read);
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

