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
using System.Xml.XPath;
using Trisoft.ISHRemote.Interfaces;
using Trisoft.ISHRemote.Objects;
using Trisoft.ISHRemote.Objects.Public;

namespace Trisoft.ISHRemote.HelperClasses
{
    internal class TriDKXmlSetupHelper
    {
        #region Private Members
        /// <summary>
        /// Logger
        /// </summary>
        private readonly ILogger _logger;
        /// <summary>
        /// Returning list of definitions
        /// </summary>
        private SortedDictionary<string, IshTypeFieldDefinition> _ishTypeFieldDefinitions;
        /// <summary>
        /// cardTypeElementName and list of fieldElementNames
        /// </summary>
        private Dictionary<string, List<CardTypeFieldDefinition>> _cardTypeFieldElementNames = new Dictionary<string, List<CardTypeFieldDefinition>>();
        /// <summary>
        /// fieldElementNames and FieldDefinition matching TriDKXmlSetup tridk:field
        /// </summary>
        private Dictionary<string, FieldDefinition> _fieldDefinitionElementNames = new Dictionary<string, FieldDefinition>();
        /// <summary>
        /// Loaded TriDKXmlSetup Full Export 
        /// </summary>
        XPathDocument _xpathDocument;
        #endregion

        internal class CardTypeFieldDefinition
        {
            internal Enumerations.ISHType Type { get; set; }
            internal Enumerations.Level Level { get; set; }
            internal string FieldName { get; set; }
            public List<Enumerations.ISHType> ReferenceType { get; }
            internal CardTypeFieldDefinition(Enumerations.ISHType ishType, Enumerations.Level ishLevel, string ishFieldName)
            {
                Type = ishType;
                Level = ishLevel;
                FieldName = ishFieldName;
                ReferenceType = new List<Enumerations.ISHType>();
            }
            internal static string Key(Enumerations.ISHType ishType, Enumerations.Level ishLevel)
            {
                return Key(ishType, ishLevel, "");
            }
            internal static string Key(Enumerations.ISHType ishType, Enumerations.Level ishLevel, string ishFieldName)
            {
                return ishType + "=" + (int)ishLevel + ishLevel + "=" + ishFieldName;
            }
            internal static string Key(CardTypeFieldDefinition cardTypeFieldDefinition)
            {
                return Key(cardTypeFieldDefinition.Type, cardTypeFieldDefinition.Level, cardTypeFieldDefinition.FieldName);
            }
        }

        internal class FieldDefinition
        {
            internal string Name { get; set; }
            internal string Label { get; set; }
            internal string Description { get; set; }
            internal string ReferenceLov { get; set; }
            internal Enumerations.DataType DataType { get; set; }
            internal bool IsMandatory { get; set; }
            internal bool IsMultiValue { get; set; }
            internal bool IsSystem { get; set; }
            internal bool IsBasic { get; set; }
            public bool IsDescriptive { get; set; }
            public bool AllowOnRead { get; set; }
            public bool AllowOnCreate { get; set; }
            public bool AllowOnUpdate { get; set; }
            public bool AllowOnSearch { get; set; }


            /// <summary>
            /// Private temporary container that initializes that custom fields are okay, system/standard fields will need correction later
            /// </summary>
            internal FieldDefinition(string name, string label, string description, string dataType, string referenceLov, long min, long max, string isPublic, string isSystem, List<string> classes)
            {
                Name = name;
                Label = label;
                Description = description;
                IsMandatory = (min >= 1);
                IsMultiValue = (max > 1);
                switch (dataType)
                {
                    case "typedate":
                        DataType = Enumerations.DataType.DateTime;
                        break;
                    case "typelov":
                        DataType = Enumerations.DataType.ISHLov;
                        ReferenceLov = referenceLov;
                        break;
                    case "typestring":
                    case "typelanguagedependentstring":
                        DataType = Enumerations.DataType.String;
                        break;
                    case "typecard":
                    case "typecardreference":
                        DataType = Enumerations.DataType.ISHType;
                        break;
                    case "typelongtext":
                        DataType = Enumerations.DataType.LongText;
                        IsMultiValue = false;
                        break;
                    case "typenumber":
                        DataType = Enumerations.DataType.Number;
                        break;
                }
                IsBasic = classes.Contains("SECURITY") ? false : isPublic.Equals("yes", StringComparison.InvariantCulture);
                IsSystem = isSystem.Equals("yes", StringComparison.InvariantCulture);
                string[] descriptiveFields = { "VERSION", "DOC-LANGUAGE", "FRESOLUTION", "FISHPUBLNGCOMBINATION", "FISHOUTPUTFORMATREF", "FISHOUTPUTFORMATNAME", "FISHEDTNAME", "FNAME", "USERNAME", "FISHUSERROLENAME", "FISHUSERGROUPNAME" };
                IsDescriptive = descriptiveFields.Contains<string>(name);
                string[] notAllowOnReadFields = { "PASSWORD" };
                AllowOnRead = !notAllowOnReadFields.Contains<string>(name);
                AllowOnCreate = classes.Contains("NEW");
                AllowOnUpdate = classes.Contains("MODIFY");
                AllowOnSearch = classes.Contains("INDEX");
            }

            internal void Correct(bool allowOnCreate, bool allowOnRead, bool allowOnUpdate, bool allowOnSearch)
            {
                AllowOnCreate = allowOnCreate;
                AllowOnRead = AllowOnRead;
                AllowOnUpdate = allowOnUpdate;
                AllowOnSearch = allowOnSearch;
            }

            internal void Correct(bool allowOnCreate, bool allowOnRead, bool allowOnUpdate, bool allowOnSearch, bool isDescriptive, bool isBasic)
            {
                AllowOnRead = AllowOnRead;
                AllowOnCreate = allowOnCreate;
                AllowOnUpdate = allowOnUpdate;
                AllowOnSearch = allowOnSearch;
                IsDescriptive = isDescriptive;
                IsBasic = isBasic;
            }
        }

        internal TriDKXmlSetupHelper(ILogger logger, string tridkXmlSetupXml)
        {
            _logger = logger;
            _ishTypeFieldDefinitions = new SortedDictionary<string, IshTypeFieldDefinition>();
            _logger.WriteDebug($"Loading tridkXmlSetupXml.Count[{tridkXmlSetupXml.Count()}]...");
            _xpathDocument = new XPathDocument(new System.IO.StringReader(tridkXmlSetupXml));
            _logger.WriteDebug($"Loading CardTypes with FieldNames...");
            LoadCardTypeFieldElementNames();
            _logger.WriteDebug($"Loaded _cardTypeFieldElementNames.Count[{_cardTypeFieldElementNames.Count()}]");
            LoadFieldDefinitions();
            _logger.WriteDebug($"Loaded _fieldDefinitionElementNames.Count[{_fieldDefinitionElementNames.Count()}]");
            CorrectFieldDefinitions();
            _logger.WriteDebug($"Corrected _fieldDefinitionElementNames.Count[{_fieldDefinitionElementNames.Count()}]");
            GenerateIshTypeFieldDefinitionsForCardTypesAndFields();
            _logger.WriteDebug($"Generated _ishTypeFieldDefinitions.Count[{_ishTypeFieldDefinitions.Count()}]");
            CorrectIshTypeFieldDefinitions();
            _logger.WriteDebug($"Corrected _ishTypeFieldDefinitions.Count[{_ishTypeFieldDefinitions.Count()}]");

            // Add Compare-IshTypeFieldDefinition
            // WriteOutput holds IshTypeFieldDefinitionCompare, holding two IshTypeFieldDefinition entries and a github-compare-like '+'/'-' Flag column
            // WriteOutput them, so ISHRemote.Format.ps1xml

            // TableFields should be done on IshTypeFieldSetup, in case the API doesn't deliver
            // AddTableFieldDefinitionForIshEvent();
            // AddTableFieldDefinitionForIshTranslationJobItems();
            // ... (see ISHRemote.Objects namespace)
        }

