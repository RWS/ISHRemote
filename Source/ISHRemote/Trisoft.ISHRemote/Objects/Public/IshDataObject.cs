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
using Trisoft.ISHRemote.HelperClasses;

namespace Trisoft.ISHRemote.Objects.Public
{

    /// <summary>
    /// <para type="description">Groups all file functionality required by Trisoft like edt, attributes, file size, mime type, Save, ToByteArray, init by ishdataobject xml element,...</para>
    /// </summary>
    public class IshDataObject 
    {
        const string DefaultEDT = "EDTUNDEFINED";

        private string _edt = DefaultEDT;
        private string _fileExtension = "";
        private string _ed = "";
        private string _ishDataRef = "";
        private int _size = 0;
        private string _mimeType = "";
        private Dictionary<Enumerations.ReferenceType, string> _objectRef;

        /// <summary>
        /// Initializing by something that looks like
        /// <![CDATA[ 
        /// <?xml version="1.0" encoding="utf-16"?>
        /// <ishdataobjects>
        ///   <ishdataobject ishlngref="1826972" ishdataref="1826973"
        ///   ed="GUID-7EE439B0-EE28-463A-A409-7AB83DE2E92D" edt="EDTCHM"
        ///   size="35802" mimetype="application/mshelp" fileextension="chm" />
        /// </ishdataobjects>
        /// ]]>
        /// </summary>
        public IshDataObject(XmlElement ishData)
        {
            if (ishData != null)
            {
                _edt = ishData.Attributes["edt"].Value;
                _ed = ishData.Attributes["ed"].Value;
                _mimeType = ishData.Attributes["mimetype"].Value;
                _fileExtension = ishData.Attributes["fileextension"].Value;
                _ishDataRef = ishData.Attributes["ishdataref"].Value;
                _objectRef = new Dictionary<Enumerations.ReferenceType, string>();
                StringEnum stringEnum = new StringEnum(typeof(Enumerations.ReferenceType));
                // Loop all reference attributes present in the xml
                foreach (string refType in stringEnum.GetStringValues())
                {
                    if (ishData.HasAttribute(refType))
                    {
                        Enumerations.ReferenceType enumValue = (Enumerations.ReferenceType)StringEnum.Parse(typeof(Enumerations.ReferenceType), refType);
                        _objectRef.Add(enumValue, ishData.Attributes[refType].Value);
                    }
                }
                int.TryParse(ishData.Attributes["size"].Value, out _size);
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
        /// Gets the identifier of the EDT.
        /// </summary>
        public string Ed
        {
            get { return _ed; }
        }     
     
        /// <summary>
        /// Gets the data reference id.
        /// </summary>
        public string IshDataRef
        {
            get { return _ishDataRef; }
        }

        /// <summary>
        /// Gets the size of the file (bytes).
        /// </summary>
        public int Size
        {
            get { return _size; }
        }

        /// <summary>
        /// Gets the mimetype of the file.
        /// </summary>
        public string MimeType
        {
            get { return _mimeType; }
        }

        /// <summary>
        /// Gets the extension of the file.
        /// </summary>
        public string FileExtension
        {
            get { return _fileExtension; }
        }

        /// <summary>
        /// Stores the variations of ishlngref, ... 
        /// </summary>
        public Dictionary<Enumerations.ReferenceType, string> ObjectRef
        {
            get { return _objectRef; }
        }

 

    }
}
