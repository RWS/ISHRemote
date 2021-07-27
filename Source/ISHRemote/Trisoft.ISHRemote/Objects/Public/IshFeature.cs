/*
* Copyright © 2014 All Rights Reserved by the RWS Group for and on behalf of its affiliates and subsidiaries.
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
    /// <para type="description">Stores a single feature part of the context that will be used as input by ConditionFilter to make an @ishcondition true. For example BLUETOOTH=Y</para>
    /// </summary>
    public class IshFeature
    {
        /* Example of features:
        <features>
	        <feature name="productserie" value="45CF"/>
	        <feature name="productserie" value="45XF"/>
	        <feature name="manufacturing date" value="20020613"/>
	        <feature name="airco" value="1"/>
        </features>
         */
        
        private string _name;
        private string _value;

        protected IshFeature()
        {
            _name = "";
            _value = "";
        }

        public IshFeature(string name, string value)
        {
            _name = name ?? "";
            _value = value ?? "";
        }


        /// <summary>
        /// Gets name of the feature
        /// </summary>
        public string Name
        {
            get { return _name; }
        }

        /// <summary>
        /// Gets value of the feature
        /// </summary>
        public string Value
        {
            get { return _value; }
        }

        /// <summary>
        /// Gets xml for the feature object
        /// </summary>
        /// <param name="xmlWriter"></param>
        public virtual void GetXml(ref XmlWriter xmlWriter)
        {
            xmlWriter.WriteStartElement("feature");
            xmlWriter.WriteAttributeString("name", Name);
            xmlWriter.WriteAttributeString("value", Value);
            xmlWriter.WriteEndElement();
        }

    }
}
