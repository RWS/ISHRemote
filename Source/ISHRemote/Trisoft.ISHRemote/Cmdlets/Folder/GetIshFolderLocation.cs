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
using System.ServiceModel;
using System.Xml;
using Trisoft.ISHRemote.Exceptions;
using Trisoft.ISHRemote.HelperClasses;
using Trisoft.ISHRemote.Objects;
using Trisoft.ISHRemote.Objects.Public;

namespace Trisoft.ISHRemote.Cmdlets.Folder
{
    /// <summary>
    /// <para type="synopsis">The Get-IshFolderLocation cmdlet returns the repository location of the folder objects in the form of a separated string e.g. "General\Folder1\Folder2"</para>
    /// <para type="description">The Get-IshFolderLocation cmdlet returns the repository location of the folder objects in the form of a separated string e.g. "General\Folder1\Folder2"</para>
    /// </summary>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/ISHWS/" -PSCredential "Admin"
    /// $folderId = 7775 #provide valid folder identifier
    /// $folderPath = Get-IshFolderLocation -FolderId $folderId
    /// </code>
    /// <para>New-IshSession will submit into SessionState, so it can be reused by this cmdlet. Retrieve folder location using provided Id</para>
    /// </example>
    [Cmdlet(VerbsCommon.Get, "IshFolderLocation", SupportsShouldProcess = false)]
    [OutputType(typeof(string))]
    public sealed class GetIshFolderLocation : FolderCmdlet
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
        /// <para type="description">Identifier of the folder for which to retrieve the location</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "FolderIdGroup"), ValidateNotNullOrEmpty]
        public long FolderId { get; set; }

        /// <summary>
        /// <para type="description">The eBaseFolder enumeration to get subfolder(s) for the specified root folder</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "BaseFolderGroup"), ValidateNotNullOrEmpty]
        public Enumerations.BaseFolder BaseFolder { get; set; }

        /// <summary>
        /// <para type="description">Folder that is passed through the pipeline or explicitly passed via the parameter.</para>
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
        /// Process the Get-IshFolderLocation commandlet.
        /// </summary>
        /// <exception cref="TrisoftAutomationException"></exception>
        /// <exception cref="Exception"></exception>
        /// <remarks>Writes FolderPath string to the pipeline.</remarks>
        protected override void ProcessRecord()
        {
            try
            {
                // 1. Validating the input
                WriteDebug("Validating");
                List<long> returnFolderIds = new List<long>();

                if (ParameterSetName == "IshFolderGroup")
                {
                    // 1a. Retrieve using IshFolder object
                    foreach (IshFolder ishFolder in IshFolder)
                    {
                        returnFolderIds.Add(ishFolder.IshFolderRef);
                    }
                }

                if (ParameterSetName == "FolderIdGroup")
                {
                    // 1b. Retrieve using FolderId
                    WriteDebug($"folderId[{FolderId}]");
                    returnFolderIds.Add(FolderId);
                }

                if (ParameterSetName == "FolderPathGroup")
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

                if (ParameterSetName == "BaseFolderGroup")
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

                WriteDebug("Retrieving FolderLocation");
                int current = 0;
                foreach (long returnFolderId in returnFolderIds)
                {
                    WriteDebug($"folderId[{returnFolderId}] {++current}/{returnFolderIds.Count}");
                    var response = IshSession.Folder25.FolderLocation(new Folder25ServiceReference.FolderLocationRequest(returnFolderId));
                    // Using basefolder name and folderPaths array make a full folder path
                    var folderPaths = new List<string>() { BaseFolderEnumToLabel(IshSession, response.baseFolder) };
                    folderPaths.AddRange(response.folderPath);
                    WriteObject(IshSession.FolderPathSeparator + string.Join(IshSession.FolderPathSeparator, folderPaths), true);
                }

                WriteVerbose("returned folderlocation count["+returnFolderIds.Count+"]");
            }
            catch (TrisoftAutomationException trisoftAutomationException)
            {
                ThrowTerminatingError(new ErrorRecord(trisoftAutomationException, base.GetType().Name, ErrorCategory.InvalidOperation, null));
            }
            catch (AggregateException aggregateException)
            {
                var flattenedAggregateException = aggregateException.Flatten();
                WriteWarning(flattenedAggregateException.ToString());
                ThrowTerminatingError(new ErrorRecord(flattenedAggregateException, base.GetType().Name, ErrorCategory.NotSpecified, null));
            }
            catch (TimeoutException timeoutException)
            {
                WriteVerbose("TimeoutException Message[" + timeoutException.Message + "] StackTrace[" + timeoutException.StackTrace + "]");
                ThrowTerminatingError(new ErrorRecord(timeoutException, base.GetType().Name, ErrorCategory.OperationTimeout, null));
            }
            catch (CommunicationException communicationException)
            {
                WriteVerbose("CommunicationException Message[" + communicationException.Message + "] StackTrace[" + communicationException.StackTrace + "]");
                ThrowTerminatingError(new ErrorRecord(communicationException, base.GetType().Name, ErrorCategory.OperationStopped, null));
            }
            catch (Exception exception)
            {
                ThrowTerminatingError(new ErrorRecord(exception, base.GetType().Name, ErrorCategory.NotSpecified, null));
            }
        }
    }
}
