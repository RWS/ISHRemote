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
    /// * The cmdlet will return an object for all the latest versions for all output formats and all language combinations for a publication folder</para>
    /// <para type="description">The Get-IshFolderContent cmdlet returns all document objects or publication outputs stored inside a given folder.
    /// You can provide filters to reduce the amount of objects returned, but if you don't provide any:
    /// * The cmdlet will return an object for all the latest versions in all language and resolution for a document object folder
    /// * The cmdlet will return an object for all the latest versions for all output formats and all language combinations for a publication folder</para>
    /// </summary>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/ISHWS/" -PSCredential "Admin"
    /// $folderId = 7775 #use correct folder id
    /// $ishObjects = Get-IshFolderContent -FolderId $folderId
    /// </code>
    /// <para>New-IshSession will submit into SessionState, so it can be reused by this cmdlet. Retrieve contents of a given folder</para>
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
        /// <para type="description">The version filter to limit the amount of objects returned. When no filter is supplied, latest version objects will be returned.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "FolderIdGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "FolderPathGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "BaseFolderGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshFolderGroup")]
        [ValidateNotNullOrEmpty]
        public string VersionFilter
        {
            get { return _versionFilter; }
            set { _versionFilter = value; }
        }

        /// <summary>
        /// <para type="description">The langauges filter to limit the amount of objects returned. When no languages filter is supplied, all object languages will be returned.</para>
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




        #region Private fields
        /// <summary>
        /// Private field to store and provide defaults for non-mandatory parameters
        /// </summary>
        private string _versionFilter = "latest";
        private string[] _languagesFilter = { };
        #endregion

        /// <summary>
        /// <para type="description">Folder for which to retrieve the content. This array can be passed through the pipeline or explicitly passed via the parameter.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "IshFolderGroup")]
        public IshFolder[] IshFolder { get; set; }

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

                if (IshFolder != null)
                {
                    // 1a. Retrieve using IshFolder object
                    foreach (IshFolder ishFolder in IshFolder)
                    {
                        returnFolderIds.Add(ishFolder.IshFolderRef);
                    }
                }
                else if (FolderId != 0)
                {
                    // 1b. Retrieve using FolderId
                    WriteDebug($"folderId[{FolderId}]");
                    returnFolderIds.Add(FolderId);
                }
                else if (FolderPath != null)
                {
                    // 1c. Retrieve using provided parameter FolderPath
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
                    IshFolders ishFolder = new IshFolders(xmlIshFolder, "ishfolder");
                    returnFolderIds.Add(ishFolder.Folders[0].IshFolderRef);
                }
                else
                {
                    // 1d. Retrieve subfolder(s) from the specified root folder using BaseFolder string (enumeration)
                    var baseFolder = EnumConverter.ToBaseFolder<Folder25ServiceReference.BaseFolder>(BaseFolder);
                    string xmlIshFolders = IshSession.Folder25.GetMetadata(
                        baseFolder,
                        new string[0],
                        "");
                    IshFolders retrievedFolders = new IshFolders(xmlIshFolders, "ishfolder");
                    returnFolderIds.Add(retrievedFolders.Folders[0].IshFolderRef);
                }

                List<IshObject> returnIshObjects = new List<IshObject>();
                int current = 0;
                foreach (long returnFolderId in returnFolderIds)
                {
                    // 2. Doing Retrieve
                    WriteDebug($"folderId[{returnFolderId}] {++current}/{returnFolderIds.Count}");
                    string xmlIshObjects = IshSession.Folder25.GetContents(returnFolderId);
                    var ishObjects = new IshObjects(xmlIshObjects);

                    WriteDebug("Retrieving Language Objects");
                    if (ishObjects.Ids.Length > 0)
                    {
                        // First handle all documents/illustrations
                        var documentLogicalIds = ishObjects.Objects
                                    .Where(ishObject => ishObject.IshType != Enumerations.ISHType.ISHPublication)
                                    .Select(ishObject => ishObject.IshRef)
                                    .ToList();
                        if (documentLogicalIds.Any())
                        {
                            Enumerations.ISHType[] ISHType = { Enumerations.ISHType.ISHIllustration, Enumerations.ISHType.ISHLibrary, Enumerations.ISHType.ISHMasterDoc, Enumerations.ISHType.ISHModule, Enumerations.ISHType.ISHTemplate };
                            IshFields requestedMetadata = IshSession.IshTypeFieldSetup.ToIshRequestedMetadataFields(IshSession.DefaultRequestedMetadata, ISHType, new IshFields(), Enumerations.ActionMode.Read);

                            xmlIshObjects = IshSession.DocumentObj25.RetrieveLanguageMetadata(documentLogicalIds.ToArray(),
                                VersionFilter, LanguagesFilter,
                                new string[0], DocumentObj25ServiceReference.StatusFilter.ISHNoStatusFilter, requestedMetadata.ToXml());
                            var documentIshObjects = new IshObjects(ISHType, xmlIshObjects);
                            returnIshObjects.AddRange(documentIshObjects.Objects);
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
                            IshFields requestedMetadata = IshSession.IshTypeFieldSetup.ToIshRequestedMetadataFields(IshSession.DefaultRequestedMetadata, ISHType, new IshFields(), Enumerations.ActionMode.Read);
                            xmlIshObjects = IshSession.PublicationOutput25.RetrieveVersionMetadata(publicationLogicalIds.ToArray(),
                                VersionFilter, "");
                            var publicationIshObjects = new IshObjects(xmlIshObjects);
                            if (publicationIshObjects.Objects.Length > 0)
                            {
                                var metadataFilterFields = new IshFields();
                                if (LanguagesFilter != null && LanguagesFilter.Length > 0)
                                {
                                    metadataFilterFields.AddField(new IshMetadataFilterField("DOC-LANGUAGE", Enumerations.Level.Lng,
                                        Enumerations.FilterOperator.In,
                                        String.Join(IshSession.Seperator, LanguagesFilter),
                                        Enumerations.ValueType.Value));
                                }
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
