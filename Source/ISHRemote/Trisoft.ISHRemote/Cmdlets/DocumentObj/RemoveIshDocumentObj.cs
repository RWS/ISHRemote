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
using System.Collections.Specialized;
using System.Linq;
using System.Management.Automation;
using Trisoft.ISHRemote.DocumentObj25ServiceReference;
using Trisoft.ISHRemote.Objects;
using Trisoft.ISHRemote.Objects.Public;
using Trisoft.ISHRemote.Exceptions;

namespace Trisoft.ISHRemote.Cmdlets.DocumentObj
{
    /// <summary>
    /// <para type="synopsis">The Remove-IshDocumentObj cmdlet removes the document objects that are passed through the pipeline or determined via provided parameters This commandlet allows to remove all types of objects (Illustrations, Maps, etc. ), except for publication (outputs). 
    /// For publication (outputs) you need to use Remove-IshPublicationOutput.</para>
    /// <para type="description">The Remove-IshDocumentObj cmdlet removes the document objects that are passed through the pipeline or determined via provided parameters This commandlet allows to remove all types of objects (Illustrations, Maps, etc. ), except for publication (outputs). 
    /// For publication (outputs) you need to use Remove-IshPublicationOutput.</para>
    /// </summary>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/InfoShareWS/" -PSCredential Admin
    /// Get-IshFolder -FolderPath $folderCmdletRootPath -Recurse | 
    /// Get-IshFolderContent |
    /// Remove-IshDocumentObj -Force
    /// </code>
    /// <para>New-IshSession will submit into SessionState, so it can be reused by this cmdlet. Removes all content objects from the listed folder and its subfolders.</para>
    /// </example>
    [Cmdlet(VerbsCommon.Remove, "IshDocumentObj", SupportsShouldProcess = true)]
    public sealed class RemoveIshDocumentObj : DocumentObjCmdlet
    {

        /// <summary>
        /// <para type="description">The IshSession variable holds the authentication and contract information. This object can be initialized using the New-IshSession cmdlet.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshObjectsGroup")]
        [ValidateNotNullOrEmpty]
        public IshSession IshSession { get; set; }

        /// <summary>
        /// <para type="description">The logical identifier of the document object</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup"), ValidateNotNullOrEmpty]
        public string LogicalId { get; set; }

        /// <summary>
        /// <para type="description">The version of the document object</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup"), ValidateNotNullOrEmpty]
        public string Version { get; set; }

        /// <summary>
        /// <para type="description">The language of the document object</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup"), ValidateNotNullOrEmpty]
        public string Lng { get; set; }

        /// <summary>
        /// <para type="description">The resolution of the document object</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup"), ValidateNotNullOrEmpty]
        public string Resolution { get; set; }

        /// <summary>
        /// <para type="description">The required current metadata of the document object. This parameter can be used to avoid that users override changes done by other users. The cmdlet will check whether the fields provided in this parameter still have the same values in the repository:</para>
        /// <para type="description">If the metadata is still the same, the document object is removed.</para>
        /// <para type="description">If the metadata is not the same anymore, an error is given and the document object is not removed.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshObjectsGroup")]
        [ValidateNotNullOrEmpty]
        public IshField[] RequiredCurrentMetadata { get; set; }

