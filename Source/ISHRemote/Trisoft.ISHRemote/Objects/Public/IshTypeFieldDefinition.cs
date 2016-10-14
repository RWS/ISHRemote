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

namespace Trisoft.ISHRemote.Objects.Public
{
    /// <summary>
    /// <para type="description">Object holding the denormalized ISHType (like ISHMasterDoc, ISHEvent,...) with field information (like FTITLE, PROGRESS,..).</para>
    /// </summary>
    /// <example>
    /// <ishtypedefinition name="ISHUser">
    /// <ishfielddefinition level="none" name="FUSERGROUP" type="ishreference" ismandatory="false" ismultivalue="true"
    ///     allowonread="true" allowoncreate="true" allowonupdate="true" allowonsearch="true"
    ///     issystem="true" isbasic="true" isdescriptive="false">
    ///     <label>Usergroup</label>
    ///     <description>Used on all card types.On the USER card type the field indicates that the user has write/modify access to documents of this usergroup.On all other objects the field contains the usergroup that owns the object and can modify the object.</description>
    ///     <ishreference>
    ///         <ishtype ishref = "ISHUserGroup" />
    ///     </ishreference>
    /// </ishfielddefinition>
    /// ...
    /// </ishtypedefinition>
    /// </example>
    public class IshTypeFieldDefinition : IComparable
    {
        protected readonly ILogger _logger;
        public Enumerations.ISHType ISHType { get; internal set; }
        public Enumerations.Level Level { get; internal set; }
        public string Name { get; internal set; }
        public Enumerations.DataType DataType { get; internal set; }
        public bool IsMandatory { get; internal set; }
        public bool IsMultiValue { get; internal set; }
        public bool AllowOnRead { get; internal set; }
        public bool AllowOnCreate { get; internal set; }
        public bool AllowOnUpdate { get; internal set; }
        public bool AllowOnSearch { get; internal set; }
        public bool IsSystem { get; internal set; }
        public bool IsBasic { get; internal set; }
        public bool IsDescriptive { get; internal set; }
        public string Label { get; internal set; }
        public string Description { get; internal set; }
        public List<Enumerations.ISHType> ReferenceType { get; internal set; }
        public string ReferenceLov { get; internal set; }


        /// <summary>
        /// PS1XML Shorthand notation of DataType-ReferenceType-ReferenceLov properties
        /// </summary>
        public string Type
        {
            get
            {
                switch (DataType)
                {
                    case Enumerations.DataType.ISHLov:
                        return ReferenceLov;
                    case Enumerations.DataType.ISHType:
                        // return sorted to avoid string compare issues when database seq differed
                        return string.Join(",",ReferenceType.OrderBy(q => q).ToList());
                    default:
                        return DataType.ToString();
                }
            }
        }

        /// <summary>
        /// PS1XML Shorthand notation of Mandatory-MultiValue properties
        /// </summary>
        public string MM
        {
            get
            {
                StringBuilder crus = new StringBuilder();
                crus.Append(IsMandatory ? 'M' : '-');
                crus.Append(IsMultiValue ? 'n' : '1');
                crus.Append(AllowOnUpdate ? 'U' : '-');
                crus.Append(AllowOnSearch ? 'S' : '-');
                return crus.ToString();
            }
        }

        /// <summary>
        /// PS1XML Shorthand notation of Create-Read-Update-Search properties
        /// </summary>
        public string CRUS
        {
            get
            {
                StringBuilder crus = new StringBuilder();
                crus.Append(AllowOnCreate ? 'C' : '-');
                crus.Append(AllowOnRead ? 'R' : '-');
                crus.Append(AllowOnUpdate ? 'U' : '-');
                crus.Append(AllowOnSearch ? 'S' : '-');
                return crus.ToString();
            }
        }

        /// <summary>
        /// PS1XML Shorthand notation of System-Descriptive-Basic
        /// </summary>
        public string SDB
        {
            get
            {
                StringBuilder sdb = new StringBuilder();
                sdb.Append(IsSystem ? 'S' : '-');
                sdb.Append(IsDescriptive ? 'D' : '-');
                sdb.Append(IsBasic ? 'B' : '-');
                return sdb.ToString();
            }
        }

