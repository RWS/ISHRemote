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
using System.Xml;
using Trisoft.ISHRemote.HelperClasses;

namespace Trisoft.ISHRemote.Objects.Public
{
    /// <summary>
    /// <para type="description">Generic pipeline object for the API holding various references</para>
    /// </summary>
    public class IshBaselineItem
    {
        // Regular IshBaselineItems look like
        // <baseline ref="GUID-D1C23864-304D-408D-86C0-52C5B58343BD">
        //   <objects>
        //     <object ref="GUID.007DFDAD.CEFD.40F3.A75E.2C081228DC89" versionnumber="2" author="Admin" source="save:LatestAvailable" created="10/12/2008 15:05:09" modified="10/12/2008 15:05:09"/>
        //     <object ref="GUID-52AA38B3-23CA-4F24-8638-C1E5C50BD22B" versionnumber="1" author="Admin" source="save:Manual" created="10/12/2008 15:05:09" modified="11/12/2008 16:23:16"/>
        //     <object ref="GUID-52E98215-6399-44E6-8F64-61041AF16D5A" versionnumber="1" author="Admin" source="save:LatestAvailable" created="10/12/2008 15:05:09" modified="10/12/2008 15:05:09"/>
        //     <object ref="IS_REUSED_OBJECT_24" versionnumber="1" author="Admin" source="save:LatestAvailable" created="10/12/2008 15:05:09" modified="10/12/2008 15:05:09"/>
        //     ...
        //   </objects>
        // </baseline>

        public string IshRef { get; internal set; }
        public string LogicalId { get; internal set; }
        public string Version { get; internal set; }
        public string Author { get; internal set; }
        public Enumerations.BaselineSourceEnumeration Source { get; internal set; }
        public DateTime CreatedOn { get; internal set; }
        public DateTime ModifiedOn { get; internal set; }

        /// <summary>
        /// IshBaselineItem using explicit parameters
        /// </summary>
        /// <remarks>Source is set to Manual and the dates are automatically filled by the server-side business code</remarks>
        /// <param name="baselineId">The identifier of the baseline ishobject</param>
        /// <param name="logicalId">The logical identifier of the content object</param>
        /// <param name="version">The version of the content object</param>
        public IshBaselineItem(string baselineId, string logicalId, string version)
        {
            IshRef = baselineId;
            LogicalId = logicalId;
            Version = version;
            Source = Enumerations.BaselineSourceEnumeration.Manual;
        }

        /// <summary>
        /// IshBaselineItem creation through an object xml element.
        /// Incoming is: <object ref="GUID.007DFDAD.CEFD.40F3.A75E.2C081228DC89" versionnumber="2" author="Admin" source="save:LatestAvailable" created="10/12/2008 15:05:09" modified="10/12/2008 15:05:09"/>
        /// </summary>
        /// <param name="baselineId">The identifier of the baseline ishobject</param>
        /// <param name="xmlIshBaselineItem">One object xml.</param>
        public IshBaselineItem(string baselineId, XmlElement xmlIshBaselineItem)
        {
            IshRef = baselineId;
            LogicalId = xmlIshBaselineItem.Attributes["ref"].Value;
            Version = xmlIshBaselineItem.Attributes["versionnumber"].Value;
            Author = xmlIshBaselineItem.Attributes["author"].Value;
            Source = Enumerations.ToBaselineSourceEnumeration(xmlIshBaselineItem.Attributes["source"].Value);
            DateTime createdOn;
            CreatedOn = DateTime.TryParse(xmlIshBaselineItem.Attributes["created"].Value, out createdOn) ? createdOn : DateTime.MinValue;
            DateTime modifiedOn;
            ModifiedOn = DateTime.TryParse(xmlIshBaselineItem.Attributes["modified"].Value, out modifiedOn) ? modifiedOn : DateTime.MinValue;
        }

        /// <summary>
        /// Property overload for *.format.ps1xml usage to allow sorting
        /// </summary>
        public string CreatedOnAsSortableDateTime
        {
            get { return CreatedOn.ToString("s"); }
        }

        /// <summary>
        /// Property overload for *.format.ps1xml usage to allow sorting
        /// </summary>
        public string ModifiedOnAsSortableDateTime
        {
            get { return ModifiedOn.ToString("s"); }
        }
    }
}
