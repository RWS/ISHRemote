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

//****************** XmlOASISCatalog class was branched from TFS development version and logging was removed ******************//
//********* We branched from the version in DEV - Changeset 6388 dated 30/01/2012 - File date was 19/02/2012 ******************//


using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Xml;


namespace Trisoft.ISHRemote.HelperClasses
{
    /// <summary>
    /// This class can resolve Public/System identifiers to one Uri DTD path and supports nextCatalog statements.
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
        private const string NextCatalogXPath = "/cat:catalog//cat:nextCatalog";

        /// <summary>
        /// XPath to find public defined DTDs
        /// </summary>
        private const string PublicXPath = "/cat:catalog//cat:public";

        /// <summary>
        /// XPath to find DTDs that are defined with a SystemId
        /// </summary>
        private const string SystemXPath = "/cat:catalog//cat:system";

        /// <summary>
        /// XPath to find the xml:base attribute starting from a specified XmlNode
        /// </summary>
        private const string BaseUriXPath = "ancestor-or-self::*[@xml:base][1]/@xml:base";

        #endregion

        #endregion

        #region Members
        /// <summary>
        /// Dictionary with the PublicIds. 
        /// The key of the dictionary is the PublicId and the value is the absoluteUri.
        /// </summary>
        private readonly Dictionary<string, string> _publicIds;

        /// <summary>
        /// Dictionary with the SystemIds. 
        /// The key of the dictionary is the SystemId and the value is the absoluteUri.
        /// </summary>
        private readonly Dictionary<string, string> _systemIds;

        /// <summary>
        /// XmlNamespaceManager stores the prefix and namespace of the OASIS catalog
        /// </summary>
        private readonly XmlNamespaceManager _nameSpaceManager;

        /// <summary>
        /// The XmlUrlResolver is internally used to append the relative uri with the base uri.
        /// </summary>
        private readonly XmlUrlResolver _xmlUrlResolver;
        /// <summary>
        /// XMLCatalogFile in the TriDKApp of the current InfoShare project
        /// </summary>
        private Uri _catalogUri;
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
            _xmlUrlResolver = new XmlUrlResolver
            {
                Credentials = CredentialCache.DefaultCredentials
            };

            _catalogUri = new Uri(catalogUri);

            // Resolve the Catalog and load the dictionaries
            if (catalogUri != string.Empty)
            {
                // Load the XML Catalog File
                var catalog = new XmlDocument();
                try
                {
                    catalog.Load(catalogUri);
                }
                catch (Exception)
                {
                    throw;
                }

                // Make one large catalog that includes all references to other catalogs
                ResolveNextCatalog(catalog);

                // Make a dictionary with the PublicIds
                LoadPublicIds(catalog);

                // Make a dictionary with the SystemIds
                LoadSystemIds(catalog);
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
            var id = Normalize(systemId);

            if (!_systemIds.TryGetValue(id, out var absoluteUri))
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
            if (!_publicIds.TryGetValue(publicId, out var absoluteUri))
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
            var id = Normalize(systemId);
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

        /// <summary>
        /// Determines whether the catalog URI is a base of the specified Uri instance, or
        /// whether the specified URI is defined in the catalog.
        /// </summary>
        /// <param name="uri">The specified URI to test.</param>
        public void ValidateUri(Uri uri)
        {
            if (_catalogUri.IsBaseOf(uri)) return;
            if (_publicIds.ContainsValue(uri.AbsoluteUri)) return;
            if (_systemIds.ContainsValue(uri.AbsoluteUri)) return;
            throw new FileNotFoundException($"The URI '{uri}' is not trusted.");
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
            var nextCatalogList = catalog.SelectNodes(NextCatalogXPath, _nameSpaceManager);
            foreach (XmlElement nextCatalog in nextCatalogList)
            {
                InsertNextCatalog(catalog, GetNextCatalog(nextCatalog), nextCatalog);
                nextCatalog.ParentNode.RemoveChild(nextCatalog);
            }
        }

        /// <summary>
        /// Insert all child nodes of the NextCatalog after the reference node.
        /// </summary>
        /// <param name="catalog">The current Catalog</param>
        /// <param name="nextCatalog">The NextCatalog</param>
        /// <param name="refNode">The reference node</param>
        private XmlNode InsertNextCatalog(XmlDocument catalog, XmlDocument nextCatalog, XmlNode refNode)
        {
            foreach (XmlNode childNode in nextCatalog.DocumentElement.ChildNodes)
            {
                if (childNode.LocalName == "nextCatalog")
                {
                    refNode = InsertNextCatalog(catalog, GetNextCatalog(childNode), refNode);
                }
                else
                {
                    if (childNode.GetType() == typeof(XmlElement))
                    {
                        var uriAttribute = childNode.Attributes.GetNamedItem(UriAttribute);
                        if (uriAttribute != null)
                        {
                            var baseUri = GetBaseUri(uriAttribute);

                            var absoluteUri = _xmlUrlResolver.ResolveUri(baseUri, uriAttribute.InnerText).AbsoluteUri;
                            uriAttribute.InnerText = absoluteUri;
                        }

                        foreach (XmlElement childOfChildNode in childNode.SelectNodes(".//*[@" + UriAttribute + "!='']"))
                        {
                            uriAttribute = childOfChildNode.Attributes.GetNamedItem(UriAttribute);
                            var baseUri = GetBaseUri(uriAttribute);
                            var absoluteUri = _xmlUrlResolver.ResolveUri(baseUri, uriAttribute.InnerText).AbsoluteUri;
                            uriAttribute.InnerText = absoluteUri;
                        }

                        var newNode = catalog.ImportNode(childNode, true);
                        refNode = catalog.DocumentElement.InsertAfter(newNode, refNode);
                    }
                }
            }

            return refNode;
        }

        /// <summary>
        /// Load the XML document of the referenced catalog
        /// </summary>
        /// <param name="nextCatalogNode">NextCatalog node</param>
        /// <returns>XML document with the NextCatalog</returns>
        private XmlDocument GetNextCatalog(XmlNode nextCatalogNode)
        {
            var nextCatalog = new XmlDocument();
            var catalogAttribute = nextCatalogNode.Attributes.GetNamedItem("catalog");

            if (catalogAttribute != null)
            {
                var absoluteUri = _xmlUrlResolver.ResolveUri(GetBaseUri(catalogAttribute), catalogAttribute.InnerText).AbsoluteUri;
                try
                {
                    nextCatalog.Load(absoluteUri);
                }
                catch (Exception)
                {
                    throw;
                }
            }

            return nextCatalog;
        }

        /// <summary>
        /// Return the xml:base attribute in the catalog.
        /// First try to find the BaseUri attribute on the start node.  
        /// If there is no BaseUri attribute on the start node, try to find the attribute on the ancestor nodes.
        /// </summary>
        /// <param name="node">Start node</param>
        /// <returns>
        /// BaseUri of the catalog.  
        /// When no xml:base attribute is specified, null is returned.
        /// </returns>
        private Uri GetBaseUri(XmlNode node)
        {
            // Code copied from AuthoringClient\...\Xml\XmlCommon\GetBaseUri.
            if (node != null)
            {
                var baseNode = node.SelectSingleNode(BaseUriXPath, _nameSpaceManager);
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

                            return new Uri(baseNode.InnerText);
                        }
                    }
                }

