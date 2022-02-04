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
using Trisoft.ISHRemote.HelperClasses;
using Trisoft.ISHRemote.ExtensionMethods;
using System.Linq;

namespace Trisoft.ISHRemote.Cmdlets.Folder
{

    /// <summary>
    /// <para type="synopsis">The Get-IshFolder cmdlet retrieves metadata for the folders by providing one of the following input data:
    /// - FolderPath string with the separated full folder path
    /// - FolderIds array containing identifiers of the folders
    /// - BaseFolder enum value referencing the specified root folder
    /// - IshFolder[] array passed through the pipeline Query and Reference folders are not supported.</para>
    /// <para type="description">The Get-IshFolder cmdlet retrieves metadata for the folders by providing one of the following input data:
    /// - FolderPath string with the separated full folder path
    /// - FolderIds array containing identifiers of the folders
    /// - BaseFolder enum value referencing the specified root folder
    /// - IshFolder[] array passed through the pipeline Query and Reference folders are not supported.</para>
    /// </summary>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/ISHWS/" -PSCredential Admin
    /// Get-IshFolder -FolderPath "\General\__ISHRemote\Add-IshPublicationOutput\Pub"
    /// </code>
    /// <para>New-IshSession will submit into SessionState, so it can be reused by this cmdlet. Returns the IshFolder object.</para>
    /// </example>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/ISHWS/" -PSCredential Admin
    /// (Get-IshFolder -BaseFolder Data).name
    /// </code>
    /// <para>New-IshSession will submit into SessionState, so it can be reused by this cmdlet. Returns the name of the root data folder, typically called 'General'.</para>
    /// </example>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/ISHWS/" -PSCredential "Admin"
    /// $requestedMetadata = Set-IshMetadataFilterField -Name "FNAME" -Level "None"
    /// $folderId = 7598 # provide a real folder identifier
    /// $ishFolder = Get-IshFolder -FolderId $folderId -RequestedMetaData $requestedMetadata
    /// $retrievedFolderName = $ishFolder.name
    /// </code>
    /// <para>Get folder name using Id with explicit requested metadata</para>
    /// </example>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/ISHWS/" -PSCredential "Admin"
    /// $ishFolders = Get-IshFolder -FolderPath "General\Myfolder" -FolderTypeFilter @("ISHModule", "ISHMasterDoc", "ISHLibrary") -Recurse
    /// </code>
    /// <para>Get folders recursively with filtering on folder type</para>
    /// </example>
    [Cmdlet(VerbsCommon.Get, "IshFolder", SupportsShouldProcess = false)]
    [OutputType(typeof(IshFolder))]
    public sealed class GetIshFolder : FolderCmdlet
    {
        /// <summary>
        /// <para type="description">The IshSession variable holds the authentication and contract information. This object can be initialized using the New-IshSession cmdlet.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "FolderIdGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "FolderPathGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshFolderGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "BaseFolderGroup")]
        [ValidateNotNullOrEmpty]
        public IshSession IshSession { get; set; }

        /// <summary>
        /// <para type="description">Separated string with the full folder path, e.g. "General\Project\Topics"</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "FolderPathGroup"), ValidateNotNull]
        public string FolderPath { get; set; }

        /// <summary>
        /// <para type="description">Unique folder identifier</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "FolderIdGroup"), ValidateNotNullOrEmpty, ValidateRange(1, long.MaxValue)]
        public long FolderId { get; set; }

        /// <summary>
        /// <para type="description">The BaseFolder enumeration to get subfolders for the specified root folder</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "BaseFolderGroup"), ValidateNotNullOrEmpty]
        public Enumerations.BaseFolder BaseFolder { get; set; }

        /// <summary>
        /// <para type="description">Folders for which to retrieve the metadata. This array can be passed through the pipeline or explicitly passed via the parameter.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshFolderGroup")]
        [AllowEmptyCollection]
        public IshFolder[] IshFolder { get; set; }

