using System;
using System.Collections.Generic;
using System.Text;
using Trisoft.ISHRemote.Objects;
using Trisoft.ISHRemote.Objects.Public;

namespace Trisoft.ISHRemote.ExtensionMethods
{
    internal static class OpenApiFieldValueExtensions
    {
        internal static IshFields ToIshFields(this ICollection<OpenApi.FieldValue> fieldValues, IshSession ishSession)
        {
            var ishFields = new IshFields();
            foreach (var fieldValue in fieldValues)
            {
                switch (fieldValue.Type)
                {
                    case OpenApi.FieldValueType.CardFieldValue:
                        {
                            var typedFieldValue = fieldValue as OpenApi.CardFieldValue;  // Can I be optimistic or is null-check required after every cast?
                            ishFields.AddField(new IshMetadataField(typedFieldValue.IshField.Name, Enumerations.ToFieldLevel(typedFieldValue.IshField.Level), Enumerations.ValueType.Value, typedFieldValue.Value.Title));
                            ishFields.AddField(new IshMetadataField(typedFieldValue.IshField.Name, Enumerations.ToFieldLevel(typedFieldValue.IshField.Level), Enumerations.ValueType.Id, typedFieldValue.Value.Id));
                            break;
                        }
                    case OpenApi.FieldValueType.MultiCardFieldValue:
                        {
                            var typedFieldValue = fieldValue as OpenApi.MultiCardFieldValue;
                            StringBuilder values = new StringBuilder();
                            StringBuilder ids = new StringBuilder();
                            // TODO [Question] BaseObject offers Title (ahum Value) and Id (probably card_id) but where do I get Element name?
                            foreach (var baseObject in typedFieldValue.Value)
                            {
                                values.Append(baseObject.Title);
                                ids.Append(baseObject.Id);
                            }
                            ishFields.AddField(new IshMetadataField(
                                typedFieldValue.IshField.Name,
                                Enumerations.ToFieldLevel(typedFieldValue.IshField.Level),
                                Enumerations.ValueType.Value,
                                string.Join(ishSession.Separator, values))
                                );
                            ishFields.AddField(new IshMetadataField(
                                typedFieldValue.IshField.Name,
                                Enumerations.ToFieldLevel(typedFieldValue.IshField.Level),
                                Enumerations.ValueType.Id,
                                string.Join(ishSession.Separator, ids))
                                );
                            break;
                        }
                    case OpenApi.FieldValueType.DateTimeFieldValue:
                        {
                            // IShSession should offer date time format string as property
                            break;
                        }
                    case OpenApi.FieldValueType.MultiDateTimeFieldValue:
                        {
                            break;
                        }
                    case OpenApi.FieldValueType.LovFieldValue:
                        {
                            var typedFieldValue = fieldValue as OpenApi.LovFieldValue;
                            ishFields.AddField(new IshMetadataField(typedFieldValue.IshField.Name, Enumerations.ToFieldLevel(typedFieldValue.IshField.Level), Enumerations.ValueType.Value, typedFieldValue.Value.Title));
                            ishFields.AddField(new IshMetadataField(typedFieldValue.IshField.Name, Enumerations.ToFieldLevel(typedFieldValue.IshField.Level), Enumerations.ValueType.Id, typedFieldValue.Value.Id));
                            break;
                        }
                    case OpenApi.FieldValueType.MultiLovFieldValue:
                        {
                            var typedFieldValue = fieldValue as OpenApi.MultiLovFieldValue;
                            StringBuilder values = new StringBuilder();
                            StringBuilder ids = new StringBuilder();
                            // TODO [Question] BaseObject offers Title (ahum Value) and Id (probably card_id) but where do I get Element name?
                            foreach (var baseObject in typedFieldValue.Value)
                            {
                                values.Append(baseObject.Title);
                                ids.Append(baseObject.Id);
                            }
                            ishFields.AddField(new IshMetadataField(
                                typedFieldValue.IshField.Name,
                                Enumerations.ToFieldLevel(typedFieldValue.IshField.Level),
                                Enumerations.ValueType.Value,
                                string.Join(ishSession.Separator, values))
                                );
                            ishFields.AddField(new IshMetadataField(
                                typedFieldValue.IshField.Name,
                                Enumerations.ToFieldLevel(typedFieldValue.IshField.Level),
                                Enumerations.ValueType.Id,
                                string.Join(ishSession.Separator, ids))
                                );
                            break;
                        }
                    case OpenApi.FieldValueType.NumberFieldValue:
                        break;
                    case OpenApi.FieldValueType.MultiNumberFieldValue:
                        break;
                    case OpenApi.FieldValueType.StringFieldValue:
                        break;
                    case OpenApi.FieldValueType.MultiStringFieldValue:
                        break;
                    case OpenApi.FieldValueType.TagFieldValue:
                        break;
                    case OpenApi.FieldValueType.MultiTagFieldValue:
                        break;
                    case OpenApi.FieldValueType.VersionFieldValue:
                        break;
                    default:
                        throw new NotImplementedException($"OpenApiFieldValueExtensions.ToIshFields cannot handle fieldValue.Type[{ fieldValue.Type }]");
                }
            }
            return ishFields;
        }
    }
}
