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
using System.IO;
using Trisoft.ISHRemote.HelperClasses;
using Trisoft.ISHRemote.Objects.Public;
using Trisoft.ISHRemote.ExtensionMethods;

namespace Trisoft.ISHRemote.Objects
{
    /// <summary>
    /// <para type="description">Container object to group multiple IshField entries, eaching holding one field metadata entry described by a name, optional level and value</para>
    /// <para type="description">Should not be used as cmdlet parameter, single IshField and IshField[] are more PowerShell friendly.</para>
    /// </summary>
    public class IshFields
    {
        private List<IshField> _fields;

        /// <summary>
        /// Creates an empty instance of the ishfield object.
        /// </summary>
        public IshFields()
        {
            _fields = new List<IshField>();
        }

        /// <summary>
        /// Creates an instance based on the incoming IshField array or an empty instance.
        /// </summary>
        public IshFields(IshField[] ishFields)
        {
            _fields = new List<IshField>(ishFields ?? new IshField[0]);
        }

        /// <summary>
        /// Creates an instance based on the incoming IshField list or an empty instance.
        /// </summary>
        public IshFields(List<IshField> ishFields)
        {
            _fields = new List<IshField>(ishFields.ToArray() ?? new IshField[0]);
        }

        /// <summary>
        /// Creates a new instance of the ishfield object.
        /// </summary>
        /// <param name="xmlIshFields">Xml element with the ishfield information.</param>
        public IshFields(XmlElement xmlIshFields)
        {
            _fields = new List<IshField>();
            if (xmlIshFields != null)
            {
                foreach (XmlElement xmlIshField in xmlIshFields.SelectNodes("ishfield"))
                {
                    AddField(new IshMetadataField(xmlIshField));
                }
            }
        }

        /// <summary>
        /// Creates a new instance based on the incoming OpenApi models. Any multi-value field are joined up by the separator (typically comma-space)
        /// </summary>
        /// <param name="oFieldValues">Incoming OpenApi Field Values</param>
        /// <param name="separator">Any multi-value field are joined up by the separator (typically comma-space), mostly coming from IshSession.</param>
        public IshFields(ICollection<OpenApi.FieldValue> oFieldValues, string separator)
        {
            _fields = new List<IshField>(); 
            _fields = oFieldValues.ToIshMetadataFields().Fields().ToList();
        }

        /// <summary>
        /// The current fields list.
        /// </summary>
        /// <returns>An array of <see cref="IshField"/>.</returns>
        public IshField[] Fields()
        {
            return _fields.ToArray();
        }

        /// <summary>
        /// Number of fields.
        /// </summary>
        /// <returns>Returns the number of fields.</returns>
        public int Count()
        {
            return _fields.Count;
        }

        /// <summary>
        /// Generates a new ishfields object where all fields only have requested field information (e.g. no Value, but ValueType if available)
        /// </summary>
        /// <returns>The current list of <see cref="IshFields"/>.</returns>
        public IshFields ToRequestedFields()
        {
            IshFields returnFields = new IshFields();
            foreach (IshField ishField in _fields)
            {
                returnFields.AddField(ishField.ToRequestedMetadataField());
            }
            return returnFields;
        }
        /// <summary>
        /// Generates a new ishfields object where all fields only have requested field information (e.g. no Value, but ValueType if available)
        /// Remove all level entries that do not match the filter.
        /// </summary>
        /// <param name="filterFieldLevel">Level attribute that should be matched to be part of the result.</param>
        /// <returns>The current list of <see cref="IshFields"/>.</returns>
        public IshFields ToRequestedFields(Enumerations.Level filterFieldLevel)
        {
            IshFields returnFields = new IshFields();
            foreach (IshField ishField in _fields)
            {
                if (ishField.Level.Equals(filterFieldLevel))
                {
                    returnFields.AddField(ishField.ToRequestedMetadataField());
                }
            }
            return returnFields;
        }

        /// <summary>
        /// Generates exact filters (using Equal operator) from the given fields
        /// </summary>
        /// <returns>The current list of <see cref="IshFields"/>.</returns>
        public IshFields ToFilterFields()
        {
            IshFields returnFields = new IshFields();
            foreach (IshField ishField in _fields)
            {
                returnFields.AddField(ishField.ToMetadataFilterField());
            }
            return returnFields;
        }

        /// <summary>
        /// Add a field to the current list.
        /// </summary>
        /// <param name="ishField">The field that needs to be added.</param>
        /// <returns>The current list of <see cref="IshFields"/>.</returns>
        public IshFields AddField(IshField ishField)
        {
            _fields.Add(ishField);
            return this;
        }

