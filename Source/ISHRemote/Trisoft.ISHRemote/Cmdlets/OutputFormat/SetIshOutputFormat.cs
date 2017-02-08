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
using System.Text;
using System.Management.Automation;
using Trisoft.ISHRemote.Objects;
using Trisoft.ISHRemote.Objects.Public;
using Trisoft.ISHRemote.Exceptions;

namespace Trisoft.ISHRemote.Cmdlets.OutputFormat
{
    /// <summary>
    /// <para type="synopsis">The Set-IshOutputFormat cmdlet updates the output formats that are passed through the pipeline or determined via provided parameters</para>
    /// <para type="description">The Set-IshOutputFormat cmdlet updates the output formats that are passed through the pipeline or determined via provided parameters</para>
    /// </summary>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/InfoShareWS/" -IshUserName "username" -IshUserPassword  "userpassword"
    /// $metadataUpdate = Set-IshMetadataField -IshSession $ishSession -Name "FISHOUTPUTFORMATNAME" -Level "none" -Value "PDF (A4 Manual) updated"
    /// $outputFormatUpdate = Set-IshOutputFormat -IshSession $ishSession `
    /// -Id "GUID-2A69335D-F025-4963-A142-5E49988C7C0C" `
    /// -Edt "EDTPDF" `
    /// -Metadata $metadataUpdate
    /// </code>
    /// <para>Update name of the existing Output format</para>
    /// </example>
    [Cmdlet(VerbsCommon.Set, "IshOutputFormat", SupportsShouldProcess = true)]
    [OutputType(typeof(IshObject))]
    public sealed class SetIshOutputFormat : OutputFormatCmdlet
    {

        /// <summary>
        /// <para type="description">The IshSession variable holds the authentication and contract information. This object can be initialized using the New-IshSession cmdlet.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshObjectsGroup")]
        [ValidateNotNullOrEmpty]
        public IshSession IshSession { get; set; }

        /// <summary>
        /// <para type="description">The element name of the output format</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup"), ValidateNotNullOrEmpty]
        public string Id { get; set; }

        /// <summary>
        /// <para type="description">The unique identifier of the Electronic Document Type of the output (e.g. EDTPDF, EDTXML, EDTHTML,...)</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup"), ValidateNotNullOrEmpty]
        public string Edt { get; set; }

        /// <summary>
        /// <para type="description">The metadata to set for the output format</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]
        [ValidateNotNull]
        public IshField[] Metadata { get; set; }

        /// <summary>
        /// <para type="description">Output formats for which to update the metadata. This array can be passed through the pipeline or explicitly passed via the parameter.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "IshObjectsGroup")]
        [AllowEmptyCollection]
        public IshObject[] IshObject { get; set; }

        /// <summary>
        /// Process the Set-IshOutputFormat commandlet.
        /// </summary>
        /// <exception cref="TrisoftAutomationException"></exception>
        /// <exception cref="Exception"></exception>
        /// <remarks>Writes an <see cref="Objects.IshObject"/> array to the pipeline.</remarks>
        protected override void ProcessRecord()
        {

            try
            {
                // 1. Validating the input
                WriteDebug("Validating");

                List<IshObject> returnedObjects = new List<IshObject>();

                if (IshObject != null && IshObject.Length == 0)
                {
                    // Do nothing
                    WriteVerbose("IshObject is empty, so nothing to set");
                }
                else
                {
                    // Updated OutputFormat Ids
                    List<string> outputFormatIdsToRetrieve = new List<string>();
                    IshFields returnFields;

                    // 2. Doing Set
                    WriteDebug("Updating");

                    // 2a. Set using provided parameters (not piped IshObject)
                    if (IshObject != null)
                    {
                        // 2b. Set using IshObject pipeline. 
                        IshObjects ishObjects = new IshObjects(IshObject);
                        int current = 0;
                        foreach (IshObject ishObject in ishObjects.Objects)
                        {
                            WriteDebug($"Id[{ishObject.IshRef}] {++current}/{IshObject.Length}");
                            string edt = ishObject.IshFields.GetFieldValue("FISHOUTPUTEDT", Enumerations.Level.None,
                                Enumerations.ValueType.Element);
                            var metadata = ishObject.IshFields;
                            metadata = RemoveSystemFields(metadata, Enumerations.ActionMode.Update);
                            if (ShouldProcess(ishObject.IshRef))
                            {
                                IshSession.OutputFormat25.Update(
                                    ishObject.IshRef,
                                    edt,
                                    metadata.ToXml());
                                outputFormatIdsToRetrieve.Add(ishObject.IshRef);
                            }
                        }
                        returnFields = (IshObject[0] == null)
                            ? new IshFields()
                            : IshObject[0].IshFields.ToRequestedFields();
                    }
                    else
                    {
                        var metadata = RemoveSystemFields(new IshFields(Metadata), Enumerations.ActionMode.Update);
                        if (ShouldProcess(Id))
                        {
                            IshSession.OutputFormat25.Update(
                                Id,
                                Edt,
                                metadata.ToXml());
                            outputFormatIdsToRetrieve.Add(Id);
                        }
                        returnFields = metadata.ToRequestedFields();
                    }

                    // 3a. Retrieve updated OutputFormat(s) from the database and write them out
                    WriteDebug("Retrieving");

                    // Remove FISHDITADLVRCLIENTSECRET field explicitly, as we are not allowed to read it
                    returnFields.RemoveField(FieldElements.DitaDeliveryClientSecret, Enumerations.Level.None, Enumerations.ValueType.All);

                    // Add the required fields (needed for pipe operations)
                    IshFields requestedMetadata = AddRequiredFields(returnFields);
                    string xmlIshObjects = IshSession.OutputFormat25.RetrieveMetadata(
                        outputFormatIdsToRetrieve.ToArray(),
                        OutputFormat25ServiceReference.ActivityFilter.None,
                        "",
                        requestedMetadata.ToXml());
                    
                    returnedObjects.AddRange(new IshObjects(xmlIshObjects).Objects);
                }

                // 3b. Write it
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
