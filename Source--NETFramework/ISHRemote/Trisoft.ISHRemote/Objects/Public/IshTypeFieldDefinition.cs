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
                return Enumerations.Key(ISHType, Level, Name);
            }
        }

        /// <summary>
        /// IshTypeFieldDefinition creation through an xml element. See Settings25.RetrieveFieldSetupByIshType
        /// </summary>
        /// <param name="logger">Instance of the ILogger interface to allow some logging although Write-* is not very thread-friendly.</param>
        /// <param name="ishType">Card type identifier</param>
        /// <param name="xmlDef">One IshTypeFieldDefinition xml.</param>
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
        /// <param name="logger">Instance of the ILogger interface to allow some logging although Write-* is not very thread-friendly.</param>
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
        /// IshTypeFieldDefinition creation with the full descriptive identifiers
        /// </summary>
        /// <param name="logger">Instance of the ILogger interface to allow some logging although Write-* is not very thread-friendly.</param>
        /// <param name="ishType">Card type identifier</param>
        /// <param name="level">The level of the field on this ISHType (card type)</param>
        /// <param name="isMandatory">Boolean attribute indicating whether the field is mandatory or not. </param>
        /// <param name="isMultiValue">Boolean attribute indicating whether the field can contain multiple values or not. </param>
        /// <param name="allowOnRead">Boolean attribute indicating whether the field can be passed as filter or passed as requested metadata to an API READ method (e.g. GetMetadata, RetrieveMetadata, Find,...). </param>
        /// <param name="allowOnCreate">Boolean attribute indicating whether the field can be set via metadata by an API CREATE method.  Note: Some fields(e.g.USERNAME) must be passed as a parameter to the CREATE method.So, although these fields are mandatory, they will have allowoncreate false! </param>
        /// <param name="allowOnUpdate">Boolean attribute indicating whether the field can be set via metadata by an API UPDATE method (e.g. SetMetadata, Update,...). </param>
        /// <param name="allowOnSearch">Boolean attribute indicating whether the field is part of the full text index and can be used as part of the search query. </param>
        /// <param name="isSystem">Boolean attribute indicating whether this field is part of the internal Content Manager business logic. </param>
        /// <param name="isBasic">Boolean attribute indicating whether this field is a basic field (e.g. FSTATUS) or a more advanced field (e.g. FISHSTATUSTYPE). </param>
        /// <param name="isDescriptive">Boolean attribute indicating whether this field is one of the fields that define an object. Note: These fields are also used by the internal Content Manager business code, therefore they don't require an extra call to the database when requested. </param>
        /// <param name="name">Name of the card field or the table column.</param>
        /// <param name="dataType">The field data type, indicating reference field or simple type</param>
        /// <param name="referenceLov">Lists the referenced list of values name (e.g. USERNAME or DBACKGROUNDTASKSTATUS)</param>
        /// <param name="description">Free text description, anything which can help an implementor</param>
        internal IshTypeFieldDefinition(ILogger logger, Enumerations.ISHType ishType, Enumerations.Level level,
            bool isMandatory, bool isMultiValue, bool allowOnRead, bool allowOnCreate, bool allowOnUpdate, bool allowOnSearch, bool isSystem, bool isBasic, bool isDescriptive,
            string name, Enumerations.DataType dataType, string referenceLov, string description)
        {
            _logger = logger;
            ISHType = ishType;
            Level = level;
            Name = name;
            DataType = dataType;
            IsMandatory = isMandatory;
            IsMultiValue = isMultiValue;
            AllowOnRead = allowOnRead;
            AllowOnCreate = allowOnCreate;
            AllowOnUpdate = allowOnUpdate;
            AllowOnSearch = allowOnSearch;
            IsSystem = isSystem;
            IsBasic = isBasic;
            IsDescriptive = isDescriptive;
            Label = "";
            Description = description;
            ReferenceLov = referenceLov;
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