        /// <summary>
        /// Removes all previous fields and insert the given field
        /// </summary>
        /// <returns>The current list of <see cref="IshFields"/>.</returns>
        public IshFields AddOrUpdateField(IshField ishField, Enumerations.ActionMode actionMode)
        {
            switch (actionMode)
            {
                // Remove all value types to avoid ambiguity, no preference of element over value, so last one wins
                case Enumerations.ActionMode.Create:
                case Enumerations.ActionMode.Update:
                    RemoveField(ishField.Name, ishField.Level, Enumerations.ValueType.All);
                    break;
                case Enumerations.ActionMode.Find:
                case Enumerations.ActionMode.Search:
                case Enumerations.ActionMode.Read:
                default:
                    RemoveField(ishField);
                    break;
            }
            AddField(ishField);
            return this;
        }

        /// <summary>
        /// Remove a field from the current list.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="fieldLevel">The level of the field (<see cref="Enumerations.Level"/>).</param>
        /// <param name="valueType">The type of the field (<see cref="Enumerations.ValueType"/>).</param>
        /// <returns></returns>
		public IshFields RemoveField(string fieldName, string fieldLevel, string valueType)
        {
            return RemoveField(fieldName, 
                (Enumerations.Level)StringEnum.Parse(typeof(Enumerations.Level), fieldLevel), 
                (Enumerations.ValueType)StringEnum.Parse(typeof(Enumerations.ValueType), valueType));
        }

        /// <summary>
        /// Remove a field from the current list.
        /// </summary>
        /// <param name="fieldName">The name of the field.</param>
        /// <param name="fieldLevel">The level of the field (<see cref="Enumerations.Level"/>).</param>
        /// <param name="valueType">The type of the field (<see cref="Enumerations.ValueType"/>).</param>
        /// <returns>The current list of <see cref="IshFields"/>.</returns>
        public IshFields RemoveField(string fieldName, Enumerations.Level fieldLevel, Enumerations.ValueType valueType)
        {
            var compareField = new IshRequestedMetadataField(fieldName, fieldLevel,valueType);
            return RemoveField(compareField);
        }

        /// <summary>
        /// Remove a field from the current list.
        /// </summary>
        /// <param name="compareField">The <see cref="IshField"/> that needs to be removed.</param>
        /// <returns>The current list of <see cref="IshFields"/>.</returns>
        public IshFields RemoveField(IshField compareField)
        {
            List<IshField> returnFields = new List<IshField>();
            foreach (IshField ishField in _fields)
            {
                if ( ! ((IshField)ishField).Equals(compareField))
                {
                    returnFields.Add(ishField);
                }
            }
            _fields = returnFields;
            return this;
        }

        /// <summary>
        /// Merge or Join the incoming ishfields (IshMetadataFields) on my own ishfields using the provided action
        /// </summary>
        /// <param name="ishFields">Fields to join with my own fields</param>
        /// <param name="valueAction">String action (overwrite/prepend/append) to use on matching fields</param>
        public virtual void JoinFields(IshFields ishFields, Enumerations.ValueAction valueAction)
        {
            foreach (IshField actionIshField in ishFields.Fields())
            {
                IshField[] overwriteFields = Retrieve(actionIshField.Name, actionIshField.Level);
                if (overwriteFields.Length > 0)
                {
                    // field is present, overwrite occurances
                    foreach (IshField ourIshField in overwriteFields)
                    {
                        ourIshField.Join(actionIshField, valueAction);
                    }
                }
                else
                { 
                    // field was not yet present, add it
                    AddField(actionIshField);
                }
            }
        }

        /// <summary>
        /// Retrieves all occurances out of the list of IshFields
        /// </summary>
        /// <returns>An array of <see cref="IshFields"/>.</returns>
        public IshField[] Retrieve(string fieldName, Enumerations.Level fieldLevel)
        {
            return Retrieve(fieldName, fieldLevel, Enumerations.ValueType.Value);
        }

        /// <summary>
        /// Retrieves the first occurance out of the list of matching IshFields
        /// </summary>
        /// <returns>The first <see cref="IshField"/> in the current list.</returns>
        public IshField RetrieveFirst(string fieldName, Enumerations.Level fieldLevel, Enumerations.ValueType valueType)
        {
            IshField[] ishFields = Retrieve(fieldName, fieldLevel, valueType);
            if ((ishFields != null) && (ishFields.Length > 0))
            {
                return ishFields[0];
            }
            return null;
        }