        /// <summary>
        /// Unique descriptive identifier of an IshTypeFieldDefinition concatenating type, level (respecting log/version/lng), and field name
        /// </summary>
        internal string Key
        {
            get
            {
                return ISHType + "=" + (int)Level + Level + "=" + Name;
            }
        }

        /// <summary>
        /// IshTypeFieldDefinition creation through an xml element. See Settings25.RetrieveFieldSetupByIshType
        /// </summary>
        /// <param name="ishType">Card type identifier</param>
        /// <param name="xmlIshTypeFieldDefinition">One IshTypeFieldDefinition xml.</param>
        internal IshTypeFieldDefinition(ILogger logger, Enumerations.ISHType ishType, XmlElement xmlDef)
        {
            _logger = logger;
            ISHType = ishType;
            Level = (Enumerations.Level)StringEnum.Parse(typeof(Enumerations.Level), xmlDef.Attributes["level"].Value);
            Name = xmlDef.Attributes["name"].Value;
            IsMandatory = Boolean.Parse(xmlDef.Attributes["ismandatory"].Value);
            IsMultiValue = Boolean.Parse(xmlDef.Attributes["ismultivalue"].Value);
            AllowOnRead = Boolean.Parse(xmlDef.Attributes["allowonread"].Value);
            AllowOnCreate = Boolean.Parse(xmlDef.Attributes["allowoncreate"].Value);
            AllowOnUpdate = Boolean.Parse(xmlDef.Attributes["allowonupdate"].Value);
            AllowOnSearch = Boolean.Parse(xmlDef.Attributes["allowonsearch"].Value);
            IsSystem = Boolean.Parse(xmlDef.Attributes["issystem"].Value);
            IsBasic = Boolean.Parse(xmlDef.Attributes["isbasic"].Value);
            IsDescriptive = Boolean.Parse(xmlDef.Attributes["isdescriptive"].Value);
            Label = xmlDef.SelectSingleNode("label").InnerText;
            Description = xmlDef.SelectSingleNode("description").InnerText;
            ReferenceLov = "";
            ReferenceType = new List<Enumerations.ISHType>();

            string type = xmlDef.Attributes["type"].Value;
            switch (type)
            {
                case "ishreference":
                    if (xmlDef.SelectSingleNode("ishreference/ishlov") != null)
                    {
                        DataType = Enumerations.DataType.ISHLov;
                        ReferenceLov = xmlDef.SelectSingleNode("ishreference/ishlov").Attributes["ishref"].Value;
                    }
                    else if (xmlDef.SelectSingleNode("ishreference") != null)
                    {
                        DataType = Enumerations.DataType.ISHType;
                        foreach (XmlNode xmlNode in xmlDef.SelectSingleNode("ishreference").SelectNodes("ishtype"))
                        {
                            ReferenceType.Add((Enumerations.ISHType)Enum.Parse(typeof(Enumerations.ISHType), xmlNode.Attributes["ishref"].Value));
                        }
                    }
                    else
                    {
                        DataType = Enumerations.DataType.ISHLov;
                        ReferenceLov = "MISSINGISHREFERENCE1";
                    }
                    break;
                case "longtext":
                    DataType = Enumerations.DataType.LongText;
                    break;
                case "number":
                    DataType = Enumerations.DataType.Number;
                    break;
                case "datetime":
                    DataType = Enumerations.DataType.DateTime;
                    break;
                case "string":
                    DataType = Enumerations.DataType.String;
                    break;
                default:
                    // something went wrong
                    DataType = Enumerations.DataType.ISHLov;
                    ReferenceLov = "MISSINGISHREFERENCE2";
                    break;
            }
        }

