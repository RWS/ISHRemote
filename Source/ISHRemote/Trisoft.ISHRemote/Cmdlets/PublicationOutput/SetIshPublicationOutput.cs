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
using Trisoft.ISHRemote.Objects;
using Trisoft.ISHRemote.Objects.Public;
using Trisoft.ISHRemote.Exceptions;
using Trisoft.ISHRemote.PublicationOutput25ServiceReference;

namespace Trisoft.ISHRemote.Cmdlets.PublicationOutput
{
    /// <summary>
    /// <para type="synopsis">The Set-IshPublicationOutput cmdlet updates the publication outputs that are passed through the pipeline or determined via provided parameters</para>
    /// <para type="description">The Set-IshPublicationOutput cmdlet updates the publication outputs that are passed through the pipeline or determined via provided parameters</para>
    /// </summary>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/InfoShareWS/" -IshUserName "username" -IshUserPassword  "userpassword"
    /// # Update a publication output
    /// $metadataUpdate = Set-IshMetadataField -IshSession $ishSession -Name 'FISHFALLBACKLNGDEFAULT' -Level 'lng' -Value 'bg' |
    ///                   Set-IshMetadataField -IshSession $ishSession -Name 'FISHFALLBACKLNGIMAGES' -Level 'lng' -Value 'bg' |
    ///                   Set-IshMetadataField -IshSession $ishSession -Name 'FISHFALLBACKLNGRESOURCES' -Level 'lng' -Value 'bg'
    /// Set-IshPublicationOutput -IshSession $ishSession `
    /// -LogicalId "GUID-F66C1BB5-076D-455C-B055-DAC5D61AB3D9" `
    /// -Version "1" `
    /// -OutputFormat "GUID-2A69335D-F025-4963-A142-5E49988C7C0C" `
    /// -LanguageCombination  "en" `
    /// -MetaData $metadataUpdate
    /// </code>
    /// <para>Updating publication outputs</para>
    /// </example>
    [Cmdlet(VerbsCommon.Set, "IshPublicationOutput", SupportsShouldProcess = true)]
    [OutputType(typeof(IshObject))]
    public sealed class SetIshPublicationOutput : PublicationOutputCmdlet
    {

        /// <summary>
        /// <para type="description">The IshSession variable holds the authentication and contract information. This object can be initialized using the New-IshSession cmdlet.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshObjectsGroup")]
        [ValidateNotNullOrEmpty]
        public IshSession IshSession { get; set; }

        /// <summary>
        /// <para type="description">Publication outputs for which to update the metadata. This array can be passed through the pipeline or explicitly passed via the parameter.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "IshObjectsGroup")]
        [AllowEmptyCollection]
        public IshObject[] IshObject { get; set; }

        /// <summary>
        /// <para type="description">The LogicalId of the publication output.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup"), ValidateNotNullOrEmpty]
        public string LogicalId { get; set; }

        /// <summary>
        /// <para type="description">The version of the publication output.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup"), ValidateNotNullOrEmpty]
        public string Version { get; set; }

        /// <summary>
        /// <para type="description">The output format of the publication output.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup"), ValidateNotNullOrEmpty]
        public string OutputFormat { get; set; }

        /// <summary>
        /// <para type="description">The language combination of the publication output.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup"), ValidateNotNullOrEmpty]
        public string LanguageCombination { get; set; }

        /// <summary>
        /// <para type="description">The metadata to set for the publication output object.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]
        [ValidateNotNull]
        public IshField[] Metadata { get; set; }

        /// <summary>
        /// <para type="description">The required current metadata of the publication output. This parameter can be used to avoid that users override changes done by other users. The cmdlet will check whether the fields provided in this parameter still have the same values in the repository:</para>
        /// <para type="description">If the metadata is still the same, the metadata will be set.</para>
        /// <para type="description">If the metadata is not the same anymore, an error is given and the metadata will be set.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshObjectsGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]
        [ValidateNotNullOrEmpty]
        public IshField[] RequiredCurrentMetadata { get; set; }


        /// <summary>
        /// Process the Set-IshPublicationOutput commandlet.
        /// </summary>
        /// <exception cref="TrisoftAutomationException"></exception>
        /// <exception cref="Exception"></exception>
        /// <remarks>Writes an <see cref="Objects.Public.IshObject"/> array to the pipeline.</remarks>
        protected override void ProcessRecord()
        {
            try
            {
                List<IshObject> returnedObjects = new List<IshObject>();

                if (IshObject != null && IshObject.Length == 0)
                {
                    // Do nothing
                    WriteVerbose("IshObject is empty, so nothing to update");
                }
                else
                {
                    IshFields requiredCurrentMetadata = new IshFields(RequiredCurrentMetadata);                    

                    WriteDebug("Updating");

                    if (IshObject != null)
                    {
                        // Using the pipeline
                        int current = 0;
                        List<long> lngCardIds = new List<long>();
                        IshObjects ishObjects = new IshObjects(IshObject);
                        foreach (IshObject ishObject in ishObjects.Objects)
                        {
                            WriteDebug($"Id[{ishObject.IshRef}] {++current}/{IshObject.Length}");
                            // Get language ref
                            long lngRef = Convert.ToInt64(ishObject.ObjectRef[Enumerations.ReferenceType.Lng]);
                            var metadata = IshSession.IshTypeFieldSetup.ToIshMetadataFields(ISHType, ishObject.IshFields, Enumerations.ActionMode.Update);
                            if (ShouldProcess(Convert.ToString(lngRef)))
                            {
                                IshSession.PublicationOutput25.SetMetadataByIshLngRef(
                                lngRef,
                                metadata.ToXml(),
                                requiredCurrentMetadata.ToXml());
                            }
                            lngCardIds.Add(lngRef);
                        }
                        var returnFields = (IshObject[0] == null)
                            ? new IshFields()
                            : IshObject[0].IshFields;
                        IshFields requestedMetadata = IshSession.IshTypeFieldSetup.ToIshRequestedMetadataFields(ISHType, returnFields, Enumerations.ActionMode.Read);
                        string xmlIshObjects = IshSession.PublicationOutput25.RetrieveMetadataByIshLngRefs(lngCardIds.ToArray(), requestedMetadata.ToXml());
                        IshObjects retrievedObjects = new IshObjects(xmlIshObjects);
                        returnedObjects.AddRange(retrievedObjects.Objects);
                    }
                    else
                    {
                        var metadata = IshSession.IshTypeFieldSetup.ToIshMetadataFields(ISHType, new IshFields(Metadata), Enumerations.ActionMode.Update);
                        PublicationOutput25ServiceReference.SetMetadataResponse response = null;
                        if (ShouldProcess(LogicalId + "=" + Version + "=" + LanguageCombination + "=" + OutputFormat))
                        {
                            response = IshSession.PublicationOutput25.SetMetadata(new SetMetadataRequest(
                            LogicalId,
                            Version,
                            OutputFormat,
                            LanguageCombination,
                            metadata.ToXml(),
                            requiredCurrentMetadata.ToXml()));
                        }
                        IshFields requestedMetadata = IshSession.IshTypeFieldSetup.ToIshRequestedMetadataFields(ISHType, metadata, Enumerations.ActionMode.Read);
                        var response2 = IshSession.PublicationOutput25.GetMetadata(new GetMetadataRequest(
                            LogicalId, response.version, OutputFormat, LanguageCombination,
                            requestedMetadata.ToXml()));
                        string xmlIshObjects = response2.xmlObjectList;
                        IshObjects retrievedObjects = new IshObjects(xmlIshObjects);
                        returnedObjects.AddRange(retrievedObjects.Objects);
                    }
                }

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
