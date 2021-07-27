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
using System.Linq;
using System.IO;
using System.Xml.Linq;

namespace Trisoft.ISHRemote.HelperClasses
{
    /// <summary>
    /// Functionality to find the generalized DTD for a specialized DTD
    /// </summary>
    public class DitaXmlGeneralizationCatalogMapping
    {

        #region Private Properties

        private string _generalizedCatalogMappingUri;
        private IEnumerable<DitaXmlCatalogMatch> _generalizedCatalogMapping;

        #endregion

        #region Constructors


        /// <summary>
        /// Creates an instance of this class and loads the generalization catalog mapping file
        /// </summary>
        /// <param name="generalizedCatalogMappingUri">Location of the generalization catalog mapping file, typically 'c:\infoshare\web\author\asp\doctypes\generalization-catalog-mapping.xml'.</param>
        public DitaXmlGeneralizationCatalogMapping(string generalizedCatalogMappingUri)
        {
            if (generalizedCatalogMappingUri == null || !File.Exists(generalizedCatalogMappingUri)) throw new Exception("'catalogUri' is mandatory or is not valid!");

            _generalizedCatalogMappingUri = generalizedCatalogMappingUri;
            _generalizedCatalogMapping = ReadCatalogMapping(generalizedCatalogMappingUri);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Searches for all matches on the specified filter criterea and returns the preferred match.
        /// The preferred match is found as follows:
        /// First we try to find a match entry where the dtdpublicid + dtdsystemid + rootelement all match -> if we find matches, the first one is returned
        /// Second we try to find a match entry where the dtdpublicid + rootelement both match -> if we find matches, the first one is returned
        /// Third we try to find a match entry where the dtdsystemid + rootelement both match -> if we find matches, the first one is returned
        /// Last we try to find a match entry where the rootelement matches -> if we find matches, the first one is returned
        /// </summary>
        /// <param name="filter">filter criterea to search on</param>
        /// <returns>The preferred match</returns>
        public DitaXmlCatalogMatch GetPreferredMatch(DitaXmlCatalogMatchFilter filter)
        {
            DitaXmlCatalogMatch match = null;

            if (!String.IsNullOrEmpty(filter.DtdPublicId) && !String.IsNullOrEmpty(filter.DtdSystemId) && !String.IsNullOrEmpty(filter.RootElement))
            {
                match = FindFirstFilterMatch(x => x.DtdPublicId == filter.DtdPublicId && x.DtdSystemId == filter.DtdSystemId && x.RootElement == filter.RootElement, filter.IgnoreMatchesWithMissingGeneralizedDTDInfo);
            }

            if (match == null && !String.IsNullOrEmpty(filter.DtdPublicId) && !String.IsNullOrEmpty(filter.RootElement))
            {
                match = FindFirstFilterMatch(x => x.DtdPublicId == filter.DtdPublicId && x.RootElement == filter.RootElement, filter.IgnoreMatchesWithMissingGeneralizedDTDInfo);
            }

            if (match == null && !String.IsNullOrEmpty(filter.DtdSystemId) && !String.IsNullOrEmpty(filter.RootElement))
            {
                match = FindFirstFilterMatch(x => x.DtdSystemId == filter.DtdSystemId && x.RootElement == filter.RootElement, filter.IgnoreMatchesWithMissingGeneralizedDTDInfo);
            }

            if (match == null && !String.IsNullOrEmpty(filter.RootElement))
            {
                match = FindFirstFilterMatch(x => x.RootElement == filter.RootElement, filter.IgnoreMatchesWithMissingGeneralizedDTDInfo);
            }

            if (match != null)
            {
                return match;
            }
            return null;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Reads the generalized-catalog-mapping.xml file into an XElement structure
        /// </summary>
        /// <param name="catalogMappingLocation"></param>
        /// <returns></returns>
        private IEnumerable<DitaXmlCatalogMatch> ReadCatalogMapping(string catalogMappingLocation)
        {
            IEnumerable<DitaXmlCatalogMatch> catalogMapping = null;
            try
            {
                // this.Log(System.Reflection.MethodInfo.GetCurrentMethod().Name, "Loading generalized catalog mapping '" + _catalogUri + "'");

                XElement catalogMappingElement = XElement.Load(_generalizedCatalogMappingUri);

                // this.Log(System.Reflection.MethodInfo.GetCurrentMethod().Name, "Deserializing generalized catalog mapping '" + _catalogUri + "'");

                var catalogMatchesList = from match in catalogMappingElement.Descendants("match")
                                         select new DitaXmlCatalogMatch()
                                         {
                                             RootElement = (match.HasAttributes && match.Attribute("rootelement") != null) ? match.Attribute("rootelement").Value : String.Empty,
                                             DtdSystemId = (match.HasAttributes && match.Attribute("dtdsystemid") != null) ? match.Attribute("dtdsystemid").Value : String.Empty,
                                             DtdPublicId = (match.HasAttributes && match.Attribute("dtdpublicid") != null) ? match.Attribute("dtdpublicid").Value : String.Empty,
                                             GeneralizedRootElement = (match.HasAttributes && match.Attribute("generalizedrootelement") != null) ? match.Attribute("generalizedrootelement").Value : String.Empty,
                                             GeneralizedDtdSystemId = (match.HasAttributes && match.Attribute("generalizeddtdsystemid") != null) ? match.Attribute("generalizeddtdsystemid").Value : String.Empty,
                                             GeneralizedDtdPublicId = (match.HasAttributes && match.Attribute("generalizeddtdpublicid") != null) ? match.Attribute("generalizeddtdpublicid").Value : String.Empty,

                                         };

                catalogMapping = catalogMatchesList;
            }
            catch (Exception)
            {
                throw;
            }

            return catalogMapping;
        }


        /// <summary>
        /// This function executes the given function and returns the first match that also complies to the ignoreMatchesWithMissingGeneralizedDTDInfo parameter
        /// </summary>
        /// <param name="matchFunction">Function to fine the matches</param>
        /// <param name="ignoreMatchesWithMissingGeneralizedDTDInfo">When true: removes all matches that do not have a generalizeddtdpublicid or generalizeddtdsystemid attribute or without a generalized root element attribute</param>
        /// <returns>The first matching mapping elements</returns>
        private DitaXmlCatalogMatch FindFirstFilterMatch(Func<DitaXmlCatalogMatch, bool> matchFunction, bool ignoreMatchesWithMissingGeneralizedDTDInfo)
        {
            List<DitaXmlCatalogMatch> filteredMatches = new List<DitaXmlCatalogMatch>();
            filteredMatches = _generalizedCatalogMapping.Where(matchFunction).ToList<DitaXmlCatalogMatch>();
            if (filteredMatches != null && ignoreMatchesWithMissingGeneralizedDTDInfo)
            {
                // Remove matches without the necess generalized DTD or generalizedrootword entries
                filteredMatches.RemoveAll(x => (string.IsNullOrEmpty(x.GeneralizedDtdPublicId) && string.IsNullOrEmpty(x.GeneralizedDtdSystemId)) || string.IsNullOrEmpty(x.GeneralizedRootElement));
            }
            if (filteredMatches != null && filteredMatches.Count() > 0)
            {
                return filteredMatches.First();
            }
            return null;
        }


        #endregion

    }
}
