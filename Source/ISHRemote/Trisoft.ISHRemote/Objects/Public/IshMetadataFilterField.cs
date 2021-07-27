/*
* Copyright © 2014 All Rights Reserved by the RWS Group for and on behalf of its affiliates and subsidiaries.
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
    /// <para type="description">Stores field name and level information via IshField. Adds a comparison/filter operator and value to allow filtering</para>
    /// </summary>
    public class IshMetadataFilterField : IshField
    {
        private string _value;
        private Enumerations.FilterOperator _filterOperator;
 
        /// <summary>
        /// Constructs a Set IshField
        /// </summary>
        public IshMetadataFilterField(string fieldName, Enumerations.Level fieldLevel, Enumerations.FilterOperator filterOperator, string value, Enumerations.ValueType valueType)
            : base(fieldName, fieldLevel, valueType)
        {
            _filterOperator = filterOperator;
            _value = (value == null) ? "" : value;
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        public IshMetadataFilterField(IshMetadataFilterField ishMetadataFilterField)
            : base(ishMetadataFilterField._fieldName, ishMetadataFilterField._fieldLevel, ishMetadataFilterField._valueType)
        {
            _filterOperator = ishMetadataFilterField._filterOperator;
            _value = ishMetadataFilterField._value;
        }

        public string Value
        {
            get { return _value; }
        }

        public Enumerations.FilterOperator FilterOperator
        {
            get { return _filterOperator; }
        }

        public override void Join(IshField ishField, Enumerations.ValueAction valueAction)
        {
            throw new NotImplementedException("Not implemented on IshMetadataFilterField");
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
            return new IshMetadataFilterField(this);
        }

        /// <summary>
        /// Generates a new ishfield object where the field becomes a metadata field (e.g. with Value, ValueType)
        /// </summary>
        public override IshField ToMetadataField()
        {
            return new IshMetadataField(Name, Level, _valueType, _value);
        }

        public override void GetXml(ref XmlWriter xmlWriter)
        {
            xmlWriter.WriteStartElement("ishfield");
            xmlWriter.WriteAttributeString("name", Name);
            xmlWriter.WriteAttributeString("level", StringEnum.GetStringValue(Level));
            xmlWriter.WriteAttributeString("ishoperator", StringEnum.GetStringValue(_filterOperator));
            xmlWriter.WriteAttributeString("ishvaluetype", StringEnum.GetStringValue(_valueType));
            xmlWriter.WriteString(_value);
            xmlWriter.WriteEndElement();
        }

        /// <summary>
        /// Debugging implementation
        /// </summary>
        /// <returns>string</returns>
        public override string ToString()
        {
            return $"Set-IshMetadataFilterField -Level {StringEnum.GetStringValue(Level)} -Name {Name} -FilterOperator {StringEnum.GetStringValue(_filterOperator)} -ValueType {StringEnum.GetStringValue(_valueType)} -Value \"{_value}\"";
            //return $"<ishfield name='{Name}' level='{StringEnum.GetStringValue(Level)}' ishoperator='{StringEnum.GetStringValue(_filterOperator)}' ishvaluetype='{StringEnum.GetStringValue(_valueType)}'>{_value}</ishfield>";
        }
    }
}
