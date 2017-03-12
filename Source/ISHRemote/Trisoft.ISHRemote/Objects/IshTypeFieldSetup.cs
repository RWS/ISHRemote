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
        /// Lookup dictionary based on field identifier (like ISHType, Level, Name concatenation)
        /// </summary>
        private SortedDictionary<string, IshTypeFieldDefinition> _ishTypeFieldDefinitions;
        /// <summary>
        /// Client side filtering of nonexisting or unallowed metadata can be done silently, with warning or not at all.
        /// </summary>
        private Enumerations.StrictMetadataPreference _strictMetadataPreference = Enumerations.StrictMetadataPreference.Continue;

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

        /// <summary>
        /// Client side filtering of nonexisting or unallowed metadata can be done silently, with warning or not at all. 
        /// </summary>
        public Enumerations.StrictMetadataPreference StrictMetadataPreference
        {
            get { return _strictMetadataPreference; }
            set { _strictMetadataPreference = value; }
        }


        #region Assist functions on allowed field usage based on IshFieldDefinition[]

        // Dimensions
        //   Enumerations.Level.None, 
        //   removal of certain value types, or all like Enumerations.ValueType.All
        //   What: descriptive (inc FTITLE?), all
        /// <summary>
        /// Remove IshField entries that are not matching with the provided actionMode - based on IshFieldDefinition.AllowOn...
        /// </summary>
        private IshFields RemoveUnallowedActionFields(Enumerations.ISHType[] ishTypes, IshFields ishFields, Enumerations.ActionMode actionMode)
        {
            // 20170310/ddemeyer I wonder if we need this assist function...
            return null;
        }

        /// <summary>
        /// Reverse lookup for all fields that are marked descriptive for the incoming ishTypes; then add them to ishFields.
        /// </summary>
        private IshFields AddDescriptiveFields(Enumerations.ISHType[] ishTypes, IshFields ishFields, Enumerations.ActionMode actionMode)
        {
            foreach (Enumerations.ISHType ishType in ishTypes)
            {
                foreach (var ishTypeFieldDefinition in _ishTypeFieldDefinitions.Values.Where(d => d.ISHType == ishType && d.IsDescriptive == true))
                {
                    //TODO [Could] IshTypeFieldSetup adding descriptive fields potentially has an issue with removing to many ValueType entries
                    ishFields.AddOrUpdateField(new IshRequestedMetadataField(ishTypeFieldDefinition.Name, ishTypeFieldDefinition.Level, Enumerations.ValueType.Value));
                }
            }
            return ishFields;
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
                foreach (IshField ishField in ishFields.Fields())
                {
                    var key = Enumerations.Key(ishType, ishField.Level, ishField.Name);
                    if (!_ishTypeFieldDefinitions.ContainsKey(key))
                    {
                        switch (_strictMetadataPreference)
                        {
                            case Enumerations.StrictMetadataPreference.SilentlyContinue:
                                _logger.WriteDebug($"ToIshRequestedMetadataFields skipping unknown ishType[{ishType}] level[{ishField.Level}] name[{ishField.Name}] valueType[{ishField.ValueType}]");
                                break;
                            case Enumerations.StrictMetadataPreference.Continue:
                                _logger.WriteVerbose($"ToIshRequestedMetadataFields skipping unknown ishType[{ishType}] level[{ishField.Level}] name[{ishField.Name}] valueType[{ishField.ValueType}]");
                                break;
                            case Enumerations.StrictMetadataPreference.Off:
                                requestedMetadataFields.AddField(ishField.ToRequestedMetadataField());
                                break;
                        }
                        continue; // move to next ishField
                    }
                    switch (actionMode)
                    {
                        case Enumerations.ActionMode.Read:
                        case Enumerations.ActionMode.Find:
                            if (!_ishTypeFieldDefinitions[key].AllowOnRead)
                            {
                                switch (_strictMetadataPreference)
                                {
                                    case Enumerations.StrictMetadataPreference.Continue:
                                        _logger.WriteVerbose($"ToIshRequestedMetadataFields AllowOnRead removed ishType[{ishType}] level[{ishField.Level}] name[{ishField.Name}] valueType[{ishField.ValueType}]");
                                        break;
                                }
                            }
                            else
                            {
                                requestedMetadataFields.AddField(ishField.ToRequestedMetadataField());
                            }
                            break;
                        case Enumerations.ActionMode.Search:
                            if (!_ishTypeFieldDefinitions[key].AllowOnSearch)
                            {
                                switch (_strictMetadataPreference)
                                {
                                    case Enumerations.StrictMetadataPreference.Continue:
                                        _logger.WriteVerbose($"ToIshRequestedMetadataFields AllowOnSearch removed ishType[{ishType}] level[{ishField.Level}] name[{ishField.Name}] valueType[{ishField.ValueType}]");
                                        break;
                                }
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
                //Add IsDescriptive fields for the incoming IshType to allow basic descriptive/minimal object initialization
                requestedMetadataFields = AddDescriptiveFields(ishTypes, requestedMetadataFields, actionMode);
                //TODO [Should] Merges in IsDescriptive for all ValueTypes (for LOV/Card)... we cannot do IMetadataBinding fields yet. Server-side they are retrieved anyway, so the only penalty is xml transfer size.
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
                foreach (IshField ishField in ishFields.Fields())
                {
                    var key = Enumerations.Key(ishType, ishField.Level, ishField.Name);
                    if (!_ishTypeFieldDefinitions.ContainsKey(key))
                    {
                        switch (_strictMetadataPreference)
                        {
                            case Enumerations.StrictMetadataPreference.SilentlyContinue:
                                _logger.WriteDebug($"ToIshMetadataFields skipping unknown ishType[{ishType}] level[{ishField.Level}] name[{ishField.Name}] valueType[{ishField.ValueType}]");
                                break;
                            case Enumerations.StrictMetadataPreference.Continue:
                                _logger.WriteVerbose($"ToIshMetadataFields skipping unknown ishType[{ishType}] level[{ishField.Level}] name[{ishField.Name}] valueType[{ishField.ValueType}]");
                                break;
                            case Enumerations.StrictMetadataPreference.Off:
                                metadataFields.AddField(ishField.ToRequestedMetadataField());
                                break;
                        }
                        continue; // move to next ishField
                    }
                    switch (actionMode)
                    {
                        case Enumerations.ActionMode.Create:
                            if (!_ishTypeFieldDefinitions[key].AllowOnCreate)
                            {
                                switch (_strictMetadataPreference)
                                {
                                    case Enumerations.StrictMetadataPreference.Continue:
                                        _logger.WriteVerbose($"ToIshMetadataFields AllowOnCreate removed ishType[{ishType}] level[{ishField.Level}] name[{ishField.Name}] valueType[{ishField.ValueType}]");
                                        break;
                                }
                            }
                            else
                            {
                                metadataFields.AddField(ishField.ToMetadataField());
                            }
                            break;
                        case Enumerations.ActionMode.Update:
                            if (!_ishTypeFieldDefinitions[key].AllowOnUpdate)
                            {
                                switch (_strictMetadataPreference)
                                {
                                    case Enumerations.StrictMetadataPreference.Continue:
                                        _logger.WriteVerbose($"ToIshMetadataFields AllowOnUpdate removed ishType[{ishType}] level[{ishField.Level}] name[{ishField.Name}]");
                                        break;
                                }
                            }
                            else
                            {
                                metadataFields.AddField(ishField.ToMetadataField());
                                //TODO [Should] IshTypeFieldSetup - Potential conflict if ishField having multiple ishvaluetype have conflicting entries for id/element/value
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
