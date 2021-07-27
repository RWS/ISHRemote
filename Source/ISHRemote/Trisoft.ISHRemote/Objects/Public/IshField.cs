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
    /// <para type="description">Holds one field metadata entry described by a name, optional level and value</para>
    /// </summary>
    public class IshField
    {
        protected string _fieldName;
        protected Enumerations.Level _fieldLevel;
        protected Enumerations.ValueType _valueType;

        protected IshField()
        {
            _fieldName = "";
            _fieldLevel = Enumerations.Level.None;
            _valueType = Enumerations.ValueType.Value;
        }

        public IshField(string fieldName, Enumerations.Level fieldLevel, Enumerations.ValueType valueType)
        {
            _fieldName = (fieldName == null) ? "" : fieldName;
            _fieldLevel = fieldLevel;
            _valueType = valueType;
        }

        public string Name
        {
            get { return _fieldName; }
        }

        public Enumerations.Level Level
        {
            get { return _fieldLevel; }
        }

        public Enumerations.ValueType ValueType
        {
            get { return _valueType; }
        }

        public virtual void Join(IshField ishField, Enumerations.ValueAction valueAction)
        {
            throw new NotImplementedException("Not implemented on IshField");
        }

        /// <summary>
        /// Generates a new ishfield object where the field only has requested field information (e.g. no Value, but ValueType if available)
        /// </summary>
        public virtual IshField ToRequestedMetadataField()
        {
            return new IshRequestedMetadataField(_fieldName, _fieldLevel, _valueType);
        }

        /// <summary>
        /// Generates a new ishfield object where the field becomes a filter field (e.g. with Value, ValueType, operator if available)
        /// </summary>
        public virtual IshField ToMetadataFilterField()
        {
            return new IshMetadataFilterField(_fieldName, _fieldLevel, Enumerations.FilterOperator.Equal, "", _valueType);
        }

        /// <summary>
        /// Generates a new ishfield object where the field becomes a metadata field (e.g. with Value, ValueType)
        /// </summary>
        public virtual IshField ToMetadataField()
        {
            return new IshMetadataField(_fieldName, _fieldLevel, _valueType, "");
        }

        public virtual void GetXml(ref XmlWriter xmlWriter)
        {
            xmlWriter.WriteStartElement("ishfield");
            xmlWriter.WriteAttributeString("name", Name);
            xmlWriter.WriteAttributeString("level", StringEnum.GetStringValue(Level));
            xmlWriter.WriteAttributeString("ishvaluetype", StringEnum.GetStringValue(ValueType));
            xmlWriter.WriteEndElement();
        }

        // override object.Equals
        public override bool Equals(object obj)
        {
            //       
            // See the full list of guidelines at
            //   http://go.microsoft.com/fwlink/?LinkID=85237  
            // and also the guidance for operator== at
            //   http://go.microsoft.com/fwlink/?LinkId=85238
            //

            if ((obj == null) || !(obj is IshField))
            {
                return false;
            }
            IshField compareIshField = (IshField)obj;
            if ((_fieldName == compareIshField.Name) && (_fieldLevel == compareIshField.Level) && (_valueType == compareIshField.ValueType || compareIshField._valueType == Enumerations.ValueType.All))
            {
                return true;
            }
            return false;
        }

        // override object.GetHashCode
        public override int GetHashCode()
        {
            // TODO: [Could]write your implementation of GetHashCode() here
            return base.GetHashCode();
        }
    }
}