        /// <summary>
        /// <para type="description">The metadata fields to retrieve</para>
        /// </summary>     
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "FolderIdGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "FolderPathGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshFolderGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "BaseFolderGroup")]
        [ValidateNotNullOrEmpty]
        public IshField[] RequestedMetadata { get; set; }

        /// <summary>
        /// <para type="description">Perform recursive retrieval of the provided incoming folder(s)</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "FolderIdGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "FolderPathGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshFolderGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "BaseFolderGroup")]
        public SwitchParameter Recurse { get; set; }

        /// <summary>
        /// <para type="description">Perform recursive retrieval of up to Depth of the provided incoming folder(s)</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "FolderIdGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "FolderPathGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshFolderGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "BaseFolderGroup")]
        public int Depth
        {
            get { return _maxDepth; }
            set { _maxDepth = value; }
        }

        /// <summary>
        /// <para type="description">Recursive retrieval will loop all folder, this filter will only return folder matching the filter to the pipeline</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "FolderIdGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "FolderPathGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshFolderGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "BaseFolderGroup")]
        [ValidateNotNullOrEmpty]
        public Enumerations.IshFolderType[] FolderTypeFilter { get; set; }

        #region Private fields 
        /// <summary>
        /// Initially holds incoming IshObject entries from the pipeline to correct the incorrect array-objects from Trisoft.Automation
        /// </summary>
        private readonly List<IshFolder> _retrievedIshFolders = new List<IshFolder>();
        /// <summary>
        /// Initially holds incoming folder id entries from the pipeline to correct the incorrect array-objects from Trisoft.Automation
        /// </summary>
        private readonly List<long> _retrievedFolderIds = new List<long>();
        /// <summary>
        /// Requested metadata to be shared across all (recursive) calls
        /// </summary>
        private IshFields _requestedMetadata;
        /// <summary>
        /// Initially set to max recursive depth we can handle 
        /// </summary>
        private int _maxDepth = int.MaxValue;
        #endregion

        protected override void BeginProcessing()
        {
            if (IshSession == null) { IshSession = (IshSession)SessionState.PSVariable.GetValue(ISHRemoteSessionStateIshSession); }
            if (IshSession == null) { throw new ArgumentException(ISHRemoteSessionStateIshSessionException); }
            WriteDebug($"Using IshSession[{IshSession.Name}] from SessionState.{ISHRemoteSessionStateIshSession}");
            base.BeginProcessing();
        }

        /// <summary>
        /// Process the Get-IshFolder commandlet.
        /// </summary>
        /// <exception cref="TrisoftAutomationException"></exception>
        /// <exception cref="Exception"></exception>
        protected override void ProcessRecord()
        {
            try
            {
                switch (ParameterSetName)
                {
                    case "IshFolderGroup":
                        foreach (IshFolder ishFolder in IshFolder)
                        {
                            _retrievedIshFolders.Add(ishFolder);
                        }
                        break;
                    case "FolderIdGroup":
                        _retrievedFolderIds.Add(FolderId);
                        break;
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

        /// <summary>
        /// Process the Get-IshFolder commandlet.
        /// </summary>
        /// <exception cref="TrisoftAutomationException"></exception>
        /// <exception cref="Exception"></exception>
        /// <remarks>Writes an <see cref="IshFolder"/> object to the pipeline.</remarks>
        protected override void EndProcessing()
        {
            try
            {
                // 1. Validating the input
                WriteDebug("Validating");

                List<IshFolder> returnIshFolders = new List<IshFolder>();

                // 2. Doing Retrieve
                WriteDebug("Retrieving");
                _requestedMetadata = IshSession.IshTypeFieldSetup.ToIshRequestedMetadataFields(IshSession.DefaultRequestedMetadata, ISHType, new IshFields(RequestedMetadata), Enumerations.ActionMode.Read);

                if (ParameterSetName == "IshFolderGroup" && _retrievedIshFolders.Count > 0)
                {
                    var folderCardIds = _retrievedIshFolders.Select(ishFolder => Convert.ToInt64(ishFolder.IshFolderRef)).ToList();
                    WriteDebug($"Retrieving CardIds.length[{folderCardIds.Count}] RequestedMetadata.length[{_requestedMetadata.ToXml().Length}] 0/{folderCardIds.Count}");
                    // Devides the list of folder card ids in different lists that all have maximally MetadataBatchSize elements
                    List<List<long>> devidedFolderCardIdsList = DevideListInBatches<long>(folderCardIds, IshSession.MetadataBatchSize);
                    int currentFolderCardIdCount = 0;
                    foreach (List<long> folderCardIdBatch in devidedFolderCardIdsList)
                    {
                        // Process card ids in batches
                        switch (IshSession.Protocol)
                        {
                            case Enumerations.Protocol.OpenApiBasicAuthentication:
                                foreach (long folderId in folderCardIdBatch)
                                {
                                    IEnumerable<OpenApi.Folder> folders = (IEnumerable<OpenApi.Folder>)IshSession.OpenApi30Service.GetFolderObjectList(
                                        folderId.ToString(),
                                        OpenApi.FolderObjectType.Folders,
                                        IshSession.UserLanguage,
                                        string.Empty,
                                        OpenApi.SelectedProperties.Descriptive,
                                        OpenApi.FieldGroup.Basic,
                                        _requestedMetadata.Fields().Select(f => f.Name),
                                        false,
                                        false
                                    );

                                    foreach (OpenApi.Folder folder in folders)
                                    {
                                        IshFolder ishFolder = new IshFolder(long.Parse(folder.Id), folder.FolderType.ToIshFolderType(), folder.Fields.ToIshFields());
                                        returnIshFolders.Add(ishFolder);
                                    }
                                }
                                break;

                            case Enumerations.Protocol.AsmxAuthenticationContext:
                                var response = IshSession.Folder25.RetrieveMetadataByIshFolderRefs(new Folder25ServiceReference.RetrieveMetadataByIshFolderRefsRequest()
                                {
                                    psAuthContext = IshSession.AuthenticationContext,
                                    palFolderRefs = folderCardIdBatch.ToArray(),
                                    psXMLRequestedMetaData = _requestedMetadata.ToXml()
                                });
                                IshSession.AuthenticationContext = response.psAuthContext;
                                string xmlIshFolders = response.psOutXMLFolderList;
                                IshFolders retrievedObjects = new IshFolders(xmlIshFolders);
                                returnIshFolders.AddRange(retrievedObjects.Folders);
                                break;
                        }
                        currentFolderCardIdCount += folderCardIdBatch.Count;
                        WriteDebug($"Retrieving CardIds.length[{ folderCardIdBatch.Count}] RequestedMetadata.length[{ _requestedMetadata.ToXml().Length}] {currentFolderCardIdCount}/{folderCardIds.Count}");
                    }
                }
                else if (ParameterSetName == "FolderIdGroup" && _retrievedFolderIds.Count > 0)
                {
                    WriteDebug($"Retrieving CardIds.length[{ _retrievedFolderIds.Count}] RequestedMetadata.length[{_requestedMetadata.ToXml().Length}] 0/{_retrievedFolderIds.Count}");
                    // Devides the list of folder card ids in different lists that all have maximally MetadataBatchSize elements
                    List<List<long>> devidedFolderCardIdsList = DevideListInBatches<long>(_retrievedFolderIds, IshSession.MetadataBatchSize);
                    int currentFolderCardIdCount = 0;
                    foreach (List<long> folderCardIdBatch in devidedFolderCardIdsList)
                    {
                        // Process card ids in batches
                        IshFolders retrievedFolders = null;
                        switch (IshSession.Protocol)
                        {
                            case Enumerations.Protocol.OpenApiBasicAuthentication:
                                OpenApi.GetFolderList folderList = new OpenApi.GetFolderList() 
                                { 
                                    Ids = folderCardIdBatch.Select(f => f.ToString()).ToList(),
                                    Fields = _requestedMetadata.ToOpenApiRequestedFields()
                                };

                                IEnumerable<OpenApi.Folder> openApiFolders = IshSession.OpenApi30Service.GetFolderList(folderList);
                                IList<IshFolder> folders = new List<IshFolder>(openApiFolders.Count());
                                foreach (OpenApi.Folder openApifolder in openApiFolders)
                                {
                                    IshFolder ishFolder = new IshFolder(long.Parse(openApifolder.Id), openApifolder.FolderType.ToIshFolderType(), openApifolder.Fields.ToIshFields());
                                    folders.Add(ishFolder);
                                }
                                retrievedFolders = new IshFolders(folders.ToArray());
                                break;

                            case Enumerations.Protocol.AsmxAuthenticationContext:
                                var response = IshSession.Folder25.RetrieveMetadataByIshFolderRefs(new Folder25ServiceReference.RetrieveMetadataByIshFolderRefsRequest()
                                {
                                    psAuthContext = IshSession.AuthenticationContext,
                                    palFolderRefs = folderCardIdBatch.ToArray(),
                                    psXMLRequestedMetaData = _requestedMetadata.ToXml()
                                });
                                IshSession.AuthenticationContext = response.psAuthContext;
                                string xmlIshFolders = response.psOutXMLFolderList;
                                retrievedFolders = new IshFolders(xmlIshFolders);
                                break;
                        }
                        returnIshFolders.AddRange(retrievedFolders.Folders);
                        currentFolderCardIdCount += folderCardIdBatch.Count;
                        WriteDebug($"Retrieving CardIds.length[{folderCardIdBatch.Count}] RequestedMetadata.length[{_requestedMetadata.ToXml().Length}] {currentFolderCardIdCount}/{_retrievedFolderIds.Count}");
                    }
                }
                else if (ParameterSetName == "FolderPathGroup")
                {
                    // Retrieve using provided parameter FolderPath
                    // Parse FolderPath input parameter: get basefolderName(1st element of an array)
                    string folderPath = FolderPath;
                    string[] folderPathElements = folderPath.Split(
                        new string[] { IshSession.FolderPathSeparator }, StringSplitOptions.RemoveEmptyEntries);
                    string baseFolderLabel = folderPathElements[0];

                    // remaining folder path elements
                    string[] folderPathTrisoft = new string[folderPathElements.Length - 1];
                    Array.Copy(folderPathElements, 1, folderPathTrisoft, 0, folderPathElements.Length - 1);

                    WriteDebug($"FolderPath[{ folderPath}]");
                    IshFolders retrievedFolders = null;
                    switch (IshSession.Protocol)
                    {
                        case Enumerations.Protocol.OpenApiBasicAuthentication:
                        // TODO [Must] Add OpenApi implementation
                        case Enumerations.Protocol.AsmxAuthenticationContext:
                            var response = IshSession.Folder25.GetMetaData(new Folder25ServiceReference.GetMetaDataRequest()
                            {
                                psAuthContext = IshSession.AuthenticationContext,
                                peBaseFolder = BaseFolderLabelToEnum(IshSession, baseFolderLabel),
                                pasFolderPath = folderPathTrisoft,
                                psXMLRequestedMetaData = _requestedMetadata.ToXml()
                            });
                            IshSession.AuthenticationContext = response.psAuthContext;
                            string xmlIshFolder = response.psOutXMLFolderList;
                            retrievedFolders = new IshFolders(xmlIshFolder, "ishfolder");
                            break;
                    }
                    returnIshFolders.AddRange(retrievedFolders.Folders);
                }
                else if (ParameterSetName == "BaseFolderGroup")
                {
                    // Retrieve using BaseFolder string (enumeration)
                    var baseFolder = EnumConverter.ToBaseFolder<Folder25ServiceReference.eBaseFolder>(BaseFolder);
                    WriteDebug($"BaseFolder[{baseFolder}]");
                    IshFolders retrievedFolders = null;
                    switch (IshSession.Protocol)
                    {
                        case Enumerations.Protocol.OpenApiBasicAuthentication:
                            IEnumerable<OpenApi.Folder> folders = IshSession.OpenApi30Service.GetRootFolderList(
                                OpenApi.SelectedProperties.ListOfValues, 
                                OpenApi.FieldGroup.None, 
                                _requestedMetadata.Fields().Select(f => f.Name), 
                                false);

                            IList<IshFolder> basefolders = new List<IshFolder>();
                            foreach (OpenApi.Folder openApifolder in folders.Where(f => f.BaseFolder == BaseFolder.ToIshBaseFolder()))
                            {
                                IshFolder ishFolder = new IshFolder(long.Parse(openApifolder.Id), openApifolder.FolderType.ToIshFolderType(), openApifolder.Fields.ToIshFields());
                                basefolders.Add(ishFolder);
                            }
                            retrievedFolders = new IshFolders(basefolders.ToArray());

                            break;

                        case Enumerations.Protocol.AsmxAuthenticationContext:
                            var response = IshSession.Folder25.GetMetaData(new Folder25ServiceReference.GetMetaDataRequest()
                            {
                                psAuthContext = IshSession.AuthenticationContext,
                                peBaseFolder = baseFolder,
                                pasFolderPath = new string[0],
                                psXMLRequestedMetaData = _requestedMetadata.ToXml()
                            });
                            IshSession.AuthenticationContext = response.psAuthContext;
                            string xmlIshFolders = response.psOutXMLFolderList;
                            retrievedFolders = new IshFolders(xmlIshFolders, "ishfolder");
                            break;
                    }
                    returnIshFolders.AddRange(retrievedFolders.Folders);
                }

                // 3b. Write it
                if (!Recurse)
                {
                    if (FolderTypeFilter == null)
                    {
                        WriteDebug($"returned object count[{returnIshFolders.Count}]");
                        WriteObject(IshSession, ISHType, returnIshFolders.ConvertAll(x => (IshBaseObject)x), true);
                    }
                    else
                    {
                        List<IshFolder> filteredIshFolders = new List<IshFolder>();
                        foreach (var returnIshFolder in returnIshFolders)
                        {
                            if (FolderTypeFilter.Contains(returnIshFolder.IshFolderType))
                            {
                                filteredIshFolders.Add(returnIshFolder);
                            }
                            else
                            {
                                string folderName = returnIshFolder.IshFields.GetFieldValue("FNAME", Enumerations.Level.None, Enumerations.ValueType.Value);
                                string folderpath = returnIshFolder.IshFields.GetFieldValue("FISHFOLDERPATH", Enumerations.Level.None, Enumerations.ValueType.Value);
                                WriteVerbose(folderpath.Replace(IshSession.Separator, IshSession.FolderPathSeparator) + IshSession.FolderPathSeparator + folderName + " skipped");
                            }
                        }

                        WriteDebug($"returned object count after filtering[{filteredIshFolders.Count}]");
                        WriteObject(IshSession, ISHType, filteredIshFolders.ConvertAll(x => (IshBaseObject)x), true);
                    }

                }
                else
                {
                    WriteParentProgress("Recursive folder retrieve...", 0, returnIshFolders.Count);
                    foreach (IshFolder ishFolder in returnIshFolders)
                    {
                        WriteParentProgress("Recursive folder retrieve...", ++_parentCurrent, returnIshFolders.Count);
                        if (_maxDepth > 0)
                        {
                            RetrieveRecursive(ishFolder, 0, _maxDepth);
                        }
                    }
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
            finally
            {
                base.EndProcessing();
            }
        }
        private void RetrieveRecursive(IshFolder ishFolder, int currentDepth, int maxDepth)
        {
            // put them on the pipeline depth-first-traversel
            string folderName = ishFolder.IshFields.GetFieldValue("FNAME", Enumerations.Level.None, Enumerations.ValueType.Value);

            // only return IshFolder objects to the pipeline if the filter is either not set, or the current filter passes the filter criteria
            if (FolderTypeFilter != null && FolderTypeFilter.Length > 0)
            {
                if (FolderTypeFilter.Contains<Enumerations.IshFolderType>(ishFolder.IshFolderType))
                {
                    WriteVerbose(new string('>', currentDepth) + IshSession.FolderPathSeparator + folderName + IshSession.FolderPathSeparator);
                    WriteObject(IshSession, ISHType, ishFolder, true);
                }
                else
                {
                    WriteVerbose(new string('>', currentDepth) + IshSession.FolderPathSeparator + folderName + IshSession.FolderPathSeparator + " skipped");
                }
            }
            else
            {
                WriteVerbose(new string('>', currentDepth) + IshSession.FolderPathSeparator + folderName + IshSession.FolderPathSeparator);
                WriteObject(IshSession, ISHType, ishFolder, true);
            }

            if (currentDepth < (maxDepth - 1))
            {
                WriteDebug($"RetrieveRecursive IshFolderRef[{ishFolder.IshFolderRef}] folderName[{folderName}] ({currentDepth}<{maxDepth})");
                IshFolders retrievedFolders = null;
                switch (IshSession.Protocol)
                {
                    case Enumerations.Protocol.OpenApiBasicAuthentication:
                        IEnumerable<OpenApi.BaseObject> baseObjects = IshSession.OpenApi30Service.GetFolderObjectList(
                            ishFolder.IshFolderRef.ToString(),
                            OpenApi.FolderObjectType.Folders,
                            string.Empty,
                            string.Empty,
                            OpenApi.SelectedProperties.ListOfValues,
                            OpenApi.FieldGroup.None,
                            new string [] { "FNAME" },
                            false,
                            false);

                        IList<IshFolder> subfolders = new List<IshFolder>();
                        foreach (OpenApi.Folder openApifolder in baseObjects)
                        {
                            IshFolder ishSubFolder = new IshFolder(long.Parse(openApifolder.Id), openApifolder.FolderType.ToIshFolderType(), openApifolder.Fields.ToIshFields());
                            subfolders.Add(ishSubFolder);
                        }
                        retrievedFolders = new IshFolders(subfolders.ToArray());

                        break;

                    case Enumerations.Protocol.AsmxAuthenticationContext:
                        var responseSubFolders = IshSession.Folder25.GetSubFoldersByIshFolderRef(new Folder25ServiceReference.GetSubFoldersByIshFolderRefRequest()
                        {
                            psAuthContext = IshSession.AuthenticationContext,
                            plFolderRef = ishFolder.IshFolderRef
                        });
                        IshSession.AuthenticationContext = responseSubFolders.psAuthContext;
                        string xmlIshFolders = responseSubFolders.psOutXMLFolderList;
                        // GetSubFolders contains ishfolder for the parent folder + ishfolder inside for the subfolders
                        retrievedFolders = new IshFolders(xmlIshFolders, "ishfolder/ishfolder");
                        break;
                }

                if (retrievedFolders.Ids.Length > 0)
                {
                    // Add the required fields (needed for pipe operations)
                    switch (IshSession.Protocol)
                    {
                        case Enumerations.Protocol.OpenApiBasicAuthentication:
                            OpenApi.GetFolderList folderList = new OpenApi.GetFolderList()
                            {
                                Ids = retrievedFolders.Ids.Select(f => f.ToString()).ToList(),
                                Fields = _requestedMetadata.ToOpenApiRequestedFields()
                            };

                            IEnumerable<OpenApi.Folder> openApiFolders = IshSession.OpenApi30Service.GetFolderList(folderList);
                            IList<IshFolder> folders = new List<IshFolder>(openApiFolders.Count());
                            foreach (OpenApi.Folder openApifolder in openApiFolders)
                            {
                                IshFolder ishretrievedFolder = new IshFolder(long.Parse(openApifolder.Id), openApifolder.FolderType.ToIshFolderType(), openApifolder.Fields.ToIshFields());
                                folders.Add(ishretrievedFolder);
                            }
                            retrievedFolders = new IshFolders(folders.ToArray());
                            break;

                        case Enumerations.Protocol.AsmxAuthenticationContext:
                            var responseRetrieve = IshSession.Folder25.RetrieveMetadataByIshFolderRefs(new Folder25ServiceReference.RetrieveMetadataByIshFolderRefsRequest()
                            {
                                psAuthContext = IshSession.AuthenticationContext,
                                palFolderRefs = retrievedFolders.Ids,
                                psXMLRequestedMetaData = _requestedMetadata.ToXml()
                            });
                            IshSession.AuthenticationContext = responseRetrieve.psAuthContext;
                            string xmlIshFolders = responseRetrieve.psOutXMLFolderList;
                            retrievedFolders = new IshFolders(xmlIshFolders);
                            break;
                    }
                    // sort them
                    ++currentDepth;
                    IshFolder[] sortedFolders = retrievedFolders.SortedFolders;
                    WriteParentProgress("Recursive folder retrieve...", _parentCurrent, _parentTotal + sortedFolders.Count());
                    foreach (IshFolder retrievedIshFolder in sortedFolders)
                    {
                        WriteParentProgress("Recursive folder retrieve...", ++_parentCurrent, _parentTotal);
                        RetrieveRecursive(retrievedIshFolder, currentDepth, maxDepth);
                    }
                }
            }
        }
    }
}
