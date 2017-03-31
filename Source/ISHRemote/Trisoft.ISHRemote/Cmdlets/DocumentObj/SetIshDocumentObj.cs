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
using Trisoft.ISHRemote.DocumentObj25ServiceReference;
using Trisoft.ISHRemote.Objects;
using Trisoft.ISHRemote.Objects.Public;
using Trisoft.ISHRemote.Exceptions;
using Trisoft.ISHRemote.HelperClasses;

namespace Trisoft.ISHRemote.Cmdlets.DocumentObj
{
    /// <summary>
    /// <para type="synopsis">The Set-IshDocumentObj cmdlet updates the document objects that are passed through the pipeline or determined via provided parameters This commandlet allows to update all types of objects (Illustrations, Maps, etc. ), except for publication (outputs). 
    /// For publication (outputs) you need to use Set-IshPublicationOutput.</para>
    /// <para type="description">The Set-IshDocumentObj cmdlet updates the document objects that are passed through the pipeline or determined via provided parameters This commandlet allows to update all types of objects (Illustrations, Maps, etc. ), except for publication (outputs). 
    /// For publication (outputs) you need to use Set-IshPublicationOutput.</para>
    /// </summary>
    [Cmdlet(VerbsCommon.Set, "IshDocumentObj", SupportsShouldProcess = true)]
    [OutputType(typeof(IshObject))]
    public sealed class SetIshDocumentObj : DocumentObjCmdlet
    {

        /// <summary>
        /// <para type="description">The IshSession variable holds the authentication and contract information. This object can be initialized using the New-IshSession cmdlet.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshObjectsGroup")]
        [ValidateNotNullOrEmpty]
        public IshSession IshSession { get; set; }

        /// <summary>
        /// <para type="description">The logical identifier of the object to update</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup"), ValidateNotNullOrEmpty]
        public string LogicalId { get; set; }

        /// <summary>
        /// <para type="description">The version of the object to update</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup"), ValidateNotNullOrEmpty]
        public string Version { get; set; }

        /// <summary>
        /// <para type="description">The language of the object to update</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup"), ValidateNotNullOrEmpty]
        public string Lng { get; set; }

        /// <summary>
        /// <para type="description">The resolution of the object to update</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup"), ValidateNotNullOrEmpty]
        public string Resolution { get; set; }

        /// <summary>
        /// <para type="description">The metadata to set for the document object</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup"), ValidateNotNull]
        public IshField[] Metadata { get; set; }

        /// <summary>
        /// <para type="description">The required current metadata of the document object. This parameter can be used to avoid that users override changes done by other users. The cmdlet will check whether the fields provided in this parameter still have the same values in the repository:</para>
        /// <para type="description">If the metadata is still the same, the metadata will be set.</para>
        /// <para type="description">If the metadata is not the same anymore, an error is given and the metadata will be set.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshObjectsGroup")]
        [ValidateNotNullOrEmpty]
        public IshField[] RequiredCurrentMetadata { get; set; }

        /// <summary>
        /// <para type="description">The unique identifier of the Electronic Document Type of the output (e.g. EDTPDF, EDTXML, EDTHTML,...)</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup"), ValidateNotNullOrEmpty]
        public string Edt { get; set; }

        /// <summary>
        /// <para type="description">The location of the file on the filesystem containing new content (xml, jpg, etc.) for the object</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup"), ValidateNotNullOrEmpty]
        public string FilePath { get; set; }

        /// <summary>
        /// <para type="description">Array with the objects for which to update the metadata. This array can be passed through the pipeline or explicitly passed via the parameter.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "IshObjectsGroup")]
        [AllowEmptyCollection]
        public IshObject[] IshObject { get; set; }


