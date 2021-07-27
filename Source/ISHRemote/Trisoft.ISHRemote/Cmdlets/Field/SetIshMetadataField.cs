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
using System.Management.Automation;
using Trisoft.ISHRemote.Objects;
using Trisoft.ISHRemote.Objects.Public;
using Trisoft.ISHRemote.Exceptions;
using Trisoft.ISHRemote.HelperClasses;
using System.Collections.Generic;

namespace Trisoft.ISHRemote.Cmdlets.Field
{
    /// <summary>
    /// <para type="synopsis">The Set-IshMetadataField -IshSession $ishSession cmdlet creates value fields based on the parameters provided When IshFields object is passed through the pipeline then new value field is added according to the parameters provided.</para>
    /// <para type="description">The Set-IshMetadataField -IshSession $ishSession cmdlet creates value fields based on the parameters provided When IshFields object is passed through the pipeline then new value field is added according to the parameters provided.</para>
    /// </summary>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/InfoShareWS/" -PSCredential Admin
    /// Set-IshMetadataField -IshSession $ishSession -Name "USERNAME"
    /// </code>
    /// <para>Creates an IshFields structure holding one IshMetadataField with name USERNAME and defaults to level None.</para>
    /// </example>
    [Cmdlet(VerbsCommon.Set, "IshMetadataField", SupportsShouldProcess = false)]
    [OutputType(typeof(IshField), typeof(IshObject), typeof(IshFolder), typeof(IshEvent))]
    [Alias("Set-IshRequiredCurrentMetadataField")]
    public sealed class SetIshMetadataField : FieldCmdlet
    {
        /// <summary>
        /// <para type="description">The IshSession variable holds the authentication and contract information. This object can be initialized using the New-IshSession cmdlet.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshFieldGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshObjectGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshFolderGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshEventGroup")]
        [ValidateNotNullOrEmpty]
        public IshSession IshSession { get; set; }

        /// <summary>
        /// <para type="description">The field name</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshFieldGroup")]
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshObjectGroup")]
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshFolderGroup")]
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshEventGroup")]
        public string Name { get; set; }

        /// <summary>
        /// <para type="description">The field level</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshFieldGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshObjectGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshFolderGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshEventGroup")]
        public Enumerations.Level Level
        {
            get { return _level; }
            set { _level = value; }
        }

        /// <summary>
        /// <para type="description">The field value</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshFieldGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshObjectGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshFolderGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshEventGroup")]
        public string Value { get; set; }

        /// <summary>
        /// <para type="description">The value type</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshFieldGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshObjectGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshFolderGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshEventGroup")]
        public Enumerations.ValueType ValueType
        {
            get { return _valueType; }
            set { _valueType = value; }
        }

        /// <summary>
        /// <para type="description">The fields container object</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipeline = true, ParameterSetName = "IshFieldGroup")]
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


        #region Private fields
        /// <summary>
        /// Private fields to store the parameters and provide a default for non-mandatory parameters
        /// </summary>
        private Enumerations.Level _level = Enumerations.Level.None;
        private Enumerations.ValueType _valueType = Enumerations.ValueType.Value;
        private IshMetadataField _ishMetadataField = null;
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
                Value = (Value == null) ? "" : Value;
                WriteDebug("name[" + Name + "] level[" + Level + "] value[" + Value + "] valueType[" + ValueType + "]");
                _ishMetadataField = new IshMetadataField(Name, Level, ValueType, Value);
                if (IshField != null)
                {
                    foreach (IshField ishField in IshField)
                    {
                        _incomingIshField.Add(ishField);
                    }
                }
                else if (IshObject != null)
                {
                    foreach (IshObject ishObject in IshObject)
                    {
                        ishObject.IshFields.AddOrUpdateField(_ishMetadataField, Enumerations.ActionMode.Update);
                        WriteObject(ishObject, true);
                    }
                }
                else if (IshFolder != null)
                {
                    foreach (IshFolder ishFolder in IshFolder)
                    {
                        ishFolder.IshFields.AddOrUpdateField(_ishMetadataField, Enumerations.ActionMode.Update);
                        WriteObject(ishFolder, true);
                    }
                }
                else if (IshEvent != null)
                {
                    foreach (IshEvent ishEvent in IshEvent)
                    {
                        ishEvent.IshFields.AddOrUpdateField(_ishMetadataField, Enumerations.ActionMode.Update);
                        WriteObject(ishEvent, true);
                    }
                }
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

        protected override void EndProcessing()
        {
            try
            {
                if (IshObject == null && IshFolder == null && IshEvent == null)
                {
                    IshFields ishFields = new IshFields(_incomingIshField);
                    ishFields.AddOrUpdateField(_ishMetadataField, Enumerations.ActionMode.Update);
                    WriteObject(ishFields.Fields(), true);
                }
            }
            catch (TrisoftAutomationException trisoftAutomationException)
            {
                ThrowTerminatingError(new ErrorRecord(trisoftAutomationException, base.GetType().Name, ErrorCategory.InvalidOperation, null));
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
