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
    /// <para type="description">Stores field name and level information via IshField. Adds a value type and value to allow set and get</para>
    /// </summary>
    public class IshMetadataField : IshField 
    {
        private string _value;


        /// <summary>
        /// Constructs a Set IshField
        /// </summary>
        public IshMetadataField(string fieldName, Enumerations.Level fieldLevel, string value)
            : base(fieldName, fieldLevel, Enumerations.ValueType.Value)
        {
            _value = (value == null) ? "" : value;
        }

        /// <summary>
        /// Constructs a Get/Requested IshField
        /// </summary>
        public IshMetadataField(string fieldName, Enumerations.Level fieldLevel, Enumerations.ValueType valueType, string value)
            : base(fieldName, fieldLevel,valueType)
        {
            _value = value;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        public IshMetadataField(IshMetadataField ishMetadataField)
            : base(ishMetadataField._fieldName, ishMetadataField._fieldLevel, ishMetadataField._valueType)
        {
            _value = ishMetadataField._value;
        }

        /// <summary>
        /// Constructs a Value Field with whatever information available
        /// </summary>
        public IshMetadataField(XmlElement xmlIshField)

        { 
            _fieldName = xmlIshField.Attributes["name"].Value;
            _fieldLevel = (Enumerations.Level)StringEnum.Parse(typeof(Enumerations.Level),xmlIshField.Attributes["level"].Value);
            _value = xmlIshField.InnerText;
            _valueType = (xmlIshField.Attributes["ishvaluetype"] == null) ? Enumerations.ValueType.Value : (Enumerations.ValueType)StringEnum.Parse(typeof(Enumerations.ValueType),xmlIshField.Attributes["ishvaluetype"].Value);
        }

        public string Value
        {
            get { return _value; }
        }

        public override void Join(IshField ishField, Enumerations.ValueAction valueAction)
        {
            if (ishField is IshMetadataField)
            {
                IshMetadataField ishMetadataField = (IshMetadataField)ishField;
                switch (valueAction)
                { 
                    case Enumerations.ValueAction.Append:
                        _value = _value + ishMetadataField.Value;
                        break;
                    case Enumerations.ValueAction.Prepend:
                        _value = ishMetadataField.Value + _value;
                        break;
                    case Enumerations.ValueAction.Overwrite:
                        _value = ishMetadataField.Value;
                        break;
                }
            }
            else
            { 
                //do nothing
            }
        }

        /// <summary>
        /// Generates a new ishfield object where the field only has requested field information (e.g. no Value, but ValueType if available)
        /// </summary>
        public override IshField ToRequestedMetadataField()
        {
            return new IshRequestedMetadataField(Name, Level, _valueType);
        }

        /// <summary>
        /// Generates a new ishfield object where the field becomes a filter field (e.g. with Value, ValueType, operator if available)
        /// </summary>
        public override IshField ToMetadataFilterField()
        {
            return new IshMetadataFilterField(_fieldName, _fieldLevel, Enumerations.FilterOperator.Equal, _value, _valueType);
        }

        /// <summary>
        /// Generates a new ishfield object where the field becomes a metadata field (e.g. with Value, ValueType)
        /// </summary>
        public override IshField ToMetadataField()
        {
            return new IshMetadataField(this);
        }

        /// <summary>
        /// Convert the IshField object to xml.
        /// </summary>
        /// <param name="xmlWriter">The <see cref="XmlWriter"/>.</param>
        public override void GetXml(ref XmlWriter xmlWriter)
        {
            xmlWriter.WriteStartElement("ishfield");
            xmlWriter.WriteAttributeString("name", Name);
            xmlWriter.WriteAttributeString("level", StringEnum.GetStringValue(Level));
            // write value type explicitly, except when it is just 'value'
            if (_valueType != Enumerations.ValueType.Value)  // TODO [Could] IshMetadataField.GetXml(...) I think also ValueType.All should be added here otherwise API will get @valuetype="" as input I think
            { 
                xmlWriter.WriteAttributeString("ishvaluetype", StringEnum.GetStringValue(_valueType));
            }
            // if we have a value write it
            if (_value.Length > 0)
            {
                xmlWriter.WriteString(_value);
            }
            xmlWriter.WriteEndElement();
        }

        /// <summary>
        /// Debugging implementation
        /// </summary>
        /// <returns>string</returns>
        public override string ToString()
        {
            return $"Set-IshMetadataField -Level {StringEnum.GetStringValue(Level)} -Name {Name} -ValueType {StringEnum.GetStringValue(_valueType)} -Value \"{_value}\"";
            //return $"<ishfield name='{Name}' level='{StringEnum.GetStringValue(Level)}' ishvaluetype='{StringEnum.GetStringValue(_valueType)}'>{_value}</ishfield>";
        }
    }
}
