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
    /// <para type="description">Container object to group multiple IshBackgroundTask entries holding BackgroundTask entries.</para>
    /// </summary>
    internal class IshBackgroundTasks
    {
        /// <summary>
        /// List with the backgroundTasks
        /// </summary>
        private List<IshBackgroundTask> _backgroundTasks;

        /// <summary>
        /// Creates a new instance of the <see cref="IshBackgroundTasks"/> class.
        /// </summary>
        /// <param name="xmlIshBackgroundTasks">The xml containing the backgroundTasks.</param>
        public IshBackgroundTasks(string xmlIshBackgroundTasks)
        {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.LoadXml(xmlIshBackgroundTasks);
            _backgroundTasks = new List<IshBackgroundTask>();
            foreach (XmlNode ishBackgroundTask in xmlDocument.SelectNodes("ishbackgroundtasks/ishbackgroundtask"))
            {
                _backgroundTasks.Add(new IshBackgroundTask((XmlElement)ishBackgroundTask));
            }
        }

        /// <summary>
        /// Creates a new instance of the <see cref="IshBackgroundTasks"/> class.
        /// </summary>
        /// <param name="ishBackgroundTasks">An <see cref="IshBackgroundTask"/> array.</param>
        public IshBackgroundTasks(IshBackgroundTask[] ishBackgroundTasks)
        {
            _backgroundTasks = new List<IshBackgroundTask>(ishBackgroundTasks == null ? new IshBackgroundTask[0] : ishBackgroundTasks);               
        }

        /// <summary>
        /// Gets the list with the current IshBackgroundTasks.
        /// </summary>
        public List<IshBackgroundTask> BackgroundTasks
        {
            get { return _backgroundTasks; }
        }

        /// <summary>
        /// Return all backgroundTask identifiers (field TASKID) present
        /// </summary>
        public string[] Ids
        {
            get
            {
                List<string> ids = new List<string>();
                foreach (IshBackgroundTask ishBackgroundTask in BackgroundTasks)
                {
                    ids.Add(ishBackgroundTask.IshRef);
                }
                return ids.ToArray();
            }
        }
    }
}

