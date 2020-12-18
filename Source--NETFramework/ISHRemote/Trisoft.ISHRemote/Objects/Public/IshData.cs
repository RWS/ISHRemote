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
using System.IO;
using System.Xml;

namespace Trisoft.ISHRemote.Objects.Public
{
    /// <summary>
    /// <para type="description">Object holding the file/blob information with Electronic Document Type (EDT) information like file extension</para>
    /// </summary>
    public class IshData
    {
        const string DefaultEDT = "EDTUNDEFINED";

        private readonly string _edt = DefaultEDT;
        private readonly byte[] _byteArray = null;
        private readonly string _fileExtension = "";

        /// <summary>
        /// Creates a new instance of the <see cref="IshData"/> class.
        /// </summary>
        /// <param name="edt">The EDT.</param>
        /// <param name="filePath">The location of the file.</param>
        public IshData(string edt, string filePath)
        {
            _edt = (edt == null) ? DefaultEDT : edt;

            filePath = (filePath == null) ? "" : filePath;
            if (File.Exists(filePath))
            {
                // initialize the byte array
                _byteArray = File.ReadAllBytes(filePath);
                _fileExtension = Path.GetExtension(filePath);
            }
        }

        /// <summary>
        /// Creates a new instance of the <see cref="IshData"/> class.
        /// </summary>
        /// <param name="edt">The EDT.</param>
        /// <param name="fileContent">Byte array with the file content.</param>
        public IshData(string edt, byte[] fileContent)
        {
            _edt = (edt == null) ? DefaultEDT : edt;
            _byteArray = fileContent;
            _fileExtension = "";
        }

        /// <summary>
        /// Creates a new instance of the <see cref="IshData"/> class.
        /// </summary>
        /// <param name="edt">The EDT.</param>
        /// <param name="fileExtension">File extension</param>
        /// <param name="fileContent">Byte array with the file content.</param>
        public IshData(string edt, string fileExtension, byte[] fileContent)
        {
            _edt = (edt == null) ? DefaultEDT : edt;
            _byteArray = fileContent;
            _fileExtension = fileExtension;
        }

        /// <summary>
        /// Initializing by something that looks like <ishdata edt="EDTXML"><![CDATA[PFhNTEZJTEU+UHJvamVjdE1hbmFnZW1lbnQ/PC9YTUxGSUxFPg0K]]></ishdata>
        /// </summary>
        public IshData(XmlElement ishData)
        {
            if (ishData != null)
            {
                _edt = ishData.Attributes["edt"].Value;
                _fileExtension = ishData.Attributes["fileextension"].Value;
                _byteArray = System.Convert.FromBase64String(ishData.InnerText);
            }
        }

        /// <summary>
        /// Gets the EDT.
        /// </summary>
        public string Edt
        {
            get { return _edt; }
        }

        /// <summary>
        /// Gets the fileextension.
        /// </summary>
        public string FileExtension
        {
            get { return _fileExtension; }
        }

        /// <summary>
        /// Gets the blob.
        /// </summary>
        public byte[] ByteArray
        {
            get { return _byteArray; }
        }

        /// <summary>
        /// Gets the blob size.
        /// </summary>
        /// <returns>The number of bytes.</returns>
        public int Size()
        {
            if (_byteArray != null)
            {
                return _byteArray.Length;
            }
            else
            {
                return 0;
            }
        }
    }
}
