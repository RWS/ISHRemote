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
using System.ServiceModel;
using System.Text;
using Trisoft.ISHRemote.Exceptions;
using Trisoft.ISHRemote.Objects;
using Trisoft.ISHRemote.Objects.Public;

namespace Trisoft.ISHRemote.Cmdlets.EDT
{
    /// <summary>
    /// <para type="synopsis">The Set-IshEDT cmdlet updates the EDTs that are passed through the pipeline or determined via provided parameters</para>
    /// <para type="description">The Set-IshEDT cmdlet updates the EDTs that are passed through the pipeline or determined via provided parameters</para>
    /// </summary>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/InfoShareWS/" -IshUserName "username" -IshUserPassword  "userpassword"
    /// $edtName = "MYEDT"
    /// $metadata = Set-IshMetadataField -IshSession $ishSession -Name "EDT-CANDIDATE" -Level "none" -Value "XML" |
    ///             Set-IshMetadataField -IshSession $ishSession -Name "EDT-FILE-EXTENSION" -Level "none" -Value "XML" |
    ///             Set-IshMetadataField -IshSession $ishSession -Name "EDT-MIME-TYPE" -Level "none" -Value "text/xml"
    /// $edtAdd = Add-IshEDT -IshSession $ishSession -Name $edtName -Metadata $metadata
    /// $metadataUpdate = Set-IshMetadataField -IshSession $ishSession -Name "NAME" -Level "none" -Value ($edtName + " updated")
    /// $requiredCurrentMetadata = Set-IshRequiredCurrentMetadataField -IshSession $ishSession -Name "EDT-FILE-EXTENSION" -Level "none" -Value "XML"
    /// Set-IshEDT -RequiredCurrentMetadata $requiredCurrentMetadata -Metadata $metadataUpdate
    /// </code>
    /// <para>New-IshSession will submit into SessionState, so it can be reused by this cmdlet. Add EDT and update name</para>
    /// </example>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/InfoShareWS/" -IshUserName "username" -IshUserPassword  "userpassword"
    /// Set-IshEDT -IshSession $ishSession -Id EDTXML -Metadata (Set-IshMetadataField -IshSession $ishSession -Name "EDT-CANDIDATE" -Level None -ValueType Value -Value "xml, dita, ditamap")
    /// </code>
    /// <para>Adding .map and .ditamap to the EDTXML object. By adding these, tools like Content-Importer/DITA2Trisoft can import .xml, .dita and .ditamap files and they will all be assigned EDTXML as Electronic Document Type.</para>
    /// </example>
    [Cmdlet(VerbsCommon.Set, "IshEDT", SupportsShouldProcess = true)]
    [OutputType(typeof(IshEDT))]
    public sealed class SetIshEdt : EDTCmdlet
    {

        /// <summary>
        /// <para type="description">The IshSession variable holds the authentication and contract information. This object can be initialized using the New-IshSession cmdlet.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshObjectsGroup")]
        [ValidateNotNullOrEmpty]
        public IshSession IshSession { get; set; }

        /// <summary>
        ///  <para type="description">EDT Element Name</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup"), ValidateNotNullOrEmpty]
        public string Id { get; set; }

        /// <summary>
        /// <para type="description">The metadata to set for the object</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]
        [ValidateNotNull]
        public IshField[] Metadata { get; set; }

        /// <summary>
        /// <para type="description">The required current metadata of the object. This parameter can be used to avoid that users override changes done by other users. The cmdlet will check whether the fields provided in this parameter still have the same values in the repository:</para>
        /// <para type="description">If the metadata is still the same, the metadata will be set.</para>
        /// <para type="description">If the metadata is not the same anymore, an error is given and the metadata will be set.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshObjectsGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "ParameterGroup")]
        [ValidateNotNullOrEmpty]
        public IshField[] RequiredCurrentMetadata { get; set; }

        /// <summary>
        /// <para type="description">Array with the objects for which to retrieve the metadata. This array can be passed through the pipeline or explicitly passed via the parameter.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "IshObjectsGroup")]
        [AllowEmptyCollection]
        public IshObject[] IshObject { get; set; }

