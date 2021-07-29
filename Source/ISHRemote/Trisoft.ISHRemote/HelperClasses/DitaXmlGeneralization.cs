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
using System.Xml;
using System.IO;
using System.Xml.Schema;

namespace Trisoft.ISHRemote.HelperClasses
{
    /// <summary>
    /// This class can be used to convert a specialized DITA DTD xml to a more generic DITA DTD xml
    /// </summary>
    public class DitaXmlGeneralization
    {

        #region Private Properties

        /// <summary>
        /// An instance of the XmlResolverUsingCatalog class. This can be used to load the specialized xml file
        /// </summary>
        private XmlResolverUsingCatalog _xmlSpecializedXmlResolver = null;
        /// <summary>
        /// An instance of the XmlResolverUsingCatalog class. This can be used to load the generalized xml file
        /// </summary>
        private XmlResolverUsingCatalog _xmlGeneralizedXmlResolver = null;
        /// <summary>
        /// An instance of the XmlGeneralizationCatalogMapping class. This is used to do the mapping of the specialized DTD to the generalized DTD
        /// </summary>
        private DitaXmlGeneralizationCatalogMapping _xmlGeneralizationCatalogMapping = null;

        #endregion

        #region Constructors

        /// <summary>
        /// Creates an instance of this class with the given parameters
        /// </summary>
        /// <param name="specializedXmlResolver">XmlResolver for a Catalog location with specialized DTDs</param>
        /// <param name="generalizedXmlResolver">XmlResolver for a Catalog with the base DTDs</param>
        /// <param name="generalizationCatalogMapping">XmlGeneralizationCatalogMapping with the mapping file to find the generalized DTD for a certain specialized DTD</param>
        public DitaXmlGeneralization(XmlResolverUsingCatalog specializedXmlResolver, XmlResolverUsingCatalog generalizedXmlResolver, DitaXmlGeneralizationCatalogMapping generalizationCatalogMapping)
        {
            _xmlSpecializedXmlResolver = specializedXmlResolver;
            _xmlGeneralizedXmlResolver = generalizedXmlResolver;
            _xmlGeneralizationCatalogMapping = generalizationCatalogMapping;
        }

        #endregion


        #region Public properties

        /// <summary>
        /// An array with the attribute names specialized from the DITA "props" attribute
        /// </summary>
        public string[] AttributesToGeneralizeToProps
        {
            get;
            set;
        }
        /// <summary>
        /// An array with the attribute names specialized from the DITA "base" attribute
        /// </summary>
        public string[] AttributesToGeneralizeToBase
        {
            get;
            set;
        }

        #endregion

        #region Public methods

