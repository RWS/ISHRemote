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
using System.Xml.Linq;
using System.Management.Automation;
using Trisoft.ISHRemote.Objects;
using Trisoft.ISHRemote.Objects.Public;
using Trisoft.ISHRemote.Exceptions;
using Trisoft.ISHRemote.HelperClasses;

namespace Trisoft.ISHRemote.Cmdlets.Folder
{
    /// <summary>
    /// <para type="synopsis">The Get-IshFolderContent cmdlet returns all document objects or publication outputs stored inside a given folder.
    /// You can provide filters to reduce the amount of objects returned, but if you don't provide any:
    /// * The cmdlet will return an object for all the latest versions in all language and resolution for a document object folder
    /// * The cmdlet will return an object for all the latest versions for all output formats and all language combinations for a publication folder
    /// Note: The default value of the VersionFilter is "latest". In order to get all versions, you need to pass an empty string in the VersionFilter.</para>
    /// <para type="description">The Get-IshFolderContent cmdlet returns all document objects or publication outputs stored inside a given folder.
    /// You can provide filters to reduce the amount of objects returned, but if you don't provide any:
    /// * The cmdlet will return an object for all the latest versions in all language and resolution for a document object folder
    /// * The cmdlet will return an object for all the latest versions for all output formats and all language combinations for a publication folder.
    /// Note: The default value of the VersionFilter is "latest". In order to get all versions, you need to pass an empty string in the VersionFilter.</para>
    /// </summary>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/ISHWS/" -PSCredential "Admin"
    /// $folderId = 7775 #use correct folder id
    /// $ishObjects = Get-IshFolderContent -FolderId $folderId
    /// </code>
    /// <para>New-IshSession will submit into SessionState, so it can be reused by this cmdlet. Retrieve contents of a given folder</para>
    /// </example>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/ISHWS/" -PSCredential "Admin"
    /// $metadataFilter = Set-IshMetadataFilterField -Level Lng -Name FSTATUS -ValueType Element -FilterOperator In -Value 'VSTATUSTOBETRANSLATED, VSTATUSINTRANSLATION' |
    ///                   Set-IshMetadataFilterField -Level Lng -Name FSOURCELANGUAGE -FilterOperator NotEmpty
    /// Get-IshFolder -FolderPath "\General\Mobile Phones Demo" -Recurse | 
    /// Get-IshFolderContent -MetadataFilter $metadataFilter
    /// </code>
    /// <para>New-IshSession will submit into SessionState, so it can be reused by this cmdlet. The metadata filter will filter out target languages/stubs in a certain status (in this example probably the ones holding deprecated pretranslation). The recursive folder allows you to control which area you do a check/conversion in, and give you progress as well.</para>
    /// </example>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/ISHWS/" -PSCredential "Admin"
    /// $metadataFilter = Set-IshMetadataFilterField -Level Lng -Name FSOURCELANGUAGE -FilterOperator Empty
    /// Get-IshFolder -FolderPath "\General\Mobile Phones Demo" -Recurse | 
    /// Get-IshFolderContent -VersionFilter LATEST -MetadataFilter $metadataFilter
    /// </code>
    /// <para>New-IshSession will submit into SessionState, so it can be reused by this cmdlet. The metadata filter will filter out source languages/stubs and the VersionFilter will only return latest versions of any object. The recursive folder allows you to control which area you do a check/conversion in, and give you progress as well.</para>
    /// </example>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/ISHWS/" -PSCredential "Admin"
    /// $metadataFilter = Set-IshMetadataFilterField -Level Lng -Name FSOURCELANGUAGE -FilterOperator Empty
    /// $requestedMetadata = Set-IshRequestedMetadataField -Level Lng -Name FISHSTATUSTYPE
    /// Get-IshFolder -FolderPath "\General\Mobile Phones Demo" -Recurse | 
    /// Get-IshFolderContent -VersionFilter "" -MetadataFilter $metadataFilter -RequestedMetadata $requestedMetadata 
    /// </code>
    /// <para>New-IshSession will submit into SessionState, so it can be reused by this cmdlet. The metadata filter will filter out source languages and the empty VersionFilter will return all versions of any object. The recursive folder allows you to control which area you do a check/conversion in, and give you progress as well.</para>
    /// <para>Note that -RequestedMetadata will be used on every folder passed over the pipeline by Get-IshFolder. Requesting metadata for Topics (ISHModule) might be unexisting on Publication folders or vice versa. Know that Get-IshFolder has a -FolderTypeFilter parameter to workaround that.</para>
    /// </example>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/ISHWS/" -PSCredential "Admin"
    /// $ishSession.MetadataBatchSize = 100
    /// $DebugPreference = "Continue"
    /// Get-IshFolderContent -IshSession $ishSession -FolderPath "General\MyPublication\FolderWithManyTopics"
    /// </code>
    /// <para>Retrieve the latest version of content objects in smaller batches to avoid "heavy" WebService calls to the server. The progress could be seen in the debug messages</para>
    /// </example>
    [Cmdlet(VerbsCommon.Get, "IshFolderContent", SupportsShouldProcess = false)]
    [OutputType(typeof(IshObject))]
    public sealed class GetIshFolderContent : FolderCmdlet
    {

