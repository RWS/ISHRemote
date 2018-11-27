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
using System.Linq;
using System.Management.Automation;
using Trisoft.ISHRemote.Objects;
using Trisoft.ISHRemote.Objects.Public;
using Trisoft.ISHRemote.Exceptions;

namespace Trisoft.ISHRemote.Cmdlets.UserGroup
{
    /// <summary>
    /// <para type="synopsis">The Add-IshUserGroup cmdlet adds the new user groups that are passed through the pipeline or determined via provided parameters</para>
    /// <para type="description">The Add-IshUserGroup cmdlet adds the new user groups that are passed through the pipeline or determined via provided parameters</para>
    /// </summary>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/InfoShareWS/" -PSCredential Admin
    /// $metadata = Set-IshMetadataField -IshSession $ishSession -Name "FDESCRIPTION" -Level None -Value "Description of $userGroupName"
    /// $ishObject = Add-IshUserGroup -Name $userGroupName -Metadata $metadata
    /// </code>
    /// <para>New-IshSession will submit into SessionState, so it can be reused by this cmdlet. Adding a user group.</para>
    /// </example>
    [Cmdlet(VerbsCommon.Add, "IshUserGroup", SupportsShouldProcess = true)]
    [OutputType(typeof(IshObject))]
    public sealed class AddIshUserGroup : UserGroupCmdlet
    {
        /// <summary>
        /// <para type="description">The IshSession variable holds the authentication and contract information. This object can be initialized using the New-IshSession cmdlet.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshObjectsGroup")]
        [ValidateNotNullOrEmpty]
        public IshSession IshSession { get; set; }

        /// <summary>
        /// <para type="description">Array with the user groups to create. This array can be passed through the pipeline or explicitly passed via the parameter.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "IshObjectsGroup")]
        [AllowEmptyCollection]
        public IshObject[] IshObject { get; set; }

        /// <summary>
        /// <para type="description">The name of the new user group.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup"), ValidateNotNullOrEmpty]
        public string Name { get; set; }

        /// <summary>
        /// <para type="description">The metadata to set on the new user group.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup"), ValidateNotNull]
        public IshField[] Metadata { get; set; }

        protected override void BeginProcessing()
        {
            if (IshSession == null) { IshSession = (IshSession)SessionState.PSVariable.GetValue(ISHRemoteSessionStateIshSession); }
            if (IshSession == null) { throw new ArgumentNullException(ISHRemoteSessionStateIshSessionException); }
            WriteDebug($"Using IshSession[{IshSession.Name}] from SessionState.{ISHRemoteSessionStateIshSession}");
            base.BeginProcessing();
        }

        protected override void ProcessRecord()
        {
            try
            {
                List<IshObject> returnedObjects = new List<IshObject>();

                if (IshObject != null && IshObject.Length == 0)
                {
                    // Do nothing
                    WriteVerbose("IshObject is empty, so nothing to create");
                    WriteVerbose("IshObject is empty, so nothing to retrieve");
                }
                else
                {
                    List<string> returnUserGroups = new List<string>();
                    IshFields returnFields;

                    // 1. Doing the update
                    WriteDebug("Adding");

                    if (IshObject != null)
                    {
                        int current = 0;
                        // 1b. Using IshObject[] pipeline or specificly set
                        foreach (IshObject ishObject in IshObject)
                        {
                            IshMetadataField userGroupNameValueField =
                                (IshMetadataField)
                                    ishObject.IshFields.Retrieve("FISHUSERGROUPNAME", Enumerations.Level.None,
                                        Enumerations.ValueType.Value)[0];
                            string userGroupName = userGroupNameValueField.Value;
                            WriteDebug($"UserGroupName[{userGroupName}] Metadata.length[{ishObject.IshFields.ToXml().Length}] {++current}/{IshObject.Length}");
                            var metadata = IshSession.IshTypeFieldSetup.ToIshMetadataFields(ISHType, ishObject.IshFields, Enumerations.ActionMode.Create);
                            if (ShouldProcess(userGroupName))
                            {
                                var userGroupId = IshSession.UserGroup25.Create(
                                    userGroupName,
                                    metadata.ToXml());
                                returnUserGroups.Add(userGroupId);
                            }
                        }
                        returnFields = (IshObject[0] == null)
                            ? new IshFields()
                            : IshObject[0].IshFields;
                    }
                    else
                    {
                        // 1a. Using Id and Metadata
                        var metadata = IshSession.IshTypeFieldSetup.ToIshMetadataFields(ISHType, new IshFields(Metadata), Enumerations.ActionMode.Create);
                        WriteVerbose("Id[" + Name + "] metadata.length[" + metadata.ToXml().Length + "]");
                        if (ShouldProcess(Name))
                        {
                            var userGroupId = IshSession.UserGroup25.Create(
                                Name,
                                metadata.ToXml());
                            returnUserGroups.Add(userGroupId);
                        }
                        returnFields = metadata;
                    }

                    // 2. Retrieve the updated material from the database and write it out
                    WriteDebug("Retrieving");
                    // 2a. Prepare list of usergroupids and requestedmetadata
                    // 2b. Retrieve the material

                    // Add the required fields (needed for pipe operations)
                    IshFields requestedMetadata = IshSession.IshTypeFieldSetup.ToIshRequestedMetadataFields(IshSession.DefaultRequestedMetadata, ISHType, returnFields, Enumerations.ActionMode.Read);
                    string xmlIshObjects = IshSession.UserGroup25.RetrieveMetadata(
                        returnUserGroups.ToArray(),
                        UserGroup25ServiceReference.ActivityFilter.None,
                        "",
                        requestedMetadata.ToXml());
                    
                    returnedObjects.AddRange(new IshObjects(xmlIshObjects).Objects);
                }

                // 3. Write it
                WriteVerbose("returned object count[" + returnedObjects.Count + "]");
                WriteObject(IshSession, ISHType, returnedObjects.ConvertAll(x => (IshBaseObject)x), true);
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
