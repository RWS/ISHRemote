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
using System.Text;
using System.Xml;
using Trisoft.ISHRemote.Objects.Public;

namespace Trisoft.ISHRemote.Objects
{
    /// <summary>
    /// <para type="description">Container object to group multiple SEQUENCED/ordered IshObject search result entries.</para>
    /// </summary>
    internal class IshSearchResults
    {
        /// <summary>
        /// List with the events
        /// </summary>
        private SortedDictionary<long,long> _searchResults;

        /// <summary>
        /// Creates a new instance of the <see cref="IshSearchResults"/> class.
        /// </summary>
        /// <param name="xmlIshSearchResults">The xml containing the search results.</param>
        public IshSearchResults(string xmlIshSearchResults)
        {
            // <ishsearchresults>
            // <ishobjects>
            // <ishobject ishsequence="1" ishref="GUID-B44976AE-73DE-41B6-8126-6F277A1B5F83" ishlogicalref="43583" ishtype="ISHModule" ishversionref="43587" ishlngref="277911" />
            // <ishobject ishsequence="2" ishref="GUID-B44976AE-73DE-41B6-8126-6F277A1B5F83" ishlogicalref="43583" ishtype="ISHModule" ishversionref="43587" ishlngref="43588" />
            // </ishobjects>
            // </ishsearchresults>
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xmlIshSearchResults);
            _searchResults = new SortedDictionary<long, long>();
            foreach (XmlNode ishSearchObject in xmlDocument.SelectNodes("ishsearchresults/ishobjects/ishobject"))
            {
                _searchResults.Add(
                    Convert.ToInt64(ishSearchObject.Attributes["ishsequence"].Value), 
                    Convert.ToInt64(ishSearchObject.Attributes["ishlngref"].Value));
            }
        }


        /// <summary>
        /// Return all language card ids (ishlngrefs) of the search result in search order (by ishsequence)
        /// </summary>
        public List<long> LngRefs
        {
            get
            {
                return _searchResults.Values.ToList();
            }
        }
    }
}

