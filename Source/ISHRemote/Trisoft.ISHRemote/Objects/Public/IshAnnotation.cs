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

namespace Trisoft.ISHRemote.Objects.Public
{
    /// <summary>
    /// A container to allow *.Format.ps1xml do magic, in essence the same as the pipeline object IshObject
    /// </summary>
    public class IshAnnotation : IshObject
    {
        public IshAnnotation(XmlElement xmlIshObject)
            : base(xmlIshObject)
        { }

        /// <summary>
        /// Returns the card_id for the Annotation
        /// </summary>
        public new string ObjectRef
        {
            get { return _objectRef[Enumerations.ReferenceType.Annotation]; }
        }

        /// <summary>
        /// Returns the card_id for the Annotation reply
        /// </summary>
        public string ReplyRef
        {
            get { return _objectRef[Enumerations.ReferenceType.AnnotationReply]; }
        }

    }
}
