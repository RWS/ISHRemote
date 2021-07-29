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

//****************** XmlResolverUsingCatalog class was branched from TFS development version and logging was removed ******************//
//**************** We branched from the version in DEV - Changeset 6365 dated 27/01/2012 - File date was 27/01//2012 ******************//


using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Net;

namespace Trisoft.ISHRemote.HelperClasses
{
    /// <summary>
    /// XmlResolver that uses the XMLOASISCatalog to resolve the location of DTD's
    /// </summary>
    public class XmlResolverUsingCatalog : XmlUrlResolver
    {
        /// <summary>
        /// The XmlOASISCatalog is used to resolve Public/System indentifiers to one Uri DTD path
        /// </summary>
        private XmlOASISCatalog _xmlOASISCatalog;

        #region Constructors
        /// <summary>
        /// Initialize an instance of the XmlResolverUsingCatalog.  
        /// </summary>
        public XmlResolverUsingCatalog()
            : base()
        {
            _xmlOASISCatalog = null;
        }
        /// <summary>
        /// Initialize an instance of the XmlResolverUsingCatalog.  
        /// This also involves the creation of a XmlOASISCatalog. 
        /// </summary>
        /// <param name="catalogUri">Filename (and path) of the XMLCatalogFile</param>
        public XmlResolverUsingCatalog(string catalogUri)
            :base()
        {
            _xmlOASISCatalog = null;
            if (catalogUri != string.Empty)
            {
                _xmlOASISCatalog = new XmlOASISCatalog(catalogUri);
            }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// This method overrides the ResolveUri of the XmlResolver. <seealso cref="System.Xml.XmlResolver.ResolveUri"/>
        /// The method uses the XmlOASISCatalog to resolve the absolute URI from the base and relative URIs.
        /// </summary>
        /// <param name="baseUri">The base URI used to resolve the relative URI</param>
        /// <param name="relativeUri">The URI to resolve. The URI can be absolute or relative.</param>
        /// <returns>A Uri representing the absolute URI or a null reference if the relative URI cannot be resolved.</returns>
        public override Uri ResolveUri(Uri baseUri, string relativeUri)
        {
            if (_xmlOASISCatalog == null)
            {
                //Use the base method
                return base.ResolveUri(baseUri, relativeUri);
            }
            else
            {
                return base.ResolveUri(baseUri, _xmlOASISCatalog.ResolveSystemId(_xmlOASISCatalog.ResolvePublicId(relativeUri)));
            }
        }

        /// <summary>
        /// Returns whether a matching entry for the given publicId was found in the catalog
        /// </summary>
        /// <param name="publicId">The PublicId</param>
        /// <param name="resolvedSystemId">The systemId corresponding to the matching publicId entry in the catalog</param>
        public bool TryResolveDTDPublicId(string publicId, out string resolvedSystemId)
        {
            string systemId;
            if (_xmlOASISCatalog.TryResolvePublicId(publicId, out systemId))
            {
                if (!_xmlOASISCatalog.TryResolveSystemId(systemId, out resolvedSystemId))
                {
                    resolvedSystemId = systemId;
                }
                return true;
            }

            resolvedSystemId = null;
            return false;
        }

        /// <summary>
        /// Returns whether a matching entry for the given systemId was found in the catalog
        /// </summary>
        /// <param name="systemId">The SystemId</param>
        /// <param name="resolvedSystemId">The systemId corresponding to the matching publicId entry in the catalog</param>
        public bool TryResolveDTDSystemId(string systemId, out string resolvedSystemId)
        {
            return _xmlOASISCatalog.TryResolveSystemId(systemId, out resolvedSystemId);
        }

        #endregion
    }
}
