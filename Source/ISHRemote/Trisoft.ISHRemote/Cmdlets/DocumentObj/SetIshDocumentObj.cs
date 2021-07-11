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
using System.Xml.Linq;
using System.IO;

namespace Trisoft.ISHRemote.Cmdlets.DocumentObj
{
    /// <summary>
    /// <para type="synopsis">The Set-IshDocumentObj cmdlet updates the document objects that are passed through the pipeline or determined via provided parameters This commandlet allows to update all types of objects (Illustrations, Maps, etc. ), except for publication (outputs). 
    /// For publication (outputs) you need to use Set-IshPublicationOutput.</para>
    /// <para type="description">The Set-IshDocumentObj cmdlet updates the document objects that are passed through the pipeline or determined via provided parameters This commandlet allows to update all types of objects (Illustrations, Maps, etc. ), except for publication (outputs). 
    /// For publication (outputs) you need to use Set-IshPublicationOutput.</para>
    /// </summary>
    /// <example>
    /// <code>
    ///$ishSession = New-IshSession -WsBaseUrl "https://example.com/ISHWS/" -PSCredential Admin
    ///Get-IshDocumentObj -IshSession $ishSession -LogicalId ISHPUBLMODULECOMBINELANGUAGES | 
    ///Set-IshMetadataField -IshSession $ishSession  -Name "FSTATUS" -Level "Lng" -ValueType "Element"  -Value "VSTATUSTOBEREVIEWED" |
    ///Set-IshDocumentObj -IshSession $ishSession |
    ///Set-IshMetadataField -IshSession $ishSession  -Name "FSTATUS" -Level "Lng" -ValueType "Element"  -Value "VSTATUSRELEASED" |
    ///Set-IshDocumentObj -IshSession $ishSession
    /// </code>
    /// <para>For all versions and languages retrieved, push them to status 'To Be Reviewed' and immediately to 'Release'. Note that also Find-IshDocumentObj or Get-IshFolderContent are ways to get to content objects.</para>
    /// </example>
    /// <example>
    /// <code>
    ///New-IshSession -WsBaseUrl "https://example.com/ISHWS/" -IshUserName "Username" -IshUserPassword  "Password"
    ///$ishFolderTopic = Add-IshFolder -ParentFolderId (Get-IshFolder -BaseFolder Data).IshFolderRef -FolderType ISHModule -FolderName "TopicFolder"
    ///$ishTopicMetadata = Set-IshMetadataField -Name "FTITLE" -Level Logical -Value "My test Topic" |
    ///                    Set-IshMetadataField -Name "FAUTHOR" -Level Lng -ValueType Value -Value "Username" |
    ///                    Set-IshMetadataField -Name "FSTATUS" -Level Lng -ValueType Element -Value "VSTATUSDRAFT"
    ///$ditaTopicFileContent = @"
    ///&lt;?xml version=&quot;1.0&quot; ?&gt;
    ///&lt;!DOCTYPE topic PUBLIC &quot;-//OASIS//DTD DITA Topic//EN&quot; &quot;topic.dtd&quot;&gt;
    ///&lt;topic&gt;&lt;title&gt;Enter the title of your topic here.&lt;/title&gt;&lt;body&gt;&lt;p&gt;This is the start of your topic&lt;/p&gt;&lt;/body&gt;&lt;/topic&gt;
    ///"@
    ///$ishObject = Add-IshDocumentObj -FolderId $ishFolderTopic.IshFolderRef -IshType ISHModule -Lng "en" -Metadata $ishTopicMetadata -FileContent $ditaTopicFileContent
    ///$ishObject = $ishObject | Set-IshDocumentObj -Metadata(Set-IshMetadataField -Name "FTITLE" -Level Lng -Value "Updated topic title")
    /// </code>
    /// <para>Create a topic in a folder and then overwrite its metadata (FTITLE) using piped IshObject to the Set-IshDocumentObj cmd-let. New-IshSession will submit into SessionState, so it can be reused by all cmd-lets.</para>
    /// </example>
    /// <example>
    /// <code>
    ///New-IshSession -WsBaseUrl "https://example.com/ISHWS/" -IshUserName "Username" -IshUserPassword  "Password"
    ///$ishFolderTopic = Add-IshFolder -ParentFolderId(Get-IshFolder -BaseFolder Data).IshFolderRef -FolderType ISHModule -FolderName "TopicFolder"
    ///$ishTopicMetadata = Set-IshMetadataField -Name "FTITLE" -Level Logical -Value "My test Topic" |
    ///                    Set-IshMetadataField -Name "FAUTHOR" -Level Lng -ValueType Value -Value "Username" |
    ///                    Set-IshMetadataField -Name "FSTATUS" -Level Lng -ValueType Element -Value "VSTATUSDRAFT"
    ///$ditaTopicFileContent = @"
    ///&lt;?xml version=&quot;1.0&quot; ?&gt;
    ///&lt;!DOCTYPE topic PUBLIC &quot;-//OASIS//DTD DITA Topic//EN&quot; &quot;topic.dtd&quot;&gt;
    ///&lt;topic&gt;&lt;title&gt;Enter the title of your topic here.&lt;/title&gt;&lt;body&gt;&lt;p&gt;This is the start of your topic&lt;/p&gt;&lt;/body&gt;&lt;/topic&gt;
    ///"@
    /// $ditaTopicFileContentUpdated = @"
    ///&lt;? xml version=&quot;1.0&quot; ?&gt;
    ///&lt;!DOCTYPE topic PUBLIC &quot;-//OASIS//DTD DITA Topic//EN&quot; &quot;topic.dtd&quot;&gt;
    ///&lt;topic&gt;&lt;title&gt;Enter the title of your topic here.&lt;/title&gt;&lt;body&gt;&lt;p&gt;This is the start of your topic(updated)&lt;/p&gt;&lt;/body&gt;&lt;/topic&gt;
    ///"@
    ///$ishObject = Add-IshDocumentObj -FolderId $ishFolderTopic.IshFolderRef -IshType ISHModule -Lng "en" -Metadata $ishTopicMetadata -FileContent $ditaTopicFileContent
    ///$ishObject = Set-IshDocumentObj -LogicalId $ishObject.IshRef `
    ///                                -Version $ishObject.version_version_value `
    ///                                -Lng $ishObject.doclanguage `
    ///                                -FileContent $ditaTopicFileContentUpdated `
    ///                                -Metadata(Set-IshMetadataField -Name "FTITLE" -Level Logical -Value "Updated topic title and content")
    /// </code>
    /// <para>Create a topic in a folder and then overwrite its metadata (FTITLE) and blob by providing parameters and FileContent value to the Set-IshDocumentObj cmd-let. New-IshSession will submit into SessionState, so it can be reused by all cmd-lets.</para>
    /// </example>
    /// <example>
    /// <code>
    ///New-IshSession -WsBaseUrl "https://example.com/ISHWS/" -IshUserName "Username" -IshUserPassword  "Password"
    ///$ishFolderTopic = Add-IshFolder -ParentFolderId(Get-IshFolder -BaseFolder Data).IshFolderRef -FolderType ISHModule -FolderName "TopicFolder"
    ///$ishTopicMetadata = Set-IshMetadataField -Name "FTITLE" -Level Logical -Value "My test Topic" |
    ///                    Set-IshMetadataField -Name "FAUTHOR" -Level Lng -ValueType Value -Value "Username" |
    ///                    Set-IshMetadataField -Name "FSTATUS" -Level Lng -ValueType Element -Value "VSTATUSDRAFT"
    ///$ditaTopicFileContent = @"
    ///&lt;?xml version=&quot;1.0&quot; ?&gt;
    ///&lt;!DOCTYPE topic PUBLIC &quot;-//OASIS//DTD DITA Topic//EN&quot; &quot;topic.dtd&quot;&gt;
    ///&lt;topic&gt;&lt;title&gt;Enter the title of your topic here.&lt;/title&gt;&lt;body&gt;&lt;p&gt;This is the start of your topic&lt;/p&gt;&lt;/body&gt;&lt;/topic&gt;
    ///"@
    ///$ishObject = Add-IshDocumentObj -FolderId $ishFolderTopic.IshFolderRef -IshType ISHModule -Lng "en" -Metadata $ishTopicMetadata -FileContent $ditaTopicFileContent
    ///$tempFilePath = (New-TemporaryFile).FullName
    ///$ditaTopicFileContent.Replace("your topic", "my topic") | Out-File -FilePath $tempFilePath 
    ///$ishObject = Set-IshDocumentObj -LogicalId $ishObject.IshRef `
    ///                                -Version $ishObject.version_version_value `
    ///                                -Lng $ishObject.doclanguage `
    ///                                -FilePath $tempFilePath `
    ///                                -Metadata(Set-IshMetadataField -Name "FTITLE" -Level Logical -Value "Updated topic title and content using file path")
    /// </code>
    /// <para>Create a topic in a folder and then overwrite its metadata (FTITLE) and blob by providing parameters and FilePath value to the Set-IshDocumentObj cmd-let. New-IshSession will submit into SessionState, so it can be reused by all cmd-lets.</para>
    /// </example>
    [Cmdlet(VerbsCommon.Set, "IshDocumentObj", SupportsShouldProcess = true)]
    [OutputType(typeof(IshDocumentObj))]
    public sealed class SetIshDocumentObj : DocumentObjCmdlet
    {
        /// <summary>
        /// <para type="description">The IshSession variable holds the authentication and contract information. This object can be initialized using the New-IshSession cmdlet.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroupFilePath")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroupFileContent")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroupIshObjects")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroupMetadata")]
        [ValidateNotNullOrEmpty]
        public IshSession IshSession { get; set; }

