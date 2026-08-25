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
using System.Xml;
using Trisoft.ISHRemote.Exceptions;
using Trisoft.ISHRemote.HelperClasses;
using Trisoft.ISHRemote.Objects;
using Trisoft.ISHRemote.Objects.Public;

namespace Trisoft.ISHRemote.Cmdlets.PublicationOutput
{
    /// <summary>
    /// <para type="synopsis">The Get-IshPublicationOutputContent cmdlet returns the IshDocumentObj objects that are directly reachable through the saved baseline of the incoming IshPublicationOutput objects.</para>
    /// <para type="description">The Get-IshPublicationOutputContent cmdlet expands the saved baseline of each incoming IshPublicationOutput and returns the IshDocumentObj objects (topics, maps, illustrations, resources) that are directly reachable — meaning only content objects for which the baseline holds a pinned version are returned. Content objects whose version is not selected in the baseline (gaps) are not returned even if they would be reachable via a full autocomplete pass.
    /// The cmdlet silently fetches all publication output metadata it needs (FISHBASELINE, FISHMASTERREF, FISHRESOURCES, FISHPUBLNGCOMBINATION, FISHOUTPUTFORMATREF and the output format's FISHRESOLUTIONS) — callers do not need to pre-fetch specific fields.
    /// Typical follow-on pipeline operations are Get-IshDocumentObjData, Set-IshDocumentObj or status-transition scripts.</para>
    /// </summary>
    /// <example>
    /// <code>
    /// Get-IshPublicationOutput -LogicalId "GUID-12345678-ABCD-EFGH-IJKL-1234567890AB" |
    /// Get-IshPublicationOutputContent
    /// </code>
    /// <para>Returns all directly reachable IshDocumentObj objects from the saved baseline of the given publication output.</para>
    /// </example>
    /// <example>
    /// <code>
    /// Get-IshPublicationOutput -LogicalId "GUID-12345678-ABCD-EFGH-IJKL-1234567890AB" |
    /// Get-IshPublicationOutputContent |
    /// Set-IshDocumentObj -Metadata (Set-IshMetadataField -Name "FSTATUS" -Level Lng -Value "Released")
    /// </code>
    /// <para>Releases all content objects that are directly reachable through the saved baseline of the given publication output.</para>
    /// </example>
    [Cmdlet(VerbsCommon.Get, "IshPublicationOutputContent", SupportsShouldProcess = false)]
    [OutputType(typeof(IshDocumentObj))]
    public sealed class GetIshPublicationOutputContent : PublicationOutputCmdlet
    {
        /// <summary>
        /// <para type="description">The IshSession variable holds the authentication and contract information. This object can be initialized using the New-IshSession cmdlet.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshObjectGroup")]
        [ValidateNotNullOrEmpty]
        public IshSession IshSession { get; set; }

        /// <summary>
        /// <para type="description">Array with the publication outputs for which to retrieve the directly reachable content objects. This array can be passed through the pipeline or explicitly passed via the parameter.</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipeline = true, ParameterSetName = "IshObjectGroup")]
        [AllowEmptyCollection]
        public IshObject[] IshObject { get; set; }

        /// <summary>
        /// <para type="description">The requested metadata fields to retrieve on the returned IshDocumentObj objects. Defaults to IshSession.DefaultRequestedMetadata.</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "IshObjectGroup")]
        [ValidateNotNull]
        public IshField[] RequestedMetadata { get; set; }

        // Accumulate incoming publication outputs across ProcessRecord calls so we can batch
        // the metadata re-fetch and the ExpandBaseline calls in EndProcessing.
        private readonly List<IshObject> _incomingIshObjects = new List<IshObject>();

        protected override void BeginProcessing()
        {
            if (IshSession == null) { IshSession = (IshSession)SessionState.PSVariable.GetValue(ISHRemoteSessionStateIshSession); }
            if (IshSession == null) { IshSession = (IshSession)SessionState.PSVariable.GetValue(ISHRemoteSessionStateGlobalIshSession); }
            if (IshSession == null) { throw new ArgumentException(ISHRemoteSessionStateIshSessionException); }
            WriteDebug($"Using IshSession[{IshSession.Name}] from SessionState.{ISHRemoteSessionStateIshSession} or in turn SessionState.{ISHRemoteSessionStateGlobalIshSession}");
            base.BeginProcessing();
        }