        /// <summary>
        /// <para type="description">The IshSession variable holds the authentication and contract information. This object can be initialized using the New-IshSession cmdlet.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "FolderIdGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "FolderPathGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "BaseFolderGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshFolderGroup")]
        [ValidateNotNullOrEmpty]
        public IshSession IshSession { get; set; }

        /// <summary>
        /// <para type="description">The metadata fields to retrieve</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "FolderIdGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "FolderPathGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "BaseFolderGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshFolderGroup")]
        [ValidateNotNull]
        public IshField[] RequestedMetadata { get; set; }

        /// <summary>
        /// <para type="description">Separated string with the full folder path</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "FolderPathGroup"), ValidateNotNull]
        public string FolderPath { get; set; }

        /// <summary>
        /// <para type="description">Identifier of the folder for which to retrieve the content</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "FolderIdGroup"), ValidateNotNullOrEmpty]
        public long FolderId { get; set; }

        /// <summary>
        /// <para type="description">The eBaseFolder enumeration to get subfolder(s) for the specified root folder</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "BaseFolderGroup"), ValidateNotNullOrEmpty]
        public Enumerations.BaseFolder BaseFolder { get; set; }

        /// <summary>
        /// <para type="description">Folder for which to retrieve the content. This array can be passed through the pipeline or explicitly passed via the parameter.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "IshFolderGroup")]
        public IshFolder[] IshFolder { get; set; }

        /// <summary>
        /// <para type="description">The version filter to limit the amount of objects returned. When no filter is supplied, latest version objects will be returned.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "FolderIdGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "FolderPathGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "BaseFolderGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshFolderGroup")]
        [ValidateNotNull]
        public string VersionFilter
        {
            get { return _versionFilter; }
            set { _versionFilter = value; }
        }

        /// <summary>
        /// <para type="description">The languages filter to limit the amount of objects returned. When no languages filter is supplied, all object languages will be returned.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "FolderIdGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "FolderPathGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "BaseFolderGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshFolderGroup")]
        [ValidateNotNull]
        [AllowEmptyCollection]
        public string[] LanguagesFilter
        {
            get { return _languagesFilter; }
            set { _languagesFilter = value; }
        }

        /// <summary>
        /// <para type="description">The metadata filter with the filter fields to limit the amount of objects returned</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "FolderIdGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "FolderPathGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "BaseFolderGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshFolderGroup")]
        [ValidateNotNullOrEmpty]
        public IshField[] MetadataFilter { get; set; }

        #region Private fields
        /// <summary>
        /// Private field to store and provide defaults for non-mandatory parameters
        /// </summary>
        private string _versionFilter = "latest";
        private string[] _languagesFilter = { };
        #endregion

