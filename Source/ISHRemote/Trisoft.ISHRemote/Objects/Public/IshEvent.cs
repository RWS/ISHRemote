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
    /// <para type="description">The IshEvent is one entry on the EventMonitor detail are, also holding progress top-level information.</para>
    /// </summary>
    public class IshEvent : IshBaseObject
    {
        // Regular IshEvents look like
        //<ishevent ishprogressref="807" ishdetailref="16317">
        //<ishdata edt="EDTXML"><![CDATA[PFhNTEZJTEU+UHJvamVjdE1hbmFnZW1lbnQ/PC9YTUxGSUxFPg0K]]></ishdata>


        private readonly string _ishRef;
        private readonly Dictionary<Enumerations.ReferenceType,string> _eventRef;
        private IshFields _ishFields;
        private IshEventData _ishEventData;

        /// <summary>
        /// IshEvent creation through an ishevent xml element.
        /// Incoming is: <ishevent ishprogressref="807"/> or <ishevent ishprogressref="807" ishdetailref="16317"/>
        /// </summary>
        /// <param name="xmlIshEvent">One ishevent xml.</param>
        public IshEvent(XmlElement xmlIshEvent)
        {
            _eventRef = new Dictionary<Enumerations.ReferenceType, string>();
            StringEnum stringEnum = new StringEnum(typeof(Enumerations.ReferenceType));
            // Loop all reference attributes present in the xml
            foreach (string refType in stringEnum.GetStringValues())
            {
                if (xmlIshEvent.HasAttribute(refType))
                {
                    Enumerations.ReferenceType enumValue = (Enumerations.ReferenceType)StringEnum.Parse(typeof(Enumerations.ReferenceType), refType);
                    _eventRef.Add(enumValue, xmlIshEvent.Attributes[refType].Value);
                }
            }
            _ishFields = new IshFields((XmlElement)xmlIshEvent.SelectSingleNode("ishfields"));
            _ishEventData = new IshEventData((XmlElement)xmlIshEvent.SelectSingleNode("ishdata"));
            _ishRef = _ishFields.GetFieldValue("EVENTID", Enumerations.Level.Progress, Enumerations.ValueType.Value);
        }

        /// <summary>
        /// Gets the IshRef property, better known as the event id of the object.
        /// </summary>
        public string IshRef
        {
            get { return _ishRef; }
        }
        /// <summary>
        /// Gets the EventId property
        /// </summary>
        public string EventId
        { 
            get { return _ishRef; } 
        }
        /// <summary>
        /// Gets the type of the event
        /// </summary>
        public string EventType
        {
            get { return _ishFields.GetFieldValue("EVENTTYPE", Enumerations.Level.Progress, Enumerations.ValueType.Value); }
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
        /// Gets and sets the IshEventData property.
        /// The IshData property is used to contain the blob (<see cref="IshData"/>).
        /// </summary>
        public IshEventData IshEventData
        {
            get { return _ishEventData; }
            set { _ishEventData = value; }
        }

        /// <summary>
        /// Stores the variations of progressref, ishlogicalref, ishuserref, ishoutputformatref,... If there are more references, like log/ver/Lng then they are available in the dictionary.
        /// </summary>
        public Dictionary<Enumerations.ReferenceType, string> ObjectRef
        {
            get { return _eventRef; }
        }
    }
}
