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
using Trisoft.ISHRemote.Objects.Public;

namespace Trisoft.ISHRemote.Objects
{
    /// <summary>
    /// <para type="description">Container object that groups multiple IshBaselineItem entries</para>
    /// </summary>
    internal class IshBaselineItems
    {
        private List<IshBaselineItem> _items;

        /// <summary>
        /// Creates a new instance of the <see cref="IshBaselineItems"/> class.
        /// </summary>
        /// <param name="baselineId">The identifier of the baseline</param>
        /// <param name="xmlIshBaselineItems">The xml containing the items.</param>
        public IshBaselineItems(string baselineId, string xmlIshBaselineItems)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xmlIshBaselineItems);
            _items = new List<IshBaselineItem>();
            foreach (XmlNode ishBaselineItem in xmlDocument.SelectNodes("baseline/objects/object"))
            {
                _items.Add(new IshBaselineItem(baselineId, (XmlElement)ishBaselineItem));
            }
        }

        /// <summary>
        /// Creates a new instance of the <see cref="IshBaselineItems"/> class.
        /// </summary>
        /// <param name="ishBaselineItems">An <see cref="IshBaselineItem"/> array.</param>
        public IshBaselineItems(IshBaselineItem[] ishBaselineItems)
        {
            _items = new List<IshBaselineItem>(ishBaselineItems == null ? new IshBaselineItem[0] : ishBaselineItems);
        }

        /// <summary>
        /// Gets the current IshBaselineItems.
        /// </summary>
        public IshBaselineItem[] Items
        {
            get { return _items.ToArray(); }
        }
    }
}
