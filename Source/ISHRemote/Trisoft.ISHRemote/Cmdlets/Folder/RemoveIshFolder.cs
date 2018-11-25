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
using System.Linq;

namespace Trisoft.ISHRemote.Cmdlets.Folder
{
    /// <summary>
    /// <para type="synopsis">The Remove-IshFolder cmdlet removes the repository folders that are passed through the pipeline or determined via provided parameters Query and Reference folders are not supported.</para>
    /// <para type="description">The Remove-IshFolder cmdlet removes the repository folders that are passed through the pipeline or determined via provided parameters Query and Reference folders are not supported.</para>
    /// </summary>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/InfoShareWS/" -IshUserName "username" -IshUserPassword  "userpassword"
    /// Remove-IshFolder -FolderId "674580"
    /// </code>
    /// <para>New-IshSession will submit into SessionState, so it can be reused by this cmdlet. Remove folder with specified Id</para>
    /// </example>
    [Cmdlet(VerbsCommon.Remove, "IshFolder", SupportsShouldProcess = true)]
    public sealed class RemoveIshFolder : FolderCmdlet
    {

        /// <summary>
        /// <para type="description">The IshSession variable holds the authentication and contract information. This object can be initialized using the New-IshSession cmdlet.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "FolderIdGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "FolderPathGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshFoldersGroup")]
        [ValidateNotNullOrEmpty]
        public IshSession IshSession { get; set; }

        /// <summary>
        /// <para type="description">Full path to the folder that needs to be removed. Use the IshSession.FolderPathSeparator.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "FolderPathGroup"), ValidateNotNullOrEmpty]
        public string FolderPath { get; set; }

        /// <summary>
        /// <para type="description">Identifier of the folder to be removed</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "FolderIdGroup"), ValidateNotNullOrEmpty]
        public long FolderId { get; set; }

        /// <summary>
        /// <para type="description">Array with the folders to remove. This array can be passed through the pipeline or explicitly passed via the parameter.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "IshFoldersGroup")]
        [AllowEmptyCollection]
        public IshFolder[] IshFolder { get; set; }

        /// <summary>
        /// <para type="description">Perform recursive retrieval of the provided incoming folder(s)</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "FolderPathGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshFoldersGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "FolderIdGroup")]
        public SwitchParameter Recurse { get; set; }

        protected override void BeginProcessing()
        {
            if (IshSession == null) { IshSession = (IshSession)SessionState.PSVariable.GetValue(ISHRemoteSessionStateIshSession); }
            if (IshSession == null) { throw new ArgumentNullException(ISHRemoteSessionStateIshSessionException); }
            WriteDebug($"Using IshSession[{IshSession.Name}] from SessionState.{ISHRemoteSessionStateIshSession}");
            base.BeginProcessing();
        }