        /// <summary>
        /// IshTypeFieldDefinition creation with the bare descriptive identifiers, defaulting values to AllowOnRead only
        /// </summary>
        /// <param name="ishType">Card type identifier</param>
        /// <param name="level">The level of the field on this ISHType (card type)</param>
        /// <param name="name">The name of the field</param>
        /// <param name="dataType">The field data type, indicating reference field or simple type</param>
        internal IshTypeFieldDefinition(ILogger logger, Enumerations.ISHType ishType, Enumerations.Level level, string name, Enumerations.DataType dataType)
        {
            _logger = logger;
            ISHType = ishType;
            Level = level;
            Name = name;
            DataType = dataType;
            IsMandatory = false;
            IsMultiValue = false;
            AllowOnRead = true;
            AllowOnCreate = false;
            AllowOnUpdate = false;
            AllowOnSearch = false;
            IsSystem = false;
            IsBasic = false;
            IsDescriptive = false;
            Label = "";
            Description = "";
            ReferenceLov = "";
            ReferenceType = new List<Enumerations.ISHType>();
        }

        /// <summary>
        /// Copy constructor
        /// </summary>
        internal IshTypeFieldDefinition(IshTypeFieldDefinition ishTypeFieldDefinition)
        {
            _logger = ishTypeFieldDefinition._logger;
            ISHType = ishTypeFieldDefinition.ISHType;
            Level = ishTypeFieldDefinition.Level;
            Name = ishTypeFieldDefinition.Name;
            DataType = ishTypeFieldDefinition.DataType;
            IsMandatory = ishTypeFieldDefinition.IsMandatory;
            IsMultiValue = ishTypeFieldDefinition.IsMultiValue;
            AllowOnRead = ishTypeFieldDefinition.AllowOnRead;
            AllowOnCreate = ishTypeFieldDefinition.AllowOnCreate;
            AllowOnUpdate = ishTypeFieldDefinition.AllowOnUpdate;
            AllowOnSearch = ishTypeFieldDefinition.AllowOnSearch;
            IsSystem = ishTypeFieldDefinition.IsSystem;
            IsBasic = ishTypeFieldDefinition.IsBasic;
            IsDescriptive = ishTypeFieldDefinition.IsDescriptive;
            Label = ishTypeFieldDefinition.Label;
            Description = ishTypeFieldDefinition.Description;
            ReferenceLov = ishTypeFieldDefinition.ReferenceLov;
            ReferenceType = ishTypeFieldDefinition.ReferenceType;
        }

        /// <summary>
        /// The role of IComparable is to provide a method of comparing two objects of a particular type. This is necessary if you want to provide any ordering capability for your object.
        /// </summary>
        public int CompareTo(object obj)
        {
            IshTypeFieldDefinition b = (IshTypeFieldDefinition)obj;
            if (!Key.Equals(b.Key, StringComparison.InvariantCulture))
            {
                return string.Compare(Key, b.Key);
            }
            // Keys match, now check the properties that matter
            if (!Type.Equals(b.Type, StringComparison.InvariantCulture))
            {
                _logger.WriteVerbose($"IshTypeFieldDefinition.CompareTo a.Key[{Key}] a.Type[{Type}] b.Type[{b.Type}]");
                return string.Compare(Type, b.Type);
            }
            if (!MM.Equals(b.MM, StringComparison.InvariantCulture))
            {
                _logger.WriteVerbose($"IshTypeFieldDefinition.CompareTo a.Key[{Key}] a.MM[{MM}] b.MM[{b.MM}]");
                return string.Compare(MM, b.MM, StringComparison.InvariantCulture);
            }
            if (!CRUS.Equals(b.CRUS, StringComparison.InvariantCulture))
            {
                _logger.WriteVerbose($"IshTypeFieldDefinition.CompareTo a.Key[{Key}] a.CRUS[{CRUS}] b.CRUS[{b.CRUS}]");
                return string.Compare(CRUS, b.CRUS, StringComparison.InvariantCulture);
            }
            if (!SDB.Equals(b.SDB, StringComparison.InvariantCulture))
            {
                _logger.WriteVerbose($"IshTypeFieldDefinition.CompareTo a.Key[{Key}] a.SDB[{SDB}] b.SDB[{b.SDB}]");
                return string.Compare(SDB, b.SDB, StringComparison.InvariantCulture);
            }
            if (!Description.Equals(b.Description, StringComparison.InvariantCulture))
            {
                _logger.WriteDebug($"IshTypeFieldDefinition.CompareTo a.Key[{Key}] a.Description[{Description}] b.Description[{b.Description}]");
                return 0;  // Difference in description is nice-to-know but considered equal
            }
            return 0;
        }
    }
}
