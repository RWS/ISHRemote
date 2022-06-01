using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Trisoft.ISHRemote.Objects;
using Trisoft.ISHRemote.Objects.Public;

namespace Trisoft.ISHRemote.ExtensionMethods
{
    internal static class FieldValueExtensions
    {
        internal static IshFields ToIshMetadataFields(this IEnumerable<OpenApi.FieldValue> fieldValues)
        {
            IshFields ishFields = new IshFields();

            foreach (OpenApi.FieldValue fieldValue in fieldValues)
            {
                IshField ishField = FieldValueToIshMetadataField(fieldValue);
                ishFields.AddField(ishField);
            }

            // TODO [Must!] OpenApi FieldValues should retain Element values (especially for IMetadataBinding) ... consider GetFieldElementAsString addition

            return ishFields;
        }

        internal static Enumerations.Level ToEnumerationsLevel(this OpenApi.Level level)
        {
            switch (level)
            {
                case OpenApi.Level.Annotation:
                    return Enumerations.Level.Annotation;
                case OpenApi.Level.Data:
                    return Enumerations.Level.Data;

                case OpenApi.Level.Detail:
                    return Enumerations.Level.Detail;

                case OpenApi.Level.Language:
                    return Enumerations.Level.Lng;

                case OpenApi.Level.Logical:
                    return Enumerations.Level.Logical;

                case OpenApi.Level.None:
                    return Enumerations.Level.None;

                case OpenApi.Level.Progress:
                    return Enumerations.Level.Progress;

                case OpenApi.Level.Reply:
                    return Enumerations.Level.Reply;

                case OpenApi.Level.Version:
                    return Enumerations.Level.Version;
                
                case OpenApi.Level.Object:
                case OpenApi.Level.Compute:
                    throw new NotImplementedException($"FieldValueExtensions.ToEnumerationsLevel unexpectedly received level[{level}]");
            }

            return Enumerations.Level.None;
        }

        /// <summary>
        /// Returns the Value (not Id/Element) as IshMetadataField
        /// </summary>
        /// <remarks>Note that separator comma-space is hardcoded. And that only the Value, not Element and not Id is retained which could lead to IMetadataBinding issues.</remarks>
        private static IshMetadataField FieldValueToIshMetadataField(OpenApi.FieldValue fieldValue)
        {
            IshMetadataField ishMetadataField = new IshMetadataField(
                fieldValue.IshField.Name, 
                fieldValue.IshField.Level.ToEnumerationsLevel(),
                GetFieldValueAsString(fieldValue));

            return ishMetadataField;
        }

        /// <summary>
        /// Converts a type-aware OpenApi.FieldValue into a string-based value for IshMetadataField usage
        /// </summary>
        /// <remarks>Note that separator comma-space is hardcoded. And that only the Value, not Element and not Id is retained which could lead to IMetadataBinding issues.</remarks>
        /// <param name="fieldValue">Type-aware OpenApi.FieldValue</param>
        /// <returns>Field value as string, for multi-value entries concatenated using comma-space as separator.</returns>
        private static string GetFieldValueAsString(OpenApi.FieldValue fieldValue)
        {
            string result = string.Empty;
            switch (fieldValue)
            {
                case OpenApi.CardFieldValue cardFieldValue:
                    result = cardFieldValue.Value?.Title;
                    break;

                case OpenApi.DateTimeFieldValue dateTimeFieldValue:
                    result = dateTimeFieldValue.Value?.ToString("u");
                    break;

                case OpenApi.LovFieldValue lovFieldValue:
                    result = lovFieldValue.Value?.Title;
                    break;

                case OpenApi.MultiCardFieldValue multiCardFieldValue:
                    result = string.Join(", ", multiCardFieldValue.Value?.Select(c => c.Title));
                    break;

                case OpenApi.MultiDateTimeFieldValue multiDateTimeFieldValue:
                    result = string.Join(", ", multiDateTimeFieldValue.Value?.Select(d => d.ToString("u")));
                    break;

                case OpenApi.MultiLovFieldValue multiLovFieldValue:
                    result = string.Join(", ", multiLovFieldValue.Value?.Select(l => l.Title));
                    break;

                case OpenApi.MultiNumberFieldValue multiNumberFieldValue:
                    result = string.Join(", ", multiNumberFieldValue.Value?.Select(n => n.ToString()));
                    break;

                case OpenApi.MultiStringFieldValue multiStringFieldValue:
                    result = string.Join(", ", multiStringFieldValue.Value);
                    break;

                case OpenApi.MultiTagFieldValue multiTagFieldValue:
                    result = string.Join(", ", multiTagFieldValue.Value?.Select(m => m.Title));
                    break;

                case OpenApi.MultiVersionFieldValue multiVersionFieldValue:
                    result = string.Join(", ", multiVersionFieldValue.Value);
                    break;

                case OpenApi.NumberFieldValue numberFieldValue:
                    result = numberFieldValue.Value?.ToString();
                    break;

                case OpenApi.StringFieldValue stringFieldValue:
                    result = stringFieldValue.Value;
                    break;

                case OpenApi.TagFieldValue tagFieldValue:
                    result = tagFieldValue.Value?.Title;
                    break;

                case OpenApi.VersionFieldValue versionFieldValue:
                    result = versionFieldValue.Value;
                    break;

                default:
                    break;
            }

            return result ?? string.Empty;
        }
    }
}