        /// <summary>
        /// Generalizes the given specialized xml file. 
        /// When the generalization succeeds a file is created at the location given in generalizedXmlFileLocation
        /// When the generalization fails a file is created at the location given in errorLogLocation. If this is empty no error file is created.
        /// </summary>
        /// <param name="specializedXmlFileLocation">The location of the specialized xml file</param>
        /// <param name="generalizedXmlFileLocation">The location for the generalized xml file</param>
        /// <returns>true if the generalization succeeded, false if not</returns>
        public void Generalize(FileInfo specializedXmlFileLocation, FileInfo generalizedXmlFileLocation)
        {
            // Validate input Xml        
            Validate(specializedXmlFileLocation.FullName, InitializeXmlReaderSettings(_xmlSpecializedXmlResolver, ValidationType.DTD));

            // Get the corresponding generalized DTD info for the DTD/root element of the specialized input Xml
            XmlGeneralizedConstructionInfo xmlGeneralizedConstructionInfo = GetGeneralizedDTD(specializedXmlFileLocation.FullName);

            // Loop over the specialized xml and generalize elements and attributes (non validating)
            using (XmlWriter xmlWriter = XmlWriter.Create(generalizedXmlFileLocation.FullName, InitializeXmlWriterSettings(xmlGeneralizedConstructionInfo.Encoding)))
            {
                using (XmlReader xmlReader = XmlReader.Create(specializedXmlFileLocation.FullName, InitializeXmlReaderSettings(_xmlSpecializedXmlResolver, ValidationType.None)))
                {
                    bool rootElementEncountered = false;
                    string previousElementName = "";

                    xmlReader.Read();
                    while (!xmlReader.EOF)
                    {
                        string currElementName = xmlReader.Name;
                        
                        switch (xmlReader.NodeType)
                        {
                            case XmlNodeType.DocumentType:
                                // Write generalized DTD
                                xmlWriter.WriteDocType(xmlGeneralizedConstructionInfo.RootElement, xmlGeneralizedConstructionInfo.DtdPublicId, xmlGeneralizedConstructionInfo.DtdSystemId, xmlGeneralizedConstructionInfo.DtdInternalSubset);
                                xmlReader.Read();
                                break;
                                        
                            case XmlNodeType.Element:                                
                                string elementNameToInsert = null;
                                if (!rootElementEncountered)
                                {
                                    // Since this is the root element, use the root element of the generalized DTD
                                    elementNameToInsert = xmlGeneralizedConstructionInfo.RootElement;
                                    rootElementEncountered = true;
                                }
                                else
                                {
                                    // Determine generalized element name
                                    elementNameToInsert = GetGeneralizedElement(xmlReader, xmlGeneralizedConstructionInfo);
                                }

                                // Write generalized element and attributes to the writer
                                if (xmlReader.IsEmptyElement)
                                {
                                    xmlWriter.WriteStartElement(elementNameToInsert);
                                    WriteGeneralizedAttributes(xmlReader, xmlWriter);
                                    xmlWriter.WriteEndElement();
                                }
                                else
                                {
                                    xmlWriter.WriteStartElement(elementNameToInsert);
                                    WriteGeneralizedAttributes(xmlReader, xmlWriter);
                                }
                                previousElementName = elementNameToInsert;

                                xmlReader.Read();

                                break;

                            case XmlNodeType.EndElement:
                                // Write element close. (WriteNode might not work)
                                xmlWriter.WriteEndElement();

                                xmlReader.Read();
                                break;
 
                            default:
                                // Just write all other nodes to the writer without any conversion
                                xmlWriter.WriteNode(xmlReader, false);
                                break;

                        }
                    }                                
                }
            }
            
            // Validate generalized result Xml
            Validate(generalizedXmlFileLocation.FullName, InitializeXmlReaderSettings(_xmlGeneralizedXmlResolver, ValidationType.DTD));
            
        }

        #endregion

        #region Private methods

        /// <summary>
        /// Parses the xml at the given file location and tries to lookup the generalized dtd in the generalized-catalog-mapping.xml file.        
        /// </summary>
        /// <param name="specializedXmlFileLocation">File location of the specialized xml file</param>
        /// <returns>
        /// If a match is found in the generalized-catalog-mapping.xml file, a XmlGeneralizedConstructionInfo with the necess info (DTD/root element/encoding) to create the generalized xml is returned
        /// If no match is found an InvalidOperationException is raised
        /// </returns>
        private XmlGeneralizedConstructionInfo GetGeneralizedDTD(string specializedXmlFileLocation)
        {
            XmlGeneralizedConstructionInfo xmlGeneralizedConstructionInfo = new XmlGeneralizedConstructionInfo();
            DitaXmlCatalogMatchFilter filter = new DitaXmlCatalogMatchFilter();

            using (XmlReader xmlReader = XmlReader.Create(specializedXmlFileLocation, InitializeXmlReaderSettings(_xmlSpecializedXmlResolver, ValidationType.None) ) )
            {
                while (xmlReader.Read())
                {
                    switch (xmlReader.NodeType)
                    {
                        case XmlNodeType.XmlDeclaration:
                            // Get specialized xml encoding
                            if (xmlReader.MoveToAttribute("encoding"))
                            {                                    
                                xmlGeneralizedConstructionInfo.Encoding = Encoding.GetEncoding(xmlReader.Value);                                    
                            }
                            break;

                        case XmlNodeType.DocumentType:
                            // Add specialized DTD info to the filter
                            filter.DtdPublicId = xmlReader.GetAttribute("PUBLIC");
                            filter.DtdSystemId = xmlReader.GetAttribute("SYSTEM");
                            xmlGeneralizedConstructionInfo.DtdInternalSubset = xmlReader.Value;
                            break;

                        case XmlNodeType.Element:
                            // Add specialized root element to the filter
                            filter.RootElement = xmlReader.Name;

                            // try to find a match in the catalog with the found filter parameters
                            DitaXmlCatalogMatch match = _xmlGeneralizationCatalogMapping.GetPreferredMatch(filter);
                            if (match != null)
                            {
                                // A match is found, so copy the found generalized DTD info to the construction info
                                xmlGeneralizedConstructionInfo.DtdPublicId = match.GeneralizedDtdPublicId;
                                xmlGeneralizedConstructionInfo.DtdSystemId = match.GeneralizedDtdSystemId;
                                xmlGeneralizedConstructionInfo.RootElement = match.GeneralizedRootElement;
                                xmlGeneralizedConstructionInfo.AllowedDomains = GetGeneralizedDomainsAttribute(match);

                                return xmlGeneralizedConstructionInfo;
                            }
                            else
                            {
                                // No match found
                                throw new InvalidOperationException("No matching generalized dtd found for rootElement=" + filter.RootElement + " in dtdPublicId=" + filter.DtdPublicId + "dtdSystemId=" + filter.DtdSystemId);
                            }
                    }
                }
            }
            return null;
        }

