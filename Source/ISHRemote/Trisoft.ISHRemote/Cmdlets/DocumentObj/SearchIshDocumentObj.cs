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
using System.Xml;

namespace Trisoft.ISHRemote.Cmdlets.DocumentObj
{
    /// <summary>
    /// <para type="synopsis">Search - so Full-Text-Index as data source, while Find is Relational Database as data source - matching the search criteria. Metadata is retrieved like Get-IshDocumentObj would from the Relational Database.</para>
    /// <para type="description">Passes the search criteria to the Search25 API where the result set is capped by -MaxHitsToReturn.</para>
    /// <para type="description">Then uses DocumentObj25 API to retrieve ishobjects.</para>
    /// </summary>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/InfoShareWS/" -PSCredential Admin
    /// $xmlQuery = @"
    /// <ishquery>
    ///   <and>
    ///     <ishfield name = 'ISHANYWHERE'  level='none' ishoperator='contains'>change oil filter</ishfield>
    ///   </and>
    ///   <ishsort>
    ///     <ishsortfield name = 'ISHSCORE' level='none' ishorder="d"/>
    ///   </ishsort>
    ///   <ishobjectfilters>
    ///     <ishversionfilter>LatestVersion</ishversionfilter>
    ///     <ishtypefilter>ISHModule</ishtypefilter>
    ///     <ishtypefilter>ISHLibrary</ishtypefilter>
    ///     <ishtypefilter>ISHMasterDoc</ishtypefilter>
    ///     <ishlanguagefilter>en</ishlanguagefilter>
    ///   </ishobjectfilters>
    /// </ishquery>
    /// "@
    /// $requestedMetadata = Set-IshRequestedMetadataField -Level Lng -Name FISHSTATUSTYPE
    /// Search-IshDocumentObj -XmlQuery $xmlQuery -MaxHitsToReturn 100 -RequestedMetadata $requestedMetadata
    /// </code>
    /// <para>New-IshSession will submit into SessionState, so it can be reused by this cmdlet. Executes a Full-Text-Index search for 'change oil filter' in the ANY field of the LatestVersion collection (compared to AllVersion), filtered on these object types and language; results are returned sorted by the Full-Text-Index engine, and enriched with FISHSTATUSTYPE field on top of the $ishSession.DefaultRequestedMetadata</para>
    /// </example>
    [Cmdlet(VerbsCommon.Search, "IshDocumentObj", SupportsShouldProcess = false)]
    [OutputType(typeof(IshDocumentObj),typeof(long))]
    public sealed class SearchIshDocumentObj : DocumentObjCmdlet
    {
        /// <summary>
        /// <para type="description">The IshSession variable holds the authentication and contract information. This object can be initialized using the New-IshSession cmdlet.</para>
        /// </summary>
        [Parameter(Mandatory =false, ValueFromPipelineByPropertyName = false, ParameterSetName = "XmlQueryGroup")]
        [Parameter(Mandatory =false, ValueFromPipelineByPropertyName = false, ParameterSetName = "SimpleQueryGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "XmlQueryCountGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "SimpleQueryCountGroup")]
        [ValidateNotNullOrEmpty]
        public IshSession IshSession { get; set; }

