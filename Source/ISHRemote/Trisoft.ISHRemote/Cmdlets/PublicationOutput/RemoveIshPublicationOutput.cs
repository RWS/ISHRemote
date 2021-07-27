/*
* Copyright © 2014 All Rights Reserved by the RWS Group for and on behalf of its affiliates and subsidiaries.
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
using Trisoft.ISHRemote.Objects;
using Trisoft.ISHRemote.Objects.Public;
using Trisoft.ISHRemote.Exceptions;
using Trisoft.ISHRemote.PublicationOutput25ServiceReference;

namespace Trisoft.ISHRemote.Cmdlets.PublicationOutput
{
    /// <summary>
    /// <para type="synopsis">The Remove-IshPublicationOutput cmdlet removes the publication outputs that are passed through the pipeline or determined via provided parameters</para>
    /// <para type="description">The Remove-IshPublicationOutput cmdlet removes the publication outputs that are passed through the pipeline or determined via provided parameters</para>
    /// </summary>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/InfoShareWS/" -IshUserName "username" -IshUserPassword  "userpassword"
    /// # Remove publication output
    /// Remove-IshPublicationOutput `
    /// -LogicalId "GUID-F66C1BB5-076D-455C-B055-DAC5D61AB3D9" `
    /// -Version "1" `
    /// -OutputFormat "PDF (A4 Manual)" `
    /// -LanguageCombination "en"
    /// </code>
    /// <para>New-IshSession will submit into SessionState, so it can be reused by this cmdlet. Removing a publication output</para>
    /// </example>
    [Cmdlet(VerbsCommon.Remove, "IshPublicationOutput", SupportsShouldProcess = true)]
    public sealed class RemoveIshPublicationOutput : PublicationOutputCmdlet
    {

        /// <summary>
        /// <para type="description">The IshSession variable holds the authentication and contract information. This object can be initialized using the New-IshSession cmdlet.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshObjectsGroup")]
        [ValidateNotNullOrEmpty]
        public IshSession IshSession { get; set; }

        /// <summary>
        /// <para type="description">Array with the publication outputs to remove. This array can be passed through the pipeline or explicitly passed via the parameter.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "IshObjectsGroup")]
        [AllowEmptyCollection]
        public IshObject[] IshObject { get; set; }

        /// <summary>
        /// <para type="description">The LogicalId of the publication output</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup"), ValidateNotNullOrEmpty]
        public string LogicalId { get; set; }

        /// <summary>
        /// <para type="description">The version of the publication output</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup"), ValidateNotNullOrEmpty]
        public string Version { get; set; }

        /// <summary>
        /// <para type="description">The output format of the publication output.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup"), ValidateNotNullOrEmpty]
        public string OutputFormat { get; set; }

        /// <summary>
        /// <para type="description">The language combination of the publication output.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup"), ValidateNotNullOrEmpty]
        public string LanguageCombination { get; set; }

        /// <summary>
        /// <para type="description">The required current metadata of the publication output. This parameter can be used to avoid that users override changes done by other users. The cmdlet will check whether the fields provided in this parameter still have the same values in the repository:</para>
        /// <para type="description">If the metadata is still the same, the publication output is removed.</para>
        /// <para type="description">If the metadata is not the same anymore, an error is given and the publication output is not removed.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshObjectsGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]
        [ValidateNotNullOrEmpty]
        public IshField[] RequiredCurrentMetadata { get; set; }

        /// <summary>
        /// <para type="description">When the Force switch is set, after deleting the publication output object, the publication version object and publication logical object will be deleted if they don't have any publication outputs anymore.
        /// Be carefull using this option, as it will delete the publication (version) and baseline if there are no outputformats left!!</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshObjectsGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]        
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
        /// Process the Remove-IshPublicationOutput commandlet.
        /// </summary>
        /// <exception cref="TrisoftAutomationException"></exception>
        /// <exception cref="Exception"></exception>
        /// <remarks>Writes an <see cref="Objects.Public.IshObject"/> array to the pipeline.</remarks>
        protected override void ProcessRecord()
        {
            try
            {               
                var logicalIdsVersionsCollection = new NameValueCollection();

                if (IshObject != null && IshObject.Length == 0)
                {
                    // Do nothing
                    WriteVerbose("IshObject is empty, so nothing to remove");
                }
                else
                {
                    WriteDebug("Removing");

                    IshFields requiredCurrentMetadata = new IshFields(RequiredCurrentMetadata);

                    if (IshObject != null)
                    {
                        // Using the pipeline
                        int current = 0;
                        IshObjects ishObjects = new IshObjects(IshObject);
                        foreach (IshObject ishObject in ishObjects.Objects)
                        {
                            long lngRef = Convert.ToInt64(ishObject.ObjectRef[Enumerations.ReferenceType.Lng]);
                            WriteDebug($"lngRef[{lngRef}] {++current}/{ishObjects.Objects.Length}");
                            if (ShouldProcess(Convert.ToString(lngRef)))
                            {
                                IshSession.PublicationOutput25.DeleteByIshLngRef(lngRef, requiredCurrentMetadata.ToXml());
                            }
                            logicalIdsVersionsCollection.Add(ishObject.IshRef,
                                ishObject.IshFields.GetFieldValue("VERSION", Enumerations.Level.Version,
                                    Enumerations.ValueType.Value));
                        }
                    }
                    else
                    {
                        if (ShouldProcess(LogicalId + "=" + Version + "=" + LanguageCombination + "=" + OutputFormat))
                        {
                            IshSession.PublicationOutput25.Delete(
                                LogicalId,
                                Version,
                                OutputFormat,
                                LanguageCombination,
                                requiredCurrentMetadata.ToXml());
                        }
                        logicalIdsVersionsCollection.Add(LogicalId, Version);
                    }
                }

                if (Force.IsPresent && logicalIdsVersionsCollection.Count > 0)
                { 
                    var xmlIshObjects = IshSession.PublicationOutput25.RetrieveMetadata(logicalIdsVersionsCollection.AllKeys.ToArray(),
                        StatusFilter.ISHNoStatusFilter, "", "<ishfields><ishfield name='VERSION' level='version'/><ishfield name='FISHPUBLNGCOMBINATION' level='lng'/></ishfields>");
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
                                if (ShouldProcess(logicalId + "=" + version + "=" + "="))
                                {
                                    IshSession.PublicationOutput25.Delete(logicalId, version, "", "", "");
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
                            if (ShouldProcess(logicalId + "==="))
                            {
                                IshSession.PublicationOutput25.Delete(logicalId, "", "", "", "");
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