        /// <summary>
        /// Determines the generalized element name from the specialized element
        /// To do this, it uses the class attributes of the specialized element and the domains and rootelement of the generalized DTD
        /// </summary>
        /// <param name="xmlReader">XmlReader positioned on an Element node</param>
        /// <param name="xmlGeneralizedConstructionInfo">XmlGeneralizedConstructionInfo with the info of the generalized DTD</param>
        /// <returns>The element name of the generalized element</returns>
        private string GetGeneralizedElement(XmlReader xmlReader, XmlGeneralizedConstructionInfo xmlGeneralizedConstructionInfo)
        {
            string elementNameToInsert = null;
            string classAttribute = xmlReader.GetAttribute("class");
            if (!String.IsNullOrEmpty(classAttribute))
            {
                DtdClass dtdClass = new DtdClass(classAttribute);

                // Lookup the parts of the specialized class in the list of allowed domains of the generalized DTD
                // We try to match the most specific class part, so we start from the last part (more specific) to the first                                    
                foreach (DtdClassDomainElementPart classPart in dtdClass.DomainElementPartsInReversedOrder)
                {
                    // First check for a domain match. If a domain match is found, we take the matching element name
                    foreach (DtdDomainsPart domainsPart in xmlGeneralizedConstructionInfo.AllowedDomains.DomainsParts)
                    {
                        if (classPart.Domain == domainsPart.DomainName)
                        {
                            elementNameToInsert = classPart.ElementName;
                            break;
                        }
                    }

                    // If found break out of foreach loop, otherwise check with the rootelement e.g. "reference"
                    if (elementNameToInsert != null)
                    {
                        break;
                    }
                    else if (classPart.Domain == xmlGeneralizedConstructionInfo.RootElement)
                    {
                        elementNameToInsert = classPart.ElementName;
                        break;
                    }

                }

                // If we now don't have an element we take the first one of the class string
                if (elementNameToInsert == null)
                {
                    elementNameToInsert = dtdClass.DomainElementParts.First<DtdClassDomainElementPart>().ElementName;
                }
            }
            else
            {
                throw new InvalidOperationException("No class attribute found for element " + xmlReader.Name);
            }

            if (elementNameToInsert == "caption")
            {
                elementNameToInsert = "p";
            }
            return elementNameToInsert;
        }


