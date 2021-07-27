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
    /// <para type="synopsis">Search - so Full-Text-Index as data source, while Find is Relational Database as data source - matching the search criteria. Metadata is retrieved like Get-IshDocumentObj would, from the Relational Database.</para>
    /// <para type="description">Search - so Full-Text-Index as data source, while Find is Relational Database as data source - matching the search criteria. Metadata is retrieved like Get-IshDocumentObj would, from the Relational Database.</para>
    /// <para type="description">Passes the search criteria to the Search25 API where the result set is capped by -MaxHitsToReturn. See online documentation for advanced query support on this 'ishquery' domain specific language.</para>
    /// <para type="description">Then the search results is enriched using the DocumentObj25 API to retrieve ishobjects. As the output are IshObjects on the pipeline any further handling by other ISHRemote cmdlets or more is enabled.</para>
    /// </summary>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/InfoShareWS/" -PSCredential Admin
    /// Search-IshDocumentObj -SimpleQuery "bluetooth" -MaxHitsToReturn 200
    /// </code>
    /// <para>New-IshSession will submit into SessionState, so it can be reused by this cmdlet.</para>
    /// <para>Executes a Full-Text-Index search for 'bluetooth' in the ANY field of the LatestVersion collection (compared to AllVersion), with no filter on object types and only in the user's language. The MaxHitsToReturn limits the Full-Text-Index result set.</para>
    /// <para>Results are returned sorted by the Full-Text-Index engine, first by score then by title.</para>
    /// </example>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/InfoShareWS/" -PSCredential Admin
    /// Search-IshDocumentObj -SimpleQuery "bluetooth" -Count
    /// </code>
    /// <para>New-IshSession will submit into SessionState, so it can be reused by this cmdlet.</para>
    /// <para>Executes a Full-Text-Index search for 'bluetooth' in the ANY field of the LatestVersion collection (compared to AllVersion), with no filter on object types and only in the user's language.</para>
    /// <para>Results a count of hits, there are no IshObjects on the pipeline (MaxHitsToReturn is 0).</para>
    /// </example>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/InfoShareWS/" -PSCredential Admin
    /// Search-IshDocumentObj -SimpleQuery "*" -MaxHitsToReturn 2000 |
    /// Out-GridView -PassThru
    /// </code>
    /// <para>New-IshSession will submit into SessionState, so it can be reused by this cmdlet.</para>
    /// <para>Executes a Full-Text-Index search for '*' in the ANY field of the LatestVersion collection (so all latest version content objects are returned), with no filter on object types and only in the user's language. The MaxHitsToReturn limits the Full-Text-Index result set.</para>
    /// <para>Results are passed to Out-GridView where manual filtering can happen and the selection is passed to the pipeline for further processing.</para>
    /// </example>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/InfoShareWS/" -PSCredential Admin
    /// Search-IshDocumentObj -SimpleQuery "red AND green AND blue" | 
    /// Get-IshDocumentObjData -FolderPath c:\temp\
    /// </code>
    /// <para>New-IshSession will submit into SessionState, so it can be reused by this cmdlet.</para>
    /// <para>Executes a Full-Text-Index search for 'red AND green AND blue' in the ANY field of the LatestVersion collection (compared to AllVersion), with no filter on object types and only in the user's language. Note that the AND keyword is recognized as boolean operator for the ISHANYWHERE field, see API documentation.</para>
    /// <para>Results are returned sorted by the Full-Text-Index engine, first by score then by title. And this IshObjects are passed to Get-IshDocumentObjData for (xml) file downloading.</para>
    /// </example>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/InfoShareWS/" -PSCredential Admin
    /// $xmlQuery = @"
    /// &lt;ishquery&gt;
    ///   &lt;and&gt;&lt;ishfield name='ISHANYWHERE' level='none' ishoperator='contains'&gt;change oil filter&lt;/ishfield&gt;&lt;/and&gt;
    ///   &lt;ishsort&gt;
    ///     &lt;ishsortfield name='ISHSCORE' level='none' ishorder='d'/&gt;
    ///     &lt;ishsortfield name='FTITLE' level='logical' ishorder='d'/&gt;
    ///   &lt;/ishsort&gt;
    ///   &lt;ishobjectfilters&gt;
    ///     &lt;ishversionfilter&gt;LatestVersion&lt;/ishversionfilter&gt;
    ///     &lt;ishtypefilter&gt;ISHModule&lt;/ishtypefilter&gt;
    ///     &lt;ishtypefilter&gt;ISHMasterDoc&lt;/ishtypefilter&gt;
    ///     &lt;ishtypefilter&gt;ISHLibrary&lt;/ishtypefilter&gt;
    ///     &lt;ishtypefilter&gt;ISHTemplate&lt;/ishtypefilter&gt;
    ///     &lt;ishtypefilter&gt;ISHIllustration&lt;/ishtypefilter&gt;
    ///     &lt;ishlanguagefilter&gt;en&lt;/ishlanguagefilter&gt;
    ///   &lt;/ishobjectfilters&gt;
    /// &lt;/ishquery&gt;
    /// "@
    /// $requestedMetadata = Set-IshRequestedMetadataField -Level Lng -Name FISHSTATUSTYPE
    /// Search-IshDocumentObj -XmlQuery $xmlQuery -MaxHitsToReturn 100 -RequestedMetadata $requestedMetadata
    /// </code>
    /// <para>New-IshSession will submit into SessionState, so it can be reused by this cmdlet.</para>
    /// <para>Executes a Full-Text-Index search for 'change oil filter' in the ANY field of the LatestVersion collection (compared to AllVersion), filtered on these object types and language; results are returned sorted by the Full-Text-Index engine, and enriched with FISHSTATUSTYPE field on top of the $ishSession.DefaultRequestedMetadata</para>
    /// <para>The provided $xmlQuery is the default query of parameter set lead by -SimpleQuery; where the ISHANYWHERE is overwritten with the given -SimpleQuery value, and the ishlanguagefilter is overwritten with the user's language.</para>
    /// </example>
    /// <example>
    /// <code>
    /// $ishSession = New-IshSession -WsBaseUrl "https://example.com/InfoShareWS/" -PSCredential Admin
    /// $xmlQuery = @"
    /// &lt;ishquery&gt;
    ///   &lt;and&gt;&lt;ishfield name='ISHANYWHERE' level='none' ishoperator='contains'&gt;change oil filter&lt;/ishfield&gt;&lt;/and&gt;
    ///   &lt;ishsort&gt;
    ///     &lt;ishsortfield name='ISHSCORE' level='none' ishorder='d'/&gt;
    ///     &lt;ishsortfield name='FTITLE' level='logical' ishorder='d'/&gt;
    ///   &lt;/ishsort&gt;
    ///   &lt;ishobjectfilters&gt;
    ///     &lt;ishversionfilter&gt;LatestVersion&lt;/ishversionfilter&gt;
    ///     &lt;ishtypefilter&gt;ISHModule&lt;/ishtypefilter&gt;
    ///     &lt;ishtypefilter&gt;ISHMasterDoc&lt;/ishtypefilter&gt;
    ///     &lt;ishtypefilter&gt;ISHLibrary&lt;/ishtypefilter&gt;
    ///     &lt;ishtypefilter&gt;ISHTemplate&lt;/ishtypefilter&gt;
    ///     &lt;ishtypefilter&gt;ISHIllustration&lt;/ishtypefilter&gt;
    ///     &lt;ishlanguagefilter&gt;en&lt;/ishlanguagefilter&gt;
    ///   &lt;/ishobjectfilters&gt;
    /// &lt;/ishquery&gt;
    /// "@
    /// $requestedMetadata = Set-IshRequestedMetadataField -Level Lng -Name FISHSTATUSTYPE
    /// Search-IshDocumentObj -XmlQuery $xmlQuery -Count
    /// </code>
    /// <para>New-IshSession will submit into SessionState, so it can be reused by this cmdlet.</para>
    /// <para>Executes a Full-Text-Index search for 'change oil filter' in the ANY field of the LatestVersion collection (compared to AllVersion), filtered on these object types and language.</para>
    /// <para>Results a count of hits, there are no IshObjects on the pipeline (MaxHitsToReturn is 0).</para>
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
                // 1. Transform the incoming simple query
                switch (ParameterSetName)
                {
                    case "SimpleQueryGroup":
                    case "SimpleQueryCountGroup":
                        WriteDebug("Loading default query");
                        _xmlQuery = @"<ishquery>
                                      <and><ishfield name='ISHANYWHERE' level='none' ishoperator='contains'>change oil filter</ishfield></and>
                                      <ishsort>
                                        <ishsortfield name='ISHSCORE' level='none' ishorder='d'/>
                                        <ishsortfield name='FTITLE' level='logical' ishorder='d'/>
                                      </ishsort>
                                      <ishobjectfilters>
                                        <ishversionfilter>LatestVersion</ishversionfilter>
                                        <ishtypefilter>ISHModule</ishtypefilter>
                                        <ishtypefilter>ISHMasterDoc</ishtypefilter>
                                        <ishtypefilter>ISHLibrary</ishtypefilter>
                                        <ishtypefilter>ISHTemplate</ishtypefilter>
                                        <ishtypefilter>ISHIllustration</ishtypefilter>
                                        <ishlanguagefilter>en</ishlanguagefilter>
                                      </ishobjectfilters>
                                      </ishquery>";
                        XmlDocument xmlDocument = new XmlDocument();
                        xmlDocument.LoadXml(_xmlQuery);
                        // Converting type filter, could be something like FolderTypeFilter in Get-IshFolder, not for now
                        WriteDebug("Setting query string based on SimpleQuery");
                        xmlDocument.SelectSingleNode("ishquery/and/ishfield").InnerText = SimpleQuery;
                        WriteDebug("Setting language based on IshSession");
                        xmlDocument.SelectSingleNode("ishquery/ishobjectfilters/ishlanguagefilter").InnerText = IshSession.UserLanguage;
                        WriteDebug("Validating transformed XmlQuery");
                        _xmlQuery = xmlDocument.OuterXml;
                        WriteDebug("Validated transformed XmlQuery[" + xmlDocument.OuterXml + "]");
                        
                        break;
                }
                // 2. Validate the incoming or generated raw xml query
                switch (ParameterSetName)
                {
                    case "XmlQueryGroup":
                    case "XmlQueryCountGroup":
                        WriteDebug("Validating incoming XmlQuery");
                        //debug//_xmlQuery = @"<ishquery><and><ishfield name='ISHANYWHERE'  level='none' ishoperator='contains'>change oil filter</ishfield></and>
                        //debug//   <ishsort><ishsortfield name='ISHSCORE' level='none' ishorder='d'/></ishsort>
                        //debug//   <ishobjectfilters><ishversionfilter>LatestVersion</ishversionfilter><ishtypefilter>ISHModule</ishtypefilter><ishtypefilter>ISHMasterDoc</ishtypefilter><ishlanguagefilter>en</ishlanguagefilter>
                        //debug//   </ishobjectfilters></ishquery>";
                        XmlDocument xmlDocument = new XmlDocument();
                        xmlDocument.LoadXml(_xmlQuery);
                        _xmlQuery = xmlDocument.OuterXml;
                        WriteDebug("Validated incoming XmlQuery[" + xmlDocument.OuterXml + "]");
                        break;
                }

                // 3. Executing
                switch (ParameterSetName)
                {
                    case "SimpleQueryCountGroup":
                    case "XmlQueryCountGroup":
                        {
                            // 3A. Executing the count query
                            WriteVerbose("Executing a count query");
                            _maxHitsToReturn = 0; // because that is what -Count does
                            Search25ServiceReference.PerformSearchResponse performSearchResponse = IshSession.Search25.PerformSearch(new Search25ServiceReference.PerformSearchRequest(_xmlQuery, _maxHitsToReturn));
                            WriteObject(performSearchResponse.totalHitsFound);
                        }
                        break;

                    case "XmlQueryGroup":
                    case "SimpleQueryGroup":
                        {
                            // 3B. Executing the result query
                            WriteVerbose("Executing a result query");
                            Search25ServiceReference.PerformSearchResponse performSearchResponse = IshSession.Search25.PerformSearch(new Search25ServiceReference.PerformSearchRequest(_xmlQuery, _maxHitsToReturn));
                            var lngCardIds = new IshSearchResults(performSearchResponse.xmlSearchResults).LngRefs;

                            if (lngCardIds.Count <= 0)
                            {
                                WriteWarning($"Search-IshDocumentObj returned 0 results for xmlQuery[{_xmlQuery}]");
                            }
                            else
                            { 
                                // 4. Retrieving metadata on the search result
                                WriteDebug("Retrieving metadata on the search result");
                                IshFields requestedMetadata = IshSession.IshTypeFieldSetup.ToIshRequestedMetadataFields(IshSession.DefaultRequestedMetadata, ISHType, new IshFields(RequestedMetadata), Enumerations.ActionMode.Read);

                                //RetrieveMetadata
                                WriteDebug("Retrieving CardIds.length[" + lngCardIds.Count + "] RequestedMetadata.length[" + requestedMetadata.ToXml().Length + "] 0/" + lngCardIds.Count);
                                // Devides the list of language card ids in different lists that all have maximally MetadataBatchSize elements
                                List<List<long>> devidedlngCardIdsList = DevideListInBatches<long>(lngCardIds, IshSession.MetadataBatchSize);
                                int currentLngCardIdCount = 0;
                                WriteParentProgress("Retrieving metadata for search results...", currentLngCardIdCount, lngCardIds.Count);
                                foreach (List<long> lngCardIdBatch in devidedlngCardIdsList)
                                {
                                    // Process language card ids in batches
                                    string xmlIshObjects = IshSession.DocumentObj25.RetrieveMetadataByIshLngRefs(
                                        lngCardIdBatch.ToArray(),
                                        requestedMetadata.ToXml());
                                    IshObjects retrievedObjects = new IshObjects(ISHType, xmlIshObjects);
                                    List<IshObject> returnIshObjects = new List<IshObject>();
                                    returnIshObjects.AddRange(retrievedObjects.Objects);
                                    WriteObject(IshSession, ISHType, returnIshObjects.ConvertAll(x => (IshBaseObject)x), true);
                                    currentLngCardIdCount += lngCardIdBatch.Count;
                                    WriteParentProgress("Retrieving metadata for search results...", currentLngCardIdCount, lngCardIds.Count);
                                    WriteDebug("Retrieving CardIds.length[" + lngCardIdBatch.Count + "] RequestedMetadata.length[" + requestedMetadata.ToXml().Length + "] " + currentLngCardIdCount + "/" + lngCardIds.Count);
                                }
                                WriteVerbose("returned object count[" + currentLngCardIdCount + "]");
                            }
                        }
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
