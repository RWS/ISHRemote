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
using System.IO;
using Trisoft.ISHRemote.HelperClasses;
using Trisoft.ISHRemote.Objects.Public;

namespace Trisoft.ISHRemote.Objects
{
    /// <summary>
    /// <para type="description">Stores a collection of features called the context. All possible combinations will be used as input by ConditionFilter to make an @ishcondition true. For example combining BLUETOOTH=Y and GAMES=SNAKE</para>
    /// </summary>
    internal class IshFeatures
    {
        private List<IshFeature> _ishFeatures;

        /// <summary>
        /// Creates an empty instance of the  object.
        /// </summary>
        public IshFeatures()
        {
            _ishFeatures = new List<IshFeature>();
        }

        /// <summary>
        /// Creates an empty instance of the  object.
        /// </summary>
        public IshFeatures(IshFeature[] ishFeatures)
        {
            _ishFeatures = new List<IshFeature>(ishFeatures ?? new IshFeature[0]);
        }

        /// <summary>
        /// Creates a new instance of the ishFeatures object.
        /// </summary>
        /// <param name="xmlFeatures">Xml element with the feature information.</param>
        public IshFeatures(XmlElement xmlFeatures)
        {
            _ishFeatures = new List<IshFeature>();
            if (xmlFeatures != null)
            {
                foreach (XmlElement xmlFeature in xmlFeatures.SelectNodes("feature"))
                {
                    string name = xmlFeature.GetAttribute("name");
                    string value = xmlFeature.GetAttribute("value");
                    AddFeature(new IshFeature(name, value));
                }
            }
        }

        /// <summary>
        /// Gets the current IshFeatures.
        /// </summary>
        public IshFeature[] Features
        {
            get { return _ishFeatures.ToArray(); }
        }

        /// <summary>
        /// Number of ishFeatures.
        /// </summary>
        /// <returns>Returns the number of ishFeatures.</returns>
        public int Count()
        {
            return _ishFeatures.Count;
        }

        /// <summary>
        /// Add a ishFeature to the current list.
        /// </summary>
        /// <param name="feature">IshFeature that needs to be added.</param>
        /// <returns>The current list of <see cref="IshFeatures"/>.</returns>
        public IshFeatures AddFeature(IshFeature ishFeature)
        {
            _ishFeatures.Add(ishFeature);
            return this;
        }

        /// <summary>
        /// Write the xml from the current list.
        /// </summary>
        /// <param name="xmlWriter">The <see cref="XmlWriter"/>.</param>
        public virtual void GetXml(ref XmlWriter xmlWriter)
        { 
            xmlWriter.WriteStartElement("features");
            foreach (IshFeature ishFeature in _ishFeatures)
            {
                ishFeature.GetXml(ref xmlWriter);
            }
            xmlWriter.WriteEndElement();
        }

        /// <summary>
        /// Get an xml string from the current list;
        /// </summary>
        /// <returns>A string with the xml.</returns>
        public string ToXml()
        {
            if (Count() <= 0)
            {
                return "";
            }
            using (StringWriter stringWriter = new StringWriter())
            {
                XmlWriter xmlWriter;
                using (xmlWriter = XmlWriter.Create(stringWriter))
                {
                    GetXml(ref xmlWriter);
                }
                return stringWriter.ToString();
            }
        }

    }
}