        public void WriteGeneralizedAttributes(XmlReader xmlReader, XmlWriter xmlWriter)
        {
            if (!xmlReader.HasAttributes)
            {
                return;
            }


            if ( (AttributesToGeneralizeToProps != null && AttributesToGeneralizeToProps.Length > 0) || (AttributesToGeneralizeToBase != null && AttributesToGeneralizeToBase.Length > 0) )
            { 
                while (xmlReader.MoveToNextAttribute())
                {
                    // Do not write attributes that are defined as IXED attributes in the DTD
                    if (!xmlReader.IsDefault)
                    { 
                        // Check whether this attribute is specialized or not
                        if (AttributesToGeneralizeToProps != null && AttributesToGeneralizeToProps.Contains(xmlReader.Name))
                        {
                            // TODO: [Should] generalize this attribute to the "props" attribute
                            // propsAttributeString += something reader.Name reader.Value
                            // Write the "props" attribute string later on, since there can be more attributes specialized from "props"
                        }
                        else if (AttributesToGeneralizeToBase != null && AttributesToGeneralizeToBase.Contains(xmlReader.Name))
                        {
                            // TODO: [Should] generalize this attribute to the "base" attribute
                            // baseAttributeString += something reader.Name reader.Value
                            // Write the "base" attribute string later on, since there can be more attributes specialized from "base"
                        }
                        else
                        {
                            xmlWriter.WriteAttributeString(xmlReader.Prefix, xmlReader.LocalName, xmlReader.NamespaceURI, xmlReader.Value);
                        }
                    }
                }
            }
            else
            {
                // There are no specialized attributes, so just write all attributes to the writer
                xmlWriter.WriteAttributes(xmlReader, false);
            }

        }


        /// <summary>
        /// Creates an XmlReaderSettings with the given resolver, validationType and ValidationCallbackHandler
        /// </summary>
        private XmlReaderSettings InitializeXmlReaderSettings(XmlResolverUsingCatalog xmlResolver, ValidationType validationType)
        {
            // When ProhibitDTD = true, the following error occurs:
            // "For security reasons DTD is prohibited in this XML document. To enable DTD processing set the ProhibitDtd property on XmlReaderSettings to false and pass the settings into XmlReader.Create method."
            XmlReaderSettings  xmlReaderSettings = new XmlReaderSettings();
            xmlReaderSettings.ConformanceLevel = ConformanceLevel.Auto;
            xmlReaderSettings.DtdProcessing = DtdProcessing.Parse;
            xmlReaderSettings.ValidationType = validationType;
            if (validationType != ValidationType.None)
            {
                xmlReaderSettings.ValidationEventHandler += new System.Xml.Schema.ValidationEventHandler(ValidationHandler);
            }
            xmlReaderSettings.XmlResolver = xmlResolver;
            xmlReaderSettings.IgnoreComments = false;
            xmlReaderSettings.IgnoreProcessingInstructions = false;
            xmlReaderSettings.IgnoreWhitespace = false;
            xmlReaderSettings.CloseInput = true;
            return xmlReaderSettings;
        }

        /// <summary>
        /// Creates an XmlWriterSettings with the given encoding
        /// </summary>
        private XmlWriterSettings InitializeXmlWriterSettings(Encoding encoding)
        {

            XmlWriterSettings xmlWriterSettings = new XmlWriterSettings();
            xmlWriterSettings.CloseOutput = true;
            xmlWriterSettings.Encoding = encoding;
            return xmlWriterSettings;
        }

        /// <summary>
        /// Loads and Validates the xml at the given file location. It uses the given xmlresolver
        /// Throws an exception when validation fails
        /// </summary>
        /// <param name="xmlFileLocation">File location of the generalized xml file</param>
        /// <param name="xmlReaderSettings">XmlReaderSettings object with ValidationType specified</param>
        /// <returns>true if the file is valid, false if not</returns>
        private void Validate(string xmlFileLocation, XmlReaderSettings xmlReaderSettings)
        {
            using (StreamReader streamReader = new StreamReader(xmlFileLocation))
            {
                using (XmlReader xmlReader = XmlReader.Create(streamReader, xmlReaderSettings))
                {
                    while (xmlReader.Read())
                    {
                    }
                }
            }

        }

        /// <summary>
        /// Handler method for validation errors. Throws an exception when an error is encountered
        /// </summary>
        /// <param name="sender">Sender of the validation event</param>
        /// <param name="args">Validation error arguments</param>
        private void ValidationHandler(object sender, System.Xml.Schema.ValidationEventArgs args)
        {
            switch (args.Severity)
            {
                case System.Xml.Schema.XmlSeverityType.Error:
                    XmlSchemaException xmlSchemaException = args.Exception as XmlSchemaException;
                    if (xmlSchemaException != null)
                    {
                        throw new InvalidOperationException("Xml validation error '" + xmlSchemaException.Message + "' at line " + xmlSchemaException.LineNumber + " position " + xmlSchemaException.LinePosition);
                    }
                    else
                    { 
                        throw new InvalidOperationException("Xml validation error '" + args.Message + "'");
                    }
                case System.Xml.Schema.XmlSeverityType.Warning:
                    if (args.Message == "No DTD found.")       // Unfortunately there does not seem to be a typed exception for not having a DTD, so need to test the message :-(
                    {
                        throw new InvalidOperationException("Xml validation error '" + args.Message + "'");
                    }
                    // Else: Do nothing
                    break;
            }
        }

