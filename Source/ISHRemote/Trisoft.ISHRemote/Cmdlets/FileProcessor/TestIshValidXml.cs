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


/*
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.IO;
using System.Xml.Schema;


namespace ConsoleApp2
{
    public class XmlFileHandler
    {
        const string _catalogUri = @"Y:\InfoShare\WebORA12\Author\ASP\DocTypes\catalog.xml";
        static XmlResolverUsingCatalog _xmlSpecializedXmlResolver = new XmlResolverUsingCatalog(_catalogUri);
        static XmlReaderSettings readerSettings = InitializeXmlReaderSettings(_xmlSpecializedXmlResolver, ValidationType.DTD);

        public void PerformValidate(string fileName)
        {
            Validate(fileName, readerSettings);
        }

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

        public void Utf8Encoder(string inputFileLocation, string outputFileLocation)
        {
            Encoding outputEncoding = new UTF8Encoding(false);
            using (var inputStream = new FileStream(inputFileLocation, FileMode.Open, FileAccess.Read, FileShare.None))
            {
                MemoryStream resultStream = new MemoryStream((int)inputStream.Length);
                // As the XmlTextReader closes the input stream, we use a wrapper class so the original stream does not get closed
                using (var reader = new XmlTextReader(inputStream))
                {
                    reader.Namespaces = false;
                    reader.WhitespaceHandling = WhitespaceHandling.All;
                    reader.EntityHandling = EntityHandling.ExpandCharEntities;
                    reader.Normalization = false;
                    reader.DtdProcessing = DtdProcessing.Parse;
                    reader.XmlResolver = null;
                    using (var resultStreamNonClosing = new NonClosingStreamWrapper(resultStream))
                    {
                        using (var writer = new XmlTextWriter(resultStreamNonClosing, outputEncoding))
                        {
                            writer.Namespaces = false;
                            reader.Read();
                            while (!reader.EOF)
                            {
                                switch (reader.NodeType)
                                {
                                    case XmlNodeType.XmlDeclaration:
                                        WriteXmlDeclaration(outputEncoding, reader, writer);
                                        reader.Read();
                                        break;
                                    case XmlNodeType.DocumentType:
                                        writer.WriteNode(reader, false);
                                        break;
                                    case XmlNodeType.ProcessingInstruction:
                                        writer.WriteNode(reader, false);
                                        break;
                                    case XmlNodeType.Element:
                                        writer.WriteNode(reader, false);
                                        break;
                                    case XmlNodeType.Comment:
                                        writer.WriteNode(reader, false);
                                        break;
                                    case XmlNodeType.Text:
                                        writer.WriteNode(reader, false);
                                        break;
                                    default:
                                        writer.WriteNode(reader, false);
                                        break;
                                }
                            }
                        }
                        using (var outputStream = new FileStream(outputFileLocation, FileMode.Create, FileAccess.Write, FileShare.None))
                        {
                            resultStream.Position = 0;
                            resultStream.CopyTo(outputStream);
                        }
                    }

                }
            }
        }


        private static Encoding _utf8encodingnobom = new UTF8Encoding(false);

        private static void WriteXmlDeclaration(Encoding encoding, XmlReader xmlReader, XmlWriter xmlWriter)
        {
            string[] attributes = xmlReader.Value.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < attributes.Length; i++)
            {
                string attribute = attributes[i];
                if (attribute.ToLower().StartsWith("encoding"))
                {
                    string preFix = attribute.Substring(0, 10);
                    string value;
                    if (encoding.Equals(Encoding.Unicode) || encoding.Equals(Encoding.BigEndianUnicode))
                    {
                        value = "UTF-16";
                    }
                    else if (encoding.Equals(Encoding.UTF8) || encoding.Equals(_utf8encodingnobom))
                    {
                        value = "UTF-8";
                    }
                    else
                    {
                        throw new NotSupportedException("Encoding '" + encoding.EncodingName + "' is not supported. Only UTF-16 and UTF-8 encodings are supported.");
                    }
                    string postFix = attribute.Substring(attribute.Length - 1);
                    attributes[i] = preFix + value + postFix;
                }
            }

            xmlWriter.WriteProcessingInstruction(xmlReader.Name, String.Join(" ", attributes));
        }

        private static XmlReaderSettings InitializeXmlReaderSettings(XmlResolverUsingCatalog xmlResolver, ValidationType validationType)
        {
            // When ProhibitDTD = true, the following error occurs:
            // "For security reasons DTD is prohibited in this XML document. To enable DTD processing set the ProhibitDtd property on XmlReaderSettings to false and pass the settings into XmlReader.Create method."
            XmlReaderSettings xmlReaderSettings = new XmlReaderSettings();
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
        /// Handler method for validation errors. Throws an exception when an error is encountered
        /// </summary>
        /// <param name="sender">Sender of the validation event</param>
        /// <param name="args">Validation error arguments</param>
        private static void ValidationHandler(object sender, System.Xml.Schema.ValidationEventArgs args)
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

    }
}
*/