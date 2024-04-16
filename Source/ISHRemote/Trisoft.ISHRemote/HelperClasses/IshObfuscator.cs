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
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;

namespace Trisoft.ISHRemote.HelperClasses
{
    /// <summary>
    /// This class can be used to obfuscate a file.
    /// </summary>
    public static class IshObfuscator
    {
        #region Private fields

        /// <summary>
        /// To replace words up to 20 characters with a fixed replacement word
        /// </summary>
        private static readonly string[] _shortWordSubstitutions = { "", "a", "be", "the", "easy", "would", "summer", "healthy", "zucchini", "breakfast", "chimpanzee",
            "alternative", "professional", "extraordinary", "representative", "confidentiality", "extraterrestrial", "telecommunication",
            "bioinstrumentation", "psychophysiological", "internationalization" };
        /// <summary>
        /// To replace words > 20 chars .. a part of this very long word can be taken
        /// </summary>
        private static readonly string _longWordSubstitution = string.Concat(Enumerable.Repeat("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz", 100000));

        #endregion

        /// <summary>
        /// Obfuscates the given xml file. 
        /// When the obfuscation succeeds a file is created at the location given in outputFileLocation
        /// </summary>
        /// <param name="inputFileLocation">The location of the input file</param>
        /// <param name="outputFileLocation">The location for the output file</param>
        /// <param name="attributesToObfuscate">Attributes to obfuscate</param>
        public static void ObfuscateXml(string inputFileLocation, string outputFileLocation, List<string> attributesToObfuscate)
        {
            Encoding outputEncoding = Encoding.Unicode;
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
                                        if (reader.Name != "ish")
                                        {
                                            writer.WriteProcessingInstruction(reader.Name, ObfuscateWords(reader.Value));
                                            reader.Read();
                                        }
                                        else
                                        {
                                            writer.WriteNode(reader, false);
                                        }
                                        break;
                                    case XmlNodeType.Element:
                                        bool isEmptyElement = reader.IsEmptyElement;
                                        string elementValue = reader.Value;
                                        writer.WriteStartElement(reader.Prefix, reader.LocalName, reader.NamespaceURI);
                                        for (int attInd = 0; attInd < reader.AttributeCount; attInd++)
                                        {
                                            reader.MoveToAttribute(attInd);
                                            var attributeValue = reader.Value;
                                            if (attributesToObfuscate != null && attributesToObfuscate.Contains(reader.Name))
                                            {
                                                attributeValue = ObfuscateWords(reader.Value);
                                            }
                                            writer.WriteAttributeString(reader.Prefix, reader.LocalName, reader.NamespaceURI, attributeValue);
                                        }
                                        if (!String.IsNullOrEmpty(elementValue))
                                        {
                                            writer.WriteString(ObfuscateWords(elementValue));
                                        }
                                        if (isEmptyElement)
                                        {
                                            writer.WriteEndElement();
                                        }
                                        reader.Read();
                                        break;
                                    case XmlNodeType.Comment:
                                        writer.WriteComment(ObfuscateWords(reader.Value));
                                        reader.Read();
                                        break;
                                    case XmlNodeType.Text:
                                        writer.WriteString(ObfuscateWords(reader.Value));
                                        reader.Read();
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

        /// <summary>
        /// Writes an xml declaration with the correct encoding
        /// </summary>
        /// <param name="encoding">Encoding</param>
        /// <param name="xmlReader">XmlReader positioned on a XmlDeclaration</param>
        /// <param name="xmlWriter">XmlWriter</param>
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
                    if (encoding == Encoding.Unicode || encoding == Encoding.BigEndianUnicode)
                    {
                        value = "UTF-16";
                    }
                    else if (encoding == Encoding.UTF8)
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

        /// <summary>
        /// Substitutes the words in a phrase or paragraph with other (hardcoded) words
        /// </summary>
        /// <param name="text">Text to obfuscate</param>
        /// <returns>
        /// Obfuscated text
        /// </returns>
        private static string ObfuscateWords(string text)
        {
            string word = String.Empty;
            StringBuilder result = new StringBuilder(text.Length);
            for (var i = 0; i < text.Length; i++)
            {
                var character = text[i];
                if (Char.IsWhiteSpace(character) || Char.IsPunctuation(character) || Char.IsSeparator(character) || Char.IsNumber(character) || character == '=')
                {
                    result.Append(ObfuscateWord(word));
                    result.Append(character);
                    word = String.Empty;
                }
                else
                {
                    word += character;
                }
            }
            if (word.Length > 0)
            {
                result.Append(ObfuscateWord(word));
            }
            return result.ToString();
        }

        /// <summary>
        /// Substitutes one word by another hardcoded word
        /// </summary>
        /// <param name="word">Word to obfuscate</param>
        /// <returns>
        /// Obfuscated word
        /// </returns>
        private static string ObfuscateWord(string word)
        {
            if (word == String.Empty)
            {
                return String.Empty;
            }
            string replace;
            if (word.Length > 20)
            {
                replace = _longWordSubstitution.Substring(0, word.Length);
            }
            else
            {
                replace = _shortWordSubstitutions[word.Length];
            }
            if (char.IsUpper(word[0]))
            {
                return char.ToUpper(replace[0]) + replace.Substring(1);
            }
            else
            {
                return replace;
            }
        }

// Overall build should treat warnings as errors, hence:
// Disabling warning regarding 'This call site is reachable on Windows all versions.' 
#pragma warning disable CA1416

        /// <summary>
        /// Obfuscates the given image file. 
        /// For that, a new image is created with the same format, width and height and a yellow background (and the filename in text in the image if it is wide enough to put it there).
        /// When the obfuscation succeeds a file is created at the location given in outputFileLocation
        /// </summary>
        /// <param name="inputFileLocation">The location of the input file</param>
        /// <param name="outputFileLocation">The location for the output file</param>
        public static void ObfuscateImage(string inputFileLocation, string outputFileLocation)
        {
#if NET6_0_OR_GREATER
            if (!OperatingSystem.IsWindows())
            {
                throw new PlatformNotSupportedException($"Obfuscate Image is only supported on Windows platform (through NET6+ extension), obfuscating image inputFile[{inputFileLocation}] is skipped. [OS:{Environment.OSVersion}]");
            }
#endif
            int width;
            int height;
            ImageFormat format;
            PixelFormat pixelFormat;
            var fileInfo = new FileInfo(inputFileLocation);
            using (Stream stream = File.OpenRead(inputFileLocation))
            {
                using (Image sourceImage = Image.FromStream(stream, false, false))
                {
                    width = sourceImage.Width;
                    height = sourceImage.Height;
                    format = sourceImage.RawFormat;
                    pixelFormat = sourceImage.PixelFormat;
                }
            }
            // Creating the image with the pixelformat gives an error, so not passing it to CreateImageWithText, which means color depth will be different
            var newImage = CreateImageWithText(fileInfo.Name, width, height);
            newImage.Save(outputFileLocation, format);
        }

        /// <summary>
        /// Creates an image with the given width and height and having the given text 
        /// </summary>
        /// <param name="text">Text to include in the image</param>
        /// <param name="width">Image width</param>
        /// <param name="height">Image height</param>
        /// <returns></returns>
        private static Image CreateImageWithText(String text, int width, int height)
        {
#if NET6_0_OR_GREATER
            if (!OperatingSystem.IsWindows())
            {
                throw new PlatformNotSupportedException($"Obfuscate Image With Text is only supported on Windows platform (through NET6+ extension). [OS:{Environment.OSVersion}]");
            }
#endif
            Font font = new Font("Arial", 11, FontStyle.Regular);
            Color textColor = Color.Red;
            Color backColor = Color.Yellow;

            //first, create a dummy bitmap just to get a graphics object
            Image img = new Bitmap(1, 1);
            Graphics drawing = Graphics.FromImage(img);

            //measure the string to see how big the image needs to be
            SizeF textSize = drawing.MeasureString(text, font);

            //free up the dummy image and old graphics object
            img.Dispose();
            drawing.Dispose();

            //create a new image of the right size
            img = new Bitmap(width, height);

            drawing = Graphics.FromImage(img);

            //paint the background
            drawing.Clear(backColor);

            if (textSize.Width < width && textSize.Height < height)
            {
                //create a brush for the text
                Brush textBrush = new SolidBrush(textColor);
                drawing.DrawString(text, font, textBrush, 0, 0);
                textBrush.Dispose();
            }

            drawing.Save();
            drawing.Dispose();

            return img;
        }

// Restoring warning regarding 'This call site is reachable on Windows all versions.' 
#pragma warning restore CA1416

    }
}

