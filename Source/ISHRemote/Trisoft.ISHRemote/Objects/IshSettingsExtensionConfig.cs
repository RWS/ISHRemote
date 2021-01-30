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
using Trisoft.ISHRemote.Interfaces;

namespace Trisoft.ISHRemote.Objects
{
    

    /// <summary>
    /// <para type="description">Stores the essence of IMetadataBinding configuration to merge into IshTypeFieldSetup</para>
    /// </summary>
    static internal class IshSettingsExtensionConfig
    {
        /// <summary>
        /// For every metadata-bounded field, update the IshTypeFieldDefinition
        /// Terrible loop-code but only runs for 13.0.0 up to 14.0.4. Did not want to adapt future-proof IshTypeFieldSetup.
        /// </summary>
        static internal void MergeIntoIshTypeFieldSetup(ILogger logger, IshTypeFieldSetup ishTypeFieldSetup, string xmlSettingsExtensionConfig)
        {
            try
            {
                logger.WriteDebug($"MergeIntoIshTypeFieldSetup on xmlSettingsExtensionConfig.Length[{xmlSettingsExtensionConfig.Length}]");
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(xmlSettingsExtensionConfig);
                // Parsing <metadatabinding ishfieldname="FTESTCONTINENTS" sourceref="CitiesConnector" issmarttaggable="true" />
                foreach (XmlNode xmlMetadataBinding in xmlDocument.SelectNodes("infoShareExtensionConfig/metadatabindings/metadatabinding"))
                {
                    string ishfieldname = xmlMetadataBinding.Attributes.GetNamedItem("ishfieldname").Value;
                    string sourceref = xmlMetadataBinding.Attributes.GetNamedItem("sourceref").Value;
                    bool issmarttaggable = false;
                    if (xmlMetadataBinding.Attributes["allowonsmarttagging"] != null)
                    {
                        issmarttaggable = Boolean.Parse(xmlMetadataBinding.Attributes.GetNamedItem("issmarttaggable").Value);
                    }
                    
                    foreach (IshTypeFieldDefinition ishTypeFieldDefinition in ishTypeFieldSetup.IshTypeFieldDefinition)
                    {
                        if (ishTypeFieldDefinition.Name == ishfieldname)
                        {
                            ishTypeFieldDefinition.DataType = Enumerations.DataType.ISHMetadataBinding;
                            ishTypeFieldDefinition.ReferenceMetadataBinding = sourceref; 
                            ishTypeFieldDefinition.AllowOnSmartTagging = issmarttaggable;
                            logger.WriteDebug($"MergeIntoIshTypeFieldSetup Merged ishfieldname[{ishfieldname}] sourceref[{sourceref}] issmarttaggable[{issmarttaggable}]");
                        }
                    }
                }
            }
            catch (Exception exception)
            {
                logger.WriteWarning($"MergeIntoIshTypeFieldSetup failed to merge MetadataBinding into IshTypeFieldSetup with exception[{exception.Message}]");
            }

        }
    }
}
