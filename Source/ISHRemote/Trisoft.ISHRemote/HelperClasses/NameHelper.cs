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
using System.Globalization;
using System.Linq;
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using Trisoft.ISHRemote.Objects;
using Trisoft.ISHRemote.Objects.Public;

namespace Trisoft.ISHRemote.HelperClasses
{
    // Inspired by
    // . https://github.com/PowerShell/PowerShell/blob/master/src/System.Management.Automation/engine/MshMemberInfo.cs
    // . https://docs.microsoft.com/en-us/dotnet/api/system.management.automation.psvariable?view=powershellsdk-1.1.0
    internal class NameHelper
    {
        private readonly IshSession _ishSession;
        private readonly string _levelNameValueTypeSeparator = "_";  // Not that many special characters allowed in Properties, e.g. '=' is assignment
        // TODO [Could] NameHelper PSNoteProperty generator could benefit of fast-lookup dictionary to store DataType and PropertyName as almost all IshObjects start from the same requested metadata request...

        public NameHelper(IshSession ishSession)
        {
            _ishSession = ishSession;
        }

        /// <summary>
        /// Returns a PSNoteProperty (PSVariable doesn't allow sortable date)
        /// * Naming convention is lowercase using '=' as separator
        /// * Levels Lng, None, Task, Progress for ValueType Value are not part of the property name. All the rest is explicit
        /// Examples
        /// * e.g. DocumentObj... fauthor, fauthor=lng=value, fauthor=lng=element … modified-on, modified-on=lng=value, modified-on=ver=value, modified-on=log=value
        /// * e.g. User... level-None types… username, username=none=value, username=none=element
        /// * e.g. Events... not a card type… userid=progress=value
        /// </summary>
        /// <remarks>PSNoteProperty: date fields converting dd/MM/yyyy HH:mm:ss to a sortable format; so ISO8601 (ToString('s', dt)) yyyy'-'MM'-'dd'T'HH':'mm':'ss 
        /// (similar format 'u' add time zone which I consider non-scope for now). Note that TranslationJob's LEASEDON is returned by the API in Utc.</remarks>
        public PSNoteProperty GetPSNoteProperty(Enumerations.ISHType[] ishTypes, IshMetadataField ishField)
        {
            foreach (var ishType in ishTypes)
            { 
                StringBuilder propertyName = new StringBuilder();
                switch (ishType)
                {
                    default:
                        switch (ishField.Level)
                        {
                            case Enumerations.Level.Lng:
                            case Enumerations.Level.None:
                            case Enumerations.Level.Task:
                            case Enumerations.Level.Progress:
                                // Incoming field "CHECK-OUT" should become "checkout" otherwise PowerShell will enforce single quote around so $ishObject.'check-out'
                                propertyName.Append(ishField.Name.Replace("-","").ToLower());
                                switch (ishField.ValueType)
                                {
                                    case Enumerations.ValueType.Element:
                                    case Enumerations.ValueType.Id:
                                        propertyName.Append(_levelNameValueTypeSeparator);
                                        propertyName.Append(ishField.Level.ToString().ToLower());
                                        propertyName.Append(_levelNameValueTypeSeparator);
                                        propertyName.Append(ishField.ValueType.ToString().ToLower());
                                        break;
                                    default:
                                        break;
                                }
                                break;
                            default:
                                propertyName.Append(ishField.Name.Replace("-", "").ToLower());
                                propertyName.Append(_levelNameValueTypeSeparator);
                                propertyName.Append(ishField.Level.ToString().ToLower());
                                propertyName.Append(_levelNameValueTypeSeparator);
                                propertyName.Append(ishField.ValueType.ToString().ToLower());
                                break;
                        }
                        break;
                }

                string propertyValue = "";
                Enumerations.DataType dataType = _ishSession.IshTypeFieldSetup.GetDataType(ishType, ishField);
                switch (dataType)
                {
                    // date fields converting dd/MM/yyy HH:mm:ss to a sortable format; either proprietary yyyyMMdd.HHmmss 
                    // or ISO8601 (ToString('s', dt)) yyyy-MM-ddTHH:mm:ss (similar format 'u' add time zone which I consider non-scope for now)
                    // Note that TranslationJob's LEASEDON is returned by the API in Utc.
                    case Enumerations.DataType.DateTime:
                        //var formatStrings = new string[] { "dd/MM/yyy HH:mm:ss", "yyyy-MM-dd hh:mm:ss", "dd/MM/yyy" };
                        DateTime dateTime;
                        if (DateTime.TryParse(ishField.Value, out dateTime))
                        {
                            propertyValue = dateTime.ToString("s");
                        }
                        else
                        {
                            propertyValue = ishField.Value;
                        }
                        break;
                    default:
                        propertyValue = ishField.Value;
                        break;

                }

                return new PSNoteProperty(propertyName.ToString(), propertyValue);
            }

            return new PSNoteProperty("zzzNameHelperError", $"ISHType[{ishTypes}] Level[{ishField.Level.ToString()}] Name[{ishField.Name}] is unknown");
        }

        // Potentially make FileNameHelper static methods public here...
    }
}