        private void LoadCardTypeFieldElementNames()
        {
            XPathNavigator xpathNavigator = _xpathDocument.CreateNavigator();
            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(xpathNavigator.NameTable);
            xmlNamespaceManager.AddNamespace("tridk", "urn:trisoft.be:Tridk:Setup:1.0");
            //xmlNamespaceManager.AddNamespace("xmlns:tridk", "urn:trisoft.be:Tridk:Setup:1.0"); 

            // Looping all wanted ISHTypes (card types)
            XPathNodeIterator cardTypeNodes = xpathNavigator.Select("/tridk:setup/tridk:cardtypes/tridk:cardtype", xmlNamespaceManager);
            // Move to the first node
            while (cardTypeNodes.MoveNext())
            {
                // now cardTypeNodes.Current points to the first selected node: card type
                XPathNavigator currentCardTypeNavigator = cardTypeNodes.Current;

                string cardTypeElementName = currentCardTypeNavigator.GetAttribute("element", currentCardTypeNavigator.NamespaceURI);
                _logger.WriteDebug($"TriDKXmlSetupHelper cardTypeElementName[{cardTypeElementName}]");
                Enumerations.ISHType ishType = Enumerations.ToIshTypeFromCardType(cardTypeElementName);
                Enumerations.Level ishLevel = Enumerations.ToLevelFromCardType(cardTypeElementName);
                if (ishType.Equals(Enumerations.ISHType.ISHNotFound))  // skipping unwanted card types like CTREUSEOBJCONFIGURATION
                {
                    _logger.WriteDebug($"TriDKXmlSetupHelper cardTypeElementName[{cardTypeElementName}] ishType[{ishType}] ishLevel[{ishLevel}] skipped");
                    continue;
                }
                _logger.WriteDebug($"TriDKXmlSetupHelper cardTypeElementName[{cardTypeElementName}] ishType[{ishType}] ishLevel[{ishLevel}]");


                _cardTypeFieldElementNames.Add(CardTypeFieldDefinition.Key(ishType, ishLevel), new List<CardTypeFieldDefinition>());

                XPathNodeIterator cardTypeFieldNodes = currentCardTypeNavigator.Select("tridk:fielddefinition/tridk:cardtypefield", xmlNamespaceManager);
                List<string> cardTypeFieldElementNames = new List<string>();
                while (cardTypeFieldNodes.MoveNext())
                {
                    // now cardTypeNodes.Current points to the first selected node: field on the card type
                    string fieldName = cardTypeFieldNodes.Current.GetAttribute("element", currentCardTypeNavigator.NamespaceURI);
                    _logger.WriteDebug($"TriDKXmlSetupHelper ishType[{ishType}] ishLevel[{ishLevel}] fieldName[{fieldName}]");
                    // building private member
                    CardTypeFieldDefinition cardTypeFieldDefinition = new CardTypeFieldDefinition(ishType, ishLevel, fieldName);
                    // loop and add any <tridk:memberdefinition>/<tridk:member> children
                    XPathNavigator memberXPathNavigator = cardTypeFieldNodes.Current.CreateNavigator();
                    XPathNodeIterator memberDefinitionNodes = memberXPathNavigator.Select("tridk:memberdefinition/tridk:member/@tridk:element", xmlNamespaceManager);
                    while (memberDefinitionNodes.MoveNext())
                    {
                        XPathNavigator currentMemberDefinition = memberDefinitionNodes.Current;
                        cardTypeFieldDefinition.ReferenceType.Add(Enumerations.ToIshTypeFromCardType(currentMemberDefinition.Value));
                    }
                    _cardTypeFieldElementNames[CardTypeFieldDefinition.Key(ishType, ishLevel)].Add(cardTypeFieldDefinition);
                }
            }
        }

        private void LoadFieldDefinitions()
        {
            XPathNavigator xpathNavigator = _xpathDocument.CreateNavigator();
            XmlNamespaceManager xmlNamespaceManager = new XmlNamespaceManager(xpathNavigator.NameTable);
            xmlNamespaceManager.AddNamespace("tridk", "urn:trisoft.be:Tridk:Setup:1.0");

            // Looping all wanted ISHTypes (card types)
            XPathNodeIterator fieldDefinitionNodes = xpathNavigator.Select("/tridk:setup/tridk:fields/tridk:field", xmlNamespaceManager);
            // Move to the first node
            while (fieldDefinitionNodes.MoveNext())
            {
                // now fieldDefinitionNodes.Current points to the first selected node: card type
                XPathNavigator currentFieldDefinitionNavigator = fieldDefinitionNodes.Current;

                string fieldDefinitionElementName = currentFieldDefinitionNavigator.GetAttribute("element", currentFieldDefinitionNavigator.NamespaceURI);
                _logger.WriteDebug($"TriDKXmlSetupHelper fieldDefinitionElementName[{fieldDefinitionElementName}]");

                string label = currentFieldDefinitionNavigator.SelectSingleNode("tridk:displaydefinition/tridk:label", xmlNamespaceManager).Value;
                string description = currentFieldDefinitionNavigator.SelectSingleNode("tridk:displaydefinition/tridk:description", xmlNamespaceManager).Value;

                XPathNodeIterator currentFieldType = currentFieldDefinitionNavigator.Select("tridk:typedefinition", xmlNamespaceManager);
                currentFieldType.MoveNext();
                XPathNavigator currentFieldTypeNavigator = currentFieldType.Current;
                currentFieldTypeNavigator.MoveToFirstChild();
                string dataType = currentFieldTypeNavigator.LocalName;
                string referenceLov = currentFieldTypeNavigator.GetAttribute("element", currentFieldTypeNavigator.NamespaceURI);
                long min = 0;
                if (currentFieldTypeNavigator.SelectSingleNode("tridk:minnoofvalues", xmlNamespaceManager) != null)
                {
                    string minnoofvalues = currentFieldTypeNavigator.SelectSingleNode("tridk:minnoofvalues", xmlNamespaceManager).GetAttribute("value", currentFieldTypeNavigator.NamespaceURI);
                    min = (minnoofvalues.Equals(string.Empty)) ? 0 : Convert.ToInt64(minnoofvalues);
                }
                long max = long.MaxValue;
                if (currentFieldTypeNavigator.SelectSingleNode("tridk:maxnoofvalues", xmlNamespaceManager) != null)
                {
                    string maxnoofvalues = currentFieldTypeNavigator.SelectSingleNode("tridk:maxnoofvalues", xmlNamespaceManager).GetAttribute("value", currentFieldTypeNavigator.NamespaceURI);
                    max = (maxnoofvalues.Equals(string.Empty)) ? long.MaxValue : Convert.ToInt64(maxnoofvalues);
                }

                string isPublic = currentFieldDefinitionNavigator.SelectSingleNode("tridk:public", xmlNamespaceManager).GetAttribute("value", currentFieldDefinitionNavigator.NamespaceURI);
                string isSystem = currentFieldDefinitionNavigator.SelectSingleNode("tridk:system", xmlNamespaceManager).GetAttribute("value", currentFieldDefinitionNavigator.NamespaceURI);

                List<string> classes = new List<string>();
                XPathNodeIterator classDefinitionNodes = currentFieldDefinitionNavigator.Select("tridk:classdefinition/tridk:class/@tridk:element", xmlNamespaceManager);
                while (classDefinitionNodes.MoveNext())
                {
                    XPathNavigator currentClassDefinition = classDefinitionNodes.Current;
                    classes.Add(currentClassDefinition.Value);
                }

                _fieldDefinitionElementNames.Add(fieldDefinitionElementName, new FieldDefinition(fieldDefinitionElementName, label, description, dataType, referenceLov, min, max, isPublic, isSystem, classes));
            }
        }

