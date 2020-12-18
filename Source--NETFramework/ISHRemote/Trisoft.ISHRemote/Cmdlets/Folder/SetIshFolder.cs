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
using System.Management.Automation;
using Trisoft.ISHRemote.Exceptions;
using Trisoft.ISHRemote.HelperClasses;
using Trisoft.ISHRemote.Objects;
using Trisoft.ISHRemote.Objects.Public;

namespace Trisoft.ISHRemote.Cmdlets.Folder
{
    /// <summary>
    /// <para type="synopsis">The Set-IshFolder cmdlet updates the folders that are passed through the pipeline or determined via provided parameters</para>
    /// <para type="description">The Set-IshFolder cmdlet updates the folders that are passed through the pipeline or determined via provided parameters</para>
    /// </summary>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/ISHWS/" -PSCredential "Admin"
    /// $folderId = 655993 # provide real folder Id
    /// $newFolderName = "Updated Folder Name"
    /// $updatedFolder = Set-IshFolder -FolderId $folderId -NewFolderName $newFolderName
    /// </code>
    /// <para>New-IshSession will submit into SessionState, so it can be reused by this cmdlet. Update name of a folder with provided FolderId</para>
    /// </example>
    [Cmdlet(VerbsCommon.Set, "IshFolder", SupportsShouldProcess = true)]
    [OutputType(typeof(IshFolder))]
    public sealed class SetIshFolder : FolderCmdlet
    {
        /// <summary>
        /// <para type="description">The IshSession variable holds the authentication and contract information. This object can be initialized using the New-IshSession cmdlet.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]
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
        /// <para type="description">The eBaseFolder enumeration to get subfolder(s) for the specified root folder</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "BaseFolderGroup"), ValidateNotNullOrEmpty]
        public Enumerations.BaseFolder BaseFolder { get; set; }

        /// <summary>
        /// <para type="description">Identifier of the folder to update</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "ParameterGroup"), ValidateNotNullOrEmpty]
        public long FolderId { get; set; }

        /// <summary>
        /// <para type="description">New name of the folder</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "FolderPathGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "BaseFolderGroup")]
        [ValidateNotNullOrEmpty]
        public string NewFolderName
        {
            get { return _newFolderName; }
            set { _newFolderName = value; }
        }

        /// <summary>
        /// <para type="description">String array with the user groups that have read access to this folder. When the string array is empty, all users have read access to the folder</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "FolderPathGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "BaseFolderGroup")]
        public string[] ReadAccess { get; set; }

        /// <summary>
        /// <para type="description">Folder for which to update the metadata. This folder can be passed through the pipeline or explicitly passed via the parameter.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "IshFolderGroup")]
        public IshFolder[] IshFolder { get; set; }


        #region Private fields
        /// <summary>
        /// Default value of the new folder name parameter, the API will skip a rename to empty ("")
        /// </summary>
        private string _newFolderName = "";
        #endregion

        protected override void BeginProcessing()
        {
            if (IshSession == null) { IshSession = (IshSession)SessionState.PSVariable.GetValue(ISHRemoteSessionStateIshSession); }
            if (IshSession == null) { throw new ArgumentException(ISHRemoteSessionStateIshSessionException); }
            WriteDebug($"Using IshSession[{IshSession.Name}] from SessionState.{ISHRemoteSessionStateIshSession}");
            base.BeginProcessing();
        }

