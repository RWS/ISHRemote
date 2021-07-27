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
using System.Collections.Generic;
using System.Linq;
using System.Management.Automation;
using Trisoft.ISHRemote.Objects;
using Trisoft.ISHRemote.Objects.Public;
using Trisoft.ISHRemote.Exceptions;
using Trisoft.ISHRemote.HelperClasses;

namespace Trisoft.ISHRemote.Cmdlets.OutputFormat
{
    /// <summary>
    /// <para type="synopsis">The Add-IshOutputFormat cmdlet adds the new output formats that are passed through the pipeline or determined via provided parameters</para>
    /// <para type="description">The Add-IshOutputFormat cmdlet adds the new output formats that are passed through the pipeline or determined via provided parameters</para>
    /// </summary>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/InfoShareWS/" -IshUserName "username" -IshUserPassword  "userpassword"
    /// $metadata = Set-IshMetadataField -IshSession $ishSession -Name "FISHRESOLUTIONS" -Level "none" -Value "Low"  |
    ///             Set-IshMetadataField -IshSession $ishSession -Name "FISHSINGLEFILE" -Level "none" -Value "TRUE" -ValueType Element |
    ///             Set-IshMetadataField -IshSession $ishSession -Name "FISHCLEANUP" -Level "none" -Value "TRUE" -ValueType Element |
    ///             Set-IshMetadataField -IshSession $ishSession -Name "FISHKEEPDTDSYSTEMID" -Level "none" -Value "TRUE" -ValueType Element |
    ///             Set-IshMetadataField -IshSession $ishSession -Name "FISHPUBRESOLVEVARIABLES" -Level "none" -Value "TRUE" -ValueType Element
    /// $outputFormat = Add-IshOutputFormat `
    /// -Name "MyOutputFormat" `
    /// -EDT "EDTPDF" `
    /// -Metadata $metadata
    /// </code>
    /// <para>New-IshSession will submit into SessionState, so it can be reused by this cmdlet. Adding a new output format</para>
    /// </example>
    [Cmdlet(VerbsCommon.Add, "IshOutputFormat", SupportsShouldProcess = true)]
    [OutputType(typeof(IshOutputFormat))]
    public sealed class AddIshOutputFormat : OutputFormatCmdlet
    {

        /// <summary>
        /// <para type="description">The IshSession variable holds the authentication and contract information. This object can be initialized using the New-IshSession cmdlet.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshObjectsGroup")]
        [ValidateNotNullOrEmpty]
        public IshSession IshSession { get; set; }

        /// <summary>
        /// <para type="description">Name for the new OutputFormat</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup"), ValidateNotNullOrEmpty]
        public string Name { get; set; }

        /// <summary>
        /// <para type="description">EDT for the new OutputFormat</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup"), ValidateNotNullOrEmpty]
        public string Edt { get; set; }

        /// <summary>
        /// <para type="description">MetaData - xml structure that will be used for the new OutputFormat</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup"), ValidateNotNullOrEmpty]
        public IshField[] Metadata { get; set; }

        /// <summary>
        /// <para type="description">IshObject - contains OutputFormat data that needs to be created. Pipeline</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "IshObjectsGroup")]
        [AllowEmptyCollection]
        public IshObject[] IshObject { get; set; }

        protected override void BeginProcessing()
        {
            if (IshSession == null) { IshSession = (IshSession)SessionState.PSVariable.GetValue(ISHRemoteSessionStateIshSession); }
            if (IshSession == null) { throw new ArgumentException(ISHRemoteSessionStateIshSessionException); }
            WriteDebug($"Using IshSession[{IshSession.Name}] from SessionState.{ISHRemoteSessionStateIshSession}");
            base.BeginProcessing();
        }

        /// <summary>
        /// Process the Add-IshOutputFormat commandlet.
        /// </summary>
        /// <exception cref="TrisoftAutomationException"></exception>
        /// <exception cref="Exception"></exception>
        /// <remarks>Writes an <see cref="Objects.Public.IshObject"/> array to the pipeline</remarks>
        protected override void ProcessRecord()
        {
            try
            {
                // 1. Validating the input
                WriteDebug("Validating");

                List<IshObject> returnIshObjects = new List<IshObject>();      
                                              
                if (IshObject != null && IshObject.Length == 0)
                {
                    // Do nothing
                    WriteVerbose("IshObject is empty, so nothing to add");
                }
                else
                {
                    WriteDebug("Adding");

                    // List of objects to write to the output in the end
                    var outputFormatIds = new List<string>();
                    IshFields returnFields;

                    if (IshObject != null)
                    {
                        // 2b. Add using IshObject[] pipeline                    
                        int current = 0;
                        IshObjects ishObjects = new IshObjects(IshObject);
                        foreach (IshObject ishObject in ishObjects.Objects)
                        {
                            // Get values
                            WriteDebug($"Id[{ishObject.IshRef}] {++current}/{IshObject.Length}");
                            var nameMetadataField = ishObject.IshFields.RetrieveFirst("FISHOUTPUTFORMATNAME", Enumerations.Level.None).ToMetadataField() as IshMetadataField;
                            string name = nameMetadataField.Value;
                            var edtMetadataField = ishObject.IshFields.RetrieveFirst("FISHOUTPUTEDT", Enumerations.Level.None).ToMetadataField() as IshMetadataField;
                            string edt = edtMetadataField.Value;
                            var metadata = IshSession.IshTypeFieldSetup.ToIshMetadataFields(ISHType, ishObject.IshFields, Enumerations.ActionMode.Create);
                            if (ShouldProcess(name))
                            {
                                string outputFormatId = IshSession.OutputFormat25.Create(
                                    name,
                                    edt,
                                    metadata.ToXml());
                                outputFormatIds.Add(outputFormatId);
                            }
                        }
                        returnFields = (IshObject[0] == null)
                            ? new IshFields()
                            : IshObject[0].IshFields;
                    }
                    else
                    {
                        // 2a. Add using provided parameters
                        var metadata = IshSession.IshTypeFieldSetup.ToIshMetadataFields(ISHType, new IshFields(Metadata), Enumerations.ActionMode.Create);
                        string edt = Edt ?? "";
                        WriteDebug($"Name[{Name}] EDT[{edt}] Metadata.Length[{metadata.ToXml().Length}]");
                        if (ShouldProcess(Name))
                        {
                            string outputFormatId = IshSession.OutputFormat25.Create(
                                Name,
                                edt,
                                metadata.ToXml());
                            outputFormatIds.Add(outputFormatId);
                        }
                        returnFields = metadata;
                    }

                    // 3a. Retrieve added OutputFormat and write it out
                    WriteDebug("Retrieving");
                    // Add the required fields (needed for pipe operations)
                    IshFields requestedMetadata = IshSession.IshTypeFieldSetup.ToIshRequestedMetadataFields(IshSession.DefaultRequestedMetadata, ISHType, returnFields, Enumerations.ActionMode.Read);
                    string xmlIshObjects = IshSession.OutputFormat25.RetrieveMetadata(
                        outputFormatIds.ToArray(),
                        OutputFormat25ServiceReference.ActivityFilter.None,
                        "",
                        requestedMetadata.ToXml());

                    returnIshObjects.AddRange(new IshObjects(ISHType, xmlIshObjects).Objects);
                }

                WriteVerbose("returned object count[" + returnIshObjects.Count + "]");
                WriteObject(IshSession, ISHType, returnIshObjects.ConvertAll(x => (IshBaseObject)x), true);
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
