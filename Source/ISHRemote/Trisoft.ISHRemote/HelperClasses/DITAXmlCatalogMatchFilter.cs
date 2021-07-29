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
using System.Text;

namespace Trisoft.ISHRemote.HelperClasses
{
    /// <summary>
    /// Filtervalues to use for searching in the DITA generalization mapping file
    /// </summary>
    public class DitaXmlCatalogMatchFilter
    {

        #region Constructor

        /// <summary>
        /// Constructor
        /// </summary>
        public DitaXmlCatalogMatchFilter()
        {
            IgnoreMatchesWithMissingGeneralizedDTDInfo = true;
        }

        #endregion


        #region Public Properties

        /// <summary>
        /// Name of the rootelement of the specialized DTD
        /// </summary>
        public string RootElement
        {
            get;
            set;
        }
        /// <summary>
        /// PublicId of the specialized DTD
        /// </summary>
        public string DtdPublicId
        {
            get;
            set;
        }
        /// <summary>
        /// SystemId of the specialized DTD
        /// </summary>
        public string DtdSystemId
        {
            get;
            set;
        }        

        /// <summary>
        /// If true ignores rows that do not have a generalizeddtdpublicid or generalizeddtdsystemid attribute or without a generalized root element attribute
        /// </summary>
        public bool IgnoreMatchesWithMissingGeneralizedDTDInfo
        {
            get;
            set;
        }


        #endregion
    }
}
