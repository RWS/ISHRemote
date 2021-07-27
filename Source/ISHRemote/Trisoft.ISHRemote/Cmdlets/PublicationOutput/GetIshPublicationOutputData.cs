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
using System.Management.Automation;
using Trisoft.ISHRemote.Objects;
using Trisoft.ISHRemote.Objects.Public;
using Trisoft.ISHRemote.HelperClasses;
using Trisoft.ISHRemote.Exceptions;
using System.Xml;
using System.IO;

namespace Trisoft.ISHRemote.Cmdlets.PublicationOutput
{
    /// <summary>
    /// <para type="synopsis">The Get-IshPublicationOutputData cmdlet retrieves the data/blob of the publication output that are passed through the pipeline or determined via provided parameters and saves them to the given Windows folder.</para>
    /// <para type="description">The Get-IshPublicationOutputData cmdlet retrieves the data/blob of the publication output that are passed through the pipeline or determined via provided parameters and saves them to the given Windows folder.</para>
    /// </summary>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/InfoShareWS/" -IshUserName "username" -IshUserPassword  "userpassword"
    /// $requestedMetadataRetrieve = Set-IshRequestedMetadataField -IshSession $ishSession -Name 'FISHOUTPUTFORMATREF' -Level "Lng" |
    ///                              Set-IshRequestedMetadataField -IshSession $ishSession -Name 'FISHPUBLNGCOMBINATION' -Level "Lng" |
    ///                              Set-IshRequestedMetadataField -IshSession $ishSession -Name 'FISHPUBSTATUS' -Level "Lng" |
    ///                              Set-IshRequestedMetadataField -IshSession $ishSession -Name 'FISHPUBLISHER' -Level "Lng" |
    ///                              Set-IshRequestedMetadataField -IshSession $ishSession -Name 'FISHPUBSTARTDATE' -Level "Lng" |
    ///                              Set-IshRequestedMetadataField -IshSession $ishSession -Name 'FISHPUBENDDATE' -Level "Lng" |
    ///                              Set-IshRequestedMetadataField -IshSession $ishSession -Name 'VERSION' -Level "Version"
    /// $metadataFilterRetrieve = Set-IshMetadataFilterField -IshSession $ishSession -Name 'FISHPUBSTATUS' -Level 'Lng' -ValueType "Value" -FilterOperator "Equal" -Value "Draft" 
    /// $publicationOutputs = Find-IshPublicationOutput -IshSession $ishSession -StatusFilter "ishnostatusfilter" -MetadataFilter $metadataFilterRetrieve -RequestedMetadata $requestedMetadataRetrieve
    /// $fileInfoArray = $publicationOutputs | Get-IshPublicationOutputData -FolderPath "c:\temp\export"
    /// </code>
    /// <para>New-IshSession will submit into SessionState, so it can be reused by this cmdlet. Downloading publication outputs</para>
    /// </example>
    [Cmdlet(VerbsCommon.Get, "IshPublicationOutputData", SupportsShouldProcess = false)]
    [OutputType(typeof(FileInfo))]
    public sealed class GetIshPublicationOutputData : PublicationOutputCmdlet
    {

        /// <summary>
        /// <para type="description">The IshSession variable holds the authentication and contract information. This object can be initialized using the New-IshSession cmdlet.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false), ValidateNotNullOrEmpty]
        public IshSession IshSession { get; set; }

        /// <summary>
        /// <para type="description">Array with the publication outputs for which to retrieve the data. This array can be passed through the pipeline or explicitly passed via the parameter.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true)]
        [AllowEmptyCollection]
        public IshObject[] IshObject { get; set; }

        /// <summary>
        /// <para type="description">Folder on the Windows filesystem where to store the retrieved data files</para>
        /// </summary>
        [Parameter(Mandatory = true)]
        public string FolderPath { get; set; }

        protected override void BeginProcessing()
        {
            if (IshSession == null) { IshSession = (IshSession)SessionState.PSVariable.GetValue(ISHRemoteSessionStateIshSession); }
            if (IshSession == null) { throw new ArgumentException(ISHRemoteSessionStateIshSessionException); }
            WriteDebug($"Using IshSession[{IshSession.Name}] from SessionState.{ISHRemoteSessionStateIshSession}");
            base.BeginProcessing();
        }

        /// <summary>
        /// Process the Get-IshPublicationOutputData commandlet.
        /// </summary>
        /// <exception cref="TrisoftAutomationException"></exception>
        /// <exception cref="Exception"></exception>
        /// <remarks>Writes an <see cref="File"/> array to the pipeline.</remarks>
        protected override void ProcessRecord()
        {
            try
            {
                List<FileInfo> fileInfo = new List<FileInfo>();

                if (IshObject != null && IshObject.Length == 0)
                {
                    // Do nothing
                    WriteVerbose("IshObject is empty, so nothing to retrieve");
                }
                else
                {
                    WriteDebug("Retrieving");

                    string tempLocation = Directory.CreateDirectory(FolderPath).FullName;
                    int current = 0;

                    var ishObjects = new IshObjects(IshObject).Objects;
                    foreach (IshObject ishObject in ishObjects)
                    {
                        // Get language ref
                        long lngRef = Convert.ToInt64(ishObject.ObjectRef[Enumerations.ReferenceType.Lng]);
                        string xmlIshDataObject = IshSession.PublicationOutput25.GetDataObjectInfoByIshLngRef(lngRef);

                        // Put the xml in a dataobject
                        XmlDocument xmlIshDataObjectDocument = new XmlDocument();
                        xmlIshDataObjectDocument.LoadXml(xmlIshDataObject);
                        XmlElement ishDataObjectElement =
                            (XmlElement) xmlIshDataObjectDocument.SelectSingleNode("ishdataobjects/ishdataobject");
                        IshDataObject ishDataObject = new IshDataObject(ishDataObjectElement);
                        string tempFilePath = FileNameHelper.GetDefaultPublicationOutputFileName(tempLocation, ishObject,
                            ishDataObject.FileExtension);

                        WriteDebug($"Writing lngRef[{lngRef}] to [{tempFilePath}] {++current}/{ishObjects.Length}");

                        //Create the file.
                        using (FileStream fs = File.Create(tempFilePath))
                        {
                            for (int offset = 0; offset < ishDataObject.Size; offset += IshSession.ChunkSize)
                            {
                                int size = IshSession.ChunkSize;
                                long offsetCount = offset;
                                byte[] byteArray = new byte[IshSession.ChunkSize];
                                var response =
                                    IshSession.PublicationOutput25.GetNextDataObjectChunkByIshLngRef(
                                        new PublicationOutput25ServiceReference.GetNextDataObjectChunkByIshLngRefRequest(
                                            lngRef,
                                            ishDataObject.Ed,
                                            offsetCount,
                                            size));
                                offsetCount = response.offSet;
                                size = response.size;
                                byteArray = response.bytes;
                                fs.Write(byteArray, 0, size);
                            }
                        }

                        // Append file info list
                        fileInfo.Add(new FileInfo(tempFilePath));
                    }
                }
                WriteVerbose("returned file count[" + fileInfo.Count + "]");
                WriteObject(fileInfo, true);
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
