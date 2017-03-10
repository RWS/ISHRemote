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


        #region Assist functions on allowed field usage based on IshFieldDefinition[]

        // Dimensions
        //   Enumerations.Level.None, 
        //   removal of certain value types, or all like Enumerations.ValueType.All
        //   What: descriptive (inc FTITLE?), all


        private IshFields AddDescriptiveFields(Enumerations.ISHType[] ishTypes, IshFields ishFields, Enumerations.ActionMode actionMode)
        {
            return null;
        }

        /// <summary>
        /// Remove IshField entries that are not matching with the provided actionMode - based on IshFieldDefinition.AllowOn...
        /// </summary>
        private IshFields RemoveUnallowedActionFields(Enumerations.ISHType[] ishTypes, IshFields ishFields, Enumerations.ActionMode actionMode)
        {
            // 20170310/ddemeyer I wonder if we need this assist function...
            return null;
        }

        /// <summary>
        /// Requested metadata fields will be duplicated and enriched to cater our Public Objects initilization.
        /// The minimal fields (IsDescriptive) will be added to allow initialization of the various IshObject types for all ValueTypes. 
        /// Unallowed fields for read operations (IshFieldDefinition.AllowOnRead) will be stripped with a Debug log message.
        /// </summary>
        /// <remarks>Logs debug entry for unknown combinations of ishTypes and ishField (name, level) entries - will not throw.</remarks>
        /// <param name="ishTypes">Given ISHTypes (like ISHMasterDoc, ISHLibrary, ISHConfiguration,...) to verify/alter the IshFields for</param>
        /// <param name="ishFields">Incoming IshFields entries will be transformed to matching and allowed IshRequestedMetadataField entries</param>
        /// <param name="actionMode">RequestedMetadataFields only has value for read operations like Read, Find, Search,...</param>
        public IshFields ToIshRequestedMetadataFields(Enumerations.ISHType[] ishTypes, IshFields ishFields, Enumerations.ActionMode actionMode)
        {
            IshFields requestedMetadataFields = new IshFields();
            foreach (Enumerations.ISHType ishType in ishTypes)
            {
                // Check incoming IshField with IshTypeFieldDefinitions
                foreach (IshMetadataField ishField in ishFields.Fields())
                {
                    var key = Enumerations.Key(ishType, ishField.Level, ishField.Name);
                    if (!_ishTypeFieldDefinitions.ContainsKey(key))
                    {
                        _logger.WriteDebug($"ToIshRequestedMetadataFields unknown ishType[{ishType}] level[{ishField.Level}] name[{ishField.Name}] valueType[{ishField.ValueType}]");
                        continue;
                    }
                    switch (actionMode)
                    {
                        case Enumerations.ActionMode.Read:
                        case Enumerations.ActionMode.Find:
                            if (!_ishTypeFieldDefinitions[key].AllowOnRead)
                            {
                                _logger.WriteDebug($"ToIshRequestedMetadataFields AllowOnRead removed ishType[{ishType}] level[{ishField.Level}] name[{ishField.Name}] valueType[{ishField.ValueType}]");
                            }
                            else
                            {
                                requestedMetadataFields.AddField(ishField.ToRequestedMetadataField());
                            }
                            break;
                        case Enumerations.ActionMode.Search:
                            if (!_ishTypeFieldDefinitions[key].AllowOnSearch)
                            {
                                _logger.WriteDebug($"ToIshRequestedMetadataFields AllowOnSearch removed ishType[{ishType}] level[{ishField.Level}] name[{ishField.Name}] valueType[{ishField.ValueType}]");
                            }
                            else
                            {
                                requestedMetadataFields.AddField(ishField.ToRequestedMetadataField());
                            }
                            break;
                        default:
                            _logger.WriteDebug($"ToIshRequestedMetadataFields called for actionMode[{actionMode}], skipping");
                            break;
                    }
                }
                //TODO [Should] Add IsDescriptive fields for the incoming IshType to allow basic descriptive/minimal object initialization
                //Probably private assist function required in this class
                //Merges in IsDescriptive for all ValueTypes (for LOV/Card)... we cannot do IMetadataBinding fields yet
            }
            return requestedMetadataFields;
        }

        /// <summary>
        /// Metadata write operations will be duplicated and cleared client side.
        /// Unallowed fields for write operations (IshFieldDefinition.AllowOnCreate/AllowOnUpdate) will be stripped with a Debug log message.
        /// </summary>
        /// <remarks>Logs debug entry for unknown combinations of ishTypes, ishField (name, level), mandatory, multi-value,... entries - will not throw.</remarks>
        /// <param name="ishTypes">Given ISHTypes (like ISHMasterDoc, ISHLibrary, ISHConfiguration,...) to verify/alter the IshFields for</param>
        /// <param name="ishFields">Incoming IshFields entries will be transformed to matching and allowed IshMetadataField entries</param>
        /// <param name="actionMode">MetadataFields only has value for write operations like Create, Update,...</param>
        public IshFields ToIshMetadataFields(Enumerations.ISHType[] ishTypes, IshFields ishFields, Enumerations.ActionMode actionMode)
        {
            IshFields metadataFields = new IshFields();
            foreach (Enumerations.ISHType ishType in ishTypes)
            {
                foreach (IshMetadataField ishField in ishFields.Fields())
                {
                    var key = Enumerations.Key(ishType, ishField.Level, ishField.Name);
                    if (!_ishTypeFieldDefinitions.ContainsKey(key))
                    {
                        _logger.WriteDebug($"ToIshMetadataFields unknown ishType[{ishType}] level[{ishField.Level}] name[{ishField.Name}] valueType[{ishField.ValueType}]");
                        continue;
                    }
                    switch (actionMode)
                    {
                        case Enumerations.ActionMode.Create:
                            if (!_ishTypeFieldDefinitions[key].AllowOnCreate)
                            {
                                _logger.WriteDebug($"ToIshMetadataFields AllowOnCreate removed ishType[{ishType}] level[{ishField.Level}] name[{ishField.Name}] valueType[{ishField.ValueType}]");
                            }
                            else
                            {
                                metadataFields.AddField(ishField.ToMetadataField());
                            }
                            break;
                        case Enumerations.ActionMode.Update:
                            if (!_ishTypeFieldDefinitions[key].AllowOnUpdate)
                            {
                                _logger.WriteDebug($"ToIshMetadataFields AllowOnUpdate removed ishType[{ishType}] level[{ishField.Level}] name[{ishField.Name}]");
                            }
                            else
                            {
                                metadataFields.AddField(ishField.ToMetadataField());
                            }
                            break;
                        default:
                            _logger.WriteDebug($"ToIshMetadataFields called for actionMode[{actionMode}], skipping");
                            break;
                    }
                }
            }
            return metadataFields;
        }

        /// <summary>
        /// Correctly named overload of ToIshMetadataFields(...). Allows tuning in the future.
        /// </summary>
        public IshFields ToIshRequiredCurrentMetadataFields(Enumerations.ISHType[] ishTypes, IshFields ishFields, Enumerations.ActionMode actionMode)
        {
            return ToIshMetadataFields(ishTypes, ishFields, actionMode);
        }
        #endregion
    }
}