        protected override void BeginProcessing()
        {
            if (IshSession == null) { IshSession = (IshSession)SessionState.PSVariable.GetValue(ISHRemoteSessionStateIshSession); }
            if (IshSession == null) { throw new ArgumentException(ISHRemoteSessionStateIshSessionException); }
            WriteDebug($"Using IshSession[{IshSession.Name}] from SessionState.{ISHRemoteSessionStateIshSession}");
            base.BeginProcessing();
        }

        /// <summary>
        /// Process the Get-IshFolderContent commandlet.
        /// </summary>
        /// <exception cref="TrisoftAutomationException"></exception>
        /// <exception cref="Exception"></exception>
        /// <remarks>Writes an <see cref="IshObject"/> array to the pipeline</remarks>
        protected override void ProcessRecord()
        {
            try
            {
                // 1. Validating the input
                WriteDebug("Validating");
                List<long> returnFolderIds = new List<long>();

                switch (ParameterSetName)
                {
                    case "IshFolderGroup":
                        foreach (IshFolder ishFolder in IshFolder)
                        {
                            returnFolderIds.Add(ishFolder.IshFolderRef);
                        }
                        break;

                    case "FolderIdGroup":
                        WriteDebug($"folderId[{FolderId}]");
                        returnFolderIds.Add(FolderId);
                        break;

                    case "FolderPathGroup":
                        // Parse FolderPath input parameter: get basefolderName(1st element of an array)
                        string folderPath = FolderPath;
                        string[] folderPathElements = folderPath.Split(
                            new string[] { IshSession.FolderPathSeparator }, StringSplitOptions.RemoveEmptyEntries);
                        string baseFolderLabel = folderPathElements[0];

                        // remaining folder path elements
                        string[] folderPathTrisoft = new string[folderPathElements.Length - 1];
                        Array.Copy(folderPathElements, 1, folderPathTrisoft, 0, folderPathElements.Length - 1);

                        WriteDebug($"FolderPath[{folderPath}]");
                        string xmlIshFolder = IshSession.Folder25.GetMetadata(
                            BaseFolderLabelToEnum(IshSession, baseFolderLabel),
                            folderPathTrisoft,
                            "");
                        IshFolders ishFolders = new IshFolders(xmlIshFolder, "ishfolder");
                        returnFolderIds.Add(ishFolders.Folders[0].IshFolderRef);
                        break;

                    case "BaseFolderGroup":
                        var baseFolder = EnumConverter.ToBaseFolder<Folder25ServiceReference.BaseFolder>(BaseFolder);
                        string xmlIshFolders = IshSession.Folder25.GetMetadata(
                            baseFolder,
                            new string[0],
                            "");
                        IshFolders retrievedFolders = new IshFolders(xmlIshFolders, "ishfolder");
                        returnFolderIds.Add(retrievedFolders.Folders[0].IshFolderRef);
                        break;
                }

                List<IshObject> returnIshObjects = new List<IshObject>();
                IshFields metadataFilterFields = new IshFields(MetadataFilter);
                // Update MetadataFilter with LanguagesFilter (if provided)
                if (LanguagesFilter != null && LanguagesFilter.Length > 0)
                {
                    if (LanguagesFilter[0].StartsWith("VLANGUAGE"))
                    {
                        metadataFilterFields.AddOrUpdateField(new IshMetadataFilterField(FieldElements.DocumentLanguage,
                            Enumerations.Level.Lng,
                            Enumerations.FilterOperator.In,
                            String.Join(IshSession.Separator, LanguagesFilter),
                            Enumerations.ValueType.Element),
                            Enumerations.ActionMode.Update);
                    }
                    else
                    {
                        metadataFilterFields.AddOrUpdateField(new IshMetadataFilterField(FieldElements.DocumentLanguage,
                            Enumerations.Level.Lng,
                            Enumerations.FilterOperator.In,
                            String.Join(IshSession.Separator, LanguagesFilter),
                            Enumerations.ValueType.Value),
                            Enumerations.ActionMode.Update);
                    }
                }

                int current = 0;
                foreach (long returnFolderId in returnFolderIds)
                {
                    // 2. Doing Retrieve
                    WriteDebug($"Looping folderId[{returnFolderId}] {++current}/{returnFolderIds.Count}");
                    string xmlIshObjects = IshSession.Folder25.GetContents(returnFolderId);
                    var ishObjects = new IshObjects(xmlIshObjects);

                    if (ishObjects.Ids.Length > 0)
                    {
                        WriteDebug("Retrieving LogicalIds.Length[" + ishObjects.Ids.Length + "]");
                        // First handle all documents/illustrations
                        var documentLogicalIds = ishObjects.Objects
                                    .Where(ishObject => ishObject.IshType != Enumerations.ISHType.ISHPublication)
                                    .Select(ishObject => ishObject.IshRef)
                                    .ToList();
                        if (documentLogicalIds.Any())
                        {
                            Enumerations.ISHType[] ISHType = { Enumerations.ISHType.ISHIllustration, Enumerations.ISHType.ISHLibrary, Enumerations.ISHType.ISHMasterDoc, Enumerations.ISHType.ISHModule, Enumerations.ISHType.ISHTemplate };
                            IshFields requestedMetadata = IshSession.IshTypeFieldSetup.ToIshRequestedMetadataFields(IshSession.DefaultRequestedMetadata, ISHType, new IshFields(RequestedMetadata), Enumerations.ActionMode.Read);

                            // Devides the list of LogicalIds in different lists that all have maximally MetadataBatchSize elements
                            List<List<string>> devidedDocumentLogicalIdsList = DevideListInBatches<string>(documentLogicalIds, IshSession.MetadataBatchSize);
                            int currentLogicalIdCount = 0;
                            foreach (List<string> logicalIdBatch in devidedDocumentLogicalIdsList)
                            {
                                currentLogicalIdCount += logicalIdBatch.Count;
                                WriteDebug($"Retrieving DocumentObj.length[{logicalIdBatch.Count}] MetadataFilter.length[{metadataFilterFields.ToXml().Length}] RequestedMetadata.length[{requestedMetadata.ToXml().Length}] {currentLogicalIdCount}/{documentLogicalIds.Count}");

                                if (VersionFilter != null && VersionFilter.Length > 0)
                                {
                                    WriteDebug($"Filtering DocumentObj using VersionFilter[{VersionFilter}]");
                                    xmlIshObjects = IshSession.DocumentObj25.RetrieveVersionMetadata(logicalIdBatch.ToArray(), VersionFilter, "");
                                    var documentIshObjects = new IshObjects(xmlIshObjects);
                                    if (documentIshObjects.Objects.Length > 0)
                                    {
                                        WriteVerbose($"Filtering DocumentObj using MetadataFilter.length[{metadataFilterFields.ToXml().Length}] and LanguagesFilter.Length[{LanguagesFilter.Length}]");
                                        var versionRefs = documentIshObjects.Objects
                                            .Select(ishObject => Convert.ToInt64(ishObject.ObjectRef[Enumerations.ReferenceType.Version]))
                                            .ToList();
                                        xmlIshObjects =
                                            IshSession.DocumentObj25.RetrieveMetadataByIshVersionRefs(versionRefs.ToArray(),
                                                DocumentObj25ServiceReference.StatusFilter.ISHNoStatusFilter, metadataFilterFields.ToXml(),
                                                requestedMetadata.ToXml());
                                        documentIshObjects = new IshObjects(ISHType, xmlIshObjects);
                                        returnIshObjects.AddRange(documentIshObjects.Objects);
                                    }
                                }
                                else
                                {
                                    WriteVerbose($"Filtering DocumentObj using MetadataFilter.length[{metadataFilterFields.ToXml().Length}] RequestedMetadata.length[{requestedMetadata.ToXml().Length}]");
                                    xmlIshObjects = IshSession.DocumentObj25.RetrieveMetadata(logicalIdBatch.ToArray(),
                                        DocumentObj25ServiceReference.StatusFilter.ISHNoStatusFilter,
                                        metadataFilterFields.ToXml(),
                                        requestedMetadata.ToXml());
                                    var documentIshObjects = new IshObjects(ISHType, xmlIshObjects);
                                    returnIshObjects.AddRange(documentIshObjects.Objects);
                                }
                            }
                        }

                        // Handle all publications
                        var publicationLogicalIds =
                               ishObjects.Objects
                                    .Where(ishObject => ishObject.IshType == Enumerations.ISHType.ISHPublication)
                                    .Select(ishObject => ishObject.IshRef)
                                    .ToList();
                        if (publicationLogicalIds.Any())
                        {
                            Enumerations.ISHType[] ISHType = { Enumerations.ISHType.ISHPublication };
                            IshFields requestedMetadata = IshSession.IshTypeFieldSetup.ToIshRequestedMetadataFields(IshSession.DefaultRequestedMetadata, ISHType, new IshFields(RequestedMetadata), Enumerations.ActionMode.Read);

                            // Devides the list of LogicalIds in different lists that all have maximally MetadataBatchSize elements
                            List<List<string>> devidedPublicationLogicalIdsList = DevideListInBatches<string>(publicationLogicalIds, IshSession.MetadataBatchSize);
                            int currentLogicalIdCount = 0;
                            foreach (List<string> logicalIdBatch in devidedPublicationLogicalIdsList)
                            {
                                currentLogicalIdCount += logicalIdBatch.Count;
                                WriteDebug($"Retrieving PublicationOutput.length[{logicalIdBatch.Count}] MetadataFilter.length[{metadataFilterFields.ToXml().Length}] {currentLogicalIdCount}/{publicationLogicalIds.Count}");

                                if (VersionFilter != null && VersionFilter.Length > 0)
                                {
                                    WriteDebug($"Filtering PublicationOutput using VersionFilter[{VersionFilter}]");
                                    xmlIshObjects = IshSession.PublicationOutput25.RetrieveVersionMetadata(logicalIdBatch.ToArray(), VersionFilter, "");
                                    var publicationIshObjects = new IshObjects(xmlIshObjects);

                                    if (publicationIshObjects.Objects.Length > 0)
                                    {
                                        WriteVerbose($"Filtering PublicationOutput using MetadataFilter.length[{metadataFilterFields.ToXml().Length}] and LanguagesFilter.Length[{LanguagesFilter.Length}]");
                                        var versionRefs = publicationIshObjects.Objects
                                            .Select(ishObject => Convert.ToInt64(ishObject.ObjectRef[Enumerations.ReferenceType.Version]))
                                            .ToList();
                                        xmlIshObjects =
                                            IshSession.PublicationOutput25.RetrieveMetadataByIshVersionRefs(versionRefs.ToArray(),
                                                PublicationOutput25ServiceReference.StatusFilter.ISHNoStatusFilter, metadataFilterFields.ToXml(),
                                                requestedMetadata.ToXml());
                                        publicationIshObjects = new IshObjects(ISHType, xmlIshObjects);
                                        returnIshObjects.AddRange(publicationIshObjects.Objects);
                                    }
                                }
                                else
                                {
                                    WriteVerbose($"Filtering PublicationOutput using MetadataFilter.length[{metadataFilterFields.ToXml().Length}]");
                                    xmlIshObjects = IshSession.PublicationOutput25.RetrieveMetadata(logicalIdBatch.ToArray(),
                                        PublicationOutput25ServiceReference.StatusFilter.ISHNoStatusFilter,
                                        metadataFilterFields.ToXml(),
                                        requestedMetadata.ToXml());
                                    var publicationIshObjects = new IshObjects(ISHType, xmlIshObjects);
                                    returnIshObjects.AddRange(publicationIshObjects.Objects);
                                }
                            }
                        }
                    }
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