        /// <summary>
        /// <para type="description">The RAW xml ishquery used by Search25 API</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "SimpleQueryGroup")]
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "SimpleQueryCountGroup")]
        [ValidateNotNullOrEmpty]
        public string SimpleQuery
        {
            get { return _simpleQuery; }
            set { _simpleQuery = value; }
        }

        /// <summary>
        /// <para type="description">The RAW xml ishquery used by Search25 API</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "XmlQueryGroup")]
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "XmlQueryCountGroup")]
        [ValidateNotNullOrEmpty]
        public string XmlQuery
        {
            get { return _xmlQuery; }
            set { _xmlQuery = value; }
        }

        /// <summary>
        /// <para type="description">The maximum amount of hits to return on the pipeline as IshObject[].</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "XmlQueryGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "SimpleQueryGroup")]
        [ValidateNotNullOrEmpty]
        public long MaxHitsToReturn
        {
            get { return _maxHitsToReturn; }
            set { _maxHitsToReturn = value; }
        }

        /// <summary>
        /// <para type="description">The metadata fields to retrieve</para>
        /// </summary>
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "XmlQueryGroup")]
        [Parameter(Mandatory = false, ValueFromPipelineByPropertyName = false, ParameterSetName = "SimpleQueryGroup")]
        [ValidateNotNullOrEmpty]
        public IshField[] RequestedMetadata { get; set; }

        /// <summary>
        /// <para type="description">Switch parameter that specifies to only return the count of the query and not IshObject[]</para>
        /// </summary>
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "XmlQueryCountGroup")]
        [Parameter(Mandatory = true, ValueFromPipelineByPropertyName = false, ParameterSetName = "SimpleQueryCountGroup")]
        [ValidateNotNullOrEmpty]
        public SwitchParameter Count
        {
            get { return _onlyCount; }
            set { _onlyCount = value; }
        }

        
        #region Private fields
        /// <summary>
        /// Private field to store the simple ISHANYWHERE query to later construct the XmlQuery
        /// </summary>
        private string _simpleQuery = "";

        /// <summary>
        /// Private field to store the incoming &gt;ishquery&lt; that is litteraly passed into Search25 API.
        /// </summary>
        private string _xmlQuery = "";

        /// <summary>
        /// Private field to store the amount of search results to push back on the pipeline
        /// </summary>
        private long _maxHitsToReturn = 20;

        /// <summary>
        /// Private field to store if only a Count query should be executed, so without returning any results on the pipeline
        /// </summary>
        private bool _onlyCount = false;
        #endregion

        protected override void BeginProcessing()
        {
            if (IshSession == null) { IshSession = (IshSession)SessionState.PSVariable.GetValue(ISHRemoteSessionStateIshSession); }
            if (IshSession == null) { throw new ArgumentException(ISHRemoteSessionStateIshSessionException); }
            WriteDebug($"Using IshSession[{IshSession.Name}] from SessionState.{ISHRemoteSessionStateIshSession}");
            base.BeginProcessing();
        }

        /// <summary>
        /// Process the cmdlet.
        /// </summary>
        protected override void ProcessRecord()
        {
            try
            {
                // 1. Validating the input
                switch (ParameterSetName)
                {
                    case "SimpleQueryGroup":
                    case "SimpleQueryCountGroup":
                        WriteDebug("Validating and converting incoming SimpleQuery");
                        break;
                    case "XmlQueryGroup":
                    case "XmlQueryCountGroup":
                        WriteDebug("Validating incoming XmlQuery");
                        break;
                }

                // 2. Executing
                List<IshObject> returnIshObjects = new List<IshObject>();
                switch (ParameterSetName)
                {
                    case "SimpleQueryCountGroup":
                    case "XmlQueryCountGroup":
                        // 2A. Executing the count query
                        WriteDebug("Executing a count query");
                        _maxHitsToReturn = 0; // because that is what -Count does
                        break;

                    case "XmlQueryGroup":
                    case "SimpleQueryGroup":
                        // 2B. Executing the result query
                        WriteDebug("Executing a result query");
                        Search25ServiceReference.PerformSearchResponse performSearchResponse = IshSession.Search25.PerformSearch(new Search25ServiceReference.PerformSearchRequest(_xmlQuery, _maxHitsToReturn));
                        var lngCardIds = new IshSearchResults(performSearchResponse.xmlSearchResults).LngRefs;

                        // 3. Retrieving metadata on the search result
                        WriteDebug("Retrieving metadata on the search result");
                        IshFields requestedMetadata = IshSession.IshTypeFieldSetup.ToIshRequestedMetadataFields(IshSession.DefaultRequestedMetadata, ISHType, new IshFields(RequestedMetadata), Enumerations.ActionMode.Read);

                        //RetrieveMetadata
                        WriteDebug("Retrieving CardIds.length[{lngCardIds.Count}] RequestedMetadata.length[{requestedMetadata.ToXml().Length}] 0/{lngCardIds.Count}");
                        // Devides the list of language card ids in different lists that all have maximally MetadataBatchSize elements
                        List<List<long>> devidedlngCardIdsList = DevideListInBatches<long>(lngCardIds, IshSession.MetadataBatchSize);
                        int currentLngCardIdCount = 0;
                        foreach (List<long> lngCardIdBatch in devidedlngCardIdsList)
                        {
                            // Process language card ids in batches
                            string xmlIshObjects = IshSession.DocumentObj25.RetrieveMetadataByIshLngRefs(
                                lngCardIdBatch.ToArray(),
                                requestedMetadata.ToXml());
                            IshObjects retrievedObjects = new IshObjects(ISHType, xmlIshObjects);
                            returnIshObjects.AddRange(retrievedObjects.Objects);
                            currentLngCardIdCount += lngCardIdBatch.Count;
                            WriteDebug($"Retrieving CardIds.length[{lngCardIdBatch.Count}] RequestedMetadata.length[{requestedMetadata.ToXml().Length}] including data {currentLngCardIdCount}/{lngCardIds.Count}");
                        }

                        //TODO [Should] Search for massive search results, the returnIshObjects should be reset per for loop and also pushed on the pipeline... count should just be long object
                        //TODO [Should] Search should get Progressbar like Get-IshFolder Recurse
                        WriteVerbose("returned object count[" + returnIshObjects.Count + "]");
                        WriteObject(IshSession, ISHType, returnIshObjects.ConvertAll(x => (IshBaseObject)x), true);
                        break;
                }
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