        /// <summary>
        /// Retrieves the first occurance out of the list of matching IshFields; preferring Id over Element and then Value.
        /// So first Id '4484', if not present then Element 'VUSERADMIN', again if not present Value 'Admin'.
        /// </summary>
        /// <returns>The first <see cref="IshField"/> in the current list.</returns>
        public IshField RetrieveFirst(string fieldName, Enumerations.Level fieldLevel)
        {
            IshField ishField = RetrieveFirst(fieldName, fieldLevel, Enumerations.ValueType.Id);
            if (ishField == null)
            {
                ishField = RetrieveFirst(fieldName, fieldLevel, Enumerations.ValueType.Element);
            }
            if (ishField == null)
            {
                ishField = RetrieveFirst(fieldName, fieldLevel, Enumerations.ValueType.Value);
            }
            return ishField;
        }

        /// <summary>
        /// Retrieves the first occurance out of the list of matching IshFields
        /// </summary>
        /// <returns>The first <see cref="IshField"/> in the current list.</returns>
        public IshField RetrieveFirst(string fieldName, string fieldLevel, string valueType)
        {
            IshField[] ishFields = Retrieve(fieldName, 
                (Enumerations.Level)StringEnum.Parse(typeof(Enumerations.Level),fieldLevel),
                (Enumerations.ValueType)StringEnum.Parse(typeof(Enumerations.ValueType),valueType));
            if ((ishFields != null) && (ishFields.Length > 0))
            {
                return ishFields[0];
            }
            return null;
        }

        /// <summary>
        /// Retrieves a list of <see cref="IshField"/> using a filter.
        /// </summary>
        /// <param name="fieldName">The field name.</param>
        /// <param name="fieldLevel">The field level <see cref="Enumerations.Level"/>.</param>
        /// <param name="valueType">The value type <see cref="Enumerations.ValueType"/>.</param>
        /// <returns>An array of <see cref="IshField"/>.</returns>
        /// <remarks></remarks>
        public IshField[] Retrieve(string fieldName, Enumerations.Level fieldLevel, Enumerations.ValueType valueType)
        {
            List<IshField> returnFields = new List<IshField>();
            foreach (IshField ishField in _fields)
            {
                if ((ishField.Name == fieldName) && (ishField.Level == fieldLevel))
                {
                    if (ishField is IshMetadataField)
                    { 
                        if ( ((IshMetadataField)ishField).ValueType == valueType )
                        {
                            returnFields.Add(ishField);
                        }
                    }
                    else if (ishField is IshMetadataFilterField)
                    {
                        if (((IshMetadataFilterField)ishField).ValueType == valueType)
                        {
                            returnFields.Add(ishField);
                        }
                    }
                    else
                    {
                        returnFields.Add(ishField);
                    }
                }
            }
            return returnFields.ToArray();
        }

        /// <summary>
        /// Write the xml from the current list.
        /// </summary>
        /// <param name="xmlWriter">The <see cref="XmlWriter"/>.</param>
        public virtual void GetXml(ref XmlWriter xmlWriter)
        { 
            xmlWriter.WriteStartElement("ishfields");
            foreach (IshField ishField in _fields)
            {
                ishField.GetXml(ref xmlWriter);
            }
            xmlWriter.WriteEndElement();
        }

        /// <summary>
        /// Get an xml string from the current list;
        /// </summary>
        /// <returns>A string with the xml.</returns>
        public string ToXml()
        {
            if (Count() <= 0)
            {
                return "";
            }
            using (StringWriter stringWriter = new StringWriter())
            {
                XmlWriter xmlWriter;
                using (xmlWriter = XmlWriter.Create(stringWriter))
                {
                    GetXml(ref xmlWriter);
                }
                return stringWriter.ToString();
            }
        }

        /// <summary>
        /// Gets the value for a field.
        /// </summary>
        /// <param name="fieldName">The fieldname to get the value from.</param>
        /// <param name="fieldLevel">The fieldlevel (<see cref="Enumerations.Level"/>).</param>
        /// <param name="valueType">The valuetype (<see cref="Enumerations.ValueType"/>).</param>
        /// <returns></returns>
        public string GetFieldValue(string fieldName, Enumerations.Level fieldLevel, Enumerations.ValueType valueType)
        {
            if (_fields == null)
                throw new InvalidOperationException("No fields are set.");

            IshField field = RetrieveFirst(fieldName, fieldLevel, valueType);
            string fieldValue = string.Empty;
            if (field != null)
            {
                var filterField = field.ToMetadataFilterField() as IshMetadataFilterField;
                if (filterField == null)
                    throw new InvalidOperationException("field.ToMetadataFilterField() is not IshMetadataFilterField");

                fieldValue = filterField.Value;
            }
            return fieldValue;
        }
    }
}