        /// <summary>
        /// Correct FieldDefinitions with default on AllowOnCreate... so the other IshTypeFieldDefinition properties
        /// </summary>
        private void CorrectFieldDefinitions()
        {
            try
            {
                //_fieldDefinitionElementNames["FXYEDITOR"].IsBasic = false; // TODO [Must] Verify if this in every database, mark obsolete. Code can crash here :-(
                _fieldDefinitionElementNames.Remove("CONDITION");
                _fieldDefinitionElementNames.Remove("DELETE-ACCESS");
                _fieldDefinitionElementNames.Remove("DOC-VERSION-MULTI-LNG");
                _fieldDefinitionElementNames.Remove("FANCESTOR");
                _fieldDefinitionElementNames.Remove("FDOCUMENTS");
                _fieldDefinitionElementNames.Remove("FFOLDERS");
                _fieldDefinitionElementNames.Remove("FINCLUDEDOBJECTS");
                _fieldDefinitionElementNames.Remove("FINFOARCH");
                _fieldDefinitionElementNames.Remove("FISHCONTEXTS");
                _fieldDefinitionElementNames.Remove("FISHDOCUMENTREFERENCES");
                _fieldDefinitionElementNames.Remove("FISHFAVORITES");
                _fieldDefinitionElementNames.Remove("FISHPLUGINLOG");
                _fieldDefinitionElementNames.Remove("FISHREUSABLEOBJECTS");
                _fieldDefinitionElementNames.Remove("LNG-VERSION");
                _fieldDefinitionElementNames.Remove("MODIFY-ACCESS");
                _fieldDefinitionElementNames.Remove("OWNER");
                _fieldDefinitionElementNames["CHECKED-OUT"].Correct(false, true, false, true, false, true);
                _fieldDefinitionElementNames["CHECKED-OUT-BY"].Correct(false, true, false, true, false, true);
                _fieldDefinitionElementNames["CREATED-ON"].IsBasic = false;
                _fieldDefinitionElementNames["ED"].Correct(false, true, false, false, false, false);
                _fieldDefinitionElementNames["ED"].IsMultiValue = false;
                _fieldDefinitionElementNames["ED"].IsSystem = true;
                _fieldDefinitionElementNames["EDT"].AllowOnUpdate = false;
                _fieldDefinitionElementNames["FCOMMENTS"].IsMultiValue = false;
                _fieldDefinitionElementNames["FDEFMASTERDOC"].IsBasic = false; // obsolete though
                _fieldDefinitionElementNames["FDOCUMENTTYPE"].AllowOnUpdate = false;
                _fieldDefinitionElementNames["FERRMASTERDOC"].IsBasic = false; // obsolete though
                _fieldDefinitionElementNames["FFRONTPAGE"].IsBasic = false; // obsolete though
                _fieldDefinitionElementNames["FHISTORYLOGGING"].Correct(false, true, false, false, false, false);
                _fieldDefinitionElementNames["FHISTORYLOGGING"].IsMultiValue = false;
                _fieldDefinitionElementNames["FHISTORYLOGGING"].IsSystem = true;
                _fieldDefinitionElementNames["FINHERITEDLANGUAGES"].Correct(false, true, false, true, false, false);
                _fieldDefinitionElementNames["FINHERITEDLANGUAGES"].IsSystem = true;
                _fieldDefinitionElementNames["FISHBASELINE"].IsDescriptive = true;
                _fieldDefinitionElementNames["FISHBASELINE"].IsSystem = true;
                _fieldDefinitionElementNames["FISHBASELINECOMPLETEMODE"].IsBasic = false;
                _fieldDefinitionElementNames["FISHBASELINECOMPLETEMODE"].IsSystem = true;
                _fieldDefinitionElementNames["FISHBRANCHNR"].Correct(false, true, false, false, false, false);
                _fieldDefinitionElementNames["FISHBRANCHNR"].IsSystem = true;
                _fieldDefinitionElementNames["FISHCONDITIONS"].Correct(false, true, false, false);
                _fieldDefinitionElementNames["FISHCURRENTPROGRESS"].IsSystem = true;
                _fieldDefinitionElementNames["FISHEDITTMPLTDESCRIPTION"].IsSystem = true;
                _fieldDefinitionElementNames["FISHEDITTMPLTDESCRIPTION"].IsBasic = false;
                _fieldDefinitionElementNames["FISHEDITTMPLTICONNAME"].Correct(true, true, true, false, false, false);
                _fieldDefinitionElementNames["FISHEDITTMPLTICONNAME"].IsSystem = true;
                _fieldDefinitionElementNames["FISHEDTNAME"].Correct(true, true, true, false);
                _fieldDefinitionElementNames["FISHEMAIL"].IsSystem = true;
                _fieldDefinitionElementNames["FISHENABLEOUTOFDATE"].Correct(true, true, true, false, false, false);
                _fieldDefinitionElementNames["FISHEVENTID"].Correct(false, true, false, false);
                _fieldDefinitionElementNames["FISHEVENTID"].IsSystem = true;
                _fieldDefinitionElementNames["FISHEXTERNALID"].Correct(true, true, true, false, false, true);
                _fieldDefinitionElementNames["FISHEXTERNALID"].IsSystem = true;
                _fieldDefinitionElementNames["FISHFALLBACKLNGDEFAULT"].IsSystem = true;
                _fieldDefinitionElementNames["FISHFALLBACKLNGIMAGES"].IsSystem = true;
                _fieldDefinitionElementNames["FISHFALLBACKLNGRESOURCES"].IsSystem = true;
                _fieldDefinitionElementNames["FISHFOLDERPATH"].Correct(false, true, false, false, false, false);
                _fieldDefinitionElementNames["FISHFRAGMENTLINKS"].Correct(false, true, false, false, false, false);
                _fieldDefinitionElementNames["FISHFRAGMENTLINKS"].IsSystem = true;
                _fieldDefinitionElementNames["FISHHYPERLINKS"].Correct(false, true, false, false, false, false);
                _fieldDefinitionElementNames["FISHHYPERLINKS"].IsSystem = true;
                _fieldDefinitionElementNames["FISHIMAGELINKS"].Correct(false, true, false, false, false, false);
                _fieldDefinitionElementNames["FISHIMAGELINKS"].IsSystem = true;
                _fieldDefinitionElementNames["FISHISRELEASED"].Correct(false, true, false, false, false, true);
                _fieldDefinitionElementNames["FISHISRELEASED"].IsSystem = true;
                _fieldDefinitionElementNames["FISHJOBSPECDESTINATION"].IsBasic = false;
                _fieldDefinitionElementNames["FISHJOBSPECDESTINATION"].IsSystem = true;
                _fieldDefinitionElementNames["FISHLABELRELEASED"].Correct(false, true, false, false);
                _fieldDefinitionElementNames["FISHLASTMODIFIEDBY"].Correct(false, true, false, true);
                _fieldDefinitionElementNames["FISHLASTMODIFIEDBY"].IsSystem = true;
                _fieldDefinitionElementNames["FISHLASTMODIFIEDON"].Correct(false, true, false, true);
                _fieldDefinitionElementNames["FISHLASTMODIFIEDON"].IsSystem = true;
                _fieldDefinitionElementNames["FISHLASTMODIFIEDON"].IsBasic = false;
                _fieldDefinitionElementNames["FISHLEASEDBY"].IsSystem = true;
                _fieldDefinitionElementNames["FISHLEASEDON"].Correct(false, true, false, false);
                _fieldDefinitionElementNames["FISHLEASEDON"].IsSystem = true;
                _fieldDefinitionElementNames["FISHLINKS"].Correct(false, true, false, false, false, false);
                _fieldDefinitionElementNames["FISHLINKS"].IsSystem = true;
                _fieldDefinitionElementNames["FISHLNGINDEPENDENT"].Correct(true, true, true, true, false, false); // obsolete, or custom field?
                _fieldDefinitionElementNames["FISHLNGINDEPENDENT"].IsSystem = true;
                _fieldDefinitionElementNames["FISHMASTERREF"].IsSystem = true;
                _fieldDefinitionElementNames["FISHMAXIMUMPROGRESS"].IsSystem = true;
                _fieldDefinitionElementNames["FISHMODIFIEDON"].Correct(false, true, false, false, false, true); // Used on the publication version to indicate the last modification date of the publication definition.
                _fieldDefinitionElementNames["FISHMODIFIEDON"].IsSystem = true;
                _fieldDefinitionElementNames["FISHMODIFIEDON"].IsBasic = false;
                _fieldDefinitionElementNames["FISHNOREVISIONS"].IsBasic = false;
                _fieldDefinitionElementNames["FISHNOREVISIONS"].IsSystem = true;
                _fieldDefinitionElementNames["FISHOBJECTACTIVE"].Correct(false, true, true, false);
                _fieldDefinitionElementNames["FISHOBJECTACTIVE"].IsSystem = true;
                _fieldDefinitionElementNames["FISHOUTPUTEDT"].Correct(false, true, false, true, false, false); // no way to create that blob
                _fieldDefinitionElementNames["FISHOUTPUTFORMATNAME"].Correct(false, true, true, false);
                _fieldDefinitionElementNames["FISHOUTPUTFORMATNAME"].IsSystem = true;
                _fieldDefinitionElementNames["FISHOUTPUTFORMATREF"].Correct(false, true, false, false);
                _fieldDefinitionElementNames["FISHOUTPUTFORMATREF"].IsSystem = true;
                _fieldDefinitionElementNames["FISHPUBCOMPARE"].IsSystem = true;
                _fieldDefinitionElementNames["FISHPUBCONTEXT"].IsBasic = false; // xml field
                _fieldDefinitionElementNames["FISHPUBCONTEXT"].IsMultiValue = false;
                _fieldDefinitionElementNames["FISHPUBCONTEXT"].IsSystem = true;
                _fieldDefinitionElementNames["FISHPUBENDDATE"].Correct(false, true, false, false);
                _fieldDefinitionElementNames["FISHPUBENDDATE"].IsSystem = true;
                _fieldDefinitionElementNames["FISHPUBLISHER"].Correct(false, true, false, false);
                _fieldDefinitionElementNames["FISHPUBLISHER"].IsSystem = true;
                _fieldDefinitionElementNames["FISHPUBLNGCOMBINATION"].Correct(false, true, false, false);
                _fieldDefinitionElementNames["FISHPUBLNGCOMBINATION"].IsSystem = true;
                _fieldDefinitionElementNames["FISHPUBREPORT"].IsBasic = false; // xml field
                _fieldDefinitionElementNames["FISHPUBREVIEWENDDATE"].IsSystem = true;
                _fieldDefinitionElementNames["FISHPUBSOURCELANGUAGES"].IsSystem = true;
                _fieldDefinitionElementNames["FISHPUBSOURCELANGUAGES"].IsMultiValue = false;
                _fieldDefinitionElementNames["FISHPUBSTARTDATE"].Correct(false, true, false, false);
                _fieldDefinitionElementNames["FISHPUBSTARTDATE"].IsSystem = true;
                _fieldDefinitionElementNames["FISHPUBSTATUS"].IsSystem = true;
                _fieldDefinitionElementNames["FISHPUSHTRANSPROGRESSIDS"].IsBasic = false;
                _fieldDefinitionElementNames["FISHPUSHTRANSPROGRESSIDS"].IsSystem = true;
                _fieldDefinitionElementNames["FISHQUERY"].Correct(false, true, false, false, false, false);
                _fieldDefinitionElementNames["FISHRELEASECANDIDATE"].IsSystem = true;
                _fieldDefinitionElementNames["FISHRELEASELABEL"].Correct(false, true, false, true, false, true);
                _fieldDefinitionElementNames["FISHRELEASELABEL"].IsSystem = true;
                _fieldDefinitionElementNames["FISHREQUIREDRESOLUTIONS"].IsSystem = true;
                _fieldDefinitionElementNames["FISHRESOURCES"].IsSystem = true;
                _fieldDefinitionElementNames["FISHREVCOUNTER"].Correct(false, true, false, false, false, false);
                _fieldDefinitionElementNames["FISHREVCOUNTER"].IsSystem = true;
                _fieldDefinitionElementNames["FISHREVISIONLOG"].IsBasic = false; // xml field
                _fieldDefinitionElementNames["FISHREVISIONLOG"].IsMultiValue = false;
                _fieldDefinitionElementNames["FISHREVISIONLOG"].IsSystem = true;
                _fieldDefinitionElementNames["FISHREVISIONS"].Correct(false, true, false, false, false, false);
                _fieldDefinitionElementNames["FISHREVISIONS"].IsSystem = true;
                _fieldDefinitionElementNames["FISHSTATUSTYPE"].Correct(false, true, false, true, false, false);
                _fieldDefinitionElementNames["FISHSTATUSTYPE"].IsSystem = true;
                _fieldDefinitionElementNames["FISHTARGETS"].Correct(false, true, false, false, false, false);
                _fieldDefinitionElementNames["FISHTARGETS"].IsSystem = true;
                _fieldDefinitionElementNames["FISHTHUMBNAIL"].Correct(true, true, true, false); //how else can you set a thumbnail?
                _fieldDefinitionElementNames["FISHTOBETRANSLWC"].Correct(false, true, false, false, false, false);
                _fieldDefinitionElementNames["FISHTOBETRANSLWC"].IsSystem = true;
                _fieldDefinitionElementNames["FISHTRANSJOBALIAS"].IsSystem = true;
                _fieldDefinitionElementNames["FISHTRANSJOBINCLTRANSLTD"].IsSystem = true;
                _fieldDefinitionElementNames["FISHTRANSJOBPROGRESSID"].IsBasic = false;
                _fieldDefinitionElementNames["FISHTRANSJOBPROGRESSID"].IsSystem = true;
                _fieldDefinitionElementNames["FISHTRANSJOBREQUIREDDATE"].IsSystem = true;
                _fieldDefinitionElementNames["FISHTRANSJOBRESOLUTIONS"].IsSystem = true;
                _fieldDefinitionElementNames["FISHTRANSJOBRETRANSL"].IsSystem = true;
                _fieldDefinitionElementNames["FISHTRANSJOBSENTOUTBY"].IsSystem = true;
                _fieldDefinitionElementNames["FISHTRANSJOBSRCLNG"].IsSystem = true;
                _fieldDefinitionElementNames["FISHTRANSJOBSTATUS"].IsSystem = true;
                _fieldDefinitionElementNames["FISHTRANSJOBTARGETFIELDS"].IsSystem = true;
                _fieldDefinitionElementNames["FISHTRANSJOBTRANSTEMPLID"].IsSystem = true;
                _fieldDefinitionElementNames["FISHTRANSJOBTYPE"].IsSystem = true;
                _fieldDefinitionElementNames["FISHTRANSLATIONLOAD"].Correct(false, true, false, false, false, false);
                _fieldDefinitionElementNames["FISHTRANSLATIONLOAD"].IsSystem = true;
                _fieldDefinitionElementNames["FISHUSERDISABLED"].AllowOnUpdate = true;
                _fieldDefinitionElementNames["FISHUSERDISABLED"].IsSystem = true;
                _fieldDefinitionElementNames["FISHUSERDISPLAYNAME"].Correct(true, true, true, false);
                _fieldDefinitionElementNames["FISHUSERDISPLAYNAME"].IsSystem = true;
                _fieldDefinitionElementNames["FISHUSERGROUPNAME"].Correct(false, true, true, false);
                _fieldDefinitionElementNames["FISHUSERLANGUAGE"].Correct(true, true, true, false, true, true);
                _fieldDefinitionElementNames["FISHUSERLANGUAGE"].IsSystem = true;
                _fieldDefinitionElementNames["FISHUSERROLENAME"].Correct(false, true, true, false);
                _fieldDefinitionElementNames["FISHUSERROLES"].Correct(true, true, true, false, true, true);
                _fieldDefinitionElementNames["FISHUSERROLES"].IsSystem = true;
                _fieldDefinitionElementNames["FISHUSERTYPE"].Correct(true, true, true, false);
                _fieldDefinitionElementNames["FISHUSERTYPE"].IsSystem = true;
                _fieldDefinitionElementNames["FISHVALIDFORPRETRANS"].Correct(false, true, false, false, false, false); // RO fields
                _fieldDefinitionElementNames["FISHVARASSIGNED"].Correct(false, true, false, false, false, false);
                _fieldDefinitionElementNames["FISHVARASSIGNED"].IsBasic = false;
                _fieldDefinitionElementNames["FISHVARASSIGNED"].IsSystem = true;
                _fieldDefinitionElementNames["FISHVARINUSE"].Correct(false, true, false, false, false, false);
                _fieldDefinitionElementNames["FISHVARINUSE"].IsBasic = false;
                _fieldDefinitionElementNames["FISHVARINUSE"].IsSystem = true;
                _fieldDefinitionElementNames["FISHWORDCOUNT"].Correct(false, true, false, false, false, false);
                _fieldDefinitionElementNames["FISHWORDCOUNT"].IsSystem = true;
                _fieldDefinitionElementNames["FMAPID"].Correct(false, true, false, true, false, false);
                _fieldDefinitionElementNames["FMAPID"].IsSystem = true;
                _fieldDefinitionElementNames["FNAME"].Correct(false, true, false, false);
                _fieldDefinitionElementNames["FNOTRANSLATIONMGMT"].IsBasic = false;
                _fieldDefinitionElementNames["FNOTRANSLATIONMGMT"].IsSystem = true;
                _fieldDefinitionElementNames["FORIGIN"].Correct(false, true, false, false, false, false); // RO fields
                _fieldDefinitionElementNames["FREQUESTEDLANGUAGES"].IsSystem = true;
                _fieldDefinitionElementNames["FRESOLUTION"].Correct(false, true, false, true, true, true);
                _fieldDefinitionElementNames["FRESOLUTION"].IsSystem = true;
                _fieldDefinitionElementNames["FREUSEDINVERSION"].Correct(false, true, false, false, false, false);
                _fieldDefinitionElementNames["FREUSEDINVERSION"].IsMultiValue = false;
                _fieldDefinitionElementNames["FREUSEDINVERSION"].IsSystem = true;
                _fieldDefinitionElementNames["FREUSEDOBJECTSSEQNR"].IsBasic = false;
                _fieldDefinitionElementNames["FSOURCELANGUAGE"].Correct(true, true, true, true, true, true);
                _fieldDefinitionElementNames["FSOURCELANGUAGE"].IsSystem = true;
                _fieldDefinitionElementNames["FSTATUS"].IsSystem = true;
                _fieldDefinitionElementNames["FSYSTEMLOCK"].Correct(false, true, false, false, false, false);
                _fieldDefinitionElementNames["FSYSTEMLOCK"].IsSystem = true;
                _fieldDefinitionElementNames["FTITLE"].IsSystem = true;
                _fieldDefinitionElementNames["FTRANSLSTARTDATE"].Correct(false, true, false, false, false, false);
                _fieldDefinitionElementNames["FTRANSLSTARTDATE"].IsSystem = true;
                _fieldDefinitionElementNames["FUSERGROUP"].AllowOnCreate = false;
                _fieldDefinitionElementNames["FUSERGROUP"].AllowOnUpdate = false;
                _fieldDefinitionElementNames["FUSERGROUP"].IsBasic = false;
                _fieldDefinitionElementNames["FUSERGROUP"].IsSystem = true;
                _fieldDefinitionElementNames["MODIFIED-ON"].IsBasic = false;
                _fieldDefinitionElementNames["NAME"].Correct(false, true, false, false, false, false);
                _fieldDefinitionElementNames["OSUSER"].IsBasic = false;
                _fieldDefinitionElementNames["PASSWORD"].Correct(true, false, true, false, false, true);
                _fieldDefinitionElementNames["PASSWORD"].IsMultiValue = false;
                _fieldDefinitionElementNames["RIGHTS"].Correct(false, true, false, false, true, true);
                _fieldDefinitionElementNames["USERNAME"].AllowOnCreate = false;
                _fieldDefinitionElementNames["VERSION"].Correct(false, true, false, true);

                // foreach (var field in _fieldDefinitionElementNames.Keys)
                // { // dummy to allow debugging by printing in the watch window}
            }
            catch (Exception exception)
            {
                _logger.WriteWarning($"TriDKXmlSetupHelper failed during CorrectFieldDefinitions, probably less incoming fields than typical. exception[{exception.Message}]");
            }
        }

