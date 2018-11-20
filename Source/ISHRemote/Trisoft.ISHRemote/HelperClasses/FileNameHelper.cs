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
using Trisoft.ISHRemote.Objects;
using Trisoft.ISHRemote.Objects.Public;

namespace Trisoft.ISHRemote.HelperClasses
{
    /// <summary>
    /// Helper class that contains methods to resolve file names.
    /// </summary>
    internal static class FileNameHelper
    {
        private const int MaximumFolderPathSize = 248;
        private const int MaximumFilePathSize = 260;

        private const int MaximumFileNameSize = 120;

        /// <summary>
        /// Outputs an filename based on an ishobject.
        /// </summary>
        /// <param name="path">The path where the file needs to be stored.</param>
        /// <param name="ishObject">The <see cref="IshObject"/>.</param>
        /// <param name="extension">The file extension.</param>
        /// <returns>
        /// A string with following format #Title#=#LogicalId#=#Version#=(#Language#=)(#Resolution#).#Extension#
        /// Values between brackets are optional.
        /// </returns>
        internal static string GetDefaultObjectFileName(string path, IshObject ishObject, string extension)
        {
            //Prereqs
            if (ishObject == null)
                throw new ArgumentNullException("ishObject");
            if (path == null)
                path = "";
            if (extension == null)
                extension = "";
            string fileName = "";
            //Logical id
            if (ishObject.IshRef != null)
            {
                fileName += Encode(ishObject.IshRef) + "=";
            }
            else
            {
                throw new ArgumentException("There is no logicalid available.");
            }
            //Version
            IshField versionField = ishObject.IshFields.RetrieveFirst("VERSION", Enumerations.Level.Version,Enumerations.ValueType.Value);
            if (versionField!=null)
            {
                fileName += Encode(((IshMetadataField)versionField.ToMetadataField()).Value) + "=";
            }
            else
            {
                throw new ArgumentException("There is no version available. Value for field VERSION is missing.");
            }
            //Language
            IshField languageField = ishObject.IshFields.RetrieveFirst("DOC-LANGUAGE", Enumerations.Level.Lng,Enumerations.ValueType.Value);
            if (languageField!=null)
            {
                fileName += Encode(((IshMetadataField)languageField.ToMetadataField()).Value) + "=";
            }
            //Resolution
            IshField resolutionField = ishObject.IshFields.RetrieveFirst("FRESOLUTION", Enumerations.Level.Lng,Enumerations.ValueType.Value);
            if (resolutionField!=null)
            {
                fileName += Encode(((IshMetadataField)resolutionField.ToMetadataField()).Value);
            }
            //Escape the current filename
            fileName = EscapeFileName(fileName);
            //Extension
            if ((!string.IsNullOrEmpty(extension)))
            {
                fileName += "." + extension.Trim(' ', '.');
            }
            //Title
            string title = string.Empty;
            IshField titleField = ishObject.IshFields.RetrieveFirst("FTITLE", Enumerations.Level.Logical,Enumerations.ValueType.Value);
            if (titleField!=null)
            {
                title = EscapeFileName(Encode(((IshMetadataField)titleField.ToMetadataField()).Value));
            }
            //Calculate the maximum length of the title
            int maxLengthTitle = 0;
            if (System.IO.Path.IsPathRooted(path))
            {
                maxLengthTitle = MaximumFilePathSize - (path.Length + fileName.Length + 2);
            }
            else if (path.Length > 0)
            {
                maxLengthTitle = MaximumFileNameSize - (path.Length + fileName.Length + 2);
            }
            else
            {
                maxLengthTitle = MaximumFileNameSize - (fileName.Length + 1);
            }
            //Strip title
            if (maxLengthTitle > 0)
            {
                if (maxLengthTitle < title.Length)
                {
                    fileName = title.Substring(0, maxLengthTitle) + "=" + fileName;
                }
                else
                {
                    fileName = title + "=" + fileName;
                }
            }
            //Combine filename + path
            if ((path.Length > 0))
            {
                return System.IO.Path.Combine(path, fileName);
            }

            return fileName;
        }

