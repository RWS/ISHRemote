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

namespace Trisoft.ISHRemote.Cmdlets.UserRole
{
    /// <summary>
    /// <para type="synopsis">The Add-IshUserRole cmdlet adds the new user roles that are passed through the pipeline or determined via provided parameters</para>
    /// <para type="description">The Add-IshUserRole cmdlet adds the new user roles that are passed through the pipeline or determined via provided parameters</para>
    /// </summary>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/InfoShareWS/" -PSCredential Admin
    /// $metadata = Set-IshMetadataField -Name "FDESCRIPTION" -Level None -Value "Description of $userRoleName"
    /// $ishObject = Add-IshUserRole -Name $userRoleName -Metadata $metadata
    /// </code>
    /// <para>New-IshSession will submit into SessionState, so it can be reused by this cmdlet. Adding a user role.</para>
    /// </example>
    [Cmdlet(VerbsCommon.Add, "IshUserRole", SupportsShouldProcess = true)]
    [OutputType(typeof(IshObject))]
    public sealed class AddIshUserRole : UserRoleCmdlet
    {
        /// <summary>
        /// <para type="description">The IshSession variable holds the authentication and contract information. This object can be initialized using the New-IshSession cmdlet.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshObjectsGroup")]
        [ValidateNotNullOrEmpty]
        public IshSession IshSession { get; set; }

        /// <summary>
        /// <para type="description">Array with the user roles to create. This array can be passed through the pipeline or explicitly passed via the parameter.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "IshObjectsGroup")]
        [AllowEmptyCollection]
        public IshObject[] IshObject { get; set; }

        /// <summary>
        /// <para type="description">The name of the new user role.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup"), ValidateNotNullOrEmpty]
        public string Name { get; set; }

        /// <summary>
        /// <para type="description">The metadata to set for the new user role.</para>
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
                    List<string> returnUserRoles = new List<string>();
                    IshFields returnFields;

                    // 1. Doing the update
                    WriteDebug("Adding");

                    if (IshObject != null)
                    {
                        int current = 0;
                        // 1b. Using IshObject[] pipeline or specificly set
                        foreach (IshObject ishObject in IshObject)
                        {
                            IshMetadataField userRoleNameValueField =
                                (IshMetadataField)
                                    ishObject.IshFields.RetrieveFirst(FieldElements.UserRoleName, Enumerations.Level.None,
                                        Enumerations.ValueType.Value);
                            string userRoleName = userRoleNameValueField.Value;
                            WriteDebug($"UserRoleName[{userRoleName}] Metadata.length[{ishObject.IshFields.ToXml().Length}] {++current}/{IshObject.Length}");
                            var metadata = IshSession.IshTypeFieldSetup.ToIshMetadataFields(ISHType, ishObject.IshFields, Enumerations.ActionMode.Create);
                            if (ShouldProcess(userRoleName))
                            {
                                var userRoleId = IshSession.UserRole25.Create(
                                    userRoleName,
                                    metadata.ToXml());
                                returnUserRoles.Add(userRoleId);
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
                            var userRoleId = IshSession.UserRole25.Create(
                                Name,
                                metadata.ToXml());
                            returnUserRoles.Add(userRoleId);
                        }
                        returnFields = metadata;
                    }

                    // 2. Retrieve the updated material from the database and write it out
                    WriteDebug("Retrieving");
                    // 2a. Prepare list of usergroupids and requestedmetadata
                    // 2b. Retrieve the material

                    // Add the required fields (needed for pipe operations)
                    IshFields requestedMetadata = IshSession.IshTypeFieldSetup.ToIshRequestedMetadataFields(IshSession.DefaultRequestedMetadata, ISHType, returnFields, Enumerations.ActionMode.Read);
                    string xmlIshObjects = IshSession.UserRole25.RetrieveMetadata(
                        returnUserRoles.ToArray(),
                        UserRole25ServiceReference.ActivityFilter.None,
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