        /// <summary>
        /// Combining CardTypeFieldDefinition defining the fields on a card type with FieldDefinitions, resulting in denormalized IshTypeFieldDefinitions
        /// </summary>
        private void GenerateIshTypeFieldDefinitionsForCardTypesAndFields()
        {
            //foreach (KeyValuePair<string, List<CardTypeFieldDefinition>> keyValuePair in _cardTypeFieldElementNames)
            foreach (string key in _cardTypeFieldElementNames.Keys)
            {
                _logger.WriteDebug($"Generating [{key}]");
                foreach (CardTypeFieldDefinition cardTypeFieldDefinition in _cardTypeFieldElementNames[key])
                {
                    _logger.WriteDebug($"Generating [{CardTypeFieldDefinition.Key(cardTypeFieldDefinition)}]");
                    FieldDefinition testFieldDefinitionExistance;
                    if (!_fieldDefinitionElementNames.TryGetValue(cardTypeFieldDefinition.FieldName, out testFieldDefinitionExistance))
                    {
                        _logger.WriteDebug($"Skipping   [{CardTypeFieldDefinition.Key(cardTypeFieldDefinition)}]");
                        continue;
                    }
                    IshTypeFieldDefinition ishTypeFieldDefinition = new IshTypeFieldDefinition(
                        _logger,
                        cardTypeFieldDefinition.Type,
                        cardTypeFieldDefinition.Level,
                        cardTypeFieldDefinition.FieldName,
                        _fieldDefinitionElementNames[cardTypeFieldDefinition.FieldName].DataType);
                    ishTypeFieldDefinition.AllowOnCreate = _fieldDefinitionElementNames[cardTypeFieldDefinition.FieldName].AllowOnCreate;
                    ishTypeFieldDefinition.AllowOnRead = _fieldDefinitionElementNames[cardTypeFieldDefinition.FieldName].AllowOnRead;
                    ishTypeFieldDefinition.AllowOnSearch = _fieldDefinitionElementNames[cardTypeFieldDefinition.FieldName].AllowOnSearch;
                    ishTypeFieldDefinition.AllowOnUpdate = _fieldDefinitionElementNames[cardTypeFieldDefinition.FieldName].AllowOnUpdate;
                    ishTypeFieldDefinition.DataType = _fieldDefinitionElementNames[cardTypeFieldDefinition.FieldName].DataType;
                    ishTypeFieldDefinition.Description = _fieldDefinitionElementNames[cardTypeFieldDefinition.FieldName].Description;
                    ishTypeFieldDefinition.IsBasic = _fieldDefinitionElementNames[cardTypeFieldDefinition.FieldName].IsBasic;
                    ishTypeFieldDefinition.IsDescriptive = _fieldDefinitionElementNames[cardTypeFieldDefinition.FieldName].IsDescriptive;
                    ishTypeFieldDefinition.IsMandatory = _fieldDefinitionElementNames[cardTypeFieldDefinition.FieldName].IsMandatory;
                    ishTypeFieldDefinition.IsMultiValue = _fieldDefinitionElementNames[cardTypeFieldDefinition.FieldName].IsMultiValue;
                    ishTypeFieldDefinition.IsSystem = _fieldDefinitionElementNames[cardTypeFieldDefinition.FieldName].IsSystem;
                    ishTypeFieldDefinition.Label = _fieldDefinitionElementNames[cardTypeFieldDefinition.FieldName].Label;
                    ishTypeFieldDefinition.ReferenceLov = _fieldDefinitionElementNames[cardTypeFieldDefinition.FieldName].ReferenceLov;
                    ishTypeFieldDefinition.ReferenceType = cardTypeFieldDefinition.ReferenceType;

                    _ishTypeFieldDefinitions.Add(CardTypeFieldDefinition.Key(cardTypeFieldDefinition), ishTypeFieldDefinition);
                }
            }
        }

