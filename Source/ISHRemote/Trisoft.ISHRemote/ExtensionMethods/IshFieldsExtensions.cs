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
using Trisoft.ISHRemote.Objects;
using Trisoft.ISHRemote.Objects.Public;
using Trisoft.ISHRemote.OpenApiISH30;

namespace Trisoft.ISHRemote.ExtensionMethods
{
    internal static class IshFieldsExtensionMethods
    {
        internal static ICollection<SetFieldValue> ToOpenApiISH30SetFieldValues(this IshFields ishFields, IshSession ishSession)
        {
            ICollection<SetFieldValue> fieldValues = new List<SetFieldValue>();

            foreach(Objects.Public.IshField ishField in ishFields.Fields())
            {
                SetFieldValue setFieldValue = IshFieldToOpenApiISH30SetFieldValue(ishField, ishSession, ishFields);
                if (setFieldValue != null)
                {
                    fieldValues.Add(setFieldValue);
                }
            }

            return fieldValues;
        }

        internal static OpenApiISH30.Level ToOpenApiISH30Level(this Enumerations.Level level)
        {
            switch(level)
            {
                case Enumerations.Level.Annotation:
                    return OpenApiISH30.Level.Annotation;

                case Enumerations.Level.Data:
                    return OpenApiISH30.Level.Data;

                case Enumerations.Level.Detail:
                    return OpenApiISH30.Level.EventProgressDetail;

                case Enumerations.Level.History:
                    // TODO [Could] API30 enumerations
                    return OpenApiISH30.Level.None;

                case Enumerations.Level.Lng:
                    return OpenApiISH30.Level.Language;

                case Enumerations.Level.Logical:
                    return OpenApiISH30.Level.Logical;

                case Enumerations.Level.None:
                    return OpenApiISH30.Level.None;

                case Enumerations.Level.Progress:
                    return OpenApiISH30.Level.None;

                case Enumerations.Level.Reply:
                    return OpenApiISH30.Level.Reply;

                case Enumerations.Level.Task:
                    // TODO [Could] API30 enumerations
                    return OpenApiISH30.Level.None;
                
                case Enumerations.Level.Version:
                    return OpenApiISH30.Level.Version;
            }

            return OpenApiISH30.Level.None;
        }

        private static SetFieldValue IshFieldToOpenApiISH30SetFieldValue(Objects.Public.IshField ishField, IshSession ishSession, IshFields ishFields)
        {
            SetFieldValue setFieldValue = null;
            IshTypeFieldDefinition ishTypeFieldDefinition = ishSession.IshTypeFieldDefinition.FirstOrDefault(f => f.Name == ishField.Name && f.Level == ishField.Level);

            if (ishTypeFieldDefinition != null)
            {
                OpenApiISH30.IshField openApiIshField = new OpenApiISH30.IshField() {  Level = ishField.Level.ToOpenApiISH30Level(), Name = ishField.Name };
                
                string fieldValue = ishFields.GetFieldValue(ishField.Name, ishField.Level, ishField.ValueType);
                string[] multiFieldValues = fieldValue.Split(ishSession.Separator.ToCharArray(), StringSplitOptions.None);

                switch (ishTypeFieldDefinition.DataType)
                {
                    case Enumerations.DataType.DateTime:
                        setFieldValue = ishTypeFieldDefinition.IsMultiValue ? (SetFieldValue)new SetMultiDateTimeFieldValue()
                        {
                            IshField = openApiIshField, 
                            Value = multiFieldValues.Select(v => DateTimeOffset.Parse(v)).ToList()
                        }
                        : new SetDateTimeFieldValue() 
                        { 
                            IshField = openApiIshField, 
                            Value = DateTime.Parse(fieldValue)
                        };
                        break;

                    case Enumerations.DataType.ISHLov:
                        setFieldValue = ishTypeFieldDefinition.IsMultiValue ? (SetFieldValue)new SetMultiLovFieldValue()
                        {
                            IshField = openApiIshField,
                            Value = multiFieldValues.Select(v => new SetLovValue() { Id = v }).ToList()
                        }
                        : new SetLovFieldValue()
                        {
                            IshField = openApiIshField,
                            Value = new SetLovValue() { Id = fieldValue }
                        };
                        break;

                    case Enumerations.DataType.ISHType:
                        // TODO [Must] How do we know what type of cardtype this is?
                        ICollection<SetBaseObject> setBaseObjects = GetSetBaseObjectFromReferenceType(multiFieldValues, ishTypeFieldDefinition);
                        setFieldValue = ishTypeFieldDefinition.IsMultiValue ? (SetFieldValue)new SetMultiCardFieldValue()
                        {
                            IshField = openApiIshField,
                            Value = setBaseObjects
                        }
                        : new SetCardFieldValue()
                        {
                            IshField = openApiIshField,
                            Value = setBaseObjects.FirstOrDefault()
                        };
                        break;

                    case Enumerations.DataType.ISHMetadataBinding:
                    case Enumerations.DataType.String:
                    case Enumerations.DataType.LongText:
                        setFieldValue = ishTypeFieldDefinition.IsMultiValue ? (SetFieldValue)new SetMultiStringFieldValue()
                        {
                            IshField = openApiIshField,
                            Value = multiFieldValues
                        }
                        : new SetStringFieldValue()
                        {
                            IshField = openApiIshField,
                            Value = fieldValue
                        };
                        break;

                    case Enumerations.DataType.Number:
                        setFieldValue = ishTypeFieldDefinition.IsMultiValue ? (SetFieldValue)new SetMultiNumberFieldValue()
                        {
                            IshField = openApiIshField,
                            Value = multiFieldValues.Select(v => Double.Parse(v)).ToList()
                        }
                        : new SetNumberFieldValue()
                        {
                            IshField = openApiIshField,
                            Value = Double.Parse(fieldValue)
                        };
                        break;
                }
            }

            return setFieldValue;
        }


        private static ICollection<SetBaseObject> GetSetBaseObjectFromReferenceType(string[] ids, IshTypeFieldDefinition ishTypeFieldDefinition)
        {
            List<SetBaseObject> setBaseObjects = new List<SetBaseObject>(ids.Count());

            // TODO [Should] ReferencyType for a field can have multiple values?
            Enumerations.ISHType ishType = ishTypeFieldDefinition.ReferenceType.First();

            foreach (string id in ids)
            {
                SetBaseObject setBaseObject = null;

                switch (ishType)
                {
                    case Enumerations.ISHType.ISHUser:
                        setBaseObject = new SetUser() { Id = id };
                        break;

                    case Enumerations.ISHType.ISHUserGroup:
                        setBaseObject = new SetUserGroup() { Id = id };
                        break;

                    case Enumerations.ISHType.ISHFolder:
                        setBaseObject = new SetFolder() { Id = id };
                        break;

                    case Enumerations.ISHType.ISHEDT:
                        setBaseObject = new SetElectronicDocumentType() { Id = id };
                        break;

                    case Enumerations.ISHType.ISHLibrary:
                    case Enumerations.ISHType.ISHMasterDoc:
                    case Enumerations.ISHType.ISHIllustration:
                    case Enumerations.ISHType.ISHModule:
                    case Enumerations.ISHType.ISHTemplate:
                        setBaseObject = new SetDocumentObject() { LogicalId = id };
                    break;
                }

                if (setBaseObject != null)
                {
                    setBaseObjects.Add(setBaseObject);
                }
            }

            return setBaseObjects;
        }
    }
}