                if (node.BaseURI != string.Empty)
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
        private void LoadPublicIds(XmlNode catalog)
        {
            foreach (XmlNode node in catalog.SelectNodes(PublicXPath, _nameSpaceManager))
            {
                var publicNode = node.Attributes.GetNamedItem(PublicIdAttribute);
                if (publicNode != null)
                {
                    var publicId = publicNode.InnerText;
                    if (publicId != string.Empty)
                    {
                        if (!_publicIds.ContainsKey(publicId))
                        {
                            var uriNode = node.Attributes.GetNamedItem(UriAttribute);
                            if (uriNode != null)
                            {
                                var relativeUri = uriNode.InnerText;
                                var baseUri = GetBaseUri(uriNode);
                                var absoluteUri = _xmlUrlResolver.ResolveUri(baseUri, relativeUri).AbsoluteUri;
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
        private void LoadSystemIds(XmlNode catalog)
        {
            foreach (XmlNode node in catalog.SelectNodes(SystemXPath, _nameSpaceManager))
            {
                var systemNode = node.Attributes.GetNamedItem(SystemIdAttribute);
                if (systemNode != null)
                {
                    var systemId = Normalize(systemNode.InnerText);
                    if (systemId != string.Empty)
                    {
                        if (!_systemIds.ContainsKey(systemId))
                        {
                            var uriNode = node.Attributes.GetNamedItem(UriAttribute);
                            if (uriNode != null)
                            {
                                var relativeUri = uriNode.InnerText;
                                var baseUri = GetBaseUri(uriNode);
                                var absoluteUri = _xmlUrlResolver.ResolveUri(baseUri, relativeUri).AbsoluteUri;
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

        public Dictionary<string, string> PublicIds => _publicIds;

        public Dictionary<string, string> SystemIds => _systemIds;

        /// <summary>
        /// To normalize the given FilePath by creating a Uri and returning the AbsoluteUri.
        /// When the normalization fails, the FilePath is returned.
        /// </summary>
        /// <param name="filePath">A FilePath</param>
        private static string Normalize(string filePath)
        {
            try
            {
                // Make sure to unescape first (to prevent characters to be escaped twice)
                // E.g. : %20 becomes %2520 if you don't use the unescape function first
                return Uri.TryCreate(Uri.UnescapeDataString(filePath), UriKind.Absolute, out var path)
                    ? path.AbsoluteUri
                    : filePath;
            }
            catch
            {
                return filePath;
            }
        }

        #endregion
    }
}
