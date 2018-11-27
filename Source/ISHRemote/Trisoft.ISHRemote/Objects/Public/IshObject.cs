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
using Trisoft.ISHRemote.HelperClasses;

namespace Trisoft.ISHRemote.Objects.Public
{
    /// <summary>
    /// <para type="description">Generic pipeline object for the API holding various references (logical id, card id,...), object type, ishfields (metadata ) and optionally ishdata (file/blob)</para>
    /// </summary>
    public class IshObject : IshBaseObject
    {
        // Regular IshObjects look like
        //<ishobject ishref="GUID-AF99C63E-7887-4485-A7ED-ACED50781F70" ishtype="ISHLibrary" ishlogicalref="16333">
        //<ishobject ishref="VUSERGROUPDEFAULTDEPARTMENT" ishtype="ISHUserGroup" ishuserref="30">
        //<ishobject ishref="GUID-2A69335D-F025-4963-A142-5E49988C7C0C" ishtype="ISHOutputFormat" ishoutputformatref="15405">
        //<ishobject ishref="VUSERROLEAUTHOR" ishtype="ISHUserRole" ishuserroleref="359934">
        //<ishobject ishref="EDTPDF" ishtype="ISHEDT" ishedtref="11461">
        // Search results look like
        //<ishobject ishsequence="1" ishref="GUID-B44976AE-73DE-41B6-8126-6F277A1B5F83" ishlogicalref="43583" ishtype="ISHModule" ishversionref="43587" ishlngref="277911" /> 
        //
        //<ishdata edt="EDTXML"><![CDATA[PFhNTEZJTEU+UHJvamVjdE1hbmFnZW1lbnQ/PC9YTUxGSUxFPg0K]]></ishdata>


        private string _ishRef;
        private Enumerations.ISHType _ishType; 
        private Dictionary<Enumerations.ReferenceType,string> _objectRef;
        private IshFields _ishFields;
        private IshData _ishData;

        /// <summary>
        /// IshObject creation through an IshField can (I think?!!) be done if you explicitly list all fields.
        /// </summary>
        /// <remarks>IshObject is typically only returned from the repository through an xml container.</remarks>
        /// <param name="ishType">Type indication, like ISHOutputFormat, ISHLibrary,...</param>
        /// <param name="ishRef">An element name, not that for some types this will be overwritten with a generated one</param>
        /// <param name="ishFields">The functions down the line will extract the required ishfields for real object creation.</param>
        public IshObject(Enumerations.ISHType ishType, string ishRef, IshFields ishFields)
        {
            _ishType = ishType;
            _ishRef = (ishRef == null) ? "" : ishRef;
            _objectRef = new Dictionary<Enumerations.ReferenceType, string>(); 
            _ishFields = ishFields;
            _ishData = null;
        }

        /// <summary>
        /// IshObject creation through an ishobject xml element.
        /// Incoming is: <ishobject ishref="D1E1030" ishtype="ISHModule" ishlogicalref="26288" ishversionref="26291" ishlngref="39490" />
        /// </summary>
        /// <param name="xmlIshObject">One ishobject xml.</param>
        public IshObject(XmlElement xmlIshObject)
        {
            _ishType = (Enumerations.ISHType)Enum.Parse(typeof(Enumerations.ISHType), xmlIshObject.Attributes["ishtype"].Value);
            _ishRef = xmlIshObject.Attributes["ishref"].Value;
            _objectRef = new Dictionary<Enumerations.ReferenceType, string>();
            StringEnum stringEnum = new StringEnum(typeof(Enumerations.ReferenceType));
            // Loop all reference attributes present in the xml
            foreach (string refType in stringEnum.GetStringValues())
            {             
                if (xmlIshObject.HasAttribute(refType))
                {
                    Enumerations.ReferenceType enumValue = (Enumerations.ReferenceType)StringEnum.Parse(typeof(Enumerations.ReferenceType), refType);
                    _objectRef.Add(enumValue,xmlIshObject.Attributes[refType].Value);
                }
            }
            _ishFields = new IshFields((XmlElement)xmlIshObject.SelectSingleNode("ishfields"));
            _ishData = new IshData((XmlElement)xmlIshObject.SelectSingleNode("ishdata"));
        }

        /// <summary>
        /// Gets the IshRef property, better known as the logical id of the object.
        /// </summary>
        public string IshRef
        {
            get { return _ishRef; }
        }

        /// <summary>
        /// Gets the IshType property.
        /// Possible values are all card types (ISHUserGroup, ISHEDT, ISHOutputFormat...)
        /// </summary>
        public Enumerations.ISHType IshType
        {
            get { return _ishType; }
        }

        /// <summary>
        /// Get a pipeline friendly base/specialized IshField objects as array
        /// </summary>
        public IshField[] IshField
        {
            get { return _ishFields.Fields(); }
        }

        /// <summary>
        /// Gets and sets the IshFields property.
        /// The IshFields property is a collection of <see cref="IshFields"/>.
        /// </summary>
        internal override IshFields IshFields
        {
            get { return _ishFields; }
            set { _ishFields = value; }
        }

        /// <summary>
        /// Gets and sets the IshData property.
        /// The IshData property is used to contain the blob (<see cref="IshData"/>).
        /// </summary>
        public IshData IshData
        {
            get { return _ishData; }
            set { _ishData = value; }
        }

        /// <summary>
        /// Stores the variations of ishlogicalref, ishuserref, ishoutputformatref,... If there are more references, like log/ver/Lng then they are available in the dictionary.
        /// </summary>
        public Dictionary<Enumerations.ReferenceType, string> ObjectRef
        {
            get { return _objectRef; }
        }
    }
}
