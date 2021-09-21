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
using Trisoft.ISHRemote.HelperClasses;

namespace Trisoft.ISHRemote.Cmdlets.User
{
    /// <summary>
    /// <para type="synopsis">The Add-IshUser cmdlet adds the new users that are passed through the pipeline or determined via provided parameters</para>
    /// <para type="description">The Add-IshUser cmdlet adds the new users that are passed through the pipeline or determined via provided parameters</para>
    /// </summary>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/InfoShareWS/" -IshUserName "username" -IshUserPassword  "userpassword"
    /// $metadata = Set-IshMetadataField -IshSession $ishSession -Name FISHUSERLANGUAGE -Level None -ValueType Element -Value "VLANGUAGEEN" |
    ///             Set-IshMetadataField -IshSession $ishSession -Name FUSERGROUP -Level None -ValueType Element -Value "VUSERGROUPDEFAULTDEPARTMENT" |
    ///             Set-IshMetadataField -IshSession $ishSession -Name PASSWORD -Level None -Value "SomethingSecret"
    /// $ishObject = Add-IshUser -Name "NewUserName" -Metadata $metadata
    /// </code>
    /// <para>New-IshSession will submit into SessionState, so it can be reused by this cmdlet. Adding a user</para>
    /// </example>
    [Cmdlet(VerbsCommon.Add, "IshUser", SupportsShouldProcess = true)]
    [OutputType(typeof(IshUser))]
    public sealed class AddIshUser : UserCmdlet
    {
        /// <summary>
        /// <para type="description">The IshSession variable holds the authentication and contract information. This object can be initialized using the New-IshSession cmdlet.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshObjectGroup")]
        [ValidateNotNullOrEmpty]
        public IshSession IshSession { get; set; }

        /// <summary>
        /// <para type="description">Array with the users to create. This array can be passed through the pipeline or explicitly passed via the parameter.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "IshObjectGroup")]
        [AllowEmptyCollection]
        public IshObject[] IshObject { get; set; }

        /// <summary>
        /// <para type="description">The name of the new user.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup"), ValidateNotNullOrEmpty]
        public string Name { get; set; }

        /// <summary>
        /// <para type="description">The metadata to set for the new user.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup"), ValidateNotNull]
        public IshField[] Metadata { get; set; }

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
                List<IshObject> returnedObjects = new List<IshObject>();

                if (IshObject != null && IshObject.Length == 0)
                {
                    // Do nothing
                    WriteVerbose("IshObject is empty, so nothing to create");
                    WriteVerbose("IshObject is empty, so nothing to retrieve");
                }
                else
                {
                    List<string> returnUsers = new List<string>();
                    IshFields returnFields;

                    // 1. Doing the update
                    WriteDebug("Adding");

                    if (IshObject != null)
                    {
                        // 1b. Using IshObject[] pipeline or specificly set
                        int current = 0;
                        foreach (IshObject ishObject in IshObject)
                        {
                            // The USERNAME field needs to be present, all other mandatory fields need to be present as well
                            IshMetadataField userNameValueField =
                                (IshMetadataField)
                                    ishObject.IshFields.Retrieve("USERNAME", Enumerations.Level.None,
                                        Enumerations.ValueType.Value)[0];
                            string userName = userNameValueField.Value;
                            WriteDebug($"UserName[{userName}] Metadata.length[{ishObject.IshFields.ToXml().Length}] {++current}/{IshObject.Length}");
                            var metadata = IshSession.IshTypeFieldSetup.ToIshMetadataFields(ISHType, ishObject.IshFields, Enumerations.ActionMode.Create);
                            if (ShouldProcess(userName))
                            {
                                var response = IshSession.User25.Create(new User25ServiceReference.CreateRequest()
                                { 
                                    psAuthContext = IshSession.AuthenticationContext,
                                    psUserName = userName,
                                    psXMLMetaData = metadata.ToXml()
                                });
                                IshSession.AuthenticationContext = response.psAuthContext;
                                string userId = response.psOutUserId;
                                returnUsers.Add(userId);
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
                        WriteVerbose("Name[" + Name + "] metadata.length[" + metadata.ToXml().Length + "]");
                        if (ShouldProcess(Name))
                        {
                            var response = IshSession.User25.Create(new User25ServiceReference.CreateRequest()
                            {
                                psAuthContext = IshSession.AuthenticationContext,
                                psUserName = Name,
                                psXMLMetaData = metadata.ToXml()
                            });
                            IshSession.AuthenticationContext = response.psAuthContext;
                            string userId = response.psOutUserId;
                            returnUsers.Add(userId);
                        }
                        returnFields = metadata;
                    }

                    // 2. Retrieve the updated material from the database and write it out
                    WriteDebug("Retrieving");

                    // 2a. Prepare list of usergroupids and requestedmetadata
                    // 2b. Retrieve the material
                    // Remove Password field explicitly, as we are not allowe to read it
                    returnFields.RemoveField(FieldElements.Password, Enumerations.Level.None, Enumerations.ValueType.All);
                    // Add the required fields (needed for pipe operations)
                    IshFields requestedMetadata = IshSession.IshTypeFieldSetup.ToIshRequestedMetadataFields(IshSession.DefaultRequestedMetadata, ISHType, returnFields, Enumerations.ActionMode.Read);
                    var responseRetrieve = IshSession.User25.RetrieveMetaData(new User25ServiceReference.RetrieveMetaDataRequest()
                    {
                        psAuthContext = IshSession.AuthenticationContext,
                        pasUserIds = returnUsers.ToArray(),
                        peActivityFilter = User25ServiceReference.eActivityFilter.None,
                        psXMLMetadataFilter = "",
                        psXMLRequestedMetaData = requestedMetadata.ToXml()
                    }); 
                    IshSession.AuthenticationContext = responseRetrieve.psAuthContext;
                    string xmlIshObjects = responseRetrieve.psOutXMLObjList;                    
                    returnedObjects.AddRange(new IshObjects(ISHType, xmlIshObjects).Objects);
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
