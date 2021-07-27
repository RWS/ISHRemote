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

//****************** XmlOASISCatalog class was branched from TFS development version and logging was removed ******************//
//********* We branched from the version in DEV - Changeset 6388 dated 30/01/2012 - File date was 19/02/2012 ******************//


using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Net;

namespace Trisoft.ISHRemote.HelperClasses
{
    /// <summary>
    /// This class can resolve Public/System indentifiers to one Uri DTD path and supports nextCatalog statements.
    /// </summary>
    public class XmlOASISCatalog
    {
        #region Constants
        /// <summary>
        /// Fixed user reference when logging
        /// </summary>
        private const string UserReference = "ISHCNFG";
        /// <summary>
        /// Fixed separator for the UserReference
        /// The UserReference is created by a combination of the UserReference and the ProcessId.
        /// </summary>
        private const string UserReferenceSeparator = ":";

        /// <summary>
        /// Namespace of the OASIS catalog
        /// </summary>
        private const string CatalogNamespace = "urn:oasis:names:tc:entity:xmlns:xml:catalog";
        /// <summary>
        /// Namespace prefix of the OASIS catalog
        /// </summary>
        private const string CatalogPrefix = "cat";
        /// <summary>
        /// Attribute with the PublicId
        /// </summary>
        private const string PublicIdAttribute = "publicId";
        /// <summary>
        /// Attribute with the SystemId
        /// </summary>
        private const string SystemIdAttribute = "systemId";
        /// <summary>
        /// Attribute with the Uri
        /// </summary>
        private const string UriAttribute = "uri";

        #region XPath Constants 
        /// <summary>
        /// XPath to find references to other catalogs (= NextCatalog)
        /// </summary>
        private const string NextCatalogXPath = "/cat:catalog/cat:nextCatalog";
        /// <summary>
        /// XPath to find public defined DTD's
        /// </summary>
        private const string PublicXPath = "/cat:catalog/cat:public";
        /// <summary>
        /// XPath to find DTD's that are defined with a SystemId
        /// </summary>
        private const string SystemXPath = "/cat:catalog/cat:system";
        /// <summary>
        /// XPath to find the xml:base attribute starting from a specified XmlNode
        /// </summary>
        private const string BaseUriXPath = "ancestor-or-self::*[@xml:base][1]/@xml:base";
        #endregion
        #endregion

        #region Members
        /// <summary>
        /// XMLCatalogFile in the TriDKApp of the current InfoShare project
        /// </summary>
        private string _catalogUri;
        /// <summary>
        /// Dictionary with the PublicIds. 
        /// The key of the dictionary is the PublicId and the value is the absoluteUri.
        /// </summary>
        private Dictionary<string, string> _publicIds;
        /// <summary>
        /// Dictionary with the SystemIds. 
        /// The key of the dictionary is the SystemId and the value is the absoluteUri.
        /// </summary>
        private Dictionary<string, string> _systemIds;
        /// <summary>
        /// XmlNamespaceManager stores the prefixe and namespace of the OASIS catalog
        /// </summary>
        private XmlNamespaceManager _nameSpaceManager;
        /// <summary>
        /// The XmlUrlResolver is internally used to append the relative uri with the base uri.
        /// </summary>
        private XmlUrlResolver _xmlUrlResolver;

        /// <summary>
        /// User reference string (inclusive ProcessId)
        /// </summary>
        private string _userReference;
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes an instance of the XmlOASISCatalog with the given CatalogUri
        /// </summary>
        /// <param name="catalogUri">Filename (and path) of the XMLCatalogFile</param>        
        public XmlOASISCatalog(string catalogUri)
        {
            _userReference = UserReference + UserReferenceSeparator + System.Diagnostics.Process.GetCurrentProcess().Id.ToString();

            // Initialize the dictionaries
            _publicIds = new Dictionary<string, string>();
            _systemIds = new Dictionary<string, string>();

            // Initialize the XmlNamespaceManager
            _nameSpaceManager = new XmlNamespaceManager(new NameTable());
            _nameSpaceManager.AddNamespace(CatalogPrefix, CatalogNamespace);

            // Initialize the XmlUrlResolver (for internal usage only)
            _xmlUrlResolver = new XmlUrlResolver();
            _xmlUrlResolver.Credentials = CredentialCache.DefaultCredentials;

            // Resolve the Catalog and load the dictionaries
            if (catalogUri != string.Empty)
            {
                _catalogUri = catalogUri;

                // Load the XML Catalog File
                XmlDocument catalog = new XmlDocument();
                try
                {
                    catalog.Load(_catalogUri);
                }
                catch (Exception ex)
                {
                    throw ex;
                }

                if (catalog != null)
                {
                    // Make one large catalog that includes all references to other catalogs
                    ResolveNextCatalog(catalog);

                    // Make a dictionary with the PublicIds
                    LoadPublicIds(catalog);

                    // Make a dictionary with the SystemIds
                    LoadSystemIds(catalog);
                }
            }
        }
        #endregion