        /// <summary>
        /// This function return the domains attribute that is on the root element of the generalized DTD
        /// </summary>
        /// <param name="match">DitaXmlCatalogMatch with the matching generalization catalog entry</param>
        /// <returns>DtdDomains object</returns>
        private DtdDomains GetGeneralizedDomainsAttribute(DitaXmlCatalogMatch match)
        {
            XmlDocument xml = new XmlDocument();
            xml.XmlResolver = _xmlGeneralizedXmlResolver;
            XmlDocumentType docType = xml.CreateDocumentType(match.GeneralizedRootElement, match.GeneralizedDtdPublicId, match.GeneralizedDtdSystemId, "");
            xml.AppendChild(docType);
            XmlElement rootElement = xml.CreateElement(match.GeneralizedRootElement);
            xml.AppendChild(rootElement);

            // FIXED attributes don't seem to be loaded now, therefore we load the xml with the DTD again
            xml.LoadXml(xml.OuterXml);
            if (xml.DocumentElement.HasAttribute("domains"))
            {
                string domains = xml.DocumentElement.GetAttribute("domains");
                xml = null;
                return new DtdDomains(domains);
            }
            else
            {
                xml = null;
                throw new InvalidOperationException("No domains attribute found for rootElement=" + match.GeneralizedRootElement + " dtdPublicId=" + match.GeneralizedDtdPublicId + "dtdSystemId=" + match.GeneralizedDtdSystemId);
            }
        }

        #endregion


        #region Private classes


        /// <summary>
        /// This class can be used to parse and store a DITA "class" attribute string
        /// </summary>
        private class DtdClass
        {
            /// <summary>
            /// Initializes an instance of the DtdClass with the given classAttribute
            /// </summary>
            /// <param name="classAttribute">DITA "class" attribute string</param>   
            public DtdClass(string classAttribute)
            {
                ClassAttribute = classAttribute;
                PrefixPart = "";

                string attribute = classAttribute.Trim();
                if (attribute.StartsWith("-"))
                {
                    attribute = attribute.Substring(2).Trim();
                    PrefixPart = "-";
                }
                else if (attribute.StartsWith("+"))
                {
                    attribute = attribute.Substring(2).Trim();
                    PrefixPart = "+";
                }

                DomainElementParts = new List<DtdClassDomainElementPart>();
                DomainElementPartsInReversedOrder = new List<DtdClassDomainElementPart>();
                string[] classParts = attribute.Split(new Char[] { ' ' });
                foreach (string part in classParts)
                {
                    string trimmedPart = part.Trim();
                    if (trimmedPart.Length > 0)
                    {
                        DomainElementParts.Add(new DtdClassDomainElementPart(trimmedPart));
                        DomainElementPartsInReversedOrder.Add(new DtdClassDomainElementPart(trimmedPart));
                    }
                }

                DomainElementPartsInReversedOrder.Reverse();

            }

            /// <summary>
            /// Entire DITA "class" attribute string
            /// </summary>
            public string ClassAttribute
            {
                get;
                private set;
            }

            /// <summary>
            /// DITA "class" prefix part, containing the "+" or "-" sign
            /// </summary>
            public string PrefixPart
            {
                get;
                private set;
            }

            /// <summary>
            /// List of the DITA "class" domain/elementname parts in order
            /// </summary>
            public List<DtdClassDomainElementPart> DomainElementParts
            {
                get;
                private set;
            }

            /// <summary>
            /// List of the DITA "class" domain/elementname parts in reverse order
            /// </summary>
            public List<DtdClassDomainElementPart> DomainElementPartsInReversedOrder
            {
                get;
                private set;
            }

        }

