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
                                    case Enumerations.Protocol.OpenApiWithOpenIdConnect:
                                        ICollection<OpenApiISH30.SetFieldValue> fieldValues = new List<OpenApiISH30.SetFieldValue>();

                                        //FNAME
                                        var fieldFolderName = new OpenApiISH30.SetStringFieldValue()
                                        {
                                            IshField = new OpenApiISH30.IshField() { Level = OpenApiISH30.Level.None, Name = "FNAME" },
                                            Value = folderName
                                        };
                                        fieldValues.Add(fieldFolderName);

                                        //FDOCUMENTTYPE (aka Folder Type)
                                        var fieldFolderType = new OpenApiISH30.SetLovFieldValue()
                                        {
                                            IshField = new OpenApiISH30.IshField() { Level = OpenApiISH30.Level.None, Name = "FDOCUMENTTYPE" },
                                            Value = new OpenApiISH30.SetLovValue() { Id = "VDOCTYPEILLUSTRATION" }  // TODO [Question] expects very raw DDOCTYPE lov value like VDOCTYPEILLUSTRATION, instead of still ugly but current API25 ISHIllustration
                                        };
                                        fieldValues.Add(fieldFolderType);

                                        //READ-ACCESS
                                        var fieldReadAccess = new OpenApiISH30.SetMultiCardFieldValue()
                                        {
                                            IshField = new OpenApiISH30.IshField() { Level = OpenApiISH30.Level.None, Name = "READ-ACCESS" },
                                            Value = new List<OpenApiISH30.SetBaseObject>()
                                        };
                                        // Convert multi-value fields of ISHRemote models via IshSession.Separator
                                        foreach (string readaccessItem in readAccessString.Split(IshSession.Separator.ToCharArray()))
                                        {
                                            fieldReadAccess.Value.Add(new OpenApiISH30.SetUserGroup() { Id = readaccessItem });
                                        }
                                        fieldValues.Add(fieldReadAccess);

                                        //FUSERGROUP (aka Owned by)
                                        var fieldWriteAccess = new OpenApiISH30.SetMultiCardFieldValue()
                                        {
                                            IshField = new OpenApiISH30.IshField() { Level = OpenApiISH30.Level.None, Name = "FUSERGROUP" },
                                            Value = new List<OpenApiISH30.SetBaseObject>()
                                        };
                                        // Convert multi-value fields of ISHRemote models via IshSession.Separator
                                        foreach (string writeaccessItem in ownedBy.Split(IshSession.Separator.ToCharArray()))
                                        {
                                            fieldReadAccess.Value.Add(new OpenApiISH30.SetUserGroup() { Id = writeaccessItem });
                                        }
                                        fieldValues.Add(fieldWriteAccess);

                                        var response = IshSession.OpenApiISH30Service.CreateFolderAsync(new OpenApiISH30.CreateFolder()
                                        {
                                            ParentId = ParentFolderId.ToString(),  // TODO [Question] Why is folder id a string and not typed as long in the CreateFolder model? BaseObject is string, so exceptional folder long is string.
                                            Fields = fieldValues
                                        }).GetAwaiter().GetResult();
                                        folderId = Convert.ToInt64(response.Id);  // TODO [Question] Why is folder id a string and not typed as long in the FolderDescriptor model?

                                        // Ivo's pitch, but worried about ValueType, so testing a manual collection first.
                                        //IshSession.OpenApi30Service.CreateFolderAsync(new OpenApiISH30.CreateFolder()
                                        //{
                                        //    ParentId = ParentFolderId.ToString(),  // TODO [Question] Why is folder id a string and not typed as long in the CreateFolder model? BaseObject is string, so exceptional folder long is string.
                                        //    //Fields = setFieldValueCollection
                                        //    Fields = ishFolder.IshFields.ToOpenApiISH30SetFieldValues(IshSession)
                                        //});

                                        foldersToRetrieve.Add(folderId);
                                        break;
                                    case Enumerations.Protocol.WcfSoapWithWsTrust:
                                        folderId = IshSession.Folder25.Create(
                                            ParentFolderId,
                                            folderName,
                                            ownedBy,
                                            readAccess,
                                            EnumConverter.ToFolderType<Folder25ServiceReference.IshFolderType>(folderType));
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
                                case Enumerations.Protocol.OpenApiWithOpenIdConnect:
                                    /*
                                    ICollection<OpenApiISH30.SetFieldValue> fieldValues = new List<OpenApiISH30.SetFieldValue>();

                                    //FNAME
                                    var fieldFolderName = new OpenApiISH30.SetStringFieldValue()
                                    {
                                        IshField = new OpenApiISH30.IshField() { Level = OpenApiISH30.Level.None, Name = "FNAME" },
                                        Value = FolderName
                                    };
                                    fieldValues.Add(fieldFolderName);

                                    //FDOCUMENTTYPE (aka Folder Type)
                                    var fieldFolderType = new OpenApiISH30.SetLovFieldValue()
                                    {
                                        IshField = new OpenApiISH30.IshField() { Level = OpenApiISH30.Level.None, Name = "FDOCUMENTTYPE" },
                                        Value = new OpenApiISH30.SetLovValue() { Id = Enumerations.ToDDOCTYPEValue(FolderType) }  // TODO [Question] expects very raw DDOCTYPE lov value like VDOCTYPEILLUSTRATION, instead of still ugly but current API25 ISHIllustration
                                    };
                                    fieldValues.Add(fieldFolderType);

                                    //READ-ACCESS
                                    var fieldReadAccess = new OpenApiISH30.SetMultiLovFieldValue()
                                    {
                                        IshField = new OpenApiISH30.IshField() { Level = OpenApiISH30.Level.None, Name = "READ-ACCESS" },
                                        Value = new List<OpenApiISH30.SetLovValue>()
                                    };
                                    // Convert multi-value fields of ISHRemote models via IshSession.Separator
                                    foreach (string readaccessItem in readAccess)
                                    {
                                        fieldReadAccess.Value.Add(new OpenApiISH30.SetLovValue() { Id = readaccessItem });
                                    }
                                    fieldValues.Add(fieldReadAccess);

                                    //FUSERGROUP (aka Owned by)
                                    var ownedByValue = String.IsNullOrEmpty(ownedBy) ? null : new OpenApiISH30.SetUserGroup() { Id = ownedBy };
                                    var fieldWriteAccess = new OpenApiISH30.SetCardFieldValue()
                                    {
                                        IshField = new OpenApiISH30.IshField() { Level = OpenApiISH30.Level.None, Name = "FUSERGROUP" },
                                        Value = ownedByValue
                                    };
                                    fieldValues.Add(fieldWriteAccess);

                                    var response = IshSession.OpenApiISH30Service.CreateFolderAsync(new OpenApiISH30.CreateFolder()
                                    {
                                        ParentId = ParentFolderId.ToString(),  // TODO [Question] Why is folder id a string and not typed as long in the CreateFolder model? BaseObject is string, so exceptional folder long is string.
                                        Fields = fieldValues
                                    }).GetAwaiter().GetResult();
                                    folderId = Convert.ToInt64(response.Id);  // TODO [Question] Why is folder id a string and not typed as long in the FolderDescriptor model?
                                    foldersToRetrieve.Add(folderId);
                                    break;
                                    */
                                case Enumerations.Protocol.WcfSoapWithWsTrust:
                                    folderId = IshSession.Folder25.Create(
                                        ParentFolderId,
                                        FolderName,
                                        ownedBy,
                                        readAccess,
                                        EnumConverter.ToFolderType<Folder25ServiceReference.IshFolderType>(FolderType));
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
                        case Enumerations.Protocol.OpenApiWithOpenIdConnect:
                            /*
                            ICollection<OpenApiISH30.RequestedField> fieldValues = new List<OpenApiISH30.RequestedField>();
                            fieldValues.Add(new OpenApiISH30.RequestedField()
                            {
                                IshField = new OpenApiISH30.IshField()
                                {
                                    Name = "FITTLE",
                                    Level = OpenApiISH30.Level.Logical
                                }
                            });
                            //ICollection<OpenApiISH30.FilterFieldValue> filterFieldValues = new List<OpenApiISH30.FilterFieldValue>();
                            //fieldValues.Add(new OpenApiISH30.CardFilterFieldValue()  // Looks a lot like SetFieldVAlue, so strongly typed
                            //{
                            //    IshField = new OpenApiISH30.IshField()
                            //    {
                            //        Name = "FITTLE",
                            //        Level = OpenApiISH30.Level.Logical,
                            //        Type = nameof(OpenApiISH30.IshField)
                            //    },
                            //    Type = nameof(OpenApiISH30.RequestedField)
                            //});
                            var responseGet = IshSession.OpenApiISH30Service.GetFolderListAsync(new OpenApiISH30.GetFolderList()
                            {
                                Ids = foldersToRetrieve.ConvertAll<string>(x => x.ToString()),
                                FieldGroup = OpenApiISH30EnumerationsExtensions.ToOpenApiISH30FieldGroup(IshSession.DefaultRequestedMetadata),
                                
                            }).GetAwaiter().GetResult();
                            returnedFolders.AddRange(new IshFolders(responseGet, IshSession.Separator).Folders);
                            break;
                            */
                        case Enumerations.Protocol.WcfSoapWithWsTrust:
                            string xmlIshFolders = IshSession.Folder25.RetrieveMetadataByIshFolderRefs(foldersToRetrieve.ToArray(), requestedMetadata.ToXml());
                            IshFolders retrievedFolders = new IshFolders(xmlIshFolders);
                            returnedFolders.AddRange(retrievedFolders.Folders);
                            break;
                    }
                }

                // 3b. Write it
                WriteVerbose("returned object count[" + returnedFolders.Count + "]");
                WriteObject(IshSession, ISHType, returnedFolders.ConvertAll(x => (IshBaseObject)x), true);
            }
            catch (TrisoftAutomationException trisoftAutomationException)
            {
                ThrowTerminatingError(new ErrorRecord(trisoftAutomationException, base.GetType().Name, ErrorCategory.InvalidOperation, null));
            }
            catch (OpenApiISH30.ApiException<OpenApiISH30.InfoShareProblemDetails> exception)
            {
                if (exception.Result != null)
                {
                    WriteWarning($"Status[{exception.Result.Status}] Title[{exception.Result.Title}] EventName[{exception.Result.EventName}] Detail[{exception.Result.Detail}]");
                    foreach (var error in exception.Result.Errors)
                    {
                        string warning = $"ErrorEventName[{error.EventName}] ErrorDetail[{error.Detail}]";
                        foreach (var relatedInfo in error.RelatedInfo)
                        {
                            var schemaValidationError = relatedInfo as OpenApiISH30.SchemaValidationError;
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
