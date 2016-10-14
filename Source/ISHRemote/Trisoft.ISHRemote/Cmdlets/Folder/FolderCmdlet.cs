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
using System.Management.Automation;
using Trisoft.ISHRemote.Objects;
using Trisoft.ISHRemote.Objects.Public;
using Trisoft.ISHRemote.Exceptions;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml;

namespace Trisoft.ISHRemote.Cmdlets.Folder
{
    public abstract class FolderCmdlet : TrisoftCmdlet
    {
        /// <summary>
        /// Add the required fields to the requested metadata so when piping the object the necesarry identifiers are provided.
        /// </summary>
        /// <param name="currentFields">The current <see cref="IshFields"/> object to append.</param>
        /// <returns>The updated <see cref="IshFields"/> object.</returns>
        public virtual IshFields AddRequiredFields(IshFields currentFields)
        {
            currentFields.AddOrUpdateField(new IshRequestedMetadataField("FNAME", Enumerations.Level.None, Enumerations.ValueType.Value));
            currentFields.AddOrUpdateField(new IshRequestedMetadataField("READ-ACCESS", Enumerations.Level.None, Enumerations.ValueType.Value));
            currentFields.AddOrUpdateField(new IshRequestedMetadataField("FUSERGROUP", Enumerations.Level.None, Enumerations.ValueType.Value));
            return currentFields;
        }

        /// <summary>
        /// Add the required fields to the requested metadata so when piping the object the necesarry identifiers are provided.
        /// </summary>
        /// <param name="currentFields">The current <see cref="IshFields"/> object to append.</param>
        /// <returns>The updated <see cref="IshFields"/> object.</returns>
        public virtual IshFields AddRequiredDocumentObjFields(IshFields currentFields)
        {
            currentFields.AddOrUpdateField(new IshRequestedMetadataField("FTITLE", Enumerations.Level.Logical, Enumerations.ValueType.Value));
            currentFields.AddOrUpdateField(new IshRequestedMetadataField("VERSION", Enumerations.Level.Version, Enumerations.ValueType.Value));
            currentFields.AddOrUpdateField(new IshRequestedMetadataField("DOC-LANGUAGE", Enumerations.Level.Lng, Enumerations.ValueType.Value));
            currentFields.AddOrUpdateField(new IshRequestedMetadataField("FRESOLUTION", Enumerations.Level.Lng, Enumerations.ValueType.Value));
            return currentFields;
        }

        /// <summary>
        /// Add the required fields to the requested metadata so when piping the object the necesarry identifiers are provided.
        /// </summary>
        /// <param name="currentFields">The current <see cref="IshFields"/> object to append.</param>
        /// <returns>The updated <see cref="IshFields"/> object.</returns>
        public virtual IshFields AddRequiredPublicationOutputFields(IshFields currentFields)
        {
            currentFields.AddOrUpdateField(new IshRequestedMetadataField("FISHPUBLNGCOMBINATION", Enumerations.Level.Lng, Enumerations.ValueType.Value));
            currentFields.AddOrUpdateField(new IshRequestedMetadataField("FISHOUTPUTFORMATREF", Enumerations.Level.Lng, Enumerations.ValueType.Element));
            currentFields.AddOrUpdateField(new IshRequestedMetadataField("FISHOUTPUTFORMATREF", Enumerations.Level.Lng, Enumerations.ValueType.Value));
            currentFields.AddOrUpdateField(new IshRequestedMetadataField("VERSION", Enumerations.Level.Version, Enumerations.ValueType.Value));
            currentFields.AddOrUpdateField(new IshRequestedMetadataField("FTITLE", Enumerations.Level.Logical, Enumerations.ValueType.Value));
            return currentFields;
        }

        /// <summary>
        /// Removes the SYSTEM fields from the given IshFields container. Making the fields ready for an update/write operation. 
        /// </summary>
        internal override IshFields RemoveSystemFields(IshFields ishFields, Enumerations.ActionMode actionMode)
        {
            if (actionMode == Enumerations.ActionMode.Read)
            {
                throw new InvalidOperationException(
                    "We will not remove system fields for read operations anymore as part of TS-9581");
            }

            const Enumerations.Level level = Enumerations.Level.None;
            const Enumerations.ValueType valueType = Enumerations.ValueType.All;

            if (actionMode != Enumerations.ActionMode.Read)
            {
                // These fields can be retrieved BUT cannot be set
                // ...all fields are set by parameters

                // Folder fields
                ishFields.RemoveField(FieldElements.FolderName, level, valueType);
                ishFields.RemoveField(FieldElements.FolderOwner, level, valueType);
                ishFields.RemoveField(FieldElements.FolderContentType, level, valueType);
                ishFields.RemoveField(FieldElements.FolderContentQuery, level, valueType);
                ishFields.RemoveField(FieldElements.FolderPath, level, valueType);

                // General fields
                ishFields.RemoveField(FieldElements.Name, level, valueType);

                // General date fields
                ishFields.RemoveField(FieldElements.CreationDate, level, valueType);
                ishFields.RemoveField(FieldElements.ModificationDate, level, valueType);

                // General security fields
                ishFields.RemoveField(FieldElements.ReadAccess, level, valueType);
                ishFields.RemoveField(FieldElements.Usergroup, level, valueType);
            }

            // Folder fields
            // You have to use GetSubFolders/GetContents to get the values of these fields
            ishFields.RemoveField(FieldElements.FolderSubFolders, level, valueType);
            ishFields.RemoveField(FieldElements.FolderContents, level, valueType);
            ishFields.RemoveField(FieldElements.FolderContentReferences, level, valueType);

            // General security fields
            ishFields.RemoveField(FieldElements.ModifyAccess, level, valueType);
            ishFields.RemoveField(FieldElements.DeleteAccess, level, valueType);
            ishFields.RemoveField(FieldElements.Owner, level, valueType);

            return ishFields;
        }
    }
}