        /// <summary>
        /// This class can be used to parse and store a DITA "domains" attribute string
        /// </summary>
        private class DtdDomains
        {
            /// <summary>
            /// Initializes an instance of the DtdDomains with the given domainsAttribute
            /// </summary>
            /// <param name="domainsAttribute">DITA "domains" attribute string</param>   
            public DtdDomains(string domainsAttribute)
            {
                DomainsAttribute = domainsAttribute;
                string attribute = domainsAttribute.Trim();

                DomainsParts = new List<DtdDomainsPart>();
                string[] domainParts = attribute.Split(new Char[] { ')' });
                foreach (string part in domainParts)
                {   
                    string trimmedPart = part.Trim();
                    if (trimmedPart.Length > 0)
                    {
                        if (trimmedPart.StartsWith("("))
                        {
                            DomainsParts.Add(new DtdDomainsPart(trimmedPart.Substring(1).Trim()));
                        }
                    }
                }

            }            

            /// <summary>
            /// List of the DITA "domain" parts
            /// </summary>
            public List<DtdDomainsPart> DomainsParts
            {
                get;
                private set;
            }

            /// <summary>
            /// Entire DITA "domains" attribute string
            /// </summary>
            public string DomainsAttribute
            {
                get;
                private set;
            }


        }


        private class DtdDomainsPart
        {
            /// <summary>
            /// Initializes an instance of the DtdDomains with the given domainsAttribute
            /// </summary>
            /// <param name="domainsAttributePart">DITA "domains" attribute part string of the form "topic ui-d"</param>   
            public DtdDomainsPart(string domainsAttributePart)
            {
                string[] parts = domainsAttributePart.Split(new Char[] { ' ' });
                DTDName = parts[0];
                DomainName = parts[1];
            }

            /// <summary>
            /// DTD name part of the domain part
            /// </summary>
            public string DTDName
            {
                get;
                private set;
            }

            /// <summary>
            /// Domain name part of the domains part
            /// </summary>
            public string DomainName
            {
                get;
                private set;
            }
        }

        /// <summary>
        /// This class stores one "domain/elementname" part of a DITA "class" attribute string
        /// </summary>
        private class DtdClassDomainElementPart
        {
            /// <summary>
            /// Initializes an instance of the DtdClassDomainElementPart with the given classAttributePart
            /// </summary>
            /// <param name="classAttributePart">DITA "class" attribute part with a domain/elementname string</param>   
            public DtdClassDomainElementPart(string classAttributePart)
            {
                int pos = classAttributePart.IndexOf("/");
                if (pos > 0)
                {
                    Domain = classAttributePart.Substring(0, pos);
                    ElementName = classAttributePart.Substring(pos+1);
                }
                else
                {
                    throw new InvalidOperationException("classAttributePart '" + classAttributePart + "'does not contain a /, so it is no domain/element part");
                }
            }

            /// <summary>
            /// Domain of the DTD class part
            /// </summary>
            public string Domain
            {
                get;
                private set;
            }
            /// <summary>
            /// ElementName of the DTD class part
            /// </summary>
            public string ElementName
            {
                get;
                private set;
            }
        }


        /// <summary>
        /// Class that holds the necessary info to create the generalized xml file: the dtd into, rootword, encoding and domains
        /// </summary>
        private class XmlGeneralizedConstructionInfo
        {
            /// <summary>
            /// Initializes an instance of the XmlGeneralizedConstructionInfo
            /// </summary>
            public XmlGeneralizedConstructionInfo()
            {
                Encoding = Encoding.Unicode;
            }

            /// <summary>
            /// Name of the rootelement for the generalized Xml
            /// </summary>
            public string RootElement
            {
                get;
                set;
            }
            /// <summary>
            /// PublicId for the generalized Xml
            /// </summary>
            public string DtdPublicId
            {
                get;
                set;
            }
            /// <summary>
            /// SystemId for the generalized Xml
            /// </summary>
            public string DtdSystemId
            {
                get;
                set;
            }

            /// <summary>
            /// Internal DTD definitions for the generalized Xml
            /// </summary>
            public string DtdInternalSubset
            {
                get;
                set;
            }
            /// <summary>
            /// Encoding for the generalized Xml
            /// </summary>
            public Encoding Encoding
            {
                get;
                set;
            }
            /// <summary>
            /// DTD Domains allowed for the generalized Xml
            /// </summary>
            public DtdDomains AllowedDomains
            {
                get;
                set;
            }
        }


        #endregion
    }

}
