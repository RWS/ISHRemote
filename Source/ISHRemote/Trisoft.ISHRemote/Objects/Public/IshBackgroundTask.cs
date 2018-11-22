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
using Trisoft.ISHRemote.HelperClasses;

namespace Trisoft.ISHRemote.Objects.Public
{
    /// <summary>
    /// <para type="description">The IshBackgroundTask is one entry on the BackgroundTask history are, also holding task top-level information.</para>
    /// </summary>
    public class IshBackgroundTask
    {
        // Regular IshBackgroundTasks look like
        //<ishbackgroundtask ishtaskref="169075" ishhistoryref="122259">
        //<ishfields>
        //    <ishfield name = "USERID" level="task">admin</ishfield>
        //    <ishfield name = "USERID" level="task" ishvaluetype="element">VUSERADMIN</ishfield>
        //    <ishfield name = "HASHID" level="task">464975</ishfield>
        //    <ishfield name = "STATUS" level="task">Failed</ishfield>
        //    <ishfield name = "STATUS" level="task" ishvaluetype="element">VBACKGROUNDTASKSTATUSFAILED</ishfield>
        //    <ishfield name = "EVENTTYPE" level="task">PUBLISH</ishfield>
        //    <ishfield name = "EVENTTYPE" level="task" ishvaluetype="element">VEVENTTYPEPUBLISH</ishfield>
        //    <ishfield name = "PROGRESSID" level="task">477496</ishfield>
        //    <ishfield name = "TRACKINGID" level="task">477496</ishfield>
        //    <ishfield name = "CREATIONDATE" level="task">21/09/2018 16:18:43</ishfield>
        //    <ishfield name = "MODIFICATIONDATE" level="task">21/09/2018 16:18:58</ishfield>
        //    <ishfield name = "EXECUTEAFTERDATE" level="task">21/09/2018 16:18:43</ishfield>
        //    <ishfield name = "LEASEDON" level="task">21/09/2018 16:18:58</ishfield>
        //    <ishfield name = "LEASEDBY" level="task"/>
        //    <ishfield name = "CURRENTATTEMPT" level="task">1</ishfield>
        //    <ishfield name = "INPUTDATAID" level="task">177428</ishfield>
        //    <ishfield name = "OUTPUTDATAID" level="task"/>
        //    <ishfield name = "EXITCODE" level="history"/>
        //    <ishfield name = "ERRORNUMBER" level="history">-142</ishfield>
        //    <ishfield name = "OUTPUT" level="history"/>
        //    <ishfield name = "ERROR" level="history">177431</ishfield>
        //    <ishfield name = "STARTDATE" level="history">21/09/2018 16:18:52</ishfield>
        //    <ishfield name = "ENDDATE" level="history">21/09/2018 16:18:58</ishfield>
        //    <ishfield name = "HOSTNAME" level="history">devserver01</ishfield>
        //</ishfields>
       

        private readonly string _ishRef;
        private readonly Dictionary<Enumerations.ReferenceType,string> _backgroundTaskRef;
        private IshFields _ishFields;

        /// <summary>
        /// IshBackgroundTask creation through an ishbackgroundTask xml element.
        /// Incoming is: &lt;ishbackgroundtask ishtaskref="169075" ishhistoryref="122259"&gt;
        /// </summary>
        /// <param name="xmlIshBackgroundTask">One ishbackgroundTask xml.</param>
        public IshBackgroundTask(XmlElement xmlIshBackgroundTask)
        {
            _backgroundTaskRef = new Dictionary<Enumerations.ReferenceType, string>();
            StringEnum stringEnum = new StringEnum(typeof(Enumerations.ReferenceType));
            // Loop all reference attributes present in the xml
            foreach (string refType in stringEnum.GetStringValues())
            {
                if (xmlIshBackgroundTask.HasAttribute(refType))
                {
                    Enumerations.ReferenceType enumValue = (Enumerations.ReferenceType)StringEnum.Parse(typeof(Enumerations.ReferenceType), refType);
                    _backgroundTaskRef.Add(enumValue, xmlIshBackgroundTask.Attributes[refType].Value);
                }
            }
            _ishFields = new IshFields((XmlElement)xmlIshBackgroundTask.SelectSingleNode("ishfields"));
            _ishRef = _ishFields.GetFieldValue("TASKID", Enumerations.Level.Task, Enumerations.ValueType.Value);
        }

        /// <summary>
        /// Gets the IshRef property, better known as the backgroundTask id of the object.
        /// </summary>
        public string IshRef
        {
            get { return _ishRef; }
        }

        /// <summary>
        /// Get a pipeline friendly base/specialized IshField objects as array
        /// </summary>
        public IshField[] IshField
        {
            get { return _ishFields.Fields(); }
        }

        /// <summary>
        /// Gets and sets the IshFields property.
        /// The IshFields property is a collection of <see cref="IshFields"/>.
        /// </summary>
        internal IshFields IshFields
        {
            get { return _ishFields; }
            set { _ishFields = value; }
        }

        /// <summary>
        /// Stores the variations of progressref, ishlogicalref, ishuserref, ishoutputformatref,... If there are more references, like log/ver/Lng then they are available in the dictionary.
        /// </summary>
        public Dictionary<Enumerations.ReferenceType, string> ObjectRef
        {
            get { return _backgroundTaskRef; }
        }
    }
}
