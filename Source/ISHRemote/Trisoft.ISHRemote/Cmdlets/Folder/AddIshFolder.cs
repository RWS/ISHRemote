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

namespace Trisoft.ISHRemote.Cmdlets.Folder
{
    /// <summary>
    /// <para type="synopsis">The Add-IshFolder cmdlet adds the new folders that are passed through the pipeline or determined via provided parameters. Query and Reference folders are not supported.</para>
    /// <para type="description">The Add-IshFolder cmdlet adds the new folders that are passed through the pipeline or determined via provided parameters. Query and Reference folders are not supported.</para>
    /// </summary>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/InfoShareWS/" -IshUserName "username" -IshUserPassword  "userpassword"
    /// $folderName = "New folder created by powershell"
    /// $parentFolderId = "7775" # provide a valid parent folder Id
    /// $ishFolders = Add-IshFolder `
    ///         -ParentFolderId $parentFolderId `
    ///         -FolderType "ISHModule" `
    ///         -FolderName $folderName `
    ///         -ReadAccess @("") `
    ///         -OwnedBy ""
    /// </code>
    /// <para>New-IshSession will submit into SessionState, so it can be reused by this cmdlet. Add a folder using input parameters</para>
    /// </example>
    [Cmdlet(VerbsCommon.Add, "IshFolder", SupportsShouldProcess = true)]
    [OutputType(typeof(IshFolder))]
    public sealed class AddIshFolder : FolderCmdlet
    {

        /// <summary>
        /// <para type="description">The IshSession variable holds the authentication and contract information. This object can be initialized using the New-IshSession cmdlet.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]
        // TODO: [Could] FolderPath means creating all intermediate folders with the same security settings, same type as the detected last parent
        //  [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "FolderPathGroup")]
        // Creating base folders is not allowed, so no 
        //  [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "BaseFolderGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshFoldersGroup")]
        [ValidateNotNullOrEmpty]
        public IshSession IshSession { get; set; }

        /// <summary>
        /// <para type="description">The identifier of the parent folder where the new folder will be created</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshFoldersGroup")]
        [ValidateNotNullOrEmpty]
        public long ParentFolderId { get; set; }

        /// <summary>
        /// <para type="description">The Type of the new Folder</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup"), ValidateNotNullOrEmpty]
        public Enumerations.IshFolderType FolderType { get; set; }

        /// <summary>
        /// <para type="description">The Name of the new Folder</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup"), ValidateNotNullOrEmpty]
        public string FolderName { get; set; }

        /// <summary>
        /// <para type="description">The name of the UserGroup that will be the owner of the new folder</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup"), ValidateNotNull]
        public string OwnedBy { get; set; }

        /// <summary>
        /// <para type="description">Array with the UserGroups that have ReadAccess to the new folder</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup"), ValidateNotNull]
        public string[] ReadAccess { get; set; }

        /// <summary>
        /// <para type="description">The IshFolder array that needs to be created. Pipeline</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "IshFoldersGroup")]
        [AllowEmptyCollection]
        public IshFolder[] IshFolder { get; set; }

        protected override void BeginProcessing()
        {
            if (IshSession == null) { IshSession = (IshSession)SessionState.PSVariable.GetValue(ISHRemoteSessionStateIshSession); }
            if (IshSession == null) { throw new ArgumentException(ISHRemoteSessionStateIshSessionException); }
            WriteDebug($"Using IshSession[{IshSession.Name}] from SessionState.{ISHRemoteSessionStateIshSession}");
            base.BeginProcessing();
        }

        /// <summary>
        /// Process the Add-IshFolder commandlet.
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

                if (FolderType == Enumerations.IshFolderType.ISHQuery ||
                    FolderType == Enumerations.IshFolderType.ISHReference)
                {
                    throw new NotSupportedException("Query and reference folders are not supported");
                }

                List<IshFolder> returnedFolders = new List<IshFolder>();

