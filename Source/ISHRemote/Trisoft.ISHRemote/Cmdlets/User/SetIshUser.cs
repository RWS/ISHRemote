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
    /// <para type="synopsis">The Set-IshUser cmdlet updates the users that are passed through the pipeline or determined via provided parameters.</para>
    /// <para type="description">The Set-IshUser cmdlet updates the users that are passed through the pipeline or determined via provided parameters.</para>
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "IshUser", SupportsShouldProcess = true)]
    [OutputType(typeof(IshObject))]
    public sealed class SetIshUser : UserCmdlet
    {
        /// <summary>
        /// <para type="description">The IshSession variable holds the authentication and contract information. This object can be initialized using the New-IshSession cmdlet.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshObjectsGroup")]
        [ValidateNotNullOrEmpty]
        public IshSession IshSession { get; set; }

        /// <summary>
        /// <para type="description">The identifier of the user to update</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup"), ValidateNotNullOrEmpty]
        public string Id { get; set; }

        /// <summary>
        /// <para type="description">The metadata to set for the user</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup"), ValidateNotNull]
        public IshField[] Metadata { get; set; }

        /// <summary>
        /// <para type="description">Users for which to update the metadata. This array can be passed through the pipeline or explicitly passed via the parameter.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "IshObjectsGroup")]
        [AllowEmptyCollection]
        public IshObject[] IshObject { get; set; }

        protected override void ProcessRecord()
        {

            try
            {
                List<IshObject> returnedObjects = new List<IshObject>();

                if (IshObject != null && IshObject.Length == 0)
                {
                    // Do nothing
                    WriteVerbose("IshObject is empty, so nothing to update");
                    WriteVerbose("IshObject is empty, so nothing to retrieve");
                }
                else
                {
                    List<string> returnUsers = new List<string>();
                    IshFields returnFields;

                    // 1. Doing the update
                    WriteDebug("Updating");
                    if (IshObject != null)
                    {
                        // 1b. Using IshObject[] pipeline or specificly set
                        int current = 0;
                        foreach (IshObject ishObject in IshObject)
                        {
                            WriteDebug($"Id[{ishObject.IshRef}] Metadata.length[{ishObject.IshFields.ToXml().Length}] {++current}/{IshObject.Length}");
                            var metadata = RemoveSystemFields(ishObject.IshFields, Enumerations.ActionMode.Update);
                            if (ShouldProcess(ishObject.IshRef))
                            {
                                IshSession.User25.Update(
                                    ishObject.IshRef,
                                    metadata.ToXml());
                            }
                            returnUsers.Add(ishObject.IshRef);
                        }
                        returnFields = (IshObject[0] == null)
                            ? new IshFields()
                            : IshObject[0].IshFields.ToRequestedFields();
                    }
                    else
                    {
                        // 1a. Using Id and Metadata
                        var metadata = RemoveSystemFields(new IshFields(Metadata), Enumerations.ActionMode.Update);
                        WriteDebug($"Id[{Id}] metadata.length[{metadata.ToXml().Length}]");
                        if (ShouldProcess(Id))
                        {
                            IshSession.User25.Update(
                                Id,
                                metadata.ToXml());
                        }
                        returnUsers.Add(Id);
                        returnFields = metadata.ToRequestedFields();
                    }

                    // 2. Retrieve the updated material from the database and write it out
                    WriteDebug("Retrieving");

                    // 2a. Prepare list of usergroupids and requestedmetadata
                    // 2b. Retrieve the material
                    // Remove Password field explicitly, as we are not allowed to read it
                    returnFields.RemoveField(FieldElements.Password, Enumerations.Level.None, Enumerations.ValueType.All);
                    // Add the required fields (needed for pipe operations)
                    IshFields requestedMetadata = AddRequiredFields(returnFields.ToRequestedFields());
                    string xmlIshObjects = IshSession.User25.RetrieveMetadata(
                        returnUsers.ToArray(),
                        User25ServiceReference.ActivityFilter.None,
                        "",
                        requestedMetadata.ToXml());
                    
                    returnedObjects.AddRange(new IshObjects(xmlIshObjects).Objects);
                }

                // 3. Write it
                WriteVerbose("returned object count[" + returnedObjects.Count + "]");
                WriteObject(returnedObjects, true);
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
