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

namespace Trisoft.ISHRemote.Cmdlets.Folder
{
    /// <summary>
    /// <para type="synopsis">The Move-IshFolder cmdlet moves folders that are passed through the pipeline or determined via provided parameters to a different folder.</para>
    /// <para type="description">The Move-IshFolder cmdlet moves folders that are passed through the pipeline or determined via provided parameters to a different folder.</para>
    /// </summary>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/ISHWS/" -PSCredential "Admin"
    /// Move-IshFolder -IshFolders (Get-IshFolder -FolderPath "General\__ISHRemote" -Recurse -Depth 2) -ToFolderId (Add-IshFolder...)
    /// </code>
    /// <para>New-IshSession will submit into SessionState, so it can be reused by this cmdlet. Moves all folders listed under "General\__ISHRemote" to some other folder.</para>
    /// </example>
    [Cmdlet(VerbsCommon.Move, "IshFolder", SupportsShouldProcess = true)]
    [OutputType(typeof(IshFolder))]
    public sealed class MoveIshFolder : FolderCmdlet
    {

        /// <summary>
        /// <para type="description">The IshSession variable holds the authentication and contract information. This object can be initialized using the New-IshSession cmdlet.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshFoldersGroup")]
        [ValidateNotNullOrEmpty]
        public IshSession IshSession { get; set; }

        /// <summary>
        /// <para type="description">The folder identifier where the folder is currently located</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup"), ValidateNotNullOrEmpty]
        public long FolderId { get; set; }

        /// <summary>
        /// <para type="description">The folder identifier where to move the folder to</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshFoldersGroup")]
        [ValidateNotNullOrEmpty]
        public long ToFolderId { get; set; }

        /// <summary>
        /// <para type="description">Array with the folders to move. This array can be passed through the pipeline or explicitly passed via the parameter.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "IshFoldersGroup")]
        [AllowEmptyCollection]
        public IshFolder[] IshFolders { get; set; }

        protected override void BeginProcessing()
        {
            if (IshSession == null) { IshSession = (IshSession)SessionState.PSVariable.GetValue(ISHRemoteSessionStateIshSession); }
            if (IshSession == null) { throw new ArgumentNullException(ISHRemoteSessionStateIshSessionException); }
            WriteDebug($"Using IshSession[{IshSession.Name}] from SessionState.{ISHRemoteSessionStateIshSession}");
            base.BeginProcessing();
        }

        /// <summary>
        /// Process the Move-IshFolder commandlet.
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
                long folderId = FolderId;
                long toFolderId = ToFolderId;

                // Folder Ids to retrieve added folder(s)
                List<long> foldersToRetrieve = new List<long>();
                List<IshFolder> returnedFolders = new List<IshFolder>();

                if (IshFolders != null && IshFolders.Length == 0)
                {
                    // Do nothing
                    WriteVerbose("IshFolders is empty, so nothing to move");
                    WriteVerbose("IshFolders is empty, so nothing to retrieve");
                }
                else
                {
                    if (IshFolders != null)
                    {
                        WriteDebug("Moving");

                        // 2b. Move using IshFolder[] pipeline. 
                        IshFolder[] ishFolders = IshFolders;

                        int current = 0;
                        foreach (IshFolder ishFolder in ishFolders)
                        {
                            // read "folderRef" from the ishFolder object
                            folderId = ishFolder.IshFolderRef;
                            WriteDebug($"folderId[{folderId}] ToFolderId[{toFolderId}] {++current}/{ishFolders.Length}");
                            if (ShouldProcess(Convert.ToString(folderId)))
                            {
                                IshSession.Folder25.Move(folderId, toFolderId);
                            foldersToRetrieve.Add(folderId);
                            }
                        }
                    }
                    else
                    {

                        // 2a. Set using provided parameters (not piped IshFolder)
                        WriteDebug($"folderId[{folderId}] ToFolderId[{toFolderId}]");
                        if (ShouldProcess(Convert.ToString(folderId)))
                        {
                            IshSession.Folder25.Move(folderId, toFolderId);
                            foldersToRetrieve.Add(folderId);
                        }
                    }

                    // 3a. Retrieve moved folder(s) from the database and write it out
                    WriteDebug("Retrieving");

                    // Add the required fields (needed for pipe operations)
                    IshFields requestedMetadata = IshSession.IshTypeFieldSetup.ToIshRequestedMetadataFields(IshSession.DefaultRequestedMetadata, ISHType, new IshFields(), Enumerations.ActionMode.Read);
                    string xmlIshFolders = IshSession.Folder25.RetrieveMetadataByIshFolderRefs(foldersToRetrieve.ToArray(), requestedMetadata.ToXml());

                    IshFolders retrievedFolders = new IshFolders(xmlIshFolders);
                    returnedFolders.AddRange(retrievedFolders.Folders);
                }

                WriteVerbose("returned object count[" + returnedFolders.Count + "]");
                WriteObject(returnedFolders,true);               
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
