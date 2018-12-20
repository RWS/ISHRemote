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
using System.Threading.Tasks;
using System.Xml;
using Trisoft.ISHRemote.Objects.Public;

namespace Trisoft.ISHRemote.Objects
{
    /// <summary>
    /// Factory to generate the most specific ISHType possible, so ISHDocumentObj over ISHObject, or ISHUser over ISHObject
    /// </summary>
    static class IshObjectFactory
    {
        /// <summary>
        /// Simplest factory is to make the cmdlet in question (like Find-IshDocumentObj) mention which type its return. 
        /// Typically in the cmdlet name.
        /// </summary>
        public static IshObject Get(Enumerations.ISHType[] ishType, XmlElement xmlIshObject)
        {
            switch (ishType[0]) // any of the ISHDocumentObj types would do
            {
                case Enumerations.ISHType.ISHIllustration:
                case Enumerations.ISHType.ISHLibrary:
                case Enumerations.ISHType.ISHModule:
                case Enumerations.ISHType.ISHMasterDoc:
                case Enumerations.ISHType.ISHTemplate:
                    return new IshDocumentObj(xmlIshObject);
                case Enumerations.ISHType.ISHPublication:
                    return new IshPublicationOutput(xmlIshObject);
                case Enumerations.ISHType.ISHBaseline:
                    return new IshBaseline(xmlIshObject);
                case Enumerations.ISHType.ISHEDT:
                    return new IshEDT(xmlIshObject);
                case Enumerations.ISHType.ISHOutputFormat:
                    return new IshOutputFormat(xmlIshObject);
                default:
                    throw new ArgumentException($"IshObjectFactory ishtype[{ishType}] is unknown");
            }
        }
    }
}
