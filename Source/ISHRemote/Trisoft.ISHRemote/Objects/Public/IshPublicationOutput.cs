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

namespace Trisoft.ISHRemote.Objects.Public
{
    /// <summary>
    /// <para type="description">A container to allow *.Format.ps1xml do magic, in essence the same as the pipeline object IshObject</para>
    /// </summary>
    public class IshPublicationOutput : IshObject
    {
        public IshPublicationOutput(XmlElement xmlIshObject)
            : base(xmlIshObject)
        { }

        /// <summary>
        /// Returns the logical card_id
        /// </summary>
        public new string ObjectRef
        {
            get { return _objectRef[Enumerations.ReferenceType.Logical]; }
        }

        /// <summary>
        /// Returns the version card_id
        /// </summary>
        public string VersionRef
        {
            get { return _objectRef[Enumerations.ReferenceType.Version]; }
        }

        /// <summary>
        /// Returns the language card_id
        /// </summary>
        public string LngRef
        {
            get { return _objectRef[Enumerations.ReferenceType.Lng]; }
        }
    }
}
