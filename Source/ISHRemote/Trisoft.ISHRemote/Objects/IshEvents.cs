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
using Trisoft.ISHRemote.Objects.Public;

namespace Trisoft.ISHRemote.Objects
{
    /// <summary>
    /// <para type="description">Container object to group multiple IshEvent entries holding EventMonitor entries.</para>
    /// </summary>
    internal class IshEvents
    {
        /// <summary>
        /// List with the events
        /// </summary>
        private List<IshEvent> _events;

        /// <summary>
        /// Creates a new instance of the <see cref="IshEvents"/> class.
        /// </summary>
        /// <param name="xmlIshEvents">The xml containing the events.</param>
        public IshEvents(string xmlIshEvents)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xmlIshEvents);
            _events = new List<IshEvent>();
            foreach (XmlNode ishEvent in xmlDocument.SelectNodes("ishevents/ishevent"))
            {
                _events.Add(new IshEvent((XmlElement)ishEvent));
            }
        }

        /// <summary>
        /// Creates a new instance of the <see cref="IshEvents"/> class.
        /// </summary>
        /// <param name="ishEvents">An <see cref="IshEvent"/> array.</param>
        public IshEvents(IshEvent[] ishEvents)
        {
            _events = new List<IshEvent>(ishEvents == null ? new IshEvent[0] : ishEvents);               
        }

        /// <summary>
        /// Gets the list with the current IshEvents.
        /// </summary>
        public List<IshEvent> Events
        {
            get { return _events; }
        }

        /// <summary>
        /// Return all event identifiers (field EVENTID) present
        /// </summary>
        public string[] Ids
        {
            get
            {
                List<string> ids = new List<string>();
                foreach (IshEvent ishEvents in Events)
                {
                    ids.Add(ishEvents.IshRef);
                }
                return ids.ToArray();
            }
        }
    }
}

