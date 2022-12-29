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
    /// <para type="description">Generic pipeline object for the Folder API holding folder reference like "ishfolderref", "ishfoldertype", "ishfields"</para>
    /// </summary>
    public class IshFolder : IshBaseObject
    {
        /* IshFolder looks like:
         
         <ishfolder ishfolderref="3222" ishfoldertype="ISHNone">
         <ishfields>
         <ishfield name="FNAME" level="none">General</ishfield>
         <ishfield name="FDOCUMENTTYPE" level="none">None</ishfield>
         <ishfield name="CREATED-ON" level="none">05/04/2002 15:45:06</ishfield>
         <ishfield name="MODIFIED-ON" level="none">25/06/2008 13:42:32</ishfield>
         <ishfield name="FISHQUERY" level="none" />
         <ishfield name="FUSERGROUP" level="none" />
         <ishfield name="READ-ACCESS" level="none" />
         </ishfields>
         </ishfolder>
         */

        private readonly long _ishFolderRef;
        private readonly Enumerations.ISHType _ishType = Enumerations.ISHType.ISHFolder;
        private readonly Enumerations.IshFolderType _ishFolderType;
        private IshFields _ishFields;

        /// <summary>
        /// IshFolder creation through explicitly listing all fields.
        /// </summary>
        /// <param name="ishFolderRef">Folder reference in the InfoShare</param>
        /// <param name="ishFolderType">Folder type</param>
        /// <param name="ishFields">????</param>
        public IshFolder(long ishFolderRef, Enumerations.IshFolderType ishFolderType, IshFields ishFields)
        {
            _ishFolderRef = ishFolderRef;
            _ishFolderType = ishFolderType;
            _ishFields = ishFields ?? new IshFields();
        }

        /// <summary>
        /// IshFolder creation through an ishfolder xml element.
        /// </summary>
        /// <param name="xmlIshFolder">One ishfolder xml.</param>
        public IshFolder(XmlElement xmlIshFolder)
        {
            _ishFolderRef = -1;
            long.TryParse(xmlIshFolder.Attributes["ishfolderref"].Value, out _ishFolderRef);
            _ishFolderType = (Enumerations.IshFolderType)Enum.Parse(typeof(Enumerations.IshFolderType), xmlIshFolder.Attributes["ishfoldertype"].Value);
            _ishFields = new IshFields((XmlElement)xmlIshFolder.SelectSingleNode("ishfields"));
        }

        /// <summary>
        /// Creates a new instance of the <see cref="IshFolder"/> class.
        /// </summary>
        /// <param name="oFolder">OpenApi model</param>
        /// <param name="separator">Any multi-value field are joined up by the separator (typically comma-space), mostly coming from IshSession.</param>
        public IshFolder(OpenApiISH30.Folder oFolder, string separator)
        {
            _ishFolderRef = Convert.ToInt64(oFolder.Id);
            _ishFolderType = (Enumerations.IshFolderType)Enum.Parse(typeof(Enumerations.IshFolderType), oFolder.FolderType.ToString());
            _ishFields = new IshFields(oFolder.Fields, separator);
        }

        /// <summary>
        /// Gets the IshType property.
        /// </summary>
        public Enumerations.ISHType IshType
        {
            get { return _ishType; }
        }

        /// <summary>
        /// Gets ishfolderref property of the folder
        /// </summary>
        public long IshFolderRef
        {
            get { return _ishFolderRef; }
        }

        /// <summary>
        /// Gets ishfoldertype property of the folder
        /// </summary>
        public Enumerations.IshFolderType IshFolderType
        {
            get { return _ishFolderType; }
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
        internal override IshFields IshFields
        {
            get { return _ishFields; }
            set { _ishFields = value; }
        }


    }
}