        protected override void ProcessRecord()
        {
            if (IshObject != null && IshObject.Length == 0)
            {
                WriteVerbose("IshObject is empty, so nothing to retrieve");
                return;
            }
            if (IshObject != null)
            {
                _incomingIshObjects.AddRange(IshObject);
            }
        }

        protected override void EndProcessing()
        {
            try
            {
                if (_incomingIshObjects.Count == 0)
                {
                    WriteVerbose("IshObject is empty, so nothing to retrieve");
                    return;
                }

                // --- Step 1: silently re-fetch all publication output metadata we need ---
                // We need (on the publication output):
                //   Version level : FISHBASELINE, FISHMASTERREF, FISHRESOURCES
                //   Lng level     : FISHPUBLNGCOMBINATION, FISHOUTPUTFORMATREF
                // lngref is always structurally present on every IshPublicationOutput so we use it
                // to batch-fetch all the fields we need regardless of what the caller had requested.
                WriteDebug("PublicationOutput metadata retrieval");
                var lngRefs = _incomingIshObjects
                    .Select(o => Convert.ToInt64(o.ObjectRef[Enumerations.ReferenceType.Lng]))
                    .ToArray();

                // Build a minimal requested metadata set covering only what ExpandBaseline needs.
                var requiredPubFields = new IshFields();
                requiredPubFields.AddField(new IshRequestedMetadataField("VERSION", Enumerations.Level.Version, Enumerations.ValueType.Value));
                requiredPubFields.AddField(new IshRequestedMetadataField("FISHBASELINE", Enumerations.Level.Version, Enumerations.ValueType.Element));
                requiredPubFields.AddField(new IshRequestedMetadataField("FISHMASTERREF", Enumerations.Level.Version, Enumerations.ValueType.Element));
                requiredPubFields.AddField(new IshRequestedMetadataField("FISHRESOURCES", Enumerations.Level.Version, Enumerations.ValueType.Element));
                requiredPubFields.AddField(new IshRequestedMetadataField("FISHPUBLNGCOMBINATION", Enumerations.Level.Lng, Enumerations.ValueType.Value));
                requiredPubFields.AddField(new IshRequestedMetadataField("FISHOUTPUTFORMATREF", Enumerations.Level.Lng, Enumerations.ValueType.Element));

                string xmlPubObjects = IshSession.PublicationOutput25.RetrieveMetadataByIshLngRefs(
                    lngRefs,
                    requiredPubFields.ToXml());
                var refreshedPubObjects = new IshObjects(ISHType, xmlPubObjects).Objects;

                WriteDebug($"PublicationOutput metadata retrieval count[{refreshedPubObjects.Length}]");

                // --- Step 2: for each publication output call ExpandBaseline ---
                var allLngRefs = new List<long>();
                int current = 0;

                foreach (var pubObject in refreshedPubObjects)
                {
                    string version = ((IshMetadataField)pubObject.IshFields.RetrieveFirst("VERSION", Enumerations.Level.Version, Enumerations.ValueType.Value)?.ToMetadataField()).Value;
                    string pubLngCombination = ((IshMetadataField)pubObject.IshFields.RetrieveFirst("FISHPUBLNGCOMBINATION", Enumerations.Level.Lng, Enumerations.ValueType.Value)?.ToMetadataField()).Value;
                    string pubOutputFormatRef = ((IshMetadataField)pubObject.IshFields.RetrieveFirst("FISHOUTPUTFORMATREF", Enumerations.Level.Lng, Enumerations.ValueType.Element)?.ToMetadataField()) .Value;
                    string pubObjectHumanId = $"={pubObject.IshRef}={version}={pubLngCombination}={pubOutputFormatRef}";
                    WriteDebug($"Processing[{pubObjectHumanId}] {++current}/{refreshedPubObjects.Length}");

                    // Extract FISHBASELINE (baseline GUID)
                    // RetrieveFirst prefers id over element over value; ToMetadataField() / cast gives .Value
                    var baselineField = pubObject.IshFields
                        .RetrieveFirst("FISHBASELINE", Enumerations.Level.Version, Enumerations.ValueType.Element)
                        ?.ToMetadataField() as IshMetadataField;
                    string baselineId = baselineField?.Value;
                    if (string.IsNullOrEmpty(baselineId))
                    {
                        WriteWarning($"Processing[{pubObjectHumanId}] has no FISHBASELINE field value — skipping.");
                        continue;
                    }

                    // Extract FISHMASTERREF (root map logical ID) — startLogicalIds[]
                    var masterRefField = pubObject.IshFields
                        .RetrieveFirst("FISHMASTERREF", Enumerations.Level.Version, Enumerations.ValueType.Element)
                        ?.ToMetadataField() as IshMetadataField;
                    string masterRef = masterRefField?.Value;
                    var startLogicalIds = string.IsNullOrEmpty(masterRef)
                        ? Array.Empty<string>()
                        : new[] { masterRef };

                    // Extract FISHRESOURCES (multi-value) — startResourceLogicalIds[]
                    var resourcesField = pubObject.IshFields
                        .RetrieveFirst("FISHRESOURCES", Enumerations.Level.Version, Enumerations.ValueType.Element)
                        ?.ToMetadataField() as IshMetadataField;
                    string resourcesRaw = resourcesField?.Value ?? string.Empty;
                    string[] startResourceLogicalIds = string.IsNullOrEmpty(resourcesRaw)
                        ? Array.Empty<string>()
                        : resourcesRaw.Split(new[] { IshSession.Separator }, StringSplitOptions.RemoveEmptyEntries);

                    // Extract FISHPUBLNGCOMBINATION — languages[], illustrationLanguages[], resourceLanguages[]
                    var langComboField = pubObject.IshFields
                        .RetrieveFirst("FISHPUBLNGCOMBINATION", Enumerations.Level.Lng, Enumerations.ValueType.Value)
                        ?.ToMetadataField() as IshMetadataField;
                    string langComboRaw = langComboField?.Value ?? string.Empty;
                    string[] languages = string.IsNullOrEmpty(langComboRaw)
                        ? Array.Empty<string>()
                        : langComboRaw.Split(new[] { IshSession.Separator }, StringSplitOptions.RemoveEmptyEntries);

                    // Extract FISHOUTPUTFORMATREF (element name of the output format card)
                    // then fetch FISHRESOLUTIONS from that output format card — resolutions[]
                    var outputFormatRefField = pubObject.IshFields
                        .RetrieveFirst("FISHOUTPUTFORMATREF", Enumerations.Level.Lng, Enumerations.ValueType.Element)
                        ?.ToMetadataField() as IshMetadataField;
                    string outputFormatRef = outputFormatRefField?.Value;
                    string[] resolutions = Array.Empty<string>();
                    if (!string.IsNullOrEmpty(outputFormatRef))
                    {
                        var outputFormatRequestedFields = new IshFields();
                        outputFormatRequestedFields.AddField(new IshRequestedMetadataField("FISHRESOLUTIONS", Enumerations.Level.None, Enumerations.ValueType.Element));
                        var outputFormatResponse = IshSession.OutputFormat25.GetMetadata(
                            new OutputFormat25ServiceReference.GetMetadataRequest(
                                outputFormatRef,
                                outputFormatRequestedFields.ToXml()));
                        Enumerations.ISHType[] outputFormatISHType = { Enumerations.ISHType.ISHOutputFormat };
                        var outputFormatObjects = new IshObjects(outputFormatISHType, outputFormatResponse.xmlObjectList);
                        if (outputFormatObjects.Objects.Length > 0)
                        {
                            var resolutionsField = outputFormatObjects.Objects[0].IshFields
                                .RetrieveFirst("FISHRESOLUTIONS", Enumerations.Level.None, Enumerations.ValueType.Element)
                                ?.ToMetadataField() as IshMetadataField;
                            string resolutionsRaw = resolutionsField?.Value ?? string.Empty;
                            resolutions = string.IsNullOrEmpty(resolutionsRaw)
                                ? Array.Empty<string>()
                                : resolutionsRaw.Split(new[] { IshSession.Separator }, StringSplitOptions.RemoveEmptyEntries);
                        }
                        WriteDebug($"Processing[{pubObjectHumanId}] OutputFormatRef[{outputFormatRef}] Resolutions[{string.Join(",", resolutions)}]");
                    }

                    WriteDebug($"Processing[{pubObjectHumanId}] BaselineId[{baselineId}] MasterRef[{masterRef}] Languages[{string.Join(",", languages)}] Resolutions[{string.Join(",", resolutions)}] Resources[{string.Join(",", startResourceLogicalIds)}]");

                    // --- Step 3: call ExpandBaseline ---
                    string xmlBaselineReport = IshSession.Baseline25.ExpandBaseline(
                        baselineId,
                        startLogicalIds,
                        startResourceLogicalIds,
                        languages,         // languages[]
                        languages,         // illustrationLanguages[] — same combination per publication output convention
                        languages,         // resourceLanguages[]     — same combination per publication output convention
                        resolutions);

                    WriteDebug($"Processing[{pubObjectHumanId}] BaselineReport.length[{xmlBaselineReport?.Length ?? 0}]");

                    // --- Step 4: collect lngrefs from reportitems with reportresult="OK" ---
                    if (!string.IsNullOrEmpty(xmlBaselineReport))
                    {
                        var reportDoc = new XmlDocument();
                        reportDoc.LoadXml(xmlBaselineReport);
                        // Each <reportitem lngref="NNN" reportresult="OK"/> contributes a language card ID
                        foreach (XmlElement reportItem in reportDoc.SelectNodes("//reportitem[@lngref]"))
                        {
                            string lngRefStr = reportItem.GetAttribute("lngref");
                            if (long.TryParse(lngRefStr, out long lngRef))
                            {
                                allLngRefs.Add(lngRef);
                            }
                        }
                    }
                }

                WriteDebug($"Total lngRefs collected[{allLngRefs.Count}]");

                if (allLngRefs.Count == 0)
                {
                    WriteVerbose("returned object count[0]");
                    return;
                }

                // --- Step 5: deduplicate (same content object may appear for multiple pub outputs) ---
                var distinctLngRefs = allLngRefs.Distinct().ToList();
                WriteDebug($"Distinct lngRefs[{distinctLngRefs.Count}]");

                // --- Step 6: batch-hydrate into IshDocumentObj via RetrieveMetadataByIshLngRefs ---
                Enumerations.ISHType[] docObjISHType = {
                    Enumerations.ISHType.ISHModule,
                    Enumerations.ISHType.ISHMasterDoc,
                    Enumerations.ISHType.ISHLibrary,
                    Enumerations.ISHType.ISHTemplate,
                    Enumerations.ISHType.ISHIllustration
                };
                IshFields requestedMetadata = IshSession.IshTypeFieldSetup.ToIshRequestedMetadataFields(
                    IshSession.DefaultRequestedMetadata,
                    docObjISHType,
                    new IshFields(RequestedMetadata),
                    Enumerations.ActionMode.Read);

                var returnedObjects = new List<IshObject>();
                var batches = DivideListInBatches<long>(distinctLngRefs, IshSession.MetadataBatchSize);
                int batchCurrent = 0;
                foreach (var batch in batches)
                {
                    batchCurrent += batch.Count;
                    WriteDebug($"Retrieving DocumentObj batch lngRefs.length[{batch.Count}] {batchCurrent}/{distinctLngRefs.Count}");
                    string xmlIshObjects = IshSession.DocumentObj25.RetrieveMetadataByIshLngRefs(
                        batch.ToArray(),
                        requestedMetadata.ToXml());
                    var batchObjects = new IshObjects(docObjISHType, xmlIshObjects);
                    returnedObjects.AddRange(batchObjects.Objects);
                }

                WriteVerbose("returned object count[" + returnedObjects.Count + "]");
                WriteObject(IshSession, docObjISHType, returnedObjects.ConvertAll(x => (IshBaseObject)x), true);
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
            finally
            {
                base.EndProcessing();
            }
        }
    }
}