        private void CorrectIshTypeFieldDefinitions()
        {
            try
            {
                CorrectAllowOnSearch();

                CorrectISHBaseline();
                CorrectISHConfiguration();
                CorrectISHEDT();
                CorrectISHFeatures();
                CorrectISHFolder();
                CorrectISHOutputFormat();
                CorrectISHPublication();
                CorrectISHReusedObj();
                CorrectISHRevision();
                CorrectISHTranslationJob();
                CorrectISHUser();
                CorrectISHUserGroup();
                CorrectISHUserRole();
            }
            catch (Exception exception)
            {
                _logger.WriteWarning($"TriDKXmlSetupHelper failed during CorrectIshTypeFieldDefinitions, probably less incoming fields than typical. exception[{exception.Message}]");
            }
        }
            

        /// <summary>
        /// AllowOnSearch should be false everywhere, except on Content Objects, the only thing we put in our Full-Text-Index
        /// </summary>
        private void CorrectAllowOnSearch()
        {
            foreach (IshTypeFieldDefinition ishTypeFieldDefinition in _ishTypeFieldDefinitions.Values)
            {
                switch (ishTypeFieldDefinition.ISHType)
                {
                    case Enumerations.ISHType.ISHIllustration:
                    case Enumerations.ISHType.ISHLibrary:
                    case Enumerations.ISHType.ISHMasterDoc:
                    case Enumerations.ISHType.ISHModule:
                    case Enumerations.ISHType.ISHTemplate:
                        break;
                    default:
                        ishTypeFieldDefinition.AllowOnSearch = false;
                        break;
                }
            }
        }

