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
using Trisoft.ISHRemote.HelperClasses;
using Trisoft.ISHRemote.Interfaces;
using Trisoft.ISHRemote.Objects.Public;

namespace Trisoft.ISHRemote.Objects
{
    /// <summary>
    /// <para type="description">Provides functionality on IshTypeFieldDefinitions</para>
    /// </summary>
    internal class IshTypeFieldSetup
    {
        private readonly ILogger _logger;
        /// <summary>
        /// Returning list of definitions
        /// </summary>
        private SortedDictionary<string, IshTypeFieldDefinition> _ishTypeFieldDefinitions;

        /// <summary>
        /// Creates a management object to work with the ISHType and FieldDefinitions.
        /// </summary>
        public IshTypeFieldSetup(ILogger logger, string xmlIshFieldSetup)
        {
            _logger = logger;
            _ishTypeFieldDefinitions = new SortedDictionary<string, IshTypeFieldDefinition>();
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xmlIshFieldSetup);
            foreach (XmlNode xmlIshTypeDefinition in xmlDocument.SelectNodes("ishfieldsetup/ishtypedefinition"))
            {
                Enumerations.ISHType ishType = (Enumerations.ISHType)Enum.Parse(typeof(Enumerations.ISHType), xmlIshTypeDefinition.Attributes.GetNamedItem("name").Value);
                _logger.WriteDebug($"IshTypeFieldSetup ishType[{ishType}]");
                foreach (XmlNode xmlIshFieldDefinition in xmlIshTypeDefinition.SelectNodes("ishfielddefinition"))
                { 
                    IshTypeFieldDefinition ishTypeFieldDefinition = new IshTypeFieldDefinition(_logger, ishType, (XmlElement)xmlIshFieldDefinition);
                    _ishTypeFieldDefinitions.Add(ishTypeFieldDefinition.Key, ishTypeFieldDefinition);
                }
            }
        }

        public IshTypeFieldSetup(ILogger logger, List<IshTypeFieldDefinition> ishTypeFieldDefinitions)
        {
            _logger = logger;
            _ishTypeFieldDefinitions = new SortedDictionary<string, IshTypeFieldDefinition>();
            foreach (IshTypeFieldDefinition ishTypeFieldDefinition in ishTypeFieldDefinitions)
            {
                //Make sure the type, level (logical before version before lng), fieldname sorting is there
                _ishTypeFieldDefinitions.Add(ishTypeFieldDefinition.Key, ishTypeFieldDefinition);
            }
        }

        internal List<IshTypeFieldDefinition> IshTypeFieldDefinition
        {
            get
            {
                return _ishTypeFieldDefinitions.Values.ToList<IshTypeFieldDefinition>();
            }
        }

        public IshTypeFieldDefinition GetValue(string key)
        {
            IshTypeFieldDefinition ishTypeFieldDefinition;
            if (_ishTypeFieldDefinitions.TryGetValue(key, out ishTypeFieldDefinition))
            {
                return ishTypeFieldDefinition;
            }
            return null;
        }
    }
}