        /// <summary>
        /// Process the Remove-IshFolder commandlet.
        /// </summary>
        /// <exception cref="TrisoftAutomationException"></exception>
        /// <exception cref="Exception"></exception>
        protected override void ProcessRecord()
        {
            try
            {
                // 1. Validating the input
                // 2. Doing Remove
                WriteDebug("Removing");
                if (IshFolder != null)
                {
                    long folderId;
                    // 2a. Remove using IshFolder[] pipeline
                    IshFolder[] ishFolders = IshFolder;
                    int current = 0;
                    foreach (IshFolder ishFolder in ishFolders)
                    {
                        // read "folderRef" from the ishFolder object
                        folderId = ishFolder.IshFolderRef;
                        WriteDebug($"folderId[{folderId}] {++current}/{ishFolders.Length}");
                        if (!Recurse)
                        {
                            if (ShouldProcess(Convert.ToString(folderId)))
                            {
                                IshSession.Folder25.Delete(folderId);
                            }
                        }
                        else
                        {
                            DeleteRecursive(ishFolder, 0);
                        }
                    }
                }
                else if (FolderId != 0)
                {
                    // 2b. Remove using provided parameters (not piped IshFolder)
                    long folderId = FolderId;
                    WriteDebug($"folderId[{folderId}]");
                    if (!Recurse)
                    {
                        if (ShouldProcess(Convert.ToString(folderId)))
                        {
                            IshSession.Folder25.Delete(folderId);
                        }
                    }
                    else
                    {
                        IshFields requestedMetadata = IshSession.IshTypeFieldSetup.ToIshRequestedMetadataFields(IshSession.DefaultRequestedMetadata, ISHType, new IshFields(), Enumerations.ActionMode.Read);
                        string xmlIshFolder = IshSession.Folder25.GetMetadataByIshFolderRef(folderId, requestedMetadata.ToXml());
                        IshFolders ishFolders = new IshFolders(xmlIshFolder, "ishfolder");
                        DeleteRecursive(ishFolders.Folders[0], 0);
                    }
                }
                else if (FolderPath != null)
                {
                    // 2c. Retrieve using provided parameter FolderPath
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
                    long folderId = ishFolder.Folders[0].IshFolderRef;
                    if (!Recurse)
                    {
                        if (ShouldProcess(Convert.ToString(folderId)))
                        {
                            IshSession.Folder25.Delete(folderId);
                        }
                    }
                    else
                    {
                        WriteParentProgress("Recursive folder remove...", _parentCurrent, 1);
                        DeleteRecursive(ishFolder.Folders[0], 0);
                    }
                }
                else
                {
                    WriteDebug("How did you get here? Probably provided  too little parameters.");
                }
                WriteVerbose("returned object count[0]");
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
        /// Recursive delete of folder, expects folders to be empty
        /// </summary>
        /// <param name="ishFolder"></param>
        /// <param name="currentDepth"></param>
        private void DeleteRecursive(IshFolder ishFolder, int currentDepth)
        {            
            string folderName = ishFolder.IshFields.GetFieldValue("FNAME", Enumerations.Level.None, Enumerations.ValueType.Value);
            WriteVerbose(new string('>', currentDepth) + IshSession.FolderPathSeparator + folderName + IshSession.FolderPathSeparator);

            WriteDebug($"DeleteRecursive IshFolderRef[{ishFolder.IshFolderRef}] folderName[{folderName}] ({currentDepth}/{int.MaxValue})");
            string xmlIshFolders = IshSession.Folder25.GetSubFoldersByIshFolderRef(ishFolder.IshFolderRef);
            // GetSubFolders contains ishfolder for the parent folder + ishfolder inside for the subfolders
            IshFolders retrievedFolders = new IshFolders(xmlIshFolders, "ishfolder/ishfolder");
            if (retrievedFolders.Ids.Length > 0)
            {
                IshFields requestedMetadata = IshSession.IshTypeFieldSetup.ToIshRequestedMetadataFields(IshSession.DefaultRequestedMetadata, ISHType, new IshFields(), Enumerations.ActionMode.Read);
                xmlIshFolders = IshSession.Folder25.RetrieveMetadataByIshFolderRefs(retrievedFolders.Ids, requestedMetadata.ToXml());
                retrievedFolders = new IshFolders(xmlIshFolders);
                // sort them
                ++currentDepth;
                IshFolder[] sortedFolders = retrievedFolders.SortedFolders;
                WriteParentProgress("Recursive folder remove...", _parentCurrent, _parentTotal + sortedFolders.Count());
                foreach (IshFolder retrievedIshFolder in sortedFolders)
                {
                    DeleteRecursive(retrievedIshFolder, currentDepth);
                }
            }
            WriteParentProgress("Recursive folder remove...", ++_parentCurrent, _parentTotal);
            if (ShouldProcess(Convert.ToString(ishFolder.IshFolderRef)))
            {
                IshSession.Folder25.Delete(ishFolder.IshFolderRef);
            }

        }
    }
}