        /// <summary>
        /// Process the Set-IshDocumentObj commandlet.
        /// </summary>
        /// <exception cref="TrisoftAutomationException"></exception>
        /// <exception cref="Exception"></exception>
        /// <remarks>Writes an <see cref="IshObject"/> array to the pipeline.</remarks>
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
                    WriteVerbose("IshObject is empty, so nothing to update");
                }
                else
                {
                    // 2. Doing the update
                    WriteDebug("Updating");

                    IshFields requiredCurrentMetadata = new IshFields(RequiredCurrentMetadata);

                    if (IshObject != null)
                    {
                        int current = 0;
                        IshObject[] ishObjects = IshObject;
                        List<long> lngCardIds = new List<long>();

                        foreach (IshObject ishObject in ishObjects)
                        {
                            // Get language ref
                            long lngRef = Convert.ToInt64(ishObject.ObjectRef[Enumerations.ReferenceType.Lng]);
                            var metadata = IshSession.IshTypeFieldSetup.ToIshMetadataFields(ISHType, ishObject.IshFields, Enumerations.ActionMode.Update);

                            if (ishObject.IshData != null)
                            {
                                WriteDebug($"lngRef[{lngRef}] Metadata.length[{metadata.ToXml().Length}] dataSize[{ishObject.IshData.Size()}] {++current}/{ishObjects.Length}");
                                if (ShouldProcess(Convert.ToString(lngRef)))
                                {
                                    IshSession.DocumentObj25.UpdateByIshLngRef(
                                        lngRef,
                                        metadata.ToXml(),
                                        requiredCurrentMetadata.ToXml(),
                                        ishObject.IshData.Edt,
                                        ishObject.IshData.ByteArray);
                                }
                            }
                            else
                            {
                                WriteDebug($"lngRef[{lngRef}] Metadata.length[{metadata.ToXml().Length}] dataSize[0] {++current}/{ishObjects.Length}");
                                if (ShouldProcess(Convert.ToString(lngRef)))
                                {
                                    IshSession.DocumentObj25.SetMetadataByIshLngRef(
                                           lngRef,
                                           metadata.ToXml(),
                                           requiredCurrentMetadata.ToXml());
                                }
                            }
                            lngCardIds.Add(lngRef);
                        }

                        var returnFields = (IshObject[0] == null)
                            ? new IshFields()
                            : IshObject[0].IshFields;
                        IshFields requestedMetadata = IshSession.IshTypeFieldSetup.ToIshRequestedMetadataFields(ISHType, returnFields, Enumerations.ActionMode.Read);
                        string xmlIshObjects = IshSession.DocumentObj25.RetrieveMetadataByIshLngRefs(lngCardIds.ToArray(), requestedMetadata.ToXml());
                        IshObjects retrievedObjects = new IshObjects(xmlIshObjects);
                        returnedObjects.AddRange(retrievedObjects.Objects);
                    }
                    else
                    {
                        string resolution = Resolution ?? "";
                        var metadata = IshSession.IshTypeFieldSetup.ToIshMetadataFields(ISHType, new IshFields(Metadata), Enumerations.ActionMode.Update);

                        string version;
                        if (Edt != null && FilePath != null)
                        {
                            IshData ishData = new IshData(Edt, FilePath);
                            WriteDebug($"Id[{LogicalId}] Version[{Version}] Lng[{Lng}] Resolution[{resolution}] Metadata.length[{metadata.ToXml().Length}] dataSize[{ishData.Size()}]");
                            DocumentObj25ServiceReference.UpdateResponse response = null;
                            if (ShouldProcess(LogicalId + "=" + Version + "=" + Lng + "=" + resolution))
                            {
                                response = IshSession.DocumentObj25.Update(new UpdateRequest(
                                    LogicalId,
                                    Version,
                                    Lng,
                                    resolution,
                                    metadata.ToXml(),
                                    requiredCurrentMetadata.ToXml(),
                                    ishData.Edt,
                                    ishData.ByteArray));
                            }
                            version = response.version;
                        }
                        else
                        {
                            WriteDebug($"Id[{LogicalId}] Version[{Version}] Lng[{Lng}] Resolution[{resolution}] Metadata.length[{metadata.ToXml().Length}] dataSize[0]");
                            DocumentObj25ServiceReference.SetMetadataResponse response = null;
                            if (ShouldProcess(LogicalId + "=" + Version + "=" + Lng + "=" + resolution))
                            {
                                response = IshSession.DocumentObj25.SetMetadata(new SetMetadataRequest(
                                LogicalId,
                                Version,
                                Lng,
                                resolution,
                                metadata.ToXml(),
                                requiredCurrentMetadata.ToXml()));
                            }
                            version = response.version;
                        }
                        IshFields requestedMetadata = IshSession.IshTypeFieldSetup.ToIshRequestedMetadataFields(ISHType, metadata, Enumerations.ActionMode.Read);
                        var response2 = IshSession.DocumentObj25.GetMetadata(new GetMetadataRequest(LogicalId,
                            version,
                            Lng,
                            resolution,
                            requestedMetadata.ToXml()));
                        IshObjects retrievedObjects = new IshObjects(response2.xmlObjectList);
                        returnedObjects.AddRange(retrievedObjects.Objects);
                    }
                }            

                // 3. Write it
                WriteVerbose("returned object count[" + returnedObjects.Count + "]");
                WriteObject(returnedObjects.ToArray());
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