        /// <summary>
        /// Outputs a filename based on an ishobject (publicationoutput variant).
        /// </summary>
        /// <param name="path">The path where the file needs to be stored.</param>
        /// <param name="ishObject">The <see cref="IshObject"/>.</param>
        /// <param name="extension">The file extension.</param>
        /// <returns>
        /// A string with following format #Title#=#LogicalId#=#Version#=#OutputFormat#=#LanguageCombination#.#Extension#
        /// </returns>
        internal static string GetDefaultPublicationOutputFileName(string path, IshObject ishObject, string extension)
        {
            //Prereqs
            if (ishObject == null)
                throw new ArgumentNullException("ishObject");           
            if (path == null)
                path = "";
            if (extension == null)
                extension = "";
            string fileName = "";
            //Logical id
            if (ishObject.IshRef != null)
            {
                fileName += Encode(ishObject.IshRef) + "=";
            }
            else
            {
                throw new ArgumentException("There is no logicalid available.");
            }
            //Version
            IshField versionField = ishObject.IshFields.RetrieveFirst("VERSION", Enumerations.Level.Version,Enumerations.ValueType.Value);
            if (versionField != null)
            {
                fileName += Encode(((IshMetadataField)versionField.ToMetadataField()).Value) + "=";
            }
            else
            {
                throw new ArgumentException("There is no version available. Value for field VERSION is missing.");
            }
            //OutputFormat
            IshField outputFormatField = ishObject.IshFields.RetrieveFirst("FISHOUTPUTFORMATREF", Enumerations.Level.Lng,Enumerations.ValueType.Value);
            if (outputFormatField != null)
            {
                fileName += Encode(((IshMetadataField)outputFormatField.ToMetadataField()).Value) + "="; 
            }
            else
            {
                throw new ArgumentException("There is no outputformat available. Value for field FISHOUTPUTFORMATREF is missing.");
            }
            //LanguageCombination
            IshField lngCombinationField = ishObject.IshFields.RetrieveFirst("FISHPUBLNGCOMBINATION", Enumerations.Level.Lng,Enumerations.ValueType.Value);
            if (lngCombinationField != null)
            {
                fileName += Encode(((IshMetadataField)lngCombinationField.ToMetadataField()).Value);
            }
            else
            {
                throw new ArgumentException("There is no languagecombination available. Value for field FISHPUBLNGCOMBINATION is missing.");
            }
            //Escape the current filename
            fileName = EscapeFileName(fileName);
            //Extension
            if ((!string.IsNullOrEmpty(extension)))
            {
                fileName += "." + extension.Trim(' ', '.');
            }
            //Title
            string title = string.Empty;
            IshField titleField = ishObject.IshFields.RetrieveFirst("FTITLE", Enumerations.Level.Logical,Enumerations.ValueType.Value);
            if (titleField != null)
            {
                title = EscapeFileName(Encode(((IshMetadataField)titleField.ToMetadataField()).Value));
            }
            //Calculate the maximum length of the title
            int maxLengthTitle = 0;
            if (System.IO.Path.IsPathRooted(path))
            {
                maxLengthTitle = MaximumFilePathSize - (path.Length + fileName.Length + 2);
            }
            else if (path.Length > 0)
            {
                maxLengthTitle = MaximumFileNameSize - (path.Length + fileName.Length + 2);
            }
            else
            {
                maxLengthTitle = MaximumFileNameSize - (fileName.Length + 1);
            }
            //Strip title
            if (maxLengthTitle > 0)
            {
                if (maxLengthTitle < title.Length)
                {
                    fileName = title.Substring(0, maxLengthTitle) + "=" + fileName;
                }
                else
                {
                    fileName = title + "=" + fileName;
                }
            }
            //Combine filename + path
            if ((path.Length > 0))
            {
                return System.IO.Path.Combine(path, fileName);
            }
            return fileName;
        }

        /// <summary>
        /// Function that cleans up invalid characters from of a file name.
        /// </summary>
        /// <param name="fileName">The file name.</param>
        /// <returns>A valid file name (without invalid characters).</returns>
        static internal string EscapeFileName(string fileName)
        {
            // Clean up invalid characters
            foreach (char c in System.IO.Path.GetInvalidFileNameChars())
            {
                fileName = fileName.Replace(c.ToString(), string.Empty);
            }

            return fileName;
        }

        /// <summary>
        /// Function that cleans up invalid characters from of a file path.
        /// </summary>
        /// <param name="path">The file path.</param>
        /// <returns>A valid file path (without invalid characters).</returns>
        static internal string EscapePath(string path)
        {
            // Clean up invalid characters
            foreach (char c in System.IO.Path.GetInvalidPathChars())
            {
                path = path.Replace(c.ToString(), string.Empty);
            }

            return path;
        }

        /// <summary>
        /// Encode the "=" char in a string.
        /// </summary>
        /// <param name="s">String to encode.</param>
        /// <returns>A string where all "=" chars are replaced by "%3d".</returns>
        static internal string Encode(string s)
        {
            return s.Replace('='.ToString(), "%3d");
        }

        /// <summary>
        /// Decodes the "=" char in a string.
        /// </summary>
        /// <param name="s">String to decode.</param>
        /// <returns>A string where all "%3d" chars are replaced by "=".</returns>
        static internal string Decode(string s)
        {
            return s.Replace("%3d".ToString(), '='.ToString());
        }
    }
}
