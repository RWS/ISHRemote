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
using Trisoft.ISHRemote.HelperClasses;
using System.Xml;

namespace Trisoft.ISHRemote.Objects.Public
{
    /// <summary>
    /// <para type="description">Stores reference for "ishlovvalueref", "ishref", "label", "description", "active"</para>
    /// </summary>
    public class IshLovValue
    {
        /*IshLovValue looks like:
         
         <ishlovvalue ishlovvalueref="5240" ishref="VILLUSTRATIONTYPEDIAGRAM">
           <label xml:space="preserve">Diagram</label>
           <description xml:space="preserve">Diagram</description>
           <active xml:space="preserve">true</active>
          </ishlovvalue>
        */

        private string _lovId;
        private long _ishLovValueRef;
        private string _ishRef;
        private string _label;
        private string _description;
        private bool _active;

        /// <summary>
        /// IshLovValue creation through an ishlovvalue xml element.
        /// </summary>
        /// <param name="lovId">Id of a list of values e.g. DERESOLUTION</param>
        /// <param name="xmlIshLovValue">One ishlovvalue xml.</param>
        public IshLovValue(string lovId, XmlElement xmlIshLovValue)
        {
            _ishLovValueRef = -1;
            _lovId = lovId;
            long.TryParse(xmlIshLovValue.Attributes["ishlovvalueref"].Value, out _ishLovValueRef);
            _ishRef = xmlIshLovValue.Attributes["ishref"].Value;
            XmlNode xmlLabel = xmlIshLovValue.SelectSingleNode("label");
            _label = xmlLabel.InnerText;
            XmlNode xmlDescription = xmlIshLovValue.SelectSingleNode("description");
            _description = xmlDescription.InnerText;
            XmlNode xmlActive = xmlIshLovValue.SelectSingleNode("active");
            _active = false;
            bool.TryParse( xmlActive.InnerText, out _active);
        }

        /// <summary>
        /// Gets the Id of a list of values e.g. DRESOLUTION
        /// </summary>
        public string LovId
        {
            get { return _lovId; }
        }

        /// <summary>
        /// Gets ishlovvalueref property of the LovValue
        /// </summary>
        public long IshLovValueRef
        {
            get { return _ishLovValueRef; }
        }

        /// <summary>
        /// Gets ishRef property of the LovValue
        /// </summary>
        public string IshRef
        {
            get { return _ishRef; }
        }

        /// <summary>
        /// Gets label field of the LovValue
        /// </summary>
        public string Label
        {
            get { return _label; }
        }

        /// <summary>
        /// Gets description field of the LovValue
        /// </summary>
        public string Description
        {
            get { return _description; }
        }

        public bool Active
        {
            get { return _active; }
        }

        public string ActiveAsString
        {
            get { switch(_active)
                  { 
                    case true:
                        return "Yes";
                    case false:
                    default:
                        return "No";
                  }
            }
        }

    }
}
