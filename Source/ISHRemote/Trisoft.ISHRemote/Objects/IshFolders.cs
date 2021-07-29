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
using Trisoft.ISHRemote.HelperClasses;
using System.Xml;
using Trisoft.ISHRemote.Objects.Public;

namespace Trisoft.ISHRemote.Objects
{
    /// <summary>
    /// <para type="description">Container object to group multiple IshFolder objects</para>
    /// </summary>
    internal class IshFolders
    {
        private List<IshFolder> _folders;

        /// <summary>
        /// Creates a new instance of the <see cref="IshFolders"/> class.
        /// </summary>
        /// <param name="xmlIshFolders">The xml containing the folders.</param>
        public IshFolders(string xmlIshFolders)
        {            
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xmlIshFolders);
            _folders = new List<IshFolder>();
            foreach (XmlNode ishFolder in xmlDocument.SelectNodes("ishfolders/ishfolder"))
            {
                _folders.Add(new IshFolder((XmlElement)ishFolder));
            }
        }

        /// <summary>
        /// Creates a new instance of the <see cref="IshFolders"/> class.
        /// </summary>
        /// <param name="xmlIshFolders">The xml containing the folders.</param>
        /// <param name="nodePath">Path to the ishfolder node</param>
        public IshFolders(string xmlIshFolders, string nodePath)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xmlIshFolders);
            _folders = new List<IshFolder>();
            foreach (XmlNode ishFolder in xmlDocument.SelectNodes(nodePath))
            {
                _folders.Add(new IshFolder((XmlElement)ishFolder));
            }
        }


        /// <summary>
        /// Creates a new instance of the <see cref="IshFolders"/> class.
        /// </summary>
        /// <param name="ishFolders">An <see cref="IshFolder"/> array.</param>
        public IshFolders(IshFolder[] ishFolders)
        {              
             _folders = new List<IshFolder>(ishFolders ?? new IshFolder[0]);               
        }

        /// <summary>
        /// Gets the current IshFolders.
        /// </summary>
        public IshFolder[] Folders
        {
            get { return _folders.ToArray(); }
        }

        /// <summary>
        /// Gets the current IshFolders as list
        /// </summary>
        public List<IshFolder> FolderList
        {
            get { return _folders; }
        }

        /// <summary>
        /// Gets the current IshFolders sorted by name
        /// </summary>
        public IshFolder[] SortedFolders
        {
            get
            {
                SortedDictionary<string, IshFolder> sortedFolders = new SortedDictionary<string, IshFolder>();
                foreach (IshFolder folder in _folders)
                {
                    sortedFolders.Add(folder.IshFields.GetFieldValue("FNAME", Enumerations.Level.None, Enumerations.ValueType.Value), folder);
                }
                List<IshFolder> returnFolders = new List<IshFolder>();
                foreach (string folderName in sortedFolders.Keys)
                {
                    returnFolders.Add(sortedFolders[folderName]);
                }
                return returnFolders.ToArray();
            }
        }

        /// <summary>
        /// Return all identifiers (ishfolderref)
        /// </summary>
        public long[] Ids
        {
            get
            {
                List<long> ids = new List<long>();
                foreach (IshFolder ishFolder in Folders)
                {
                    ids.Add(ishFolder.IshFolderRef);
                }
                return ids.ToArray();
            }
        }
    }
}
