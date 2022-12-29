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
using System.Text;
using Trisoft.ISHRemote.Objects;
using Trisoft.ISHRemote.Objects.Public;

namespace Trisoft.ISHRemote.ExtensionMethods
{
    internal static class OpenApiISH30FieldValueExtensions
    {
        /* #115 attempt to convertincoming fieldvalues to IShFields, not linked/used to my knowledge
        internal static IshFields ToIshFields(this ICollection<OpenApiISH30.FieldValue> fieldValues, IshSession ishSession)
        {
            var ishFields = new IshFields();
            foreach (var fieldValue in fieldValues)
            {
                switch (fieldValue.Type)
                {
                    case OpenApiISH30.FieldValueType.CardFieldValue:
                        {
                            var typedFieldValue = fieldValue as OpenApiISH30.CardFieldValue;  // Can I be optimistic or is null-check required after every cast?
                            ishFields.AddField(new IshMetadataField(typedFieldValue.IshField.Name, typedFieldValue.IshField.Level.ToISHFieldLevel(), Enumerations.ValueType.Value, typedFieldValue.Value.Title));
                            ishFields.AddField(new IshMetadataField(typedFieldValue.IshField.Name, typedFieldValue.IshField.Level.ToISHFieldLevel(), Enumerations.ValueType.Id, typedFieldValue.Value.Id));
                            break;
                        }
                    case OpenApiISH30.FieldValueType.MultiCardFieldValue:
                        {
                            var typedFieldValue = fieldValue as OpenApiISH30.MultiCardFieldValue;
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
                                typedFieldValue.IshField.Level.ToISHFieldLevel(),
                                Enumerations.ValueType.Value,
                                string.Join(ishSession.Separator, values))
                                );
                            ishFields.AddField(new IshMetadataField(
                                typedFieldValue.IshField.Name,
                                typedFieldValue.IshField.Level.ToISHFieldLevel(),
                                Enumerations.ValueType.Id,
                                string.Join(ishSession.Separator, ids))
                                );
                            break;
                        }
                    case OpenApiISH30.FieldValueType.DateTimeFieldValue:
                        {
                            // IShSession should offer date time format string as property
                            break;
                        }
                    case OpenApiISH30.FieldValueType.MultiDateTimeFieldValue:
                        {
                            break;
                        }
                    case OpenApiISH30.FieldValueType.LovFieldValue:
                        {
                            var typedFieldValue = fieldValue as OpenApiISH30.LovFieldValue;
                            ishFields.AddField(new IshMetadataField(typedFieldValue.IshField.Name, typedFieldValue.IshField.Level.ToISHFieldLevel(), Enumerations.ValueType.Value, typedFieldValue.Value.Title));
                            ishFields.AddField(new IshMetadataField(typedFieldValue.IshField.Name, typedFieldValue.IshField.Level.ToISHFieldLevel(), Enumerations.ValueType.Id, typedFieldValue.Value.Id));
                            break;
                        }
                    case OpenApiISH30.FieldValueType.MultiLovFieldValue:
                        {
                            var typedFieldValue = fieldValue as OpenApiISH30.MultiLovFieldValue;
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
                                typedFieldValue.IshField.Level.ToISHFieldLevel(),
                                Enumerations.ValueType.Value,
                                string.Join(ishSession.Separator, values))
                                );
                            ishFields.AddField(new IshMetadataField(
                                typedFieldValue.IshField.Name,
                                typedFieldValue.IshField.Level.ToISHFieldLevel(),
                                Enumerations.ValueType.Id,
                                string.Join(ishSession.Separator, ids))
                                );
                            break;
                        }
                    case OpenApiISH30.FieldValueType.NumberFieldValue:
                        break;
                    case OpenApiISH30.FieldValueType.MultiNumberFieldValue:
                        break;
                    case OpenApiISH30.FieldValueType.StringFieldValue:
                        break;
                    case OpenApiISH30.FieldValueType.MultiStringFieldValue:
                        break;
                    case OpenApiISH30.FieldValueType.TagFieldValue:
                        break;
                    case OpenApiISH30.FieldValueType.MultiTagFieldValue:
                        break;
                    case OpenApiISH30.FieldValueType.VersionFieldValue:
                        break;
                    default:
                        throw new NotImplementedException($"OpenApiISH30FieldValueExtensions.ToIshFields cannot handle fieldValue.Type[{ fieldValue.Type }]");
                }
            }
            return ishFields;
        }
        */
    }
}