        /// <summary>
        /// Process the Set-IshFolder commandlet.
        /// </summary>
        /// <exception cref="TrisoftAutomationException"></exception>
        /// <exception cref="Exception"></exception>
        /// <remarks>Writes an <see cref="IshFolder"/> array to the pipeline</remarks>
        protected override void ProcessRecord()
        {
            try
            {
                // 1. Validating the input
                WriteDebug("Validating");
                string xmlIshFolders = "";
                long folderId = -5;
                string[] readAccess;
                string newFolderName;
                List<long> returnFolderIds = new List<long>();
                if (IshFolder != null)
                {
                    // 1a. read all info from the IshFolder/IshFields object
                    IshFolder[] ishFolders = IshFolder;
                    int current = 0;
                    foreach (IshFolder ishFolder in ishFolders)
                    {
                        // read "folderRef" from the ishFolder object
                        folderId = ishFolder.IshFolderRef;
                        WriteDebug($"folderId[{folderId}] {++current}/{ishFolders.Length}");
                        string readAccessString = ishFolder.IshFields.GetFieldValue("READ-ACCESS", Enumerations.Level.None, Enumerations.ValueType.Value);
                        readAccess = readAccessString.Split(new[] { IshSession.Separator }, StringSplitOptions.RemoveEmptyEntries);
                        newFolderName = ishFolder.IshFields.GetFieldValue("FNAME", Enumerations.Level.None, Enumerations.ValueType.Value);
                        if (readAccess == null)
                        {
                            WriteDebug($"Renaming folderId[{folderId}]");
                            if (ShouldProcess(Convert.ToString(folderId)))
                            {
                                IshSession.Folder25.Rename(folderId, newFolderName);
                            }
                        }
                        else
                        {
                            WriteDebug($"Updating folderId[{folderId}]");
                            if (ShouldProcess(Convert.ToString(folderId)))
                            {
                                IshSession.Folder25.Update(folderId, newFolderName, readAccess);
                            }
                        }
                        returnFolderIds.Add(folderId);
                    }
                }
                else if (FolderId != 0)
                {
                    // 1b. Retrieve using FolderId
                    WriteDebug($"folderId[{FolderId}]");
                    folderId = FolderId;
                    readAccess = ReadAccess;
                    newFolderName = _newFolderName;
                    if (newFolderName == null && readAccess == null)
                    {
                        WriteDebug($"Renaming folderId[{folderId}]");
                        if (ShouldProcess(Convert.ToString(folderId)))
                        {
                            IshSession.Folder25.Rename(folderId, newFolderName);
                        }
                    }
                    else
                    {
                        WriteDebug($"Updating folderId[{folderId}]");
                        if (ShouldProcess(Convert.ToString(folderId)))
                        {
                            IshSession.Folder25.Update(folderId, newFolderName, readAccess);
                        }
                    }
                    returnFolderIds.Add(folderId);
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
                    folderId = ishFolder.Folders[0].IshFolderRef;
                    readAccess = ReadAccess;
                    newFolderName = _newFolderName;
                    if (newFolderName == "")
                    {
                        WriteWarning("Skipping rename to empty for folderId[" + folderId + "]");
                    }
                    else if (readAccess == null)
                    {
                        WriteDebug($"Renaming folderId[{folderId}]");
                        if (ShouldProcess(Convert.ToString(folderId)))
                        {
                            IshSession.Folder25.Rename(folderId, newFolderName);
                        }
                    }
                    else
                    {
                        WriteDebug($"Updating folderId[{folderId}]");
                        if (ShouldProcess(Convert.ToString(folderId)))
                        {
                            IshSession.Folder25.Update(folderId, newFolderName, readAccess);
                        }
                    }
                    returnFolderIds.Add(folderId);
                }
                else
                {
                    // 1d. Retrieve subfolder(s) from the specified root folder using BaseFolder string (enumeration)
                    var baseFolder = EnumConverter.ToBaseFolder<Folder25ServiceReference.BaseFolder>(BaseFolder);
                    xmlIshFolders = IshSession.Folder25.GetMetadata(
                        baseFolder,
                        new string[0],
                        "");
                    IshFolders retrievedFolders = new IshFolders(xmlIshFolders, "ishfolder");
                    folderId = retrievedFolders.Folders[0].IshFolderRef;
                    readAccess = ReadAccess;
                    newFolderName = _newFolderName;
                    if (newFolderName == "")
                    {
                        WriteWarning("Skipping rename to empty for folderId[" + folderId + "]");
                    }
                    if (readAccess == null)
                    {
                        WriteDebug($"Renaming folderId[{folderId}]");
                        if (ShouldProcess(Convert.ToString(folderId)))
                        {
                            IshSession.Folder25.Rename(folderId, newFolderName);
                        }
                    }
                    else
                    {
                        WriteDebug($"Updating folderId[{folderId}]");
                        if (ShouldProcess(Convert.ToString(folderId)))
                        {
                            IshSession.Folder25.Update(folderId, newFolderName, readAccess);
                        }
                    }
                    returnFolderIds.Add(folderId);
                }


                // Retrieve updated folder from the database and write it out
                WriteDebug("Retrieving");
                
                // Add the required fields (needed for pipe operations)
                IshFields requestedMetadata = IshSession.IshTypeFieldSetup.ToIshRequestedMetadataFields(IshSession.DefaultRequestedMetadata, ISHType, new IshFields(), Enumerations.ActionMode.Read);
                xmlIshFolders = IshSession.Folder25.RetrieveMetadataByIshFolderRefs(returnFolderIds.ToArray(), requestedMetadata.ToXml());

                // 3b. Write it
                var returnIshObjects = new IshFolders(xmlIshFolders).FolderList;
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
