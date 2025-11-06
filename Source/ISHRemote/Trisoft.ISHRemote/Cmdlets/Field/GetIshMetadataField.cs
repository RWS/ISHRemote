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
    /// <para type="synopsis">The Get-IshMetadataField -IshSession $ishSession cmdlet return the value of a field with a certain name, level and value type.</para>
    /// <para type="description">The Get-IshMetadataField -IshSession $ishSession cmdlet return the value of a field with a certain name, level and value type. Best practice is to supply IshSession for future functionality.</para>
    /// </summary>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/InfoShareWS/" -PSCredential Admin
    /// Get-IshUser -IshSession $ishSession | Get-IshMetadataField -IshSession $ishSession -Name "USERNAME"
    /// </code>
    /// <para>Retrieves your username from the return IshUser object. The Get-IshMetadataField defaults to level None and type value.</para>
    /// </example>
    [Cmdlet(VerbsCommon.Get, "IshMetadataField", SupportsShouldProcess = false)]
    [OutputType(typeof(string))]
    public sealed class GetIshMetadataField : FieldCmdlet
    {
        /// <summary>
        /// <para type="description">The IshSession variable holds the authentication and contract information. This object can be initialized using the New-IshSession cmdlet.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshFieldGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshObjectGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshFolderGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshEventGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshBackgroundTaskGroup")]
        [ValidateNotNullOrEmpty]
        public IshSession IshSession { get; set; }

        /// <summary>
        /// <para type="description">The field name</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshFieldGroup")]
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshObjectGroup")]
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshFolderGroup")]
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshEventGroup")]
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshBackgroundTaskGroup")]
        public string Name { get; set; }

        /// <summary>
        /// <para type="description">The field level</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshFieldGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshObjectGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshFolderGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshEventGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshBackgroundTaskGroup")]
        public Enumerations.Level Level
        {
            get { return _level; }
            set { _level = value; }
        }

        /// <summary>
        /// <para type="description">The value type</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshFieldGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshObjectGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshFolderGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshEventGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshBackgroundTaskGroup")]
        public Enumerations.ValueType ValueType
        {
            get { return _valueType; }
            set { _valueType = value; }
        }

        /// <summary>
        /// <para type="description">The fields container object, accepts multiple ISHFields objects</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "IshFieldGroup")]
        public IshField[] IshField { get; set; }

        /// <summary>
        /// <para type="description">The objects container object</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "IshObjectGroup")]
        public IshObject[] IshObject { get; set; }

        /// <summary>
        /// <para type="description">The objects container object, specialized for folder handling</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "IshFolderGroup")]
        public IshFolder[] IshFolder { get; set; }

        /// <summary>
        /// <para type="description">The objects container object, specialized for event handling</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "IshEventGroup")]
        public IshEvent[] IshEvent { get; set; }

        /// <summary>
        /// <para type="description">The objects container object, specialized for event handling</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "IshBackgroundTaskGroup")]
        public IshBackgroundTask[] IshBackgroundTask { get; set; }

        #region Private fields
        /// <summary>
        /// Private fields to store the parameters and provide a default for non-mandatory parameters
        /// </summary>
        private Enumerations.Level _level = Enumerations.Level.None;
        private Enumerations.ValueType _valueType = Enumerations.ValueType.Value;
        #endregion

        protected override void BeginProcessing()
        {
            if (IshSession == null) { IshSession = (IshSession)SessionState.PSVariable.GetValue(ISHRemoteSessionStateIshSession); }
            //if (IshSession == null) { throw new ArgumentException(ISHRemoteSessionStateIshSessionException); } // don't throw as for Field cmdlets this trully is optional
            //WriteDebug($"Using IshSession[{IshSession.Name}] from SessionState.{ISHRemoteSessionStateIshSession}");
            base.BeginProcessing();
        }

        protected override void ProcessRecord()
        {
            try
            {
                WriteDebug("name[" + Name + "] level[" + Level + "] valueType[" + ValueType + "]");
                if (IshField != null)
                {
                    IshFields ishFields = new IshFields(IshField);
                    // Below code is retrieves all fields, but on IshObject/IshFolder/... only the RetrieveFirst is returned. For IshField[] there is only 1 field per ProcessRecord
                    // foreach (IshField ishField in ishFields.Retrieve(Name, Level, ValueType))
                    // { 
                    //     var metadataField = ishField.ToMetadataField() as IshMetadataField;
                    //     if (metadataField == null)
                    //         throw new InvalidOperationException($"field.ToMetadataField() is not IshMetadataField name[{ishField.Name}] level[{ ishField.Level}] valuetype[{ishField.ValueType}]");
                    //     WriteObject(metadataField.Value);
                    // }
                    WriteObject(ishFields.GetFieldValue(Name, Level, ValueType), true);
                }
                else if (IshObject != null)
                {
                    foreach (IshObject ishObject in IshObject)
                    {
                        WriteObject(ishObject.IshFields.GetFieldValue(Name, Level, ValueType), true);
                    }
                }
                else if (IshFolder != null)
                {
                    foreach (IshFolder ishFolder in IshFolder)
                    {
                        WriteObject(ishFolder.IshFields.GetFieldValue(Name, Level, ValueType), true);
                    }
                }
                else if (IshEvent != null)
                {
                    foreach (IshEvent ishEvent in IshEvent)
                    {
                        WriteObject(ishEvent.IshFields.GetFieldValue(Name, Level, ValueType), true);
                    }
                }
                else if (IshBackgroundTask != null)
                {
                    foreach (IshBackgroundTask ishBackgroundTask in IshBackgroundTask)
                    {
                        WriteObject(ishBackgroundTask.IshFields.GetFieldValue(Name, Level, ValueType), true);
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
    }
}