        protected override void BeginProcessing()
        {
            if (IshSession == null) { IshSession = (IshSession)SessionState.PSVariable.GetValue(ISHRemoteSessionStateIshSession); }
            if (IshSession == null) { IshSession = (IshSession)SessionState.PSVariable.GetValue(ISHRemoteSessionStateGlobalIshSession); }
            if (IshSession == null) { throw new ArgumentException(ISHRemoteSessionStateIshSessionException); }
            WriteDebug($"Using IshSession[{IshSession.Name}] from SessionState.{ISHRemoteSessionStateIshSession} or in turn SessionState.{ISHRemoteSessionStateGlobalIshSession}");
            base.BeginProcessing();
        }

        /// <summary>
        /// Process the Set-IshEDT commandlet.
        /// </summary>
        /// <exception cref="TrisoftAutomationException"></exception>
        /// <exception cref="Exception"></exception>
        /// <remarks>Writes an <see cref="Objects.Public.IshObject"/> array to the pipeline.</remarks>
        protected override void ProcessRecord()
        {

            try
            {
                // 1. Validating the input
                WriteDebug("Validating");

                List<IshObject> returnedObjects = new List<IshObject>();

                if (IshObject != null && IshObject.Length == 0)
                {
                    // Do nothing
                    WriteVerbose("IshObject is empty, so nothing to update");
                }
                else
                {
                    IshFields requiredCurrentMetadata = new IshFields(RequiredCurrentMetadata);

                    // Updated EDT Ids
                    List<string> EDTIdsToRetrieve = new List<string>();
                    IshFields returnFields;

                    // 2. Doing Set
                    WriteDebug("Updating");

                    // 2a. Set using provided parameters (not piped IshObject)
                    if (IshObject != null)
                    {
                        // 2b. Set using IshObject pipeline. 
                        IshObjects ishObjects = new IshObjects(IshObject);
                        int current = 0;
                        foreach (IshObject ishObject in ishObjects.Objects)
                        {
                            WriteDebug($"Id[{ishObject.IshRef}] {++current}/{IshObject.Length}");
                            var metadata = IshSession.IshTypeFieldSetup.ToIshMetadataFields(ISHType, ishObject.IshFields, Enumerations.ActionMode.Update);
                            if (ShouldProcess(ishObject.IshRef))
                            {
                                IshSession.EDT25.Update(
                                    ishObject.IshRef,
                                    metadata.ToXml(),
                                    requiredCurrentMetadata.ToXml());
                                EDTIdsToRetrieve.Add(ishObject.IshRef);
                            }
                        }
                        returnFields = (IshObject[0] == null)
                            ? new IshFields()
                            : IshObject[0].IshFields;
                    }
                    else
                    {
                        var metadata = IshSession.IshTypeFieldSetup.ToIshMetadataFields(ISHType, new IshFields(Metadata), Enumerations.ActionMode.Update);
                        if (ShouldProcess(Id))
                        {
                            IshSession.EDT25.Update(
                                Id,
                                metadata.ToXml(),
                                requiredCurrentMetadata.ToXml());
                            EDTIdsToRetrieve.Add(Id);
                        }
                        returnFields = metadata;
                    }

                    // 3a. Retrieve updated EDT(s) from the database and write them out
                    WriteDebug("Retrieving");

                    // Add the required fields (needed for pipe operations)
                    IshFields requestedMetadata = IshSession.IshTypeFieldSetup.ToIshRequestedMetadataFields(IshSession.DefaultRequestedMetadata, ISHType, returnFields, Enumerations.ActionMode.Read);
                    string xmlIshObjects = IshSession.EDT25.RetrieveMetadata(
                       EDTIdsToRetrieve.ToArray(),
                       EDT25ServiceReference.ActivityFilter.None,
                       "",
                       requestedMetadata.ToXml());
                    
                    returnedObjects.AddRange(new IshObjects(ISHType, xmlIshObjects).Objects);
                }

                // 3b. Write it
                WriteVerbose("returned object count[" + returnedObjects.Count + "]");
                WriteObject(IshSession, ISHType, returnedObjects.ConvertAll(x => (IshBaseObject)x), true);
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