        #region Public methods
        /// <summary>
        /// Returns the absolute Uri of the given SystemId.
        /// </summary>
        /// <param name="systemId">The SystemId</param>
        public string ResolveSystemId(string systemId)
        {
            string id = Normalize(systemId);

            string absoluteUri;
            if (!_systemIds.TryGetValue(id, out absoluteUri))
            {
                absoluteUri = id;
            }
            return absoluteUri;
        }
        /// <summary>
        /// Returns the absolute Uri of the given PublicId.
        /// </summary>
        /// <param name="publicId">The PublicId</param>
        public string ResolvePublicId(string publicId)
        {
            string absoluteUri;
            if (!_publicIds.TryGetValue(publicId, out absoluteUri))
            {
                absoluteUri = publicId;
            }
            return absoluteUri;
        }

        /// <summary>
        /// Returns whether a matching entry for the given systemId was found in the catalog
        /// </summary>
        /// <param name="systemId">The SystemId</param>
        /// <param name="resolvedSystemId">The systemId corresponding to the matching publicId entry in the catalog</param>
        public bool TryResolveSystemId(string systemId, out string resolvedSystemId)
        {
            string id = Normalize(systemId);

            return _systemIds.TryGetValue(id, out resolvedSystemId);
        }
        /// <summary>
        /// Returns whether a matching entry for the given publicId was found in the catalog
        /// </summary>
        /// <param name="publicId">The PublicId</param>
        /// <param name="resolvedSystemId">The systemId corresponding to the matching publicId entry in the catalog</param>
        public bool TryResolvePublicId(string publicId, out string resolvedSystemId)
        {
            return _publicIds.TryGetValue(publicId, out resolvedSystemId);
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Resolve references to other catalogs (= NextCatalog) and include the full catalog.
        /// This method results in one large catalog with all information.
        /// </summary>
        /// <param name="catalog">The main catalog</param>
        private void ResolveNextCatalog(XmlDocument catalog)
        {
            XmlNodeList nextCatalogList = catalog.SelectNodes(NextCatalogXPath, _nameSpaceManager);
            foreach (XmlElement nextCatalog in nextCatalogList)
            {
                XmlNode refNode = nextCatalog;
                InsertNextCatalog(catalog, GetNextCatalog(nextCatalog), refNode);
                nextCatalog.ParentNode.RemoveChild(nextCatalog);
            }
        }
        /// <summary>
        /// Insert all childnodes of the NextCatalog after the reference node.
        /// </summary>
        /// <param name="catalog">The current Catalog</param>
        /// <param name="nextCatalog">The NextCatalog</param>
        /// <param name="refNode">The reference node</param>
        private void InsertNextCatalog(XmlDocument catalog, XmlDocument nextCatalog, XmlNode refNode)
        {

            
            foreach (XmlNode childNode in nextCatalog.DocumentElement.ChildNodes)
            {
                if (childNode.LocalName == "nextCatalog")
                {
                    InsertNextCatalog(catalog, GetNextCatalog(childNode), refNode);
                }
                else
                {
                    if (childNode.GetType() == typeof(System.Xml.XmlElement))
                    {

                        XmlNode uriAttribute = childNode.Attributes.GetNamedItem(UriAttribute);
                        if (uriAttribute != null)
                        {                           
                            Uri baseUri = GetBaseUri(uriAttribute);

                            string absoluteUri = _xmlUrlResolver.ResolveUri(baseUri, uriAttribute.InnerText).AbsoluteUri;
                            uriAttribute.InnerText = absoluteUri;
                        }

                        XmlNode newNode = catalog.ImportNode(childNode, true);
                        refNode = catalog.DocumentElement.InsertAfter(newNode, refNode);
                    }
                }
            }
        }
        /// <summary>
        /// Load the XML document of the referenced catalog
        /// </summary>
        /// <param name="nextCatalogNode">NextCatalog node</param>
        /// <returns>XML document with the NextCatalog</returns>
        private XmlDocument GetNextCatalog(XmlNode nextCatalogNode)
        {


            XmlDocument nextCatalog = new XmlDocument();
            XmlNode catalogAttribute = nextCatalogNode.Attributes.GetNamedItem("catalog");

            if (catalogAttribute != null)
            {

                string absoluteUri = _xmlUrlResolver.ResolveUri(GetBaseUri(catalogAttribute), catalogAttribute.InnerText).AbsoluteUri;
                try
                {
                    nextCatalog.Load(absoluteUri);
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }
            return nextCatalog;
        }
        /// <summary>
        /// Return the xml:base attribute in the catalog.
        /// First try to find the BaseUri attribute on the start node.  
        /// If there is no BaseUri attribute on the start node, try to find the attribute on the ancestor nodes.
        /// </summary>
        /// <param name="node">Startnode</param>
        /// <returns>
        /// BaseUri of the catalog.  
        /// When no xml:base attribute is specified, null is returned.
        /// </returns>
        private Uri GetBaseUri(XmlNode node)
        {
            // Code copied from AuthoringClient\...\Xml\XmlCommon\GetBaseUri.
            if (node != null)
            {
                XmlNode baseNode = node.SelectSingleNode(BaseUriXPath,_nameSpaceManager);
                if (baseNode != null)
                {
                    if (baseNode.InnerText != string.Empty)
                    {
                        if (baseNode.BaseURI == node.BaseURI)
                        {
                            if (baseNode.BaseURI != string.Empty)
                            {
                                return new Uri(new Uri(node.BaseURI), baseNode.InnerText);
                            }
                            else
                            {
                                return new Uri(baseNode.InnerText);
                            }
                        }
                    }
                }

                if (node.BaseURI != String.Empty)
                {
                    return new Uri(node.BaseURI);
                }
            }
            return null;
        }
        /// <summary>
        /// Initialize the dictionary with all PublicIds and their AbsoluteUri from the complete catalog
        /// </summary>
        /// <param name="catalog">XML document with the catalog</param>
        private void LoadPublicIds(XmlDocument catalog)
        {


            string publicId;
            string relativeUri;
            Uri baseUri;
            string absoluteUri;

            foreach (XmlNode node in catalog.SelectNodes(PublicXPath, _nameSpaceManager))
            {
                XmlNode publicNode = node.Attributes.GetNamedItem(PublicIdAttribute);
                if (publicNode != null)
                {
                    publicId = publicNode.InnerText;
                    if (publicId != string.Empty)
                    {
                        if (!_publicIds.ContainsKey(publicId))
                        {
                            XmlNode uriNode = node.Attributes.GetNamedItem(UriAttribute);
                            if (uriNode != null)
                            {
                                relativeUri = uriNode.InnerText;
                                baseUri = GetBaseUri(uriNode);
                                absoluteUri = _xmlUrlResolver.ResolveUri(baseUri, relativeUri).AbsoluteUri;
                                if (absoluteUri != string.Empty)
                                {
                                    _publicIds.Add(publicId, absoluteUri);

                                }
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// Initialize the dictionary with all SystemIds and their AbsoluteUri from the complete catalog
        /// </summary>
        /// <param name="catalog">XML document with the catalog</param>
        private void LoadSystemIds(XmlDocument catalog)
        {


            string systemId;
            string relativeUri;
            Uri baseUri;
            string absoluteUri;

            foreach (XmlNode node in catalog.SelectNodes(SystemXPath, _nameSpaceManager))
            {
                XmlNode systemNode = node.Attributes.GetNamedItem(SystemIdAttribute);
                if (systemNode != null)
                {
                    systemId = Normalize(systemNode.InnerText);
                    if (systemId != string.Empty)
                    {
                        if (!_systemIds.ContainsKey(systemId))
                        {
                            XmlNode uriNode = node.Attributes.GetNamedItem(UriAttribute);
                            if (uriNode != null)
                            {
                                relativeUri = uriNode.InnerText;
                                baseUri = GetBaseUri(uriNode);
                                absoluteUri = _xmlUrlResolver.ResolveUri(baseUri, relativeUri).AbsoluteUri;
                                if (absoluteUri != string.Empty)
                                {
                                    _systemIds.Add(systemId, absoluteUri);
                                }
                            }
                        }
                    }
                }
            }
        }
        /// <summary>
        /// To normalize the given FilePath by creating a Uri and returning the AbsoluteUri.
        /// When the normalizatin fails, the FilePath is returned.
        /// </summary>
        /// <param name="filePath">A FilePath</param>
        private string Normalize(string filePath)
        {
            try
            {
                Uri path = new Uri(filePath);
                return path.AbsoluteUri;
            }
            catch
            {
                return filePath;
            }
        }
        #endregion
    }
}
