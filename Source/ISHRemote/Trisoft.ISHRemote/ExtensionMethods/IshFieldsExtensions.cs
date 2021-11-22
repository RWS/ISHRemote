using System;
using System.Collections.Generic;
using System.Linq;
using Trisoft.ISHRemote.Objects;
using Trisoft.ISHRemote.Objects.Public;
using Trisoft.ISHRemote.OpenApi;

namespace Trisoft.ISHRemote.ExtensionMehods
{
    internal static class IshFieldsExtensionMethods
    {
        internal static ICollection<SetFieldValue> ToSetFieldValues(this IshFields ishFields, IshSession ishSession )
        {
            ICollection<SetFieldValue> fieldValues = new List<SetFieldValue>();

            foreach(Objects.Public.IshField ishField in ishFields.Fields())
            {
                SetFieldValue setFieldValue = IshFieldToSetFieldValue(ishField, ishSession, ishFields);
                if (setFieldValue != null)
                {
                    fieldValues.Add(setFieldValue);
                }
            }

            return fieldValues;
        }

        internal static OpenApi.Level ToOpenApiLevel(this Enumerations.Level level)
        {
            switch(level)
            {
                case Enumerations.Level.Annotation:
                    return OpenApi.Level.Annotation;

                case Enumerations.Level.Data:
                    return OpenApi.Level.Data;

                case Enumerations.Level.Detail:
                    return OpenApi.Level.Detail;

                case Enumerations.Level.History:
                    // TODO
                    return OpenApi.Level.None;

                case Enumerations.Level.Lng:
                    return OpenApi.Level.Language;

                case Enumerations.Level.Logical:
                    return OpenApi.Level.Logical;

                case Enumerations.Level.None:
                    return OpenApi.Level.None;

                case Enumerations.Level.Progress:
                    return OpenApi.Level.None;

                case Enumerations.Level.Reply:
                    return OpenApi.Level.Reply;

                case Enumerations.Level.Task:
                    // TODO
                    return OpenApi.Level.None;
                
                case Enumerations.Level.Version:
                    return OpenApi.Level.Version;
            }

            return OpenApi.Level.None;
        }

        private static SetFieldValue IshFieldToSetFieldValue(Objects.Public.IshField ishField, IshSession ishSession, IshFields ishFields)
        {
            SetFieldValue setFieldValue = null;
            IshTypeFieldDefinition ishTypeFieldDefinition = ishSession.IshTypeFieldDefinition.FirstOrDefault(f => f.Name == ishField.Name && f.Level == ishField.Level);

            if (ishTypeFieldDefinition != null)
            {
                OpenApi.IshField openApiIshField = new OpenApi.IshField() {  Level = ishField.Level.ToOpenApiLevel(), Name = ishField.Name, Type = nameof(OpenApi.IshField ) };
                
                string fieldValue = ishFields.GetFieldValue(ishField.Name, ishField.Level, ishField.ValueType);
                // TODO why is the separator on IshSession a string?
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
                            Value = multiFieldValues.Select(v => new SetLovValue(){ Id = v, Type = nameof(SetLovValue) }).ToList()
                        }
                        : new SetLovFieldValue()
                        {
                            IshField = openApiIshField,
                            Value = new SetLovValue() { Id = fieldValue, Type = nameof(SetLovValue) }
                        };
                        break;

                    case Enumerations.DataType.ISHType:
                        // TODO: How do we know what type of cardtype this is?
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

            // TODO ReferencyType for a field can have multiple values?
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
                        setBaseObject = new SetDocumentObject() { Id = id };
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