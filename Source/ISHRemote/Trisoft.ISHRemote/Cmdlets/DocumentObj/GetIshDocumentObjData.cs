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
using System.IO;
using System.Management.Automation;
using System.Xml;
using Trisoft.ISHRemote.Exceptions;
using Trisoft.ISHRemote.HelperClasses;
using Trisoft.ISHRemote.Objects;
using Trisoft.ISHRemote.Objects.Public;

namespace Trisoft.ISHRemote.Cmdlets.DocumentObj
{
    /// <summary>
    /// <para type="synopsis">The Get-IshDocumentObjData cmdlet retrieves the data/blob of the document objects that are passed through the pipeline or determined via provided parameters and saves them to the given Windows folder. This commandlet allows to retrieve all types of objects (Illustrations, Maps, etc. ), except for publication (outputs). 
    /// For publication (outputs) you need to use Get-IshPublicationOutputData.</para>
    /// <para type="description">The Get-IshDocumentObjData cmdlet retrieves the data/blob of the document objects that are passed through the pipeline or determined via provided parameters and saves them to the given Windows folder. This commandlet allows to retrieve all types of objects (Illustrations, Maps, etc. ), except for publication (outputs). 
    /// For publication (outputs) you need to use Get-IshPublicationOutputData.</para>
    /// </summary>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/InfoShareWS/" -IshUserName "username" -IshUserPassword  "userpassword"
    /// $ishMetadataFilterFields = Set-IshMetadataFilterField -IshSession $ishSession -Name "FTITLE" -Level "Logical" -FilterOperator "Like" -Value "%topic%" |
    ///                            Set-IshMetadataFilterField -IshSession $ishSession -Name "FAUTHOR" -Level "Lng" -FilterOperator "Equal" -Value "admin" | 
    ///                            Set-IshMetadataFilterField -IshSession $ishSession -Name "FSTATUS" -Level "Lng" -FilterOperator "Equal" -Value "To be translated"    
    /// $ishRequestedFields = Set-IshRequestedMetadataField -IshSession $ishSession -Name "FTITLE" -Level "logical" |
    ///                       Set-IshRequestedMetadataField -IshSession $ishSession -Name "VERSION" -Level "version" |
    ///                       Set-IshRequestedMetadataField -IshSession $ishSession -Name "DOC-LANGUAGE" -Level "lng"
    /// $fileList = Find-IshDocumentObj -IshSession $ishSession -MetadataFilter $ishMetadataFilterFields `
    /// -IshTypeFilter "ISHModule" `
    /// -RequestedMetadata $ishRequestedFields |
    /// Get-IshDocumentObjData $ishSession `
    /// -FolderPath "c:\temp\export"
    /// </code>
    /// <para>New-IshSession will submit into SessionState, so it can be reused by this cmdlet. Download modules to a temp location</para>
    /// </example>
    [Cmdlet(VerbsCommon.Get, "IshDocumentObjData", SupportsShouldProcess = false)]
    [OutputType(typeof(IshObject), typeof(FileInfo))]
    public sealed class GetIshDocumentObjData : DocumentObjCmdlet
    {
        /// <summary>
        /// <para type="description">The IshSession variable holds the authentication and contract information. This object can be initialized using the New-IshSession cmdlet.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false)]
        [ValidateNotNullOrEmpty]
        public IshSession IshSession { get; set; }

        /// <summary>
        /// <para type="description">Array with the objects for which to retrieve the data. This array can be passed through the pipeline or explicitly passed via the parameter.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true)]
        [AllowEmptyCollection]
        public IshObject[] IshObject { get; set; }

        /// <summary>
        /// <para type="description">Folder on the Windows filesystem where to store the retrieved data files</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipeline = false)]
        public string FolderPath { get; set; }

        /// <summary>
        /// <para type="description">The condition context to use for conditional filtering.  If no context is provided, the elements containing ishcondition attributes will always remain in the data content. You can use the Set-IshFeature cmdlet to create a condition context.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false)]
        public IshFeature[] IshFeature { get; set; }

        protected override void BeginProcessing()
        {
            if (IshSession == null) { IshSession = (IshSession)SessionState.PSVariable.GetValue(ISHRemoteSessionStateIshSession); }
            if (IshSession == null) { throw new ArgumentException(ISHRemoteSessionStateIshSessionException); }
            WriteDebug($"Using IshSession[{IshSession.Name}] from SessionState.{ISHRemoteSessionStateIshSession}");
            base.BeginProcessing();
        }

        /// <summary>
        /// Process the Get-IshDocumentObjData commandlet.
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
                    WriteVerbose("IshObject is empty, so nothing to retrieve");
                }
                else
                {
                    WriteDebug("Retrieving");

                    IshFeatures productDefinitionFeatures = new IshFeatures(IshFeature);
                    int current = 0;

                    var ishObjects = new IshObjects(IshObject).Objects;
                    foreach (IshObject ishObject in ishObjects)
                    {
                        // Get language ref
                        long lngRef = Convert.ToInt64(ishObject.ObjectRef[Enumerations.ReferenceType.Lng]);
                        long[] lngRefsArray = new long[1] { lngRef };
                        var dataObjectResponse = IshSession.DocumentObj25.RetrieveObjectsByIshLngRefs(lngRefsArray, productDefinitionFeatures.ToXml(), "");
                        XmlDocument xmlIshDataObject = new XmlDocument();
                        xmlIshDataObject.LoadXml(dataObjectResponse);
                        XmlElement ishDataObjectElement = (XmlElement)xmlIshDataObject.SelectSingleNode("ishobjects/ishobject/ishdata");
                        IshData ishData = new IshData(ishDataObjectElement);

                        if (FolderPath != null)
                        {
                            string tempLocation = Directory.CreateDirectory(FolderPath).FullName;
                            WriteDebug($"Writing lngRef[{lngRef}] to [{tempLocation}] {++current}/{ishObjects.Length}");
                            //Create the file.
                            string tempFilePath = FileNameHelper.GetDefaultObjectFileName(tempLocation, ishObject, ishData.FileExtension);
                            using (FileStream fs = File.Create(tempFilePath))
                            {
                                fs.Write(ishData.ByteArray, 0, ishData.Size());
                            }
                            // Append file info list
                            fileInfo.Add(new FileInfo(tempFilePath));
                            WriteObject(fileInfo, true);
                        }
                        else
                        {
                            WriteDebug($"Enriching ishObject[{ishObject.ObjectRef[Enumerations.ReferenceType.Lng]}] with IshData {++current}/{IshObject.Length}");
                            ishObject.IshData = ishData;
                            WriteObject(IshSession, ISHType, ishObject, true);
                        }       
                    }
                    WriteVerbose("returned file count[" + current + "]");
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
    }
}
