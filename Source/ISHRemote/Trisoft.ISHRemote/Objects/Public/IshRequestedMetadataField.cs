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
    /// <para type="description">Stores field name and level information via IshField.</para>
    /// </summary>
    public class IshRequestedMetadataField : IshField
    {
    
        /// <summary>
        /// Constructs a Set IshField
        /// </summary>
        public IshRequestedMetadataField(string fieldName, Enumerations.Level fieldLevel, Enumerations.ValueType valueType)
            : base(fieldName, fieldLevel, valueType)
        {
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        public IshRequestedMetadataField(IshRequestedMetadataField ishRequestedMetadataField)
            : base(ishRequestedMetadataField._fieldName, ishRequestedMetadataField._fieldLevel, ishRequestedMetadataField._valueType)
        {
        }

        /// <summary>
        /// Debugging implementation
        /// </summary>
        /// <returns>string</returns>
        public override string ToString()
        {
            return $"Set-IshRequestedMetadataField -Level {StringEnum.GetStringValue(Level)} -Name {Name} -ValueType {StringEnum.GetStringValue(_valueType)}";
            //return $"<ishfield name='{Name}' level='{StringEnum.GetStringValue(Level)}' ishvaluetype='{StringEnum.GetStringValue(_valueType)}'></ishfield>";
        }
    }
}