                if (IshFolder != null && IshFolder.Length == 0)
                {
                    // Do nothing
                    WriteVerbose("IshFolders is empty, so nothing to add");
                    WriteVerbose("IshFolders is empty, so nothing to retrieve");
                }
                else
                {
                    WriteDebug("Adding");

                    List<long> foldersToRetrieve = new List<long>();
                    IshFields returnFields;

                    // 2a. Add using provided parameters (not piped IshFolder)
                    if (IshFolder != null)
                    {
                        // 2b. Add using IshFolder[] pipeline
                        int current = 0;
                        foreach (IshFolder ishFolder in IshFolder)
                        {
                            // read all info from the ishFolder/ishFields object
                            string folderName = ishFolder.IshFields.GetFieldValue("FNAME", Enumerations.Level.None, Enumerations.ValueType.Value);
                            var folderType = ishFolder.IshFolderType;
                            string readAccessString = ishFolder.IshFields.GetFieldValue("READ-ACCESS", Enumerations.Level.None, Enumerations.ValueType.Value);
                            string[] readAccess = readAccessString.Split(new string[] { IshSession.Separator }, StringSplitOptions.None);
                            string ownedBy = ishFolder.IshFields.GetFieldValue("FUSERGROUP", Enumerations.Level.None, Enumerations.ValueType.Value);

                            WriteDebug($"Adding ParentFolderId[{ParentFolderId}] FolderType[{folderType}] FolderName[{folderName}] {++current}/{IshFolder.Length}");
                            if (ShouldProcess(folderName))
                            {
                                long folderId = 0;
                                switch (IshSession.Protocol)
                                {
                                    case Enumerations.Protocol.OpenApiBasicAuthentication:
                                        ICollection<OpenApi.SetFieldValue> fieldValues = new List<OpenApi.SetFieldValue>();

                                        //FNAME
                                        var fieldFolderName = new OpenApi.SetStringFieldValue()
                                        {
                                            IshField = new OpenApi.IshField() { Level = OpenApi.Level.None, Name = "FNAME", Type = nameof(OpenApi.IshField) },
                                            Value = folderName
                                        };
                                        fieldValues.Add(fieldFolderName);

                                        //FDOCUMENTTYPE (aka Folder Type)
                                        var fieldFolderType = new OpenApi.SetLovFieldValue()
                                        {
                                            IshField = new OpenApi.IshField() { Level = OpenApi.Level.None, Name = "FDOCUMENTTYPE", Type = nameof(OpenApi.IshField) },
                                            Value = new OpenApi.SetLovValue() { Id = "VDOCTYPEILLUSTRATION" }  // TODO [Question] expects very raw DDOCTYPE lov value like VDOCTYPEILLUSTRATION, instead of still ugly but current API25 ISHIllustration
                                        };
                                        fieldValues.Add(fieldFolderType);

                                        //READ-ACCESS
                                        var fieldReadAccess = new OpenApi.SetMultiCardFieldValue()
                                        {
                                            IshField = new OpenApi.IshField() { Level = OpenApi.Level.None, Name = "READ-ACCESS", Type = nameof(OpenApi.IshField) },
                                            Value = new List<OpenApi.SetBaseObject>()
                                        };
                                        // Convert multi-value fields of ISHRemote models via IshSession.Separator
                                        foreach (string readaccessItem in readAccessString.Split(IshSession.Separator.ToCharArray()))
                                        {
                                            fieldReadAccess.Value.Add(new OpenApi.SetUserGroup() { Id = readaccessItem });
                                        }
                                        fieldValues.Add(fieldReadAccess);

                                        //FUSERGROUP (aka Owned by)
                                        var fieldWriteAccess = new OpenApi.SetMultiCardFieldValue()
                                        {
                                            IshField = new OpenApi.IshField() { Level = OpenApi.Level.None, Name = "FUSERGROUP", Type = nameof(OpenApi.IshField) },
                                            Value = new List<OpenApi.SetBaseObject>()
                                        };
                                        // Convert multi-value fields of ISHRemote models via IshSession.Separator
                                        foreach (string writeaccessItem in ownedBy.Split(IshSession.Separator.ToCharArray()))
                                        {
                                            fieldReadAccess.Value.Add(new OpenApi.SetUserGroup() { Id = writeaccessItem });
                                        }
                                        fieldValues.Add(fieldWriteAccess);

                                        var response = IshSession.OpenApi30Service.CreateFolderAsync(new OpenApi.CreateFolder()
                                        {
                                            ParentId = ParentFolderId.ToString(),  // TODO [Question] Why is folder id a string and not typed as long in the CreateFolder model? BaseObject is string, so exceptional folder long is string.
                                            Fields = fieldValues
                                        }).GetAwaiter().GetResult();
                                        folderId = Convert.ToInt64(response.Id);  // TODO [Question] Why is folder id a string and not typed as long in the FolderDescriptor model?

                                        // Ivo's pitch, but worried about ValueType, so testing a manual collection first.
                                        //IshSession.OpenApi30Service.CreateFolderAsync(new OpenApi.CreateFolder()
                                        //{
                                        //    ParentId = ParentFolderId.ToString(),  // TODO [Question] Why is folder id a string and not typed as long in the CreateFolder model? BaseObject is string, so exceptional folder long is string.
                                        //    //Fields = setFieldValueCollection
                                        //    Fields = ishFolder.IshFields.ToSetFieldValues(IshSession)
                                        //});

                                        foldersToRetrieve.Add(folderId);
                                        break;
                                    case Enumerations.Protocol.AsmxAuthenticationContext:
                                        var responseCreate = IshSession.Folder25.Create(new Folder25ServiceReference.CreateRequest()
                                        {
                                            psAuthContext = IshSession.AuthenticationContext,
                                            plParentFolderRef = ParentFolderId,
                                            psFolderName = folderName,
                                            psOwnedBy = ownedBy,
                                            pasReadAccess = readAccess,
                                            peISHFolderType = EnumConverter.ToFolderType<Folder25ServiceReference.eISHFolderType>(folderType),
                                            plOutNewFolderRef = folderId
                                        });
                                        IshSession.AuthenticationContext = responseCreate.psAuthContext;
                                        folderId = responseCreate.plOutNewFolderRef;
                                        foldersToRetrieve.Add(folderId);
                                        break;
                                }
                            }
                        }
                        returnFields = (IshFolder[0] == null) ? new IshFields() : IshFolder[0].IshFields;
                    }
                    else
                    {
                        string ownedBy = OwnedBy ?? "";
                        string[] readAccess = ReadAccess ?? new string[] { };

                        WriteDebug($"Adding ParentFolderId[{ParentFolderId}] FolderType[{FolderType}] FolderName[{FolderName}]");
                        if (ShouldProcess(FolderName))
                        {
                            long folderId = 0;
                            switch (IshSession.Protocol)
                            {
                                case Enumerations.Protocol.OpenApiBasicAuthentication:
                                    ICollection<OpenApi.SetFieldValue> fieldValues = new List<OpenApi.SetFieldValue>();

                                    //FNAME
                                    var fieldFolderName = new OpenApi.SetStringFieldValue()
                                    {
                                        IshField = new OpenApi.IshField() { Level = OpenApi.Level.None, Name = "FNAME", Type = nameof(OpenApi.IshField) },
                                        Value = FolderName
                                    };
                                    fieldValues.Add(fieldFolderName);

                                    //FDOCUMENTTYPE (aka Folder Type)
                                    var fieldFolderType = new OpenApi.SetLovFieldValue()
                                    {
                                        IshField = new OpenApi.IshField() { Level = OpenApi.Level.None, Name = "FDOCUMENTTYPE", Type = nameof(OpenApi.IshField) },
                                        Value = new OpenApi.SetLovValue() { Id = Enumerations.ToDDOCTYPEValue(FolderType), Type = nameof(OpenApi.SetLovValue) }  // TODO [Question] expects very raw DDOCTYPE lov value like VDOCTYPEILLUSTRATION, instead of still ugly but current API25 ISHIllustration
                                    };
                                    fieldValues.Add(fieldFolderType);

                                    //READ-ACCESS
                                    var fieldReadAccess = new OpenApi.SetMultiLovFieldValue()
                                    {
                                        IshField = new OpenApi.IshField() { Level = OpenApi.Level.None, Name = "READ-ACCESS", Type = nameof(OpenApi.IshField) },
                                        Value = new List<OpenApi.SetLovValue>()
                                    };
                                    // Convert multi-value fields of ISHRemote models via IshSession.Separator
                                    foreach (string readaccessItem in readAccess)
                                    {
                                        fieldReadAccess.Value.Add(new OpenApi.SetLovValue() { Id = readaccessItem, Type = nameof(OpenApi.SetLovValue) });
                                    }
                                    fieldValues.Add(fieldReadAccess);

                                    //FUSERGROUP (aka Owned by)
                                    var ownedByValue = String.IsNullOrEmpty(ownedBy) ? null : new OpenApi.SetUserGroup() { Id = ownedBy };
                                    var fieldWriteAccess = new OpenApi.SetCardFieldValue()
                                    {
                                        IshField = new OpenApi.IshField() { Level = OpenApi.Level.None, Name = "FUSERGROUP", Type = nameof(OpenApi.IshField) },
                                        Value = ownedByValue
                                    };
                                    fieldValues.Add(fieldWriteAccess);

                                    var response = IshSession.OpenApi30Service.CreateFolderAsync(new OpenApi.CreateFolder()
                                    {
                                        ParentId = ParentFolderId.ToString(),  // TODO [Question] Why is folder id a string and not typed as long in the CreateFolder model? BaseObject is string, so exceptional folder long is string.
                                        Fields = fieldValues
                                    }).GetAwaiter().GetResult();
                                    folderId = Convert.ToInt64(response.Id);  // TODO [Question] Why is folder id a string and not typed as long in the FolderDescriptor model?
                                    foldersToRetrieve.Add(folderId);
                                    break;
                                case Enumerations.Protocol.AsmxAuthenticationContext:
                                    var responseCreate = IshSession.Folder25.Create(new Folder25ServiceReference.CreateRequest()
                                    {
                                        psAuthContext = IshSession.AuthenticationContext,
                                        plParentFolderRef = ParentFolderId,
                                        psFolderName = FolderName,
                                        psOwnedBy = ownedBy,
                                        pasReadAccess = readAccess,
                                        peISHFolderType = EnumConverter.ToFolderType<Folder25ServiceReference.eISHFolderType>(FolderType),
                                        plOutNewFolderRef = folderId
                                    });
                                    IshSession.AuthenticationContext = responseCreate.psAuthContext;
                                    folderId = responseCreate.plOutNewFolderRef;
                                    foldersToRetrieve.Add(folderId);
                                    break;
                            }
                        }
                        returnFields = new IshFields();
                    }

                    // 3a. Retrieve added folder from the database and write it out
                    WriteDebug("Retrieving");

                    // Add the required fields (needed for pipe operations)
                    IshFields requestedMetadata = IshSession.IshTypeFieldSetup.ToIshRequestedMetadataFields(IshSession.DefaultRequestedMetadata, ISHType, returnFields, Enumerations.ActionMode.Read);
                    switch (IshSession.Protocol)
                    {
                        case Enumerations.Protocol.OpenApiBasicAuthentication:
                        // TODO [Must] Add OpenApi implementation
                        case Enumerations.Protocol.AsmxAuthenticationContext:
                            var response = IshSession.Folder25.RetrieveMetadataByIshFolderRefs(new Folder25ServiceReference.RetrieveMetadataByIshFolderRefsRequest()
                            {
                                psAuthContext = IshSession.AuthenticationContext,
                                palFolderRefs = foldersToRetrieve.ToArray(),
                                psXMLRequestedMetaData = requestedMetadata.ToXml()
                            });
                            IshSession.AuthenticationContext = response.psAuthContext;
                            string xmlIshFolders = response.psOutXMLFolderList;
                            IshFolders retrievedFolders = new IshFolders(xmlIshFolders);
                            returnedFolders.AddRange(retrievedFolders.Folders);
                            break;
                    }
                }