        private void CorrectISHBaseline()
        {
            _ishTypeFieldDefinitions[CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHBaseline, Enumerations.Level.None, "FISHDOCUMENTRELEASE")].AllowOnCreate = false;
            _ishTypeFieldDefinitions[CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHBaseline, Enumerations.Level.None, "FISHDOCUMENTRELEASE")].IsDescriptive = true;
            _ishTypeFieldDefinitions[CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHBaseline, Enumerations.Level.None, "FISHDOCUMENTRELEASE")].IsBasic = true;
            _ishTypeFieldDefinitions[CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHBaseline, Enumerations.Level.None, "FISHDOCUMENTRELEASE")].IsSystem = true;
            _ishTypeFieldDefinitions[CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHBaseline, Enumerations.Level.None, "FISHBASELINEACTIVE")].IsSystem = true;
            _ishTypeFieldDefinitions[CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHBaseline, Enumerations.Level.None, "FISHBASELINEACTIVE")].IsDescriptive = true;
            _ishTypeFieldDefinitions[CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHBaseline, Enumerations.Level.None, "FISHLABELRELEASED")].IsSystem = true;
            _ishTypeFieldDefinitions[CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHBaseline, Enumerations.Level.None, "FISHLABELRELEASED")].IsDescriptive = true;
            _ishTypeFieldDefinitions[CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHBaseline, Enumerations.Level.None, "NAME")].AllowOnUpdate = true;
            _ishTypeFieldDefinitions[CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHBaseline, Enumerations.Level.None, "NAME")].IsDescriptive = true;
            _ishTypeFieldDefinitions.Remove(CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHBaseline, Enumerations.Level.None, "ED"));
            _ishTypeFieldDefinitions.Remove(CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHBaseline, Enumerations.Level.None, "CHECKED-OUT"));
            _ishTypeFieldDefinitions.Remove(CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHBaseline, Enumerations.Level.None, "CHECKED-OUT-BY"));
            _ishTypeFieldDefinitions.Remove(CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHBaseline, Enumerations.Level.None, "FUSERGROUP"));
            _ishTypeFieldDefinitions.Remove(CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHBaseline, Enumerations.Level.None, "READ-ACCESS"));
        }

        private void CorrectISHConfiguration()
        {
            _ishTypeFieldDefinitions[CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHConfiguration, Enumerations.Level.None, "FINBOXCONFIGURATION")].IsSystem = true;
            _ishTypeFieldDefinitions[CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHConfiguration, Enumerations.Level.None, "FINBOXCONFIGURATION")].IsMultiValue = false;
            _ishTypeFieldDefinitions[CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHConfiguration, Enumerations.Level.None, "FISHBACKGROUNDTASKCONFIG")].IsSystem = true;
            _ishTypeFieldDefinitions[CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHConfiguration, Enumerations.Level.None, "FISHBACKGROUNDTASKCONFIG")].IsMultiValue = false;
            _ishTypeFieldDefinitions[CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHConfiguration, Enumerations.Level.None, "FISHCHANGETRACKERCONFIG")].IsSystem = true;
            _ishTypeFieldDefinitions[CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHConfiguration, Enumerations.Level.None, "FISHCHANGETRACKERCONFIG")].IsMultiValue = false;
            _ishTypeFieldDefinitions[CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHConfiguration, Enumerations.Level.None, "FISHENABLEOUTOFDATE")].IsSystem = true;
            _ishTypeFieldDefinitions[CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHConfiguration, Enumerations.Level.None, "FISHENABLEOUTOFDATE")].IsBasic = false;
            _ishTypeFieldDefinitions[CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHConfiguration, Enumerations.Level.None, "FISHENRICHURI")].IsSystem = true;
            _ishTypeFieldDefinitions[CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHConfiguration, Enumerations.Level.None, "FISHEXTENSIONCONFIG")].IsSystem = true;
            _ishTypeFieldDefinitions[CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHConfiguration, Enumerations.Level.None, "FISHEXTENSIONCONFIG")].IsMultiValue = false;
            _ishTypeFieldDefinitions[CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHConfiguration, Enumerations.Level.None, "FISHLCURI")].IsSystem = true;
            _ishTypeFieldDefinitions[CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHConfiguration, Enumerations.Level.None, "FISHPLUGINCONFIGXML")].IsMultiValue = false;
            _ishTypeFieldDefinitions[CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHConfiguration, Enumerations.Level.None, "FISHPUBSTATECONFIG")].IsBasic = false;
            _ishTypeFieldDefinitions[CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHConfiguration, Enumerations.Level.None, "FISHPUBSTATECONFIG")].IsSystem = true;
            _ishTypeFieldDefinitions[CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHConfiguration, Enumerations.Level.None, "FISHPUBSTATECONFIG")].IsMultiValue = false;
            _ishTypeFieldDefinitions[CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHConfiguration, Enumerations.Level.None, "FISHSYSTEMRESOLUTION")].IsSystem = true;
            _ishTypeFieldDefinitions[CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHConfiguration, Enumerations.Level.None, "FISHTRANSJOBLEASES")].IsSystem = true;
            _ishTypeFieldDefinitions[CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHConfiguration, Enumerations.Level.None, "FISHTRANSJOBLEASES")].IsMultiValue = false;
            _ishTypeFieldDefinitions[CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHConfiguration, Enumerations.Level.None, "FISHTRANSJOBSTATECONFIG")].IsBasic = false;
            _ishTypeFieldDefinitions[CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHConfiguration, Enumerations.Level.None, "FISHTRANSJOBSTATECONFIG")].IsSystem = true;
            _ishTypeFieldDefinitions[CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHConfiguration, Enumerations.Level.None, "FISHTRANSJOBSTATECONFIG")].IsMultiValue = false;
            _ishTypeFieldDefinitions[CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHConfiguration, Enumerations.Level.None, "FISHWRITEOBJPLUGINCFG")].IsSystem = true;
            _ishTypeFieldDefinitions[CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHConfiguration, Enumerations.Level.None, "FISHWRITEOBJPLUGINCFG")].IsMultiValue = false;
            _ishTypeFieldDefinitions[CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHConfiguration, Enumerations.Level.None, "FSTATECONFIGURATION")].IsSystem = true;
            _ishTypeFieldDefinitions[CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHConfiguration, Enumerations.Level.None, "FSTATECONFIGURATION")].IsMultiValue = false;
            _ishTypeFieldDefinitions[CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHConfiguration, Enumerations.Level.None, "FTRANSLATIONCONFIGURATION")].IsSystem = true;
            _ishTypeFieldDefinitions[CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHConfiguration, Enumerations.Level.None, "FTRANSLATIONCONFIGURATION")].IsMultiValue = false;
            _ishTypeFieldDefinitions[CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHConfiguration, Enumerations.Level.None, "NAME")].AllowOnUpdate = false;
            _ishTypeFieldDefinitions.Remove(CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHConfiguration, Enumerations.Level.None, "READ-ACCESS"));
        }

        private void CorrectISHEDT()
        {
            _ishTypeFieldDefinitions[CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHEDT, Enumerations.Level.None, "NAME")].AllowOnUpdate = true;
            _ishTypeFieldDefinitions[CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHEDT, Enumerations.Level.None, "EDT-CANDIDATE")].IsBasic = true;
            _ishTypeFieldDefinitions[CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHEDT, Enumerations.Level.None, "EDT-FILE-EXTENSION")].IsSystem = true;
            _ishTypeFieldDefinitions[CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHEDT, Enumerations.Level.None, "EDT-FILE-EXTENSION")].IsDescriptive = true;
            _ishTypeFieldDefinitions[CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHEDT, Enumerations.Level.None, "EDT-MIME-TYPE")].IsDescriptive = true;
            _ishTypeFieldDefinitions[CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHEDT, Enumerations.Level.None, "EDT-MIME-TYPE")].IsBasic = true;
            _ishTypeFieldDefinitions[CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHEDT, Enumerations.Level.None, "FISHEDTNAME")].IsSystem = true;
            _ishTypeFieldDefinitions[CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHEDT, Enumerations.Level.None, "FISHEDTNAME")].AllowOnCreate = false;
            _ishTypeFieldDefinitions.Remove(CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHEDT, Enumerations.Level.None, "READ-ACCESS"));
        }

        private void CorrectISHFeatures()
        {
            // in essence, ISHFeatures is completely stripped out
            _ishTypeFieldDefinitions.Remove(CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHFeatures, Enumerations.Level.None, "CHECKED-OUT"));
            _ishTypeFieldDefinitions.Remove(CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHFeatures, Enumerations.Level.None, "CHECKED-OUT-BY"));
            _ishTypeFieldDefinitions.Remove(CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHFeatures, Enumerations.Level.None, "CREATED-ON"));
            _ishTypeFieldDefinitions.Remove(CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHFeatures, Enumerations.Level.None, "ED"));
            _ishTypeFieldDefinitions.Remove(CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHFeatures, Enumerations.Level.None, "FISHCLASSIFICATIONS"));
            _ishTypeFieldDefinitions.Remove(CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHFeatures, Enumerations.Level.None, "MODIFIED-ON"));
            _ishTypeFieldDefinitions.Remove(CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHFeatures, Enumerations.Level.None, "NAME"));
            _ishTypeFieldDefinitions.Remove(CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHFeatures, Enumerations.Level.None, "READ-ACCESS"));
        }

        private void CorrectISHFolder()
        {
            _ishTypeFieldDefinitions[CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHFolder, Enumerations.Level.None, "FUSERGROUP")].IsDescriptive = true;
            _ishTypeFieldDefinitions[CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHFolder, Enumerations.Level.None, "FUSERGROUP")].IsBasic = true;
            _ishTypeFieldDefinitions[CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHFolder, Enumerations.Level.None, "FDOCUMENTTYPE")].IsSystem = true;
            _ishTypeFieldDefinitions[CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHFolder, Enumerations.Level.None, "FDOCUMENTTYPE")].IsDescriptive = true;
            _ishTypeFieldDefinitions[CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHFolder, Enumerations.Level.None, "FDOCUMENTTYPE")].AllowOnCreate = false;
            _ishTypeFieldDefinitions[CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHFolder, Enumerations.Level.None, "FISHFOLDERPATH")].IsSystem = true;
            _ishTypeFieldDefinitions[CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHFolder, Enumerations.Level.None, "FISHFOLDERPATH")].IsBasic = true;
            _ishTypeFieldDefinitions[CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHFolder, Enumerations.Level.None, "FISHQUERY")].IsSystem = true;
            _ishTypeFieldDefinitions[CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHFolder, Enumerations.Level.None, "FISHQUERY")].IsBasic = true;
            _ishTypeFieldDefinitions[CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHFolder, Enumerations.Level.None, "FISHQUERY")].IsMultiValue = false;
            _ishTypeFieldDefinitions[CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHFolder, Enumerations.Level.None, "FNAME")].IsSystem = true;
            _ishTypeFieldDefinitions[CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHFolder, Enumerations.Level.None, "NAME")].IsDescriptive = true;
            _ishTypeFieldDefinitions[CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHFolder, Enumerations.Level.None, "NAME")].AllowOnUpdate = false;
            _ishTypeFieldDefinitions[CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHFolder, Enumerations.Level.None, "READ-ACCESS")].IsBasic = true;
        }

        private void CorrectISHRevision()
        {
            _ishTypeFieldDefinitions.Remove(CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHRevision, Enumerations.Level.None, "CREATED-ON"));
            _ishTypeFieldDefinitions.Remove(CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHRevision, Enumerations.Level.None, "CURRENT-FILE-NAME"));
            _ishTypeFieldDefinitions.Remove(CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHRevision, Enumerations.Level.None, "EDT"));
            _ishTypeFieldDefinitions.Remove(CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHRevision, Enumerations.Level.None, "MODIFIED-ON"));
            _ishTypeFieldDefinitions.Remove(CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHRevision, Enumerations.Level.None, "NAME"));
            _ishTypeFieldDefinitions.Remove(CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHRevision, Enumerations.Level.None, "READ-ACCESS"));
        }

        private void CorrectISHOutputFormat()
        {
            _ishTypeFieldDefinitions[CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHOutputFormat, Enumerations.Level.None, "FISHOUTPUTEDT")].IsBasic = true;
            _ishTypeFieldDefinitions[CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHOutputFormat, Enumerations.Level.None, "FISHOUTPUTEDT")].IsDescriptive = false;
            _ishTypeFieldDefinitions[CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHOutputFormat, Enumerations.Level.None, "FISHOUTPUTEDT")].IsSystem = true;
            _ishTypeFieldDefinitions[CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHOutputFormat, Enumerations.Level.None, "FISHCLEANUP")].IsSystem = true;
            _ishTypeFieldDefinitions[CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHOutputFormat, Enumerations.Level.None, "FISHKEEPDTDSYSTEMID")].IsSystem = true;
            _ishTypeFieldDefinitions[CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHOutputFormat, Enumerations.Level.None, "FISHKEEPFIXEDATTRIBUTES")].IsSystem = true;
            _ishTypeFieldDefinitions[CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHOutputFormat, Enumerations.Level.None, "FISHPUBRESOLVEVARIABLES")].IsSystem = true;
            _ishTypeFieldDefinitions[CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHOutputFormat, Enumerations.Level.None, "FISHRESOLUTIONS")].IsSystem = true;
            _ishTypeFieldDefinitions[CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHOutputFormat, Enumerations.Level.None, "FISHSINGLEFILE")].IsSystem = true;
            _ishTypeFieldDefinitions[CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHOutputFormat, Enumerations.Level.None, "FSDLLIVECONTENTSKIN")].IsBasic = false;
            _ishTypeFieldDefinitions[CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHOutputFormat, Enumerations.Level.None, "NAME")].AllowOnUpdate = true;
            _ishTypeFieldDefinitions.Remove(CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHOutputFormat, Enumerations.Level.None, "CHECKED-OUT"));
            _ishTypeFieldDefinitions.Remove(CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHOutputFormat, Enumerations.Level.None, "CHECKED-OUT-BY"));
            _ishTypeFieldDefinitions.Remove(CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHOutputFormat, Enumerations.Level.None, "ED"));
            _ishTypeFieldDefinitions.Remove(CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHOutputFormat, Enumerations.Level.None, "READ-ACCESS"));
        }

        private void CorrectISHPublication()
        {
            _ishTypeFieldDefinitions[CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHPublication, Enumerations.Level.Lng, "DOC-LANGUAGE")].IsDescriptive = false;
            _ishTypeFieldDefinitions[CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHPublication, Enumerations.Level.Lng, "DOC-LANGUAGE")].IsBasic = false;
            _ishTypeFieldDefinitions[CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHPublication, Enumerations.Level.Lng, "DOC-LANGUAGE")].AllowOnCreate = false;
            _ishTypeFieldDefinitions[CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHPublication, Enumerations.Level.Lng, "DOC-LANGUAGE")].AllowOnUpdate = false;
            _ishTypeFieldDefinitions.Remove(CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHPublication, Enumerations.Level.Lng, "FISHPUBREPORT"));
            _ishTypeFieldDefinitions.Remove(CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHPublication, Enumerations.Level.Lng, "CHECKED-OUT"));
            _ishTypeFieldDefinitions.Remove(CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHPublication, Enumerations.Level.Lng, "CHECKED-OUT-BY"));
        }

        private void CorrectISHReusedObj()
        {
            // in essence, ISHReusedObj is completely stripped out
        }

        private void CorrectISHTranslationJob()
        {
            _ishTypeFieldDefinitions[CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHTranslationJob, Enumerations.Level.None, "NAME")].IsDescriptive = true;
            _ishTypeFieldDefinitions[CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHTranslationJob, Enumerations.Level.None, "FNAME")].IsSystem = true;
            _ishTypeFieldDefinitions[CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHTranslationJob, Enumerations.Level.None, "FNAME")].AllowOnCreate = true;
            _ishTypeFieldDefinitions[CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHTranslationJob, Enumerations.Level.None, "FNAME")].AllowOnUpdate = true;
            _ishTypeFieldDefinitions.Remove(CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHTranslationJob, Enumerations.Level.None, "READ-ACCESS"));
            _ishTypeFieldDefinitions.Remove(CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHTranslationJob, Enumerations.Level.None, "FUSERGROUP"));
        }

        private void CorrectISHUser()
        {
            _ishTypeFieldDefinitions[CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHUser, Enumerations.Level.None, "NAME")].AllowOnUpdate = true;
            _ishTypeFieldDefinitions[CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHUser, Enumerations.Level.None, "FUSERGROUP")].IsDescriptive = true;
            _ishTypeFieldDefinitions[CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHUser, Enumerations.Level.None, "FUSERGROUP")].IsBasic = true;
            _ishTypeFieldDefinitions[CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHUser, Enumerations.Level.None, "FUSERGROUP")].AllowOnCreate = true;
            _ishTypeFieldDefinitions[CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHUser, Enumerations.Level.None, "FUSERGROUP")].AllowOnUpdate = true;
            _ishTypeFieldDefinitions.Remove(CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHUser, Enumerations.Level.None, "READ-ACCESS"));
        }

        private void CorrectISHUserGroup()
        {
            _ishTypeFieldDefinitions[CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHUserGroup, Enumerations.Level.None, "NAME")].AllowOnUpdate = true;
            _ishTypeFieldDefinitions[CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHUserGroup, Enumerations.Level.None, "FISHUSERGROUPNAME")].IsSystem = true;
            _ishTypeFieldDefinitions.Remove(CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHUserGroup, Enumerations.Level.None, "READ-ACCESS"));
        }

        private void CorrectISHUserRole()
        {
            _ishTypeFieldDefinitions[CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHUserRole, Enumerations.Level.None, "NAME")].AllowOnUpdate = true;
            _ishTypeFieldDefinitions[CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHUserRole, Enumerations.Level.None, "FISHUSERROLENAME")].IsSystem = true;
            _ishTypeFieldDefinitions.Remove(CardTypeFieldDefinition.Key(Enumerations.ISHType.ISHUserRole, Enumerations.Level.None, "READ-ACCESS"));
        }
        internal List<IshTypeFieldDefinition> IshTypeFieldDefinition
        {
            get
            {
                return _ishTypeFieldDefinitions.Values.ToList<IshTypeFieldDefinition>();
            }
        }
    }
}
