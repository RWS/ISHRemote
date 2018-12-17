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
    /// <para type="description">Container object that groups multiple IshObject entries</para>
    /// </summary>
    internal class IshObjects
    {
        private List<IshObject> _objects;

        /// <summary>
        /// Creates a new instance of the <see cref="IshObjects"/> class.
        /// </summary>
        /// <param name="xmlIshObjects">The xml containing the objects.</param>
        public IshObjects(string xmlIshObjects)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xmlIshObjects);
            _objects = new List<IshObject>();
            foreach (XmlNode ishObject in xmlDocument.SelectNodes("ishobjects/ishobject"))
            {
                _objects.Add(new IshObject((XmlElement)ishObject));
            }
        }

        /// <summary>
        /// Creates a new instance of the <see cref="IshObjects"/> class over the IshObjectFactory
        /// </summary>
        /// <param name="xmlIshObjects">The xml containing the objects.</param>
        public IshObjects(Enumerations.ISHType[] ishType, string xmlIshObjects)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xmlIshObjects);
            _objects = new List<IshObject>();
            foreach (XmlNode ishObject in xmlDocument.SelectNodes("ishobjects/ishobject"))
            {
                _objects.Add(IshObjectFactory.Get(ishType, (XmlElement)ishObject));
            }
        }

        /// <summary>
        /// Creates a new instance of the <see cref="IshObjects"/> class.
        /// </summary>
        /// <param name="ishObjects">An <see cref="IshObject"/> array.</param>
        public IshObjects(IshObject[] ishObjects)
        {              
             _objects = new List<IshObject>(ishObjects == null ? new IshObject[0] : ishObjects);               
        }

        /// <summary>
        /// Gets the current IshObjects.
        /// </summary>
        public IshObject[] Objects
        {
            get { return _objects.ToArray(); }
        }

        /// <summary>
        /// Return all logical/element identifiers (ishref) present
        /// </summary>
        public string[] Ids
        {
            get
            {
                List<string> ids = new List<string>();
                foreach (IshObject ishObject in Objects)
                {
                    ids.Add(ishObject.IshRef);
                }
                return ids.ToArray();
            }
        }

        /// <summary>
        /// Gets the current IshObjects as list
        /// </summary>
        public List<IshObject> ObjectList
        {
            get { return _objects;  }
        }
    }
}