                // 3b. Write it
                WriteVerbose("returned object count[" + returnedFolders.Count + "]");
                WriteObject(IshSession, ISHType, returnedFolders.ConvertAll(x => (IshBaseObject)x), true);
            }

            catch (NotSupportedException notSupportedException)
            {
                WriteError(new ErrorRecord(notSupportedException, "-1", ErrorCategory.NotImplemented, null));
            }
            catch (TrisoftAutomationException trisoftAutomationException)
            {
                ThrowTerminatingError(new ErrorRecord(trisoftAutomationException, base.GetType().Name, ErrorCategory.InvalidOperation, null));
            }
            catch (OpenApi.ApiException<OpenApi.InfoShareProblemDetails> exception)
            {
                if (exception.Result != null)
                {
                    WriteWarning($"Status[{exception.Result.Status}] Title[{exception.Result.Title}] EventName[{exception.Result.EventName}] Detail[{exception.Result.Detail}]");
                    foreach (var error in exception.Result.Errors)
                    {
                        string warning = $"ErrorEventName[{error.EventName}] ErrorDetail[{error.Detail}]";
                        foreach (var relatedInfo in error.RelatedInfo)
                        {
                            var schemaValidationError = relatedInfo as OpenApi.SchemaValidationError;
                            warning += $" on ErrorPath[{schemaValidationError.Path}]";
                        }
                        WriteWarning(warning);
                    }
                }
                ThrowTerminatingError(new ErrorRecord(exception, base.GetType().Name, ErrorCategory.InvalidOperation, null));
            }
            catch (AggregateException aggregateException)
            {
                var flattenedAggregateException = aggregateException.Flatten();
                WriteWarning(flattenedAggregateException.ToString());
                ThrowTerminatingError(new ErrorRecord(flattenedAggregateException, base.GetType().Name, ErrorCategory.NotSpecified, null));
            }
            catch (Exception exception)
            {
                if (exception.InnerException != null)
                {
                    WriteWarning(exception.InnerException.ToString());
                }
                ThrowTerminatingError(new ErrorRecord(exception, base.GetType().Name, ErrorCategory.NotSpecified, null));
            }
        }
    }
}