        /// <summary>
        /// <para type="description">Array with the objects to remove. This array can be passed through the pipeline or explicitly passed via the parameter.</para>
        /// </summary>
        /// TODO [Should] Promote parameter IshObject to IshObject[] processing
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "IshObjectsGroup")]
        public IshObject IshObject { get; set; }

        /// <summary>
        /// <para type="description">When the Force switch is set, after deleting the language object, the version object and logical object will be deleted if they don't have any language objects anymore</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshObjectsGroup")]
        [ValidateNotNullOrEmpty]
        public SwitchParameter Force { get; set; }

        protected override void BeginProcessing()
        {
            if (IshSession == null) { IshSession = (IshSession)SessionState.PSVariable.GetValue(ISHRemoteSessionStateIshSession); }
            if (IshSession == null) { throw new ArgumentException(ISHRemoteSessionStateIshSessionException); }
            WriteDebug($"Using IshSession[{IshSession.Name}] from SessionState.{ISHRemoteSessionStateIshSession}");
            base.BeginProcessing();
        }

        /// <summary>
        /// Process the Remove-IshDocumentObj commandlet.
        /// </summary>
        /// <exception cref="TrisoftAutomationException"></exception>
        /// <exception cref="Exception"></exception>
        protected override void ProcessRecord()
        {

            try
            {
                // 1. Validating the input
                WriteDebug("Validating");

                var logicalIdsVersionsCollection = new NameValueCollection();
                WriteDebug("Removing");

                IshFields requiredCurrentMetadata = new IshFields(RequiredCurrentMetadata);               

                if (IshObject != null)
                {                    
                    long lngRef = Convert.ToInt64(IshObject.ObjectRef[Enumerations.ReferenceType.Lng]);
                    WriteDebug($"Removing lngCardId[{lngRef}]");
                    if (ShouldProcess(Convert.ToString(lngRef)))
                    {
                        var response = IshSession.DocumentObj25.DeleteByIshLngRef(new DeleteByIshLngRefRequest()
                        {
                            psAuthContext = IshSession.AuthenticationContext,
                            plLngRef = lngRef,
                            psXMLRequiredCurrentMetadata = requiredCurrentMetadata.ToXml()
                        });
                        IshSession.AuthenticationContext = response.psAuthContext;
                    }
                    logicalIdsVersionsCollection.Add(IshObject.IshRef, IshObject.IshFields.GetFieldValue("VERSION", Enumerations.Level.Version, Enumerations.ValueType.Value));
                }
                else
                {
                    string resolution = Resolution ?? "";
                    string lng = Lng ?? "";
                    if (lng.Length > 0)
                    {
                        // only delete when you have a lanuage, lower the version delete (so empty lng) is handled
                        WriteDebug($"Removing LogicalId[{LogicalId}] Version[{Version}] Lng[{lng}] Resolution[{resolution}]");
                        if (ShouldProcess(LogicalId + "=" + Version + "=" + lng + "=" + resolution))
                        {
                            var response = IshSession.DocumentObj25.Delete(new DeleteRequest()
                            {
                                psAuthContext = IshSession.AuthenticationContext,
                                psLogicalId = LogicalId,
                                psVersion = Version,
                                psLanguage = lng,
                                psResolution = resolution,
                                psXMLRequiredCurrentMetadata = requiredCurrentMetadata.ToXml()
                            });
                            IshSession.AuthenticationContext = response.psAuthContext;
                        }
                    }
                    logicalIdsVersionsCollection.Add(LogicalId, Version);
                }

                if (Force.IsPresent && logicalIdsVersionsCollection.Count > 0)
                {
                    var response = IshSession.DocumentObj25.RetrieveMetadata(new RetrieveMetadataRequest()
                    {
                        psAuthContext = IshSession.AuthenticationContext,
                        pasLogicalIds = logicalIdsVersionsCollection.AllKeys.ToArray(),
                        peStatusFilter = eISHStatusgroup.ISHNoStatusFilter,
                        psXMLMetadataFilter = "",
                        psXMLRequestedMetadata = "<ishfields><ishfield name='VERSION' level='version'/><ishfield name='DOC-LANGUAGE' level='lng'/></ishfields>"
                    });
                    IshSession.AuthenticationContext = response.psAuthContext;
                    string xmlIshObjects = response.psOutXMLObjList;
                    List<IshObject> retrievedObjects = new List<IshObject>();
                    retrievedObjects.AddRange(new IshObjects(xmlIshObjects).Objects);

                    // Delete versions which do not have any language card anymore
                    foreach (string logicalId in logicalIdsVersionsCollection.AllKeys)
                    {                            
                        var versions = logicalIdsVersionsCollection.GetValues(logicalId).Distinct();
                        foreach (var version in versions)
                        {
                            bool versionWithLanguagesFound = false;
                            foreach (var retrievedObject in retrievedObjects)
                            {
                                if (retrievedObject.IshRef == logicalId && retrievedObject.IshFields.GetFieldValue("VERSION", Enumerations.Level.Version, Enumerations.ValueType.Value) == version)
                                {
                                    versionWithLanguagesFound = true;
                                }
                            }
                            if (!versionWithLanguagesFound)
                            {
                                if (ShouldProcess(LogicalId + "=" + version + "=" + "="))
                                {
                                    var responseDelete = IshSession.DocumentObj25.Delete(new DeleteRequest()
                                    {
                                        psAuthContext = IshSession.AuthenticationContext,
                                        psLogicalId = logicalId,
                                        psVersion = version,
                                        psLanguage = "",
                                        psResolution = "",
                                        psXMLRequiredCurrentMetadata = ""
                                    });
                                    IshSession.AuthenticationContext = responseDelete.psAuthContext;
                                }
                            }
                        }
                    }

                    // Delete logical cards which do not have any languages anymore
                    foreach (string logicalId in logicalIdsVersionsCollection.AllKeys)
                    {
                        bool logicalIdFound = false;
                        foreach (var retrievedObject in retrievedObjects)
                        {
                            if (retrievedObject.IshRef == logicalId)
                            {
                                logicalIdFound = true;
                            }
                        }
                        if (!logicalIdFound)
                        {
                            if (ShouldProcess(LogicalId + "=" + "=" + "="))
                            {
                                var responseDelete = IshSession.DocumentObj25.Delete(new DeleteRequest()
                                {
                                    psAuthContext = IshSession.AuthenticationContext,
                                    psLogicalId = logicalId,
                                    psVersion = "",
                                    psLanguage = "",
                                    psResolution = "",
                                    psXMLRequiredCurrentMetadata = ""
                                });
                                IshSession.AuthenticationContext = responseDelete.psAuthContext;
                            }
                        }
                    }
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
    }
}