        /// <summary>
        /// <para type="description">The logical identifier of the object to update</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroupFilePath")]
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroupFileContent")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroupMetadata")]
        [ValidateNotNullOrEmpty]
        public string LogicalId { get; set; }

        /// <summary>
        /// <para type="description">The version of the object to update</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroupFilePath")]
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroupFileContent")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroupMetadata")]
        [ValidateNotNullOrEmpty]
        public string Version { get; set; }

        /// <summary>
        /// <para type="description">The language of the object to update</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroupFilePath")]
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroupFileContent")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroupMetadata")]
        [ValidateNotNullOrEmpty]
        public string Lng { get; set; }

        /// <summary>
        /// <para type="description">The resolution of the object to update</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroupFilePath")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroupFileContent")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroupMetadata")]
        [ValidateNotNullOrEmpty]
        public string Resolution { get; set; }

        /// <summary>
        /// <para type="description">The metadata to set for the document object</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroupFilePath")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroupFileContent")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroupIshObjects")]
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroupMetadata")]
        [ValidateNotNull]
        public IshField[] Metadata { get; set; }

        /// <summary>
        /// <para type="description">The required current metadata of the document object. This parameter can be used to avoid that users override changes done by other users. The cmdlet will check whether the fields provided in this parameter still have the same values in the repository:</para>
        /// <para type="description">If the metadata is still the same, the metadata will be set.</para>
        /// <para type="description">If the metadata is not the same anymore, an error is given and the metadata will not be set.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroupFilePath")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroupFileContent")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroupIshObjects")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroupMetadata")]
        [ValidateNotNullOrEmpty]
        public IshField[] RequiredCurrentMetadata { get; set; }

        /// <summary>
        /// <para type="description">The unique identifier of the Electronic Document Type of the output (e.g. EDTPDF, EDTXML, EDTHTML,...)</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroupFilePath")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroupFileContent")]
        [ValidateNotNullOrEmpty]
        public string Edt
        {
            get { return _edt; }
            set { _edt = value; }
        }

        /// <summary>
        /// <para type="description">The location of the file on the filesystem containing new content (xml, jpg, etc.) for the object</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroupFilePath"), ValidateNotNullOrEmpty]
        public string FilePath { get; set; }

        /// <summary>
        /// <para type="description">String with XML content of the DocumentObj</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroupFileContent"), ValidateNotNullOrEmpty]
        public string FileContent { get; set; }

        /// <summary>
        /// <para type="description">Array with the objects for which to update the metadata. This array can be passed through the pipeline or explicitly passed via the parameter.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "ParameterGroupIshObjects")]
        [AllowEmptyCollection]
        public IshObject[] IshObject { get; set; }

        #region Private fields 
        /// <summary>
        /// EDT will be defaulted to EDTXML. Needs to match up with FileContent or FilePath content.
        /// </summary>
        private string _edt = "EDTXML";
        #endregion

        protected override void BeginProcessing()
        {
            if (IshSession == null) { IshSession = (IshSession)SessionState.PSVariable.GetValue(ISHRemoteSessionStateIshSession); }
            if (IshSession == null) { throw new ArgumentException(ISHRemoteSessionStateIshSessionException); }
            WriteDebug($"Using IshSession[{IshSession.Name}] from SessionState.{ISHRemoteSessionStateIshSession}");
            base.BeginProcessing();
        }

        /// <summary>
        /// Process the Set-IshDocumentObj command-let.
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
                if (IshObject != null && IshObject.Length == 0)
                {
                    // Do nothing
                    WriteVerbose("IshObject is empty, so nothing to update");
                    return;
                }

                // 2. Doing the update
                WriteDebug("Updating");
                List<IshObject> returnedObjects = new List<IshObject>();
                IshFields requiredCurrentMetadata = new IshFields(RequiredCurrentMetadata);

                if (ParameterSetName == "ParameterGroupIshObjects")
                {
                    int current = 0;
                    IshObject[] ishObjects = IshObject;
                    List<long> lngCardIds = new List<long>();

                    foreach (IshObject ishObject in ishObjects)
                    {
                        // Get language ref
                        long lngRef = Convert.ToInt64(ishObject.ObjectRef[Enumerations.ReferenceType.Lng]);
                        // Use incoming Metadata for update operation, or re-submit metadata of incoming IshObjects
                        var metadata = (Metadata != null) ?
                            IshSession.IshTypeFieldSetup.ToIshMetadataFields(ISHType, new IshFields(Metadata), Enumerations.ActionMode.Update) :
                            IshSession.IshTypeFieldSetup.ToIshMetadataFields(ISHType, ishObject.IshFields, Enumerations.ActionMode.Update);
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

                    var returnFields = IshObject[0].IshFields ?? new IshFields();
                    IshFields requestedMetadata = IshSession.IshTypeFieldSetup.ToIshRequestedMetadataFields(IshSession.DefaultRequestedMetadata, ISHType, returnFields, Enumerations.ActionMode.Read);
                    string xmlIshObjects = IshSession.DocumentObj25.RetrieveMetadataByIshLngRefs(lngCardIds.ToArray(), requestedMetadata.ToXml());
                    IshObjects retrievedObjects = new IshObjects(ISHType, xmlIshObjects);
                    returnedObjects.AddRange(retrievedObjects.Objects);
                }
                else
                {
                    // ParameterGroups FilePath, FileContent, Metadata
                    string resolution = Resolution ?? "";
                    IshData ishData = null;
                    var metadata = IshSession.IshTypeFieldSetup.ToIshMetadataFields(ISHType, new IshFields(Metadata), Enumerations.ActionMode.Update);
                    string version = "-1";

                    if (ParameterSetName == "ParameterGroupFilePath")
                    {
                        ishData = new IshData(_edt, FilePath);
                    }

                    if (ParameterSetName == "ParameterGroupFileContent")
                    {
                        if (!_edt.Equals("EDTXML", StringComparison.Ordinal))
                        {
                            throw new NotImplementedException("FileContent parameter is only supported with Edt='EDTXML'");
                        }

                        var doc = XDocument.Parse(FileContent, LoadOptions.PreserveWhitespace);
                        var ms = new MemoryStream();
                        doc.Save(ms, SaveOptions.DisableFormatting);
                        ishData = new IshData(_edt, ms.ToArray());
                    }

                    if (ParameterSetName == "ParameterGroupMetadata")
                    {
                        WriteDebug($"Id[{LogicalId}] Version[{Version}] Lng[{Lng}] Resolution[{resolution}] Metadata.length[{metadata.ToXml().Length}] dataSize[0]");
                        if (ShouldProcess(LogicalId + "=" + Version + "=" + Lng + "=" + resolution))
                        {
                            SetMetadataResponse response = IshSession.DocumentObj25.SetMetadata(new SetMetadataRequest(
                                LogicalId,
                                Version,
                                Lng,
                                resolution,
                                metadata.ToXml(),
                                requiredCurrentMetadata.ToXml()));
                            version = response.version;
                        }
                    }
                    else
                    {
                        WriteDebug($"Id[{LogicalId}] Version[{Version}] Lng[{Lng}] Resolution[{resolution}] Metadata.length[{metadata.ToXml().Length}] dataSize[{ishData.Size()}]");
                        if (ShouldProcess(LogicalId + "=" + Version + "=" + Lng + "=" + resolution))
                        {
                            UpdateResponse response = IshSession.DocumentObj25.Update(new UpdateRequest(
                                LogicalId,
                                Version,
                                Lng,
                                resolution,
                                metadata.ToXml(),
                                requiredCurrentMetadata.ToXml(),
                                ishData.Edt,
                                ishData.ByteArray));
                            version = response.version;
                        }
                    }

                    // Get the updated object
                    IshFields requestedMetadata = IshSession.IshTypeFieldSetup.ToIshRequestedMetadataFields(IshSession.DefaultRequestedMetadata, ISHType, metadata, Enumerations.ActionMode.Read);
                    var response2 = IshSession.DocumentObj25.GetMetadata(new GetMetadataRequest(LogicalId,
                        version,
                        Lng,
                        resolution,
                        requestedMetadata.ToXml()));
                    string xmlIshObjects = response2.xmlObjectList;
                    IshObjects retrievedObjects = new IshObjects(ISHType, xmlIshObjects);
                    returnedObjects.AddRange(retrievedObjects.Objects);
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
